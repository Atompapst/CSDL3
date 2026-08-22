// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Threads {
    /// <inheritdoc cref="CSDL.Internal.Docs.Thread"/>
    public partial class Thread : NativeHandle<Opaque.SdlThread> {
        

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.CreateThread"/>
        public Thread(ThreadFunction fn, string name, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(fn);
            ManagedThreadState state = new ManagedThreadState(fn, userdata);
            NativePtr<Opaque.SdlThread> handle = SDL.CreateThread(ManagedThreadEntry, name, state.StatePtr, IntPtr.Zero, IntPtr.Zero);
            if (handle.IsNull) {
                state.Release();
            }
            Handle = handle.ThrowIfInvalid();
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.GetThreadID"/>
        public ThreadID ID => SDL.GetThreadID(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.GetCurrentThreadID"/>
        public static ThreadID GetCurrentThreadID => SDL.GetCurrentThreadID();

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.CleanupTLS"/>
        public static void CleanupTLS() {
            SDL.CleanupTLS();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.CreateThreadWithProperties"/>
        /// <remarks>
        ///     <paramref name="props" />'s <see cref="ThreadProperties.EntryFunction" /> and
        ///     <see cref="ThreadProperties.Userdata" /> are raw native pointers set directly on the property
        ///     group, so the caller is responsible for keeping any managed delegate they marshalled from alive
        ///     for the lifetime of the thread.
        /// </remarks>
        public Thread(ThreadProperties props) {
            Handle = SDL.CreateThreadWithProperties(props.Handle, IntPtr.Zero, IntPtr.Zero).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.GetThreadName"/>
        public string Name => SDL.GetThreadName(Handle).ToUtf8String() ?? string.Empty;

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.SetCurrentThreadPriority"/>
        public static bool SetCurrentThreadPriority(ThreadPriority priority) {
            return SDL.SetCurrentThreadPriority(priority).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.GetThreadState"/>
        public ThreadState State => (ThreadState)SDL.GetThreadState(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.DetachThread"/>
        public void Detach() {
            if (Handle.IsNull) return;
            SDL.DetachThread(Handle);
            Handle = null;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.WaitThread"/>
        public void Wait(out int status) {
            if (Handle.IsNull) throw new InvalidOperationException("Thread handle is null.");
            SDL.WaitThread(Handle, out status);
            Handle = null;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.DetachThread"/>
        protected override void DisposeResource() {
            SDL.DetachThread(Handle);
        }
    }
}
