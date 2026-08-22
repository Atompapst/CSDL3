// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;

namespace CSDL.Input {
    public static class Touch {
        private static readonly Dictionary<TouchID, TouchItem> _touchDevices = new Dictionary<TouchID, TouchItem>();

        static Touch() {
            Init.InitSubSystem(InitFlags.Video);
            Refresh();
        }

        public static IReadOnlyCollection<TouchItem> Devices => _touchDevices.Values;

        private static void Refresh() {
            NativePtr<TouchID> ids = SDL.GetTouchDevices(out int count);
            if (ids == null) {
                return;
            }

            try {
                _touchDevices.Clear();
                for (int i = 0; i < count; i++) {
                    _touchDevices[ids[i]] = new TouchItem(ids[i], 0);
                }
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        internal static void OnTouchUpdated(ulong id, ulong timestamp) {
            if (!_touchDevices.TryGetValue(id, out TouchItem? item)) {
                _touchDevices[id] = new TouchItem(id, timestamp);
                return;
            }
            item.LastTimestampNs = timestamp;
        }

        public static bool IsPresent(ulong id) {
            return _touchDevices.ContainsKey(id);
        }

        public static TouchItem? Get(ulong id) {
            return _touchDevices.GetValueOrDefault(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Touch.GetTouchFingers"/>
        public static Finger[] GetFingers(ulong touchID) {
            IntPtr ptr = SDL.GetTouchFingers(touchID, out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetFingers));
                return System.Array.Empty<Finger>();
            }

            try {
                NativePtr<NativePtr<Finger>> fingers = ptr;
                Finger[] result = new Finger[count];
                for (int i = 0; i < count; i++) {
                    result[i] = fingers[i].Read();
                }
                return result;
            }
            finally {
                Memory.Free(ptr);
            }
        }
    }
}
