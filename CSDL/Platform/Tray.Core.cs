// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    public readonly partial struct Tray : INativeHandle, IHandleOwner, IEquatable<Tray> {
        private readonly HandleId<Opaque.SdlTray> _id;

        internal Tray(NativePtr<Opaque.SdlTray> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal Tray(NativePtr<Opaque.SdlTray> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>The table identity of this handle.</summary>
        internal HandleId<Opaque.SdlTray> TableId => _id;

        /// <inheritdoc cref="IHandleOwner.AsOwner" />
        internal OwnerId AsOwner => _id.AsOwner;

        OwnerId IHandleOwner.AsOwner => _id.AsOwner;

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The resource is gone.</exception>
        internal NativePtr<Opaque.SdlTray> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.DestroyTray" />
        /// <remarks>
        ///     Takes the menu and every entry below it, which is what SDL does natively.
        /// </remarks>
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlTray> handle, out bool ownsHandle)) return;
            if (!ownsHandle || handle.IsNull) return;

            try {
                SDL.DestroyTray(handle);
            } finally {
                ReleaseClickCallbacks(handle.Ptr);
            }
        }

        public bool Equals(Tray other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is Tray other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(Tray left, Tray right) {
            return left.Equals(right);
        }

        public static bool operator !=(Tray left, Tray right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(Tray)}({_id})";
        }
    }
}
