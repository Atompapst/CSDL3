// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Video {
    public sealed class DisplayItem {
        public uint Id { get; }
        public string Name => GetName() ?? "Unknown Display";

        public float ContentScale => SDL.GetDisplayContentScale(Id);

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetNaturalDisplayOrientation"/>
        public DisplayOrientation NaturalOrientation => SDL.GetNaturalDisplayOrientation(Id);
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetCurrentDisplayOrientation"/>
        public DisplayOrientation CurrentOrientation => SDL.GetCurrentDisplayOrientation(Id);

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetDisplayProperties"/>
        public DisplayProperties Properties => new DisplayProperties(SDL.GetDisplayProperties(Id));

        public Rect Bounds => GetDisplayBounds();
        public Rect? UsableBounds => GetDisplayUsableBounds();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetDesktopDisplayMode"/>
        public DisplayMode? DesktopMode => GetDesktopDisplayMode();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetCurrentDisplayMode"/>
        public DisplayMode? CurrentMode => GetCurrentDisplayMode();

        internal DisplayItem(uint id) {
            Id = id;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetFullscreenDisplayModes"/>
        public DisplayMode[] FullscreenModes() {
            IntPtr ptr = SDL.GetFullscreenDisplayModes(Id, out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(FullscreenModes));
                return Array.Empty<DisplayMode>();
            }

            NativePtr<NativePtr<DisplayMode>> modes = ptr;
            DisplayMode[] result = new DisplayMode[count];
            for (int i = 0; i < count; i++) {
                result[i] = modes[i].Read();
            }
            Memory.Free(ptr);
            return result;
        }

        /// <summary>
        /// Get the closest match to the requested display mode.
        /// </summary>
        public bool TryGetClosestFullscreenMode(
            int w,
            int h,
            float refreshRate,
            bool includeHighDensityModes,
            out DisplayMode closest
        ) {
            return SDL.GetClosestFullscreenDisplayMode(Id, w, h, refreshRate, includeHighDensityModes, out closest).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetDisplayName"/>
        private string GetName() {
            return SDL.GetDisplayName(Id).ToUtf8StringOrLog() ?? "Unknown Display";
        }

        private Rect GetDisplayBounds() {
            SDL.GetDisplayBounds(Id, out Rect rect).LogIfFalse();
            return rect;
        }

        private Rect GetDisplayUsableBounds() {
            SDL.GetDisplayUsableBounds(Id, out Rect rect).LogIfFalse();
            return rect;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetDesktopDisplayMode"/>
        private DisplayMode? GetDesktopDisplayMode() {
            NativePtr<DisplayMode> mode = SDL.GetDesktopDisplayMode(Id).LogIfInvalid();
            return mode.IsNull ? null : mode.Read();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetCurrentDisplayMode"/>
        private DisplayMode? GetCurrentDisplayMode() {
            NativePtr<DisplayMode> mode = SDL.GetCurrentDisplayMode(Id).LogIfInvalid();
            return mode.IsNull ? null : mode.Read();
        }

        public override string ToString() {
            return $"{Name} (ID: {Id})";
        }
    }

}
