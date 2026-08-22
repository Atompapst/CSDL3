// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Extensions;
using CSDL.File;

namespace CSDL.Input {
    public static class Gamepads {
        private static readonly Dictionary<uint, GamepadItem> _gamepads = new Dictionary<uint, GamepadItem>();

        static Gamepads() {
            Init.InitSubSystem(InitFlags.Gamepad);
            Refresh();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.HasGamepad"/>
        public static bool HasAnyGamepad => SDL.HasGamepad();

        public static IReadOnlyCollection<GamepadItem> Devices => _gamepads.Values;

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadEventsEnabled"/>
        public static bool EventsEnabled {
            get => SDL.GamepadEventsEnabled();
            set => SDL.SetGamepadEventsEnabled(value);
        }

        private static void Refresh() {
            NativePtr<JoystickID> ids = SDL.GetGamepads(out int count).LogIfInvalid();
            if (ids.IsNull) {
                return;
            }

            try {
                _gamepads.Clear();
                for (int i = 0; i < count; i++) {
                    _gamepads[ids[i]] = new GamepadItem(ids[i], 0);
                }
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        internal static void OnGamepadAdded(uint id, ulong timestamp) {
            if (!_gamepads.ContainsKey(id)) {
                _gamepads[id] = new GamepadItem(id, timestamp);
            }
        }

        internal static void OnGamepadRemoved(uint id) {
            _gamepads.Remove(id);
        }

        internal static void OnGamepadUpdated(uint id, ulong timestamp) {
            if (_gamepads.TryGetValue(id, out GamepadItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(uint id) {
            return _gamepads.ContainsKey(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.IsGamepad"/>
        public static bool IsGamepad(uint id) {
            return SDL.IsGamepad(id);
        }

        public static GamepadItem? Get(uint id) {
            return _gamepads.GetValueOrDefault(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepads"/>
        public static JoystickID[] GetConnectedGamepads() {
            NativePtr<JoystickID> ids = SDL.GetGamepads(out int count);
            if (ids == null) {
                return Array.Empty<JoystickID>();
            }

            try {
                return count > 0 ? ids.ToManaged(count) : Array.Empty<JoystickID>();
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.UpdateGamepads"/>
        public static void Update() {
            SDL.UpdateGamepads();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadMappings"/>
        public static string[] GetMappings() {
            IntPtr ptr = SDL.GetGamepadMappings(out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetMappings));
                return Array.Empty<string>();
            }

            try {
                NativePtr<NativePtr<byte>> mappings = ptr;
                string[] result = new string[count];
                for (int i = 0; i < count; i++) {
                    result[i] = mappings[i].ToUtf8String() ?? string.Empty;
                }
                return result;
            }
            finally {
                Memory.Free(ptr);
            }
        }

        // public static string GetMapping(GUID guid) {
        //     string mapping = SDL.GetGamepadMappingForGUID(guid);
        //     if (mapping == null) {
        //         Error.LogError(nameof(SDL.GetGamepadMappingForGUID));
        //     }
        //     return mapping;
        // }

        // public static int AddMapping(string mapping) {
        //     int result = SDL.AddGamepadMapping(mapping);
        //     if (result == -1) {
        //         Error.LogError(nameof(AddMapping));
        //     }
        //     return result;
        // }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.AddGamepadMappingsFromFile"/>
        public static int AddMappingsFromFile(string file) {
            return SDL.AddGamepadMappingsFromFile(file).LogIfInvalid(-1);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.AddGamepadMappingsFromIO"/>
        public static int AddMappingsFromIO(IOStream source, bool closeIO = false) {
            ArgumentNullException.ThrowIfNull(source);

            int result = SDL.AddGamepadMappingsFromIO(source.Handle, closeIO).LogIfInvalid(-1);
            if (closeIO) {
                source.Handle = NativePtr<Opaque.SdlIOStream>.Zero;
            }

            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.ReloadGamepadMappings"/>
        public static bool ReloadMappings() {
            return SDL.ReloadGamepadMappings().LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadTypeFromString"/>
        public static GamepadType GetType(string type) {
            return SDL.GetGamepadTypeFromString(type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadStringForType"/>
        public static string GetTypeString(GamepadType type) {
            return SDL.GetGamepadStringForType(type).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadAxisFromString"/>
        public static GamepadAxis GetAxis(string axis) {
            return SDL.GetGamepadAxisFromString(axis);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadStringForAxis"/>
        public static string GetAxisString(GamepadAxis axis) {
            return SDL.GetGamepadStringForAxis(axis).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadButtonFromString"/>
        public static GamepadButton GetButton(string button) {
            return SDL.GetGamepadButtonFromString(button);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadStringForButton"/>
        public static string GetButtonString(GamepadButton button) {
            return SDL.GetGamepadStringForButton(button).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadButtonLabelForType"/>
        public static GamepadButtonLabel GetButtonLabel(GamepadType type, GamepadButton button) {
            return SDL.GetGamepadButtonLabelForType(type, button);
        }
    }
}
