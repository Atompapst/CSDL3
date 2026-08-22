// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.TTF {
    public partial struct GLAtlasDrawSequence {
        /// <summary>
        /// The <see cref="NumVertices"/> vertices of this sequence, as a view on the memory SDL_ttf
        /// owns.
        /// </summary>
        /// <remarks>
        /// Only valid as long as the draw data it came from is - see
        /// <see cref="TextObject.GetGLDrawData"/>.
        /// </remarks>
        public ReadOnlySpan<GLAtlasDrawVertex> VertexSpan =>
            new NativePtr<GLAtlasDrawVertex>(Vertices).AsReadOnlySpan(NumVertices);

        /// <summary>
        /// The <see cref="NumIndices"/> indices into <see cref="VertexSpan"/>, as a view on the
        /// memory SDL_ttf owns.
        /// </summary>
        /// <inheritdoc cref="VertexSpan"/>
        public ReadOnlySpan<int> IndexSpan =>
            new NativePtr<int>(Indices).AsReadOnlySpan(NumIndices);
    }
}
