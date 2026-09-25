// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    /// <summary>Manages native resource acquisition, reference counts and invalidation.</summary>
    internal static partial class HandleTable {
        /// <summary>
        ///     Registers a resource. Borrowed pointers reuse live slots as views; owned pointers replace them.
        /// </summary>
        /// <param name="pointer">The resource SDL just handed over.</param>
        /// <param name="kind">Whether this process has to free it again.</param>
        /// <param name="owner">The resource SDL destroys it with, or <see cref="OwnerId.None" />.</param>
        /// <param name="survivesOwner">
        ///     Whether one release still has to reach SDL after <paramref name="owner" /> is gone - see
        ///     the remarks on <c>Slot.SurvivesOwner</c>. Almost nothing needs this.
        /// </param>
        internal static HandleId<T> Acquire<T>(NativePtr<T> pointer, HandleKind kind, OwnerId owner = default,
            bool survivesOwner = false) where T : unmanaged {
            if (pointer.IsNull) return default;

            // Outside the lock on purpose: the first read of this runs a type initialiser that takes
            // Gate itself. See the remarks on TypeTag<T>.
            ushort tag = TypeTag<T>.Value;

            lock (Gate) {
                if (Interned.TryGetValue(pointer.Ptr, out int existing)) {
                    ref Slot known = ref SegmentOf(existing)[existing & SegmentMask];

                    if (kind == HandleKind.Owned || !IsOwnerChainAliveLocked(known.Owner)) {
                        // Retire recycled addresses and slots whose owner is gone.
                        RetireLocked(existing);
                    } else {
                        return new HandleId<T>(Pack(existing, known.Generation, view: true));
                    }
                }

                return CreateSlotLocked(pointer, kind, owner, tag, survivesOwner);
            }
        }

        /// <inheritdoc cref="Acquire{T}(NativePtr{T}, HandleKind, OwnerId)" />
        /// <param name="pointer">The resource SDL just handed over.</param>
        /// <param name="ownsHandle">Whether this process has to free it again.</param>
        /// <param name="owner">Whoever SDL destroys it together with.</param>
        internal static HandleId<T> Acquire<T>(NativePtr<T> pointer, bool ownsHandle, OwnerId owner = default)
            where T : unmanaged {
            return Acquire(pointer, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed, owner);
        }

        /// <summary>
        ///     Acquires a shared reference, reusing a live slot. Each acquisition requires one release.
        /// </summary>
        internal static HandleId<T> AcquireShared<T>(NativePtr<T> pointer, OwnerId owner = default)
            where T : unmanaged {
            if (pointer.IsNull) return default;

            // Outside the lock on purpose - see Acquire.
            ushort tag = TypeTag<T>.Value;

            lock (Gate) {
                if (Interned.TryGetValue(pointer.Ptr, out int existing)) {
                    ref Slot known = ref SegmentOf(existing)[existing & SegmentMask];
                    if (IsOwnerChainAliveLocked(known.Owner)) {
                        known.RefCount++;
                        return new HandleId<T>(Pack(existing, known.Generation, view: false));
                    }

                    // Retire the slot whose owner is gone.
                    RetireLocked(existing);
                }

                return CreateSlotLocked(pointer, HandleKind.Owned, owner, tag, survivesOwner: false);
            }
        }

        /// <summary>
        ///     Takes one more reference to the resource this handle already names.
        /// </summary>
        /// <returns><see langword="false" /> if the handle is no longer live, in which case nothing was taken.</returns>
        internal static bool AddRef<T>(HandleId<T> id) where T : unmanaged {
            if (id.IsDefault) return false;

            lock (Gate) {
                if (!TryGetLiveSlotLocked(id.Value, out int slot)) return false;
                SegmentOf(slot)[slot & SegmentMask].RefCount++;
                return true;
            }
        }

        /// <summary>
        ///     Releases one reference and retires the slot on the last release. Views do nothing.
        /// </summary>
        /// <returns>
        ///     True when the caller must clean up the resource; false for remaining references or stale handles.
        /// </returns>
        internal static bool Release<T>(HandleId<T> id, out NativePtr<T> pointer, out bool owned)
            where T : unmanaged {
            return Release(id, out pointer, out owned, out _);
        }

        /// <inheritdoc cref="Release{T}(HandleId{T}, out NativePtr{T}, out bool)" />
        /// <param name="id">The handle to retire.</param>
        /// <param name="pointer">The native pointer the caller now has to clean up.</param>
        /// <param name="owned">Whether this process is the one responsible for freeing it.</param>
        /// <param name="owner">The handle to free <paramref name="pointer" /> with, or <c>0</c>.</param>
        internal static bool Release<T>(HandleId<T> id, out NativePtr<T> pointer, out bool owned, out nint owner)
            where T : unmanaged {
            pointer = default;
            owned = false;
            owner = 0;

            // A view never owned anything, so it has nothing to give back.
            if (id.IsDefault || id.IsView) return false;

            lock (Gate) {
                if (!TryGetReleasableSlotLocked(id.Value, out int slot)) return false;

                ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];

                // Only the last reference receives the pointer for cleanup.
                if (--entry.RefCount > 0) return false;

                pointer = new NativePtr<T>(entry.Pointer);
                owned = entry.Kind == HandleKind.Owned;
                owner = ResolveOwnerPointerLocked(entry.Owner);
                RetireLocked(slot);
                return true;
            }
        }

        /// <summary>
        ///     Releases one reference taken through <see cref="AcquireShared{T}" />. Unlike
        ///     <see cref="Release{T}(HandleId{T}, out NativePtr{T}, out bool)" />, this hands back the
        ///     pointer on every call, not just the last: a shared slot stands for a resource the native
        ///     library refcounts itself and re-refs on every hand-out (see the remarks on
        ///     <see cref="AcquireShared{T}" />), so every acquisition owes its own release call, and the
        ///     local slot is retired. Purely as bookkeeping.
        /// </summary>
        /// <returns>
        ///     <see langword="false" /> only for a stale or default handle. Never for a remaining
        ///     reference, since every reference here has native cleanup of its own to do.
        /// </returns>
        internal static bool ReleaseShared<T>(HandleId<T> id, out NativePtr<T> pointer) where T : unmanaged {
            pointer = default;

            if (id.IsDefault || id.IsView) return false;

            lock (Gate) {
                if (!TryGetReleasableSlotLocked(id.Value, out int slot)) return false;

                ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
                pointer = new NativePtr<T>(entry.Pointer);

                // The slot itself only goes away once every reference into it has been released.
                if (--entry.RefCount <= 0) RetireLocked(slot);

                return true;
            }
        }

        /// <summary>
        ///     Moves a live resource under a different owner, for the few SDL calls that re-home one.
        /// </summary>
        /// <returns><see langword="false" /> if the handle is no longer live, in which case nothing moved.</returns>
        internal static bool Reparent<T>(HandleId<T> id, OwnerId owner) where T : unmanaged {
            if (id.IsDefault) return false;

            lock (Gate) {
                if (!TryGetLiveSlotLocked(id.Value, out int slot)) return false;
                SegmentOf(slot)[slot & SegmentMask].Owner = owner.Value;
                return true;
            }
        }

        /// <summary>
        ///     Retires a resource SDL already reclaimed, including views and all outstanding references.
        /// </summary>
        internal static bool Invalidate<T>(HandleId<T> id) where T : unmanaged {
            if (id.IsDefault) return false;

            lock (Gate) {
                if (!TryGetReleasableSlotLocked(id.Value, out int slot)) return false;
                RetireLocked(slot);
                return true;
            }
        }

        /// <summary>Whether this pointer already has a slot, i.e. SDL handed back something we know.</summary>
        internal static bool IsInterned(nint pointer) {
            if (pointer == 0) return false;
            lock (Gate) {
                return Interned.ContainsKey(pointer);
            }
        }

        /// <summary>
        ///     Retires every <typeparamref name="T" /> whose immediate owner is <paramref name="owner" />,
        ///     without freeing anything natively.
        /// </summary>
        /// <returns>How many handles were retired.</returns>
        internal static int InvalidateOwnedBy<T>(OwnerId owner) where T : unmanaged {
            if (owner.IsNone) return 0;

            ushort tag = TypeTag<T>.Value;
            int retired = 0;

            lock (Gate) {
                for (int slot = 0; slot < _slotsUsed; slot++) {
                    ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
                    if (entry.Pointer == 0 || entry.TypeTag != tag || entry.Owner != owner.Value) continue;
                    RetireLocked(slot);
                    retired++;
                }
            }

            return retired;
        }

        /// <summary>
        ///     One live resource an owner has, as <see cref="FindOwned" /> reports it.
        /// </summary>
        internal readonly struct OwnedEntry {
            internal OwnedEntry(ushort tag, long id) {
                Tag = tag;
                Id = id;
            }

            /// <summary>Which type it is - compare against <see cref="TagOf{T}" />.</summary>
            internal readonly ushort Tag;

            /// <summary>Its id, ready to be wrapped in a <see cref="HandleId{T}" /> of the matching type.</summary>
            internal readonly long Id;
        }

        /// <summary>The process-local type tag for <typeparamref name="T" />; never persist it.</summary>
        internal static ushort TagOf<T>() where T : unmanaged {
            return TypeTag<T>.Value;
        }

        /// <summary>
        ///     The live resources whose immediate owner is <paramref name="owner" />.
        /// </summary>
        /// <returns>
        ///     The number of entries written, limited to <paramref name="destination" />'s length.
        /// </returns>
        internal static int FindOwned(OwnerId owner, Span<OwnedEntry> destination) {
            if (owner.IsNone || destination.IsEmpty) return 0;

            int found = 0;

            lock (Gate) {
                if (!IsOwnerChainAliveLocked(owner.Value)) return 0;

                for (int slot = 0; slot < _slotsUsed && found < destination.Length; slot++) {
                    ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
                    if (entry.Pointer == 0 || entry.Owner != owner.Value) continue;
                    destination[found++] = new OwnedEntry(entry.TypeTag, Pack(slot, entry.Generation, view: false));
                }
            }

            return found;
        }
    }
}
