// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface IWindowEvents {
        WindowEvent? LastEvent { get; }
        event Action<WindowEvent>? Any;

        bool HasEvent();
        bool Shown();
        bool Hidden();
        bool Exposed();
        bool Moved();
        bool Resized();
        bool PixelSizeChanged();
        bool MetalViewResized();
        bool Minimized();
        bool Maximized();
        bool Restored();
        bool MouseEnter();
        bool MouseLeave();
        bool FocusGained();
        bool FocusLost();
        bool CloseRequested();
        bool HitTest();
        bool ICCProfileChanged();
        bool DisplayChanged();
        bool DisplayScaleChanged();
        bool SafeAreaChanged();
        bool Occluded();
        bool EnteredFullscreen();
        bool LeftFullscreen();
        bool Destroyed();
        bool HDRStateChanged();
        bool LastWas(EventType type);
        bool ForWindow(uint windowId);
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Window : EventHandlerBase, Interfaces.IWindowEvents {
        private bool _shown;
        private bool _hidden;
        private bool _exposed;
        private bool _moved;
        private bool _resized;
        private bool _pixelSizeChanged;
        private bool _metalViewResized;
        private bool _minimized;
        private bool _maximized;
        private bool _restored;
        private bool _mouseEnter;
        private bool _mouseLeave;
        private bool _focusGained;
        private bool _focusLost;
        private bool _closeRequested;
        private bool _hitTest;
        private bool _iccProfileChanged;
        private bool _displayChanged;
        private bool _displayScaleChanged;
        private bool _safeAreaChanged;
        private bool _occluded;
        private bool _enterFullscreen;
        private bool _leaveFullscreen;
        private bool _destroyed;
        private bool _hdrStateChanged;

        public event Action<WindowEvent>? Any;

        public WindowEvent? LastEvent { get; private set; }

        internal void Handle(WindowEvent windowEvent) {
            LastEvent = windowEvent;
            MarkDirty();

            switch (windowEvent.Type) {
                case EventType.WindowShown:
                    _shown = true;
                    break;
                case EventType.WindowHidden:
                    _hidden = true;
                    break;
                case EventType.WindowExposed:
                    _exposed = true;
                    break;
                case EventType.WindowMoved:
                    _moved = true;
                    break;
                case EventType.WindowResized:
                    _resized = true;
                    break;
                case EventType.WindowPixelSizeChanged:
                    _pixelSizeChanged = true;
                    break;
                case EventType.WindowMetalViewResized:
                    _metalViewResized = true;
                    break;
                case EventType.WindowMinimized:
                    _minimized = true;
                    break;
                case EventType.WindowMaximized:
                    _maximized = true;
                    break;
                case EventType.WindowRestored:
                    _restored = true;
                    break;
                case EventType.WindowMouseEnter:
                    _mouseEnter = true;
                    break;
                case EventType.WindowMouseLeave:
                    _mouseLeave = true;
                    break;
                case EventType.WindowFocusGained:
                    _focusGained = true;
                    break;
                case EventType.WindowFocusLost:
                    _focusLost = true;
                    break;
                case EventType.WindowCloseRequested:
                    _closeRequested = true;
                    break;
                case EventType.WindowHitTest:
                    _hitTest = true;
                    break;
                case EventType.WindowICCprofChanged:
                    _iccProfileChanged = true;
                    break;
                case EventType.WindowDisplayChanged:
                    _displayChanged = true;
                    break;
                case EventType.WindowDisplayScaleChanged:
                    _displayScaleChanged = true;
                    break;
                case EventType.WindowSafeAreaChanged:
                    _safeAreaChanged = true;
                    break;
                case EventType.WindowOccluded:
                    _occluded = true;
                    break;
                case EventType.WindowEnterFullscreen:
                    _enterFullscreen = true;
                    break;
                case EventType.WindowLeaveFullscreen:
                    _leaveFullscreen = true;
                    break;
                case EventType.WindowDestroyed:
                    _destroyed = true;
                    break;
                case EventType.WindowHDRStateChanged:
                    _hdrStateChanged = true;
                    break;
            }

            Any?.Invoke(windowEvent);
        }

        public bool HasEvent() {
            return LastEvent.HasValue;
        }

        public bool Shown() {
            return _shown;
        }
        public bool Hidden() {
            return _hidden;
        }
        public bool Exposed() {
            return _exposed;
        }
        public bool Moved() {
            return _moved;
        }
        public bool Resized() {
            return _resized;
        }
        public bool PixelSizeChanged() {
            return _pixelSizeChanged;
        }
        public bool MetalViewResized() {
            return _metalViewResized;
        }
        public bool Minimized() {
            return _minimized;
        }
        public bool Maximized() {
            return _maximized;
        }
        public bool Restored() {
            return _restored;
        }
        public bool MouseEnter() {
            return _mouseEnter;
        }
        public bool MouseLeave() {
            return _mouseLeave;
        }
        public bool FocusGained() {
            return _focusGained;
        }
        public bool FocusLost() {
            return _focusLost;
        }
        public bool CloseRequested() {
            return _closeRequested;
        }
        public bool HitTest() {
            return _hitTest;
        }
        public bool ICCProfileChanged() {
            return _iccProfileChanged;
        }
        public bool DisplayChanged() {
            return _displayChanged;
        }
        public bool DisplayScaleChanged() {
            return _displayScaleChanged;
        }
        public bool SafeAreaChanged() {
            return _safeAreaChanged;
        }
        public bool Occluded() {
            return _occluded;
        }
        public bool EnteredFullscreen() {
            return _enterFullscreen;
        }
        public bool LeftFullscreen() {
            return _leaveFullscreen;
        }
        public bool Destroyed() {
            return _destroyed;
        }
        public bool HDRStateChanged() {
            return _hdrStateChanged;
        }

        public bool LastWas(EventType type) {
            return LastEvent?.Type == type;
        }

        public bool ForWindow(uint windowId) {
            return LastEvent?.WindowID == windowId;
        }

        protected override void ResetState() {
            _shown = false;
            _hidden = false;
            _exposed = false;
            _moved = false;
            _resized = false;
            _pixelSizeChanged = false;
            _metalViewResized = false;
            _minimized = false;
            _maximized = false;
            _restored = false;
            _mouseEnter = false;
            _mouseLeave = false;
            _focusGained = false;
            _focusLost = false;
            _closeRequested = false;
            _hitTest = false;
            _iccProfileChanged = false;
            _displayChanged = false;
            _displayScaleChanged = false;
            _safeAreaChanged = false;
            _occluded = false;
            _enterFullscreen = false;
            _leaveFullscreen = false;
            _destroyed = false;
            _hdrStateChanged = false;
        }
    }
}
