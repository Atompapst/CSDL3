// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;
namespace CSDL.Video {
    /// <summary>
    /// The properties SDL keeps for an existing <see cref="Texture"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="TextureCreateProperties"/>, which the application fills in and hands to
    /// <see cref="Texture(Renderer,TextureCreateProperties)"/>, this group is created and owned by SDL and
    /// lives as long as the texture does. It must not be disposed, so the finalizer that would otherwise
    /// destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Texture.Properties"/>
    public sealed class TextureProperties : PropertyGroup {
        internal TextureProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.Props.TextureColorspaceNumber"/>
        public NumberProperty Colorspace => PropNumber(Props.TextureColorspaceNumber);

        /// <inheritdoc cref="CSDL.Props.TextureFormatNumber"/>
        public NumberProperty Format => PropNumber(Props.TextureFormatNumber);

        /// <inheritdoc cref="CSDL.Props.TextureAccessNumber"/>
        public NumberProperty Access => PropNumber(Props.TextureAccessNumber);

        /// <inheritdoc cref="CSDL.Props.TextureWidthNumber"/>
        public NumberProperty Width => PropNumber(Props.TextureWidthNumber);

        /// <inheritdoc cref="CSDL.Props.TextureHeightNumber"/>
        public NumberProperty Height => PropNumber(Props.TextureHeightNumber);

        /// <inheritdoc cref="CSDL.Props.TextureSdrWhitePointFloat"/>
        public FloatProperty SDRWhitePoint => PropFloat(Props.TextureSdrWhitePointFloat);

        /// <inheritdoc cref="CSDL.Props.TextureHDRHeadroomFloat"/>
        public FloatProperty HDRHeadroom => PropFloat(Props.TextureHDRHeadroomFloat);

        /// <inheritdoc cref="CSDL.Props.TextureD3D11TexturePointer"/>
        public PointerProperty D3D11Texture => PropPointer(Props.TextureD3D11TexturePointer);

        /// <inheritdoc cref="CSDL.Props.TextureD3D11TextureUPointer"/>
        public PointerProperty D3D11TextureU => PropPointer(Props.TextureD3D11TextureUPointer);

        /// <inheritdoc cref="CSDL.Props.TextureD3D11TextureVPointer"/>
        public PointerProperty D3D11TextureV => PropPointer(Props.TextureD3D11TextureVPointer);

        /// <inheritdoc cref="CSDL.Props.TextureD3D12TexturePointer"/>
        public PointerProperty D3D12Texture => PropPointer(Props.TextureD3D12TexturePointer);

        /// <inheritdoc cref="CSDL.Props.TextureD3D12TextureUPointer"/>
        public PointerProperty D3D12TextureU => PropPointer(Props.TextureD3D12TextureUPointer);

        /// <inheritdoc cref="CSDL.Props.TextureD3D12TextureVPointer"/>
        public PointerProperty D3D12TextureV => PropPointer(Props.TextureD3D12TextureVPointer);

        /// <inheritdoc cref="CSDL.Props.TextureGPUTexturePointer"/>
        public PointerProperty GPUTexture => PropPointer(Props.TextureGPUTexturePointer);

        /// <inheritdoc cref="CSDL.Props.TextureGPUTextureUPointer"/>
        public PointerProperty GPUTextureU => PropPointer(Props.TextureGPUTextureUPointer);

        /// <inheritdoc cref="CSDL.Props.TextureGPUTextureVPointer"/>
        public PointerProperty GPUTextureV => PropPointer(Props.TextureGPUTextureVPointer);

        /// <inheritdoc cref="CSDL.Props.TextureGPUTextureUvPointer"/>
        public PointerProperty GPUTextureUV => PropPointer(Props.TextureGPUTextureUvPointer);

        /// <inheritdoc cref="CSDL.Props.TextureMetalTexturePointer"/>
        public PointerProperty MetalTexture => PropPointer(Props.TextureMetalTexturePointer);

        /// <inheritdoc cref="CSDL.Props.TextureMetalTextureUPointer"/>
        public PointerProperty MetalTextureU => PropPointer(Props.TextureMetalTextureUPointer);

        /// <inheritdoc cref="CSDL.Props.TextureMetalTextureVPointer"/>
        public PointerProperty MetalTextureV => PropPointer(Props.TextureMetalTextureVPointer);

        /// <inheritdoc cref="CSDL.Props.TextureMetalTextureUvPointer"/>
        public PointerProperty MetalTextureUV => PropPointer(Props.TextureMetalTextureUvPointer);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTextureNumber"/>
        public NumberProperty OpenGLTexture => PropNumber(Props.TextureOpenglTextureNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTextureUNumber"/>
        public NumberProperty OpenGLTextureU => PropNumber(Props.TextureOpenglTextureUNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTextureVNumber"/>
        public NumberProperty OpenGLTextureV => PropNumber(Props.TextureOpenglTextureVNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTextureUvNumber"/>
        public NumberProperty OpenGLTextureUV => PropNumber(Props.TextureOpenglTextureUvNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTextureTargetNumber"/>
        public NumberProperty OpenGLTextureTarget => PropNumber(Props.TextureOpenglTextureTargetNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTexWFloat"/>
        public FloatProperty OpenGLTexWidth => PropFloat(Props.TextureOpenglTexWFloat);

        /// <inheritdoc cref="CSDL.Props.TextureOpenglTexHFloat"/>
        public FloatProperty OpenGLTexHeight => PropFloat(Props.TextureOpenglTexHFloat);

        /// <inheritdoc cref="CSDL.Props.TextureOPENGLES2TextureNumber"/>
        public NumberProperty OpenGLES2Texture => PropNumber(Props.TextureOPENGLES2TextureNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOPENGLES2TextureUNumber"/>
        public NumberProperty OpenGLES2TextureU => PropNumber(Props.TextureOPENGLES2TextureUNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOPENGLES2TextureVNumber"/>
        public NumberProperty OpenGLES2TextureV => PropNumber(Props.TextureOPENGLES2TextureVNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOPENGLES2TextureUvNumber"/>
        public NumberProperty OpenGLES2TextureUV => PropNumber(Props.TextureOPENGLES2TextureUvNumber);

        /// <inheritdoc cref="CSDL.Props.TextureOPENGLES2TextureTargetNumber"/>
        public NumberProperty OpenGLES2TextureTarget => PropNumber(Props.TextureOPENGLES2TextureTargetNumber);

        /// <inheritdoc cref="CSDL.Props.TextureVulkanTextureNumber"/>
        public NumberProperty VulkanTexture => PropNumber(Props.TextureVulkanTextureNumber);
    }
}
