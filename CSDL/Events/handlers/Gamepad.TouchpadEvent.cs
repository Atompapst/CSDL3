// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        GamepadTouchpadEvent? LastTouchpadEvent { get; }
        int TouchpadCount { get; }

        event Action<GamepadTouchpadEvent>? TouchpadChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad {
        private Counter _touchpadCount;

        public GamepadTouchpadEvent? LastTouchpadEvent { get; private set; }
        public int TouchpadCount => _touchpadCount;

        public event Action<GamepadTouchpadEvent>? TouchpadChanged;

        internal void Handle(GamepadTouchpadEvent touchpadEvent) {
            Input.Gamepads.OnGamepadUpdated(touchpadEvent.Which, touchpadEvent.Timestamp);

            LastTouchpadEvent = touchpadEvent;
            IncrementCounter(ref _touchpadCount);

            TouchpadChanged?.Invoke(touchpadEvent);
        }

        partial void ResetTouchpadState() {
            if (!_touchpadCount.HasEvents) return;
            _touchpadCount.Reset();
        }
    }
}
