// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Video {
    public sealed partial class Window {
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.ShowWindow"/>
        public bool Show() {
            bool ok = SDL.ShowWindow(Handle).LogIfFalse();
            if (ok) {
                _flags &= ~WindowFlags.Hidden;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.HideWindow"/>
        public bool Hide() {
            bool ok = SDL.HideWindow(Handle).LogIfFalse();
            if (ok) {
                _flags |= WindowFlags.Hidden;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.MaximizeWindow"/>
        public bool Maximize() {
            bool ok = SDL.MaximizeWindow(Handle).LogIfFalse();
            if (ok) {
                _flags |= WindowFlags.Maximized;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.MinimizeWindow"/>
        public bool Minimize() {
            bool ok = SDL.MinimizeWindow(Handle).LogIfFalse();
            if (ok) {
                _flags |= WindowFlags.Minimized;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.RestoreWindow"/>
        public bool Restore() {
            bool ok = SDL.RestoreWindow(Handle).LogIfFalse();
            if (ok) {
                _flags &= ~(WindowFlags.Maximized | WindowFlags.Minimized);
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.RaiseWindow"/>
        public bool Raise() {
            return SDL.RaiseWindow(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowParent"/>
        public bool SetWindowParent(Window parent) {
            return SDL.SetWindowParent(Handle, parent.Handle.Ptr).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowSurface"/>
        public Surface? GetWindowSurface() {
            // SDL invalidates the previous surface when it creates a replacement after a resize.
            InvalidateWindowSurfaces();
            NativePtr<SurfaceData> s = SDL.GetWindowSurface(Handle);
            if (s.IsNull) {
                return null;
            }

            Surface surface = new Surface(s);
            RegisterWindowSurface(surface);
            return surface;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.UpdateWindowSurface"/>
        public bool UpdateWindowSurface() {
            return SDL.UpdateWindowSurface(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.UpdateWindowSurfaceRects"/>
        public bool UpdateWindowSurfaceRects(Rect[] rects) {
            if (rects == null || rects.Length == 0) return true;
            NativePtr<Rect> raw = rects.ToUnmanaged();
            try {
                return SDL.UpdateWindowSurfaceRects(Handle, raw, rects.Length).LogIfFalse();
            }
            finally {
                raw.Free();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.DestroyWindowSurface"/>
        public bool DestroyWindowSurface() {
            bool destroyed = SDL.DestroyWindowSurface(Handle).LogIfFalse();
            if (destroyed) {
                InvalidateWindowSurfaces();
            }
            return destroyed;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.ShowWindowSystemMenu"/>
        public bool ShowWindowSystemMenu(int x, int y) {
            return SDL.ShowWindowSystemMenu(Handle, x, y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SyncWindow"/>
        public bool SyncWindow() {
            return SDL.SyncWindow(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.WindowHasSurface"/>
        public bool HasSurface => SDL.WindowHasSurface(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowSurfaceVSync"/>
        public int SurfaceVSync {
            get {
                SDL.GetWindowSurfaceVSync(Handle, out int vsync).LogIfFalse();
                return vsync;
            }
            set => SDL.SetWindowSurfaceVSync(Handle, value).LogIfFalse();
        }

        private SDL_HitTestNative? _hitTestNative;
        private string? _hitTestCallbackId;

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowHitTest"/>
        public bool SetHitTest(HitTest? callback, IntPtr callbackData = default) {
            if (callback == null) {
                if (!SDL.SetWindowHitTest(Handle, null!, callbackData).LogIfFalse()) {
                    return false;
                }

                ClearHitTestRegistration();
                return true;
            }

            SDL_HitTestNative native = HitTestWrapper.Create(callback);
            string nextId = $"hittest:{Handle.Ptr}:{Guid.NewGuid():N}";
            CallbackRegistry.Register(nextId, callback, native);
            if (!SDL.SetWindowHitTest(Handle, native, callbackData).LogIfFalse()) {
                CallbackRegistry.Unregister<HitTest, SDL_HitTestNative>(nextId);
                return false;
            }

            ClearHitTestRegistration();
            _hitTestNative = native;
            _hitTestCallbackId = nextId;
            return true;
        }

        private void ClearHitTestRegistration() {
            if (_hitTestCallbackId != null) {
                CallbackRegistry.Unregister<HitTest, SDL_HitTestNative>(_hitTestCallbackId);
            }
            _hitTestCallbackId = null;
            _hitTestNative = null;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowShape"/>
        public bool SetShape(Surface shape) {
            return SDL.SetWindowShape(Handle, shape.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.FlashWindow"/>
        public bool Flash(FlashOperation operation) {
            return SDL.FlashWindow(Handle, operation).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderer"/>
        internal NativePtr<CSDL.Opaque.SdlRenderer> GetRenderer() {
            return SDL.GetRenderer(Handle).LogIfInvalid();
        }
    }
}
