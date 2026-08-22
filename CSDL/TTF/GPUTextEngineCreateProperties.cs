// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.GPU;
using CSDL.Properties;

namespace CSDL.TTF {
    /// <summary>
    /// The property set an application fills in and hands to
    /// <see cref="GPUTextEngine(GPUTextEngineCreateProperties)"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Device"/> is required; the constructor taking a <see cref="GPUDevice"/> fills it in.
    /// </remarks>
    public sealed class GPUTextEngineCreateProperties : PropertyGroup {
        public GPUTextEngineCreateProperties() { }

        /// <summary>
        /// Creates a property set with <see cref="Device"/> pointing at <paramref name="device"/>.
        /// </summary>
        public GPUTextEngineCreateProperties(GPUDevice device) {
            Device.Set(device.NativePointer);
        }

        /// <inheritdoc cref="CSDL.TTF.Props.GPUTextEngineDevicePointer"/>
        public PointerProperty Device => PropPointer(Props.GPUTextEngineDevicePointer);

        /// <inheritdoc cref="CSDL.TTF.Props.GPUTextEngineAtlasTextureSizeNumber"/>
        public NumberProperty AtlasTextureSize => PropNumber(Props.GPUTextEngineAtlasTextureSizeNumber);
    }
}
