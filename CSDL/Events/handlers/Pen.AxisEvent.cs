// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IPenEvents {
        PenAxisEvent? LastAxisEvent { get; }
        int AxisCount { get; }

        event Action<PenAxisEvent>? AxisChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Pen {
        private Counter _axisCount;

        public PenAxisEvent? LastAxisEvent { get; private set; }
        public int AxisCount => _axisCount;

        public event Action<PenAxisEvent>? AxisChanged;

        internal void Handle(PenAxisEvent axisEvent) {
            Input.Pens.OnPenUpdated(axisEvent.Which, axisEvent.Timestamp);

            LastAxisEvent = axisEvent;
            IncrementCounter(ref _axisCount);

            AxisChanged?.Invoke(axisEvent);
        }

        partial void ResetAxisState() {
            if (!_axisCount.HasEvents) return;
            _axisCount.Reset();
        }
    }
}
