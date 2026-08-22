// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.TTF {
    /// <summary>
    /// The properties SDL_ttf keeps for a <see cref="TextObject"/>.
    /// </summary>
    /// <remarks>
    /// SDL_ttf defines no properties of its own here - the group exists so applications can attach
    /// their own data to a text object (see <see cref="PropertyGroup.String"/> and friends). It is
    /// created and owned by SDL_ttf and lives as long as the text object does, so it must not be
    /// disposed and the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="TextObject.Properties"/>
    public sealed class TextProperties : PropertyGroup {
        internal TextProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }
    }
}
