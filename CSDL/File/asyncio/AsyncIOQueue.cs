// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;
namespace CSDL.File {
    public class AsyncIOQueue : NativeHandle<Opaque.SdlAsyncIOQueue> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.CreateAsyncIOQueue"/>
        public AsyncIOQueue() {
            Handle = SDL.CreateAsyncIOQueue().ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.GetAsyncIOResult"/>
        public bool GetResult(out AsyncIOOutcome outcome) {
            outcome = default;
            return SDL.GetAsyncIOResult(Handle, ref outcome);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.WaitAsyncIOResult"/>
        public bool WaitResult(out AsyncIOOutcome outcome, int timeoutMS) {
            outcome = default;
            return SDL.WaitAsyncIOResult(Handle, ref outcome, timeoutMS);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.SignalAsyncIOQueue"/>
        public void Signal() {
            SDL.SignalAsyncIOQueue(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.LoadFileAsync"/>
        public static bool LoadFileAsync(string file, AsyncIOQueue queue, IntPtr userdata) {
            return SDL.LoadFileAsync(file, queue.Handle, userdata).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.DestroyAsyncIOQueue"/>
        protected override void DisposeResource() {
            SDL.DestroyAsyncIOQueue(Handle);
        }
    }
}
