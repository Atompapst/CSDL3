// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    public class SharedObject : NativeHandle<Opaque.SdlSharedObject> {


        /// <inheritdoc cref="CSDL.Internal.Docs.SharedObject.LoadObject"/>
        public void Load(string sofile) {
            Handle = SDL.LoadObject(sofile);
            if (Handle.IsNull) {
                throw new SDLException($"SDL.LoadObject failed: {Error.GetError()}");
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.SharedObject.LoadFunction"/>
        public IntPtr LoadFunction(string name) {
            IntPtr address = SDL.LoadFunction(Handle, name);
            if (address == IntPtr.Zero) {
                Error.Throw(nameof(SDL.LoadFunction));
            }
            return address;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.SharedObject.UnloadObject"/>
        protected override void DisposeResource() {
            SDL.UnloadObject(Handle);
        }
    }
}
