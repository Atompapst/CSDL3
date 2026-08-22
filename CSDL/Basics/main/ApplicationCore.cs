// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace CSDL {
    /// <summary>
    /// Shared plumbing behind <see cref="Game"/> and <see cref="Game{TState}"/>: wires managed hook
    /// methods into SDL's native main-callback delegates, marshals <c>argv</c>, and converts an
    /// exception thrown from any hook into <see cref="AppResult.Failure"/> - rethrowing it, with its
    /// original stack trace intact, only after <see cref="CoreQuit"/> has run.
    /// </summary>
    /// <remarks>
    /// Deliberately <c>internal</c>-in-spirit: the two public shapes (<see cref="Game"/> without
    /// appstate, <see cref="Game{TState}"/> with it) are what users are meant to derive from, and both
    /// seal off the <c>Core*</c> hooks so nothing below this class can be reached directly. Both talk
    /// to SDL exclusively through the raw <c>nint</c> appstate here, so this is the one place that owns
    /// the native call and the appstate round-trip; neither subclass needs to know how the other works.
    /// </remarks>
    public abstract class ApplicationCore {
        /// <summary>Called once before the main loop starts; produces the raw appstate handed back on every later call.</summary>
        protected abstract AppResult CoreInit(string[] args, out nint state);

        /// <summary>Called repeatedly; a single step of the app's main loop.</summary>
        protected abstract AppResult CoreIterate(nint state);

        /// <summary>Called once for every event pumped from the queue.</summary>
        protected abstract AppResult CoreEvent(nint state, ref Event @event);

        /// <summary>Called exactly once before the app terminates, with the result that ended it.</summary>
        protected abstract void CoreQuit(nint state, AppResult result);

        /// <summary>
        /// Enters SDL's main-callback loop, driving this instance's lifecycle hooks until the app
        /// requests termination.
        /// </summary>
        /// <param name="args">
        /// The command line arguments to pass to <see cref="CoreInit"/>. Defaults to the current
        /// process's command line arguments when omitted.
        /// </param>
        /// <returns>The ANSI-C style return code SDL produced for the process.</returns>
        /// <exception cref="Exception">
        /// Whatever exception first escaped a hook (or, failing that, whatever the native call itself
        /// threw), rethrown with its original stack trace intact after <see cref="CoreQuit"/> has run.
        /// </exception>
        /// <seealso cref="CSDL.Internal.Docs.Main.EnterAppMainCallbacks">EnterAppMainCallbacks</seealso>
        public int Run(string[]? args = null) {
            using NativeStringArray.Native argv = NativeStringArray.Allocate(args ?? Environment.GetCommandLineArgs());

            RunContext context = new RunContext(this);
            ExceptionDispatchInfo? nativeFailure = null;
            int rc = 0;

            try {
                rc = SDL.EnterAppMainCallbacks(
                    argv.Count,
                    argv.Ptr,
                    AppInitFuncWrapper.Create(context.Init),
                    AppIterateFuncWrapper.Create(context.Iterate),
                    AppEventFuncWrapper.Create(context.HandleEvent),
                    AppQuitFuncWrapper.Create(context.Quit)
                );
            } catch (Exception ex) {
                nativeFailure = ExceptionDispatchInfo.Capture(ex);
            }
            finally {
                // Guarantees CoreQuit still runs - freeing whatever Game{TState} pinned via its
                // GCHandle - even if the native call above threw before SDL got to invoke the real
                // AppQuit callback itself.
                context.EnsureQuit();
            }

            nativeFailure?.Throw();
            context.ThrowIfFaulted();
            GC.KeepAlive(context);
            return rc;
        }

        /// <summary>
        /// Bridges the native callbacks to this instance's <c>Core*</c> hooks, guarding against SDL's
        /// documented concurrency: <c>SDL_AppEvent</c> may be invoked from another thread while
        /// <c>SDL_AppIterate</c> (or another <c>SDL_AppEvent</c>) is still running on the main thread.
        /// <c>SDL_AppQuit</c> waits for every in-flight call to finish before it runs <see cref="CoreQuit"/>,
        /// so nothing ever touches the appstate after it's been handed to <see cref="CoreQuit"/> - in
        /// particular, <see cref="Game{TState}"/> never frees its <see cref="System.Runtime.InteropServices.GCHandle"/>
        /// while another thread is still mid-callback.
        /// </summary>
        private sealed class RunContext {
            private readonly ApplicationCore _owner;
            private readonly object _sync = new object();
            private nint _state;
            private ExceptionDispatchInfo? _callbackFailure;
            private AppResult _terminalResult = AppResult.Failure;
            private int _activeCallbacks;
            private bool _quitStarted;
            private bool _quitCompleted;

            internal RunContext(ApplicationCore owner) {
                _owner = owner;
            }

            internal AppResult Init(out nint state, int argc, string[] argv) {
                try {
                    AppResult result = _owner.CoreInit(argv, out nint createdState);
                    lock (_sync) {
                        _state = createdState;
                        if (!_quitStarted) _terminalResult = result;
                    }

                    state = createdState;
                    return result;
                } catch (Exception ex) {
                    CaptureFailure(ex);
                    state = 0;
                    SetTerminalResultIfRunning(AppResult.Failure);
                    return AppResult.Failure;
                }
            }

            internal AppResult Iterate(nint state) {
                if (!TryEnterCallback()) return GetTerminalResult();

                try {
                    AppResult result = _owner.CoreIterate(state);
                    SetTerminalResultIfRunning(result);
                    return result;
                } catch (Exception ex) {
                    CaptureFailure(ex);
                    SetTerminalResultIfRunning(AppResult.Failure);
                    return AppResult.Failure;
                }
                finally {
                    ExitCallback();
                }
            }

            internal AppResult HandleEvent(nint state, ref Event @event) {
                if (!TryEnterCallback()) return GetTerminalResult();

                try {
                    AppResult result = _owner.CoreEvent(state, ref @event);
                    SetTerminalResultIfRunning(result);
                    return result;
                } catch (Exception ex) {
                    CaptureFailure(ex);
                    SetTerminalResultIfRunning(AppResult.Failure);
                    return AppResult.Failure;
                }
                finally {
                    ExitCallback();
                }
            }

            internal void Quit(nint state, AppResult result) {
                Complete(result);
            }

            internal void EnsureQuit() {
                Complete(AppResult.Failure);
            }

            internal void ThrowIfFaulted() {
                _callbackFailure?.Throw();
            }

            private bool TryEnterCallback() {
                lock (_sync) {
                    if (_quitStarted) return false;
                    _activeCallbacks++;
                    return true;
                }
            }

            private void ExitCallback() {
                lock (_sync) {
                    _activeCallbacks--;
                    if (_activeCallbacks == 0) Monitor.PulseAll(_sync);
                }
            }

            private AppResult GetTerminalResult() {
                lock (_sync) {
                    return _terminalResult;
                }
            }

            private void SetTerminalResultIfRunning(AppResult result) {
                lock (_sync) {
                    if (!_quitStarted) _terminalResult = result;
                }
            }

            private void Complete(AppResult result) {
                nint stateToQuit;

                lock (_sync) {
                    if (_quitStarted) {
                        while (!_quitCompleted) {
                            Monitor.Wait(_sync);
                        }
                        return;
                    }

                    _quitStarted = true;
                    _terminalResult = result;
                    while (_activeCallbacks != 0) {
                        Monitor.Wait(_sync);
                    }
                    stateToQuit = _state;
                }

                try {
                    _owner.CoreQuit(stateToQuit, result);
                } catch (Exception ex) {
                    CaptureFailure(ex);
                }
                finally {
                    lock (_sync) {
                        _quitCompleted = true;
                        Monitor.PulseAll(_sync);
                    }
                }
            }

            private void CaptureFailure(Exception ex) {
                lock (_sync) {
                    _callbackFailure ??= ExceptionDispatchInfo.Capture(ex);
                }
            }
        }
    }
}
