// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CSDL {
    /// <summary>Tracks native/managed callback pairs so their function pointers and userdata handles can be looked up and freed later.</summary>
    internal static class CallbackRegistry {
        private abstract class CallbackBinding {
            public required string Id { get; set; }
            public required Delegate PublicCallback { get; init; }
            public required Delegate NativeCallback { get; init; }
            public object? Userdata { get; init; }
            public required IntPtr FunctionPtr { get; init; }
            public required IntPtr UserdataPtr { get; init; }
        }

        private sealed class CallbackBinding<TPublic, TNative> : CallbackBinding
            where TPublic : Delegate
            where TNative : Delegate {
            public required TPublic Public { get; init; }
            public required TNative Native { get; init; }
        }

        private static readonly ConcurrentDictionary<string, CallbackBinding> Callbacks = new ConcurrentDictionary<string, CallbackBinding>();
        private static readonly ConcurrentDictionary<Type, string> Singletons = new ConcurrentDictionary<Type, string>();

        /// <summary>Registers a callback under the given id, pinning its native function pointer and optional userdata.</summary>
        public static (IntPtr functionPtr, IntPtr userdataPtr) Register<TPublic, TNative>(string id, TPublic publicCallback, TNative nativeCallback, object? userdata = null)
            where TPublic : Delegate where TNative : Delegate {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Callback id must not be null or empty.", nameof(id));

            ArgumentNullException.ThrowIfNull(publicCallback);
            ArgumentNullException.ThrowIfNull(nativeCallback);

            IntPtr functionPtr = Marshal.GetFunctionPointerForDelegate(nativeCallback);

            IntPtr userdataPtr = IntPtr.Zero;
            if (userdata is not null) {
                GCHandle userdataHandle = GCHandle.Alloc(userdata);
                userdataPtr = GCHandle.ToIntPtr(userdataHandle);
            }

            CallbackBinding<TPublic, TNative> binding = new CallbackBinding<TPublic, TNative> {
                Id = id,
                Public = publicCallback,
                Native = nativeCallback,
                PublicCallback = publicCallback,
                NativeCallback = nativeCallback,
                Userdata = userdata,
                FunctionPtr = functionPtr,
                UserdataPtr = userdataPtr,
            };

            if (!Callbacks.TryAdd(id, binding)) {
                FreeUserdata(userdataPtr);
                throw new InvalidOperationException($"A callback with id '{id}' is already registered.");
            }

            return (functionPtr, userdataPtr);
        }

        /// <summary>Registers a callback as the sole instance for its native delegate type, replacing any prior registration.</summary>
        public static (IntPtr functionPtr, IntPtr userdataPtr) RegisterSingle<TPublic, TNative>(
            TPublic publicCallback,
            TNative nativeCallback,
            object? userdata = null)
            where TPublic : Delegate
            where TNative : Delegate {
            ArgumentNullException.ThrowIfNull(publicCallback);
            ArgumentNullException.ThrowIfNull(nativeCallback);

            Type key = typeof(TNative);
            string id = $"single:{key.FullName}";

            if (Singletons.TryGetValue(key, out string? existingId)) {
                Unregister<TPublic, TNative>(existingId);
                Singletons.TryRemove(key, out _);
            }

            (IntPtr functionPtr, IntPtr userdataPtr) result =
                Register(id, publicCallback, nativeCallback, userdata);

            Singletons[key] = id;
            return result;
        }

        /// <summary>Removes a registered callback by id and frees its userdata handle.</summary>
        public static bool Unregister<TPublic, TNative>(string id)
            where TPublic : Delegate
            where TNative : Delegate {
            if (!Callbacks.TryGetValue(id, out CallbackBinding? binding))
                return false;

            if (binding is not CallbackBinding<TPublic, TNative>)
                return false;

            if (!Callbacks.TryRemove(id, out CallbackBinding? removed))
                return false;

            FreeUserdata(removed.UserdataPtr);

            if (Singletons.TryGetValue(typeof(TNative), out string? singletonId) && singletonId == id)
                Singletons.TryRemove(typeof(TNative), out _);

            return true;
        }

        /// <summary>Removes the singleton callback registered for the given native delegate type.</summary>
        public static bool UnregisterSingle<TPublic, TNative>()
            where TPublic : Delegate
            where TNative : Delegate {
            Type key = typeof(TNative);
            string id = $"single:{key.FullName}";
            return Unregister<TPublic, TNative>(id);
        }

        /// <summary>Tries to look up the singleton callback registered for the given native delegate type.</summary>
        public static bool TryGet<TPublic, TNative>(
            [NotNullWhen(true)] out TPublic? publicCallback,
            [NotNullWhen(true)] out TNative? nativeCallback,
            out object? userdata)
            where TPublic : Delegate
            where TNative : Delegate {
            Type key = typeof(TNative);
            string id = $"single:{key.FullName}";
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                publicCallback = typed.Public;
                nativeCallback = typed.Native;
                userdata = typed.Userdata;
                return true;
            }

            publicCallback = null;
            nativeCallback = null;
            userdata = null;
            return false;
        }

        /// <summary>Tries to look up a registered callback by id.</summary>
        public static bool TryGet<TPublic, TNative>(
            string id,
            [NotNullWhen(true)] out TPublic? publicCallback,
            [NotNullWhen(true)] out TNative? nativeCallback,
            out object? userdata)
            where TPublic : Delegate
            where TNative : Delegate {
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                publicCallback = typed.Public;
                nativeCallback = typed.Native;
                userdata = typed.Userdata;
                return true;
            }

            publicCallback = null;
            nativeCallback = null;
            userdata = null;
            return false;
        }

        /// <summary>Tries to get the public callback delegate for the singleton registered under the given native delegate type.</summary>
        public static bool TryGetPublic<TPublic, TNative>([NotNullWhen(true)] out TPublic? publicCallback)
            where TPublic : Delegate
            where TNative : Delegate {
            Type key = typeof(TNative);
            string singletonId = $"single:{key.FullName}";
            if (Callbacks.TryGetValue(singletonId, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                publicCallback = typed.Public;
                return true;
            }

            publicCallback = null;
            return false;
        }

        /// <summary>Tries to get the public callback delegate for a registered callback by id.</summary>
        public static bool TryGetPublic<TPublic, TNative>(string id, [NotNullWhen(true)] out TPublic? publicCallback)
            where TPublic : Delegate
            where TNative : Delegate {
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                publicCallback = typed.Public;
                return true;
            }

            publicCallback = null;
            return false;
        }

        /// <summary>Tries to get the native callback delegate for the singleton registered under the given native delegate type.</summary>
        public static bool TryGetNative<TPublic, TNative>([NotNullWhen(true)] out TNative? nativeCallback)
            where TPublic : Delegate
            where TNative : Delegate {
            Type key = typeof(TNative);
            string id = $"single:{key.FullName}";
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                nativeCallback = typed.Native;
                return true;
            }

            nativeCallback = null;
            return false;
        }

        /// <summary>Tries to get the native callback delegate for a registered callback by id.</summary>
        public static bool TryGetNative<TPublic, TNative>(string id, [NotNullWhen(true)] out TNative? nativeCallback)
            where TPublic : Delegate
            where TNative : Delegate {
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                nativeCallback = typed.Native;
                return true;
            }

            nativeCallback = null;
            return false;
        }

        /// <summary>Tries to get the userdata object for a registered callback by id.</summary>
        public static bool TryGetUserdata<TPublic, TNative>(string id, out object? userdata)
            where TPublic : Delegate
            where TNative : Delegate {
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative> typed) {
                userdata = typed.Userdata;
                return true;
            }

            userdata = null;
            return false;
        }

        /// <summary>Tries to get the raw GCHandle pointer to the userdata for a registered callback by id.</summary>
        public static bool TryGetUserdataPtr<TPublic, TNative>(string id, out IntPtr userdataPtr)
            where TPublic : Delegate
            where TNative : Delegate {
            if (Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                binding is CallbackBinding<TPublic, TNative>) {
                userdataPtr = binding.UserdataPtr;
                return true;
            }

            userdataPtr = IntPtr.Zero;
            return false;
        }

        /// <summary>Resolves a GCHandle pointer back to its target userdata object.</summary>
        public static object? GetUserdata(IntPtr userdataPtr) {
            if (userdataPtr == IntPtr.Zero) {
                return null;
            }

            GCHandle handle = GCHandle.FromIntPtr(userdataPtr);
            return handle.Target;
        }

        /// <summary>Resolves a GCHandle pointer back to its target userdata, cast to <typeparamref name="T"/>.</summary>
        [return: MaybeNull]
        public static T GetUserData<T>(IntPtr userdataPtr) {
            object? userdata = GetUserdata(userdataPtr);
            if (userdata is null)
                return default;

            if (userdata is T typed)
                return typed;

            throw new InvalidCastException(
                $"Stored userdata is of type '{userdata.GetType().FullName}', not '{typeof(T).FullName}'.");
        }

        /// <summary>Renames a registered callback's id, updating the singleton mapping if it was the active singleton.</summary>
        public static bool UpdateId<TPublic, TNative>(string oldId, string newId)
            where TPublic : Delegate
            where TNative : Delegate {
            if (string.IsNullOrWhiteSpace(oldId))
                throw new ArgumentException("Old id must not be null or empty.", nameof(oldId));

            if (string.IsNullOrWhiteSpace(newId))
                throw new ArgumentException("New id must not be null or empty.", nameof(newId));

            if (!Callbacks.TryGetValue(oldId, out CallbackBinding? binding))
                return false;

            if (binding is not CallbackBinding<TPublic, TNative> typed)
                return false;

            if (Callbacks.ContainsKey(newId))
                return false;

            if (!Callbacks.TryRemove(oldId, out _))
                return false;

            typed.Id = newId;

            if (!Callbacks.TryAdd(newId, typed)) {
                typed.Id = oldId;
                Callbacks.TryAdd(oldId, typed);
                return false;
            }

            if (Singletons.TryGetValue(typeof(TNative), out string? singletonId) && singletonId == oldId)
                Singletons[typeof(TNative)] = newId;

            return true;
        }

        /// <summary>Checks whether a callback with the given id and delegate types is registered.</summary>
        public static bool IsRegistered<TPublic, TNative>(string id)
            where TPublic : Delegate
            where TNative : Delegate {
            return Callbacks.TryGetValue(id, out CallbackBinding? binding) &&
                   binding is CallbackBinding<TPublic, TNative>;
        }

        /// <summary>Searches all registered callbacks for one whose public delegate (and optionally userdata) matches by reference.</summary>
        public static bool TryFindByManagedCallback<TPublic, TNative>(
            TPublic publicCallback,
            object? userdata,
            [NotNullWhen(true)] out string? id,
            [NotNullWhen(true)] out TNative? nativeCallback,
            out IntPtr userdataPtr)
            where TPublic : Delegate
            where TNative : Delegate {
            ArgumentNullException.ThrowIfNull(publicCallback);

            foreach (KeyValuePair<string, CallbackBinding> kvp in Callbacks) {
                if (kvp.Value is CallbackBinding<TPublic, TNative> typed &&
                    ReferenceEquals(typed.Public, publicCallback)) {

                    // If userdata is provided, it must match
                    if (userdata != null && !ReferenceEquals(typed.Userdata, userdata)) {
                        continue;
                    }

                    id = kvp.Key;
                    nativeCallback = typed.Native;
                    userdataPtr = typed.UserdataPtr;
                    return true;
                }
            }

            id = null;
            nativeCallback = null;
            userdataPtr = IntPtr.Zero;
            return false;
        }

        /// <summary>Frees the GCHandle backing a userdata pointer, if any.</summary>
        private static void FreeUserdata(IntPtr userdataPtr) {
            if (userdataPtr == IntPtr.Zero)
                return;

            GCHandle handle = GCHandle.FromIntPtr(userdataPtr);
            if (handle.IsAllocated)
                handle.Free();
        }
    }
}
