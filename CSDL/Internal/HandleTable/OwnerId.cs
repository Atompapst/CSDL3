// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    /// <summary>The untyped identity of the resource SDL destroys a child together with.</summary>
    internal readonly struct OwnerId : IEquatable<OwnerId> {
        internal readonly long Value;

        internal OwnerId(long value) {
            Value = value;
        }

        /// <summary>No parent resource; lifetime is bounded by <see cref="Init.Quit" />.</summary>
        internal static OwnerId None => default;

        internal bool IsNone => Value == 0;

        public bool Equals(OwnerId other) {
            return Value == other.Value;
        }

        public override bool Equals(object? obj) {
            return obj is OwnerId other && Equals(other);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public static bool operator ==(OwnerId left, OwnerId right) {
            return left.Value == right.Value;
        }

        public static bool operator !=(OwnerId left, OwnerId right) {
            return left.Value != right.Value;
        }

        public override string ToString() {
            return IsNone ? "<root>" : HandleTable.Describe(Value);
        }
    }
}
