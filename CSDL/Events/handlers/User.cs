// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface IUserEvents {
        Action<UserEvent>? OnUserEvent { get; set; }
    }
}

namespace CSDL.EventHandlers {
    internal sealed class User : Interfaces.IUserEvents {
        public Action<UserEvent>? OnUserEvent { get; set; }

        internal void Handle(UserEvent userEvent) {
            OnUserEvent?.Invoke(userEvent);
        }
    }
}
