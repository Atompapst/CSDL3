// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Audio {
    public static class AudioConvert {
        /// <summary>
        /// Convert some audio data of one format to another format.
        /// </summary>
        /// <param name="srcData">the audio clip to be converted.</param>
        /// <param name="dstSpec">the format details of the output audio.</param>
        /// <returns>New Clip with the desired format, or <see langword="null"/> if <paramref name="srcData"/> has no data. Throws on failure</returns>
        /// <seealso cref="CSDL.Internal.Docs.Audio.ConvertAudioSamples">ConvertAudioSamples</seealso>
        /// <seealso cref="Convert(byte[], AudioSpec, AudioSpec)">Convert(byte[], AudioSpec, AudioSpec)</seealso>
        public static AudioClip? Convert(this AudioClip srcData, AudioSpec dstSpec) {
            if (srcData.Handle.IsNull) return null;

            SDL.ConvertAudioSamples(srcData.Spec, srcData.Handle, (int)srcData.Length, dstSpec, out NativePtr<byte> dstData, out int dstLen).ThrowIfFalse();
            return new AudioClip(dstSpec, dstData, (uint)dstLen);
        }

        /// <summary>
        /// Converts audio data from one format to another.
        /// </summary>
        /// <param name="srcData">The input audio data to be converted.</param>
        /// <param name="srcSpec">The format specifications of the input audio.</param>
        /// <param name="dstSpec">The format specifications of the output audio.</param>
        /// <returns>A byte array containing the converted audio data. Throws on failure</returns>
        /// <seealso cref="CSDL.Internal.Docs.Audio.ConvertAudioSamples">ConvertAudioSamples</seealso>
        /// <seealso cref="Convert(AudioClip, AudioSpec)">Convert(AudioClip, AudioSpec)</seealso>
        public static byte[] Convert(byte[] srcData, AudioSpec srcSpec, AudioSpec dstSpec) {
            if (srcData == null || srcData.Length == 0) {
                return System.Array.Empty<byte>();
            }

            unsafe {
                fixed (byte* srcPtr = srcData) {
                    SDL.ConvertAudioSamples(srcSpec, srcPtr, srcData.Length, dstSpec, out NativePtr<byte> dstData, out int dstLen).ThrowIfFalse();
                    byte[] result = new NativePtr<byte>(dstData).ToManaged(dstLen);
                    Memory.Free(dstData);
                    return result;
                }
            }
        }
    }
}
