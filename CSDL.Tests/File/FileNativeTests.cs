// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.File;
using CSDL3.Tests.TestSupport;

namespace CSDL3.Tests.Files {
    [Collection(SdlCollection.Name)]
    public sealed class FileNativeTests {
        private readonly SdlFixture _sdl;

        public FileNativeTests(SdlFixture sdl) {
            _sdl = sdl;
        }

        [Fact]
        public void AsyncIO_Close_InvalidatesTheHandleAndReportsItsOutcome() {
            string path = _sdl.ScratchPath("async-close.txt");
            System.IO.File.WriteAllText(path, "data");

            using AsyncIOQueue queue = new AsyncIOQueue();
            AsyncIO io = new AsyncIO(path, "r");

            Assert.True(io.Close(flush: false, queue: queue, userdata: IntPtr.Zero));
            Assert.Equal(IntPtr.Zero, io.NativePointer);
            Assert.Throws<ObjectDisposedException>(() => _ = io.Size);
            Assert.True(queue.WaitResult(out AsyncIOOutcome outcome, 1000));
            Assert.Equal(AsyncIOTaskType.Close, outcome.Type);
        }

        [Fact]
        public void AsyncIO_Dispose_ClosesUsingAnInternalQueue() {
            string path = _sdl.ScratchPath("async-dispose.txt");
            System.IO.File.WriteAllText(path, "data");

            using (AsyncIO io = new AsyncIO(path, "r")) {
                Assert.NotEqual(IntPtr.Zero, io.NativePointer);
            }
        }

        [Fact]
        public void CustomIOStream_CallbackExceptionBecomesANativeFailure() {
            using IOStream stream = IOStream.FromCustom(
                (object userdata) => throw new InvalidOperationException("Expected test exception."),
                (object userdata, long offset, IOWhence whence) => -1,
                (object userdata, nint ptr, nuint size, out IOStatus status) => {
                    status = IOStatus.Error;
                    return 0;
                },
                (object userdata, nint ptr, nuint size, out IOStatus status) => {
                    status = IOStatus.Error;
                    return 0;
                },
                (object userdata, out IOStatus status) => {
                    status = IOStatus.Error;
                    return false;
                },
                (object userdata) => true);

            Assert.Equal(-1, stream.Size);
        }

        [Fact]
        public void CustomStorage_CallbackExceptionsBecomeFailures() {
            Storage storage = Storage.FromCustom(
                _ => throw new InvalidOperationException("Expected test exception."),
                _ => throw new InvalidOperationException("Expected test exception."));

            Assert.False(storage.IsReady);
            storage.Dispose();
        }

        [Fact]
        public void EnumerateDirectory_PassesManagedUserdataToTheCallback() {
            string path = _sdl.ScratchPath("enumeration-userdata");
            object userdata = new object();
            object received = null;
            System.IO.Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(System.IO.Path.Combine(path, "entry.txt"), "data");

            bool result = CSDL.File.Path.EnumerateDirectory(path, (object callbackUserdata, string dirname, string fname) => {
                received = callbackUserdata;
                return EnumerationResult.Success;
            }, userdata);

            Assert.True(result);
            Assert.Same(userdata, received);
        }

        [Fact]
        public void CustomIOStream_SaveWithCloseIO_DisposesTheManagedWrapper() {
            int closeCalls = 0;
            IOStream stream = IOStream.FromCustom(
                (object userdata) => 0,
                (object userdata, long offset, IOWhence whence) => -1,
                (object userdata, nint ptr, nuint size, out IOStatus status) => {
                    status = IOStatus.Eof;
                    return 0;
                },
                (object userdata, nint ptr, nuint size, out IOStatus status) => {
                    status = IOStatus.Ready;
                    return size;
                },
                (object userdata, out IOStatus status) => {
                    status = IOStatus.Ready;
                    return true;
                },
                (object userdata) => {
                    closeCalls++;
                    return true;
                });

            Assert.True(stream.Save(Array.Empty<byte>(), closeIO: true));
            Assert.Equal(1, closeCalls);
            Assert.Throws<ObjectDisposedException>(() => _ = stream.Size);
        }
    }
}
