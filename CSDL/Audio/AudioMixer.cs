// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CSDL.Audio {
    /// <summary>
    /// Provides functions for mixing audio buffers and clips.
    /// </summary>
    /// <seealso cref="CSDL.Internal.Docs.Audio.MixAudio">SDL_MixAudio</seealso>
    public static class AudioMixer {
        // =========================
        // byte[]
        // =========================

        /// <summary>
        /// Mixes two audio buffers into a new buffer.
        /// </summary>
        /// <param name="a">The first audio buffer.</param>
        /// <param name="b">The second audio buffer.</param>
        /// <param name="spec">The format specifications of both audio buffers.</param>
        /// <param name="volume">The volume of the shorter buffer, from 0.0 to 1.0.</param>
        /// <returns>A new buffer containing the mixed audio data.</returns>
        /// <exception cref="ArgumentException">The buffers could not be mixed.</exception>
        /// <seealso cref="TryMix(byte[], byte[], AudioSpec, out byte[], float)">TryMix(byte[], byte[], AudioSpec, out byte[], float)</seealso>
        public static byte[] Mix(byte[] a, byte[] b, AudioSpec spec, float volume = 1.0f) {
            if (!TryMix(a, b, spec, out byte[]? result, volume)) {
                throw new ArgumentException("Could not mix buffers.");
            }

            return result;
        }

        /// <summary>
        /// Tries to mix two audio buffers into a new buffer.
        /// </summary>
        /// <param name="a">The first audio buffer.</param>
        /// <param name="b">The second audio buffer.</param>
        /// <param name="spec">The format specifications of both audio buffers.</param>
        /// <param name="result">When this method returns <see langword="true"/>, contains a new buffer with the mixed audio data; otherwise, <see langword="null"/>.</param>
        /// <param name="volume">The volume of the shorter buffer, from 0.0 to 1.0.</param>
        /// <returns><see langword="true"/> if the buffers were mixed successfully; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="Mix(byte[], byte[], AudioSpec, float)">Mix(byte[], byte[], AudioSpec, float)</seealso>
        public static bool TryMix(byte[] a, byte[] b, AudioSpec spec, [NotNullWhen(true)] out byte[]? result, float volume = 1.0f) {
            result = null;

            if (a == null || b == null || spec.FrameSize <= 0) {
                return false;
            }

            byte[] destination;
            byte[] source;

            if (a.Length >= b.Length) {
                destination = a;
                source = b;
            } else {
                destination = b;
                source = a;
            }

            result = new byte[destination.Length];
            Array.Copy(destination, result, destination.Length);

            int sourceFrameCount = source.Length / spec.FrameSize;
            return TryMixInto(result, 0, source, 0, sourceFrameCount, spec, volume);
        }

        /// <summary>
        /// Tries to mix all remaining source frames into a destination buffer.
        /// </summary>
        /// <param name="destination">The buffer that receives the mixed audio data.</param>
        /// <param name="destinationFrameOffset">The frame offset at which to begin writing to <paramref name="destination"/>.</param>
        /// <param name="source">The audio buffer to mix into <paramref name="destination"/>.</param>
        /// <param name="sourceFrameOffset">The frame offset at which to begin reading from <paramref name="source"/>.</param>
        /// <param name="spec">The format specifications of both audio buffers.</param>
        /// <param name="volume">The source volume, from 0.0 to 1.0.</param>
        /// <returns><see langword="true"/> if the buffers were mixed successfully; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="TryMixInto(byte[], int, byte[], int, int, AudioSpec, float)">TryMixInto(byte[], int, byte[], int, int, AudioSpec, float)</seealso>
        public static bool TryMixInto(
            byte[] destination, int destinationFrameOffset,
            byte[] source, int sourceFrameOffset,
            AudioSpec spec,
            float volume = 1.0f) {

            if (source == null || spec.FrameSize <= 0) {
                return false;
            }

            int sourceFrameCount = source.Length / spec.FrameSize;
            int remainingFrames = sourceFrameCount - sourceFrameOffset;
            return TryMixInto(destination, destinationFrameOffset, source, sourceFrameOffset, remainingFrames, spec, volume);
        }

        /// <summary>
        /// Tries to mix a specified number of source frames into a destination buffer.
        /// </summary>
        /// <param name="destination">The buffer that receives the mixed audio data.</param>
        /// <param name="destinationFrameOffset">The frame offset at which to begin writing to <paramref name="destination"/>.</param>
        /// <param name="source">The audio buffer to mix into <paramref name="destination"/>.</param>
        /// <param name="sourceFrameOffset">The frame offset at which to begin reading from <paramref name="source"/>.</param>
        /// <param name="frameCount">The number of frames to mix.</param>
        /// <param name="spec">The format specifications of both audio buffers.</param>
        /// <param name="volume">The source volume, from 0.0 to 1.0.</param>
        /// <returns><see langword="true"/> if the buffers were mixed successfully; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="CSDL.Internal.Docs.Audio.MixAudio">MixAudio</seealso>
        public static bool TryMixInto(
            byte[] destination, int destinationFrameOffset,
            byte[] source, int sourceFrameOffset,
            int frameCount,
            AudioSpec spec,
            float volume = 1.0f) {

            if (destination == null || source == null || spec.FrameSize <= 0 || frameCount < 0) {
                return false;
            }

            if (destinationFrameOffset < 0 || sourceFrameOffset < 0) return false;
            long destinationOffset = (long)destinationFrameOffset * spec.FrameSize;
            long sourceOffset = (long)sourceFrameOffset * spec.FrameSize;
            long length = (long)frameCount * spec.FrameSize;
            if (destinationOffset > int.MaxValue || sourceOffset > int.MaxValue || length > int.MaxValue) return false;

            return TryMixBytes(destination, (int)destinationOffset, source, (int)sourceOffset, (int)length, spec.Format, volume);
        }

        // =========================
        // AudioClip
        // =========================

        /// <summary>
        /// Mixes two audio clips into a new clip.
        /// </summary>
        /// <param name="a">The first audio clip.</param>
        /// <param name="b">The second audio clip.</param>
        /// <param name="volume">The volume of the shorter clip, from 0.0 to 1.0.</param>
        /// <returns>A new <see cref="AudioClip"/> containing the mixed audio data.</returns>
        /// <exception cref="ArgumentException">The clips could not be mixed.</exception>
        /// <seealso cref="TryMix(AudioClip, AudioClip, out AudioClip, float)">TryMix(AudioClip, AudioClip, out AudioClip, float)</seealso>
        public static AudioClip Mix(AudioClip a, AudioClip b, float volume = 1.0f) {
            if (!TryMix(a, b, out AudioClip? result, volume)) {
                throw new ArgumentException("Could not mix clips.");
            }

            return result;
        }

        /// <summary>
        /// Tries to mix two audio clips into a new clip.
        /// </summary>
        /// <param name="a">The first audio clip.</param>
        /// <param name="b">The second audio clip.</param>
        /// <param name="result">When this method returns <see langword="true"/>, contains a new clip with the mixed audio data; otherwise, <see langword="null"/>.</param>
        /// <param name="volume">The volume of the shorter clip, from 0.0 to 1.0.</param>
        /// <returns><see langword="true"/> if the clips were mixed successfully; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="Mix(AudioClip, AudioClip, float)">Mix(AudioClip, AudioClip, float)</seealso>
        public static bool TryMix(AudioClip a, AudioClip b, [NotNullWhen(true)] out AudioClip? result, float volume = 1.0f) {
            result = null;

            if (a == null || b == null) {
                return false;
            }

            if (!a.Spec.Equals(b.Spec) || a.Spec.FrameSize <= 0) {
                return false;
            }

            AudioClip destination;
            AudioClip source;

            if (a.Length >= b.Length) {
                destination = a;
                source = b;
            } else {
                destination = b;
                source = a;
            }

            byte[] mixed = destination.ToArray();
            byte[] sourceData = source.ToArray();

            int sourceFrameCount = (int)source.Length / destination.Spec.FrameSize;
            if (!TryMixInto(mixed, 0, sourceData, 0, sourceFrameCount, destination.Spec, volume)) {
                return false;
            }

            result = new AudioClip(destination.Spec, mixed);
            return true;
        }

        /// <summary>
        /// Tries to mix all remaining source frames into a destination clip.
        /// </summary>
        /// <param name="destination">The clip that receives the mixed audio data.</param>
        /// <param name="destinationFrameOffset">The frame offset at which to begin writing to <paramref name="destination"/>.</param>
        /// <param name="source">The audio clip to mix into <paramref name="destination"/>.</param>
        /// <param name="sourceFrameOffset">The frame offset at which to begin reading from <paramref name="source"/>.</param>
        /// <param name="volume">The source volume, from 0.0 to 1.0.</param>
        /// <returns><see langword="true"/> if the clips were mixed successfully; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="TryMixInto(AudioClip, int, AudioClip, int, int, float)">TryMixInto(AudioClip, int, AudioClip, int, int, float)</seealso>
        public static bool TryMixInto(
            AudioClip destination, int destinationFrameOffset,
            AudioClip source, int sourceFrameOffset,
            float volume = 1.0f) {

            if (destination == null || source == null || destination.Spec.FrameSize <= 0) {
                return false;
            }

            int sourceFrameCount = (int)source.Length / destination.Spec.FrameSize;
            int remainingFrames = sourceFrameCount - sourceFrameOffset;

            return TryMixInto(destination, destinationFrameOffset, source, sourceFrameOffset, remainingFrames, volume);
        }

        /// <summary>
        /// Tries to mix a specified number of source frames into a destination clip.
        /// </summary>
        /// <param name="destination">The clip that receives the mixed audio data.</param>
        /// <param name="destinationFrameOffset">The frame offset at which to begin writing to <paramref name="destination"/>.</param>
        /// <param name="source">The audio clip to mix into <paramref name="destination"/>.</param>
        /// <param name="sourceFrameOffset">The frame offset at which to begin reading from <paramref name="source"/>.</param>
        /// <param name="frameCount">The number of frames to mix.</param>
        /// <param name="volume">The source volume, from 0.0 to 1.0.</param>
        /// <returns><see langword="true"/> if the clips were mixed successfully; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="CSDL.Internal.Docs.Audio.MixAudio">MixAudio</seealso>
        public static bool TryMixInto(
            AudioClip destination, int destinationFrameOffset,
            AudioClip source, int sourceFrameOffset,
            int frameCount,
            float volume = 1.0f) {

            if (destination == null || source == null) {
                return false;
            }

            if (!destination.Spec.Equals(source.Spec) || destination.Spec.FrameSize <= 0 || frameCount < 0) {
                return false;
            }

            if (destinationFrameOffset < 0 || sourceFrameOffset < 0) return false;
            long destinationOffset = (long)destinationFrameOffset * destination.Spec.FrameSize;
            long sourceOffset = (long)sourceFrameOffset * destination.Spec.FrameSize;
            long length = (long)frameCount * destination.Spec.FrameSize;
            if (destinationOffset > int.MaxValue || sourceOffset > int.MaxValue || length > int.MaxValue) return false;

            return TryMixClipBytes(destination, (int)destinationOffset, source, (int)sourceOffset, (int)length, volume);
        }

        // =========================
        // private core
        // =========================

        private static bool TryMixClipBytes(
            AudioClip destination, int destinationOffset,
            AudioClip source, int sourceOffset,
            int length,
            float volume) {

            if (destination == null || source == null) {
                return false;
            }

            if (!destination.Spec.Equals(source.Spec)) {
                return false;
            }

            if (destinationOffset < 0 || sourceOffset < 0 || length < 0) {
                return false;
            }

            if ((long)destinationOffset + length > destination.Length) {
                return false;
            }

            if ((long)sourceOffset + length > source.Length) {
                return false;
            }

            return SDL.MixAudio(
                destination.Handle.Offset(destinationOffset),
                source.Handle.Offset(sourceOffset),
                destination.Spec.Format,
                (uint)length,
                volume).LogIfFalse();
        }

        private static bool TryMixBytes(
            byte[] destination, int destinationOffset,
            byte[] source, int sourceOffset,
            int length,
            AudioFormat format,
            float volume) {

            if (destination == null || source == null) {
                return false;
            }

            if (destinationOffset < 0 || sourceOffset < 0 || length < 0) {
                return false;
            }

            if ((long)destinationOffset + length > destination.Length) {
                return false;
            }

            if ((long)sourceOffset + length > source.Length) {
                return false;
            }

            unsafe {
                fixed (byte* dstPtr = destination)
                fixed (byte* srcPtr = source) {
                    return SDL.MixAudio(
                        (IntPtr)(dstPtr + destinationOffset),
                        (IntPtr)(srcPtr + sourceOffset),
                        format,
                        (uint)length,
                        volume).LogIfFalse();
                }
            }
        }
    }
}
