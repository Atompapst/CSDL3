// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IMouseEvents {
        int MotionCount { get; }
        MouseMotionEvent LastMotionEvent { get; }
        event Action<MouseMotionEvent>? Moved;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Mouse {
        private Counter _motionCount;

        public int MotionCount => _motionCount;
        public MouseMotionEvent LastMotionEvent { get; private set; }
        public event Action<MouseMotionEvent>? Moved;

        internal void Handle(MouseMotionEvent motionEvent) {
            Input.Mouse.OnMouseUpdated(motionEvent.Which, motionEvent.Timestamp);

            LastMotionEvent = motionEvent;
            IncrementCounter(ref _motionCount);

            Moved?.Invoke(motionEvent);
        }

        partial void ResetMotionState() {
            if (!_motionCount.HasEvents) return;
            _motionCount.Reset();
        }
    }
}
