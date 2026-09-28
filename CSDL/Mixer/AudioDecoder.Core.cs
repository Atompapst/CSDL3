// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Mixer {
    public readonly partial struct AudioDecoder : INativeHandle, IEquatable<AudioDecoder> {
        private readonly HandleId<Opaque.SdlAudioDecoder> _id;

        internal AudioDecoder(NativePtr<Opaque.SdlAudioDecoder> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal AudioDecoder(NativePtr<Opaque.SdlAudioDecoder> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The resource is gone.</exception>
        internal NativePtr<Opaque.SdlAudioDecoder> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyAudioDecoder" />
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlAudioDecoder> handle, out bool ownsHandle)) return;
            if (ownsHandle && !handle.IsNull) {
                SDL.DestroyAudioDecoder(handle);
            }
        }

        /// <summary>Two wrappers around the same resource are equal, whoever created them.</summary>
        public bool Equals(AudioDecoder other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is AudioDecoder other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(AudioDecoder left, AudioDecoder right) {
            return left.Equals(right);
        }

        public static bool operator !=(AudioDecoder left, AudioDecoder right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(AudioDecoder)}({_id})";
        }
    }
}
