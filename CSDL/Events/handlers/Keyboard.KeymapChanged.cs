// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IKeyboardEvents {
        event Action<CommonEvent>? KeymapChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Keyboard {
        public event Action<CommonEvent>? KeymapChanged;

        internal void Handle(CommonEvent commonEvent) {
            KeymapChanged?.Invoke(commonEvent);
        }
    }
}
