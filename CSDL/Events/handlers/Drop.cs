// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface IDropEvents {
        event Action<DropEvent>? Any;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Drop : Interfaces.IDropEvents {
        public event Action<DropEvent>? Any;

        internal void Handle(DropEvent dropEvent) {
            Any?.Invoke(dropEvent);
        }
    }
}
