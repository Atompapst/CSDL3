// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    internal readonly record struct CBool {
        private readonly byte _value;

        private const byte FalseValue = 0;
        private const byte TrueValue = 1;

        private CBool(bool b) {
            _value = b ? TrueValue : FalseValue;
        }

        internal CBool(byte value) {
            _value = value;
        }

        public static implicit operator bool(CBool b) {
            return b._value != FalseValue;
        }

        public static implicit operator CBool(bool b) {
            return new CBool(b ? TrueValue : FalseValue);
        }

        public bool Equals(CBool other) {
            return (bool)other == (bool)this;
        }

        public override int GetHashCode() {
            return ((bool)this).GetHashCode();
        }
    }
}
