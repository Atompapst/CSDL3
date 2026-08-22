// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;

namespace CSDL.File {
    public partial struct IOStreamInterface {
        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.GetIOSize"/>
        public delegate long SizeDelegate(object? userdata);

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.SeekIO"/>
        public delegate long SeekDelegate(object? userdata, long offset, IOWhence whence);

        /// <summary>Reads up to <paramref name="size"/> bytes from the stream into <paramref name="ptr"/>, returning the number of bytes actually read.</summary>
        public delegate nuint ReadDelegate(object? userdata, nint ptr, nuint size, out IOStatus status);

        /// <summary>Writes exactly <paramref name="size"/> bytes from <paramref name="ptr"/> to the stream, returning the number of bytes actually written.</summary>
        public delegate nuint WriteDelegate(object? userdata, nint ptr, nuint size, out IOStatus status);

        /// <summary>Flushes any buffered data. Returns <see langword="true"/> on success.</summary>
        public delegate bool FlushDelegate(object? userdata, out IOStatus status);

        /// <summary>Closes the stream and releases any resources it holds. Returns <see langword="true"/> on success.</summary>
        public delegate bool CloseDelegate(object? userdata);

        // The actual native ABI shape SDL calls - registered with CallbackRegistry exactly like every
        // other SDL callback in this codebase, so CBool/keep-alive/cleanup work the same way. Only
        // internal, so unlike the public delegates above they're free to use CBool as a return type.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long SizeDelegateNative(nint userdata);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long SeekDelegateNative(nint userdata, long offset, IOWhence whence);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate nuint ReadDelegateNative(nint userdata, nint ptr, nuint size, out IOStatus status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate nuint WriteDelegateNative(nint userdata, nint ptr, nuint size, out IOStatus status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate CBool FlushDelegateNative(nint userdata, out IOStatus status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate CBool CloseDelegateNative(nint userdata);

        private sealed class Callbacks {
            public required SizeDelegate Size { get; init; }
            public required SeekDelegate Seek { get; init; }
            public required ReadDelegate Read { get; init; }
            public required WriteDelegate Write { get; init; }
            public required FlushDelegate Flush { get; init; }
            public required CloseDelegate Close { get; init; }
            public object? UserData { get; init; }
        }

        private static long SizeTrampoline(nint userdata) {
            try {
                Callbacks cb = (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
                return cb.Size(cb.UserData);
            } catch {
                SetCallbackError();
                return -1;
            }
        }

        private static long SeekTrampoline(nint userdata, long offset, IOWhence whence) {
            try {
                Callbacks cb = (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
                return cb.Seek(cb.UserData, offset, whence);
            } catch {
                SetCallbackError();
                return -1;
            }
        }

        private static nuint ReadTrampoline(nint userdata, nint ptr, nuint size, out IOStatus status) {
            try {
                Callbacks cb = (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
                return cb.Read(cb.UserData, ptr, size, out status);
            } catch {
                SetCallbackError();
                status = IOStatus.Error;
                return 0;
            }
        }

        private static nuint WriteTrampoline(nint userdata, nint ptr, nuint size, out IOStatus status) {
            try {
                Callbacks cb = (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
                return cb.Write(cb.UserData, ptr, size, out status);
            } catch {
                SetCallbackError();
                status = IOStatus.Error;
                return 0;
            }
        }

        private static CBool FlushTrampoline(nint userdata, out IOStatus status) {
            try {
                Callbacks cb = (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
                return cb.Flush(cb.UserData, out status);
            } catch {
                SetCallbackError();
                status = IOStatus.Error;
                return false;
            }
        }

        private static CBool CloseTrampoline(nint userdata) {
            try {
                Callbacks cb = (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
                return cb.Close(cb.UserData);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static void SetCallbackError() {
            try {
                Error.SetError("Managed IO stream callback threw an exception.");
            } catch {
                // Never allow error reporting to escape a native callback either.
            }
        }

        /// <summary>
        /// Reproduces the <c>SDL_INIT_INTERFACE</c> macro's <c>version</c> assignment (<c>iface-&gt;version = sizeof(*iface)</c>).
        /// </summary>
        /// <remarks>
        /// Call this on a freshly zeroed (<see langword="default"/>) instance before calling
        /// <see cref="Attach"/> - SDL rejects an <see cref="IOStreamInterface"/> whose
        /// <see cref="Version"/> doesn't match the size it expects for the SDL version linked at
        /// runtime, as a defense against an app built against a header with a different field layout
        /// than the shared library it's actually running against.
        /// </remarks>
        public void InitVersion() {
            Version = (uint)Marshal.SizeOf<IOStreamInterface>();
        }

        /// <summary>
        /// Registers all six operations with <see cref="CallbackRegistry"/>, assigns the resulting
        /// native function pointers, and returns the id (pass to <see cref="Detach"/> once the stream
        /// closes) and the single userdata pointer to pass as <c>SDL_OpenIO</c>'s <c>userData</c>.
        /// </summary>
        internal string Attach(
            SizeDelegate size, SeekDelegate seek, ReadDelegate read, WriteDelegate write, FlushDelegate flush, CloseDelegate close,
            object? userData, out IntPtr userdataPtr) {
            string id = Guid.NewGuid().ToString("N");
            Callbacks callbacks = new Callbacks {
                Size = size, Seek = seek, Read = read, Write = write, Flush = flush, Close = close, UserData = userData,
            };

            // Only the first registration actually allocates a GCHandle (Register's userdata param is
            // null for the rest) - SDL only ever hands back the one pointer this call returns, so a
            // separate handle per operation would just be four wasted allocations that are never used.
            (IntPtr sizePtr, IntPtr sharedUserdataPtr) = CallbackRegistry.Register<SizeDelegate, SizeDelegateNative>(id + ":size", size, SizeTrampoline, callbacks);
            (IntPtr seekPtr, _) = CallbackRegistry.Register<SeekDelegate, SeekDelegateNative>(id + ":seek", seek, SeekTrampoline);
            (IntPtr readPtr, _) = CallbackRegistry.Register<ReadDelegate, ReadDelegateNative>(id + ":read", read, ReadTrampoline);
            (IntPtr writePtr, _) = CallbackRegistry.Register<WriteDelegate, WriteDelegateNative>(id + ":write", write, WriteTrampoline);
            (IntPtr flushPtr, _) = CallbackRegistry.Register<FlushDelegate, FlushDelegateNative>(id + ":flush", flush, FlushTrampoline);
            (IntPtr closePtr, _) = CallbackRegistry.Register<CloseDelegate, CloseDelegateNative>(id + ":close", close, CloseTrampoline);

            Size = sizePtr;
            Seek = seekPtr;
            Read = readPtr;
            Write = writePtr;
            Flush = flushPtr;
            Close = closePtr;
            userdataPtr = sharedUserdataPtr;
            return id;
        }

        /// <summary>Unregisters everything <see cref="Attach"/> registered under <paramref name="id"/>, freeing the shared userdata GCHandle.</summary>
        internal static void Detach(string id) {
            CallbackRegistry.Unregister<SizeDelegate, SizeDelegateNative>(id + ":size");
            CallbackRegistry.Unregister<SeekDelegate, SeekDelegateNative>(id + ":seek");
            CallbackRegistry.Unregister<ReadDelegate, ReadDelegateNative>(id + ":read");
            CallbackRegistry.Unregister<WriteDelegate, WriteDelegateNative>(id + ":write");
            CallbackRegistry.Unregister<FlushDelegate, FlushDelegateNative>(id + ":flush");
            CallbackRegistry.Unregister<CloseDelegate, CloseDelegateNative>(id + ":close");
        }
    }
}
