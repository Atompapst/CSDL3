// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.File;
using CSDL.Video;

namespace CSDL.Image {
    public class Animation : NativeHandle<AnimationData> {
        // Always populated by BuildCache, which every constructor calls before this instance is
        // otherwise usable; default to empty so the fields are never observably null.
        private Surface[] _frames = Array.Empty<Surface>();
        private int[] _delays = Array.Empty<int>();

        private int _elapsedMs;
        private int _frameIndex;
        private int _loopDuration; // sum of delays

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAnimation"/>
        public Animation(string path) {
            LoadAnimation(path);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAnimation_IO"/>
        public Animation(IOStream stream) {
            LoadAnimation(stream);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAnimationTyped_IO"/>
        public Animation(IOStream stream, ImageType type) {
            LoadAnimation(stream, type);
        }
        internal Animation(NativePtr<AnimationData> handle) {
            Handle = handle;
            BuildCache();
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Count { get; private set; }

        public ReadOnlySpan<Surface> Frames => _frames;
        public ReadOnlySpan<int> Delays => _delays;

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAnimation"/>
        private Animation LoadAnimation(string path) {
            Handle = SDL.LoadAnimation(path).ThrowIfInvalid();
            BuildCache();
            return this;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAnimation_IO"/>
        private Animation LoadAnimation(IOStream src, bool closeAfter = false) {
            Handle = SDL.LoadAnimation_IO(src.Handle, closeAfter).ThrowIfInvalid();
            BuildCache();
            return this;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.LoadAnimationTyped_IO"/>
        private Animation LoadAnimation(IOStream src, ImageType type, bool closeAfter = false) {
            Handle = SDL.LoadAnimationTyped_IO(src.Handle, closeAfter, type.ToString()).ThrowIfInvalid();
            BuildCache();
            return this;
        }

        private void ValidateIndex(int index) {
            if ((uint)index >= (uint)Count) throw new SDLException("Invalid frame index");
        }

        /// <summary>
        ///     Gets a cached frame by index.
        /// </summary>
        public Surface GetFrame(int index) {
            ValidateIndex(index);
            return _frames[index];
        }

        /// <summary>
        ///     Gets the delay for a frame by index in milliseconds.
        /// </summary>
        public int GetDelay(int index) {
            ValidateIndex(index);
            return _delays[index];
        }

        /// <summary>
        ///     Advances playback using elapsed time and returns the current frame.
        /// </summary>
        /// <param name="deltaMs">Elapsed time in milliseconds since the last call.</param>
        /// <param name="delayMs">The current frame's delay in milliseconds.</param>
        /// <returns>The current frame, or null if the animation has no frames.</returns>
        public Surface? GetNextFrame(int deltaMs, out int delayMs) {
            if (Count == 0) {
                delayMs = 0;
                return null;
            }

            int remainingInCurrent = _delays[_frameIndex] - _elapsedMs;

            int t = deltaMs;

            if (t < remainingInCurrent) {
                // Same Frame
                _elapsedMs += t;
                delayMs = _delays[_frameIndex];
                return _frames[_frameIndex];
            }

            t -= remainingInCurrent;
            int idx = _frameIndex + 1;
            if (idx == Count) idx = 0;

            // Skip whole loops in one modulo, then at most one pass across frames
            if (t >= _loopDuration) {
                t %= _loopDuration;
            }

            // Walk frames until we locate the one containing the leftover time t
            while (true) {
                int d = _delays[idx];
                if (t < d) {
                    _frameIndex = idx;
                    _elapsedMs = t;
                    delayMs = d;
                    return _frames[idx];
                }
                t -= d;
                idx++;
                if (idx == Count) idx = 0;
            }
        }

        /// <summary>
        ///     Advances playback using elapsed time in seconds and returns the current frame.
        /// </summary>
        /// <param name="deltaSeconds">Elapsed time in seconds since the last call.</param>
        /// <param name="delaySeconds">The current frame's delay in seconds.</param>
        /// <returns>The current frame, or null if the animation has no frames.</returns>
        public Surface? GetNextFrame(float deltaSeconds, out float delaySeconds) {
            int deltaMs = (int)(deltaSeconds * 1000f);
            Surface? surface = GetNextFrame(deltaMs, out int delay);
            delaySeconds = delay / 1000f;
            return surface;
        }

        /// <summary>
        ///     Resets playback to the first frame.
        /// </summary>
        public void ResetPlayback() {
            _frameIndex = 0;
            _elapsedMs = 0;
        }

        private void BuildCache() {
            Width = Ref.W;
            Height = Ref.H;
            Count = Ref.Count;

            if (Count <= 0) {
                _frames = Array.Empty<Surface>();
                _delays = Array.Empty<int>();
                _loopDuration = 0;
                return;
            }

            _frames = new Surface[Count];
            _delays = new int[Count];

            NativePtr<nint> framesPtr = Ref.Frames;
            for (int i = 0; i < Count; i++) {
                _frames[i] = new Surface(framesPtr[i]);
            }

            NativePtr<int> delaysPtr = Ref.Delays;
            for (int i = 0; i < Count; i++) {
                int d = delaysPtr[i];
                if (d <= 0) d = 1; // normalize
                _delays[i] = d;
            }

            // loop duration
            int sum = 0;
            for (int i = 0; i < Count; i++) {
                sum += _delays[i];
            }
            // if sum 0, set to count (cause normalization)
            _loopDuration = sum > 0 ? sum : Count;

            // Reset playback state
            _frameIndex = 0;
            _elapsedMs = 0;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.FreeAnimation"/>
        protected override void DisposeResource() {
            _frames.Dispose();
            _frames = Array.Empty<Surface>();
            _delays = Array.Empty<int>();
            SDL.FreeAnimation(Handle.Ptr);
        }
    }

}
