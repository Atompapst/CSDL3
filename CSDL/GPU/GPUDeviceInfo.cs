// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;
namespace CSDL.GPU {
    /// <summary>
    /// The read-only properties SDL reports for an existing <see cref="GPUDevice"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="GPUDeviceProperties"/>, which is filled in by the application and handed to
    /// <see cref="GPUDevice(GPUDeviceProperties)"/>, this group is created and owned by SDL. It must
    /// not be disposed, so the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="GPUDevice.Info"/>
    public sealed class GPUDeviceInfo : PropertyGroup {
        internal GPUDeviceInfo(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.Props.GPUDeviceNameString"/>
        public StringProperty Name => PropString(Props.GPUDeviceNameString);

        /// <inheritdoc cref="CSDL.Props.GPUDeviceDriverNameString"/>
        public StringProperty DriverName => PropString(Props.GPUDeviceDriverNameString);

        /// <inheritdoc cref="CSDL.Props.GPUDeviceDriverVersionString"/>
        public StringProperty DriverVersion => PropString(Props.GPUDeviceDriverVersionString);

        /// <inheritdoc cref="CSDL.Props.GPUDeviceDriverInfoString"/>
        public StringProperty DriverInfo => PropString(Props.GPUDeviceDriverInfoString);
    }
}
