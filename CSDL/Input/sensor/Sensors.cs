// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
using CSDL.Extensions;

namespace CSDL.Input {
    public static class Sensors {
        private static readonly Dictionary<SensorID, SensorItem> _sensors = new Dictionary<SensorID, SensorItem>();

        static Sensors() {
            Init.InitSubSystem(InitFlags.Sensor);
            Refresh();
        }

        public static IReadOnlyCollection<SensorItem> Devices => _sensors.Values;

        private static void Refresh() {
            NativePtr<SensorID> ids = SDL.GetSensors(out int count).LogIfInvalid();
            if (ids.IsNull) {
                return;
            }
            try {
                _sensors.Clear();
                for (int i = 0; i < count; i++) {
                    if (!_sensors.ContainsKey(ids[i])) {
                        _sensors[ids[i]] = new SensorItem(ids[i], 0);
                    }
                }
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        internal static void OnSensorUpdated(SensorID id, ulong timestamp) {
            if (_sensors.TryGetValue(id, out SensorItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(SensorID id) {
            return _sensors.ContainsKey(id);
        }

        public static SensorItem? Get(SensorID id) {
            return _sensors.GetValueOrDefault(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.UpdateSensors"/>
        public static void Update() {
            SDL.UpdateSensors();
        }
    }
}
