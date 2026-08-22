// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;

namespace CSDL.Input {
    public static class Pens {
        private static readonly Dictionary<uint, PenItem> _pens = new Dictionary<uint, PenItem>();

        public static IReadOnlyCollection<PenItem> Devices => _pens.Values;

        internal static void OnPenAdded(uint id, ulong timestamp) {
            if (!_pens.ContainsKey(id)) {
                _pens[id] = new PenItem(id, timestamp);
            }
        }

        internal static void OnPenRemoved(uint id) {
            _pens.Remove(id);
        }

        internal static void OnPenUpdated(uint id, ulong timestamp) {
            if (_pens.TryGetValue(id, out PenItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(uint id) {
            return _pens.ContainsKey(id);
        }

        public static PenItem? Get(uint id) {
            return _pens.GetValueOrDefault(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pen.GetPenDeviceType"/>
        public static PenDeviceType GetDeviceType(uint id) {
            return SDL.GetPenDeviceType(id);
        }
    }
}
