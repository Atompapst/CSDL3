// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Video {
    public sealed partial class Window {

        /// <summary>
        /// window usable with OpenGL context.
        /// </summary>
        public bool OpenGL => _flags.HasFlag(WindowFlags.Opengl);

        /// <summary>
        /// window usable for Vulkan surface
        /// </summary>
        public bool Vulkan => _flags.HasFlag(WindowFlags.Vulkan);

        /// <summary>
        /// window usable for Metal view
        /// </summary>
        public bool Metal => _flags.HasFlag(WindowFlags.Metal);

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowFullscreen"/>
        public bool Fullscreen {
            get => _flags.HasFlag(WindowFlags.Fullscreen);
            set {
                if (SetWindowFullscreen(value)) {
                    if (value) _flags |= WindowFlags.Fullscreen;
                    else _flags &= ~WindowFlags.Fullscreen;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowAlwaysOnTop"/>
        public bool AlwaysOnTop {
            get => _flags.HasFlag(WindowFlags.AlwaysOnTop);
            set {
                if (SetWindowAlwaysOnTop(value)) {
                    if (value) _flags |= WindowFlags.AlwaysOnTop;
                    else _flags &= ~WindowFlags.AlwaysOnTop;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowBordered"/>
        public bool Bordered {
            get => !_flags.HasFlag(WindowFlags.Borderless);
            set {
                if (SetWindowBordered(value)) {
                    if (value) _flags &= ~WindowFlags.Borderless;
                    else _flags |= WindowFlags.Borderless;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowFocusable"/>
        public bool Focusable {
            get => !_flags.HasFlag(WindowFlags.NotFocusable);
            set {
                if (SetWindowFocusable(value)) {
                    if (value) _flags &= ~WindowFlags.NotFocusable;
                    else _flags |= WindowFlags.NotFocusable;
                }
            }
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowResizable"/>
        public bool Resizable {
            get => _flags.HasFlag(WindowFlags.Resizable);
            set {
                if (SetWindowResizable(value)) {
                    if (value) _flags |= WindowFlags.Resizable;
                    else _flags &= ~WindowFlags.Resizable;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowModal"/>
        public bool Modal {
            get => _flags.HasFlag(WindowFlags.Modal);
            set {
                if (SetWindowModal(value)) {
                    if (value) _flags |= WindowFlags.Modal;
                    else _flags &= ~WindowFlags.Modal;
                }
            }
        }

        /// <summary>
        /// window is in fill-document mode (Emscripten only), since SDL 3.4.0
        /// </summary>
        public bool FillDocument {
            get => _flags.HasFlag(WindowFlags.FillDocument);
            set {
                if (SetWindowFillDocument(value)) {
                    if (value) _flags |= WindowFlags.FillDocument;
                    else _flags &= ~WindowFlags.FillDocument;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowKeyboardGrab"/>
        public bool KeyboardGrab {
            get => GetWindowKeyboardGrab();
            set {
                if (SetWindowKeyboardGrab(value)) {
                    if (value) _flags |= WindowFlags.KeyboardGrabbed;
                    else _flags &= ~WindowFlags.KeyboardGrabbed;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMouseGrab"/>
        public bool MouseGrab {
            get => GetWindowMouseGrab();
            set {
                if (SetWindowMouseGrab(value)) {
                    if (value) _flags |= WindowFlags.MouseGrabbed;
                    else _flags &= ~WindowFlags.MouseGrabbed;
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowAlwaysOnTop"/>
        private bool SetWindowAlwaysOnTop(bool onTop) {
            return SDL.SetWindowAlwaysOnTop(Handle, onTop).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowBordered"/>
        private bool SetWindowBordered(bool bordered) {
            return SDL.SetWindowBordered(Handle, bordered).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowFocusable"/>
        private bool SetWindowFocusable(bool focusable) {
            return SDL.SetWindowFocusable(Handle, focusable).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowResizable"/>
        private bool SetWindowResizable(bool resizable) {
            return SDL.SetWindowResizable(Handle, resizable).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowModal"/>
        private bool SetWindowModal(bool modal) {
            return SDL.SetWindowModal(Handle, modal).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowFullscreen"/>
        private bool SetWindowFullscreen(bool fullscreen) {
            bool changed = SDL.SetWindowFullscreen(Handle, fullscreen).LogIfFalse();
            if (changed) {
                InvalidateWindowSurfaces();
            }
            return changed;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowFillDocument"/>
        private bool SetWindowFillDocument(bool fill) {
            return SDL.SetWindowFillDocument(Handle, fill).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowKeyboardGrab"/>
        private bool GetWindowKeyboardGrab() {
            return SDL.GetWindowKeyboardGrab(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowKeyboardGrab"/>
        private bool SetWindowKeyboardGrab(bool grabbed) {
            return SDL.SetWindowKeyboardGrab(Handle, grabbed).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowMouseGrab"/>
        private bool GetWindowMouseGrab() {
            return SDL.GetWindowMouseGrab(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMouseGrab"/>
        private bool SetWindowMouseGrab(bool grabbed) {
            return SDL.SetWindowMouseGrab(Handle, grabbed).LogIfFalse();
        }

    }
}
