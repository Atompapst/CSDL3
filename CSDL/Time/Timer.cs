// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL {
    /// <summary>
    ///     Definition of the timer ID type.
    /// </summary>
    public partial class Timer : IDisposable {

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.AddTimer"/>
        public Timer(uint interval, TimerCallback callback, object? userdata = null) {
            AddTimer(interval, callback, userdata);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.AddTimerNS"/>
        public Timer(ulong interval, NSTimerCallback callback, object? userdata = null) {
            AddTimerNs(interval, callback, userdata);
        }

        /// <inheritdoc cref="Macros.MsPerSecond"/>
        public static uint MsPerSecond => Macros.MsPerSecond;

        /// <inheritdoc cref="Macros.UsPerSecond"/>
        public static uint UsPerSecond => Macros.UsPerSecond;

        /// <inheritdoc cref="Macros.NsPerSecond"/>
        public static ulong NSPerSecond => Macros.NsPerSecond;

        /// <inheritdoc cref="Macros.NsPerMs"/>
        public static uint NsPerMs => Macros.NsPerMs;

        /// <inheritdoc cref="Macros.NsPerUs"/>
        public static uint NsPerUs => Macros.NsPerUs;

        /// <inheritdoc cref="Macros.SecondsToNs"/>
        public static ulong SecondsToNs(ulong seconds) {
            return Macros.SecondsToNs(seconds);
        }

        /// <inheritdoc cref="Macros.NsToSeconds"/>
        public static ulong NsToSeconds(ulong ns) {
            return Macros.NsToSeconds(ns);
        }

        /// <inheritdoc cref="Macros.MsToNs"/>
        public static ulong MsToNs(ulong ms) {
            return Macros.MsToNs(ms);
        }

        /// <inheritdoc cref="Macros.NsToMs"/>
        public static ulong NsToMs(ulong ns) {
            return Macros.NsToMs(ns);
        }

        /// <inheritdoc cref="Macros.UsToNs"/>
        public static ulong UsToNs(ulong us) {
            return Macros.UsToNs(us);
        }

        /// <inheritdoc cref="Macros.NsToUs"/>
        public static ulong NsToUs(ulong ns) {
            return Macros.NsToUs(ns);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetTicks"/>
        public static ulong GetTicks() {
            return SDL.GetTicks();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetTicksNS"/>
        public static ulong GetTicksNs() {
            return SDL.GetTicksNS();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetPerformanceCounter"/>
        public static ulong GetPerformanceCounter() {
            return SDL.GetPerformanceCounter();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.GetPerformanceFrequency"/>
        public static ulong GetPerformanceFrequency() {
            return SDL.GetPerformanceFrequency();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.Delay"/>
        public static void Delay(uint ms) {
            SDL.Delay(ms);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.DelayNS"/>
        public static void DelayNs(ulong ns) {
            SDL.DelayNS(ns);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Timer.DelayPrecise"/>
        public static void DelayPrecise(ulong ns) {
            SDL.DelayPrecise(ns);
        }

    }
}
