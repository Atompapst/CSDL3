// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// A single playable slot within a <see cref="Mixer"/>: assign <see cref="Audio"/> to it, then
    /// play/pause/resume/stop and loop it independently of the mixer's other tracks.
    /// </summary>
    public sealed class Track : NativeHandle<Opaque.SdlTrack> {
        private readonly object _callbackLock = new object();
        private CSDL.Audio.AudioStream? _inputStream;
        private File.IOStream? _inputIO;
        private string? _cookedCallbackId;
        private string? _rawCallbackId;
        private string? _stoppedCallbackId;

        internal Track(NativePtr<Opaque.SdlTrack> handle, bool ownsHandle, Mixer? owner) : base(handle, ownsHandle) {
            owner?.RegisterChild(Invalidation);
        }

        /// <summary>
        /// The mixer that was passed to <see cref="Mixer.CreateTrack"/> to create this track. The
        /// returned wrapper is a borrowed handle - do not dispose it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackMixer"/>
        public Mixer? Mixer {
            get {
                NativePtr<Opaque.SdlMixer> mixer = SDL.GetTrackMixer(Handle);
                if (mixer.IsNull) {
                    Error.LogError(nameof(SDL.GetTrackMixer));
                    return null;
                }
                return new Mixer(mixer, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackProperties"/>
        public TrackProperties? Properties {
            get {
                uint id = SDL.GetTrackProperties(Handle);
                if (id == 0) {
                    Error.LogError(nameof(SDL.GetTrackProperties));
                    return null;
                }
                return new TrackProperties(id);
            }
        }

        /// <summary>
        /// The audio assigned through <see cref="SetAudio"/>, as a borrowed handle - do not dispose
        /// it. Null if this track has no input, or an input that isn't a <see cref="Audio"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackAudio"/>
        public Audio? GetAudio() {
            NativePtr<Opaque.SdlAudio> audio = SDL.GetTrackAudio(Handle);
            return audio.IsNull ? null : new Audio(audio, false);
        }

        /// <summary>
        /// The stream assigned through <see cref="SetAudioStream"/>, as a borrowed handle - do not
        /// dispose it. Null if this track has no input, or an input that isn't an
        /// <see cref="CSDL.Audio.AudioStream"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackAudioStream"/>
        public CSDL.Audio.AudioStream? GetAudioStream() {
            NativePtr<Opaque.SdlAudioStream> stream = SDL.GetTrackAudioStream(Handle);
            return stream.IsNull ? null : new CSDL.Audio.AudioStream(stream.Ptr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackAudio"/>
        public bool SetAudio(Audio? audio) {
            bool ok = SDL.SetTrackAudio(Handle, audio?.Handle ?? NativePtr<Opaque.SdlAudio>.Zero).LogIfFalse();
            if (ok) ClearInputReferences();
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackAudioStream"/>
        public bool SetAudioStream(CSDL.Audio.AudioStream stream) {
            ArgumentNullException.ThrowIfNull(stream);
            bool ok = SDL.SetTrackAudioStream(Handle, stream.Handle).LogIfFalse();
            if (ok) {
                if (!ReferenceEquals(_inputStream, stream)) {
                    ReleaseInputStream();
                    stream.AcquireTrackLease();
                }
                _inputStream = stream;
                ReleaseInputIO();
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackIOStream"/>
        public bool SetIOStream(File.IOStream io, bool closeIo = false) {
            ArgumentNullException.ThrowIfNull(io);
            bool ok = SDL.SetTrackIOStream(Handle, io.Handle, closeIo).LogIfFalse();
            if (closeIo) io.Invalidate();
            if (ok) {
                ReleaseInputStream();
                bool sameIO = ReferenceEquals(_inputIO, io);
                if (closeIo || !sameIO) {
                    ReleaseInputIO();
                    if (!closeIo) io.AcquireTrackLease();
                }
                _inputIO = closeIo ? null : io;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackRawIOStream"/>
        public bool SetRawIOStream(File.IOStream io, CSDL.Audio.AudioSpec spec, bool closeIo = false) {
            ArgumentNullException.ThrowIfNull(io);
            bool ok = SDL.SetTrackRawIOStream(Handle, io.Handle, in spec, closeIo).LogIfFalse();
            if (closeIo) io.Invalidate();
            if (ok) {
                ReleaseInputStream();
                bool sameIO = ReferenceEquals(_inputIO, io);
                if (closeIo || !sameIO) {
                    ReleaseInputIO();
                    if (!closeIo) io.AcquireTrackLease();
                }
                _inputIO = closeIo ? null : io;
            }
            return ok;
        }

        /// <summary>Removes the track's current audio input.</summary>
        public bool ClearInput() => SetAudio(null);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackGroup"/>
        public bool SetGroup(Group? group) {
            return SDL.SetTrackGroup(Handle, group?.Handle ?? default).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackOutputChannelMap"/>
        public bool SetOutputChannelMap(int[]? channelMap) {
            return channelMap is null
                ? SDL.SetTrackOutputChannelMap(Handle, NativePtr<int>.Zero, 0).LogIfFalse()
                : SDL.SetTrackOutputChannelMap(Handle, channelMap, channelMap.Length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.PlayTrack"/>
        public bool Play(PropertiesID options = default) {
            return SDL.PlayTrack(Handle, options).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.StopTrack"/>
        public bool Stop(long fadeOutFrames = 0) {
            return SDL.StopTrack(Handle, fadeOutFrames).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.PauseTrack"/>
        public bool Pause() {
            return SDL.PauseTrack(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.ResumeTrack"/>
        public bool Resume() {
            return SDL.ResumeTrack(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.TrackPlaying"/>
        public bool IsPlaying => SDL.TrackPlaying(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.TrackPaused"/>
        public bool IsPaused => SDL.TrackPaused(Handle);

        /// <summary>
        /// Gets or sets the number of times this track repeats after its first playthrough; -1
        /// loops forever, 0 plays once.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackLoops"/>
        public int Loops {
            get => SDL.GetTrackLoops(Handle);
            set => SDL.SetTrackLoops(Handle, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets whether this track loops forever, via <see cref="Loops"/>.
        /// </summary>
        public bool Infinite {
            get => Loops == -1;
            set => Loops = value ? -1 : 0;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackGain"/>
        public float Gain {
            get => SDL.GetTrackGain(Handle);
            set => SDL.SetTrackGain(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackFrequencyRatio"/>
        public float FrequencyRatio {
            get => SDL.GetTrackFrequencyRatio(Handle);
            set => SDL.SetTrackFrequencyRatio(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackPlaybackPosition"/>
        public long PlaybackPositionFrames {
            get => SDL.GetTrackPlaybackPosition(Handle);
            set => SDL.SetTrackPlaybackPosition(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackRemaining"/>
        public long RemainingFrames => SDL.GetTrackRemaining(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackFadeFrames"/>
        public long FadeFrames => SDL.GetTrackFadeFrames(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackStereo"/>
        public bool SetStereo(StereoGains? gains) {
            StereoGains value = gains.GetValueOrDefault();
            unsafe {
                StereoGains* ptr = gains.HasValue ? &value : null;
                return SDL.SetTrackStereoNullable(Handle, ptr).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrack3DPosition"/>
        public bool Set3DPosition(Point3D? position) {
            Point3D value = position.GetValueOrDefault();
            unsafe {
                Point3D* ptr = position.HasValue ? &value : null;
                return SDL.SetTrack3DPositionNullable(Handle, ptr).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrack3DPosition"/>
        public bool Get3DPosition(out Point3D position) {
            position = default;
            return SDL.GetTrack3DPosition(Handle, ref position).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.TagTrack"/>
        public bool AddTag(string tag) {
            return SDL.TagTrack(Handle, tag).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.UntagTrack"/>
        public void RemoveTag(string tag) {
            SDL.UntagTrack(Handle, tag);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackTags"/>
        public string[] GetTags() {
            IntPtr tags = SDL.GetTrackTags(Handle, out int count);
            if (tags == IntPtr.Zero) {
                Error.LogError(nameof(GetTags));
                return Array.Empty<string>();
            }

            string[] result = NativeStringArray.ToArray(tags, count);
            Memory.Free(tags);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.TrackFramesToMS"/>
        public long FramesToMS(long frames) {
            return SDL.TrackFramesToMS(Handle, frames);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.TrackMSToFrames"/>
        public long MSToFrames(long ms) {
            return SDL.TrackMSToFrames(Handle, ms);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackCookedCallback"/>
        public bool SetCookedCallback(TrackMixCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            string id = $"TrackCooked:{Guid.NewGuid()}";
            MIX_TrackMixCallbackNative native = TrackMixCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(id, callback, native, userData);
            lock (_callbackLock) {
                bool ok = SDL.SetTrackCookedCallback(Handle, native, reg.userdataPtr).LogIfFalse();
                if (!ok) {
                    CallbackRegistry.Unregister<TrackMixCallback, MIX_TrackMixCallbackNative>(id);
                    return false;
                }
                UnregisterCookedCallback();
                _cookedCallbackId = id;
                return true;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackRawCallback"/>
        public bool SetRawCallback(TrackMixCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            string id = $"TrackRaw:{Guid.NewGuid()}";
            MIX_TrackMixCallbackNative native = TrackMixCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(id, callback, native, userData);
            lock (_callbackLock) {
                bool ok = SDL.SetTrackRawCallback(Handle, native, reg.userdataPtr).LogIfFalse();
                if (!ok) {
                    CallbackRegistry.Unregister<TrackMixCallback, MIX_TrackMixCallbackNative>(id);
                    return false;
                }
                UnregisterRawCallback();
                _rawCallbackId = id;
                return true;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackStoppedCallback"/>
        public bool SetStoppedCallback(TrackStoppedCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            string id = $"TrackStopped:{Guid.NewGuid()}";
            MIX_TrackStoppedCallbackNative native = TrackStoppedCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(id, callback, native, userData);
            lock (_callbackLock) {
                bool ok = SDL.SetTrackStoppedCallback(Handle, native, reg.userdataPtr).LogIfFalse();
                if (!ok) {
                    CallbackRegistry.Unregister<TrackStoppedCallback, MIX_TrackStoppedCallbackNative>(id);
                    return false;
                }
                UnregisterStoppedCallback();
                _stoppedCallbackId = id;
                return true;
            }
        }

        public bool ClearCookedCallback() => ClearCallback<MIX_TrackMixCallbackNative>((track, callback, userData) => SDL.SetTrackCookedCallback(track, callback, userData), UnregisterCookedCallback);
        public bool ClearRawCallback() => ClearCallback<MIX_TrackMixCallbackNative>((track, callback, userData) => SDL.SetTrackRawCallback(track, callback, userData), UnregisterRawCallback);
        public bool ClearStoppedCallback() => ClearCallback<MIX_TrackStoppedCallbackNative>((track, callback, userData) => SDL.SetTrackStoppedCallback(track, callback, userData), UnregisterStoppedCallback);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyTrack"/>
        protected override void DisposeResource() {
            lock (_callbackLock) {
                SDL.SetTrackCookedCallback(Handle, null!, IntPtr.Zero);
                SDL.SetTrackRawCallback(Handle, null!, IntPtr.Zero);
                SDL.SetTrackStoppedCallback(Handle, null!, IntPtr.Zero);
                UnregisterCookedCallback();
                UnregisterRawCallback();
                UnregisterStoppedCallback();
            }
            SDL.DestroyTrack(Handle);
            ClearInputReferences();
        }

        private bool ClearCallback<TNative>(Func<NativePtr<Opaque.SdlTrack>, TNative, IntPtr, CBool> set, Action unregister) where TNative : Delegate {
            lock (_callbackLock) {
                bool ok = set(Handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok) unregister();
                return ok;
            }
        }

        private void ClearInputReferences() {
            ReleaseInputStream();
            ReleaseInputIO();
        }

        private void ReleaseInputStream() {
            CSDL.Audio.AudioStream? stream = _inputStream;
            _inputStream = null;
            stream?.ReleaseTrackLease();
        }

        private void ReleaseInputIO() {
            File.IOStream? io = _inputIO;
            _inputIO = null;
            io?.ReleaseTrackLease();
        }

        protected override void InvalidateResource() {
            UnregisterCookedCallback();
            UnregisterRawCallback();
            UnregisterStoppedCallback();
            ClearInputReferences();
        }

        private void UnregisterCookedCallback() {
            if (_cookedCallbackId is null) return;
            CallbackRegistry.Unregister<TrackMixCallback, MIX_TrackMixCallbackNative>(_cookedCallbackId);
            _cookedCallbackId = null;
        }

        private void UnregisterRawCallback() {
            if (_rawCallbackId is null) return;
            CallbackRegistry.Unregister<TrackMixCallback, MIX_TrackMixCallbackNative>(_rawCallbackId);
            _rawCallbackId = null;
        }

        private void UnregisterStoppedCallback() {
            if (_stoppedCallbackId is null) return;
            CallbackRegistry.Unregister<TrackStoppedCallback, MIX_TrackStoppedCallbackNative>(_stoppedCallbackId);
            _stoppedCallbackId = null;
        }
    }
}
