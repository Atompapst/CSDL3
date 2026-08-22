// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using System.Collections.Generic;
using CSDL.Video;

namespace CSDL.Input {
    public static class Mouse {
        private static readonly Dictionary<MouseID, MouseItem> _mice = new Dictionary<MouseID, MouseItem>();
        private static readonly object _transformLock = new object();

        static Mouse() {
            Refresh();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.HasMouse"/>
        public static bool HasAnyMouse => SDL.HasMouse();

        public static IReadOnlyCollection<MouseItem> All => _mice.Values;

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetMouseFocus"/>
        public static nint FocusedWindow => SDL.GetMouseFocus();

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetMice"/>
        private static void Refresh() {
            NativePtr<MouseID> ids = SDL.GetMice(out int count).LogIfInvalid();
            if (ids.IsNull) {
                return;
            }
            try {
                _mice.Clear();
                for (int index = 0; index < count; index++) {
                    MouseID id = ids[index];
                    if (!_mice.ContainsKey(id)) {
                        _mice[id] = new MouseItem(id, 0);
                    }
                }
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        internal static void OnMouseAdded(uint id, ulong timestamp) {
            if (!_mice.ContainsKey(id)) {
                _mice[id] = new MouseItem(id, timestamp);
            }
        }

        internal static void OnMouseRemoved(uint id) {
            _mice.Remove(id);
        }

        internal static void OnMouseUpdated(uint id, ulong timestamp) {
            if (_mice.TryGetValue(id, out MouseItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(uint id) {
            return _mice.ContainsKey(id);
        }

        public static MouseItem? Get(uint id) {
            return _mice.GetValueOrDefault(id);
        }

        public static MouseID[] GetConnectedMice() {
            NativePtr<MouseID> ids = SDL.GetMice(out int count);
            if (ids == null) {
                return Array.Empty<MouseID>();
            }

            try {
                return count > 0 ? ids.ToManaged(count) : Array.Empty<MouseID>();
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetMouseState"/>
        public static (FPoint Position, MouseButtonFlags Buttons) GetState() {
            float x = 0, y = 0;
            MouseButtonFlags buttons = SDL.GetMouseState(NativePtr<float>.FromRef(ref x), NativePtr<float>.FromRef(ref y));
            return (new FPoint(x, y), buttons);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetGlobalMouseState"/>
        public static (FPoint Position, MouseButtonFlags Buttons) GetGlobalState() {
            float x = 0, y = 0;
            MouseButtonFlags buttons = SDL.GetGlobalMouseState(NativePtr<float>.FromRef(ref x), NativePtr<float>.FromRef(ref y));
            return (new FPoint(x, y), buttons);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetRelativeMouseState"/>
        public static (FPoint Position, MouseButtonFlags Buttons) GetRelativeState() {
            float x = 0, y = 0;
            MouseButtonFlags buttons = SDL.GetRelativeMouseState(NativePtr<float>.FromRef(ref x), NativePtr<float>.FromRef(ref y));
            return (new FPoint(x, y), buttons);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.WarpMouseInWindow"/>
        public static void WarpInWindow(Window window, float x, float y) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            SDL.WarpMouseInWindow(window.Handle, x, y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.WarpMouseGlobal"/>
        public static bool WarpGlobal(float x, float y) {
            return SDL.WarpMouseGlobal(x, y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CaptureMouse"/>
        public static bool Capture(bool enabled) {
            return SDL.CaptureMouse(enabled).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CursorVisible"/>
        public static bool CursorVisible => SDL.CursorVisible();

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetWindowRelativeMouseMode"/>
        public static bool GetWindowRelativeMode(Window window) {
            ArgumentNullException.ThrowIfNull(window);
            return SDL.GetWindowRelativeMouseMode(window.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.HideCursor"/>
        public static bool HideCursor() {
            return SDL.HideCursor().LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.ShowCursor"/>
        public static bool ShowCursor() {
            return SDL.ShowCursor().LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.SetCursor"/>
        public static bool SetCursor(Cursor? cursor) {
            return SDL.SetCursor(cursor?.Handle ?? NativePtr<Opaque.SdlCursor>.Zero).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.SetWindowRelativeMouseMode"/>
        public static bool SetWindowRelativeMode(Window window, bool enabled) {
            ArgumentNullException.ThrowIfNull(window);
            return SDL.SetWindowRelativeMouseMode(window.Handle, enabled).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.SetRelativeMouseTransform"/>
        public static bool SetRelativeMouseTransform(MouseMotionTransformCallback? callback, object? userData = null) {
            lock (_transformLock) {
                if (callback is null) {
                    return SDL.SetRelativeMouseTransform(null!, IntPtr.Zero).LogIfFalse();
                }

                SDL_MouseMotionTransformCallbackNative nativeCallback = MouseMotionTransformCallbackWrapper.Create(callback);
                string id = "mouse-transform:" + Guid.NewGuid().ToString("N");
                (IntPtr _, IntPtr userDataPtr) = CallbackRegistry.Register(id, callback, nativeCallback, userData);
                if (SDL.SetRelativeMouseTransform(nativeCallback, userDataPtr)) {
                    // SDL may invoke the former transform on its realtime input thread while this setter runs.
                    // Retain prior registrations for the process lifetime rather than freeing their userdata early.
                    return true;
                }

                CallbackRegistry.Unregister<MouseMotionTransformCallback, SDL_MouseMotionTransformCallbackNative>(id);
                Error.LogError(nameof(SetRelativeMouseTransform));
                return false;
            }
        }
    }
}
