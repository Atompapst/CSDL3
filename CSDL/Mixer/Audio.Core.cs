// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Mixer {
    public readonly partial struct Audio : INativeHandle, IEquatable<Audio> {
        private readonly HandleId<Opaque.SdlAudio> _id;

        internal Audio(NativePtr<Opaque.SdlAudio> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal Audio(NativePtr<Opaque.SdlAudio> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The resource is gone.</exception>
        internal NativePtr<Opaque.SdlAudio> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyAudio" />
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlAudio> handle, out bool ownsHandle)) return;
            if (ownsHandle && !handle.IsNull) {
                SDL.DestroyAudio(handle);
            }
        }

        public bool Equals(Audio other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is Audio other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(Audio left, Audio right) {
            return left.Equals(right);
        }

        public static bool operator !=(Audio left, Audio right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(Audio)}({_id})";
        }
    }
}
