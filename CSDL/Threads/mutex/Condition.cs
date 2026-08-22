// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Threads {
    /// <inheritdoc cref="CSDL.Internal.Docs.Mutex"/>
    public sealed class Condition : NativeHandle<Opaque.SdlCondition> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.CreateCondition"/>
        public Condition() {
            Handle = SDL.CreateCondition().ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.SignalCondition"/>
        public void Signal() {
            SDL.SignalCondition(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.BroadcastCondition"/>
        public void Broadcast() {
            SDL.BroadcastCondition(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.WaitCondition"/>
        public void Wait(Mutex mutex) {
            if (mutex == null) throw new ArgumentNullException(nameof(mutex));
            SDL.WaitCondition(Handle, mutex.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.WaitConditionTimeout"/>
        public bool WaitTimeout(Mutex mutex, int timeoutMS) {
            if (mutex == null) throw new ArgumentNullException(nameof(mutex));
            return SDL.WaitConditionTimeout(Handle, mutex.Handle, timeoutMS);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.DestroyCondition"/>
        protected override void DisposeResource() {
            SDL.DestroyCondition(Handle);
        }
    }
}
