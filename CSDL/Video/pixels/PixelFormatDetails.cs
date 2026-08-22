// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    public partial struct PixelFormatDetails {
        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.GetRGB"/>
        public readonly void GetRGB(uint pixelValue, Palette? palette, out byte r, out byte g, out byte b) {
            SDL.GetRGB(pixelValue, in this, palette?.Handle ?? default, out r, out g, out b);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.GetRGBA"/>
        public readonly void GetRGBA(uint pixelValue, Palette? palette, out byte r, out byte g, out byte b, out byte a) {
            SDL.GetRGBA(pixelValue, in this, palette?.Handle ?? default, out r, out g, out b, out a);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.MapRGB"/>
        public readonly uint MapRGB(Palette? palette, byte r, byte g, byte b) {
            return SDL.MapRGB(in this, palette?.Handle ?? default, r, g, b);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.MapRGBA"/>
        public readonly uint MapRGBA(Palette? palette, byte r, byte g, byte b, byte a) {
            return SDL.MapRGBA(in this, palette?.Handle ?? default, r, g, b, a);
        }
    }
}
