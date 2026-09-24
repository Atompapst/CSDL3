// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSDL {
    internal static partial class HandleTable {
        /// <summary>
        ///     How far up an owner chain a resolve will walk before giving up.
        /// </summary>
        private const int MaxOwnerDepth = 8;

        /// <summary>
        ///     The hot path: one segment lookup and a generation compare per level of ownership.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     The handle was disposed, an owner above it was destroyed, or it is <see langword="default" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativePtr<T> Resolve<T>(HandleId<T> id) where T : unmanaged {
            if (TryResolve(id, out NativePtr<T> pointer)) return pointer;
            throw Stale<T>();
        }

        /// <summary>
        ///     Resolves the handle and the native pointer of its owner, in one walk.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The handle is no longer live.</exception>
        internal static NativePtr<T> Resolve<T>(HandleId<T> id, out nint owner) where T : unmanaged {
            if (TryResolve(id, out NativePtr<T> pointer, out owner)) return pointer;
            throw Stale<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryResolve<T>(HandleId<T> id, out NativePtr<T> pointer) where T : unmanaged {
            return TryResolve(id, out pointer, out _);
        }

        private static bool TryResolve<T>(HandleId<T> id, out NativePtr<T> pointer, out nint owner)
            where T : unmanaged {
            Slot[]?[] segments = Volatile.Read(ref _segments);

            if (TryLookup(segments, id.Value, out nint raw, out long ownerId)
                && TryResolveOwnerChain(segments, ownerId, out owner)) {
                pointer = new NativePtr<T>(raw);
                return true;
            }

            pointer = default;
            owner = 0;
            return false;
        }

        /// <summary>The raw pointer, or <c>0</c> for a handle that is no longer live - never throws.</summary>
        internal static nint PointerOrZero<T>(HandleId<T> id) where T : unmanaged {
            return TryResolve(id, out NativePtr<T> pointer) ? pointer.Ptr : 0;
        }

        internal static bool IsLive<T>(HandleId<T> id) where T : unmanaged {
            return TryResolve(id, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryLookup(Slot[]?[] segments, long id, out nint pointer, out long owner) {
            pointer = 0;
            owner = 0;
            if (id == 0) return false;

            int slot = (int)((uint)id & SlotMask);
            int segment = slot >> SegmentShift;
            if ((uint)segment >= (uint)segments.Length) return false;

            Slot[]? chunk = Volatile.Read(ref segments[segment]);
            if (chunk == null) return false;

            ref Slot entry = ref chunk[slot & SegmentMask];
            nint raw = entry.Pointer;
            if (raw == 0 || entry.Generation != (uint)(id >> 32)) return false;

            pointer = raw;
            owner = entry.Owner;
            return true;
        }

        /// <summary>
        ///     Walks a resource's ancestry and reports the immediate owner's pointer along the way.
        /// </summary>
        private static bool TryResolveOwnerChain(Slot[]?[] segments, long owner, out nint ownerPointer) {
            ownerPointer = 0;
            if (owner == 0) return true;

            if (!TryLookup(segments, owner, out ownerPointer, out long next)) return false;

            for (int depth = 1; next != 0; depth++) {
                if (depth >= MaxOwnerDepth) return false;
                if (!TryLookup(segments, next, out _, out long above)) return false;
                next = above;
            }

            return true;
        }

        private static bool IsOwnerChainAliveLocked(long owner) {
            return TryResolveOwnerChain(_segments, owner, out _);
        }

        private static nint ResolveOwnerPointerLocked(long owner) {
            return TryResolveOwnerChain(_segments, owner, out nint pointer) ? pointer : 0;
        }

        private static bool TryGetLiveSlotLocked(long id, out int slot) {
            if (!TryLookup(_segments, id, out _, out long owner)) {
                slot = 0;
                return false;
            }

            slot = (int)((uint)id & SlotMask);
            return IsOwnerChainAliveLocked(owner);
        }

        /// <summary>
        ///     The slot this id names, if it is still this handle's to give back.
        /// </summary>
        /// <remarks>
        ///     Usually the same question as <see cref="TryGetLiveSlotLocked" />: a resource whose owner
        ///     SDL destroyed was destroyed with it, so there is nothing left to hand over and nothing
        ///     left to retire. The exception is a slot marked <c>SurvivesOwner</c> - a renderer, whose
        ///     allocation <c>SDL_DestroyWindow</c> deliberately leaves behind. Its own generation still
        ///     matching is enough, and giving it back here is also what finally retires the slot.
        /// </remarks>
        private static bool TryGetReleasableSlotLocked(long id, out int slot) {
            if (!TryLookup(_segments, id, out _, out long owner)) {
                slot = 0;
                return false;
            }

            slot = (int)((uint)id & SlotMask);
            if (IsOwnerChainAliveLocked(owner)) return true;

            return SegmentOf(slot)[slot & SegmentMask].SurvivesOwner;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectDisposedException Stale<T>() where T : unmanaged {
            return new ObjectDisposedException(
                typeof(T).Name,
                "The handle was disposed, was destroyed together with its owner, or was never initialised.");
        }
    }
}
