// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Extensions;

namespace CSDL.Input {
    public static class Joysticks {
        private static readonly Dictionary<uint, JoystickItem> _joysticks = new Dictionary<uint, JoystickItem>();

        static Joysticks() {
            Init.InitSubSystem(InitFlags.Joystick);
            Refresh();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.HasJoystick"/>
        public static bool HasAnyJoystick => SDL.HasJoystick();

        public static IReadOnlyCollection<JoystickItem> Devices => _joysticks.Values;

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickEventsEnabled"/>
        public static bool EventsEnabled {
            get => SDL.JoystickEventsEnabled();
            set => SDL.SetJoystickEventsEnabled(value);
        }

        private static void Refresh() {
            NativePtr<JoystickID> ids = SDL.GetJoysticks(out int count).LogIfInvalid();
            if (ids.IsNull) {
                return;
            }
            try {
                _joysticks.Clear();
                for (int i = 0; i < count; i++) {
                    uint id = ids[i];
                    if (!_joysticks.ContainsKey(id)) {
                        _joysticks[id] = new JoystickItem(id, 0);
                    }
                }
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        internal static void OnJoystickAdded(uint id, ulong timestamp) {
            if (!_joysticks.ContainsKey(id)) {
                _joysticks[id] = new JoystickItem(id, timestamp);
            }
        }

        internal static void OnJoystickRemoved(uint id) {
            _joysticks.Remove(id);
        }

        internal static void OnJoystickUpdated(uint id, ulong timestamp) {
            if (_joysticks.TryGetValue(id, out JoystickItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(uint id) {
            return _joysticks.ContainsKey(id);
        }

        public static JoystickItem? Get(uint id) {
            return _joysticks.GetValueOrDefault(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickGUIDInfo"/>
        public static void GetGUIDDataInfo(GUIDData guid, out ushort vendor, out ushort product, out ushort version, out ushort crc16) {
            SDL.GetJoystickGUIDInfo(guid, out vendor, out product, out version, out crc16);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.UpdateJoysticks"/>
        public static void Update() {
            SDL.UpdateJoysticks();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.LockJoysticks"/>
        public static void Lock() {
            SDL.LockJoysticks();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.TryLockJoysticks"/>
        public static bool TryLock() {
            return SDL.TryLockJoysticks();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.UnlockJoysticks"/>
        public static void Unlock() {
            SDL.UnlockJoysticks();
        }
    }
}
