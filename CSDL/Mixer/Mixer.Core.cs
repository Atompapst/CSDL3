// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Mixer {
    public readonly partial struct Mixer : INativeHandle, IHandleOwner, IEquatable<Mixer> {
        private readonly HandleId<Opaque.SdlMixer> _id;

        internal Mixer(NativePtr<Opaque.SdlMixer> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal Mixer(NativePtr<Opaque.SdlMixer> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <inheritdoc cref="IHandleOwner.AsOwner" />
        internal OwnerId AsOwner => _id.AsOwner;

        OwnerId IHandleOwner.AsOwner => _id.AsOwner;

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The resource is gone.</exception>
        internal NativePtr<Opaque.SdlMixer> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyMixer" />
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlMixer> handle, out bool ownsHandle)) return;
            if (!ownsHandle || handle.IsNull) return;

            ReleaseCallbacks(handle.Ptr);
            SDL.DestroyMixer(handle);
        }

        /// <summary>Two wrappers around the same resource are equal, whoever created them.</summary>
        public bool Equals(Mixer other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is Mixer other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(Mixer left, Mixer right) {
            return left.Equals(right);
        }

        public static bool operator !=(Mixer left, Mixer right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(Mixer)}({_id})";
        }
    }
}
