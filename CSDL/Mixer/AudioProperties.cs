// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.Mixer {
    /// <summary>
    /// The metadata SDL_mixer read out of an <see cref="Audio"/> or <see cref="AudioDecoder"/>, plus
    /// room for app-specific data (see <see cref="PropertyGroup.String"/> and friends).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This group is created and owned by SDL_mixer and lives as long as the audio does. It must not
    /// be disposed, so the finalizer that would otherwise destroy it is suppressed.
    /// </para>
    /// <para>
    /// The metadata is whatever SDL_mixer found in things like ID3 tags: it is often unformatted,
    /// frequently missing, and can be outright wrong if the source data is untrustworthy.
    /// </para>
    /// </remarks>
    /// <seealso cref="Audio.Properties"/>
    /// <seealso cref="AudioDecoder.Properties"/>
    public sealed class AudioProperties : PropertyGroup {
        internal AudioProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Props.MetadataTitleString"/>
        public StringProperty Title => PropString(Props.MetadataTitleString);

        /// <inheritdoc cref="Props.MetadataArtistString"/>
        public StringProperty Artist => PropString(Props.MetadataArtistString);

        /// <inheritdoc cref="Props.MetadataAlbumString"/>
        public StringProperty Album => PropString(Props.MetadataAlbumString);

        /// <inheritdoc cref="Props.MetadataCopyrightString"/>
        public StringProperty Copyright => PropString(Props.MetadataCopyrightString);

        /// <inheritdoc cref="Props.MetadataTrackNumber"/>
        public NumberProperty TrackNumber => PropNumber(Props.MetadataTrackNumber);

        /// <inheritdoc cref="Props.MetadataTotalTracksNumber"/>
        public NumberProperty TotalTracks => PropNumber(Props.MetadataTotalTracksNumber);

        /// <inheritdoc cref="Props.MetadataYearNumber"/>
        public NumberProperty Year => PropNumber(Props.MetadataYearNumber);

        /// <inheritdoc cref="Props.MetadataDurationFramesNumber"/>
        public NumberProperty DurationFrames => PropNumber(Props.MetadataDurationFramesNumber);

        /// <inheritdoc cref="Props.MetadataDurationInfiniteBoolean"/>
        public BooleanProperty DurationInfinite => PropBool(Props.MetadataDurationInfiniteBoolean);

        /// <inheritdoc cref="Props.AudioDecoderString"/>
        public StringProperty Decoder => PropString(Props.AudioDecoderString);
    }
}
