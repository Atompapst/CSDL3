// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface ISensorEvents {
        SensorEvent LastSensorEvent { get; }
        int SensorCount { get; }
        event Action<SensorEvent>? Updated;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Sensor : EventHandlerBase, Interfaces.ISensorEvents {
        private Counter _sensorCount;

        public SensorEvent LastSensorEvent { get; private set; }
        public int SensorCount => _sensorCount;

        public event Action<SensorEvent>? Updated;

        protected override void ResetState() {
            _sensorCount.Reset();
        }

        internal void Handle(SensorEvent sensorEvent) {
            Input.Sensors.OnSensorUpdated(sensorEvent.Which, sensorEvent.Timestamp);

            LastSensorEvent = sensorEvent;
            IncrementCounter(ref _sensorCount);

            Updated?.Invoke(sensorEvent);
        }
    }
}
