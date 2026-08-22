// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CSDL {
    // Allows multiple timers to be registered with SDL.
    public partial class Timer {
        private uint _timerId;
        private nint _token;
        private int _disposed;
        private static long _nextToken;
        private static readonly ConcurrentDictionary<nint, Registration> Registrations = new ConcurrentDictionary<nint, Registration>();
        private static readonly SDL_TimerCallbackNative MillisecondEntry = InvokeMillisecond;
        private static readonly SDL_NSTimerCallbackNative NanosecondEntry = InvokeNanosecond;

        private sealed class Registration {
            public required object? Userdata { get; init; }
            public TimerCallback? MillisecondCallback { get; init; }
            public NSTimerCallback? NanosecondCallback { get; init; }
        }

        private void AddTimer(uint interval, TimerCallback callback, object? userdata) {
            ArgumentNullException.ThrowIfNull(callback);

            _token = Register(new Registration { Userdata = userdata, MillisecondCallback = callback });
            _timerId = SDL.AddTimer(interval, MillisecondEntry, _token);
            if (_timerId == 0) {
                Registrations.TryRemove(_token, out _);
                throw new SDLException($"Failed to create timer: {SDL.GetError()}");
            }
        }

        private void AddTimerNs(ulong interval, NSTimerCallback callback, object? userdata) {
            ArgumentNullException.ThrowIfNull(callback);

            _token = Register(new Registration { Userdata = userdata, NanosecondCallback = callback });
            _timerId = SDL.AddTimerNS(interval, NanosecondEntry, _token);
            if (_timerId == 0) {
                Registrations.TryRemove(_token, out _);
                throw new SDLException($"Failed to create timer: {SDL.GetError()}");
            }
        }

        private static nint Register(Registration registration) {
            nint token;
            do {
                token = (nint)Interlocked.Increment(ref _nextToken);
            } while (token == nint.Zero || !Registrations.TryAdd(token, registration));
            return token;
        }

        private static uint InvokeMillisecond(nint token, TimerID timerId, uint interval) {
            if (!Registrations.TryGetValue(token, out Registration? registration)) {
                return 0;
            }

            uint nextInterval = 0;
            try {
                nextInterval = registration.MillisecondCallback!(registration.Userdata, timerId, interval);
                return nextInterval;
            } catch (Exception ex) {
                Log.Error(ex, "Managed timer callback threw an exception.");
                return 0;
            } finally {
                if (nextInterval == 0) {
                    Registrations.TryRemove(token, out _);
                }
            }
        }

        private static ulong InvokeNanosecond(nint token, TimerID timerId, ulong interval) {
            if (!Registrations.TryGetValue(token, out Registration? registration)) {
                return 0;
            }

            ulong nextInterval = 0;
            try {
                nextInterval = registration.NanosecondCallback!(registration.Userdata, timerId, interval);
                return nextInterval;
            } catch (Exception ex) {
                Log.Error(ex, "Managed timer callback threw an exception.");
                return 0;
            } finally {
                if (nextInterval == 0) {
                    Registrations.TryRemove(token, out _);
                }
            }
        }

        /// <summary>
        ///     Unregisters the timer's callback and removes the underlying SDL timer.
        /// </summary>
        /// <seealso cref="CSDL.Internal.Docs.Timer.RemoveTimer">RemoveTimer</seealso>
        public void Dispose() {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) {
                return;
            }

            if (_timerId != 0) {
                SDL.RemoveTimer(_timerId);
            }
            if (_token != nint.Zero) {
                Registrations.TryRemove(_token, out _);
            }
            GC.SuppressFinalize(this);
        }
    }
}
