// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.Input {
    /// <summary>
    /// The read-only properties SDL reports for an opened <see cref="HidDevice"/>.
    /// </summary>
    /// <remarks>
    /// This group is created and owned by SDL and lives as long as the device does. It must not be
    /// disposed, so the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="HidDevice.Properties"/>
    public sealed class HidDeviceProperties : PropertyGroup {
        internal HidDeviceProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.Props.HidapiLibusbDeviceHandlePointer"/>
        public PointerProperty LibUsbDeviceHandle => PropPointer(Props.HidapiLibusbDeviceHandlePointer);
    }
}
