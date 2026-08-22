// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.File {
    public class AsyncIO : NativeHandle<Opaque.SdlAsyncIO> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.AsyncIOFromFile"/>
        public AsyncIO(string file, string mode) {
            Handle = SDL.AsyncIOFromFile(file, mode).ThrowIfInvalid();
        }

        internal AsyncIO(NativePtr<Opaque.SdlAsyncIO> handle) {
            Handle = handle.ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.GetAsyncIOSize"/>
        public long Size => SDL.GetAsyncIOSize(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.ReadAsyncIO"/>
        public bool Read(IntPtr ptr, ulong offset, ulong size, AsyncIOQueue queue, IntPtr userdata) {
            return SDL.ReadAsyncIO(Handle, ptr, offset, size, queue.Handle, userdata).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.WriteAsyncIO"/>
        public bool Write(IntPtr ptr, ulong offset, ulong size, AsyncIOQueue queue, IntPtr userdata) {
            return SDL.WriteAsyncIO(Handle, ptr, offset, size, queue.Handle, userdata).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.CloseAsyncIO"/>
        public bool Close(bool flush, AsyncIOQueue queue, IntPtr userdata) {
            if (!SDL.CloseAsyncIO(Handle, flush, queue.Handle, userdata).LogIfFalse()) {
                return false;
            }

            Invalidate();
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Asyncio.CloseAsyncIO"/>
        protected override void DisposeResource() {
            // SDL requires every close to have a queue. Destroying this private queue waits for
            // the close task and releases its outcome before the handle is discarded.
            using AsyncIOQueue queue = new AsyncIOQueue();
            Close(flush: false, queue, IntPtr.Zero);
        }
    }

}
