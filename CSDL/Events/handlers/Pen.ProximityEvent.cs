// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IPenEvents {
        PenProximityEvent? LastProximityEvent { get; }
        int ProximityCount { get; }

        event Action<PenProximityEvent>? ProximityIn;
        event Action<PenProximityEvent>? ProximityOut;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Pen {
        private Counter _proximityCount;

        public PenProximityEvent? LastProximityEvent { get; private set; }
        public int ProximityCount => _proximityCount;

        public event Action<PenProximityEvent>? ProximityIn;
        public event Action<PenProximityEvent>? ProximityOut;

        internal void Handle(PenProximityEvent proximityEvent) {
            switch (proximityEvent.Type) {
                case EventType.PenProximityIn:
                    Input.Pens.OnPenAdded(proximityEvent.Which, proximityEvent.Timestamp);
                    EnsureButtonStateContainers(proximityEvent.Which);
                    ProximityIn?.Invoke(proximityEvent);
                    break;

                case EventType.PenProximityOut:
                    Input.Pens.OnPenRemoved(proximityEvent.Which);
                    RemoveButtonState(proximityEvent.Which);
                    ProximityOut?.Invoke(proximityEvent);
                    break;
            }

            LastProximityEvent = proximityEvent;
            IncrementCounter(ref _proximityCount);
        }

        partial void ResetProximityState() {
            if (!_proximityCount.HasEvents) return;
            _proximityCount.Reset();
        }
    }
}
