// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Extensions;
using CSDL.GPU;

namespace CSDL.Video {
    public partial class Renderer : NativeHandle<CSDL.Opaque.SdlRenderer> {
        private readonly List<WeakReference<Internal.InvalidationRegistration>> _children = new List<WeakReference<Internal.InvalidationRegistration>>();

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateRenderer"/>
        public Renderer(Window window, string? name = null) {
            CreateRenderer(window, name);
            window.RegisterChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateSoftwareRenderer"/>
        public Renderer(Surface surface) {
            CreateSoftwareRenderer(surface);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateRendererWithProperties"/>
        public Renderer(RendererCreateProperties properties) {
            CreateRenderer(properties);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateGPURenderer"/>
        public Renderer(GPUDevice? device = null, Window? window = null) {
            CreateGPU(device, window);
        }

        internal Renderer(NativePtr<CSDL.Opaque.SdlRenderer> renderer, bool ownsHandle = false) : base(renderer, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderer"/>
        public static Renderer GetRenderer(Window window) {
            NativePtr<CSDL.Opaque.SdlRenderer> r = window.GetRenderer();
            return new Renderer(r);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRendererFromTexture"/>
        public static Renderer GetRenderer(Texture gpuTexture) {
            NativePtr<CSDL.Opaque.SdlRenderer> r = SDL.GetRendererFromTexture(gpuTexture.Handle).LogIfInvalid();
            return new Renderer(r);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateRenderer"/>
        private void CreateRenderer(Window window, string? name) {
            Handle = SDL.CreateRenderer(window.Handle, name).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateRendererWithProperties"/>
        private void CreateRenderer(RendererCreateProperties properties) {
            Handle = SDL.CreateRendererWithProperties(properties.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateSoftwareRenderer"/>
        private void CreateSoftwareRenderer(Surface surface) {
            Handle = SDL.CreateSoftwareRenderer(surface.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.DestroyRenderer"/>
        protected override void DisposeResource() {
            SDL.DestroyRenderer(Handle);
            InvalidateChildren();
        }

        protected override void InvalidateResource() {
            InvalidateChildren();
        }

        internal void RegisterChild(Internal.InvalidationRegistration child) {
            _children.Add(new WeakReference<Internal.InvalidationRegistration>(child));
        }

        private void InvalidateChildren() {
            foreach (WeakReference<Internal.InvalidationRegistration> child in _children) {
                if (child.TryGetTarget(out Internal.InvalidationRegistration? target)) {
                    target.Invalidate();
                }
            }
            _children.Clear();
        }
    }
}
