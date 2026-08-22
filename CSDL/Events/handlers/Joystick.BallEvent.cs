// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        JoyBallEvent? LastBallEvent { get; }
        int BallCount { get; }

        event Action<JoyBallEvent>? BallMoved;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick {
        private Counter _ballCount;

        public JoyBallEvent? LastBallEvent { get; private set; }
        public int BallCount => _ballCount;

        public event Action<JoyBallEvent>? BallMoved;

        internal void Handle(JoyBallEvent ballEvent) {
            Input.Joysticks.OnJoystickUpdated(ballEvent.Which, ballEvent.Timestamp);

            LastBallEvent = ballEvent;
            IncrementCounter(ref _ballCount);

            BallMoved?.Invoke(ballEvent);
        }

        partial void ResetBallState() {
            if (!_ballCount.HasEvents) return;
            _ballCount.Reset();
        }
    }
}
