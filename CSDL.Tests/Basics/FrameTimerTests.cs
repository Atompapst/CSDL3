// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL3.Tests.TestSupport;
using Sdl = CSDL;

namespace CSDL3.Tests.Basics {
    [Collection(SdlCollection.Name)]
    public class FrameTimerTests {
        [Fact]
        public void Uncapped_EndDoesNotWait() {
            Sdl.FrameTimer timer = new Sdl.FrameTimer();

            timer.Begin();
            ulong before = Sdl.Timer.GetTicksNs();
            timer.End();
            ulong elapsedNs = Sdl.Timer.GetTicksNs() - before;

            Assert.True(elapsedNs < 1_000_000, $"End() with TargetFps=0 should not wait, took {elapsedNs}ns");
        }

        [Fact]
        public void TargetFps_EndWaitsOutTheRemainingFrameBudget() {
            const uint targetFps = 100; // 10ms budget
            Sdl.FrameTimer timer = new Sdl.FrameTimer(targetFps);

            timer.Begin();
            ulong before = Sdl.Timer.GetTicksNs();
            timer.End();
            double elapsedMs = (Sdl.Timer.GetTicksNs() - before) / 1_000_000.0;

            Assert.True(elapsedMs >= 9.0, $"expected End() to wait close to 10ms, only took {elapsedMs}ms");
        }

        [Fact]
        public void TargetFps_FrameThatAlreadyExceedsBudget_EndDoesNotWait() {
            const uint targetFps = 1000; // 1ms budget
            Sdl.FrameTimer timer = new Sdl.FrameTimer(targetFps);

            timer.Begin();
            System.Threading.Thread.Sleep(5); // already over budget
            ulong before = Sdl.Timer.GetTicksNs();
            timer.End();
            ulong elapsedNs = Sdl.Timer.GetTicksNs() - before;

            Assert.True(elapsedNs < 1_000_000, $"End() should not wait once the frame already ran over budget, took {elapsedNs}ns");
        }

        [Fact]
        public void DeltaTimeSeconds_IsZeroUntilASecondFrameBegins() {
            Sdl.FrameTimer timer = new Sdl.FrameTimer();

            Assert.Equal(0.0, timer.DeltaTimeSeconds);
            timer.Begin();
            Assert.Equal(0.0, timer.DeltaTimeSeconds);
        }

        [Fact]
        public void DeltaTimeSeconds_TracksRealTimeBetweenBeginCalls() {
            Sdl.FrameTimer timer = new Sdl.FrameTimer();

            timer.Begin();
            System.Threading.Thread.Sleep(20);
            timer.End();
            timer.Begin();

            Assert.True(timer.DeltaTimeSeconds >= 0.018, $"expected DeltaTimeSeconds close to 0.02s, got {timer.DeltaTimeSeconds}");
        }
    }
}
