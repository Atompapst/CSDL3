// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class TouchItem {
        public ulong Id { get; }
        public ulong LastTimestampNs { get; internal set; }

        internal TouchItem(ulong id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Touch.GetTouchDeviceName"/>
        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Touch.GetTouchDeviceType"/>
        public TouchDeviceType DeviceType => SDL.GetTouchDeviceType(Id);

        private string GetName() {
            return SDL.GetTouchDeviceName(Id).ToUtf8StringOrLog() ?? "Unknown Touch Device";
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {DeviceType})";
        }
    }
}
