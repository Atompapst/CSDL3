// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    public partial class Surface {
        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.MapSurfaceRGB"/>
        public uint MapRGB(byte r, byte g, byte b) {
            return SDL.MapSurfaceRGB(Handle, r, g, b);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Pixels.MapSurfaceRGBA"/>
        public uint MapRGBA(byte r, byte g, byte b, byte a) {
            return SDL.MapSurfaceRGBA(Handle, r, g, b, a);
        }
    }
}
