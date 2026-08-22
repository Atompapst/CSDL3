// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IKeyboardEvents {
        int ScreenKeyboardCount { get; }

        event Action<KeyboardItem>? KeyboardAdded;
        event Action<uint>? KeyboardRemoved;
        event Action<KeyboardDeviceEvent>? ScreenKeyboardShown;
        event Action<KeyboardDeviceEvent>? ScreenKeyboardHidden;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Keyboard {
        private Counter _screenKeyboardCount;

        public int ScreenKeyboardCount => _screenKeyboardCount;

        public event Action<KeyboardItem>? KeyboardAdded;
        public event Action<uint>? KeyboardRemoved;
        public event Action<KeyboardDeviceEvent>? ScreenKeyboardShown;
        public event Action<KeyboardDeviceEvent>? ScreenKeyboardHidden;

        internal void Handle(KeyboardDeviceEvent e) {
            switch (e.Type) {
                case EventType.KeyboardAdded:
                {
                    Keyboards.OnKeyboardAdded(e.Which, e.Timestamp);
                    KeyboardItem? added = Keyboards.Get(e.Which);
                    if (added != null) {
                        KeyboardAdded?.Invoke(added);
                    }
                    break;
                }

                case EventType.KeyboardRemoved:
                {
                    Keyboards.OnKeyboardRemoved(e.Which);
                    KeyboardRemoved?.Invoke(e.Which);
                    break;
                }

                case EventType.ScreenKeyboardShown:
                {
                    IncrementCounter(ref _screenKeyboardCount);
                    ScreenKeyboardShown?.Invoke(e);
                    break;
                }

                case EventType.ScreenKeyboardHidden:
                {
                    IncrementCounter(ref _screenKeyboardCount);
                    ScreenKeyboardHidden?.Invoke(e);
                    break;
                }
            }
        }

        partial void ResetDeviceState() {
            if (!_screenKeyboardCount.HasEvents) return;
            _screenKeyboardCount.Reset();
        }
    }
}
