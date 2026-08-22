// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        GamepadButtonEvent? LastButtonEvent { get; }
        int ButtonCount { get; }

        event Action<GamepadButtonEvent>? ButtonChanged;
        event Action<GamepadButtonEvent>? ButtonDown;
        event Action<GamepadButtonEvent>? ButtonUp;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad {
        private readonly Dictionary<uint, HashSet<GamepadButton>> _buttonStates = new Dictionary<uint, HashSet<GamepadButton>>();
        private readonly Dictionary<uint, HashSet<GamepadButton>> _pressedThisCycle = new Dictionary<uint, HashSet<GamepadButton>>();
        private readonly Dictionary<uint, HashSet<GamepadButton>> _releasedThisCycle = new Dictionary<uint, HashSet<GamepadButton>>();

        private Counter _buttonCount;

        public GamepadButtonEvent? LastButtonEvent { get; private set; }
        public int ButtonCount => _buttonCount;

        public event Action<GamepadButtonEvent>? ButtonChanged;
        public event Action<GamepadButtonEvent>? ButtonDown;
        public event Action<GamepadButtonEvent>? ButtonUp;

        internal void Handle(GamepadButtonEvent buttonEvent) {
            Gamepads.OnGamepadUpdated(buttonEvent.Which, buttonEvent.Timestamp);
            EnsureButtonStateContainers(buttonEvent.Which);

            HashSet<GamepadButton> buttons = _buttonStates[buttonEvent.Which];
            HashSet<GamepadButton> pressed = _pressedThisCycle[buttonEvent.Which];
            HashSet<GamepadButton> released = _releasedThisCycle[buttonEvent.Which];

            bool wasDown = buttons.Contains((GamepadButton)buttonEvent.Button);

            if (buttonEvent.Down) {
                buttons.Add((GamepadButton)buttonEvent.Button);
                if (!wasDown) {
                    pressed.Add((GamepadButton)buttonEvent.Button);
                }
            } else {
                buttons.Remove((GamepadButton)buttonEvent.Button);
                if (wasDown) {
                    released.Add((GamepadButton)buttonEvent.Button);
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
                _buttonStates[id] = new HashSet<GamepadButton>();
            }

            if (!_pressedThisCycle.ContainsKey(id)) {
                _pressedThisCycle[id] = new HashSet<GamepadButton>();
            }

            if (!_releasedThisCycle.ContainsKey(id)) {
                _releasedThisCycle[id] = new HashSet<GamepadButton>();
            }
        }

        private void RemoveButtonState(uint id) {
            _buttonStates.Remove(id);
            _pressedThisCycle.Remove(id);
            _releasedThisCycle.Remove(id);
        }

        partial void ResetButtonState() {
            if (!_buttonCount.HasEvents) return;

            foreach (HashSet<GamepadButton> buttons in _pressedThisCycle.Values) {
                buttons.Clear();
            }

            foreach (HashSet<GamepadButton> buttons in _releasedThisCycle.Values) {
                buttons.Clear();
            }

            _buttonCount.Reset();
        }
    }
}
