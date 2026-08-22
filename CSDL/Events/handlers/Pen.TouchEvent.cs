// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IPenEvents {
        PenTouchEvent? LastTouchEvent { get; }
        int TouchCount { get; }

        event Action<PenTouchEvent>? TouchChanged;
        event Action<PenTouchEvent>? TouchDown;
        event Action<PenTouchEvent>? TouchUp;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Pen {
        private Counter _touchCount;

        public PenTouchEvent? LastTouchEvent { get; private set; }
        public int TouchCount => _touchCount;

        public event Action<PenTouchEvent>? TouchChanged;
        public event Action<PenTouchEvent>? TouchDown;
        public event Action<PenTouchEvent>? TouchUp;

        internal void Handle(PenTouchEvent touchEvent) {
            Input.Pens.OnPenUpdated(touchEvent.Which, touchEvent.Timestamp);

            LastTouchEvent = touchEvent;
            IncrementCounter(ref _touchCount);

            TouchChanged?.Invoke(touchEvent);

            if (touchEvent.Down) {
                TouchDown?.Invoke(touchEvent);
            } else {
                TouchUp?.Invoke(touchEvent);
            }
        }

        partial void ResetTouchState() {
            if (!_touchCount.HasEvents) return;
            _touchCount.Reset();
        }
    }
}
