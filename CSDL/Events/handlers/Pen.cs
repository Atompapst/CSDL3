// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IPenEvents {
        bool IsButtonDown(uint penId, byte button);
        bool IsButtonUp(uint penId, byte button);
        bool PressedThisCycle(uint penId, byte button);
        bool ReleasedThisCycle(uint penId, byte button);
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Pen : EventHandlerBase, Interfaces.IPenEvents {
        private readonly Dictionary<uint, HashSet<byte>> _buttonStates = new Dictionary<uint, HashSet<byte>>();
        private readonly Dictionary<uint, HashSet<byte>> _pressedThisCycle = new Dictionary<uint, HashSet<byte>>();
        private readonly Dictionary<uint, HashSet<byte>> _releasedThisCycle = new Dictionary<uint, HashSet<byte>>();

        public bool IsButtonDown(uint penId, byte button) {
            return _buttonStates.TryGetValue(penId, out HashSet<byte>? buttons) && buttons.Contains(button);
        }

        public bool IsButtonUp(uint penId, byte button) {
            return !IsButtonDown(penId, button);
        }

        public bool PressedThisCycle(uint penId, byte button) {
            return _pressedThisCycle.TryGetValue(penId, out HashSet<byte>? buttons) && buttons.Contains(button);
        }

        public bool ReleasedThisCycle(uint penId, byte button) {
            return _releasedThisCycle.TryGetValue(penId, out HashSet<byte>? buttons) && buttons.Contains(button);
        }

        protected override void ResetState() {
            ResetProximityState();
            ResetTouchState();
            ResetMotionState();
            ResetButtonState();
            ResetAxisState();
        }

        partial void ResetProximityState();
        partial void ResetTouchState();
        partial void ResetMotionState();
        partial void ResetButtonState();
        partial void ResetAxisState();
    }
}
