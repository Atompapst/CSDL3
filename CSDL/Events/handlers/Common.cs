// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface IApplicationEvents {
        CommonEvent? LastEvent { get; }
        event Action<CommonEvent>? Any;

        bool HasEvent();
        bool QuitRequested();
        bool Terminating();
        bool LowMemory();
        bool WillEnterBackground();
        bool DidEnterBackground();
        bool WillEnterForeground();
        bool DidEnterForeground();
        bool LocaleChanged();
        bool SystemThemeChanged();
        bool LastWas(EventType type);
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Common : EventHandlerBase, Interfaces.IApplicationEvents {
        private bool _quitRequested;
        private bool _terminating;
        private bool _lowMemory;
        private bool _willEnterBackground;
        private bool _didEnterBackground;
        private bool _willEnterForeground;
        private bool _didEnterForeground;
        private bool _localeChanged;
        private bool _systemThemeChanged;

        public event Action<CommonEvent>? Any;

        public CommonEvent? LastEvent { get; private set; }

        internal void Handle(CommonEvent commonEvent) {
            LastEvent = commonEvent;
            MarkDirty();

            switch ((EventType)commonEvent.Type) {
                case EventType.Quit:
                    _quitRequested = true;
                    break;
                case EventType.Terminating:
                    _terminating = true;
                    break;
                case EventType.LowMemory:
                    _lowMemory = true;
                    break;
                case EventType.WillEnterBackground:
                    _willEnterBackground = true;
                    break;
                case EventType.DidEnterBackground:
                    _didEnterBackground = true;
                    break;
                case EventType.WillEnterForeground:
                    _willEnterForeground = true;
                    break;
                case EventType.DidEnterForeground:
                    _didEnterForeground = true;
                    break;
                case EventType.LocaleChanged:
                    _localeChanged = true;
                    break;
                case EventType.SystemThemeChanged:
                    _systemThemeChanged = true;
                    break;
            }

            Any?.Invoke(commonEvent);
        }

        public bool HasEvent() {
            return LastEvent.HasValue;
        }

        public bool QuitRequested() {
            return _quitRequested;
        }
        public bool Terminating() {
            return _terminating;
        }
        public bool LowMemory() {
            return _lowMemory;
        }
        public bool WillEnterBackground() {
            return _willEnterBackground;
        }
        public bool DidEnterBackground() {
            return _didEnterBackground;
        }
        public bool WillEnterForeground() {
            return _willEnterForeground;
        }
        public bool DidEnterForeground() {
            return _didEnterForeground;
        }
        public bool LocaleChanged() {
            return _localeChanged;
        }
        public bool SystemThemeChanged() {
            return _systemThemeChanged;
        }

        public bool LastWas(EventType type) {
            return LastEvent?.Type == (uint)type;
        }

        protected override void ResetState() {
            _quitRequested = false;
            _terminating = false;
            _lowMemory = false;
            _willEnterBackground = false;
            _didEnterBackground = false;
            _willEnterForeground = false;
            _didEnterForeground = false;
            _localeChanged = false;
            _systemThemeChanged = false;
        }
    }
}
