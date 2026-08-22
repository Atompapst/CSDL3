// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Audio {
    public readonly struct AudioFormatInfo {
        public AudioFormat Format { get; }

        public AudioFormatInfo(AudioFormat format) {
            Format = format;
        }

        /// <inheritdoc cref="Macros.DefineAudioFormat"/>
        public AudioFormatInfo(bool isFloat, bool isSigned, bool isBigEndian, int size) {
            Format = Macros.DefineAudioFormat(isSigned, isBigEndian, isFloat, size);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioFormatName"/>
        public string Name => SDL.GetAudioFormatName(Format).ToUtf8String() ?? "SDL_AUDIO_UNKNOWN";
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetSilenceValueForFormat"/>
        public int SilenceValue => SDL.GetSilenceValueForFormat(Format);

        /// <inheritdoc cref="Macros.AudioBitSize"/>
        public uint BitsPerSample => Macros.AudioBitSize(Format);
        /// <inheritdoc cref="Macros.AudioByteSize"/>
        public uint BytesPerSample => Macros.AudioByteSize(Format);
        /// <inheritdoc cref="Macros.AudioIsFloat"/>
        public bool IsFloat => Macros.AudioIsFloat(Format);
        /// <inheritdoc cref="Macros.AudioIsInt"/>
        public bool IsInt => Macros.AudioIsInt(Format);
        /// <inheritdoc cref="Macros.AudioIsSigned"/>
        public bool IsSigned => Macros.AudioIsSigned(Format);
        /// <inheritdoc cref="Macros.AudioIsUnsigned"/>
        public bool IsUnsigned => Macros.AudioIsUnsigned(Format);
        /// <inheritdoc cref="Macros.AudioIsBigEndian"/>
        public bool IsBigEndian => Macros.AudioIsBigEndian(Format);
        /// <inheritdoc cref="Macros.AudioIsLittleEndian"/>
        public bool IsLittleEndian => Macros.AudioIsLittleEndian(Format);
        /// <inheritdoc cref="Macros.AudioMaskBigEndian"/>
        public static uint MaskBigEndian => Macros.AudioMaskBigEndian;
        /// <inheritdoc cref="Macros.AudioMaskBitSize"/>
        public static uint MaskBitSize => Macros.AudioMaskBitSize;
        /// <inheritdoc cref="Macros.AudioMaskFloat"/>
        public static uint MaskFloat => Macros.AudioMaskFloat;
        /// <inheritdoc cref="Macros.AudioMaskSigned"/>
        public static uint MaskSigned => Macros.AudioMaskSigned;
    }
}
