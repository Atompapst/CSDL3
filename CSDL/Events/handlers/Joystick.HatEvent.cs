// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        JoyHatEvent? LastHatEvent { get; }
        int HatCount { get; }

        event Action<JoyHatEvent>? HatChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick {
        private readonly Dictionary<uint, Dictionary<byte, byte>> _hatStates = new Dictionary<uint, Dictionary<byte, byte>>();

        private Counter _hatCount;

        public JoyHatEvent? LastHatEvent { get; private set; }
        public int HatCount => _hatCount;

        public event Action<JoyHatEvent>? HatChanged;

        internal void Handle(JoyHatEvent hatEvent) {
            Input.Joysticks.OnJoystickUpdated(hatEvent.Which, hatEvent.Timestamp);
            EnsureHatStateContainer(hatEvent.Which);

            _hatStates[hatEvent.Which][hatEvent.Hat] = hatEvent.Value;

            LastHatEvent = hatEvent;
            IncrementCounter(ref _hatCount);

            HatChanged?.Invoke(hatEvent);
        }

        private void EnsureHatStateContainer(uint id) {
            if (!_hatStates.ContainsKey(id)) {
                _hatStates[id] = new Dictionary<byte, byte>();
            }
        }

        private void RemoveHatState(uint id) {
            _hatStates.Remove(id);
        }

        partial void ResetHatState() {
            if (!_hatCount.HasEvents) return;
            _hatCount.Reset();
        }
    }
}
