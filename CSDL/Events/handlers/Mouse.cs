// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IMouseEvents {
        bool HasAnyMouse { get; }
        bool IsDown(MouseButtonFlags button);
        bool IsUp(MouseButtonFlags button);
        bool PressedThisCycle(MouseButtonFlags button);
        bool ReleasedThisCycle(MouseButtonFlags button);
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Mouse : EventHandlerBase, Interfaces.IMouseEvents {
        public bool HasAnyMouse => Input.SDL.HasMouse();

        public bool IsDown(MouseButtonFlags button) {
            return _buttonStates.TryGetValue(button, out bool down) && down;
        }

        public bool IsUp(MouseButtonFlags button) {
            return !IsDown(button);
        }

        public bool PressedThisCycle(MouseButtonFlags button) {
            return _pressedThisCycle.Contains(button);
        }

        public bool ReleasedThisCycle(MouseButtonFlags button) {
            return _releasedThisCycle.Contains(button);
        }

        protected override void ResetState() {
            ResetMotionState();
            ResetButtonState();
            ResetWheelState();
            ResetDeviceState();
        }

        partial void ResetMotionState();
        partial void ResetButtonState();
        partial void ResetWheelState();
        partial void ResetDeviceState();
    }
}
