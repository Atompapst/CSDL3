// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Audio {
    /// <summary>
    ///     Represents a block of PCM audio data with an associated audio specification.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         These are intended to represent audio content such as sound effects, short samples, or decoded waveform data.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AudioSpec" />
    /// <seealso cref="AudioLoader" />
    /// <seealso cref="PlaybackDevice" />
    public readonly partial struct AudioClip {

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioClip" /> from managed PCM audio data.
        /// </summary>
        /// <param name="spec">The audio format specification for the clip.</param>
        /// <param name="data">The PCM audio data for the clip.</param>
        public AudioClip(AudioSpec spec, byte[] data)
            : this(spec, data.ToUnmanaged(), (uint)(data?.Length ?? 0)) { }


        /// <summary>
        /// Number of frames in the audio clip.
        /// </summary>
        public int FrameCount {
            get {
                if (Spec.FrameSize <= 0) return 0;
                uint frames = Length / (uint)Spec.FrameSize;
                return frames <= int.MaxValue ? (int)frames : int.MaxValue;
            }
        }

        /// <summary>
        /// Duration of the audio clip in seconds.
        /// </summary>
        public double DurationSeconds => Spec.FrameSize == 0 || Spec.Freq == 0 ? 0 : (double)Length / Spec.FrameSize / Spec.Freq;

        /// <summary>
        ///     Gets the audio data of this clip as a managed byte array.
        /// </summary>
        /// <returns>a copy of the clip's PCM audio data.</returns>
        public byte[] ToArray() {
            return Handle.ToManaged<byte>((int)Length);
        }

        public override string ToString() {
            return $"{Spec}, Bytes: {Length}";
        }
    }
}
