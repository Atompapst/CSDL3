// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using System.Text;
using CSDL.Extensions;

namespace CSDL.Input {
    /// <summary>An opened HID device. Dispose it to close the native handle.</summary>
    public sealed class HidDevice : NativeHandle<Opaque.SdlHidDevice> {
        internal HidDevice(NativePtr<Opaque.SdlHidDevice> handle)
            : base(handle, true) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_properties"/>
        /// <remarks>
        /// The returned group is owned by SDL; it is read-only and must not be disposed.
        /// </remarks>
        public HidDeviceProperties Properties => new HidDeviceProperties(SDL.hid_get_properties(Handle));

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_device_info"/>
        public HidDeviceInfo? GetDeviceInfo() {
            IntPtr info = SDL.hid_get_device_info(Handle);
            return info == IntPtr.Zero ? null : HidDeviceInfo.FromNative(new NativePtr<hid_device_info>(info).Read());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_feature_report"/>
        public unsafe int GetFeatureReport(Span<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.hid_get_feature_report(Handle, (NativePtr<byte>)dataPtr, (nuint)data.Length).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_input_report"/>
        public unsafe int GetInputReport(Span<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.hid_get_input_report(Handle, (NativePtr<byte>)dataPtr, (nuint)data.Length).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_report_descriptor"/>
        public unsafe int GetReportDescriptor(Span<byte> buffer) {
            fixed (byte* bufferPtr = buffer) {
                return SDL.hid_get_report_descriptor(Handle, (NativePtr<byte>)bufferPtr, (nuint)buffer.Length).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_read"/>
        public unsafe int Read(Span<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.hid_read(Handle, (NativePtr<byte>)dataPtr, (nuint)data.Length).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_read_timeout"/>
        public unsafe int Read(Span<byte> data, int milliseconds) {
            fixed (byte* dataPtr = data) {
                return SDL.hid_read_timeout(Handle, (NativePtr<byte>)dataPtr, (nuint)data.Length, milliseconds).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_send_feature_report"/>
        public unsafe int SendFeatureReport(ReadOnlySpan<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.hid_send_feature_report(Handle, (NativePtr<byte>)dataPtr, (nuint)data.Length).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_set_nonblocking"/>
        public bool SetNonBlocking(bool enabled) {
            return SDL.hid_set_nonblocking(Handle, enabled ? 1 : 0).LogIfInvalid(-1) == 0;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_write"/>
        public unsafe int Write(ReadOnlySpan<byte> data) {
            fixed (byte* dataPtr = data) {
                return SDL.hid_write(Handle, (NativePtr<byte>)dataPtr, (nuint)data.Length).LogIfInvalid(-1);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_indexed_string"/>
        public string? GetIndexedString(int index, int maxLength = 256) {
            return GetWideString(maxLength, (buffer, length) => SDL.hid_get_indexed_string(Handle, index, buffer, length));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_manufacturer_string"/>
        public string? GetManufacturerString(int maxLength = 256) {
            return GetWideString(maxLength, (buffer, length) => SDL.hid_get_manufacturer_string(Handle, buffer, length));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_product_string"/>
        public string? GetProductString(int maxLength = 256) {
            return GetWideString(maxLength, (buffer, length) => SDL.hid_get_product_string(Handle, buffer, length));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_get_serial_number_string"/>
        public string? GetSerialNumberString(int maxLength = 256) {
            return GetWideString(maxLength, (buffer, length) => SDL.hid_get_serial_number_string(Handle, buffer, length));
        }

        private unsafe string? GetWideString(int maxLength, Func<NativePtr<byte>, nuint, int> getString) {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);
            int characterSize = OperatingSystem.IsWindows() ? sizeof(char) : sizeof(uint);
            byte[] buffer = new byte[checked(maxLength * characterSize)];
            fixed (byte* bufferPtr = buffer) {
                if (getString((NativePtr<byte>)bufferPtr, (nuint)maxLength) < 0) {
                    Error.LogError(nameof(GetWideString));
                    return null;
                }
            }

            return OperatingSystem.IsWindows()
                ? Encoding.Unicode.GetString(buffer).TrimEnd('\0')
                : Encoding.UTF32.GetString(buffer).TrimEnd('\0');
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_close"/>
        protected override void DisposeResource() {
            SDL.hid_close(Handle).LogIfInvalid(-1);
        }
    }
}
