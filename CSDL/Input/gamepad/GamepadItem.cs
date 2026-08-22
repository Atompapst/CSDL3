// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class GamepadItem {
        public uint Id { get; }
        public ulong LastTimestampNs { get; internal set; }

        internal GamepadItem(uint id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadNameForID"/>
        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPathForID"/>
        public string Path => GetPath();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPlayerIndexForID"/>
        public int PlayerIndex => GetPlayerIndex();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadGUIDForID"/>
        public GUIDData Guid => GetGuid();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadVendorForID"/>
        public ushort Vendor => GetVendor();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProductForID"/>
        public ushort Product => GetProduct();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProductVersionForID"/>
        public ushort ProductVersion => GetProductVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadTypeForID"/>
        public GamepadType Type => GetGamepadType();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetRealGamepadTypeForID"/>
        public GamepadType RealType => GetRealType();

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadMapping"/>
        public string? Mapping {
            get => GetMapping();
            set => SetMapping(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.IsGamepad"/>
        public bool IsGamepad => GetIsGamepad();

        public GamepadDevice Open() {
            return new GamepadDevice(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadNameForID"/>
        private string GetName() {
            return SDL.GetGamepadNameForID(Id).ToUtf8StringOrLog() ?? "Unknown Gamepad";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPathForID"/>
        private string GetPath() {
            return SDL.GetGamepadPathForID(Id).ToUtf8StringOrLog() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadPlayerIndexForID"/>
        private int GetPlayerIndex() {
            return SDL.GetGamepadPlayerIndexForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadGUIDForID"/>
        private GUIDData GetGuid() {
            return SDL.GetGamepadGUIDForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadVendorForID"/>
        private ushort GetVendor() {
            return SDL.GetGamepadVendorForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProductForID"/>
        private ushort GetProduct() {
            return SDL.GetGamepadProductForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadProductVersionForID"/>
        private ushort GetProductVersion() {
            return SDL.GetGamepadProductVersionForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadTypeForID"/>
        private GamepadType GetGamepadType() {
            return SDL.GetGamepadTypeForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetRealGamepadTypeForID"/>
        private GamepadType GetRealType() {
            return SDL.GetRealGamepadTypeForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.GetGamepadMappingForID"/>
        private string? GetMapping() {
            return SDL.GetGamepadMappingForID(Id).ToUtf8StringAndFree();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.SetGamepadMapping"/>
        private bool SetMapping(string? value) {
            IntPtr ptr = value is null ? IntPtr.Zero : Marshal.StringToCoTaskMemUTF8(value);
            try {
                return SDL.SetGamepadMapping(Id, new NativePtr<byte>(ptr)).LogIfFalse();
            }
            finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Gamepad.IsGamepad"/>
        private bool GetIsGamepad() {
            return SDL.IsGamepad(Id);
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {Type}, Player: {PlayerIndex})";
        }
    }
}
