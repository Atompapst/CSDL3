// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Threads {
    /// <inheritdoc cref="CSDL.Internal.Docs.Mutex"/>
    public sealed class Semaphore : NativeHandle<Opaque.SdlSemaphore> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.CreateSemaphore"/>
        public Semaphore(uint initialValue) {
            Handle = SDL.CreateSemaphore(initialValue).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.WaitSemaphore"/>
        public void Wait() {
            SDL.WaitSemaphore(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.TryWaitSemaphore"/>
        public bool TryWait() {
            return SDL.TryWaitSemaphore(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.WaitSemaphoreTimeout"/>
        public bool WaitTimeout(int timeoutMS) {
            return SDL.WaitSemaphoreTimeout(Handle, timeoutMS);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.SignalSemaphore"/>
        public void Signal() {
            SDL.SignalSemaphore(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.GetSemaphoreValue"/>
        public uint Value => SDL.GetSemaphoreValue(Handle);

        protected override void DisposeResource() {
            SDL.DestroySemaphore(Handle);
        }
    }
}
