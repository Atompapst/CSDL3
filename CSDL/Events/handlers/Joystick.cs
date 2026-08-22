// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        short GetAxis(uint id, byte axis);
        bool IsButtonDown(uint id, byte button);
        bool IsButtonUp(uint id, byte button);
        bool PressedThisCycle(uint id, byte button);
        bool ReleasedThisCycle(uint id, byte button);
        byte GetHat(uint id, byte hat);
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick : EventHandlerBase, Interfaces.IJoystickEvents {
        public short GetAxis(uint id, byte axis) {
            if (_axisStates.TryGetValue(id, out Dictionary<byte, short>? axes) &&
                axes.TryGetValue(axis, out short value)) {
                return value;
            }
            return 0;
        }

        public bool IsButtonDown(uint id, byte button) {
            return _buttonStates.TryGetValue(id, out HashSet<byte>? buttons) && buttons.Contains(button);
        }

        public bool IsButtonUp(uint id, byte button) {
            return !IsButtonDown(id, button);
        }

        public bool PressedThisCycle(uint id, byte button) {
            return _pressedThisCycle.TryGetValue(id, out HashSet<byte>? buttons) && buttons.Contains(button);
        }

        public bool ReleasedThisCycle(uint id, byte button) {
            return _releasedThisCycle.TryGetValue(id, out HashSet<byte>? buttons) && buttons.Contains(button);
        }

        public byte GetHat(uint id, byte hat) {
            if (_hatStates.TryGetValue(id, out Dictionary<byte, byte>? hats) &&
                hats.TryGetValue(hat, out byte value)) {
                return value;
            }
            return 0;
        }

        protected override void ResetState() {
            ResetDeviceState();
            ResetAxisState();
            ResetBallState();
            ResetHatState();
            ResetButtonState();
            ResetBatteryState();
        }

        partial void ResetDeviceState();
        partial void ResetAxisState();
        partial void ResetBallState();
        partial void ResetHatState();
        partial void ResetButtonState();
        partial void ResetBatteryState();
    }
}
