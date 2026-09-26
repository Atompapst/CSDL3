// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;

namespace CSDL.Audio {
    public readonly partial struct AudioStream {
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.CreateAudioStream"/>
        public AudioStream(AudioSpec? srcSpec, AudioSpec? dstSpec)
            : this(Create(srcSpec, dstSpec), HandleKind.Owned) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.CreateAudioStream"/>
        private static unsafe NativePtr<Opaque.SdlAudioStream> Create(AudioSpec? srcSpec, AudioSpec? dstSpec) {
            Init.InitSubSystem(InitFlags.Audio);
            AudioSpec src = srcSpec.GetValueOrDefault();
            AudioSpec dst = dstSpec.GetValueOrDefault();
            AudioSpec* srcPtr = srcSpec.HasValue ? &src : null;
            AudioSpec* dstPtr = dstSpec.HasValue ? &dst : null;
            return SDL.CreateAudioStreamNullable(srcPtr, dstPtr).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.FlushAudioStream"/>
        public bool Flush() {
            return SDL.FlushAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ClearAudioStream"/>
        public bool Clear() {
            return SDL.ClearAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PauseAudioStreamDevice"/>
        public bool PauseDevice() {
            return SDL.PauseAudioStreamDevice(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ResumeAudioStreamDevice"/>
        public bool ResumeDevice() {
            return SDL.ResumeAudioStreamDevice(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamFormat"/>
        public unsafe bool SetAudioStreamFormat(AudioSpec? srcSpec, AudioSpec? dstSpec) {
            AudioSpec src = srcSpec.GetValueOrDefault();
            AudioSpec dst = dstSpec.GetValueOrDefault();
            AudioSpec* srcPtr = srcSpec.HasValue ? &src : null;
            AudioSpec* dstPtr = dstSpec.HasValue ? &dst : null;
            return SDL.SetAudioStreamFormatNullable(Handle, srcPtr, dstPtr).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.LockAudioStream"/>
        public AudioStreamLock AcquireLock() {
            if (!SDL.LockAudioStream(Handle).LogIfFalse()) {
                return default;
            }

            return new AudioStreamLock(this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.LockAudioStream"/>
        public bool Lock() {
            return SDL.LockAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnlockAudioStream"/>
        public bool Unlock() {
            return SDL.UnlockAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStream"/>
        public void Unbind() {
            SDL.UnbindAudioStream(Handle);
        }
    }

    public readonly struct AudioStreamLock : IDisposable {
        private readonly AudioStream _stream;

        internal AudioStreamLock(AudioStream stream) {
            _stream = stream;
        }

        public void Dispose() {
            if (_stream.IsValid) {
                _stream.Unlock();
            }
        }
    }
}
