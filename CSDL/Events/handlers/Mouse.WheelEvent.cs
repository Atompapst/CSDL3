// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IMouseEvents {
        int WheelCount { get; }
        MouseWheelEvent LastWheelEvent { get; }
        event Action<MouseWheelEvent>? Wheel;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Mouse {
        private Counter _wheelCount;

        public int WheelCount => _wheelCount;
        public MouseWheelEvent LastWheelEvent { get; private set; }
        public event Action<MouseWheelEvent>? Wheel;

        internal void Handle(MouseWheelEvent wheelEvent) {
            Input.Mouse.OnMouseUpdated(wheelEvent.Which, wheelEvent.Timestamp);

            LastWheelEvent = wheelEvent;
            IncrementCounter(ref _wheelCount);

            Wheel?.Invoke(wheelEvent);
        }

        partial void ResetWheelState() {
            if (!_wheelCount.HasEvents) return;
            _wheelCount.Reset();
        }
    }
}
