// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL {
    public class Tray : NativeHandle<Opaque.SdlTray> {
        private string[]? _clickCallbackIds;
        private GCHandle[]? _clickUserdataHandles;

        static Tray() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.CreateTray"/>
        /// <remarks>
        ///     SDL keeps using <paramref name="icon"/> only for the duration of this call, so the surface
        ///     can be disposed right after. Both arguments are optional - a tray without an icon shows
        ///     whatever the platform picks as a default.
        /// </remarks>
        public Tray(Surface? icon = null, string? tooltip = null) {
            NativePtr<SurfaceData> iconPtr = icon?.Handle ?? NativePtr<SurfaceData>.Zero;
            Handle = SDL.CreateTray(iconPtr, tooltip).ThrowIfInvalid(nameof(SDL.CreateTray));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.CreateTrayWithProperties"/>
        public Tray(TrayCreateProperties properties) {
            ArgumentNullException.ThrowIfNull(properties);
            Handle = SDL.CreateTrayWithProperties(properties.Handle).ThrowIfInvalid(nameof(SDL.CreateTrayWithProperties));
            (_clickCallbackIds, _clickUserdataHandles) = properties.TakeClickCallbackRegistrations();
        }

        public Tray(NativePtr<Opaque.SdlTray> handle, bool ownsHandle) : base(handle, ownsHandle) {
            Handle = handle;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayIcon"/>
        /// <param name="icon">the new icon, or <see langword="null"/> to remove the current one.</param>
        public void SetIcon(Surface? icon) {
            NativePtr<SurfaceData> iconPtr = icon?.Handle ?? NativePtr<SurfaceData>.Zero;
            SDL.SetTrayIcon(Handle, iconPtr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayTooltip"/>
        /// <param name="tooltip">the new tooltip, or <see langword="null"/> to remove the current one.</param>
        public void SetTooltip(string? tooltip) {
            SDL.SetTrayTooltip(Handle, tooltip);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.CreateTrayMenu"/>
        /// <remarks>
        ///     Call this at most once per tray - SDL creates the menu on the first call and
        ///     <see cref="Menu"/> hands back the same one afterwards.
        /// </remarks>
        public TrayMenu CreateMenu() {
            NativePtr<Opaque.SdlTrayMenu> menu = SDL.CreateTrayMenu(Handle).ThrowIfInvalid(nameof(SDL.CreateTrayMenu));
            return new TrayMenu(menu);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.GetTrayMenu"/>
        /// <value>The menu created by <see cref="CreateMenu"/>, or <see langword="null"/> if there is none yet.</value>
        public TrayMenu? Menu {
            get {
                NativePtr<Opaque.SdlTrayMenu> menu = SDL.GetTrayMenu(Handle);
                return menu.IsNull ? null : new TrayMenu(menu);
            }
        }

        /// <summary>
        ///     Returns the existing tray menu, creating it on first use.
        /// </summary>
        /// <seealso cref="CreateMenu"/>
        public TrayMenu GetOrCreateMenu() {
            return Menu ?? CreateMenu();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.UpdateTrays"/>
        public static void Update() {
            SDL.UpdateTrays();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.DestroyTray"/>
        protected override void DisposeResource() {
            try {
                SDL.DestroyTray(Handle);
            } finally {
                ReleaseClickCallbackRegistrations();
            }
        }

        private void ReleaseClickCallbackRegistrations() {
            if (_clickCallbackIds is not null) {
                foreach (string id in _clickCallbackIds) {
                    CallbackRegistry.Unregister<TrayClickCallback, SDL_TrayClickCallbackNative>(id);
                }
                _clickCallbackIds = null;
            }

            if (_clickUserdataHandles is not null) {
                foreach (GCHandle handle in _clickUserdataHandles) {
                    if (handle.IsAllocated) {
                        handle.Free();
                    }
                }
                _clickUserdataHandles = null;
            }
        }

    }
}
