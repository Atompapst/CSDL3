// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface ITouchEvents {
        PinchFingerEvent? LastPinchEvent { get; }
        int PinchCount { get; }

        event Action<PinchFingerEvent>? Pinch;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Touch {
        private Counter _pinchCount;

        public PinchFingerEvent? LastPinchEvent { get; private set; }
        public int PinchCount => _pinchCount;

        public event Action<PinchFingerEvent>? Pinch;

        internal void Handle(PinchFingerEvent pinchEvent) {
            LastPinchEvent = pinchEvent;
            IncrementCounter(ref _pinchCount);

            Pinch?.Invoke(pinchEvent);
        }

        partial void ResetPinchState() {
            if (!_pinchCount.HasEvents) return;
            _pinchCount.Reset();
        }
    }
}
