// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Video {
    public static class Clipboard {
        static Clipboard() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.ClearClipboardData"/>
        public static bool ClearData() {
            return SDL.ClearClipboardData().LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.GetClipboardData"/>
        public static byte[]? GetData(string mimeType) {
            ArgumentNullException.ThrowIfNull(mimeType);

            IntPtr ptr = SDL.GetClipboardData(mimeType, out nuint size);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetData));
                return null;
            }

            byte[] result = new NativePtr<byte>(ptr).ToManaged((int)size);
            Memory.Free(ptr);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.GetClipboardMimeTypes"/>
        public static string[] GetMimeTypes() {
            IntPtr ptr = SDL.GetClipboardMimeTypes(out nuint count);
            if (ptr == IntPtr.Zero || count == 0) {
                return Array.Empty<string>();
            }

            string[] result = NativeStringArray.ToArray(ptr, (int)count);
            Memory.Free(ptr);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.GetClipboardText"/>
        public static string GetText() {
            return SDL.GetClipboardText().ToUtf8StringAndFree() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.GetPrimarySelectionText"/>
        public static string GetPrimarySelectionText() {
            return SDL.GetPrimarySelectionText().ToUtf8StringAndFree() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.HasClipboardData"/>
        public static bool HasData(string mimeType) {
            return SDL.HasClipboardData(mimeType);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.HasClipboardText"/>
        public static bool HasText => SDL.HasClipboardText();

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.HasPrimarySelectionText"/>
        public static bool HasPrimarySelectionText => SDL.HasPrimarySelectionText();

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.SetClipboardData"/>
        public static bool SetData(ClipboardDataCallback callback, ClipboardCleanupCallback cleanup, string[] mimeTypes, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            ArgumentNullException.ThrowIfNull(cleanup);
            ArgumentNullException.ThrowIfNull(mimeTypes);

            SDL_ClipboardDataCallbackNative nativeCallback = ClipboardDataCallbackWrapper.Create(callback);
            string dataCallbackId = $"ClipboardData:{Guid.NewGuid()}";
            string cleanupCallbackId = $"ClipboardCleanup:{Guid.NewGuid()}";
            ClipboardCleanupCallback registeredCleanup = data => {
                try {
                    cleanup(data);
                } finally {
                    // SDL invokes cleanup before it discards the userdata pointer.
                    CallbackRegistry.Unregister<ClipboardDataCallback, SDL_ClipboardDataCallbackNative>(dataCallbackId);
                    CallbackRegistry.Unregister<ClipboardCleanupCallback, SDL_ClipboardCleanupCallbackNative>(cleanupCallbackId);
                }
            };
            SDL_ClipboardCleanupCallbackNative nativeCleanup = ClipboardCleanupCallbackWrapper.Create(registeredCleanup);

            (IntPtr functionPtr, IntPtr userdataPtr) dataReg =
                CallbackRegistry.Register(dataCallbackId, callback, nativeCallback, userData);
            try {
                CallbackRegistry.Register(cleanupCallbackId, registeredCleanup, nativeCleanup);
            } catch {
                CallbackRegistry.Unregister<ClipboardDataCallback, SDL_ClipboardDataCallbackNative>(dataCallbackId);
                throw;
            }

            using NativeStringArray.Native mimeTypesNative = NativeStringArray.Allocate(mimeTypes);
            bool ok = SDL.SetClipboardData(nativeCallback, nativeCleanup, dataReg.userdataPtr, mimeTypesNative.Ptr, (nuint)mimeTypesNative.Count).LogIfFalse();
            if (!ok) {
                // SDL never took ownership, so its cleanup callback will never fire to unregister these.
                CallbackRegistry.Unregister<ClipboardDataCallback, SDL_ClipboardDataCallbackNative>(dataCallbackId);
                CallbackRegistry.Unregister<ClipboardCleanupCallback, SDL_ClipboardCleanupCallbackNative>(cleanupCallbackId);
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.SetClipboardText"/>
        public static bool SetText(string text) {
            return SDL.SetClipboardText(text).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Clipboard.SetPrimarySelectionText"/>
        public static bool SetPrimarySelectionText(string text) {
            return SDL.SetPrimarySelectionText(text).LogIfFalse();
        }
    }
}
