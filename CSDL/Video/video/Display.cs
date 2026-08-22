// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
using CSDL.Extensions;
namespace CSDL.Video {
    public static class Display {
        private static readonly Dictionary<DisplayID, DisplayItem> Displays = new Dictionary<DisplayID, DisplayItem>();

        private static void Refresh() {
            NativePtr<DisplayID> ids = SDL.GetDisplays(out int count).LogIfInvalid();
            if (ids.IsNull) {
                return;
            }

            Displays.Clear();

            // Add
            for (int i = 0; i < count; i++) {
                if (Displays.ContainsKey(ids[i])) continue;

                DisplayItem item = new DisplayItem(ids[i]);
                Displays[ids[i]] = item;
            }
        }

        internal static void OnDisplayAdded(uint id) {
            if (!Displays.ContainsKey(id)) {
                Displays[id] = new DisplayItem(id);
            }
        }

        internal static void OnDisplayRemoved(uint id) {
            Displays.Remove(id);
        }

        static Display() {
            Refresh();
        }

        public static bool IsPresent(uint id) {
            return Displays.ContainsKey(id);
        }

        public static DisplayItem? Get(uint id) {
            return Displays.GetValueOrDefault(id);
        }

        public static IReadOnlyCollection<DisplayItem> All => Displays.Values;

        public static DisplayItem? Primary => GetPrimary();

        public static DisplayItem? GetFor(Point point) {
            DisplayID id = SDL.GetDisplayForPoint(point);
            if (id == 0) {
                Error.LogError(nameof(SDL.GetDisplayForPoint));
                return null;
            }
            return Displays.GetValueOrDefault(id) ?? RegisterIfMissing(id);
        }

        public static DisplayItem? GetFor(Rect rect) {
            DisplayID id = SDL.GetDisplayForRect(rect);
            if (id == 0) {
                Error.LogError(nameof(SDL.GetDisplayForPoint));
                return null;
            }
            return Displays.GetValueOrDefault(id) ?? RegisterIfMissing(id);
        }

        public static DisplayItem? GetFor(Window window) {
            DisplayID id = SDL.GetDisplayForWindow(window.Handle);
            if (id == 0) {
                Error.LogError(nameof(SDL.GetDisplayForPoint));
                return null;
            }
            return Displays.GetValueOrDefault(id) ?? RegisterIfMissing(id);
        }

        private static DisplayItem? GetPrimary() {
            DisplayID id = SDL.GetPrimaryDisplay();
            if (id == 0) {
                Error.LogError(nameof(SDL.GetPrimaryDisplay));
                return null;
            }
            return Displays.GetValueOrDefault(id) ?? RegisterIfMissing(id);
        }

        // Falls back to a fresh lookup instead of throwing when a display was just hot-plugged
        // and its SDL_EVENT_DISPLAY_ADDED hasn't been pumped through OnDisplayAdded yet.
        private static DisplayItem RegisterIfMissing(DisplayID id) {
            DisplayItem item = new DisplayItem(id);
            Displays[id] = item;
            return item;
        }


    }
}
