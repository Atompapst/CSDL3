// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.File;
namespace CSDL.Audio {
    /// <summary>
    /// Functions for loading audio data into <see cref="AudioClip"/> instances.
    /// </summary>
    /// <seealso cref="LoadWav(string)"/>
    /// <seealso cref="LoadWav(IOStream, bool)"/>
    public static class AudioLoader {
        /// <summary>
        /// Loads the audio data of a WAVE file into memory.
        /// </summary>
        /// <param name="path">The file path of the WAVE file to load.</param>
        /// <returns>A new <see cref="AudioClip"/> containing the loaded audio data. Throws on failure.</returns>
        /// <seealso cref="CSDL.Internal.Docs.Audio.LoadWAV">LoadWAV</seealso>
        /// <seealso cref="LoadWav(IOStream, bool)">LoadWavIO(IOStream, bool)</seealso>
        public static AudioClip LoadWav(string path) {
            SDL.LoadWAV(path, out AudioSpec spec, out NativePtr<byte> buf, out uint len).ThrowIfFalse();
            return new AudioClip(spec, buf, len);
        }

        /// <summary>
        /// Loads the audio data of a WAVE file from an <see cref="IOStream"/> into memory.
        /// </summary>
        /// <param name="stream">The data source for the WAVE data.</param>
        /// <param name="closeAfter">Whether to close <paramref name="stream"/> before returning, including when loading fails.</param>
        /// <returns>A new <see cref="AudioClip"/> containing the loaded audio data. Throws on failure.</returns>
        /// <seealso cref="CSDL.Internal.Docs.Audio.LoadWAV_IO">LoadWAV_IO</seealso>
        /// <seealso cref="LoadWav(string)">LoadWav(string)</seealso>
        public static AudioClip LoadWav(IOStream stream, bool closeAfter = false) {
            CBool ok = SDL.LoadWAV_IO(stream.Handle, closeAfter, out AudioSpec spec, out NativePtr<byte> buf, out uint len);
            if (closeAfter) {
                stream.Invalidate();
            }
            ok.ThrowIfFalse();
            return new AudioClip(spec, buf, len);
        }
    }
}
