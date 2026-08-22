// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Properties {
     public abstract class PropertyGroup : IDisposable {
        public uint Handle { get; private set; }
        private readonly bool _ownsHandle;
        private bool _disposed;

        /// <summary>
        /// Wraps a property set that SDL owns and manages the lifetime of (e.g. a window's or
        /// display's properties). The wrapper never owns the set, so it must never destroy it -
        /// its finalizer is suppressed immediately.
        /// </summary>
        protected PropertyGroup(uint handle) : this(handle, false) { }

        /// <summary>
        /// Wraps a property set with explicit control over whether this instance owns it and is
        /// responsible for destroying it on <see cref="Dispose"/>.
        /// </summary>
        protected PropertyGroup(uint handle, bool ownsHandle) {
            Handle = handle;
            _ownsHandle = ownsHandle;
            if (!ownsHandle) {
                GC.SuppressFinalize(this);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.CreateProperties"/>
        protected PropertyGroup() : this(SDL.CreateProperties(), true) {
            if (Handle == 0) {
                Error.LogError();
            }
        }

        protected StringProperty PropString(string name) {
            return new StringProperty(Handle, name);
        }
        protected NumberProperty PropNumber(string name) {
            return new NumberProperty(Handle, name);
        }
        protected BooleanProperty PropBool(string name) {
            return new BooleanProperty(Handle, name);
        }
        protected FloatProperty PropFloat(string name) {
            return new FloatProperty(Handle, name);
        }
        protected PointerProperty PropPointer(string name) {
            return new PointerProperty(Handle, name);
        }

        /// <summary>
        /// Accesses an arbitrary string property by name - for property sets that also carry
        /// app-specific data next to the ones SDL defines.
        /// </summary>
        public StringProperty String(string name) {
            return PropString(name);
        }

        /// <inheritdoc cref="String"/>
        public NumberProperty Number(string name) {
            return PropNumber(name);
        }

        /// <inheritdoc cref="String"/>
        public BooleanProperty Bool(string name) {
            return PropBool(name);
        }

        /// <inheritdoc cref="String"/>
        public FloatProperty Float(string name) {
            return PropFloat(name);
        }

        /// <inheritdoc cref="String"/>
        public PointerProperty Pointer(string name) {
            return PropPointer(name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetNumProperties"/>
        public int Count => SDL.GetNumProperties(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.CopyProperties"/>
        public static bool CopyProperties(PropertyGroup src, PropertyGroup dst) {
            return SDL.CopyProperties(src.Handle, dst.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.LockProperties"/>
        public bool LockProperties() {
            return SDL.LockProperties(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.UnlockProperties"/>
        public void UnlockProperties() {
            SDL.UnlockProperties(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.ClearProperty"/>
        public bool Clear(string name) {
            return SDL.ClearProperty(Handle, name).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.HasProperty"/>
        public bool HasProperty(string name) {
            return SDL.HasProperty(Handle, name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetPropertyType"/>
        public PropertyType GetPropertyType(string name) {
            return SDL.GetPropertyType(Handle, name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.EnumerateProperties"/>
        public bool EnumerateProperties(EnumeratePropertiesCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"PropertiesEnumerate:{Guid.NewGuid()}";
            SDL_EnumeratePropertiesCallbackNative native = EnumeratePropertiesCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(id, callback, native, userdata);

            try {
                return SDL.EnumerateProperties(Handle, native, cb.userdataPtr).LogIfFalse();
            } finally {
                CallbackRegistry.Unregister<EnumeratePropertiesCallback, SDL_EnumeratePropertiesCallbackNative>(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.DestroyProperties"/>
        public virtual void Dispose() {
            if (_disposed) return;
            _disposed = true;

            if (_ownsHandle && Handle != 0) {
                SDL.DestroyProperties(Handle);
            }
            Handle = 0;
            GC.SuppressFinalize(this);
        }

        ~PropertyGroup() {
            Dispose();
        }
    }
}
