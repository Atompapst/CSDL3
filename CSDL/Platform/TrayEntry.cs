// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    public class TrayEntry : NativeHandle<Opaque.SdlTrayEntry> {

        public TrayEntry(NativePtr<Opaque.SdlTrayEntry> handle, bool ownsHandle) : base(handle, ownsHandle) {
            Handle = handle;
        }

        private string CallbackId => $"TrayEntry:{Handle.Ptr}";

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayEntryLabel"/>
        /// <remarks>Reading this gives back <see langword="null"/> for a separator.</remarks>
        public string? Label {
            get => SDL.GetTrayEntryLabel(Handle).ToUtf8String();
            set => SDL.SetTrayEntryLabel(Handle, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayEntryEnabled"/>
        public bool Enabled {
            get => SDL.GetTrayEntryEnabled(Handle);
            set => SDL.SetTrayEntryEnabled(Handle, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayEntryChecked"/>
        /// <remarks>Only meaningful for entries created with <see cref="TrayEntryFlags.Checkbox"/>.</remarks>
        public bool Checked {
            get => SDL.GetTrayEntryChecked(Handle);
            set => SDL.SetTrayEntryChecked(Handle, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.GetTrayEntryParent"/>
        public TrayMenu? Parent {
            get {
                NativePtr<Opaque.SdlTrayMenu> menu = SDL.GetTrayEntryParent(Handle);
                return menu.IsNull ? null : new TrayMenu(menu);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.GetTraySubmenu"/>
        /// <value>This entry's submenu, or <see langword="null"/> if it has none.</value>
        public TrayMenu? Submenu {
            get {
                NativePtr<Opaque.SdlTrayMenu> menu = SDL.GetTraySubmenu(Handle);
                return menu.IsNull ? null : new TrayMenu(menu);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.CreateTraySubmenu"/>
        /// <remarks>
        ///     The entry must have been created with <see cref="TrayEntryFlags.Submenu"/>, and this may
        ///     only be called once per entry - use <see cref="Submenu"/> afterwards.
        /// </remarks>
        public TrayMenu CreateSubmenu() {
            NativePtr<Opaque.SdlTrayMenu> menu = SDL.CreateTraySubmenu(Handle).ThrowIfInvalid(nameof(SDL.CreateTraySubmenu));
            return new TrayMenu(menu);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.ClickTrayEntry"/>
        public void Click() {
            SDL.ClickTrayEntry(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.SetTrayEntryCallback"/>
        /// <remarks>
        ///     Replaces any callback previously set on this entry. The delegate and
        ///     <paramref name="userdata"/> are rooted for as long as they are installed, so neither
        ///     needs to be kept alive by the caller.
        /// </remarks>
        public void SetCallback(TrayCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            RemoveCallback();

            SDL_TrayCallbackNative native = TrayCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(CallbackId, callback, native, userdata);
            SDL.SetTrayEntryCallback(Handle, native, cb.userdataPtr);
        }

        /// <summary>
        ///     Removes the callback previously installed by <see cref="SetCallback"/>, if any.
        /// </summary>
        /// <seealso cref="CSDL.Internal.Docs.Tray.SetTrayEntryCallback">SDL_SetTrayEntryCallback</seealso>
        public void RemoveCallback() {
            if (!CallbackRegistry.IsRegistered<TrayCallback, SDL_TrayCallbackNative>(CallbackId)) {
                return;
            }

            SDL.SetTrayEntryCallback(Handle, null!, IntPtr.Zero);
            CallbackRegistry.Unregister<TrayCallback, SDL_TrayCallbackNative>(CallbackId);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.RemoveTrayEntry"/>
        protected override void DisposeResource() {
            CallbackRegistry.Unregister<TrayCallback, SDL_TrayCallbackNative>(CallbackId);
            SDL.RemoveTrayEntry(Handle);
        }

    }
}
