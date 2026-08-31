// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Text;

namespace CSDL.TTF {

    public static partial class TTF {

        #region CSDL_IMPL TTF_StringToTag : SDL_ttf#TTF_StringToTag

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.StringToTag"/>
        public static uint StringToTag(string? tag) {
            // pack the first 4 bytes of the UTF-8 encoding of `tag` big-endian, zero-padding if shorter.
            Span<byte> bytes = stackalloc byte[4];
            Span<byte> runeBytes = stackalloc byte[4];
            if (!string.IsNullOrEmpty(tag)) {
                int written = 0;
                int i = 0;
                while (i < tag.Length && written < 4) {
                    char c = tag[i];
                    if (c == '\0') {
                        break;
                    }

                    if (c < 0x80) {
                        bytes[written++] = (byte)c;
                        i++;
                        continue;
                    }

                    // Native marshals the managed string to UTF-8 before ever reaching the C side,
                    // and that marshaller substitutes U+FFFD for an unpaired surrogate rather than
                    // stopping - match that instead of just bailing out here.
                    Rune rune;
                    if (Rune.TryGetRuneAt(tag, i, out rune)) {
                        i += rune.Utf16SequenceLength;
                    } else {
                        rune = Rune.ReplacementChar;
                        i++;
                    }

                    rune.TryEncodeToUtf8(runeBytes, out int runeByteCount);
                    int copyCount = Math.Min(runeByteCount, 4 - written);
                    runeBytes[..copyCount].CopyTo(bytes[written..]);
                    written += copyCount;
                }
            }

            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        #endregion

        #region CSDL_IMPL TTF_TagToString : SDL_ttf#TTF_TagToString

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.TagToString"/>
        public static string TagToString(uint tag) {
            // unpack the 4 bytes big-endian, then stop at the first NUL byte the way a C string reader would.
            Span<byte> bytes = stackalloc byte[4];
            for (int i = 0; i < 4; i++) {
                bytes[i] = (byte)(tag >> 24);
                tag <<= 8;
            }

            int length = bytes.IndexOf((byte)0);
            if (length < 0) {
                length = 4;
            }

            return Encoding.UTF8.GetString(bytes[..length]);
        }

        #endregion

    }
}
