// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Input {
    /// <summary>
    /// A joystick that exists without hardware directly backing it, with program-supplied inputs -
    /// matches <c>SDL_AttachVirtualJoystick</c>/<c>SDL_DetachVirtualJoystick</c>.
    /// </summary>
    public sealed class VirtualJoystick : IDisposable {
        private readonly string _callbackId;
        private bool _disposed;

        /// <summary>The instance ID SDL assigned this virtual joystick, usable with every other joystick/gamepad API.</summary>
        public JoystickID Id { get; }

        private VirtualJoystick(JoystickID id, string callbackId) {
            Id = id;
            _callbackId = callbackId;
        }

        /// <summary>
        /// Attaches a new virtual joystick, matching <c>SDL_AttachVirtualJoystick</c>. Every callback
        /// parameter is optional (<see langword="null"/>), matching SDL's own "all elements of this
        /// structure are optional" contract.
        /// </summary>
        public static VirtualJoystick Attach(
            JoystickType type,
            ushort vendorId = 0,
            ushort productId = 0,
            uint buttonMask = 0,
            uint axisMask = 0,
            string? name = null,
            VirtualJoystickTouchpadDesc[]? touchpads = null,
            VirtualJoystickSensorDesc[]? sensors = null,
            VirtualJoystickDesc.UpdateDelegate? update = null,
            VirtualJoystickDesc.SetPlayerIndexDelegate? setPlayerIndex = null,
            VirtualJoystickDesc.RumbleDelegate? rumble = null,
            VirtualJoystickDesc.RumbleTriggersDelegate? rumbleTriggers = null,
            VirtualJoystickDesc.SetLedDelegate? setLed = null,
            VirtualJoystickDesc.SendEffectDelegate? sendEffect = null,
            VirtualJoystickDesc.SetSensorsEnabledDelegate? setSensorsEnabled = null,
            VirtualJoystickDesc.CleanupDelegate? cleanup = null,
            object? userData = null,
            ushort axes = 0,
            ushort buttons = 0,
            ushort balls = 0,
            ushort hats = 0) {
            VirtualJoystickDesc desc = default;
            desc.InitVersion();
            desc.Type = (ushort)type;
            desc.VendorID = vendorId;
            desc.ProductID = productId;
            desc.Naxes = axes;
            desc.Nbuttons = buttons;
            desc.Nballs = balls;
            desc.Nhats = hats;
            desc.ButtonMask = buttonMask;
            desc.AxisMask = axisMask;

            if (type == JoystickType.Gamepad) {
                desc.Naxes = axes != 0 ? axes : CountSetBits(axisMask);
                desc.Nbuttons = buttons != 0 ? buttons : CountSetBits(buttonMask);
            }

            string callbackId = desc.Attach(
                name, touchpads, sensors,
                update, setPlayerIndex, rumble, rumbleTriggers, setLed, sendEffect, setSensorsEnabled, cleanup,
                userData);

            JoystickID id = SDL.AttachVirtualJoystick(in desc);
            if (id.Value == 0) {
                VirtualJoystickDesc.Detach(callbackId);
                Error.ThrowIfError(nameof(Attach));
            }

            return new VirtualJoystick(id, callbackId);
        }

        private static ushort CountSetBits(uint mask) {
            ushort count = 0;
            while (mask != 0) {
                count += (ushort)(mask & 1);
                mask >>= 1;
            }
            return count;
        }

        public void Dispose() {
            if (_disposed) return;
            _disposed = true;

            // SDL invokes the app's Cleanup callback (if any) during this call - Detach below only
            // needs to run afterward, to tear down the managed side (CallbackRegistry entries, the
            // name/touchpad/sensor unmanaged allocations).
            SDL.DetachVirtualJoystick(Id).LogIfFalse();
            VirtualJoystickDesc.Detach(_callbackId);
        }
    }
}
