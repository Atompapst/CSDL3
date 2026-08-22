// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IPenEvents {
        PenButtonEvent? LastButtonEvent { get; }
        int ButtonCount { get; }

        event Action<PenButtonEvent>? ButtonChanged;
        event Action<PenButtonEvent>? ButtonDown;
        event Action<PenButtonEvent>? ButtonUp;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Pen {
        private Counter _buttonCount;

        public PenButtonEvent? LastButtonEvent { get; private set; }
        public int ButtonCount => _buttonCount;

        public event Action<PenButtonEvent>? ButtonChanged;
        public event Action<PenButtonEvent>? ButtonDown;
        public event Action<PenButtonEvent>? ButtonUp;

        internal void Handle(PenButtonEvent buttonEvent) {
            Input.Pens.OnPenUpdated(buttonEvent.Which, buttonEvent.Timestamp);
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
