// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.Mixer {
    /// <summary>
    /// What SDL_mixer reports about a <see cref="Mixer"/>, plus room for app-specific data (see
    /// <see cref="PropertyGroup.String"/> and friends).
    /// </summary>
    /// <remarks>
    /// This group is created and owned by SDL_mixer and lives as long as the mixer does. It must not
    /// be disposed, so the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Mixer.Properties"/>
    public sealed class MixerProperties : PropertyGroup {
        internal MixerProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Props.MixerDeviceNumber"/>
        public NumberProperty DeviceID => PropNumber(Props.MixerDeviceNumber);
    }
}
