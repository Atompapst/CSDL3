// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Properties {

    public readonly struct PointerProperty(uint handle, string name) {
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetPointerProperty"/>
        public nint Get() {
            return SDL.GetPointerProperty(handle, name, nint.Zero);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.SetPointerProperty"/>
        public bool Set(nint value) {
            return SDL.SetPointerProperty(handle, name, value).LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.SetPointerPropertyWithCleanup"/>
        public bool SetWithCleanup(nint value, CleanupPropertyCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"PropertyCleanup:{Guid.NewGuid()}";
            CleanupPropertyCallback registeredCallback = (data, pointer) => {
                try {
                    callback(data, pointer);
                } finally {
                    CallbackRegistry.Unregister<CleanupPropertyCallback, SDL_CleanupPropertyCallbackNative>(id);
                }
            };
            SDL_CleanupPropertyCallbackNative native = CleanupPropertyCallbackWrapper.Create(registeredCallback);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(id, registeredCallback, native, userdata);

            CBool ok = SDL.SetPointerPropertyWithCleanup(handle, name, value, native, cb.userdataPtr);
            if (!ok) {
                CallbackRegistry.Unregister<CleanupPropertyCallback, SDL_CleanupPropertyCallbackNative>(id);
            }
            return ok.LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.ClearProperty"/>
        public bool Clear() {
            return SDL.ClearProperty(handle, name).LogIfFalse();
        }
        public static implicit operator nint(PointerProperty prop) {
            return prop.Get();
        }

        public override string ToString() {
            nint value = Get();
            return value == nint.Zero ? "null" : $"0x{value:X}";
        }
    }
}
