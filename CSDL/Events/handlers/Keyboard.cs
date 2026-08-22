// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IKeyboardEvents {
        bool HasAnyKeyboard { get; }
        Keymod Modifiers { get; set; }
        nint FocusedWindow { get; }

        bool IsDown(Scancode scancode);
        bool IsUp(Scancode scancode);
        bool PressedThisCycle(Scancode scancode);
        bool ReleasedThisCycle(Scancode scancode);
        bool RepeatedThisCycle(Scancode scancode);

        // Modifier convenience properties
        bool IsShiftPressed { get; }
        bool IsCtrlPressed { get; }
        bool IsAltPressed { get; }
        bool IsGuiPressed { get; }
        bool IsCapsLockOn { get; }
        bool IsNumLockOn { get; }
        bool IsScrollLockOn { get; }

        // Individual modifier checks
        bool IsModifierPressed(Keymod modifier);
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Keyboard : EventHandlerBase, Interfaces.IKeyboardEvents {
        public bool HasAnyKeyboard => Input.SDL.HasKeyboard();

        public Keymod Modifiers {
            get => Input.SDL.GetModState();
            set => Input.SDL.SetModState(value);
        }

        public nint FocusedWindow => Input.SDL.GetKeyboardFocus();

        public bool IsDown(Scancode scancode) {
            return _downKeys.Contains(scancode);
        }

        public bool IsUp(Scancode scancode) {
            return !_downKeys.Contains(scancode);
        }

        public bool PressedThisCycle(Scancode scancode) {
            return _pressedThisCycle.Contains(scancode);
        }

        public bool ReleasedThisCycle(Scancode scancode) {
            return _releasedThisCycle.Contains(scancode);
        }

        public bool RepeatedThisCycle(Scancode scancode) {
            return _repeatedThisCycle.Contains(scancode);
        }

        public bool IsShiftPressed => (_currentModifiers & Keymod.Shift) != 0;
        public bool IsCtrlPressed => (_currentModifiers & Keymod.Ctrl) != 0;
        public bool IsAltPressed => (_currentModifiers & Keymod.Alt) != 0;
        public bool IsGuiPressed => (_currentModifiers & Keymod.Gui) != 0;
        public bool IsCapsLockOn => (_currentModifiers & Keymod.Caps) != 0;
        public bool IsNumLockOn => (_currentModifiers & Keymod.Num) != 0;
        public bool IsScrollLockOn => (_currentModifiers & Keymod.Scroll) != 0;

        public bool IsModifierPressed(Keymod modifier) {
            return (_currentModifiers & modifier) != 0;
        }

        protected override void ResetState() {
            ResetDeviceState();
            ResetKeyState();
            ResetTextInputState();
            ResetTextEditingState();
        }

        partial void ResetDeviceState();
        partial void ResetKeyState();
        partial void ResetTextInputState();
        partial void ResetTextEditingState();
    }
}
