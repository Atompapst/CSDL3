// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        JoyButtonEvent? LastButtonEvent { get; }
        int ButtonCount { get; }

        event Action<JoyButtonEvent>? ButtonChanged;
        event Action<JoyButtonEvent>? ButtonDown;
        event Action<JoyButtonEvent>? ButtonUp;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick {
        private readonly Dictionary<uint, HashSet<byte>> _buttonStates = new Dictionary<uint, HashSet<byte>>();
        private readonly Dictionary<uint, HashSet<byte>> _pressedThisCycle = new Dictionary<uint, HashSet<byte>>();
        private readonly Dictionary<uint, HashSet<byte>> _releasedThisCycle = new Dictionary<uint, HashSet<byte>>();

        private Counter _buttonCount;

        public JoyButtonEvent? LastButtonEvent { get; private set; }
        public int ButtonCount => _buttonCount;

        public event Action<JoyButtonEvent>? ButtonChanged;
        public event Action<JoyButtonEvent>? ButtonDown;
        public event Action<JoyButtonEvent>? ButtonUp;

        internal void Handle(JoyButtonEvent buttonEvent) {
            Input.Joysticks.OnJoystickUpdated(buttonEvent.Which, buttonEvent.Timestamp);
            EnsureButtonStateContainers(buttonEvent.Which);

            HashSet<byte> buttons = _buttonStates[buttonEvent.Which];
            HashSet<byte> pressed = _pressedThisCycle[buttonEvent.Which];
            HashSet<byte> released = _releasedThisCycle[buttonEvent.Which];

            bool wasDown = buttons.Contains(buttonEvent.Button);

            if (buttonEvent.Down) {
                buttons.Add(buttonEvent.Button);
                if (!wasDown) {
                    pressed.Add(buttonEvent.Button);
                }
            } else {
                buttons.Remove(buttonEvent.Button);
                if (wasDown) {
                    released.Add(buttonEvent.Button);
                }
            }

            LastButtonEvent = buttonEvent;
            IncrementCounter(ref _buttonCount);

            ButtonChanged?.Invoke(buttonEvent);

            if (buttonEvent.Down) {
                ButtonDown?.Invoke(buttonEvent);
            } else {
                ButtonUp?.Invoke(buttonEvent);
            }
        }

        private void EnsureButtonStateContainers(uint id) {
            if (!_buttonStates.ContainsKey(id)) {
                _buttonStates[id] = new HashSet<byte>();
            }

            if (!_pressedThisCycle.ContainsKey(id)) {
                _pressedThisCycle[id] = new HashSet<byte>();
            }

            if (!_releasedThisCycle.ContainsKey(id)) {
                _releasedThisCycle[id] = new HashSet<byte>();
            }
        }

        private void RemoveButtonState(uint id) {
            _buttonStates.Remove(id);
            _pressedThisCycle.Remove(id);
            _releasedThisCycle.Remove(id);
        }

        partial void ResetButtonState() {
            if (!_buttonCount.HasEvents) return;

            foreach (HashSet<byte> buttons in _pressedThisCycle.Values) {
                buttons.Clear();
            }

            foreach (HashSet<byte> buttons in _releasedThisCycle.Values) {
                buttons.Clear();
            }

            _buttonCount.Reset();
        }
    }
}
