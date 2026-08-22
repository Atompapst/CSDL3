// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Input {
    public sealed class JoystickItem {

        internal JoystickItem(uint id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
        }
        public uint Id { get; }
        public ulong LastTimestampNs { get; internal set; }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickNameForID"/>
        public string Name => GetName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPathForID"/>
        public string Path => GetPath();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPlayerIndexForID"/>
        public int PlayerIndex => GetPlayerIndex();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickGUIDForID"/>
        public GUIDData Guid => GetGuid();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickVendorForID"/>
        public ushort Vendor => GetVendor();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProductForID"/>
        public ushort Product => GetProduct();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProductVersionForID"/>
        public ushort ProductVersion => GetProductVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickTypeForID"/>
        public JoystickType Type => GetType();

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.IsJoystickVirtual"/>
        public bool IsVirtual => GetIsVirtual();

        public JoystickDevice Open() {
            return new JoystickDevice(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickNameForID"/>
        private string GetName() {
            return SDL.GetJoystickNameForID(Id).ToUtf8StringOrLog() ?? "Unknown Joystick";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPathForID"/>
        private string GetPath() {
            return SDL.GetJoystickPathForID(Id).ToUtf8StringOrLog() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickPlayerIndexForID"/>
        private int GetPlayerIndex() {
            return SDL.GetJoystickPlayerIndexForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickGUIDForID"/>
        private GUIDData GetGuid() {
            return SDL.GetJoystickGUIDForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickVendorForID"/>
        private ushort GetVendor() {
            return SDL.GetJoystickVendorForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProductForID"/>
        private ushort GetProduct() {
            return SDL.GetJoystickProductForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickProductVersionForID"/>
        private ushort GetProductVersion() {
            return SDL.GetJoystickProductVersionForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.GetJoystickTypeForID"/>
        private JoystickType GetType() {
            return SDL.GetJoystickTypeForID(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Joystick.IsJoystickVirtual"/>
        private bool GetIsVirtual() {
            return SDL.IsJoystickVirtual(Id);
        }

        public override string ToString() {
            return $"{Name} (ID: {Id}, Type: {Type}, Player: {PlayerIndex})";
        }
    }
}
