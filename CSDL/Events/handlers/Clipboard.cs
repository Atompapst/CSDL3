// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;

namespace CSDL.EventHandlers.Interfaces {
    public interface IClipboardEvents {
        bool IsOwner { get; }
        ulong TimestampNs { get; }
        IReadOnlyList<string> MimeTypes { get; }
        event Action<IClipboardEvents>? Updated;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Clipboard : Interfaces.IClipboardEvents {
        private bool _isOwner;
        private ulong _timestampNs;
        private readonly List<string> _mimeTypes = new List<string>();

        public bool IsOwner => _isOwner;
        public ulong TimestampNs => _timestampNs;
        public IReadOnlyList<string> MimeTypes => _mimeTypes;

        public event Action<Interfaces.IClipboardEvents>? Updated;

        internal void Handle(ClipboardEvent clipboardEvent) {
            _isOwner = clipboardEvent.Owner;
            _timestampNs = clipboardEvent.Timestamp;
            _mimeTypes.Clear();
            string[] mimeTypesArray = clipboardEvent.MimeTypes;
            foreach (string mimeType in mimeTypesArray) {
                if (!string.IsNullOrEmpty(mimeType)) {
                    _mimeTypes.Add(mimeType);
                }
            }

            Updated?.Invoke(this);
        }
    }


}
