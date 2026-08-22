// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Video;

namespace CSDL {
    /// <summary>
    ///     The platform's native file and folder pickers.
    /// </summary>
    /// <remarks>
    ///     All of these are asynchronous: they return immediately and the chosen paths arrive through
    ///     the callback while the event loop is running. The callback, its userdata and the filter array
    ///     are kept alive until it fires, so nothing needs to be rooted by the caller.
    /// </remarks>
    public static class FileDialog {

        /// <inheritdoc cref="CSDL.Internal.Docs.Dialog.ShowOpenFileDialog"/>
        /// <param name="callback">invoked with the chosen file(s), or an empty list if the user cancelled.</param>
        /// <param name="window">the window the dialog should be modal to, or <see langword="null"/> for none.</param>
        /// <param name="filters">the (name, pattern) filters to offer, e.g. <c>("PNG images", "png")</c>.</param>
        /// <param name="defaultLocation">the folder (or file) to start in.</param>
        /// <param name="allowMany"><see langword="true"/> to let the user pick more than one file.</param>
        /// <param name="userdata">passed through to the callback.</param>
        public static void ShowOpenFile(DialogFileCallback callback, Window? window = null,
            IReadOnlyList<(string Name, string Pattern)>? filters = null, string? defaultLocation = null,
            bool allowMany = false, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            NativeFileFilters nativeFilters = NativeFileFilters.Allocate(filters);
            Registration registration = Registration.Create(callback, userdata, nativeFilters);

            SDL.ShowOpenFileDialog(registration.Native, registration.UserdataPtr, WindowHandle(window),
                nativeFilters.Ptr, nativeFilters.Count, defaultLocation, allowMany);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Dialog.ShowSaveFileDialog"/>
        /// <inheritdoc cref="ShowOpenFile" path="/param"/>
        public static void ShowSaveFile(DialogFileCallback callback, Window? window = null,
            IReadOnlyList<(string Name, string Pattern)>? filters = null, string? defaultLocation = null,
            object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            NativeFileFilters nativeFilters = NativeFileFilters.Allocate(filters);
            Registration registration = Registration.Create(callback, userdata, nativeFilters);

            SDL.ShowSaveFileDialog(registration.Native, registration.UserdataPtr, WindowHandle(window),
                nativeFilters.Ptr, nativeFilters.Count, defaultLocation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Dialog.ShowOpenFolderDialog"/>
        /// <inheritdoc cref="ShowOpenFile" path="/param"/>
        public static void ShowOpenFolder(DialogFileCallback callback, Window? window = null,
            string? defaultLocation = null, bool allowMany = false, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            Registration registration = Registration.Create(callback, userdata, null);

            SDL.ShowOpenFolderDialog(registration.Native, registration.UserdataPtr, WindowHandle(window),
                defaultLocation, allowMany);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Dialog.ShowFileDialogWithProperties"/>
        /// <param name="type">which kind of dialog to show.</param>
        /// <param name="callback">invoked with the chosen file(s).</param>
        /// <param name="properties">the dialog configuration.</param>
        /// <param name="userdata">passed through to the callback.</param>
        /// <remarks>
        ///     <paramref name="properties"/> stays owned by the caller and must not be disposed before
        ///     the callback has run - anything it points at (filters especially) is read by SDL while
        ///     the dialog is open.
        /// </remarks>
        public static void Show(FileDialogType type, DialogFileCallback callback, FileDialogProperties properties, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);
            ArgumentNullException.ThrowIfNull(properties);

            Registration registration = Registration.Create(callback, userdata, null);

            SDL.ShowFileDialogWithProperties(type, registration.Native, registration.UserdataPtr, properties.Handle);
        }

        private static NativePtr<Opaque.SdlWindow> WindowHandle(Window? window) {
            return window?.Handle ?? NativePtr<Opaque.SdlWindow>.Zero;
        }

        /// <summary>
        ///     One pending dialog: keeps the managed callback rooted (and the filter allocation alive)
        ///     until SDL has invoked it.
        /// </summary>
        private sealed class Registration {
            private Registration(SDL_DialogFileCallbackNative native, nint userdataPtr) {
                Native = native;
                UserdataPtr = userdataPtr;
            }

            public SDL_DialogFileCallbackNative Native { get; }
            public nint UserdataPtr { get; }

            public static Registration Create(DialogFileCallback callback, object? userdata, NativeFileFilters? filters) {
                string id = $"FileDialog:{Guid.NewGuid()}";

                DialogFileCallback wrapper = (data, filelist, filter) => {
                    try {
                        callback(data, filelist, filter);
                    }
                    finally {
                        filters?.Dispose();
                        CallbackRegistry.Unregister<DialogFileCallback, SDL_DialogFileCallbackNative>(id);
                    }
                };

                SDL_DialogFileCallbackNative native = DialogFileCallbackWrapper.Create(wrapper);
                (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(id, wrapper, native, userdata);
                return new Registration(native, cb.userdataPtr);
            }
        }
    }
}
