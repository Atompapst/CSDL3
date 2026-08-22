// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

global using static CSDL.Internal.NativeLibrary;

namespace CSDL.Internal {
    internal static class NativeLibrary {
        internal const string SDLLibrary = "SDL3";
        internal const string MixerLibrary = "SDL3_mixer";
        internal const string ImageLibrary = "SDL3_image";
        internal const string TTFLibrary = "SDL3_ttf";
        internal const string ShaderCrossLibrary = "SDL3_shadercross";
        internal const string NetLibrary = "SDL3_net";
        internal const string RtfLibrary = "SDL3_rtf";
    }
}
