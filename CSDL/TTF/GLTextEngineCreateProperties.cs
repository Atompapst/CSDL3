// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;

namespace CSDL.TTF {
    /// <summary>
    /// The property set an application fills in and hands to
    /// <see cref="GLTextEngine(GLTextEngineCreateProperties)"/>.
    /// </summary>
    public sealed class GLTextEngineCreateProperties : PropertyGroup {
        /// <inheritdoc cref="CSDL.TTF.Props.GLTextEngineAtlasTextureSizeNumber"/>
        public NumberProperty AtlasTextureSize => PropNumber(Props.GLTextEngineAtlasTextureSizeNumber);
    }
}
