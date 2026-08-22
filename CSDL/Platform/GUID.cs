// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;

namespace CSDL {
    public partial struct GUIDData {
        /// <summary>The number of bytes an <c>SDL_GUID</c> is made of.</summary>
        public const int Size = 16;

        // SDL_GUIDToString writes 32 hex digits plus the NUL terminator.
        private const int StringBufferSize = 33;

        /// <summary><see langword="true"/> if every byte is zero - what SDL hands back for an unparsable string.</summary>
        public readonly bool IsZero {
            get {
                for (int i = 0; i < Size; i++) {
                    if (GetData(i) != 0) return false;
                }
                return true;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GUID.GUIDToString"/>
        public override readonly unsafe string ToString() {
            byte* buffer = stackalloc byte[StringBufferSize];
            SDL.GUIDToString(this, buffer, StringBufferSize);
            return Marshal.PtrToStringUTF8((nint)buffer) ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GUID.StringToGUID"/>
        public static GUIDData Parse(string value) {
            ArgumentNullException.ThrowIfNull(value);
            return SDL.StringToGUID(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GUID.StringToGUID"/>
        /// <returns>
        ///     <see langword="false"/> if SDL could not make sense of <paramref name="value"/>, which it
        ///     reports by handing back an all-zero GUID.
        /// </returns>
        public static bool TryParse(string? value, out GUIDData guid) {
            if (string.IsNullOrWhiteSpace(value)) {
                guid = default;
                return false;
            }

            guid = SDL.StringToGUID(value);
            return !guid.IsZero;
        }

        /// <summary>Copies the 16 raw bytes into a managed array.</summary>
        public readonly byte[] ToArray() {
            byte[] bytes = new byte[Size];
            for (int i = 0; i < Size; i++) {
                bytes[i] = GetData(i);
            }
            return bytes;
        }
    }
}
