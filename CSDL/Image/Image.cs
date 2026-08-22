// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.File;
using CSDL.GPU;
using CSDL.Video;
using Renderer = CSDL.Video.Renderer;

namespace CSDL.Image {
    public static partial class Image {
        /// <inheritdoc cref="CSDL.Internal.Docs.Image.Version"/>
        public static int Version => SDL.Version();

        /// <summary>
        /// Gets the major version of SDL_image this binding was generated against.
        /// </summary>
        public static uint MajorVersion => Macros.ImageMajorVersion;

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.Load"/>
        public static Surface Load(string path) {
            NativePtr<SurfaceData> surface = SDL.Load(path).ThrowIfInvalid();
            return new Surface(surface, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.Load_IO"/>
        public static Surface Load(IOStream src, bool closeAfter = false) {
            NativePtr<SurfaceData> surface = CompleteStreamLoad(src, closeAfter, SDL.Load_IO(src.Handle, closeAfter));
            return new Surface(surface, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadTyped_IO"/>
        public static Surface Load(IOStream src, ImageType type, bool closeAfter = false) {
            NativePtr<SurfaceData> surface = CompleteStreamLoad(src, closeAfter, SDL.LoadTyped_IO(src.Handle, closeAfter, type.ToString()));
            return new Surface(surface, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadTexture"/>
        public static Texture Load(Renderer renderer, string path) {
            NativePtr<TextureData> texture = SDL.LoadTexture(renderer.Handle, path).ThrowIfInvalid();
            return new Texture(texture, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadTexture_IO"/>
        public static Texture Load(Renderer renderer, IOStream src, bool closeAfter = false) {
            NativePtr<TextureData> texture = CompleteStreamLoad(src, closeAfter, SDL.LoadTexture_IO(renderer.Handle, src.Handle, closeAfter));
            return new Texture(texture, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadTextureTyped_IO"/>
        public static Texture Load(Renderer renderer, IOStream src, ImageType type, bool closeAfter = false) {
            NativePtr<TextureData> texture = CompleteStreamLoad(src, closeAfter, SDL.LoadTextureTyped_IO(renderer.Handle, src.Handle, closeAfter, type.ToString()));
            return new Texture(texture, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadGPUTexture"/>
        public static GPUTexture Load(GPUDevice device, GPUCopyPass copyPass, string path) {
            NativePtr<Opaque.SdlGPUTexture> texture = SDL.LoadGPUTexture(device.Handle, copyPass.Handle, path, out int width, out int height).ThrowIfInvalid();
            return new GPUTexture(texture, (uint)width, (uint)height, device, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadGPUTexture_IO"/>
        public static GPUTexture Load(GPUDevice device, GPUCopyPass copyPass, IOStream src, bool closeAfter = false) {
            NativePtr<Opaque.SdlGPUTexture> texture = CompleteStreamLoad(src, closeAfter, SDL.LoadGPUTexture_IO(device.Handle, copyPass.Handle, src.Handle, closeAfter, out int width, out int height));
            return new GPUTexture(texture, (uint)width, (uint)height, device, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadGPUTextureTyped_IO"/>
        public static GPUTexture Load(GPUDevice device, GPUCopyPass copyPass, IOStream src, ImageType type, bool closeAfter = false) {
            NativePtr<Opaque.SdlGPUTexture> texture = CompleteStreamLoad(src, closeAfter, SDL.LoadGPUTextureTyped_IO(device.Handle, copyPass.Handle, src.Handle, closeAfter, type.ToString(), out int width, out int height));
            return new GPUTexture(texture, (uint)width, (uint)height, device, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.GetClipboardImage"/>
        public static Surface GetClipboardImage() {
            NativePtr<SurfaceData> surface = SDL.GetClipboardImage().ThrowIfInvalid();
            return new Surface(surface, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.Save"/>
        public static bool Save(Surface surface, string path) {
            return SDL.Save(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveJPG"/>
        public static bool SaveJPG(Surface surface, string path, int quality) {
            return SDL.SaveJPG(surface.Handle, path, quality).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SavePNG"/>
        public static bool SavePNG(Surface surface, string path) {
            return SDL.SavePNG(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveAVIF"/>
        public static bool SaveAVIF(Surface surface, string path, int quality) {
            return SDL.SaveAVIF(surface.Handle, path, quality).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveBMP"/>
        public static bool SaveBMP(Surface surface, string path) {
            return SDL.SaveBMP(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveCUR"/>
        public static bool SaveCUR(Surface surface, string path) {
            return SDL.SaveCUR(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveGIF"/>
        public static bool SaveGIF(Surface surface, string path) {
            return SDL.SaveGIF(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveICO"/>
        public static bool SaveICO(Surface surface, string path) {
            return SDL.SaveICO(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveTGA"/>
        public static bool SaveTGA(Surface surface, string path) {
            return SDL.SaveTGA(surface.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveWEBP"/>
        public static bool SaveWEBP(Surface surface, string path, float quality) {
            return SDL.SaveWEBP(surface.Handle, path, quality).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveTyped_IO"/>
        public static bool Save(Surface surface, IOStream destination, ImageType type, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveTyped_IO(surface.Handle, destination.Handle, closeAfter, type.ToString()));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveAVIF_IO"/>
        public static bool SaveAVIF(Surface surface, IOStream destination, int quality, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveAVIF_IO(surface.Handle, destination.Handle, closeAfter, quality));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveBMP_IO"/>
        public static bool SaveBMP(Surface surface, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveBMP_IO(surface.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveCUR_IO"/>
        public static bool SaveCUR(Surface surface, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveCUR_IO(surface.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveGIF_IO"/>
        public static bool SaveGIF(Surface surface, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveGIF_IO(surface.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveICO_IO"/>
        public static bool SaveICO(Surface surface, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveICO_IO(surface.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveJPG_IO"/>
        public static bool SaveJPG(Surface surface, IOStream destination, int quality, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveJPG_IO(surface.Handle, destination.Handle, closeAfter, quality));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SavePNG_IO"/>
        public static bool SavePNG(Surface surface, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SavePNG_IO(surface.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveTGA_IO"/>
        public static bool SaveTGA(Surface surface, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveTGA_IO(surface.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveWEBP_IO"/>
        public static bool SaveWEBP(Surface surface, IOStream destination, float quality, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveWEBP_IO(surface.Handle, destination.Handle, closeAfter, quality));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveAnimation"/>
        public static bool SaveAnimation(Animation animation, string path) {
            return SDL.SaveAnimation(animation.Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveAnimationTyped_IO"/>
        public static bool SaveAnimation(Animation animation, IOStream destination, ImageType type, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveAnimationTyped_IO(animation.Handle, destination.Handle, closeAfter, type.ToString()));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveANIAnimation_IO"/>
        public static bool SaveANI(Animation animation, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveANIAnimation_IO(animation.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveAPNGAnimation_IO"/>
        public static bool SaveAPNG(Animation animation, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveAPNGAnimation_IO(animation.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveAVIFAnimation_IO"/>
        public static bool SaveAVIF(Animation animation, IOStream destination, int quality, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveAVIFAnimation_IO(animation.Handle, destination.Handle, closeAfter, quality));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveGIFAnimation_IO"/>
        public static bool SaveGIF(Animation animation, IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveGIFAnimation_IO(animation.Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.SaveWEBPAnimation_IO"/>
        public static bool SaveWEBP(Animation animation, IOStream destination, int quality, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveWEBPAnimation_IO(animation.Handle, destination.Handle, closeAfter, quality));
        }

        /// <summary>
        /// Detects the image type contained in the stream by probing its content, without loading it.
        /// </summary>
        public static ImageType IsImageType(IOStream stream) {
            NativePtr<Opaque.SdlIOStream> src = stream.Handle;
            if (SDL.isAVIF(src)) return ImageType.AVIF;
            if (SDL.isANI(src)) return ImageType.ANI;
            if (SDL.isBMP(src)) return ImageType.BMP;
            if (SDL.isCUR(src)) return ImageType.CUR;
            if (SDL.isGIF(src)) return ImageType.GIF;
            if (SDL.isICO(src)) return ImageType.ICO;
            if (SDL.isJPG(src)) return ImageType.JPG;
            if (SDL.isJXL(src)) return ImageType.JXL;
            if (SDL.isLBM(src)) return ImageType.LBM;
            if (SDL.isPCX(src)) return ImageType.PCX;
            if (SDL.isPNG(src)) return ImageType.PNG;
            if (SDL.isPNM(src)) return ImageType.PNM;
            if (SDL.isQOI(src)) return ImageType.QOI;
            if (SDL.isSVG(src)) return ImageType.SVG;
            if (SDL.isTIF(src)) return ImageType.TIFF;
            if (SDL.isWEBP(src)) return ImageType.WEBP;
            if (SDL.isXCF(src)) return ImageType.XCF;
            if (SDL.isXPM(src)) return ImageType.XPM;
            if (SDL.isXV(src)) return ImageType.XV;
            return ImageType.Unknown;
        }

        // SDL closes an input stream before returning when closeio=true, including on failure.
        private static NativePtr<T> CompleteStreamLoad<T>(IOStream source, bool closeAfter, NativePtr<T> result) where T : unmanaged {
            if (closeAfter) {
                source.Invalidate();
            }

            return result.ThrowIfInvalid();
        }

        // SDL closes a stream passed with closeio=true, so prevent IOStream.Dispose from closing it again.
        private static bool CompleteStreamSave(IOStream destination, bool closeAfter, CBool result) {
            if (closeAfter) {
                destination.Handle = NativePtr<Opaque.SdlIOStream>.Zero;
            }

            return result.LogIfFalse();
        }
    }
}
