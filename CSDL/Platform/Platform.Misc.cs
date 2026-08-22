// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    public static partial class Platform {
        /// <inheritdoc cref="CSDL.Internal.Docs.Misc.OpenURL"/>
        /// <param name="url">the URL (or local file <c>file://</c> URI) to hand to the system.</param>
        /// <remarks>
        ///     Fire-and-forget: this returns as soon as the system was asked to open
        ///     <paramref name="url"/>, which says nothing about whether the browser (or whatever else
        ///     handles the scheme) actually managed to.
        /// </remarks>
        public static bool OpenUrl(string url) {
            Error.ThrowIfNullOrWhiteSpace(url, nameof(url), nameof(SDL.OpenURL));
            return SDL.OpenURL(url).LogIfFalse(nameof(SDL.OpenURL));
        }

        /// <summary>
        ///     This machine's byte order, as one of SDL's <c>SDL_LIL_ENDIAN</c> / <c>SDL_BIG_ENDIAN</c>
        ///     values - handy when talking to SDL's own endian helpers or a file format that stores
        ///     SDL's constants.
        /// </summary>
        /// <seealso cref="Macros.LilEndian"/>
        /// <seealso cref="Macros.BigEndian"/>
        public static uint ByteOrder => BitConverter.IsLittleEndian ? Macros.LilEndian : Macros.BigEndian;

        /// <summary><c>true</c> if this machine is little-endian; otherwise, <c>false</c>.</summary>
        public static bool IsLittleEndian => BitConverter.IsLittleEndian;

        /// <summary><c>true</c> if this machine is big-endian; otherwise, <c>false</c>.</summary>
        public static bool IsBigEndian => !BitConverter.IsLittleEndian;
    }
}
