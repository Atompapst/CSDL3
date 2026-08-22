// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL {
    /// <summary>
    ///     Windows-only entry points: the D3D9/DXGI adapter lookups and the Win32 message hook.
    /// </summary>
    public static class Windows {
        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetDirect3D9AdapterIndex"/>
        /// <returns>The adapter index, or -1 on failure.</returns>
        public static int GetDirect3D9AdapterIndex(DisplayID displayID) {
            int index = SDL.GetDirect3D9AdapterIndex(displayID);
            if (index < 0) {
                Error.LogError(nameof(SDL.GetDirect3D9AdapterIndex));
            }
            return index;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetDXGIOutputInfo"/>
        public static bool TryGetDXGIOutputInfo(DisplayID displayID, out int adapterIndex, out int outputIndex) {
            return SDL.GetDXGIOutputInfo(displayID, out adapterIndex, out outputIndex)
                .LogIfFalse(nameof(SDL.GetDXGIOutputInfo));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.SetWindowsMessageHook"/>
        /// <param name="callback">the hook to install, or <see langword="null"/> to remove the current one.</param>
        /// <param name="userdata">passed through to the hook.</param>
        /// <remarks>
        ///     Only one hook can be installed at a time; the delegate stays rooted while it is.
        /// </remarks>
        public static void SetMessageHook(WindowsMessageHook? callback, object? userdata = null) {
            if (callback is null) {
                SDL.SetWindowsMessageHook(null!, IntPtr.Zero);
                CallbackRegistry.UnregisterSingle<WindowsMessageHook, SDL_WindowsMessageHookNative>();
                return;
            }

            SDL_WindowsMessageHookNative native = WindowsMessageHookWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.RegisterSingle(callback, native, userdata);
            SDL.SetWindowsMessageHook(native, cb.userdataPtr);
        }
    }

    /// <summary>
    ///     X11-only entry points.
    /// </summary>
    public static class X11 {
        /// <inheritdoc cref="CSDL.Internal.Docs.System.SetX11EventHook"/>
        /// <param name="callback">the hook to install, or <see langword="null"/> to remove the current one.</param>
        /// <param name="userdata">passed through to the hook.</param>
        /// <remarks>
        ///     Only one hook can be installed at a time; the delegate stays rooted while it is.
        /// </remarks>
        public static void SetEventHook(X11EventHook? callback, object? userdata = null) {
            if (callback is null) {
                SDL.SetX11EventHook(null!, IntPtr.Zero);
                CallbackRegistry.UnregisterSingle<X11EventHook, SDL_X11EventHookNative>();
                return;
            }

            SDL_X11EventHookNative native = X11EventHookWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.RegisterSingle(callback, native, userdata);
            SDL.SetX11EventHook(native, cb.userdataPtr);
        }
    }

    /// <summary>
    ///     Linux-only entry points: the thread scheduling knobs that need privileges SDL's own
    ///     <see cref="CSDL.Threads.ThreadPriority"/> path cannot assume.
    /// </summary>
    public static class Linux {
        /// <inheritdoc cref="CSDL.Internal.Docs.System.SetLinuxThreadPriority"/>
        /// <param name="threadID">the Linux thread ID (<c>gettid()</c>), not a pthread handle.</param>
        /// <param name="priority">the nice value to set.</param>
        public static bool SetThreadPriority(long threadID, int priority) {
            return SDL.SetLinuxThreadPriority(threadID, priority).LogIfFalse(nameof(SDL.SetLinuxThreadPriority));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.SetLinuxThreadPriorityAndPolicy"/>
        /// <param name="threadID">the Linux thread ID (<c>gettid()</c>), not a pthread handle.</param>
        /// <param name="priority">the SDL thread priority to map onto the policy.</param>
        /// <param name="schedPolicy">the <c>sched(7)</c> policy, e.g. <c>SCHED_OTHER</c> or <c>SCHED_RR</c>.</param>
        public static bool SetThreadPriorityAndPolicy(long threadID, CSDL.Threads.ThreadPriority priority, int schedPolicy) {
            return SDL.SetLinuxThreadPriorityAndPolicy(threadID, (int)priority, schedPolicy)
                .LogIfFalse(nameof(SDL.SetLinuxThreadPriorityAndPolicy));
        }
    }
}
