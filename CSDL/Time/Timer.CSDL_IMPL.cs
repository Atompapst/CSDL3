// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Diagnostics;
using System.Threading;

namespace CSDL {
    public partial class Timer {
        #region CSDL_IMPL SDL_GetPerformanceCounter : SDL_timer#SDL_GetPerformanceCounter

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetPerformanceCounter"/>
        public static ulong GetPerformanceCounter() {
            return (ulong)Stopwatch.GetTimestamp();
        }

        #endregion

        #region CSDL_IMPL SDL_GetPerformanceFrequency : SDL_timer#SDL_GetPerformanceFrequency

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetPerformanceFrequency"/>
        public static ulong GetPerformanceFrequency() {
            return (ulong)Stopwatch.Frequency;
        }

        #endregion
    }
}
