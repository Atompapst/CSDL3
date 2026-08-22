// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class GamepadDevice : NativeHandle<Opaque.SdlGamepad> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadID"/>
        public uint Id => GetGamepadID();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadName"/>
        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPath"/>
        public string Path => GetPath();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadType"/>
        public GamepadType Type => GetType();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetRealGamepadType"/>
        public GamepadType RealType => GetRealType();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadPlayerIndex"/>
        public int PlayerIndex {
            get => GetPlayerIndex();
            set => SetPlayerIndex(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadVendor"/>
        public ushort Vendor => GetVendor();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProduct"/>
        public ushort Product => GetProduct();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProductVersion"/>
        public ushort ProductVersion => GetProductVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadFirmwareVersion"/>
        public ushort FirmwareVersion => GetFirmwareVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadSerial"/>
        public string Serial => GetSerial();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadSteamHandle"/>
        public ulong SteamHandle => GetSteamHandle();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadConnectionState"/>
        public JoystickConnectionState ConnectionState => GetConnectionState();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadConnected"/>
        public bool Connected => GetConnected();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadMapping"/>
        public string? Mapping => GetMapping();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProperties"/>
        public uint Properties => GetProperties();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.OpenGamepad"/>
        public GamepadDevice(uint id) {
            Handle = SDL.OpenGamepad(id).ThrowIfInvalid();
        }

        internal GamepadDevice(NativePtr<Opaque.SdlGamepad> handle, bool ownsHandle = false)
            : base(handle, ownsHandle) {
            handle.ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadFromID"/>
        /// <remarks>
        /// The gamepad is still owned by whoever opened it, so disposing the returned wrapper does
        /// not close it.
        /// </remarks>
        public static GamepadDevice? FromID(JoystickID instanceID) {
            NativePtr<Opaque.SdlGamepad> gamepad = SDL.GetGamepadFromID(instanceID);
            return gamepad.IsNull ? null : new GamepadDevice(gamepad, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadFromPlayerIndex"/>
        /// <remarks>
        /// The gamepad is still owned by whoever opened it, so disposing the returned wrapper does
        /// not close it.
        /// </remarks>
        public static GamepadDevice? FromPlayerIndex(int playerIndex) {
            NativePtr<Opaque.SdlGamepad> gamepad = SDL.GetGamepadFromPlayerIndex(playerIndex);
            return gamepad.IsNull ? null : new GamepadDevice(gamepad, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPowerInfo"/>
        public PowerState GetPowerInfo(out int percent) {
            return SDL.GetGamepadPowerInfo(Handle, out percent);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadHasAxis"/>
        public bool HasAxis(GamepadAxis axis) {
            return SDL.GamepadHasAxis(Handle, axis);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadAxis"/>
        public short GetAxis(GamepadAxis axis) {
            return SDL.GetGamepadAxis(Handle, axis);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadHasButton"/>
        public bool HasButton(GamepadButton button) {
            return SDL.GamepadHasButton(Handle, button);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadButton"/>
        public bool GetButton(GamepadButton button) {
            return SDL.GetGamepadButton(Handle, button);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadHasCapSense"/>
        public bool HasCapSense(GamepadCapSenseType type) {
            return SDL.GamepadHasCapSense(Handle, type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadCapSense"/>
        public bool GetCapSense(GamepadCapSenseType type) {
            return SDL.GetGamepadCapSense(Handle, type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadBindings"/>
        public GamepadBinding[] GetBindings() {
            IntPtr ptr = SDL.GetGamepadBindings(Handle, out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetBindings));
                return Array.Empty<GamepadBinding>();
            }

            try {
                NativePtr<NativePtr<GamepadBinding>> bindings = ptr;
                GamepadBinding[] result = new GamepadBinding[count];
                for (int i = 0; i < count; i++) {
                    result[i] = bindings[i].Read();
                }
                return result;
            }
            finally {
                Memory.Free(ptr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadButtonLabel"/>
        public GamepadButtonLabel GetButtonLabel(GamepadButton button) {
            return SDL.GetGamepadButtonLabel(Handle, button);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadAppleSFSymbolsNameForButton"/>
        public string? GetAppleSFSymbolsName(GamepadButton button) {
            return SDL.GetGamepadAppleSFSymbolsNameForButton(Handle, button).ToUtf8String();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadAppleSFSymbolsNameForAxis"/>
        public string? GetAppleSFSymbolsName(GamepadAxis axis) {
            return SDL.GetGamepadAppleSFSymbolsNameForAxis(Handle, axis).ToUtf8String();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.RumbleGamepad"/>
        public bool Rumble(ushort lowFrequencyRumble, ushort highFrequencyRumble, uint durationMs) {
            return SDL.RumbleGamepad(Handle, lowFrequencyRumble, highFrequencyRumble, durationMs).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.RumbleGamepadTriggers"/>
        public bool RumbleTriggers(ushort leftRumble, ushort rightRumble, uint durationMs) {
            return SDL.RumbleGamepadTriggers(Handle, leftRumble, rightRumble, durationMs).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SendGamepadEffect"/>
        public unsafe bool SendEffect(ReadOnlySpan<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.SendGamepadEffect(Handle, (IntPtr)dataPtr, data.Length).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadLED"/>
        public bool SetLED(byte red, byte green, byte blue) {
            return SDL.SetGamepadLED(Handle, red, green, blue).LogIfFalse();
        }

        /// <summary>
        /// Sets the gamepad's LED color using the given <see cref="Video.Color"/>.
        /// </summary>
        public bool SetLED(Video.Color color) {
            return SDL.SetGamepadLED(Handle, color.R, color.G, color.B).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetNumGamepadTouchpads"/>
        public int GetNumTouchpads() {
            return SDL.GetNumGamepadTouchpads(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetNumGamepadTouchpadFingers"/>
        public int GetNumTouchpadFingers(int touchpad) {
            return SDL.GetNumGamepadTouchpadFingers(Handle, touchpad);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadTouchpadFinger"/>
        public bool GetTouchpadFinger(int touchpad, int finger, out bool down, out float x, out float y, out float pressure) {
            bool ok = SDL.GetGamepadTouchpadFinger(Handle, touchpad, finger, out CBool downRaw, out x, out y, out pressure).LogIfFalse();
            down = downRaw;
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadHasSensor"/>
        public bool HasSensor(SensorType type) {
            return SDL.GamepadHasSensor(Handle, type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadSensorEnabled"/>
        public bool SetSensorEnabled(SensorType type, bool enabled) {
            return SDL.SetGamepadSensorEnabled(Handle, type, enabled).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadSensorEnabled"/>
        public bool SensorEnabled(SensorType type) {
            return SDL.GamepadSensorEnabled(Handle, type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadSensorDataRate"/>
        public float GetSensorDataRate(SensorType type) {
            return SDL.GetGamepadSensorDataRate(Handle, type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadSensorData"/>
        public bool GetSensorData(SensorType type, float[] data) {
            if (data == null || data.Length == 0) return false;
            return SDL.GetGamepadSensorData(Handle, type, data, data.Length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadJoystick"/>
        public JoystickDevice? GetJoystick() {
            NativePtr<Opaque.SdlJoystick> ptr = SDL.GetGamepadJoystick(Handle);
            return ptr.IsNull ? null : new JoystickDevice(ptr, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadID"/>
        private uint GetGamepadID() {
            return SDL.GetGamepadID(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadName"/>
        private string GetName() {
            return SDL.GetGamepadName(Handle).ToUtf8String() ?? "Unknown Gamepad";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPath"/>
        private string GetPath() {
            return SDL.GetGamepadPath(Handle).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadType"/>
        private GamepadType GetType() {
            return SDL.GetGamepadType(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetRealGamepadType"/>
        private GamepadType GetRealType() {
            return SDL.GetRealGamepadType(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPlayerIndex"/>
        private int GetPlayerIndex() {
            return SDL.GetGamepadPlayerIndex(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadPlayerIndex"/>
        private void SetPlayerIndex(int playerIndex) {
            SDL.SetGamepadPlayerIndex(Handle, playerIndex).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadVendor"/>
        private ushort GetVendor() {
            return SDL.GetGamepadVendor(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProduct"/>
        private ushort GetProduct() {
            return SDL.GetGamepadProduct(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProductVersion"/>
        private ushort GetProductVersion() {
            return SDL.GetGamepadProductVersion(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadFirmwareVersion"/>
        private ushort GetFirmwareVersion() {
            return SDL.GetGamepadFirmwareVersion(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadSerial"/>
        private string GetSerial() {
            return SDL.GetGamepadSerial(Handle).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadSteamHandle"/>
        private ulong GetSteamHandle() {
            return SDL.GetGamepadSteamHandle(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadConnectionState"/>
        private JoystickConnectionState GetConnectionState() {
            return SDL.GetGamepadConnectionState(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GamepadConnected"/>
        private bool GetConnected() {
            return SDL.GamepadConnected(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadMapping"/>
        private string? GetMapping() {
            return SDL.GetGamepadMapping(Handle).ToUtf8StringAndFree();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProperties"/>
        private uint GetProperties() {
            return SDL.GetGamepadProperties(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.CloseGamepad"/>
        protected override void DisposeResource() {
            SDL.CloseGamepad(Handle);
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {Type}, Player: {PlayerIndex})";
        }
    }
}
