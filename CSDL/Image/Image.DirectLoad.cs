// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.File;
using CSDL.Video;

namespace CSDL.Image {
    /// <summary>
    /// The format-specific loaders. These skip SDL_image's format detection, so only reach for them
    /// when the format is already known - <see cref="Load(IOStream,bool)"/> and its overloads are the
    /// better default. None of them close the source stream; the caller keeps owning it.
    /// </summary>
    public static partial class Image {
        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAVIF_IO"/>
        public static Surface LoadAVIF(IOStream src) {
            return Wrap(SDL.LoadAVIF_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadBMP_IO"/>
        public static Surface LoadBMP(IOStream src) {
            return Wrap(SDL.LoadBMP_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadCUR_IO"/>
        public static Surface LoadCUR(IOStream src) {
            return Wrap(SDL.LoadCUR_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadGIF_IO"/>
        public static Surface LoadGIF(IOStream src) {
            return Wrap(SDL.LoadGIF_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadICO_IO"/>
        public static Surface LoadICO(IOStream src) {
            return Wrap(SDL.LoadICO_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadJPG_IO"/>
        public static Surface LoadJPG(IOStream src) {
            return Wrap(SDL.LoadJPG_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadJXL_IO"/>
        public static Surface LoadJXL(IOStream src) {
            return Wrap(SDL.LoadJXL_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadLBM_IO"/>
        public static Surface LoadLBM(IOStream src) {
            return Wrap(SDL.LoadLBM_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadPCX_IO"/>
        public static Surface LoadPCX(IOStream src) {
            return Wrap(SDL.LoadPCX_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadPNG_IO"/>
        public static Surface LoadPNG(IOStream src) {
            return Wrap(SDL.LoadPNG_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadPNM_IO"/>
        public static Surface LoadPNM(IOStream src) {
            return Wrap(SDL.LoadPNM_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadQOI_IO"/>
        public static Surface LoadQOI(IOStream src) {
            return Wrap(SDL.LoadQOI_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadSVG_IO"/>
        public static Surface LoadSVG(IOStream src) {
            return Wrap(SDL.LoadSVG_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadSizedSVG_IO"/>
        public static Surface LoadSVG(IOStream src, int width, int height) {
            return Wrap(SDL.LoadSizedSVG_IO(Stream(src), width, height));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadTGA_IO"/>
        public static Surface LoadTGA(IOStream src) {
            return Wrap(SDL.LoadTGA_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadTIF_IO"/>
        public static Surface LoadTIF(IOStream src) {
            return Wrap(SDL.LoadTIF_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadWEBP_IO"/>
        public static Surface LoadWEBP(IOStream src) {
            return Wrap(SDL.LoadWEBP_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadXCF_IO"/>
        public static Surface LoadXCF(IOStream src) {
            return Wrap(SDL.LoadXCF_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadXPM_IO"/>
        public static Surface LoadXPM(IOStream src) {
            return Wrap(SDL.LoadXPM_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadXV_IO"/>
        public static Surface LoadXV(IOStream src) {
            return Wrap(SDL.LoadXV_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadANIAnimation_IO"/>
        public static Animation LoadANIAnimation(IOStream src) {
            return WrapAnimation(SDL.LoadANIAnimation_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAPNGAnimation_IO"/>
        public static Animation LoadAPNGAnimation(IOStream src) {
            return WrapAnimation(SDL.LoadAPNGAnimation_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAVIFAnimation_IO"/>
        public static Animation LoadAVIFAnimation(IOStream src) {
            return WrapAnimation(SDL.LoadAVIFAnimation_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadGIFAnimation_IO"/>
        public static Animation LoadGIFAnimation(IOStream src) {
            return WrapAnimation(SDL.LoadGIFAnimation_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadWEBPAnimation_IO"/>
        public static Animation LoadWEBPAnimation(IOStream src) {
            return WrapAnimation(SDL.LoadWEBPAnimation_IO(Stream(src)));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.ReadXPMFromArray"/>
        /// <param name="xpm">a complete XPM image: the header line, the color lines, then the pixel rows.</param>
        public static Surface ReadXPM(string[] xpm) {
            using NativeStringArray.Native native = AllocateXPM(xpm);
            return Wrap(SDL.ReadXPMFromArray(native.Ptr));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.ReadXPMFromArrayToRGB888"/>
        /// <param name="xpm">a complete XPM image: the header line, the color lines, then the pixel rows.</param>
        public static Surface ReadXPMToRGB888(string[] xpm) {
            using NativeStringArray.Native native = AllocateXPM(xpm);
            return Wrap(SDL.ReadXPMFromArrayToRGB888(native.Ptr));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimatedCursor"/>
        /// <param name="animation">the animation to build the cursor from.</param>
        /// <param name="hotX">the x position of the cursor hot spot, shared by all frames.</param>
        /// <param name="hotY">the y position of the cursor hot spot, shared by all frames.</param>
        public static Input.Cursor CreateAnimatedCursor(Animation animation, int hotX, int hotY) {
            ArgumentNullException.ThrowIfNull(animation);
            return new Input.Cursor(SDL.CreateAnimatedCursor(animation.Handle, hotX, hotY).ThrowIfInvalid(), true);
        }

        private static NativePtr<Opaque.SdlIOStream> Stream(IOStream src) {
            ArgumentNullException.ThrowIfNull(src);
            return src.Handle;
        }

        private static NativeStringArray.Native AllocateXPM(string[] xpm) {
            ArgumentNullException.ThrowIfNull(xpm);
            if (xpm.Length == 0) {
                throw new ArgumentException("An XPM image needs at least a header line.", nameof(xpm));
            }
            return NativeStringArray.Allocate(xpm);
        }

        private static Surface Wrap(NativePtr<SurfaceData> surface) {
            return new Surface(surface.ThrowIfInvalid(), true);
        }

        private static Animation WrapAnimation(NativePtr<AnimationData> animation) {
            return new Animation(animation.ThrowIfInvalid());
        }
    }
}
