// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    public static class Init {
        private static Threads.InitState _state;

        /// <summary>
        /// Raised when the initialization process is completed successfully.
        /// This event is invoked internally when all required subsystems are initialized.
        /// </summary>
        /// <remarks>
        /// Subscribers to this event can perform actions or execute logic upon the successful initialization
        /// of the system. It is triggered only once during the lifecycle of the application.
        /// </remarks>
        /// <seealso cref="CSDL.Init.Initialize"/>
        /// <seealso cref="CSDL.Init.InitSubSystem"/>
        public static event Action? OnInit;

        /// <summary>
        /// Raised when the quit process is triggered.
        /// This event is invoked internally during the shutdown sequence of the application.
        /// </summary>
        /// <remarks>
        /// Subscribers to this event can perform necessary cleanup or resource disposal
        /// operations prior to application termination. It is typically called once
        /// when the application is exiting and may include cleanup of subsystems and resources.
        /// </remarks>
        /// <seealso cref="CSDL.Init.Quit"/>
        /// <seealso cref="CSDL.Hints.DisposeAll"/>
        public static event Action? OnQuit;

        static Init() {
            _state = new Threads.InitState();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.IsMainThread"/>
        public static bool IsMainThread => SDL.IsMainThread();

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.WasInit"/>
        public static InitFlags Initialized => SDL.WasInit(0);

        public static bool IsInitialized => Initialized != 0;

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.SetAppMetadata"/>
        public static bool SetAppMetadata(string appname, string appversion, string appidentifier) {
            return SDL.SetAppMetadata(appname, appversion, appidentifier).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.RunOnMainThread"/>
        public static void RunOnMainThread(MainThreadCallback callback, object userdata, bool waitComplete) {
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"MainThread:{Guid.NewGuid()}";
            SDL_MainThreadCallbackNative callbackWrapper = MainThreadCallbackWrapper.Create(callback);
            SDL_MainThreadCallbackNative cb = userDataPtr => {
                try {
                    callbackWrapper(userDataPtr);
                }
                finally {
                    // SDL owns the queued callback until it has been invoked. Keep the
                    // delegate and userdata alive until then, including waitComplete:false.
                    CallbackRegistry.Unregister<MainThreadCallback, SDL_MainThreadCallbackNative>(id);
                }
            };

            (IntPtr _, IntPtr userdataPtr) =
                CallbackRegistry.Register<MainThreadCallback, SDL_MainThreadCallbackNative>(id, callback, cb, userdata);

            CBool ok = SDL.RunOnMainThread(cb, userdataPtr, waitComplete);
            if (!ok) {
                CallbackRegistry.Unregister<MainThreadCallback, SDL_MainThreadCallbackNative>(id);
            }
            ok.ThrowIfFalse(nameof(SDL.RunOnMainThread));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.SdlInit"/>
        public static bool Initialize(InitFlags flags) {
            return InitMissing(flags);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.InitSubSystem"/>
        public static bool InitSubSystem(InitFlags flags) {
            return InitMissing(flags);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.QuitSubSystem"/>
        public static void QuitSubSystem(InitFlags flags) {
            if (flags == 0) {
                return;
            }
            SDL.QuitSubSystem(flags);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.Quit"/>
        /// Additional calls <see cref="OnQuit">OnQuit</see>
        public static void Quit() {
            if (!_state.ShouldQuit && Initialized == 0) {
                return;
            }

            Exception? callbackException = null;

            try {
                try {
                    OnQuit?.Invoke();
                } catch (Exception ex) {
                    callbackException = ex;
                }
                SDL.Quit();
            }
            finally {
                _state.SetFalse();
            }

            if (callbackException != null) {
                throw callbackException;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.WasInit"/>
        /// <seealso cref="AreAllInitialized"/>
        /// <seealso cref="IsAnyInitialized"/>
        public static InitFlags WasInit(InitFlags flags = 0) {
            return SDL.WasInit(flags);
        }

        /// <summary>
        /// Determines if all specified initialization flags are set.
        /// </summary>
        /// <param name="flags">The initialization flags to check. If set to 0, checks if any module is initialized.</param>
        /// <returns>True if all specified flags are initialized; otherwise, false.</returns>
        /// <seealso cref="IsAnyInitialized"/>
        /// <seealso cref="WasInit"/>
        public static bool AreAllInitialized(InitFlags flags) {
            return flags == 0
                ? IsInitialized
                : (SDL.WasInit(flags) & flags) == flags;
        }

        /// <summary>
        /// Checks whether any of the specified initialization flags are set or if any module is initialized.
        /// </summary>
        /// <param name="flags">
        /// The initialization flags to check. If set to 0, the method determines whether any module is initialized.
        /// </param>
        /// <returns>
        /// True if any of the specified flags are initialized, or if any module is initialized when flags are 0; otherwise, false.
        /// </returns>
        /// <seealso cref="AreAllInitialized"/>
        /// <seealso cref="WasInit"/>
        public static bool IsAnyInitialized(InitFlags flags) {
            return flags == 0
                ? IsInitialized
                : (SDL.WasInit(flags) & flags) != 0;
        }

        private static bool InitMissing(InitFlags flags) {
            if (flags == 0) {
                return false;
            }

            InitFlags missing = flags & ~Initialized;

            if (missing == 0) {
                return false;
            }

            bool firstInitThroughWrapper = _state.ShouldInit;

            if (Initialized == 0) {
                InitCore(missing, SDL.Init, nameof(SDL.Init));
            } else {
                InitCore(missing, SDL.InitSubSystem, nameof(SDL.InitSubSystem));
            }

            _state.SetTrue();

            if (firstInitThroughWrapper) {
                OnInit?.Invoke();
            }

            return true;
        }

        private static void InitCore(InitFlags flags, Func<InitFlags, CBool> initFunc, string operation) {
            initFunc(flags).ThrowIfFalse(operation);
        }
    }
}
