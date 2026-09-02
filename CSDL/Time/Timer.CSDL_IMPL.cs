// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Diagnostics;
using System.Threading;

namespace CSDL {
    public partial class Timer {
        #region CSDL_IMPL SDL_GetPerformanceCounter : SDL_systimer#SDL_GetPerformanceCounter

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetPerformanceCounter"/>
        public static ulong GetPerformanceCounter() {
            return (ulong)Stopwatch.GetTimestamp();
        }

        #endregion

        #region CSDL_IMPL SDL_GetPerformanceFrequency : SDL_systimer#SDL_GetPerformanceFrequency

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetPerformanceFrequency"/>
        public static ulong GetPerformanceFrequency() {
            return (ulong)Stopwatch.Frequency;
        }

        #endregion

        #region CSDL_IMPL SDL_DelayPrecise : SDL_timer#SDL_DelayPrecise, SDL_GetPerformanceCounter, SDL_GetPerformanceFrequency

        // 1:1 port of SDL_DelayPrecise. current/target never leave this call, so unlike GetTicksNS/GetTicks
        // measuring with our own clock is safe here: nothing outside this method ever compares against it.
        // SDL_GetTicksNS() is replaced by NowNs()
        // SDL_SYS_DelayNS() by the existing SDL.DelayNS() P/Invoke
        // SDL_CPUPauseInstruction() by Thread.SpinWait, .NET's portable equivalent.
        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.DelayPrecise"/>
        public static void DelayPrecise(ulong ns) {
            ulong freq = GetPerformanceFrequency();
            ulong currentValue = NowNs(freq);
            ulong targetValue = currentValue + ns;

            const ulong shortSleepNs = Macros.NsPerMs;

            ulong maxSleepNs = shortSleepNs;
            while (currentValue + maxSleepNs < targetValue) {
                SDL.DelayNS(shortSleepNs);

                ulong now = NowNs(freq);
                ulong nextSleepNs = now - currentValue;
                if (nextSleepNs > maxSleepNs) {
                    maxSleepNs = nextSleepNs;
                }
                currentValue = now;
            }

            if (currentValue < targetValue && targetValue - currentValue > maxSleepNs - shortSleepNs) {
                ulong delayNs = targetValue - currentValue - (maxSleepNs - shortSleepNs);
                SDL.DelayNS(delayNs);
                currentValue = NowNs(freq);
            }

            while (currentValue + shortSleepNs < targetValue) {
                SDL.DelayNS(shortSleepNs);
                currentValue = NowNs(freq);
            }

            while (currentValue < targetValue) {
                Thread.SpinWait(1);
                currentValue = NowNs(freq);
            }
        }

        private static ulong NowNs(ulong freq) {
            return (ulong)((UInt128)GetPerformanceCounter() * Macros.NsPerSecond / freq);
        }

        #endregion

    }
}
