// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class SensorItem {
        public SensorID Id { get; }
        public ulong LastTimestampNs { get; internal set; }

        internal SensorItem(uint id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
        }

        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorTypeForID"/>
        public SensorType Type => SDL.GetSensorTypeForID(Id);

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorNonPortableTypeForID"/>
        public int NonPortableType => SDL.GetSensorNonPortableTypeForID(Id);

        public SensorDevice Open() {
            return new SensorDevice(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorNameForID"/>
        private string GetName() {
            return SDL.GetSensorNameForID(Id).ToUtf8String() ?? "Unknown Sensor";
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {Type})";
        }
    }
}
