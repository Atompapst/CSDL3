// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;

namespace CSDL.Mixer {
    /// <summary>
    /// The knobs for <see cref="Audio.Load(AudioLoadProperties)"/> - the load path that exposes
    /// everything the simpler <see cref="Audio"/> constructors don't. <see cref="Source"/> is the only
    /// required one. This group is created and owned by the caller, so dispose it when done.
    /// </summary>
    /// <remarks>
    /// Individual decoders may accept further custom properties (where to find SoundFonts for MIDI
    /// playback, for instance); set those by name through <see cref="PropertyGroup.String"/> and friends.
    /// </remarks>
    public class AudioLoadProperties : PropertyGroup {
        private File.IOStream? _source;
        private bool _closeAfter;
        public AudioLoadProperties() { }

        /// <inheritdoc cref="Props.AudioLoadIostreamPointer"/>
        public PointerProperty IOStream => PropPointer(Props.AudioLoadIostreamPointer);

        /// <inheritdoc cref="Props.AudioLoadCloseioBoolean"/>
        public BooleanProperty CloseIO => PropBool(Props.AudioLoadCloseioBoolean);

        /// <inheritdoc cref="Props.AudioLoadPredecodeBoolean"/>
        public BooleanProperty Predecode => PropBool(Props.AudioLoadPredecodeBoolean);

        /// <inheritdoc cref="Props.AudioLoadPreferredMixerPointer"/>
        public PointerProperty PreferredMixer => PropPointer(Props.AudioLoadPreferredMixerPointer);

        /// <inheritdoc cref="Props.AudioLoadSkipMetadataTagsBoolean"/>
        public BooleanProperty SkipMetadataTags => PropBool(Props.AudioLoadSkipMetadataTagsBoolean);

        /// <inheritdoc cref="Props.AudioLoadIgnoreLoopsBoolean"/>
        public BooleanProperty IgnoreLoops => PropBool(Props.AudioLoadIgnoreLoopsBoolean);

        /// <inheritdoc cref="Props.AudioDecoderString"/>
        public StringProperty Decoder => PropString(Props.AudioDecoderString);

        /// <summary>
        /// Sets <see cref="IOStream"/> (required) - and <see cref="CloseIO"/>, if SDL_mixer should
        /// close <paramref name="source"/> once it is done loading.
        /// </summary>
        public AudioLoadProperties Source(File.IOStream source, bool closeAfter = false) {
            System.ArgumentNullException.ThrowIfNull(source);
            IOStream.Set(source.NativePointer);
            CloseIO.Set(closeAfter);
            _source = source;
            _closeAfter = closeAfter;
            return this;
        }

        internal void CompleteLoad() {
            if (_closeAfter) {
                _source?.Invalidate();
            }
            _source = null;
        }
    }
}
