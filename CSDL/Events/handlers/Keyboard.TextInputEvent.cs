// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IKeyboardEvents {
        TextInputEvent? LastTextInput { get; }
        int TextInputCount { get; }

        event Action<TextInputEvent>? TextInput;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Keyboard {
        private Counter _textInputCount;

        public TextInputEvent? LastTextInput { get; private set; }
        public int TextInputCount => _textInputCount;

        public event Action<TextInputEvent>? TextInput;

        internal void Handle(TextInputEvent e) {
            LastTextInput = e;
            IncrementCounter(ref _textInputCount);

            TextInput?.Invoke(e);
        }

        partial void ResetTextInputState() {
            if (!_textInputCount.HasEvents) return;
            _textInputCount.Reset();
        }
    }
}
