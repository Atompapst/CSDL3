// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class JoystickDevice : NativeHandle<Opaque.SdlJoystick> {
        static JoystickDevice() {
            Init.InitSubSystem(InitFlags.Joystick);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickID"/>
        public uint Id => GetJoystickID();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickName"/>
        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPath"/>
        public string Path => GetPath();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickPlayerIndex"/>
        public int PlayerIndex {
            get => GetPlayerIndex();
            set => SetPlayerIndex(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickGUID"/>
        public GUIDData Guid => GetGuid();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickVendor"/>
        public ushort Vendor => GetVendor();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProduct"/>
        public ushort Product => GetProduct();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProductVersion"/>
        public ushort ProductVersion => GetProductVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickFirmwareVersion"/>
        public ushort FirmwareVersion => GetFirmwareVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickSerial"/>
        public string Serial => GetSerial();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickType"/>
        public JoystickType Type => GetJoystickType();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.JoystickConnected"/>
        public bool Connected => GetConnected();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickConnectionState"/>
        public JoystickConnectionState ConnectionState => GetConnectionState();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProperties"/>
        public uint Properties => GetProperties();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickAxes"/>
        public int NumAxes => GetNumAxes();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickBalls"/>
        public int NumBalls => GetNumBalls();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickHats"/>
        public int NumHats => GetNumHats();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickButtons"/>
        public int NumButtons => GetNumButtons();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.OpenJoystick"/>
        public JoystickDevice(uint id) {
            Handle = SDL.OpenJoystick(id).ThrowIfInvalid();
        }

        internal JoystickDevice(NativePtr<Opaque.SdlJoystick> handle, bool ownsHandle = false)
            : base(handle, ownsHandle) {
            handle.ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickFromID"/>
        /// <remarks>
        /// The joystick is still owned by whoever opened it, so disposing the returned wrapper does
        /// not close it.
        /// </remarks>
        public static JoystickDevice? FromID(JoystickID instanceID) {
            NativePtr<Opaque.SdlJoystick> joystick = SDL.GetJoystickFromID(instanceID);
            return joystick.IsNull ? null : new JoystickDevice(joystick, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickFromPlayerIndex"/>
        /// <remarks>
        /// The joystick is still owned by whoever opened it, so disposing the returned wrapper does
        /// not close it.
        /// </remarks>
        public static JoystickDevice? FromPlayerIndex(int playerIndex) {
            NativePtr<Opaque.SdlJoystick> joystick = SDL.GetJoystickFromPlayerIndex(playerIndex);
            return joystick.IsNull ? null : new JoystickDevice(joystick, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickAxis"/>
        public short GetAxis(int axis) {
            return SDL.GetJoystickAxis(Handle, axis);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickAxisInitialState"/>
        public bool GetAxisInitialState(int axis, out short state) {
            return SDL.GetJoystickAxisInitialState(Handle, axis, out state).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickBall"/>
        public bool GetBall(int ball, out int dx, out int dy) {
            dx = 0;
            dy = 0;
            return SDL.GetJoystickBall(Handle, ball, NativePtr<int>.FromRef(ref dx), NativePtr<int>.FromRef(ref dy)).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickHat"/>
        public byte GetHat(int hat) {
            return SDL.GetJoystickHat(Handle, hat);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickButton"/>
        public bool GetButton(int button) {
            return SDL.GetJoystickButton(Handle, button);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPowerInfo"/>
        public PowerState GetPowerInfo(out int percent) {
            return SDL.GetJoystickPowerInfo(Handle, out percent);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.RumbleJoystick"/>
        public bool Rumble(ushort lowFrequencyRumble, ushort highFrequencyRumble, uint durationMs) {
            if (!SDL.RumbleJoystick(Handle, lowFrequencyRumble, highFrequencyRumble, durationMs)) {
                Error.LogError(nameof(Rumble));
                return false;
            }
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.RumbleJoystickTriggers"/>
        public bool RumbleTriggers(ushort leftRumble, ushort rightRumble, uint durationMs) {
            if (!SDL.RumbleJoystickTriggers(Handle, leftRumble, rightRumble, durationMs)) {
                Error.LogError(nameof(RumbleTriggers));
                return false;
            }
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickLED"/>
        public bool SetLED(byte red, byte green, byte blue) {
            return SDL.SetJoystickLED(Handle, red, green, blue).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SendJoystickEffect"/>
        public unsafe bool SendEffect(ReadOnlySpan<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.SendJoystickEffect(Handle, (IntPtr)dataPtr, data.Length).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SendJoystickVirtualSensorData"/>
        public bool SendVirtualSensorData(SensorType type, ulong timestamp, ReadOnlySpan<float> data) {
            return SDL.SendJoystickVirtualSensorData(Handle, type, timestamp, data, data.Length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickVirtualAxis"/>
        public bool SetVirtualAxis(int axis, short value) {
            return SDL.SetJoystickVirtualAxis(Handle, axis, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickVirtualBall"/>
        public bool SetVirtualBall(int ball, short xrel, short yrel) {
            return SDL.SetJoystickVirtualBall(Handle, ball, xrel, yrel).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickVirtualButton"/>
        public bool SetVirtualButton(int button, bool down) {
            return SDL.SetJoystickVirtualButton(Handle, button, down).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickVirtualHat"/>
        public bool SetVirtualHat(int hat, byte value) {
            return SDL.SetJoystickVirtualHat(Handle, hat, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickVirtualTouchpad"/>
        public bool SetVirtualTouchpad(int touchpad, int finger, bool down, float x, float y, float pressure) {
            return SDL.SetJoystickVirtualTouchpad(Handle, touchpad, finger, down, x, y, pressure).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickID"/>
        private uint GetJoystickID() {
            return SDL.GetJoystickID(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickName"/>
        private string GetName() {
            return SDL.GetJoystickName(Handle).ToUtf8StringOrLog() ?? "Unknown Joystick";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPath"/>
        private string GetPath() {
            return SDL.GetJoystickPath(Handle).ToUtf8StringOrLog() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPlayerIndex"/>
        private int GetPlayerIndex() {
            return SDL.GetJoystickPlayerIndex(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.SetJoystickPlayerIndex"/>
        private void SetPlayerIndex(int playerIndex) {
            SDL.SetJoystickPlayerIndex(Handle, playerIndex).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickGUID"/>
        private GUIDData GetGuid() {
            return SDL.GetJoystickGUID(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickVendor"/>
        private ushort GetVendor() {
            return SDL.GetJoystickVendor(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProduct"/>
        private ushort GetProduct() {
            return SDL.GetJoystickProduct(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProductVersion"/>
        private ushort GetProductVersion() {
            return SDL.GetJoystickProductVersion(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickFirmwareVersion"/>
        private ushort GetFirmwareVersion() {
            return SDL.GetJoystickFirmwareVersion(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickSerial"/>
        private string GetSerial() {
            return SDL.GetJoystickSerial(Handle).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickType"/>
        private JoystickType GetJoystickType() {
            return SDL.GetJoystickType(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.JoystickConnected"/>
        private bool GetConnected() {
            return SDL.JoystickConnected(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickConnectionState"/>
        private JoystickConnectionState GetConnectionState() {
            return SDL.GetJoystickConnectionState(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProperties"/>
        private uint GetProperties() {
            return SDL.GetJoystickProperties(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickAxes"/>
        private int GetNumAxes() {
            return SDL.GetNumJoystickAxes(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickBalls"/>
        private int GetNumBalls() {
            return SDL.GetNumJoystickBalls(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickHats"/>
        private int GetNumHats() {
            return SDL.GetNumJoystickHats(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetNumJoystickButtons"/>
        private int GetNumButtons() {
            return SDL.GetNumJoystickButtons(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.CloseJoystick"/>
        protected override void DisposeResource() {
            SDL.CloseJoystick(Handle);
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {Type}, Player: {PlayerIndex})";
        }
    }
}
