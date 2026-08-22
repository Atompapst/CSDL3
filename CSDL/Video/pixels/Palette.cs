// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Video {
    public class Palette : NativeHandle<PaletteData> {

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.CreatePalette"/>
        public Palette(int ncolors) {
            Handle = SDL.CreatePalette(ncolors).LogIfInvalid();
        }

        internal Palette(NativePtr<PaletteData> ptr, bool ownsHandle = false) : base(ptr, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.SetPaletteColors"/>
        public bool SetColors(Color[] colors, int firstColor = 0) {
            int ncolors = colors?.Length ?? 0;
            unsafe {
                fixed (Color* c = colors) {
                    return SDL.SetPaletteColors(Handle, c, firstColor, ncolors).LogIfFalse();
                }
            }

        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.DestroyPalette"/>
        protected override void DisposeResource() {
            SDL.DestroyPalette(Handle);
        }
    }
}
