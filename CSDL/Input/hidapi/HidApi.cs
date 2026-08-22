// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CSDL.Extensions;

namespace CSDL.Input {
    public static class HidApi {
        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_init"/>
        public static bool Initialize() {
            return SDL.hid_init().LogIfInvalid(-1) == 0;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_exit"/>
        public static bool Exit() {
            return SDL.hid_exit().LogIfInvalid(-1) == 0;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_ble_scan"/>
        public static void SetBleScan(bool active) {
            SDL.hid_ble_scan(active);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_device_change_count"/>
        public static uint DeviceChangeCount => SDL.hid_device_change_count();

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_enumerate"/>
        public static HidDeviceInfo[] Enumerate(ushort vendorID = 0, ushort productID = 0) {
            IntPtr list = SDL.hid_enumerate(vendorID, productID);
            if (list == IntPtr.Zero) {
                return Array.Empty<HidDeviceInfo>();
            }

            List<HidDeviceInfo> devices = new List<HidDeviceInfo>();
            try {
                NativePtr<hid_device_info> current = list;
                while (!current.IsNull) {
                    hid_device_info info = current.Read();
                    devices.Add(HidDeviceInfo.FromNative(info));
                    current = info.Next;
                }
            }
            finally {
                SDL.hid_free_enumeration(new NativePtr<nint>(list));
            }

            return devices.ToArray();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_open"/>
        public static HidDevice Open(ushort vendorID, ushort productID, string? serialNumber = null) {
            IntPtr serialNumberPtr = serialNumber is null ? IntPtr.Zero : AllocateWideString(serialNumber);
            try {
                return new HidDevice(SDL.hid_open(vendorID, productID, new NativePtr<byte>(serialNumberPtr)).ThrowIfInvalid());
            }
            finally {
                if (serialNumberPtr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(serialNumberPtr);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hidapi.hid_open_path"/>
        public static HidDevice OpenPath(string path) {
            ArgumentException.ThrowIfNullOrEmpty(path);
            return new HidDevice(SDL.hid_open_path(path).ThrowIfInvalid());
        }

        private static IntPtr AllocateWideString(string value) {
            if (OperatingSystem.IsWindows()) {
                return Marshal.StringToCoTaskMemUni(value);
            }

            byte[] bytes = Encoding.UTF32.GetBytes(value + '\0');
            IntPtr ptr = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return ptr;
        }
    }

}
