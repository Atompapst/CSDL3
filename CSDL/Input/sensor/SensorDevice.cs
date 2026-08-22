// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class SensorDevice : NativeHandle<Opaque.SdlSensor> {
        static SensorDevice() {
            Init.InitSubSystem(InitFlags.Sensor);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorID"/>
        public uint Id => SDL.GetSensorID(Handle);

        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorType"/>
        public SensorType Type => SDL.GetSensorType(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorNonPortableType"/>
        public int NonPortableType => SDL.GetSensorNonPortableType(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorProperties"/>
        public uint Properties => SDL.GetSensorProperties(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.OpenSensor"/>
        public SensorDevice(SensorID instanceID) {
            Handle = SDL.OpenSensor(instanceID).ThrowIfInvalid();
        }

        internal SensorDevice(NativePtr<Opaque.SdlSensor> handle, bool ownsHandle = false)
            : base(handle, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorFromID"/>
        /// <remarks>
        /// The sensor is still owned by whoever opened it, so disposing the returned wrapper does
        /// not close it.
        /// </remarks>
        public static SensorDevice? FromID(SensorID instanceID) {
            NativePtr<Opaque.SdlSensor> sensor = SDL.GetSensorFromID(instanceID);
            return sensor.IsNull ? null : new SensorDevice(sensor, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorData"/>
        public bool GetData(float[] buffer) {
            if (buffer == null || buffer.Length == 0) return false;
            return SDL.GetSensorData(Handle, buffer, buffer.Length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.GetSensorName"/>
        private string GetName() {
            return SDL.GetSensorName(Handle).ToUtf8StringOrLog() ?? "Unknown Sensor";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Sensor.CloseSensor"/>
        protected override void DisposeResource() {
            SDL.CloseSensor(Handle);
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {Type})";
        }
    }
}
