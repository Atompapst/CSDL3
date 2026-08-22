// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Extensions;

namespace CSDL.Video {
    public sealed partial class Window : NativeHandle<CSDL.Opaque.SdlWindow> {
        private WindowFlags _flags;
        private readonly List<WeakReference<Internal.InvalidationRegistration>> _children = new List<WeakReference<Internal.InvalidationRegistration>>();
        private readonly List<WeakReference<Surface>> _windowSurfaces = new List<WeakReference<Surface>>();

        static Window() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreateWindow"/>
        public Window(string title, int width, int height, WindowFlags flags = 0) {
            CreateWindow(title, width, height, flags);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreateWindow"/>
        public Window(string title, Resolution resolution, WindowFlags flags = 0) {
            ResolutionHelper.ResolutionInfo info = ResolutionHelper.GetResolutionInfo(resolution);
            CreateWindow(title, info.Width, info.Height, flags);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreateWindowWithProperties"/>
        public Window(WindowCreateProperties properties) {
            CreateWindowWithProperties(properties);
        }

        internal Window(NativePtr<CSDL.Opaque.SdlWindow> window, bool ownsHandle = false) : base(window, ownsHandle) {
            if (IsValid) {
                _flags = SDL.GetWindowFlags(Handle);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreatePopupWindow"/>
        public Window(Window parent, int offsetX, int offsetY, int width, int height, WindowFlags flags = 0) {
            CreatePopupWindow(parent, offsetX, offsetY, width, height, flags);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowFromID"/>
        internal Window(uint id) : base(SDL.GetWindowFromID(id).ThrowIfInvalid(), false) {
            _flags = SDL.GetWindowFlags(Handle);
        }

        /// <summary>
        /// Resolves a <see cref="WindowID"/> - e.g. one read off an event such as
        /// <c>WindowEvent.WindowID</c> - back into its <see cref="Window"/>, or <see langword="null"/>
        /// if no window with that ID currently exists (it may already have been destroyed).
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowFromID"/>
        public static Window? FromID(WindowID id) {
            NativePtr<CSDL.Opaque.SdlWindow> ptr = SDL.GetWindowFromID(id).LogIfInvalid();
            return ptr.IsNull ? null : new Window(ptr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowFlags"/>
        public WindowFlags Flags => GetWindowFlags();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowFlags"/>
        private WindowFlags GetWindowFlags() {
            _flags = SDL.GetWindowFlags(Handle);
            return _flags;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetGrabbedWindow"/>
        public static Window? GrabbedWindow {
            get {
                IntPtr ptr = SDL.GetGrabbedWindow();
                return ptr == IntPtr.Zero ? null : new Window(ptr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindows"/>
        public static Window[] Windows => GetWindows();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreateWindowAndRenderer"/>
        public static void CreateWindowAndRenderer(string title, int width, int height, WindowFlags flags, out Window window, out Renderer renderer) {
            SDL.CreateWindowAndRenderer(title, width, height, flags, out NativePtr<CSDL.Opaque.SdlWindow> windowPtr, out NativePtr<CSDL.Opaque.SdlRenderer> rendererPtr).ThrowIfFalse();
            window = new Window(windowPtr, true);
            renderer = new Renderer(rendererPtr, true);
            window.RegisterChild(renderer.Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreateWindow"/>
        private void CreateWindow(string title, int width, int height, WindowFlags flags) {
            Handle = SDL.CreateWindow(title, width, height, flags).ThrowIfInvalid();
            _flags = SDL.GetWindowFlags(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreatePopupWindow"/>
        private void CreatePopupWindow(Window parent, int offsetX, int offsetY, int width, int height, WindowFlags flags = 0) {
            Handle = SDL.CreatePopupWindow(parent.Handle, offsetX, offsetY, width, height, flags).ThrowIfInvalid();
            _flags = SDL.GetWindowFlags(Handle);
            parent.RegisterChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.CreateWindowWithProperties"/>
        private void CreateWindowWithProperties(WindowCreateProperties properties) {
            Handle = SDL.CreateWindowWithProperties(properties.Handle).ThrowIfInvalid();
            _flags = SDL.GetWindowFlags(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowICCProfile"/>
        public IntPtr GetWindowICCProfile(out nuint size) {
            // `length` may be a caller-supplied field, whose address is not safe to hand to native
            // code (a GC compaction mid-call could relocate it). Use a local instead and copy out.
            nuint localLength = 0;
            IntPtr result = SDL.GetWindowICCProfile(Handle, NativePtr<nuint>.FromRef(ref localLength)).LogIfInvalid();
            size = localLength;
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindows"/>
        private static Window[] GetWindows() {
            IntPtr ptr = SDL.GetWindows(out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetWindows));
                return Array.Empty<Window>();
            }

            NativePtr<NativePtr<CSDL.Opaque.SdlWindow>> windows = ptr;
            Window[] result = new Window[count];
            for (int i = 0; i < count; i++) {
                result[i] = new Window(windows[i]);
            }
            Memory.Free(ptr);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.DestroyWindow"/>
        protected override void DisposeResource() {
            ClearHitTestRegistration();
            SDL.DestroyWindow(Handle);
            InvalidateWindowSurfaces();
            InvalidateChildren();
            _flags = 0;
        }

        protected override void InvalidateResource() {
            ClearHitTestRegistration();
            InvalidateWindowSurfaces();
            InvalidateChildren();
            _flags = 0;
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

        private void RegisterWindowSurface(Surface surface) {
            _windowSurfaces.Add(new WeakReference<Surface>(surface));
        }

        private void InvalidateWindowSurfaces() {
            foreach (WeakReference<Surface> surface in _windowSurfaces) {
                if (surface.TryGetTarget(out Surface? target)) {
                    target.Invalidate();
                }
            }
            _windowSurfaces.Clear();
        }
    }
}
