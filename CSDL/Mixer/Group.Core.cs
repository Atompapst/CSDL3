// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Mixer {
    public readonly partial struct Group : INativeHandle, IEquatable<Group> {
        private readonly HandleId<Opaque.SdlGroup> _id;

        internal Group(NativePtr<Opaque.SdlGroup> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal Group(NativePtr<Opaque.SdlGroup> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The resource is gone.</exception>
        internal NativePtr<Opaque.SdlGroup> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyGroup" />
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlGroup> handle, out bool ownsHandle)) return;
            if (!ownsHandle || handle.IsNull) return;

            ReleaseCallbacks(handle.Ptr);
            SDL.DestroyGroup(handle);
        }

        public bool Equals(Group other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is Group other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(Group left, Group right) {
            return left.Equals(right);
        }

        public static bool operator !=(Group left, Group right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(Group)}({_id})";
        }
    }
}
