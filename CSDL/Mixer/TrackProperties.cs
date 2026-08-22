// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.Mixer {
    /// <summary>
    /// A <see cref="Track"/>'s property set. SDL_mixer assigns no properties of its own here, so this
    /// is purely a convenient place to hang app-specific data off a track - reach for
    /// <see cref="PropertyGroup.String"/> and friends to get at it by name.
    /// </summary>
    /// <remarks>
    /// This group is created and owned by SDL_mixer and lives as long as the track does. It must not
    /// be disposed, so the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Track.Properties"/>
    public sealed class TrackProperties : PropertyGroup {
        internal TrackProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }
    }
}
