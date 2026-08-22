// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface INotificationEvents {
        event Action<NotificationEvent>? ActionInvoked;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Notification : Interfaces.INotificationEvents {
        public event Action<NotificationEvent>? ActionInvoked;

        internal void Handle(NotificationEvent notificationEvent) {
            ActionInvoked?.Invoke(notificationEvent);
        }
    }
}
