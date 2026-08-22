// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface ITouchEvents {
        TouchFingerEvent? LastFingerEvent { get; }
        int FingerCount { get; }

        event Action<TouchFingerEvent>? FingerChanged;
        event Action<TouchFingerEvent>? FingerDown;
        event Action<TouchFingerEvent>? FingerUp;
        event Action<TouchFingerEvent>? FingerMoved;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Touch {
        private Counter _fingerCount;

        public TouchFingerEvent? LastFingerEvent { get; private set; }
        public int FingerCount => _fingerCount;

        public event Action<TouchFingerEvent>? FingerChanged;
        public event Action<TouchFingerEvent>? FingerDown;
        public event Action<TouchFingerEvent>? FingerUp;
        public event Action<TouchFingerEvent>? FingerMoved;

        internal void Handle(TouchFingerEvent fingerEvent) {
            Input.Touch.OnTouchUpdated(fingerEvent.TouchID, fingerEvent.Timestamp);

            LastFingerEvent = fingerEvent;
            IncrementCounter(ref _fingerCount);

            FingerChanged?.Invoke(fingerEvent);

            switch (fingerEvent.Type) {
                case EventType.FingerDown:
                    FingerDown?.Invoke(fingerEvent);
                    break;
                case EventType.FingerUp:
                    FingerUp?.Invoke(fingerEvent);
                    break;
                case EventType.FingerMotion:
                    FingerMoved?.Invoke(fingerEvent);
                    break;
            }
        }

        partial void ResetFingerState() {
            if (!_fingerCount.HasEvents) return;
            _fingerCount.Reset();
        }
    }
}
