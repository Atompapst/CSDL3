// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL.Input {
    public static class Keyboards {
        private static readonly Dictionary<uint, KeyboardItem> _keyboards = new Dictionary<uint, KeyboardItem>();

        static Keyboards() {
            Refresh();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.HasKeyboard"/>
        public static bool HasAnyKeyboard => SDL.HasKeyboard();

        public static IReadOnlyCollection<KeyboardItem> Devices => _keyboards.Values;

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetKeyboardFocus"/>
        public static nint FocusedWindow => SDL.GetKeyboardFocus();

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.SetModState"/>
        public static Keymod Modifiers {
            get => SDL.GetModState();
            set => SDL.SetModState(value);
        }

        private static void Refresh() {
            NativePtr<KeyboardID> ids = SDL.GetKeyboards(out int count).LogIfInvalid();
            if (ids.IsNull) {
                return;
            }

            try {
                _keyboards.Clear();

                for (int i = 0; i < count; i++) {
                    KeyboardID id = ids[i];
                    if (!_keyboards.ContainsKey(id)) {
                        _keyboards[id] = new KeyboardItem(id, 0);
                    }
                }
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        internal static void OnKeyboardAdded(uint id, ulong timestamp) {
            if (!_keyboards.ContainsKey(id)) {
                _keyboards[id] = new KeyboardItem(id, timestamp);
            }
        }

        internal static void OnKeyboardRemoved(uint id) {
            _keyboards.Remove(id);
        }

        internal static void OnKeyboardUpdated(uint id, ulong timestamp) {
            if (_keyboards.TryGetValue(id, out KeyboardItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(uint id) {
            return _keyboards.ContainsKey(id);
        }

        public static KeyboardItem? Get(uint id) {
            return _keyboards.GetValueOrDefault(id);
        }

        public static KeyboardID[] GetConnectedKeyboards() {
            NativePtr<KeyboardID> ids = SDL.GetKeyboards(out int count);
            if (ids == null) {
                return Array.Empty<KeyboardID>();
            }

            try {
                return count > 0 ? ids.ToManaged(count) : Array.Empty<KeyboardID>();
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        // public static bool[] GetState() {
        //     ReadOnlySpan<bool> keyStates = SDL.GetKeyboardState(out int numKeys);
        //     bool[] result = new bool[numKeys];
        //
        //     for (int i = 0; i < numKeys; i++) {
        //         result[i] = keyStates[i];
        //     }
        //
        //     return result;
        // }
        //
        // public static bool IsScancodePressed(Scancode scancode) {
        //     ReadOnlySpan<bool> keyStates = SDL.GetKeyboardState(out int numKeys);
        //     int index = (int)scancode;
        //     return index >= 0 && index < numKeys && keyStates[index];
        // }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.ResetKeyboard"/>
        public static void Reset() {
            SDL.ResetKeyboard();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetKeyFromScancode"/>
        public static Keycode GetKeyFromScancode(Scancode scancode, Keymod modstate = 0, bool keyEvent = true) {
            return SDL.GetKeyFromScancode(scancode, modstate, keyEvent);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetScancodeFromKey"/>
        public static Scancode GetScancodeFromKey(Keycode key, out Keymod modstate) {
            modstate = default;
            return SDL.GetScancodeFromKey(key, ref modstate);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.SetScancodeName"/>
        public static bool SetScancodeName(Scancode scancode, string name) {
            return SDL.SetScancodeName(scancode, name).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetScancodeFromName"/>
        public static Scancode GetScancodeByName(string name) {
            return SDL.GetScancodeFromName(name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetScancodeName"/>
        public static string GetScancodeName(Scancode scancode) {
            return SDL.GetScancodeName(scancode).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetKeyFromName"/>
        public static Keycode GetKeyByName(string name) {
            return SDL.GetKeyFromName(name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetKeyName"/>
        public static string GetKeyName(Keycode key) {
            return SDL.GetKeyName(key).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.HasScreenKeyboardSupport"/>
        public static bool HasScreenKeyboardSupport() {
            try {
                return SDL.HasScreenKeyboardSupport();
            } catch {
                return false;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.ScreenKeyboardShown"/>
        public static bool IsScreenKeyboardShown(Window window) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            try {
                return SDL.ScreenKeyboardShown(window.Handle);
            } catch {
                return false;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.StartTextInput"/>
        public static bool StartTextInput(Window window) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return SDL.StartTextInput(window.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.StartTextInputWithProperties"/>
        public static bool StartTextInput(Window window, uint properties) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return SDL.StartTextInputWithProperties(window.Handle, properties).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.TextInputActive"/>
        public static bool TextInputActive(Window window) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return SDL.TextInputActive(window.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.StopTextInput"/>
        public static bool StopTextInput(Window window) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return SDL.StopTextInput(window.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.ClearComposition"/>
        public static bool ClearComposition(Window window) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return SDL.ClearComposition(window.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.SetTextInputArea"/>
        public static bool SetTextInputArea(Window window, Rect rect, int cursor) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return SDL.SetTextInputArea(window.Handle, rect, cursor).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Keyboard.GetTextInputArea"/>
        public static bool GetTextInputArea(Window window, out Rect rect, out int cursor) {
            if (window.Handle.IsNull) {
                throw new ArgumentNullException(nameof(window));
            }

            cursor = 0;
            return SDL.GetTextInputArea(window.Handle, out rect, NativePtr<int>.FromRef(ref cursor)).LogIfFalse();
        }
    }
}
