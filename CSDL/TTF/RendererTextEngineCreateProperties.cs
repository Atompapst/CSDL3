// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
using CSDL.Video;

namespace CSDL.TTF {
    /// <summary>
    /// The property set an application fills in and hands to
    /// <see cref="RendererTextEngine(RendererTextEngineCreateProperties)"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Renderer"/> is required; the constructor taking a
    /// <see cref="CSDL.Video.Renderer"/> fills it in.
    /// </remarks>
    public sealed class RendererTextEngineCreateProperties : PropertyGroup {
        public RendererTextEngineCreateProperties() { }

        /// <summary>
        /// Creates a property set with <see cref="Renderer"/> pointing at <paramref name="renderer"/>.
        /// </summary>
        public RendererTextEngineCreateProperties(Renderer renderer) {
            Renderer.Set(renderer.NativePointer);
        }

        /// <inheritdoc cref="CSDL.TTF.Props.RendererTextEngineRendererPointer"/>
        public PointerProperty Renderer => PropPointer(Props.RendererTextEngineRendererPointer);

        /// <inheritdoc cref="CSDL.TTF.Props.RendererTextEngineAtlasTextureSizeNumber"/>
        public NumberProperty AtlasTextureSize => PropNumber(Props.RendererTextEngineAtlasTextureSizeNumber);
    }
}
