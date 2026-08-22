// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Video;

namespace CSDL.TTF {
    public partial struct GPUAtlasDrawSequence {
        /// <summary>
        /// The <see cref="NumVertices"/> vertex positions of this sequence, as a view on the memory
        /// SDL_ttf owns.
        /// </summary>
        /// <remarks>
        /// Only valid as long as the draw data it came from is - see
        /// <see cref="TextObject.GetGPUDrawData"/>.
        /// </remarks>
        public ReadOnlySpan<FPoint> PositionSpan =>
            new NativePtr<FPoint>(Xy).AsReadOnlySpan(NumVertices);

        /// <summary>
        /// The normalized texture coordinate of each vertex, as a view on the memory SDL_ttf owns.
        /// Empty for a solid fill sequence, which carries no texture coordinates.
        /// </summary>
        /// <inheritdoc cref="PositionSpan"/>
        public ReadOnlySpan<FPoint> TexCoordSpan =>
            Uv == 0 ? default : new NativePtr<FPoint>(Uv).AsReadOnlySpan(NumVertices);

        /// <summary>
        /// The <see cref="NumIndices"/> indices into <see cref="PositionSpan"/> and
        /// <see cref="TexCoordSpan"/>, as a view on the memory SDL_ttf owns.
        /// </summary>
        /// <inheritdoc cref="PositionSpan"/>
        public ReadOnlySpan<int> IndexSpan =>
            new NativePtr<int>(Indices).AsReadOnlySpan(NumIndices);
    }
}
