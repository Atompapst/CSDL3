// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class KeyboardItem {
        public uint Id { get; }
        public string Name { get; }
        public ulong LastTimestampNs { get; internal set; }

        internal KeyboardItem(uint id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
            Name = GetName(id) ?? "Unknown Keyboard";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetKeyboardNameForID"/>
        private static string? GetName(uint id) {
            return SDL.GetKeyboardNameForID(id).ToUtf8StringOrLog();
        }

        public override string ToString() {
            return $"{Name} (ID: {Id})";
        }
    }
}
