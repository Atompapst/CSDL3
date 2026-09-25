// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    /// <summary>
    ///     The handle/ownership bookkeeping for <see cref="TrayEntry" />, kept apart from the SDL surface of
    ///     the wrapper itself.
    /// </summary>
    /// <remarks>
    ///     SDL has no separate lifetime for an entry - it lives and dies with its <see cref="Tray" />, which the entry records as its owner.
    ///     Destroying the tray therefore makes every entry handle below it stale at once, including copies held by a click handler.
    /// </remarks>
    public readonly partial struct TrayEntry : INativeHandle, IEquatable<TrayEntry> {
        private readonly HandleId<Opaque.SdlTrayEntry> _id;

        internal TrayEntry(NativePtr<Opaque.SdlTrayEntry> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal TrayEntry(NativePtr<Opaque.SdlTrayEntry> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>The table identity of this handle.</summary>
        internal HandleId<Opaque.SdlTrayEntry> TableId => _id;

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The resource is gone.</exception>
        internal NativePtr<Opaque.SdlTrayEntry> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        /// <remarks>Returns <c>0</c> rather than throwing once the resource is gone.</remarks>
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.RemoveTrayEntry" />
        /// <remarks>
        ///     A no-op once the owning <see cref="Tray" /> is gone - SDL removed the entry with it, and
        ///     the owner link says so without anybody having kept a list.
        /// </remarks>
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlTrayEntry> handle, out bool ownsHandle)) return;
            if (!ownsHandle || handle.IsNull) return;

            CallbackRegistry.Unregister<TrayCallback, SDL_TrayCallbackNative>(CallbackIdFor(handle.Ptr));
            SDL.RemoveTrayEntry(handle);
        }

        /// <summary>Two wrappers around the same resource are equal, whoever created them.</summary>
        public bool Equals(TrayEntry other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is TrayEntry other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(TrayEntry left, TrayEntry right) {
            return left.Equals(right);
        }

        public static bool operator !=(TrayEntry left, TrayEntry right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(TrayEntry)}({_id})";
        }
    }
}
