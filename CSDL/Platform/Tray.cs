// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL {
    public readonly partial struct Tray {
        /// <summary>
        ///     Click slots SDL knows, and the only keys under which a tray's click callbacks
        ///     are ever registered. Knowing them statically is what lets <see cref="Dispose"/> clean up
        ///     without the tray having carried an array of ids around.
        /// </summary>
        internal static readonly string[] ClickSlots = {
            nameof(TrayCreateProperties.LeftClickCallback),
            nameof(TrayCreateProperties.MiddleClickCallback),
            nameof(TrayCreateProperties.RightClickCallback),
            //TODO uncomment after sdl 3.6.0 release nameof(TrayCreateProperties.DoubleClickCallback),
        };

        /// <summary>
        ///     The <see cref="CallbackRegistry"/> key for one click slot, keyed by whatever currently
        ///     identifies the tray - the property group before it exists, its native pointer after.
        /// </summary>
        internal static string ClickCallbackIdFor(object owner, string slot) {
            return $"TrayClick:{owner}:{slot}";
        }

        // No static constructor: on a struct the type initialiser also runs for an instance member on
        // `default`, so `default(Tray).IsValid` would spin up the video subsystem just to answer false.
        private static void EnsureVideo() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.CreateTray"/>
        /// <remarks>
        ///     SDL keeps using <paramref name="icon"/> only for the duration of this call, so the surface
        ///     can be disposed right after. Both arguments are optional - a tray without an icon shows
        ///     whatever the platform picks as a default.
        /// </remarks>
        public Tray(Surface icon = default, string? tooltip = null)
            : this(Create(icon, tooltip), HandleKind.Owned) { }

        private static NativePtr<Opaque.SdlTray> Create(Surface icon, string? tooltip) {
            EnsureVideo(); //TODO when surface becomes struct done => icon.IsDefault
            NativePtr<SurfaceData> iconPtr = icon is { IsValid: true } ? icon.Handle : NativePtr<SurfaceData>.Zero;
            return SDL.CreateTray(iconPtr, tooltip).ThrowIfInvalid(nameof(SDL.CreateTray));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.CreateTrayWithProperties"/>
        public Tray(TrayCreateProperties properties)
            : this(CreateWithProperties(properties), HandleKind.Owned) { }

        /// <remarks>
        ///     The click callbacks were registered against the property group, because their ids had to
        ///     exist before the tray did. Now that it does, they are renamed onto its pointer - which is
        ///     what lets every copy of this value, and <see cref="Dispose"/> after the handle is
        ///     retired, find the same registrations without a field holding them.
        /// </remarks>
        private static NativePtr<Opaque.SdlTray> CreateWithProperties(TrayCreateProperties properties) {
            EnsureVideo();
            ArgumentNullException.ThrowIfNull(properties);
            NativePtr<Opaque.SdlTray> tray = SDL.CreateTrayWithProperties(properties.Handle)
                .ThrowIfInvalid(nameof(SDL.CreateTrayWithProperties));
            properties.RehomeClickCallbacks(tray.Ptr);
            return tray;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayIcon"/>
        /// <param name="icon">the new icon, or <see langword="default"/> to remove the current one.</param>
        public void SetIcon(Surface icon) {
            //TODO when surface becomes struct done => icon.IsDefault
            NativePtr<SurfaceData> iconPtr = icon is { IsValid: true } ? icon.Handle : NativePtr<SurfaceData>.Zero;
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

        /// <summary>
        ///     Drops every click callback registered for this tray, freeing the userdata each one pinned.
        /// </summary>
        private static void ReleaseClickCallbacks(nint tray) {
            foreach (string slot in ClickSlots) {
                CallbackRegistry.Unregister<TrayClickCallback, SDL_TrayClickCallbackNative>(
                    ClickCallbackIdFor(tray, slot));
            }
        }

    }
}
