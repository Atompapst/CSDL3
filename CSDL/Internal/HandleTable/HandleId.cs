// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    /// <summary>
    ///     A typed slot and generation in <see cref="HandleTable" />. Default is never live.
    /// </summary>
    internal readonly struct HandleId<T> : IEquatable<HandleId<T>> where T : unmanaged {
        internal readonly long Value;

        internal HandleId(long value) {
            Value = value;
        }

        internal int Slot => (int)((uint)Value & HandleTable.SlotMask);

        internal uint Generation => (uint)(Value >> 32);

        /// <summary>
        ///     Whether this is the zero handle - one that was never given a resource, as opposed
        ///     to one whose resource is gone.
        /// </summary>
        /// <remarks>
        ///     The two are different things and the distinction matters wherever SDL takes an
        ///     optional handle: <see langword="default" /> means "no argument", while a handle whose
        ///     resource was destroyed still has to throw rather than quietly become "no argument".
        ///     A zero handle is never live, but not every invalid handle is zero.
        /// </remarks>
        internal bool IsDefault => Value == 0;

        /// <summary>A non-owning view: same identity, ignored by release.</summary>
        internal bool IsView => ((uint)Value & HandleTable.ViewFlag) != 0;

        /// <summary>This handle seen as something other resources can be created against.</summary>
        internal OwnerId AsOwner => new OwnerId(Value & ~(long)HandleTable.ViewFlag);

        public bool Equals(HandleId<T> other) {
            // The view bit says who releases, not what is addressed: a view and the handle it
            // was derived from sit on the same slot, so identity has to ignore it.
            return (Value & ~(long)HandleTable.ViewFlag) == (other.Value & ~(long)HandleTable.ViewFlag);
        }

        public override bool Equals(object? obj) {
            return obj is HandleId<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return (Value & ~(long)HandleTable.ViewFlag).GetHashCode();
        }

        public static bool operator ==(HandleId<T> left, HandleId<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(HandleId<T> left, HandleId<T> right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return IsDefault ? "<default>" : HandleTable.Describe(Value);
        }
    }
}
