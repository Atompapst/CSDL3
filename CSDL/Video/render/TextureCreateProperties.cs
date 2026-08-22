// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Video {
    public class TextureCreateProperties : PropertyGroup {
        /// <inheritdoc cref="CSDL.Props.TextureCreateColorspaceNumber"/>
        public NumberProperty Colorspace => PropNumber(Props.TextureCreateColorspaceNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateFormatNumber"/>
        public NumberProperty Format => PropNumber(Props.TextureCreateFormatNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateAccessNumber"/>
        public NumberProperty Access => PropNumber(Props.TextureCreateAccessNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateWidthNumber"/>
        public NumberProperty Width => PropNumber(Props.TextureCreateWidthNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateHeightNumber"/>
        public NumberProperty Height => PropNumber(Props.TextureCreateHeightNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateSdrWhitePointFloat"/>
        public FloatProperty SdrWhitePoint => PropFloat(Props.TextureCreateSdrWhitePointFloat);
        /// <inheritdoc cref="CSDL.Props.TextureCreateHDRHeadroomFloat"/>
        public FloatProperty HDRHeadroom => PropFloat(Props.TextureCreateHDRHeadroomFloat);
        /// <inheritdoc cref="CSDL.Props.TextureCreateD3D11TexturePointer"/>
        public PointerProperty D3D11Texture => PropPointer(Props.TextureCreateD3D11TexturePointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateD3D11TextureUPointer"/>
        public PointerProperty D3D11TextureU => PropPointer(Props.TextureCreateD3D11TextureUPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateD3D11TextureVPointer"/>
        public PointerProperty D3D11TextureV => PropPointer(Props.TextureCreateD3D11TextureVPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateD3D12TexturePointer"/>
        public PointerProperty D3D12Texture => PropPointer(Props.TextureCreateD3D12TexturePointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateD3D12TextureUPointer"/>
        public PointerProperty D3D12TextureU => PropPointer(Props.TextureCreateD3D12TextureUPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateD3D12TextureVPointer"/>
        public PointerProperty D3D12TextureV => PropPointer(Props.TextureCreateD3D12TextureVPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateMetalPixelbufferPointer"/>
        public PointerProperty MetalPixelBuffer => PropPointer(Props.TextureCreateMetalPixelbufferPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateMetalTexturePointer"/>
        public PointerProperty MetalTexture => PropPointer(Props.TextureCreateMetalTexturePointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateMetalTextureUPointer"/>
        public PointerProperty MetalTextureU => PropPointer(Props.TextureCreateMetalTextureUPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateMetalTextureVPointer"/>
        public PointerProperty MetalTextureV => PropPointer(Props.TextureCreateMetalTextureVPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateMetalTextureUvPointer"/>
        public PointerProperty MetalTextureUV => PropPointer(Props.TextureCreateMetalTextureUvPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateMetalTextureUsageNumber"/>
        public NumberProperty MetalTextureUsage => PropNumber(Props.TextureCreateMetalTextureUsageNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOpenglTextureNumber"/>
        public NumberProperty OpenGLTexture => PropNumber(Props.TextureCreateOpenglTextureNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOpenglTextureUNumber"/>
        public NumberProperty OpenGLTextureU => PropNumber(Props.TextureCreateOpenglTextureUNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOpenglTextureVNumber"/>
        public NumberProperty OpenGLTextureV => PropNumber(Props.TextureCreateOpenglTextureVNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOpenglTextureUvNumber"/>
        public NumberProperty OpenGLTextureUV => PropNumber(Props.TextureCreateOpenglTextureUvNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOPENGLES2TextureNumber"/>
        public NumberProperty OpenGLES2Texture => PropNumber(Props.TextureCreateOPENGLES2TextureNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOPENGLES2TextureUNumber"/>
        public NumberProperty OpenGLES2TextureU => PropNumber(Props.TextureCreateOPENGLES2TextureUNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOPENGLES2TextureVNumber"/>
        public NumberProperty OpenGLES2TextureV => PropNumber(Props.TextureCreateOPENGLES2TextureVNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateOPENGLES2TextureUvNumber"/>
        public NumberProperty OpenGLES2TextureUV => PropNumber(Props.TextureCreateOPENGLES2TextureUvNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateVulkanTextureNumber"/>
        public NumberProperty VulkanTexture => PropNumber(Props.TextureCreateVulkanTextureNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreateVulkanLayoutNumber"/>
        public NumberProperty VulkanLayout => PropNumber(Props.TextureCreateVulkanLayoutNumber);
        /// <inheritdoc cref="CSDL.Props.TextureCreatePalettePointer"/>
        public PointerProperty Palette => PropPointer(Props.TextureCreatePalettePointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateGPUTexturePointer"/>
        public PointerProperty GPUTexture => PropPointer(Props.TextureCreateGPUTexturePointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateGPUTextureUPointer"/>
        public PointerProperty GPUTextureU => PropPointer(Props.TextureCreateGPUTextureUPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateGPUTextureVPointer"/>
        public PointerProperty GPUTextureV => PropPointer(Props.TextureCreateGPUTextureVPointer);
        /// <inheritdoc cref="CSDL.Props.TextureCreateGPUTextureUvPointer"/>
        public PointerProperty GPUTextureUV => PropPointer(Props.TextureCreateGPUTextureUvPointer);
    }
}
