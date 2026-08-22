// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.File;

namespace CSDL.Video {
    public partial class Surface {
        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadSurface"/>
        public static Surface Load(string file) {
            return new Surface(SDL.LoadSurface(file).ThrowIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadSurface_IO"/>
        public static Surface Load(IOStream source, bool closeAfter = false) {
            return CompleteStreamLoad(source, closeAfter, SDL.LoadSurface_IO(source.Handle, closeAfter), nameof(SDL.LoadSurface_IO));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadBMP"/>
        public static Surface LoadBMP(string file) {
            return new Surface(SDL.LoadBMP(file).ThrowIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadBMP_IO"/>
        public static Surface LoadBMP(IOStream source, bool closeAfter = false) {
            return CompleteStreamLoad(source, closeAfter, SDL.LoadBMP_IO(source.Handle, closeAfter), nameof(SDL.LoadBMP_IO));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadPNG"/>
        public static Surface LoadPNG(string file) {
            return new Surface(SDL.LoadPNG(file).ThrowIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadPNG_IO"/>
        public static Surface LoadPNG(IOStream source, bool closeAfter = false) {
            return CompleteStreamLoad(source, closeAfter, SDL.LoadPNG_IO(source.Handle, closeAfter), nameof(SDL.LoadPNG_IO));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadJPG"/>
        public static Surface LoadJPG(string file) {
            return new Surface(SDL.LoadJPG(file).ThrowIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LoadJPG_IO"/>
        public static Surface LoadJPG(IOStream source, bool closeAfter = false) {
            return CompleteStreamLoad(source, closeAfter, SDL.LoadJPG_IO(source.Handle, closeAfter), nameof(SDL.LoadJPG_IO));
        }

        // SDL closes a stream passed with closeio=true - even on failure - so prevent IOStream.Dispose
        // from closing it a second time before the null check throws.
        private static Surface CompleteStreamLoad(IOStream source, bool closeAfter, NativePtr<SurfaceData> result, string operation) {
            if (closeAfter) {
                source.Handle = NativePtr<CSDL.Opaque.SdlIOStream>.Zero;
            }

            return new Surface(result.ThrowIfInvalid(operation), true);
        }
    }
}
