// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Threading;
using Sdl = CSDL;
using CSDL3.Tests.TestSupport;

namespace CSDL3.Tests.Basics {
    [Collection(SdlCollection.Name)]
    public class HintCallbackNativeTests {
        [Fact]
        public void RemoveCallback_StopsReceivingChangesForItsHint() {
            Sdl.Hints.Hint hint = Sdl.Hints.For($"CSDL3_TEST_HINT_{Guid.NewGuid():N}");
            int calls = 0;

            Sdl.HintCallback callback = (_, _, _, _) => Interlocked.Increment(ref calls);
            hint.AddCallback(callback);
            int callsBeforeRemoval = calls;

            hint.RemoveCallback();
            Assert.True(hint.Set("changed"));

            Assert.Equal(callsBeforeRemoval, calls);
        }
    }
}
