// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Threading;
using CSDL.Threads;
using CSDL3.Tests.TestSupport;
using Sdl = CSDL;

namespace CSDL3.Tests.Basics {
    [Collection(SdlCollection.Name)]
    public class CallbackLifetimeNativeTests {
        [Fact]
        public void Timers_CanRunConcurrently() {
            using ManualResetEventSlim firstFired = new ManualResetEventSlim();
            using ManualResetEventSlim secondFired = new ManualResetEventSlim();
            using Sdl.Timer first = new Sdl.Timer(1u, (_, _, _) => {
                firstFired.Set();
                return 0;
            });
            using Sdl.Timer second = new Sdl.Timer(1u, (_, _, _) => {
                secondFired.Set();
                return 0;
            });

            Assert.True(firstFired.Wait(TimeSpan.FromSeconds(1)));
            Assert.True(secondFired.Wait(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public void Timer_CallbackException_IsContainedAndCancelsTimer() {
            using ManualResetEventSlim fired = new ManualResetEventSlim();
            using Sdl.Timer timer = new Sdl.Timer(1u, (_, _, _) => {
                fired.Set();
                throw new InvalidOperationException("Expected test exception.");
            });

            Assert.True(fired.Wait(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public void ManagedThread_CallbackException_IsContainedAndReturnsZero() {
            using CSDL.Threads.Thread thread = new CSDL.Threads.Thread(
                _ => throw new InvalidOperationException("Expected test exception."),
                "CSDL3-throwing-test-thread"
            );

            thread.Wait(out int status);

            Assert.Equal(0, status);
        }

        [Fact]
        public void PropertyGroup_DisposeThroughIDisposable_RunsPropertyCleanup() {
            int cleanupCalls = 0;
            ThreadProperties properties = new ThreadProperties();
            Assert.True(properties.Pointer("CSDL3_TEST_POINTER").SetWithCleanup(1, (_, _) => Interlocked.Increment(ref cleanupCalls)));

            IDisposable disposable = properties;
            disposable.Dispose();

            Assert.Equal(1, cleanupCalls);
        }
    }
}
