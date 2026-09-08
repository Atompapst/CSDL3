// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    ///     Paces a game loop to a target frame rate. Call <see cref="Begin"/> at the start of a frame
    ///     and <see cref="End"/> at the end; <see cref="End"/> sleeps out whatever's left of the frame
    ///     budget for <see cref="TargetFps"/>. <see cref="DeltaTimeSeconds"/> tracks real time between
    ///     frames (work plus wait), for driving physics/animation.
    /// </summary>
    public sealed class FrameTimer {
        private ulong _frameStartNs;
        private bool _hasPreviousFrame;

        /// <summary>
        ///     Target frames per second <see cref="End"/> paces to. 0 means uncapped - <see cref="End"/>
        ///     then only stops timing the frame without waiting.
        /// </summary>
        public uint TargetFps { get; set; }

        /// <summary>
        ///     Time elapsed since the previous <see cref="Begin"/> call, in seconds - including any
        ///     wait the previous <see cref="End"/> performed. 0 until a second frame has begun.
        /// </summary>
        public double DeltaTimeSeconds { get; private set; }

        public FrameTimer(uint targetFps = 0) {
            TargetFps = targetFps;
        }

        /// <summary>
        ///     Marks the start of a frame and updates <see cref="DeltaTimeSeconds"/> from the previous one.
        /// </summary>
        public void Begin() {
            ulong now = Timer.GetTicksNs();
            if (_hasPreviousFrame) {
                DeltaTimeSeconds = (now - _frameStartNs) / (double)Macros.NsPerSecond;
            }
            _frameStartNs = now;
            _hasPreviousFrame = true;
        }

        /// <summary>
        ///     Marks the end of a frame and, if <see cref="TargetFps"/> is nonzero, sleeps out the
        ///     remainder of the frame budget.
        /// </summary>
        public void End() {
            uint targetFps = TargetFps;
            if (targetFps == 0) return;

            ulong targetFrameNs = Macros.NsPerSecond / targetFps;
            ulong elapsed = Timer.GetTicksNs() - _frameStartNs;
            if (elapsed < targetFrameNs) {
                Timer.DelayPrecise(targetFrameNs - elapsed);
            }
        }
    }
}
