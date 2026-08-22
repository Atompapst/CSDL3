// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Input {
    public sealed class PenItem {
        public uint Id { get; }
        public ulong LastTimestampNs { get; internal set; }

        internal PenItem(uint id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pen.GetPenDeviceType"/>
        public PenDeviceType DeviceType => SDL.GetPenDeviceType(Id);

        public override string ToString() {
            return $"Pen (ID: {Id}, Type: {DeviceType})";
        }
    }
}
