// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.GPU {
    /// <summary>
    /// GPU device properties for <see cref="GPUDevice"/>
    /// </summary>
    ///
    /// <seealso cref="GPUDevice(GPUDeviceProperties)"/>
    public sealed class GPUDeviceProperties(uint handle) : PropertyGroup(handle) {
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateDebugmodeBoolean"/>
        public BooleanProperty DebugMode => PropBool(Props.GPUDeviceCreateDebugmodeBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreatePreferlowpowerBoolean"/>
        public BooleanProperty PreferLowPower => PropBool(Props.GPUDeviceCreatePreferlowpowerBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateVerboseBoolean"/>
        public BooleanProperty Verbose => PropBool(Props.GPUDeviceCreateVerboseBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateNameString"/>
        public StringProperty Name => PropString(Props.GPUDeviceCreateNameString);

        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateFeatureClipDistanceBoolean"/>
        public BooleanProperty FeatureClipDistance => PropBool(Props.GPUDeviceCreateFeatureClipDistanceBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateFeatureDepthClampingBoolean"/>
        public BooleanProperty FeatureDepthClamping => PropBool(Props.GPUDeviceCreateFeatureDepthClampingBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateFeatureIndirectDrawFirstInstanceBoolean"/>
        public BooleanProperty FeatureIndirectDrawFirstInstance => PropBool(Props.GPUDeviceCreateFeatureIndirectDrawFirstInstanceBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateFeatureAnisotropyBoolean"/>
        public BooleanProperty FeatureAnisotropy => PropBool(Props.GPUDeviceCreateFeatureAnisotropyBoolean);

        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateShadersPrivateBoolean"/>
        public BooleanProperty ShadersPrivate => PropBool(Props.GPUDeviceCreateShadersPrivateBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateShadersSpirvBoolean"/>
        public BooleanProperty ShadersSPIRV => PropBool(Props.GPUDeviceCreateShadersSpirvBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateShadersDxbcBoolean"/>
        public BooleanProperty ShadersDXBC => PropBool(Props.GPUDeviceCreateShadersDxbcBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateShadersDxilBoolean"/>
        public BooleanProperty ShadersDXIL => PropBool(Props.GPUDeviceCreateShadersDxilBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateShadersMslBoolean"/>
        public BooleanProperty ShadersMSL => PropBool(Props.GPUDeviceCreateShadersMslBoolean);
        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateShadersMetallibBoolean"/>
        public BooleanProperty ShadersMetallib => PropBool(Props.GPUDeviceCreateShadersMetallibBoolean);

        /// <inheritdoc cref="CSDL.Props.GPUDeviceCreateD3D12SemanticNameString"/>
        public StringProperty D3D12SemanticName => PropString(Props.GPUDeviceCreateD3D12SemanticNameString);
    }
}
