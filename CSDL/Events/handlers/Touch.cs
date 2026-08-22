// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.EventHandlers.Interfaces {
    public partial interface ITouchEvents { }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Touch : EventHandlerBase, Interfaces.ITouchEvents {
        protected override void ResetState() {
            ResetFingerState();
            ResetPinchState();
        }

        partial void ResetFingerState();
        partial void ResetPinchState();
    }
}
