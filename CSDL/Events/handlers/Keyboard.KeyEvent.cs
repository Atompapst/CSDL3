// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IKeyboardEvents {
        KeyboardEvent? LastKeyEvent { get; }
        int KeyCount { get; }

        event Action<KeyboardEvent>? KeyChanged;
        event Action<KeyboardEvent>? KeyDown;
        event Action<KeyboardEvent>? KeyUp;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Keyboard {
        private readonly HashSet<Scancode> _downKeys = new HashSet<Scancode>();
        private readonly HashSet<Scancode> _pressedThisCycle = new HashSet<Scancode>();
        private readonly HashSet<Scancode> _releasedThisCycle = new HashSet<Scancode>();
        private readonly HashSet<Scancode> _repeatedThisCycle = new HashSet<Scancode>();

        private Counter _keyCount;
        private Keymod _currentModifiers;

        public KeyboardEvent? LastKeyEvent { get; private set; }
        public int KeyCount => _keyCount;

        public event Action<KeyboardEvent>? KeyChanged;
        public event Action<KeyboardEvent>? KeyDown;
        public event Action<KeyboardEvent>? KeyUp;

        internal void Handle(KeyboardEvent e) {
            Keyboards.OnKeyboardUpdated(e.Which, e.Timestamp);

            bool wasDown = _downKeys.Contains(e.Scancode);

            if (e.Down) {
                if (e.Repeat) {
                    // repeat: the key is still down and repeating
                    _repeatedThisCycle.Add(e.Scancode);
                } else {
                    // Initial key press
                    _downKeys.Add(e.Scancode);

                    if (!wasDown) {
                        _pressedThisCycle.Add(e.Scancode);
                    }
                }
            } else {
                // Key release
                _downKeys.Remove(e.Scancode);

                if (wasDown) {
                    _releasedThisCycle.Add(e.Scancode);
                }
            }

            // Track current modifiers
            _currentModifiers = e.Mod;

            LastKeyEvent = e;
            IncrementCounter(ref _keyCount);

            KeyChanged?.Invoke(e);

            if (e.Down) {
                KeyDown?.Invoke(e);
            } else {
                KeyUp?.Invoke(e);
            }
        }

        partial void ResetKeyState() {
            if (!_keyCount.HasEvents) return;
            _pressedThisCycle.Clear();
            _releasedThisCycle.Clear();
            _repeatedThisCycle.Clear();
            _keyCount.Reset();
        }
    }
}
