// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    public static partial class Application {

        /// <inheritdoc cref="CSDL.Internal.Docs.Main.GDKSuspendComplete"/>
        public static void GdkSuspendComplete() {
            SDL.GDKSuspendComplete();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Main.SetMainReady"/>
        public static void SetMainReady() {
            SDL.SetMainReady();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Main.RegisterApp"/>
        public static bool RegisterApp(string? name = null, uint style = 0, nint hInstance = 0) {
            return SDL.RegisterApp(name, style, hInstance).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Main.UnregisterApp"/>
        public static void UnregisterApp() {
            SDL.UnregisterApp();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Main.RunApp"/>
        /// <remarks>
        /// SDL's other main-callback mechanism: a single ANSI-C-style <c>main(argc, argv)</c> function,
        /// as an alternative to the <see cref="Game"/>/<see cref="Game{TState}"/> lifecycle.
        /// </remarks>
        public static int RunApp(string[] args, MainFunc mainFunction) {
            ArgumentNullException.ThrowIfNull(args);
            ArgumentNullException.ThrowIfNull(mainFunction);

            SDL_main_funcNative native = MainFuncWrapper.Create(mainFunction);
            using NativeStringArray.Native argv = NativeStringArray.Allocate(args);

            return SDL.RunApp(argv.Count, argv.Ptr, native, IntPtr.Zero);
        }

        /// <summary>
        /// Runs an SDL application through <see cref="RunApp(string[],MainFunc)"/>, using the current
        /// process's command line arguments.
        /// </summary>
        public static int RunApp(MainFunc mainFunction) {
            return RunApp(Environment.GetCommandLineArgs(), mainFunction);
        }
    }
}
