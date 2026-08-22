// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    /// Base class for apps driven by SDL's main-callback lifecycle (<c>SDL_MAIN_USE_CALLBACKS</c>),
    /// instead of a hand-rolled main loop.
    /// </summary>
    /// <remarks>
    /// Derive from this class, override the lifecycle hooks you need, and call <see cref="ApplicationCore.Run"/>
    /// from your own <c>Main</c> method. SDL calls <see cref="OnInit"/> once, then <see cref="OnIterate"/> and
    /// <see cref="OnEvent"/> repeatedly until one of them returns <see cref="AppResult.Success"/> or
    /// <see cref="AppResult.Failure"/>, at which point <see cref="OnQuit"/> is called exactly once before
    /// <see cref="ApplicationCore.Run"/> returns.
    /// </remarks>
    /// <seealso cref="Game{TState}"/>
    public abstract class Game : ApplicationCore, IApplication {
        /// <summary>
        /// Called once by SDL at startup. The default implementation does nothing and continues.
        /// </summary>
        /// <param name="args">The process's command line arguments.</param>
        protected virtual AppResult OnInit(string[] args) {
            return AppResult.Continue;
        }

        /// <summary>
        /// Called repeatedly by SDL; this is where a single step of the app's main loop belongs.
        /// </summary>
        protected abstract AppResult OnIterate();

        /// <summary>
        /// Called by SDL once for every event pumped from the queue. The default implementation
        /// ignores the event and continues.
        /// </summary>
        protected virtual AppResult OnEvent(ref Event @event) {
            return AppResult.Continue;
        }

        /// <summary>
        /// Called exactly once by SDL before the process terminates, with the result that ended the
        /// app. The default implementation does nothing.
        /// </summary>
        protected virtual void OnQuit(AppResult result) { }

        // There's nothing to round-trip through the native appstate here: unlike Game{TState}, the
        // instance itself (accessed via "this" in the On* overrides) already lives for the whole run.
        protected sealed override AppResult CoreInit(string[] args, out nint state) {
            state = 0;
            return OnInit(args);
        }

        protected sealed override AppResult CoreIterate(nint state) {
            return OnIterate();
        }
        protected sealed override AppResult CoreEvent(nint state, ref Event @event) {
            return OnEvent(ref @event);
        }
        protected sealed override void CoreQuit(nint state, AppResult result) {
            OnQuit(result);
        }

        AppResult IApplication.Init(string[] args) {
            return OnInit(args);
        }
        AppResult IApplication.Iterate() {
            return OnIterate();
        }
        AppResult IApplication.HandleEvent(ref Event @event) {
            return OnEvent(ref @event);
        }
        void IApplication.Quit(AppResult result) {
            OnQuit(result);
        }
    }
}
