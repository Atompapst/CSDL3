// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IMouseEvents {
        int ButtonCount { get; }
        MouseButtonEvent LastButtonEvent { get; }

        event Action<MouseButtonEvent>? ButtonChanged;
        event Action<MouseButtonEvent>? ButtonDown;
        event Action<MouseButtonEvent>? ButtonUp;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Mouse {
        private readonly Dictionary<MouseButtonFlags, bool> _buttonStates = new Dictionary<MouseButtonFlags, bool>();
        private readonly HashSet<MouseButtonFlags> _pressedThisCycle = new HashSet<MouseButtonFlags>();
        private readonly HashSet<MouseButtonFlags> _releasedThisCycle = new HashSet<MouseButtonFlags>();

        private Counter _buttonCount;

        public int ButtonCount => _buttonCount;
        public MouseButtonEvent LastButtonEvent { get; private set; }

        public event Action<MouseButtonEvent>? ButtonChanged;
        public event Action<MouseButtonEvent>? ButtonDown;
        public event Action<MouseButtonEvent>? ButtonUp;

        internal void Handle(MouseButtonEvent buttonEvent) {
            Input.Mouse.OnMouseUpdated(buttonEvent.Which, buttonEvent.Timestamp);

            MouseButtonFlags button = ToButtonFlag(buttonEvent.Button);
            bool wasDown = IsDown(button);

            _buttonStates[button] = buttonEvent.Down;

            if (buttonEvent.Down) {
                if (!wasDown) {
                    _pressedThisCycle.Add(button);
                }
            } else {
                if (wasDown) {
                    _releasedThisCycle.Add(button);
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

        private static MouseButtonFlags ToButtonFlag(byte button) {
            return button switch {
                1 => MouseButtonFlags.Left,
                2 => MouseButtonFlags.Middle,
                3 => MouseButtonFlags.Right,
                4 => MouseButtonFlags.X1,
                5 => MouseButtonFlags.X2,
                _ => (MouseButtonFlags)(1u << button - 1),
            };
        }

        partial void ResetButtonState() {
            if (!_buttonCount.HasEvents) return;
            _pressedThisCycle.Clear();
            _releasedThisCycle.Clear();
            _buttonCount.Reset();
        }
    }
}
