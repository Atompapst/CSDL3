// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Input;
using CSDL3.Tests.TestSupport;
using Sdl = CSDL;

namespace CSDL3.Tests.Input {
    [Collection(SdlCollection.Name)]
    public sealed class InputNativeTests {
        [Fact]
        public void VirtualGamepad_DerivesControlsFromItsMasksAndReturnsBindings() {
            uint axisMask = 1u << (int)GamepadAxis.Leftx;
            uint buttonMask = 1u << (int)GamepadButton.South;
            using VirtualJoystick virtualJoystick = VirtualJoystick.Attach(
                JoystickType.Gamepad,
                axisMask: axisMask,
                buttonMask: buttonMask);
            using JoystickDevice joystick = new JoystickDevice(virtualJoystick.Id);
            using GamepadDevice gamepad = new GamepadDevice(virtualJoystick.Id);

            Assert.Equal(1, joystick.NumAxes);
            Assert.Equal(1, joystick.NumButtons);
            Assert.True(joystick.SetVirtualAxis(0, 123));
            Assert.True(joystick.SetVirtualButton(0, true));
            Assert.NotEmpty(gamepad.GetBindings());
        }

        [Fact]
        public void VirtualJoystick_ExposesExplicitControlCountsAndContainsCleanupExceptions() {
            bool cleanupCalled = false;
            VirtualJoystick virtualJoystick = VirtualJoystick.Attach(
                JoystickType.Wheel,
                axes: 2,
                buttons: 3,
                balls: 1,
                hats: 1,
                cleanup: _ => {
                    cleanupCalled = true;
                    throw new InvalidOperationException("Expected test exception.");
                });

            try {
                using JoystickDevice joystick = new JoystickDevice(virtualJoystick.Id);
                Assert.Equal(2, joystick.NumAxes);
                Assert.Equal(3, joystick.NumButtons);
                Assert.Equal(1, joystick.NumHats);
                Assert.True(joystick.SetVirtualBall(0, 2, -2));
                Assert.True(joystick.SetVirtualHat(0, 1));
            }
            finally {
                virtualJoystick.Dispose();
            }

            Assert.True(cleanupCalled);
        }

        [Fact]
        public void SimulatedInputIdentifiers_MatchSdlSentinelValues() {
            Assert.Equal(uint.MaxValue, Macros.TouchMouseID.Value);
            Assert.Equal(ulong.MaxValue, Macros.MouseTouchID.Value);
        }

        [Fact]
        public void ConnectedDeviceQueries_ReturnManagedSnapshots() {
            _ = Keyboards.GetConnectedKeyboards();
            _ = Mouse.GetConnectedMice();
            _ = Gamepads.GetConnectedGamepads();
            _ = Haptics.GetConnected();
        }
    }
}
