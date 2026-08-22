// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    /// Contract for an app driven by SDL's main-callback lifecycle: initialized once, then iterated
    /// and fed events repeatedly, then quit exactly once when a hook requests termination.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Game"/>. It exists as its own interface so the lifecycle can be driven
    /// - and unit-tested - without depending on <see cref="Game"/>'s protected hook methods or on SDL
    /// itself; see <see cref="Application.Run(IApplication, string[], System.Action{System.Action{Event}}, int)">Application.Run()</see>.
    /// </remarks>
    public interface IApplication {
        /// <summary>Called once before the main loop starts.</summary>
        /// <param name="args">The process's command line arguments.</param>
        AppResult Init(string[] args);

        /// <summary>Called repeatedly; a single step of the app's main loop.</summary>
        AppResult Iterate();

        /// <summary>Called once for every event pumped from the queue.</summary>
        AppResult HandleEvent(ref Event @event);

        /// <summary>Called exactly once before the app terminates, with the result that ended it.</summary>
        void Quit(AppResult result);
    }
}
