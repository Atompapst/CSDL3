// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Audio {
    public partial struct AudioSpec {
        public AudioFormatInfo FormatInfo => new AudioFormatInfo(Format);
        /// <inheritdoc cref="Macros.AudioFrameSize"/>
        public int FrameSize {
            get {
                uint size = Macros.AudioFrameSize(this);
                return size <= int.MaxValue ? (int)size : 0;
            }
        }

        public AudioSpec(AudioSpec spec) {
            this = spec;
        }

        public AudioSpec(AudioFormat format, int frequency, int channels) {
            Format = format;
            Freq = frequency;
            Channels = channels;
        }

        public override string ToString() {
            return $"AudioSpec(Freq: {Freq}, Format: {FormatInfo.Format}, Channels: {Channels})";
        }

        public bool Equals(AudioSpec other) {
            return Freq == other.Freq && Format == other.Format && Channels == other.Channels;
        }

        public override bool Equals(object? obj) {
            return obj is AudioSpec spec && Equals(spec);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(Freq, Format, Channels);
        }

        public static bool operator ==(AudioSpec left, AudioSpec right) {
            return left.Equals(right);
        }

        public static bool operator !=(AudioSpec left, AudioSpec right) {
            return !left.Equals(right);
        }

    }
}
