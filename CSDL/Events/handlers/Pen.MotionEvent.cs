// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IPenEvents {
        PenMotionEvent? LastMotionEvent { get; }
        int MotionCount { get; }

        event Action<PenMotionEvent>? Moved;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Pen {
        private Counter _motionCount;

        public PenMotionEvent? LastMotionEvent { get; private set; }
        public int MotionCount => _motionCount;

        public event Action<PenMotionEvent>? Moved;

        internal void Handle(PenMotionEvent motionEvent) {
            Input.Pens.OnPenUpdated(motionEvent.Which, motionEvent.Timestamp);

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
