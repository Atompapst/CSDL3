// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;
namespace CSDL.Video {
    /// <summary>
    /// The properties SDL keeps for an existing <see cref="Renderer"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="RendererCreateProperties"/>, which the application fills in and hands to
    /// <see cref="Renderer(RendererCreateProperties)"/>, this group is created and owned by SDL and lives
    /// as long as the renderer does. It must not be disposed, so the finalizer that would otherwise
    /// destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Renderer.Properties"/>
    public sealed class RendererProperties : PropertyGroup {
        internal RendererProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.Props.RendererNameString"/>
        public StringProperty Name => PropString(Props.RendererNameString);

        /// <inheritdoc cref="CSDL.Props.RendererWindowPointer"/>
        public PointerProperty Window => PropPointer(Props.RendererWindowPointer);

        /// <inheritdoc cref="CSDL.Props.RendererSurfacePointer"/>
        public PointerProperty Surface => PropPointer(Props.RendererSurfacePointer);

        /// <inheritdoc cref="CSDL.Props.RendererVsyncNumber"/>
        public NumberProperty Vsync => PropNumber(Props.RendererVsyncNumber);

        /// <inheritdoc cref="CSDL.Props.RendererMaxTextureSizeNumber"/>
        public NumberProperty MaxTextureSize => PropNumber(Props.RendererMaxTextureSizeNumber);

        /// <inheritdoc cref="CSDL.Props.RendererTextureFormatsPointer"/>
        public PointerProperty TextureFormats => PropPointer(Props.RendererTextureFormatsPointer);

        /// <inheritdoc cref="CSDL.Props.RendererTextureWrappingBoolean"/>
        public BooleanProperty TextureWrapping => PropBool(Props.RendererTextureWrappingBoolean);

        /// <inheritdoc cref="CSDL.Props.RendererOutputColorspaceNumber"/>
        public NumberProperty OutputColorspace => PropNumber(Props.RendererOutputColorspaceNumber);

        /// <inheritdoc cref="CSDL.Props.RendererHDREnabledBoolean"/>
        public BooleanProperty HDREnabled => PropBool(Props.RendererHDREnabledBoolean);

        /// <inheritdoc cref="CSDL.Props.RendererSdrWhitePointFloat"/>
        public FloatProperty SDRWhitePoint => PropFloat(Props.RendererSdrWhitePointFloat);

        /// <inheritdoc cref="CSDL.Props.RendererHDRHeadroomFloat"/>
        public FloatProperty HDRHeadroom => PropFloat(Props.RendererHDRHeadroomFloat);

        /// <inheritdoc cref="CSDL.Props.RendererD3D9DevicePointer"/>
        public PointerProperty D3D9Device => PropPointer(Props.RendererD3D9DevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererD3D11DevicePointer"/>
        public PointerProperty D3D11Device => PropPointer(Props.RendererD3D11DevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererD3D11SwapchainPointer"/>
        public PointerProperty D3D11Swapchain => PropPointer(Props.RendererD3D11SwapchainPointer);

        /// <inheritdoc cref="CSDL.Props.RendererD3D12DevicePointer"/>
        public PointerProperty D3D12Device => PropPointer(Props.RendererD3D12DevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererD3D12SwapchainPointer"/>
        public PointerProperty D3D12Swapchain => PropPointer(Props.RendererD3D12SwapchainPointer);

        /// <inheritdoc cref="CSDL.Props.RendererD3D12CommandQueuePointer"/>
        public PointerProperty D3D12CommandQueue => PropPointer(Props.RendererD3D12CommandQueuePointer);

        /// <inheritdoc cref="CSDL.Props.RendererGPUDevicePointer"/>
        public PointerProperty GPUDevice => PropPointer(Props.RendererGPUDevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererMetalDevicePointer"/>
        public PointerProperty MetalDevice => PropPointer(Props.RendererMetalDevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererMetalCommandQueuePointer"/>
        public PointerProperty MetalCommandQueue => PropPointer(Props.RendererMetalCommandQueuePointer);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanInstancePointer"/>
        public PointerProperty VulkanInstance => PropPointer(Props.RendererVulkanInstancePointer);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanSurfaceNumber"/>
        public NumberProperty VulkanSurface => PropNumber(Props.RendererVulkanSurfaceNumber);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanPhysicalDevicePointer"/>
        public PointerProperty VulkanPhysicalDevice => PropPointer(Props.RendererVulkanPhysicalDevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanDevicePointer"/>
        public PointerProperty VulkanDevice => PropPointer(Props.RendererVulkanDevicePointer);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanGraphicsQueueFamilyIndexNumber"/>
        public NumberProperty VulkanGraphicsQueueFamilyIndex => PropNumber(Props.RendererVulkanGraphicsQueueFamilyIndexNumber);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanPresentQueueFamilyIndexNumber"/>
        public NumberProperty VulkanPresentQueueFamilyIndex => PropNumber(Props.RendererVulkanPresentQueueFamilyIndexNumber);

        /// <inheritdoc cref="CSDL.Props.RendererVulkanSwapchainImageCountNumber"/>
        public NumberProperty VulkanSwapchainImageCount => PropNumber(Props.RendererVulkanSwapchainImageCountNumber);
    }
}
