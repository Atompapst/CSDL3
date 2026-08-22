// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Video {
    public class RendererCreateProperties : PropertyGroup {
        /// <inheritdoc cref="CSDL.Props.RendererCreateNameString"/>
        public StringProperty Name => PropString(Props.RendererCreateNameString);
        /// <inheritdoc cref="CSDL.Props.RendererCreateWindowPointer"/>
        public PointerProperty Window => PropPointer(Props.RendererCreateWindowPointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateSurfacePointer"/>
        public PointerProperty Surface => PropPointer(Props.RendererCreateSurfacePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateOutputColorspaceNumber"/>
        public NumberProperty OutputColorspace => PropNumber(Props.RendererCreateOutputColorspaceNumber);
        /// <inheritdoc cref="CSDL.Props.RendererCreatePresentVsyncNumber"/>
        public NumberProperty PresentVsync => PropNumber(Props.RendererCreatePresentVsyncNumber);
        /// <inheritdoc cref="CSDL.Props.RendererCreateGPUDevicePointer"/>
        public PointerProperty GPUDevice => PropPointer(Props.RendererCreateGPUDevicePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateGPUShadersDxilBoolean"/>
        public BooleanProperty GPUShadersDxil => PropBool(Props.RendererCreateGPUShadersDxilBoolean);
        /// <inheritdoc cref="CSDL.Props.RendererCreateGPUShadersMslBoolean"/>
        public BooleanProperty GPUShadersMsl => PropBool(Props.RendererCreateGPUShadersMslBoolean);
        /// <inheritdoc cref="CSDL.Props.RendererCreateGPUShadersSpirvBoolean"/>
        public BooleanProperty GPUShadersSpirv => PropBool(Props.RendererCreateGPUShadersSpirvBoolean);
        /// <inheritdoc cref="CSDL.Props.RendererCreateMetalCommandQueuePointer"/>
        public PointerProperty MetalCommandQueue => PropPointer(Props.RendererCreateMetalCommandQueuePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateMetalDevicePointer"/>
        public PointerProperty MetalDevice => PropPointer(Props.RendererCreateMetalDevicePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateVulkanInstancePointer"/>
        public PointerProperty VulkanInstance => PropPointer(Props.RendererCreateVulkanInstancePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateVulkanSurfaceNumber"/>
        public NumberProperty VulkanSurface => PropNumber(Props.RendererCreateVulkanSurfaceNumber);
        /// <inheritdoc cref="CSDL.Props.RendererCreateVulkanPhysicalDevicePointer"/>
        public PointerProperty VulkanPhysicalDevice => PropPointer(Props.RendererCreateVulkanPhysicalDevicePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateVulkanDevicePointer"/>
        public PointerProperty VulkanDevice => PropPointer(Props.RendererCreateVulkanDevicePointer);
        /// <inheritdoc cref="CSDL.Props.RendererCreateVulkanGraphicsQueueFamilyIndexNumber"/>
        public NumberProperty VulkanGraphicsQueueFamilyIndex => PropNumber(Props.RendererCreateVulkanGraphicsQueueFamilyIndexNumber);
        /// <inheritdoc cref="CSDL.Props.RendererCreateVulkanPresentQueueFamilyIndexNumber"/>
        public NumberProperty VulkanPresentQueueFamilyIndex => PropNumber(Props.RendererCreateVulkanPresentQueueFamilyIndexNumber);
    }
}
