// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.TTF {
    /// <summary>
    /// The properties SDL_ttf keeps for an existing <see cref="Font"/>, plus room for app-specific
    /// data (see <see cref="PropertyGroup.String"/> and friends).
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="FontCreateProperties"/>, which the application fills in and hands to
    /// <see cref="Font(FontCreateProperties)"/>, this group is created and owned by SDL_ttf and lives
    /// as long as the font does. It must not be disposed, so the finalizer that would otherwise
    /// destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Font.Properties"/>
    public sealed class FontProperties : PropertyGroup {
        internal FontProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.TTF.Props.FontOutlineLineCapNumber"/>
        public NumberProperty OutlineLineCap => PropNumber(Props.FontOutlineLineCapNumber);

        /// <inheritdoc cref="CSDL.TTF.Props.FontOutlineLineJoinNumber"/>
        public NumberProperty OutlineLineJoin => PropNumber(Props.FontOutlineLineJoinNumber);

        /// <inheritdoc cref="CSDL.TTF.Props.FontOutlineMiterLimitNumber"/>
        public NumberProperty OutlineMiterLimit => PropNumber(Props.FontOutlineMiterLimitNumber);
    }
}
