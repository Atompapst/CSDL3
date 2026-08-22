// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSDL.TTF {
    public sealed partial class TextObject {
        /// <summary>
        /// Gets the geometry needed to draw this text object with OpenGL, one entry per atlas
        /// texture and image type.
        /// </summary>
        /// <remarks>
        /// The pointers inside the returned sequences - and the spans built from them, such as
        /// <see cref="GLAtlasDrawSequence.VertexSpan"/> - point into memory SDL_ttf owns and are
        /// only valid until the text object changes or the draw data is queried again. Copy what you
        /// need if you want to hold on to it. An empty array means the text object is empty; on
        /// failure the SDL error is logged.
        /// </remarks>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGLTextDrawData"/>
        public GLAtlasDrawSequence[] GetGLDrawData() {
            List<GLAtlasDrawSequence> sequences = new List<GLAtlasDrawSequence>();
            NativePtr<GLAtlasDrawSequence> node = SDL.GetGLTextDrawData(ref Ref);
            while (node.IsNull) {
                ref GLDrawSequenceNode current = ref new NativePtr<GLDrawSequenceNode>(node).AsRef();
                sequences.Add(current.Sequence);
                node = current.Next;
            }

            return sequences.ToArray();
        }

        /// <summary>
        /// Gets the geometry needed to draw this text object with the SDL GPU API, one entry per
        /// atlas texture and image type.
        /// </summary>
        /// <remarks>
        /// The pointers inside the returned sequences - and the spans built from them, such as
        /// <see cref="GPUAtlasDrawSequence.PositionSpan"/> - point into memory SDL_ttf owns and are
        /// only valid until the text object changes or the draw data is queried again. Copy what you
        /// need if you want to hold on to it. An empty array means the text object is empty; on
        /// failure the SDL error is logged.
        /// </remarks>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGPUTextDrawData"/>
        public GPUAtlasDrawSequence[] GetGPUDrawData() {
            List<GPUAtlasDrawSequence> sequences = new List<GPUAtlasDrawSequence>();
            IntPtr node = SDL.GetGPUTextDrawData(ref Ref);
            while (node != IntPtr.Zero) {
                ref GPUDrawSequenceNode current = ref new NativePtr<GPUDrawSequenceNode>(node).AsRef();
                sequences.Add(current.Sequence);
                node = current.Next;
            }

            return sequences.ToArray();
        }

        // Both draw sequences are singly linked lists in C: the last member of the native struct is
        // a pointer to the next sequence. CSDL.TTF.GLAtlasDrawSequence and
        // CSDL.TTF.GPUAtlasDrawSequence stop before that pointer, so these nodes bolt it back on to
        // walk the list. Both wrapped structs are sequential and pointer-aligned, so the trailing
        // pointer lands on exactly the offset the C compiler gave 'next'.
        [StructLayout(LayoutKind.Sequential)]
        private struct GLDrawSequenceNode {
            public GLAtlasDrawSequence Sequence;
            public IntPtr Next;
        }

        /// <inheritdoc cref="GLDrawSequenceNode"/>
        [StructLayout(LayoutKind.Sequential)]
        private struct GPUDrawSequenceNode {
            public GPUAtlasDrawSequence Sequence;
            public IntPtr Next;
        }
    }
}
