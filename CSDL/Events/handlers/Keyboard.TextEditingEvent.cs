// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IKeyboardEvents {
        TextEditingEvent? LastTextEditing { get; }
        TextEditingCandidatesEvent? LastTextEditingCandidates { get; }
        int TextEditingCount { get; }
        int TextEditingCandidatesCount { get; }

        event Action<TextEditingEvent>? TextEditing;
        event Action<TextEditingCandidatesEvent>? TextEditingCandidates;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Keyboard {
        private Counter _textEditingCount;
        private Counter _textEditingCandidatesCount;

        public TextEditingEvent? LastTextEditing { get; private set; }
        public TextEditingCandidatesEvent? LastTextEditingCandidates { get; private set; }
        public int TextEditingCount => _textEditingCount;
        public int TextEditingCandidatesCount => _textEditingCandidatesCount;

        public event Action<TextEditingEvent>? TextEditing;
        public event Action<TextEditingCandidatesEvent>? TextEditingCandidates;

        internal void Handle(TextEditingEvent e) {
            LastTextEditing = e;
            IncrementCounter(ref _textEditingCount);

            TextEditing?.Invoke(e);
        }

        internal void Handle(TextEditingCandidatesEvent e) {
            LastTextEditingCandidates = e;
            IncrementCounter(ref _textEditingCandidatesCount);

            TextEditingCandidates?.Invoke(e);
        }

        private static string PtrToStringUtf8(IntPtr ptr) {
            return ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
        }

        private static string[] ReadUtf8StringArray(IntPtr arrayPtr, int count) {
            if (arrayPtr == IntPtr.Zero || count <= 0) {
                return Array.Empty<string>();
            }

            string[] result = new string[count];

            for (int i = 0; i < count; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
                result[i] = PtrToStringUtf8(strPtr);
            }

            return result;
        }

        partial void ResetTextEditingState() {
            if (_textEditingCount.HasEvents) {
                _textEditingCount.Reset();
            }

            if (_textEditingCandidatesCount.HasEvents) {
                _textEditingCandidatesCount.Reset();
            }
        }
    }
}
