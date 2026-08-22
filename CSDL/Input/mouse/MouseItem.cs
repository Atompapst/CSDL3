// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class MouseItem {
        public MouseID Id { get; }
        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetMouseNameForID"/>
        public string Name { get; }
        public ulong LastTimestampNs { get; internal set; }

        internal MouseItem(MouseID id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
            Name = GetName(id) ?? "Unknown Mouse";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetMouseNameForID"/>
        private static string? GetName(MouseID id) {
            return SDL.GetMouseNameForID(id).ToUtf8StringOrLog();
        }

        public override string ToString() {
            return $"{Name} (ID: {Id})";
        }
    }
}
