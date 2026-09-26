// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL.Mixer {
    public readonly partial struct Track : INativeHandle, IEquatable<Track> {
        private readonly HandleId<Opaque.SdlTrack> _id;

        /// <summary>
        ///     Serialises "tell SDL_mixer" against "record it in the registry" for every track at once.
        /// </summary>
        /// <inheritdoc cref="CSDL.Audio.AudioStream" />
        private static readonly object CallbackLock = new object();

        /// <summary>The table id of the stream this track holds a reference on, or 0 for none.</summary>
        internal const string InputAudioStreamProperty = "csdl.track.input.audiostream";

        /// <summary>The table id of the <see cref="File.IOStream" /> this track holds a reference on.</summary>
        internal const string InputIOStreamProperty = "csdl.track.input.iostream";

        // internal Track(NativePtr<Opaque.SdlTrack> handle, bool ownsHandle, Mixer owner)
        //     : this(handle, ownsHandle ? HandleKind.Owned : HandleKind.Borrowed, owner.AsOwner) { }

        internal Track(NativePtr<Opaque.SdlTrack> handle, HandleKind kind, OwnerId owner = default) {
            _id = HandleTable.Acquire(handle, kind, owner);
        }

        /// <summary>The table identity of this track.</summary>
        internal HandleId<Opaque.SdlTrack> TableId => _id;

        /// <summary>The <see cref="CallbackRegistry" /> key of this track's cooked-mix callback.</summary>
        internal static string CookedCallbackIdFor(nint track) {
            return $"trackcooked:{track}";
        }

        /// <inheritdoc cref="CookedCallbackIdFor" />
        internal static string RawCallbackIdFor(nint track) {
            return $"trackraw:{track}";
        }

        /// <inheritdoc cref="CookedCallbackIdFor" />
        internal static string StoppedCallbackIdFor(nint track) {
            return $"trackstopped:{track}";
        }

        /// <summary>
        ///     The live pointer behind this handle.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     The track is gone, or the mixer that created it is.
        /// </exception>
        internal NativePtr<Opaque.SdlTrack> Handle => HandleTable.Resolve(_id);

        /// <inheritdoc cref="INativeHandle.NativePointer" />
        /// <remarks>Returns <c>0</c> rather than throwing once the track is gone.</remarks>
        public nint NativePointer => HandleTable.PointerOrZero(_id);

        /// <inheritdoc cref="INativeHandle.IsValid" />
        public bool IsValid => HandleTable.IsLive(_id);

        /// <inheritdoc cref="HandleId{T}.IsDefault"/>
        public bool IsDefault => _id.IsDefault;

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyTrack" />
        public void Dispose() {
            if (!HandleTable.Release(_id, out NativePtr<Opaque.SdlTrack> handle, out bool ownsHandle)) return;
            if (!ownsHandle || handle.IsNull) return;

            // Before the track goes: the notes saying what this track owes are in its property set,
            // which SDL_mixer frees along with it.
            ReleaseInputs(handle);

            lock (CallbackLock) {
                SDL.SetTrackCookedCallback(handle, null!, IntPtr.Zero);
                SDL.SetTrackRawCallback(handle, null!, IntPtr.Zero);
                SDL.SetTrackStoppedCallback(handle, null!, IntPtr.Zero);
                CallbackRegistry.Unregister<TrackMixCallback, MIX_TrackMixCallbackNative>(CookedCallbackIdFor(handle.Ptr));
                CallbackRegistry.Unregister<TrackMixCallback, MIX_TrackMixCallbackNative>(RawCallbackIdFor(handle.Ptr));
                CallbackRegistry.Unregister<TrackStoppedCallback, MIX_TrackStoppedCallbackNative>(StoppedCallbackIdFor(handle.Ptr));
            }

            SDL.DestroyTrack(handle);
        }

        /// <summary>
        ///     Takes a reference on this track's new input and notes it in the track's property set,
        ///     giving back the reference the previous input held.
        /// </summary>
        internal void AdoptInput(CSDL.Audio.AudioStream stream) {
            uint properties = InputProperties();
            if (properties == 0) return;

            (long previousStream, long previousIO) = TakeNotes(properties);
            if (stream.AddRef()) {
                new NumberProperty(properties, InputAudioStreamProperty).Set(stream.TableId.Value);
            }
            Release(previousStream, previousIO);
        }

        /// <inheritdoc cref="AdoptInput(CSDL.Audio.AudioStream)" />
        internal void AdoptInput(File.IOStream io) {
            uint properties = InputProperties();
            if (properties == 0) return;

            (long previousStream, long previousIO) = TakeNotes(properties);
            //TODO does not work if (io.AddRef()) {
            //     new NumberProperty(properties, InputIOStreamProperty).Set(io.TableId.Value);
            // }
            Release(previousStream, previousIO);
        }

        private uint InputProperties() {
            uint properties = SDL.GetTrackProperties(Handle);
            if (properties == 0) Error.LogError(nameof(SDL.GetTrackProperties));
            return properties;
        }

        /// <summary>Gives back whichever input reference this track is holding, if any.</summary>
        internal void ReleaseInputs() {
            ReleaseInputs(Handle);
        }

        private static void ReleaseInputs(NativePtr<Opaque.SdlTrack> track) {
            uint properties = SDL.GetTrackProperties(track);
            if (properties == 0) return;

            (long stream, long io) = TakeNotes(properties);
            Release(stream, io);
        }

        /// <summary>
        ///     Reads both notes and clears them, so each reference is given back exactly once.
        /// </summary>
        private static (long Stream, long IO) TakeNotes(uint properties) {
            return (Take(properties, InputAudioStreamProperty), Take(properties, InputIOStreamProperty));
        }

        private static long Take(uint properties, string property) {
            NumberProperty slot = new NumberProperty(properties, property);
            long id = slot.Get(0);
            if (id != 0) slot.Clear();
            return id;
        }

        /// <remarks>A note whose handle is no longer live rebuilds into one that releases nothing.</remarks>
        private static void Release(long stream, long io) {
            throw new NotImplementedException();
            if (stream != 0) CSDL.Audio.AudioStream.FromTableId(stream).Dispose();
            //TODO if (io != 0) File.IOStream.FromTableId(io).Dispose();
        }

        /// <summary>Two wrappers around the same track are equal, whoever created them.</summary>
        public bool Equals(Track other) {
            return _id == other._id;
        }

        public override bool Equals(object? obj) {
            return obj is Track other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        public static bool operator ==(Track left, Track right) {
            return left.Equals(right);
        }

        public static bool operator !=(Track left, Track right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(Track)}({_id})";
        }
    }
}
