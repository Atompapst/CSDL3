// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Threading;

namespace CSDL {
    internal static partial class HandleTable {
        /// <summary>Marks an id as a view of somebody else's slot. Lives in the id, never in the slot.</summary>
        internal const uint ViewFlag = 0x8000_0000u;

        internal const uint SlotMask = 0x7FFF_FFFFu;

        private const int SegmentShift = 10;
        private const int SegmentSize = 1 << SegmentShift;
        private const int SegmentMask = SegmentSize - 1;

        private static readonly object Gate = new object();
        private static readonly Stack<int> FreeSlots = new Stack<int>();

        /// <summary>Maps a live native pointer back to its slot, so two views of one resource share an identity.</summary>
        private static readonly Dictionary<nint, int> Interned = new Dictionary<nint, int>();

        /// <summary>Type names for diagnostics, indexed by <see cref="Slot.TypeTag" />.</summary>
        private static readonly List<string> TypeNames = new List<string> { "<none>" };

        private static Slot[]?[] _segments = new Slot[]?[4];
        private static int _slotsUsed;

        /// <summary>Retires since the last sweep; zero means nothing can have been orphaned.</summary>
        private static int _retiresSinceSweep;

        /// <summary>Rents since the last sweep, so its cost can be amortised against them.</summary>
        private static int _rentsSinceSweep;

        /// <summary>Drops all bookkeeping. Only valid once SDL itself has been torn down.</summary>
        internal static void Reset() {
            lock (Gate) {
                Volatile.Write(ref _segments, new Slot[]?[4]);
                FreeSlots.Clear();
                Interned.Clear();
                _slotsUsed = 0;
                _retiresSinceSweep = 0;
                _rentsSinceSweep = 0;
            }
        }

        /// <param name="typeTag">
        ///     <see cref="TypeTag{T}" /> for <typeparamref name="T" />, resolved by the caller before it
        ///     took <see cref="Gate" /> - see the remarks on <see cref="TypeTag{T}" />.
        /// </param>
        private static HandleId<T> CreateSlotLocked<T>(NativePtr<T> pointer, HandleKind kind, OwnerId owner,
            ushort typeTag, bool survivesOwner) where T : unmanaged {
            int slot = RentLocked();
            ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
            entry.Pointer = pointer.Ptr;
            entry.Kind = kind;
            entry.Owner = owner.Value;
            entry.TypeTag = typeTag;
            entry.SurvivesOwner = survivesOwner;
            entry.RefCount = 1;
            Interned[pointer.Ptr] = slot;
            return new HandleId<T>(Pack(slot, entry.Generation, view: false));
        }

        /// <summary>Empties a slot; its new generation also makes descendants fail resolution.</summary>
        private static void RetireLocked(int slot) {
            ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
            Interned.Remove(entry.Pointer);
            entry.Pointer = 0;
            entry.Kind = HandleKind.Borrowed;
            entry.Owner = 0;
            entry.TypeTag = 0;
            entry.SurvivesOwner = false;
            entry.RefCount = 0;
            entry.Generation++;
            _retiresSinceSweep++;

            // Never reuse a slot after generation overflow; old handles must stay stale.
            if (entry.Generation != 0) FreeSlots.Push(slot);
        }

        private static int RentLocked() {
            _rentsSinceSweep++;

            if (FreeSlots.Count > 0) return FreeSlots.Pop();
            
            if (ShouldSweepLocked()) {
                SweepOrphansLocked();
                if (FreeSlots.Count > 0) return FreeSlots.Pop();
            }

            int slot = _slotsUsed;
            int segment = slot >> SegmentShift;

            if (segment >= _segments.Length) {
                Slot[]?[] grown = new Slot[]?[_segments.Length * 2];
                Array.Copy(_segments, grown, _segments.Length);
                // Keep existing segments valid for readers holding the old outer array.
                Volatile.Write(ref _segments, grown);
            }

            if (_segments[segment] == null) {
                Volatile.Write(ref _segments[segment], new Slot[SegmentSize]);
            }

            _slotsUsed = slot + 1;
            SegmentOf(slot)[slot & SegmentMask].Generation = 1;
            return slot;
        }

        /// <summary>
        ///     Whether a scan for orphaned slots is worth its cost right now.
        /// </summary>
        private static bool ShouldSweepLocked() {
            return _retiresSinceSweep > 0 && _rentsSinceSweep >= (_slotsUsed / 2) + 8;
        }

        /// <summary>
        ///     Retires every slot whose owner is gone, and reports how many that was.
        /// </summary>
        /// <remarks>
        ///     A slot marked <see cref="Slot.SurvivesOwner" /> is left alone.
        ///     It is the one kind that still owes SDL a call.
        /// </remarks>
        private static int SweepOrphansLocked() {
            _retiresSinceSweep = 0;
            _rentsSinceSweep = 0;

            int reclaimed = 0;

            for (int slot = 0; slot < _slotsUsed; slot++) {
                ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
                if (entry.Pointer == 0 || entry.Owner == 0 || entry.SurvivesOwner) continue;
                if (IsOwnerChainAliveLocked(entry.Owner)) continue;

                RetireLocked(slot);
                reclaimed++;
            }

            // RetireLocked counted every one of those; they are the sweep, not work for the next one.
            _retiresSinceSweep = 0;
            return reclaimed;
        }

        private static Slot[] SegmentOf(int slot) {
            return _segments[slot >> SegmentShift]!;
        }

        private static long Pack(int slot, uint generation, bool view) {
            return ((long)generation << 32) | (uint)slot | (view ? ViewFlag : 0u);
        }

        private static ushort RegisterTypeName(string name) {
            lock (Gate) {
                TypeNames.Add(name);
                return (ushort)(TypeNames.Count - 1);
            }
        }

        /// <summary>A type's index into <see cref="TypeNames" />, resolved once per type by the JIT.</summary>
        private static class TypeTag<T> where T : unmanaged {
            internal static readonly ushort Value = RegisterTypeName(typeof(T).Name);
        }

        private struct Slot {
            /// <summary>The native pointer, or <c>0</c> when the slot is free.</summary>
            public nint Pointer;

            /// <summary>
            ///     The <see cref="OwnerId" /> of whoever SDL destroys this resource together with, or <c>0</c>.
            /// </summary>
            public long Owner;

            /// <summary>Bumped on every release; starts at 1 so that a zeroed id never matches.</summary>
            public uint Generation;

            /// <summary>Index into <see cref="TypeNames" />, so a leak can say what leaked.</summary>
            public ushort TypeTag;

            /// <summary>Whether this process is responsible for freeing <see cref="Pointer" />.</summary>
            public HandleKind Kind;

            /// <summary>
            ///     Whether the resource is still ours to free after <see cref="Owner" /> is gone.
            /// </summary>
            /// <remarks>
            ///     Almost nothing is. The one case SDL documents is a renderer: <c>SDL_DestroyWindow</c>
            ///     calls <c>SDL_DestroyRendererWithoutFreeing</c>, which tears the renderer down but
            ///     deliberately leaves the allocation alone so that an app may destroy window and
            ///     renderer in either order. Only <c>SDL_DestroyRenderer</c> frees it.
            /// </remarks>
            public bool SurvivesOwner;

            /// <summary>
            ///     How many outstanding handles have to be released before the resource is freed.
            /// </summary>
            public int RefCount;
        }
    }
}
