// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CSDL.Extensions;

namespace CSDL {
    /// <summary>
    /// Provides utilities for managing SDL hints and their configurations.
    /// </summary>
    public static partial class Hints {
        private static readonly ConcurrentDictionary<string, Hint> _hints = new ConcurrentDictionary<string, Hint>(StringComparer.Ordinal);

        /// <summary>
        /// Retrieves or creates a new instance of a hint with the specified name.
        /// </summary>
        /// <param name="name">The name of the hint to retrieve or create.</param>
        /// <returns>A <see cref="CSDL.Hints.Hint">Hint</see> instance associated with the given name.</returns>
        /// <seealso cref="CSDL.Hints.Names">Hints.Names</seealso>
        public static Hint For(string name) {
            return _hints.GetOrAdd(name, static n => new Hint(n));
        }

        static Hints() {
            Init.OnQuit += DisposeAll;
        }

        /// <summary>
        /// Represents a configurable hint with associated operations for managing its value,
        /// priority, and callback within the SDL environment.
        /// </summary>
        public class Hint : IDisposable {
            private readonly string _id;
            private HintCallback? Callback { get; set; }
            private IntPtr DataPtr { get; set; }
            public string Name { get; }

            public Hint(string name) {
                ArgumentNullException.ThrowIfNull(name);
                Name = name;
                _id = $"Hint:{Name}:{Guid.NewGuid()}";
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.SetHintWithPriority"/>
            public bool SetWithPriority(string value, HintPriority priority) {
                return SDL.SetHintWithPriority(Name, value, priority).LogIfFalse();
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.SetHint"/>
            public bool Set(string value) {
                return SDL.SetHint(Name, value).LogIfFalse();
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.ResetHint"/>
            public bool Reset() {
                return SDL.ResetHint(Name).LogIfFalse();
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.GetHint"/>
            public string? Get() {
                return SDL.GetHint(Name).ToUtf8String();
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.GetHintBoolean"/>
            public bool GetBoolean(bool defaultValue = false) {
                return SDL.GetHintBoolean(Name, defaultValue);
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.AddHintCallback"/>
            public void AddCallback(HintCallback callback, object? userdata = null) {
                ArgumentNullException.ThrowIfNull(callback);

                // this object represents a single callback registration.
                // Remove the previous one before replacing it so its native delegate and userdata remain valid.
                RemoveCallback();

                SDL_HintCallbackNative native = HintCallbackWrapper.Create(callback);
                (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(_id, callback, native, userdata);

                try {
                    SDL.AddHintCallback(Name, native, cb.userdataPtr)
                        .ThrowIfFalse(nameof(SDL.AddHintCallback));

                    Callback = callback;
                    DataPtr = cb.userdataPtr;
                } catch {
                    CallbackRegistry.Unregister<HintCallback, SDL_HintCallbackNative>(_id);
                    throw;
                }
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Hints.RemoveHintCallback"/>
            public void RemoveCallback() {
                if (CallbackRegistry.TryGetNative<HintCallback, SDL_HintCallbackNative>(_id, out SDL_HintCallbackNative? callback)) {
                    SDL.RemoveHintCallback(Name, callback, DataPtr);
                    CallbackRegistry.Unregister<HintCallback, SDL_HintCallbackNative>(_id);
                }

                Callback = null;
                DataPtr = IntPtr.Zero;
            }

            public void Dispose() {
                RemoveCallback();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Hints.ResetHints"/>
        public static void ResetAll() {
            SDL.ResetHints();
        }

        internal static void DisposeAll() {
            foreach (KeyValuePair<string, Hint> pair in _hints) {
                pair.Value.Dispose();
            }
            _hints.Clear();
        }
    }
}
