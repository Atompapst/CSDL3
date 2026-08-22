// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    public static partial class Application {
        /// <summary>
        /// Drives <paramref name="app"/> through Init -> (events + Iterate) -> Quit without going
        /// through SDL's native main-callback entry point: <see cref="IApplication.Init"/> once, then
        /// <see cref="IApplication.HandleEvent"/> for every polled event followed by
        /// <see cref="IApplication.Iterate"/> each loop turn, until one of them returns something
        /// other than <see cref="AppResult.Continue"/>, at which point <see cref="IApplication.Quit"/>
        /// is called exactly once.
        /// </summary>
        /// <remarks>
        /// This mirrors the sequencing <c>SDL_EnterAppMainCallbacks</c> performs natively, so it doubles
        /// as a way to unit-test an <see cref="IApplication"/> in isolation and as a manual host for the
        /// rare case where the native callback loop isn't available.
        /// </remarks>
        /// <param name="app">The application to drive.</param>
        /// <param name="args">Command line arguments passed to <see cref="IApplication.Init"/>.</param>
        /// <param name="pollEvents">
        /// Invoked once per loop turn, with a callback to deliver each event to before
        /// <see cref="IApplication.Iterate"/> runs that turn - the same shape as <see cref="Events.PollEvent"/>,
        /// so it can be passed directly: <c>Application.Run(app, pollEvents: Events.PollEvent)</c>. Pass
        /// <c>null</c> to skip event dispatch.
        /// </param>
        /// <param name="maxIterations">
        /// A safety bound on the number of <see cref="IApplication.Iterate"/> calls, mainly useful in
        /// tests against an app that never terminates on its own. Defaults to unbounded.
        /// </param>
        /// <returns>The final <see cref="AppResult"/> the app terminated with.</returns>
        /// <exception cref="Exception">
        /// Whatever exception first escaped a hook, rethrown after <see cref="IApplication.Quit"/> has
        /// run - mirroring how <see cref="ApplicationCore.Run"/> surfaces callback exceptions.
        /// </exception>
        public static AppResult Run(
            IApplication app,
            string[]? args = null,
            Action<Action<Event>>? pollEvents = null,
            uint maxIterations = uint.MaxValue) {
            ArgumentNullException.ThrowIfNull(app);
            args ??= Array.Empty<string>();

            Exception? callbackException = null;
            AppResult result = Invoke(() => app.Init(args), ref callbackException);

            uint iterations = 0;
            while (result == AppResult.Continue && iterations < maxIterations) {
                if (pollEvents != null) {
                    pollEvents(polled => {
                        if (result != AppResult.Continue) return;
                        Event current = polled;
                        result = Invoke(() => app.HandleEvent(ref current), ref callbackException);
                    });
                }

                if (result == AppResult.Continue) {
                    result = Invoke(app.Iterate, ref callbackException);
                }

                iterations++;
            }

            try {
                app.Quit(result);
            } catch (Exception ex) {
                callbackException ??= ex;
            }

            if (callbackException != null) {
                throw callbackException;
            }

            return result;
        }

        private static AppResult Invoke(Func<AppResult> hook, ref Exception? callbackException) {
            if (callbackException != null) {
                return AppResult.Failure;
            }

            try {
                return hook();
            } catch (Exception ex) {
                callbackException = ex;
                return AppResult.Failure;
            }
        }

        /// <summary>
        /// Same sequencing as <see cref="Run(IApplication,string[],System.Action{System.Action{Event}},uint)"/>, for an <see cref="IApplication{TState}"/> whose state
        /// is an explicit <typeparamref name="TState"/> object rather than the app instance's own
        /// fields. Unlike the real native path (<see cref="ApplicationCore.Run"/>, via a <see cref="System.Runtime.InteropServices.GCHandle"/>),
        /// this just holds <typeparamref name="TState"/> in a local variable - there's no pointer to marshal.
        /// </summary>
        /// <param name="app">The application to drive.</param>
        /// <param name="args">Command line arguments passed to <see cref="IApplication{TState}.Init"/>.</param>
        /// <param name="pollEvents">
        /// Invoked once per loop turn, with a callback to deliver each event to before
        /// <see cref="IApplication{TState}.Iterate"/> runs that turn - the same shape as
        /// <see cref="Events.PollEvent"/>, so it can be passed directly. Pass <c>null</c> to skip event dispatch.
        /// </param>
        /// <param name="maxIterations">
        /// A safety bound on the number of <see cref="IApplication{TState}.Iterate"/> calls, mainly
        /// useful in tests against an app that never terminates on its own. Defaults to unbounded.
        /// </param>
        /// <returns>The final <see cref="AppResult"/> the app terminated with.</returns>
        public static AppResult Run<TState>(
            IApplication<TState> app,
            string[]? args = null,
            Action<Action<Event>>? pollEvents = null,
            uint maxIterations = uint.MaxValue
        ) where TState : class {
            ArgumentNullException.ThrowIfNull(app);
            args ??= Array.Empty<string>();

            Exception? callbackException = null;
            TState? state = null;
            AppResult result = Invoke(() => app.Init(args, out state), ref callbackException);

            uint iterations = 0;
            while (result == AppResult.Continue && iterations < maxIterations) {
                if (pollEvents != null) {
                    pollEvents(polled => {
                        if (result != AppResult.Continue) return;
                        Event current = polled;
                        // Only reached while result == Continue, which Invoke never leaves set
                        // after Init unless it completed successfully and assigned state.
                        result = Invoke(() => app.HandleEvent(state!, ref current), ref callbackException);
                    });
                }

                if (result == AppResult.Continue) {
                    // Same invariant as above: Continue here means Init already produced a state.
                    result = Invoke(() => app.Iterate(state!), ref callbackException);
                }

                iterations++;
            }

            try {
                // If Init itself threw, state was never assigned and is still null here - Quit is
                // still called unconditionally (mirroring ApplicationCore.Run's finally/EnsureQuit
                // guarantee), so this can genuinely pass null through to IApplication<TState>.Quit
                // despite its non-nullable TState parameter.
                app.Quit(state!, result);
            } catch (Exception ex) {
                callbackException ??= ex;
            }

            if (callbackException != null) {
                throw callbackException;
            }

            return result;
        }
    }
}
