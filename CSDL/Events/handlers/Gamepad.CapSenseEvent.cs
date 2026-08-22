// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        GamepadCapSenseEvent? LastCapSenseEvent { get; }
        int CapSenseCount { get; }

        event Action<GamepadCapSenseEvent>? CapSenseTouch;
        event Action<GamepadCapSenseEvent>? CapSenseRelease;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad {
        private Counter _capSenseCount;

        public GamepadCapSenseEvent? LastCapSenseEvent { get; private set; }
        public int CapSenseCount => _capSenseCount;

        public event Action<GamepadCapSenseEvent>? CapSenseTouch;
        public event Action<GamepadCapSenseEvent>? CapSenseRelease;

        internal void Handle(GamepadCapSenseEvent capSenseEvent) {
            Input.Gamepads.OnGamepadUpdated(capSenseEvent.Which, capSenseEvent.Timestamp);

            LastCapSenseEvent = capSenseEvent;
            IncrementCounter(ref _capSenseCount);

            switch (capSenseEvent.Type) {
                case EventType.GamepadCapsenseTouch:
                    CapSenseTouch?.Invoke(capSenseEvent);
                    break;
                case EventType.GamepadCapsenseRelease:
                    CapSenseRelease?.Invoke(capSenseEvent);
                    break;
            }
        }

        partial void ResetCapSenseState() {
            if (!_capSenseCount.HasEvents) return;
            _capSenseCount.Reset();
        }
    }
}
