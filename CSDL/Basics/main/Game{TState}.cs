// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.InteropServices;

namespace CSDL {
    /// <summary>
    /// Base class for apps driven by SDL's main-callback lifecycle whose state is an explicit
    /// <typeparamref name="TState"/> object rather than fields on the app class itself - the same
    /// shape SDL's own <c>void *appstate</c> follows in C, for apps that would rather keep their
    /// state separate from the class driving the lifecycle.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TState"/> is created once by <see cref="OnInit"/> and handed back to every
    /// later hook. It's kept alive across the run with a single pinned <see cref="GCHandle"/>, freed
    /// after <see cref="OnQuit"/> returns. See <see cref="Game"/> for the simpler shape where the app's
    /// own fields are enough and a separate state object isn't needed.
    /// </remarks>
    /// <seealso cref="Game"/>
    public abstract class Game<TState> : ApplicationCore, IApplication<TState> where TState : class {
        /// <summary>Called once by SDL at startup; produces the state passed to every later hook.</summary>
        /// <param name="args">The process's command line arguments.</param>
        /// <param name="state">The state to pass to every later call.</param>
        protected abstract AppResult OnInit(string[] args, out TState state);

        /// <summary>
        /// Called repeatedly by SDL; this is where a single step of the app's main loop belongs.
        /// </summary>
        protected abstract AppResult OnIterate(TState state);

        /// <summary>
        /// Called by SDL once for every event pumped from the queue. The default implementation
        /// ignores the event and continues.
        /// </summary>
        protected virtual AppResult OnEvent(TState state, ref Event @event) {
            return AppResult.Continue;
        }

        /// <summary>
        /// Called exactly once by SDL before the process terminates, with the result that ended the
        /// app. The default implementation does nothing.
        /// </summary>
        protected virtual void OnQuit(TState state, AppResult result) { }

        // GCHandle.Target is object? in general, but every handle resolved here was allocated by
        // CoreInit via GCHandle.Alloc(appState) with a non-null TState instance, and is only ever
        // freed by CoreQuit after this is called for the last time - so it is never null here.
        private static TState Resolve(nint state) {
            return (TState)GCHandle.FromIntPtr(state).Target!;
        }

        protected sealed override AppResult CoreInit(string[] args, out nint state) {
            AppResult result = OnInit(args, out TState appState);
            state = GCHandle.ToIntPtr(GCHandle.Alloc(appState));
            return result;
        }

        protected sealed override AppResult CoreIterate(nint state) {
            return OnIterate(Resolve(state));
        }

        protected sealed override AppResult CoreEvent(nint state, ref Event @event) {
            return OnEvent(Resolve(state), ref @event);
        }

        protected sealed override void CoreQuit(nint state, AppResult result) {
            GCHandle handle = GCHandle.FromIntPtr(state);
            try {
                OnQuit(Resolve(state), result);
            }
            finally {
                handle.Free();
            }
        }

        AppResult IApplication<TState>.Init(string[] args, out TState state) {
            return OnInit(args, out state);
        }
        AppResult IApplication<TState>.Iterate(TState state) {
            return OnIterate(state);
        }
        AppResult IApplication<TState>.HandleEvent(TState state, ref Event @event) {
            return OnEvent(state, ref @event);
        }
        void IApplication<TState>.Quit(TState state, AppResult result) {
            OnQuit(state, result);
        }
    }
}
