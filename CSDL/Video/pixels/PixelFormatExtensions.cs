// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.Video {
    public static class PixelFormatExtensions {
        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.GetPixelFormatDetails"/>
        public static PixelFormatDetails GetDetails(this PixelFormat format) {
            return SDL.GetPixelFormatDetails(format).ThrowIfInvalid().AsReadOnlyRef();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.GetMasksForPixelFormat"/>
        public static bool TryGetMasks(this PixelFormat format, out int bitsPerPixel, out uint rMask, out uint gMask, out uint bMask, out uint aMask) {
            bitsPerPixel = 0;
            return SDL.GetMasksForPixelFormat(format, NativePtr<int>.FromRef(ref bitsPerPixel), out rMask, out gMask, out bMask, out aMask).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Video.Macros.DefinePixelfourcc"/>
        public static PixelFormat FromFourCC(char a, char b, char c, char d) {
            return (PixelFormat)Macros.DefinePixelfourcc(a, b, c, d);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.GetPixelFormatName"/>
        public static string GetName(this PixelFormat format) {
            return SDL.GetPixelFormatName(format).ToUtf8String() ?? "SDL_PIXELFORMAT_UNKNOWN";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.GetPixelFormatForMasks"/>
        public static PixelFormat FromMasks(int bitsPerPixel, uint rMask, uint gMask, uint bMask, uint aMask) {
            return SDL.GetPixelFormatForMasks(bitsPerPixel, rMask, gMask, bMask, aMask);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.DefinePixelformat"/>
        /// <param name="type">the pixel type.</param>
        /// <param name="order">the channel order - a <see cref="BitmapOrder"/>, <see cref="PackedOrder"/> or
        /// <see cref="ArrayOrder"/> value, depending on <paramref name="type"/>.</param>
        /// <param name="layout">the bit layout, or <see cref="PackedLayout.None"/> where a layout doesn't apply.</param>
        /// <param name="bitsPerPixel">the bits per pixel.</param>
        /// <param name="bytesPerPixel">the bytes per pixel.</param>
        public static PixelFormat Define(PixelType type, uint order, PackedLayout layout, uint bitsPerPixel, uint bytesPerPixel) {
            return (PixelFormat)Macros.DefinePixelformat((uint)type, order, (uint)layout, bitsPerPixel, bytesPerPixel);
        }

        /// <inheritdoc cref="Define(PixelType,uint,PackedLayout,uint,uint)"/>
        public static PixelFormat Define(PixelType type, BitmapOrder order, PackedLayout layout, uint bitsPerPixel, uint bytesPerPixel) {
            return Define(type, (uint)order, layout, bitsPerPixel, bytesPerPixel);
        }

        /// <inheritdoc cref="Define(PixelType,uint,PackedLayout,uint,uint)"/>
        public static PixelFormat Define(PixelType type, PackedOrder order, PackedLayout layout, uint bitsPerPixel, uint bytesPerPixel) {
            return Define(type, (uint)order, layout, bitsPerPixel, bytesPerPixel);
        }

        /// <inheritdoc cref="Define(PixelType,uint,PackedLayout,uint,uint)"/>
        public static PixelFormat Define(PixelType type, ArrayOrder order, PackedLayout layout, uint bitsPerPixel, uint bytesPerPixel) {
            return Define(type, (uint)order, layout, bitsPerPixel, bytesPerPixel);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Bitsperpixel"/>
        public static uint BitsPerPixel(this PixelFormat format) {
            return Macros.Bitsperpixel((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Bytesperpixel"/>
        public static uint BytesPerPixel(this PixelFormat format) {
            return Macros.Bytesperpixel((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Pixelflag"/>
        public static uint Flag(this PixelFormat format) {
            return Macros.Pixelflag((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Pixellayout"/>
        public static uint Layout(this PixelFormat format) {
            return Macros.Pixellayout((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Pixelorder"/>
        public static uint Order(this PixelFormat format) {
            return Macros.Pixelorder((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Pixeltype"/>
        public static uint Type(this PixelFormat format) {
            return Macros.Pixeltype((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IspixelformatFourcc"/>
        public static bool IsFourCC(this PixelFormat format) {
            return Macros.IspixelformatFourcc((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IspixelformatAlpha"/>
        public static bool HasAlpha(this PixelFormat format) {
            return Macros.IspixelformatAlpha((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IspixelformatArray"/>
        public static bool IsArray(this PixelFormat format) {
            return Macros.IspixelformatArray((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IspixelformatFloat"/>
        public static bool IsFloat(this PixelFormat format) {
            return Macros.IspixelformatFloat((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IspixelformatIndexed"/>
        public static bool IsIndexed(this PixelFormat format) {
            return Macros.IspixelformatIndexed((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IspixelformatPacked"/>
        public static bool IsPacked(this PixelFormat format) {
            return Macros.IspixelformatPacked((uint)format);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Ispixelformat10BIT"/>
        public static bool Is10Bit(this PixelFormat format) {
            return Macros.Ispixelformat10BIT((uint)format);
        }
    }
}
