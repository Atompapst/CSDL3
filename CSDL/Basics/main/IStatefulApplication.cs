// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    /// Contract for an app driven by SDL's main-callback lifecycle whose state is an explicit
    /// <typeparamref name="TState"/> object, produced once by <see cref="Init"/> and handed back to
    /// every later call - the same shape SDL's own <c>void *appstate</c> follows in C.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Game{TState}"/>. See <see cref="IApplication"/> for the simpler
    /// contract used when the app's own fields (via <c>this</c>) are enough and a separate state
    /// object isn't needed.
    /// </remarks>
    public interface IApplication<TState> where TState : class {
        /// <summary>Called once before the main loop starts; produces the state handed back on every later call.</summary>
        /// <param name="args">The process's command line arguments.</param>
        /// <param name="state">The state to pass to every later call.</param>
        AppResult Init(string[] args, out TState state);

        /// <summary>Called repeatedly; a single step of the app's main loop.</summary>
        AppResult Iterate(TState state);

        /// <summary>Called once for every event pumped from the queue.</summary>
        AppResult HandleEvent(TState state, ref Event @event);

        /// <summary>Called exactly once before the app terminates, with the result that ended it.</summary>
        void Quit(TState state, AppResult result);
    }
}
