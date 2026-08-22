// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Video {
    public sealed class MetalView : NativeHandle<CSDL.Opaque.SdlMetalView> {
        static MetalView() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Metal.Metal_CreateView"/>
        public MetalView(Window window) {
            ArgumentNullException.ThrowIfNull(window);
            Handle = SDL.Metal_CreateView(window.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Metal.Metal_GetLayer"/>
        public IntPtr Layer => SDL.Metal_GetLayer(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Metal.Metal_DestroyView"/>
        protected override void DisposeResource() {
            SDL.Metal_DestroyView(Handle);
        }
    }
}
