// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// A mixing engine: owns an audio output device (or generates PCM with no device at all) and
    /// plays <see cref="Track"/>s and one-shot <see cref="Audio"/> into it.
    /// </summary>
    public sealed class Mixer : NativeHandle<Opaque.SdlMixer> {
        private static bool _initialized;
        private static readonly object InitializationLock = new object();
        private readonly List<WeakReference<Internal.InvalidationRegistration>> _children = new List<WeakReference<Internal.InvalidationRegistration>>();
        private readonly object _callbackLock = new object();
        private string? _postMixCallbackId;

        static Mixer() {
            EnsureInitialized();
            Init.OnQuit += Quit;
        }

        internal static void EnsureInitialized() {
            lock (InitializationLock) {
                if (_initialized) return;
                SDL.Init().ThrowIfFalse(nameof(SDL.Init));
                _initialized = true;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.Quit"/>
        internal static void Quit() {
            lock (InitializationLock) {
                if (!_initialized) return;
                SDL.Quit();
                _initialized = false;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.Version"/>
        public static int Version => SDL.Version();

        /// <summary>
        /// Gets the major version of SDL_mixer this binding was generated against.
        /// </summary>
        public static uint MajorVersion => Macros.MixerMajorVersion;

        /// <summary>
        /// Gets the minor version of SDL_mixer this binding was generated against.
        /// </summary>
        public static uint MinorVersion => Macros.MixerMinorVersion;

        /// <summary>
        /// Gets the micro (patch) version of SDL_mixer this binding was generated against.
        /// </summary>
        public static uint MicroVersion => Macros.MixerMicroVersion;

        /// <inheritdoc cref="CSDL.Mixer.Macros.MixerVersionAtleast"/>
        public static bool VersionAtLeast(uint major, uint minor, uint micro) {
            return Macros.MixerVersionAtleast(major, minor, micro);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.FramesToMS"/>
        public static long FramesToMS(int sampleRate, long frames) {
            return SDL.FramesToMS(sampleRate, frames);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.MSToFrames"/>
        public static long MSToFrames(int sampleRate, long ms) {
            return SDL.MSToFrames(sampleRate, ms);
        }

        /// <summary>
        /// Opens a mixer bound to an audio output device.
        /// </summary>
        /// <param name="device">The audio device to open, e.g. <see cref="CSDL.Audio.Macros.AudioDeviceDefaultPlayback"/>.</param>
        /// <param name="spec">The format to mix and play at. Defaults to 48kHz 16-bit stereo if omitted.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateMixerDevice"/>
        public Mixer(CSDL.Audio.AudioDeviceID device, CSDL.Audio.AudioSpec? spec = null) {
            CSDL.Audio.AudioSpec resolvedSpec = spec ?? new CSDL.Audio.AudioSpec(CSDL.Audio.AudioFormats.S16, 48000, 2);
            Handle = SDL.CreateMixerDevice(device, in resolvedSpec).ThrowIfInvalid();
        }

        /// <summary>
        /// Creates a mixer with no audio device attached - useful for generating PCM data (see
        /// <see cref="Generate"/>) without ever opening real audio hardware.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateMixer"/>
        public Mixer(CSDL.Audio.AudioSpec spec) {
            Handle = SDL.CreateMixer(in spec).ThrowIfInvalid();
        }

        internal Mixer(NativePtr<Opaque.SdlMixer> handle, bool ownsHandle) : base(handle, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetMixerProperties"/>
        public MixerProperties? Properties {
            get {
                uint id = SDL.GetMixerProperties(Handle);
                if (id == 0) {
                    Error.LogError(nameof(SDL.GetMixerProperties));
                    return null;
                }
                return new MixerProperties(id);
            }
        }

        /// <summary>
        /// Opens a mixer on the system's default playback device.
        /// </summary>
        public static Mixer OpenDefaultDevice(CSDL.Audio.AudioSpec? spec = null) {
            return new Mixer(CSDL.Audio.Macros.AudioDeviceDefaultPlayback, spec);
        }

        /// <summary>
        /// Gets or sets the overall gain (volume) applied to everything this mixer plays.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetMixerGain"/>
        public float Gain {
            get => SDL.GetMixerGain(Handle);
            set => SDL.SetMixerGain(Handle, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the playback speed multiplier for everything this mixer plays.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetMixerFrequencyRatio"/>
        public float FrequencyRatio {
            get => SDL.GetMixerFrequencyRatio(Handle);
            set => SDL.SetMixerFrequencyRatio(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetMixerFormat"/>
        public bool GetFormat(out CSDL.Audio.AudioSpec spec) {
            return SDL.GetMixerFormat(Handle, out spec).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateTrack"/>
        public Track CreateTrack() {
            NativePtr<Opaque.SdlTrack> track = SDL.CreateTrack(Handle).ThrowIfInvalid();
            return new Track(track, true, this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateGroup"/>
        public Group CreateGroup() {
            NativePtr<Opaque.SdlGroup> group = SDL.CreateGroup(Handle).ThrowIfInvalid();
            return new Group(group, this);
        }

        /// <summary>
        /// Gets every currently-existing track carrying <paramref name="tag"/>, or every track this
        /// mixer owns if <paramref name="tag"/> is <see langword="null"/>. The returned tracks are
        /// borrowed handles - the mixer still owns them, so do not dispose them.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetTaggedTracks"/>
        public Track[] GetTaggedTracks(string? tag = null) {
            IntPtr tracks = SDL.GetTaggedTracks(Handle, tag, out int count);
            if (tracks == IntPtr.Zero) {
                Error.LogError(nameof(GetTaggedTracks));
                return Array.Empty<Track>();
            }

            Track[] result = new Track[count];
            for (int i = 0; i < count; i++) {
                IntPtr trackPtr = Marshal.ReadIntPtr(tracks, i * IntPtr.Size);
                result[i] = new Track(trackPtr, false, this);
            }

            Memory.Free(tracks);
            return result;
        }

        /// <summary>
        /// Plays <paramref name="audio"/> once, on an internally-managed track. Use
        /// <see cref="CreateTrack"/> instead if you need to pause, loop, or otherwise control
        /// playback afterward.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.PlayAudio"/>
        public bool Play(Audio audio) {
            ArgumentNullException.ThrowIfNull(audio);
            return SDL.PlayAudio(Handle, audio.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.PauseAllTracks"/>
        public bool PauseAll() {
            return SDL.PauseAllTracks(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.ResumeAllTracks"/>
        public bool ResumeAll() {
            return SDL.ResumeAllTracks(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.StopAllTracks"/>
        public bool StopAll(long fadeOutMs = 0) {
            return SDL.StopAllTracks(Handle, fadeOutMs).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.PlayTag"/>
        public bool PlayTag(string tag, PropertiesID options = default) {
            return SDL.PlayTag(Handle, tag, options).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.PauseTag"/>
        public bool PauseTag(string tag) {
            return SDL.PauseTag(Handle, tag).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.ResumeTag"/>
        public bool ResumeTag(string tag) {
            return SDL.ResumeTag(Handle, tag).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.StopTag"/>
        public bool StopTag(string tag, long fadeOutMs = 0) {
            return SDL.StopTag(Handle, tag, fadeOutMs).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetTagGain"/>
        public bool SetTagGain(string tag, float gain) {
            return SDL.SetTagGain(Handle, tag, gain).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetPostMixCallback"/>
        public bool SetPostMixCallback(PostMixCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            string id = $"MixerPostMix:{Guid.NewGuid()}";
            MIX_PostMixCallbackNative native = PostMixCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(id, callback, native, userData);
            lock (_callbackLock) {
                bool ok = SDL.SetPostMixCallback(Handle, native, reg.userdataPtr).LogIfFalse();
                if (!ok) {
                    CallbackRegistry.Unregister<PostMixCallback, MIX_PostMixCallbackNative>(id);
                    return false;
                }
                if (_postMixCallbackId is not null) {
                    CallbackRegistry.Unregister<PostMixCallback, MIX_PostMixCallbackNative>(_postMixCallbackId);
                }
                _postMixCallbackId = id;
                return true;
            }
        }

        /// <summary>Removes the mixer post-mix callback.</summary>
        public bool ClearPostMixCallback() {
            lock (_callbackLock) {
                bool ok = SDL.SetPostMixCallback(Handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok && _postMixCallbackId is not null) {
                    CallbackRegistry.Unregister<PostMixCallback, MIX_PostMixCallbackNative>(_postMixCallbackId);
                    _postMixCallbackId = null;
                }
                return ok;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LockMixer"/>
        public void Lock() {
            SDL.LockMixer(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.UnlockMixer"/>
        public void Unlock() {
            SDL.UnlockMixer(Handle);
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.Generate"/>
        public int Generate(byte[] buffer) {
            ArgumentNullException.ThrowIfNull(buffer);
            unsafe {
                fixed (byte* ptr = buffer) {
                    return SDL.Generate(Handle, (IntPtr)ptr, buffer.Length).LogIfInvalid(-1);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.Generate"/>
        public int Generate(byte[] buffer, int length) {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            if (length > buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(length), "The requested length exceeds the buffer length.");
            }
            unsafe {
                fixed (byte* ptr = buffer) {
                    return SDL.Generate(Handle, (IntPtr)ptr, length).LogIfInvalid(-1);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.Generate"/>
        public int Generate(NativePtr<byte> buffer, int length) {
            return SDL.Generate(Handle, buffer, length).LogIfInvalid(-1);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyMixer"/>
        protected override void DisposeResource() {
            ClearPostMixCallback();
            SDL.DestroyMixer(Handle);
            foreach (WeakReference<Internal.InvalidationRegistration> child in _children) {
                if (child.TryGetTarget(out Internal.InvalidationRegistration? target)) {
                    target.Invalidate();
                }
            }
            _children.Clear();
        }

        internal void RegisterChild(Internal.InvalidationRegistration child) {
            _children.Add(new WeakReference<Internal.InvalidationRegistration>(child));
        }
    }
}
