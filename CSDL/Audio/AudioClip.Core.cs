// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Audio {
    public readonly partial struct AudioClip : INativeHandle, IEquatable<AudioClip> {
        private readonly HandleId<byte> _id;

        internal AudioClip(AudioSpec spec, NativePtr<byte> data, uint length, bool ownsHandle = true) {
            _id = HandleTable.Acquire(data, ownsHandle);
            Spec = spec;
            Length = length;
        }

        /// <summary>Gets the audio format specification of this clip.</summary>
        public AudioSpec Spec { get; }

        /// <summary>Gets the length of the PCM data in bytes.</summary>
        public uint Length { get; }

        /// <summary>
        ///     The live pointer to the PCM data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The clip has been disposed.</exception>
        internal NativePtr<byte> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        /// <remarks>Returns <c>0</c> rather than throwing once the clip is gone.</remarks>
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <summary>
        ///     Frees the PCM buffer.
        /// </summary>
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<byte> handle, out bool ownsHandle)) return;
            if (ownsHandle && !handle.IsNull) {
                Memory.Free(handle);
            }
        }

        /// <summary>Two wrappers around the same PCM buffer are equal, whoever created them.</summary>
        public bool Equals(AudioClip other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is AudioClip other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(AudioClip left, AudioClip right) {
            return left.Equals(right);
        }

        public static bool operator !=(AudioClip left, AudioClip right) {
            return !left.Equals(right);
        }
    }
}
