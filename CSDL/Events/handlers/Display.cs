// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface IDisplayEvents {
        DisplayEvent? LastEvent { get; }
        event Action<Video.DisplayItem>? DisplayAdded;
        event Action<Video.DisplayItem>? DisplayRemoved;
        event Action<DisplayEvent>? OrientationChanged;
        event Action<DisplayEvent>? Moved;
        event Action<DisplayEvent>? DesktopModeChanged;
        event Action<DisplayEvent>? CurrentModeChanged;
        event Action<DisplayEvent>? ContentScaleChanged;
        event Action<DisplayEvent>? UsableBoundsChanged;
        bool LastWas(EventType type);
        Video.DisplayItem? GetDisplay();
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Display : Interfaces.IDisplayEvents {
        public event Action<Video.DisplayItem>? DisplayAdded;
        public event Action<Video.DisplayItem>? DisplayRemoved;
        public event Action<DisplayEvent>? OrientationChanged;
        public event Action<DisplayEvent>? Moved;
        public event Action<DisplayEvent>? DesktopModeChanged;
        public event Action<DisplayEvent>? CurrentModeChanged;
        public event Action<DisplayEvent>? ContentScaleChanged;
        public event Action<DisplayEvent>? UsableBoundsChanged;

        public DisplayEvent? LastEvent { get; private set; }

        internal void Handle(DisplayEvent displayEvent) {
            LastEvent = displayEvent;

            switch (displayEvent.Type) {
                case EventType.DisplayAdded:
                {
                    Video.Display.OnDisplayAdded(displayEvent.DisplayID);
                    Video.DisplayItem? added = Video.Display.Get(displayEvent.DisplayID);
                    if (added != null) DisplayAdded?.Invoke(added);
                    break;
                }

                case EventType.DisplayRemoved:
                {
                    Video.DisplayItem? removed = Video.Display.Get(displayEvent.DisplayID);
                    Video.Display.OnDisplayRemoved(displayEvent.DisplayID);
                    if (removed != null) DisplayRemoved?.Invoke(removed);
                    break;
                }

                case EventType.DisplayOrientation:
                    OrientationChanged?.Invoke(displayEvent);
                    break;

                case EventType.DisplayMoved:
                    Moved?.Invoke(displayEvent);
                    break;

                case EventType.DisplayDesktopModeChanged:
                    DesktopModeChanged?.Invoke(displayEvent);
                    break;

                case EventType.DisplayCurrentModeChanged:
                    CurrentModeChanged?.Invoke(displayEvent);
                    break;

                case EventType.DisplayContentScaleChanged:
                    ContentScaleChanged?.Invoke(displayEvent);
                    break;

                case EventType.DisplayUsableBoundsChanged:
                    UsableBoundsChanged?.Invoke(displayEvent);
                    break;
            }
        }

        public bool LastWas(EventType type) {
            return LastEvent?.Type == type;
        }

        public Video.DisplayItem? GetDisplay() {
            return LastEvent.HasValue
                ? Video.Display.Get(LastEvent.Value.DisplayID)
                : null;
        }
    }
}
