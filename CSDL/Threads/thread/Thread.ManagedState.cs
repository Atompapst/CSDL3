// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
namespace CSDL.Threads {
    public partial class Thread {
        // This delegate is process-long-lived. Each invocation releases its own state only after
        // the managed callback has returned, so detaching a running thread cannot unroot it early.
        private static readonly SDL_ThreadFunctionNative ManagedThreadEntry = InvokeManagedThread;
        
        private static int InvokeManagedThread(IntPtr statePtr) {
            ManagedThreadState state = (ManagedThreadState)GCHandle.FromIntPtr(statePtr).Target!;
            try {
                return state.Callback(state.UserdataPtr);
            } catch (Exception ex) {
                Log.Error(ex, "Managed thread callback threw an exception.");
                return 0;
            } finally {
                state.Release();
            }
        }

        private sealed class ManagedThreadState {
            private GCHandle _selfHandle;
            private GCHandle _userdataHandle;
            private readonly bool _hasUserdata;

            public ManagedThreadState(ThreadFunction callback, object? userdata) {
                Callback = callback;
                if (userdata is not null) {
                    _userdataHandle = GCHandle.Alloc(userdata);
                    _hasUserdata = true;
                }
                _selfHandle = GCHandle.Alloc(this);
            }

            public ThreadFunction Callback { get; }
            public IntPtr StatePtr => GCHandle.ToIntPtr(_selfHandle);
            public IntPtr UserdataPtr => _hasUserdata ? GCHandle.ToIntPtr(_userdataHandle) : IntPtr.Zero;

            public void Release() {
                if (_hasUserdata && _userdataHandle.IsAllocated) {
                    _userdataHandle.Free();
                }
                if (_selfHandle.IsAllocated) {
                    _selfHandle.Free();
                }
            }
        }
    }
}
