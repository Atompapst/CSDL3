// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Audio {
    public readonly partial struct AudioStream : INativeHandle, IEquatable<AudioStream> {
        private readonly HandleId<Opaque.SdlAudioStream> _id;

        /// <summary>
        ///     Serialises "tell SDL" against "record it in the registry" for every stream at once.
        /// </summary>
        private static readonly object CallbackLock = new object();

        internal AudioStream(NativePtr<Opaque.SdlAudioStream> handle, bool ownsHandle = false)
            : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed) { }

        internal AudioStream(NativePtr<Opaque.SdlAudioStream> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>The table identity of this stream.</summary>
        internal HandleId<Opaque.SdlAudioStream> TableId => _id;

        /// <summary>
        ///     The <see cref="CallbackRegistry" /> key of the stream's get callback.
        /// </summary>
        internal static string GetCallbackIdFor(nint stream) {
            return $"audiostreamget:{stream}";
        }

        /// <inheritdoc cref="GetCallbackIdFor" />
        internal static string PutCallbackIdFor(nint stream) {
            return $"audiostreamput:{stream}";
        }

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     The last reference to the stream is gone, or whoever owned it destroyed it.
        /// </exception>
        internal NativePtr<Opaque.SdlAudioStream> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        /// <remarks>Returns <c>0</c> rather than throwing once the stream is gone.</remarks>
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.DestroyAudioStream" />
        /// <remarks>
        ///     A mixer track feeding from this stream holds a reference of its own,
        ///     so disposing the wrapper while one is outstanding
        ///     drops this caller's reference and leaves the stream alive - which is what the wrapper
        ///     used to achieve with a lease counter.
        /// </remarks>
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlAudioStream> handle, out bool ownsHandle)) return;
            if (!ownsHandle || handle.IsNull) return;

            lock (CallbackLock) {
                SDL.SetAudioStreamGetCallback(handle, null!, IntPtr.Zero);
                SDL.SetAudioStreamPutCallback(handle, null!, IntPtr.Zero);
                Unregister(GetCallbackIdFor(handle.Ptr));
                Unregister(PutCallbackIdFor(handle.Ptr));
            }
            SDL.DestroyAudioStream(handle);
        }

        /// <summary>
        ///     Rebuilds a handle from the id a caller wrote somewhere durable.
        /// </summary>
        /// <remarks>
        ///     For the one case a value type cannot serve on its own: something has to hold a reference
        ///     on this stream for as long as it uses it, and give that same reference back later. A
        ///     class wrapper kept the object in a field; an id in a native property set is the same
        ///     note, written where the thing that owes the release can find it again. A stale id
        ///     rebuilds into a handle that simply is not live, so releasing one twice is a no-op rather
        ///     than a double free.
        /// </remarks>
        internal static AudioStream FromTableId(long id) {
            return new AudioStream(new HandleId<Opaque.SdlAudioStream>(id));
        }

        private AudioStream(HandleId<Opaque.SdlAudioStream> id) {
            _id = id;
        }

        /// <summary>Takes one more reference, so the stream outlives this wrapper's disposal.</summary>
        internal bool AddRef() {
            return HandleTable.AddRef(_id);
        }

        /// <summary>
        ///     Retires every handle to this stream without destroying anything - SDL already did.
        /// </summary>
        internal void Invalidate() {
            HandleTable.Invalidate(_id);
        }

        /// <summary>Drops a callback registration if there is one.</summary>
        private static bool Unregister(string id) {
            return CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(id);
        }

        public bool Equals(AudioStream other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is AudioStream other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(AudioStream left, AudioStream right) {
            return left.Equals(right);
        }

        public static bool operator !=(AudioStream left, AudioStream right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(AudioStream)}({_id})";
        }
    }
}
