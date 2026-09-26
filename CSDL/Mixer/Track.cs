// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// A single playable slot within a <see cref="Mixer"/>: assign <see cref="Audio"/> to it, then
    /// play/pause/resume/stop and loop it independently of the mixer's other tracks.
    /// </summary>
    public readonly partial struct Track {
        /// <summary>
        /// The mixer that was passed to <see cref="Mixer.CreateTrack"/> to create this track. The
        /// returned wrapper is a borrowed handle - do not dispose it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackMixer"/>
        /// <returns>A borrowed view of the owning mixer, or <see langword="default"/> if there is none.</returns>
        public Mixer Mixer {
            get {
                NativePtr<Opaque.SdlMixer> mixer = SDL.GetTrackMixer(Handle);
                if (mixer.IsNull) {
                    Error.LogError(nameof(SDL.GetTrackMixer));
                }
                return new Mixer(mixer, false);
                //TODO return new Mixer(mixer, HandleKind.Borrowed);
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
        /// it. <see langword="default"/> if this track has no input, or an input that isn't a
        /// <see cref="Audio"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackAudio"/>
        public Audio GetAudio() {
            
            return new Audio(SDL.GetTrackAudio(Handle), false);
            //TODO return new Audio(SDL.GetTrackAudio(Handle), HandleKind.Borrowed);
        }

        /// <summary>
        /// The stream assigned through <see cref="SetAudioStream"/>, as a borrowed handle - do not
        /// dispose it. <see langword="default"/> if this track has no input, or an input that isn't
        /// an <see cref="CSDL.Audio.AudioStream"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTrackAudioStream"/>
        public CSDL.Audio.AudioStream GetAudioStream() {
            return new CSDL.Audio.AudioStream(SDL.GetTrackAudioStream(Handle), HandleKind.Borrowed);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackAudio"/>
        /// <param name="audio">the clip to play, or <see langword="default"/> to clear the input.</param>
        public bool SetAudio(Audio audio) {
            //TODO NativePtr<Opaque.SdlAudio> handle = audio.IsDefault ? NativePtr<Opaque.SdlAudio>.Zero : audio.Handle;
            NativePtr<Opaque.SdlAudio> handle = audio.IsValid ? NativePtr<Opaque.SdlAudio>.Zero : audio.Handle;
            if (!SDL.SetTrackAudio(Handle, handle).LogIfFalse()) return false;

            // An Audio is a whole decoded clip the caller owns outright.
            ReleaseInputs();
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackAudioStream"/>
        public bool SetAudioStream(CSDL.Audio.AudioStream stream) {
            stream.ThrowIfInvalid(nameof(stream));
            if (!SDL.SetTrackAudioStream(Handle, stream.Handle).LogIfFalse()) return false;
            AdoptInput(stream);
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackIOStream"/>
        /// <inheritdoc cref="SetAudioStream"/>
        public bool SetIOStream(File.IOStream io, bool closeIo = false) {
            io.ThrowIfInvalid(nameof(io));
            bool ok = SDL.SetTrackIOStream(Handle, io.Handle, closeIo).LogIfFalse();
            if (closeIo) io.Invalidate();
            if (ok) {
                if (closeIo) ReleaseInputs();
                else AdoptInput(io);
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackRawIOStream"/>
        public bool SetIOStreamRaw(File.IOStream io, CSDL.Audio.AudioSpec spec, bool closeIo = false) {
            io.ThrowIfInvalid(nameof(io));
            bool ok = SDL.SetTrackRawIOStream(Handle, io.Handle, in spec, closeIo).LogIfFalse();
            if (closeIo) io.Invalidate();
            if (ok) {
                if (closeIo) ReleaseInputs();
                else AdoptInput(io);
            }
            return ok;
        }

        /// <summary>Removes the track's current audio input.</summary>
        public bool ClearInput() => SetAudio(default);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackGroup"/>
        /// <param name="group">the group to join, or <see langword="default"/> to leave any group.</param>
        public bool SetGroup(Group group) {
            //TODO return SDL.SetTrackGroup(Handle, group.IsDefault ? default : group.Handle).LogIfFalse();
            return SDL.SetTrackGroup(Handle, group.IsValid ? default : group.Handle).LogIfFalse();
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
            NativePtr<Opaque.SdlTrack> track = Handle;
            return Install(
                SetTrackCooked, track, TrackMixCallbackWrapper.Create(callback), callback, userData,
                CookedCallbackIdFor(track.Ptr));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackRawCallback"/>
        public bool SetRawCallback(TrackMixCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            NativePtr<Opaque.SdlTrack> track = Handle;
            return Install(
                SetTrackRaw, track, TrackMixCallbackWrapper.Create(callback), callback, userData,
                RawCallbackIdFor(track.Ptr));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTrackStoppedCallback"/>
        public bool SetStoppedCallback(TrackStoppedCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            NativePtr<Opaque.SdlTrack> track = Handle;
            return Install(
                SetTrackStopped, track, TrackStoppedCallbackWrapper.Create(callback), callback, userData,
                StoppedCallbackIdFor(track.Ptr));
        }

        public bool ClearCookedCallback() {
            NativePtr<Opaque.SdlTrack> track = Handle;
            return Clear<TrackMixCallback, MIX_TrackMixCallbackNative>(SetTrackCooked, track, CookedCallbackIdFor(track.Ptr));
        }

        public bool ClearRawCallback() {
            NativePtr<Opaque.SdlTrack> track = Handle;
            return Clear<TrackMixCallback, MIX_TrackMixCallbackNative>(SetTrackRaw, track, RawCallbackIdFor(track.Ptr));
        }

        public bool ClearStoppedCallback() {
            NativePtr<Opaque.SdlTrack> track = Handle;
            return Clear<TrackStoppedCallback, MIX_TrackStoppedCallbackNative>(SetTrackStopped, track, StoppedCallbackIdFor(track.Ptr));
        }

        // Named methods rather than lambdas: a lambda in a struct that touched Handle would capture
        // `this` and not compile (CS1673), and these capture nothing to begin with.
        private static CBool SetTrackCooked(NativePtr<Opaque.SdlTrack> track, MIX_TrackMixCallbackNative callback, IntPtr userData) {
            return SDL.SetTrackCookedCallback(track, callback, userData);
        }

        private static CBool SetTrackRaw(NativePtr<Opaque.SdlTrack> track, MIX_TrackMixCallbackNative callback, IntPtr userData) {
            return SDL.SetTrackRawCallback(track, callback, userData);
        }

        private static CBool SetTrackStopped(NativePtr<Opaque.SdlTrack> track, MIX_TrackStoppedCallbackNative callback, IntPtr userData) {
            return SDL.SetTrackStoppedCallback(track, callback, userData);
        }

        /// <summary>
        ///     Hands <paramref name="native"/> to SDL_mixer and, if it takes it, files the registration
        ///     under <paramref name="id"/> - replacing whatever was there.
        /// </summary>
        /// <remarks>
        ///     The throwaway id exists because <paramref name="id"/> is derived from the track pointer
        ///     and is therefore still held by the callback being replaced, which
        ///     <see cref="CallbackRegistry.Register"/> refuses to duplicate. Freeing the old one first
        ///     would release its userdata while SDL_mixer could still be calling it.
        /// </remarks>
        private static bool Install<TPublic, TNative>(
            Func<NativePtr<Opaque.SdlTrack>, TNative, IntPtr, CBool> set,
            NativePtr<Opaque.SdlTrack> track,
            TNative native,
            TPublic callback,
            object? userData,
            string id)
            where TPublic : Delegate
            where TNative : Delegate {
            string staging = $"{id}:pending:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(staging, callback, native, userData);

            lock (CallbackLock) {
                if (!set(track, native, reg.userdataPtr).LogIfFalse()) {
                    CallbackRegistry.Unregister<TPublic, TNative>(staging);
                    return false;
                }

                CallbackRegistry.Unregister<TPublic, TNative>(id);
                return CallbackRegistry.UpdateId<TPublic, TNative>(staging, id);
            }
        }

        private static bool Clear<TPublic, TNative>(
            Func<NativePtr<Opaque.SdlTrack>, TNative, IntPtr, CBool> set,
            NativePtr<Opaque.SdlTrack> track,
            string id)
            where TPublic : Delegate
            where TNative : Delegate {
            lock (CallbackLock) {
                bool ok = set(track, null!, IntPtr.Zero).LogIfFalse();
                if (ok) CallbackRegistry.Unregister<TPublic, TNative>(id);
                return ok;
            }
        }
    }
}
