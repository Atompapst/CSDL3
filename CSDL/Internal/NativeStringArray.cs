// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSDL {
    /// <summary>
    /// Helper for converting native char** pointers to managed string arrays.
    /// </summary>
    internal static class NativeStringArray {
        /// <summary>
        /// Converts a pointer to an array of C strings to a managed string[] using the provided count.
        /// </summary>
        public static string[] ToArray(nint ptr, int count) {
            if (count <= 0 || ptr == IntPtr.Zero) return Array.Empty<string>();
            string[] result = new string[count];
            for (int i = 0; i < count; i++) {
                IntPtr p = Marshal.ReadIntPtr((IntPtr)ptr, i * IntPtr.Size);
                result[i] = p == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(p) ?? string.Empty;
            }
            return result;
        }

        /// <summary>
        /// Converts a null-terminated array of C strings to a managed string[].
        /// </summary>
        public static string[] ToArray(nint ptr) {
            if (ptr == IntPtr.Zero) return Array.Empty<string>();
            List<string> list = new List<string>();
            int index = 0;
            // Only used for SDL_DialogFileCallback's filelist, which SDL documents as NULL-terminated
            // (no accompanying count) - not safe to use with an arbitrary, undocumented char** source.
            while (true) {
                IntPtr p = Marshal.ReadIntPtr((IntPtr)ptr, index * IntPtr.Size);
                if (p == IntPtr.Zero) break;
                list.Add(Marshal.PtrToStringUTF8(p) ?? string.Empty);
                index++;
            }
            return list.ToArray();
        }

        /// <summary>
        /// Allocates a native <c>char**</c>-style argv from a managed string[] - one UTF8, null-terminated
        /// copy per element, packed into an array of pointers. Dispose the result to free both.
        /// </summary>
        public static Native Allocate(string[] args) {
            args ??= Array.Empty<string>();

            IntPtr[] entries = new IntPtr[args.Length];
            for (int i = 0; i < args.Length; i++) {
                entries[i] = Marshal.StringToCoTaskMemUTF8(args[i] ?? string.Empty);
            }

            IntPtr argv = Marshal.AllocCoTaskMem(IntPtr.Size * Math.Max(args.Length, 1));
            for (int i = 0; i < args.Length; i++) {
                Marshal.WriteIntPtr(argv, i * IntPtr.Size, entries[i]);
            }

            return new Native(argv, entries, args.Length);
        }

        /// <summary>
        /// Allocates a native <c>char**</c> like <see cref="Allocate"/>, but with an extra NULL entry
        /// after the last string - the shape C APIs expect when they take an argv without a separate
        /// count (e.g. <c>SDL_CreateProcess</c>). Dispose the result to free both.
        /// </summary>
        public static Native AllocateNullTerminated(string[] args) {
            args ??= Array.Empty<string>();

            IntPtr[] entries = new IntPtr[args.Length];
            for (int i = 0; i < args.Length; i++) {
                entries[i] = Marshal.StringToCoTaskMemUTF8(args[i] ?? string.Empty);
            }

            IntPtr argv = Marshal.AllocCoTaskMem(IntPtr.Size * (args.Length + 1));
            for (int i = 0; i < args.Length; i++) {
                Marshal.WriteIntPtr(argv, i * IntPtr.Size, entries[i]);
            }
            Marshal.WriteIntPtr(argv, args.Length * IntPtr.Size, IntPtr.Zero);

            return new Native(argv, entries, args.Length);
        }

        /// <summary>
        /// Owns a native argv array allocated by <see cref="Allocate"/>; frees every entry and the
        /// pointer array itself on <see cref="Dispose"/>.
        /// </summary>
        public readonly struct Native : IDisposable {
            private readonly IntPtr[] _entries;
            private readonly IntPtr _argv;

            internal Native(IntPtr argv, IntPtr[] entries, int count) {
                _argv = argv;
                _entries = entries;
                Count = count;
            }

            /// <summary>The number of argv entries.</summary>
            public int Count { get; }

            /// <summary>The native <c>char**</c> pointer.</summary>
            public NativePtr<nint> Ptr => new NativePtr<nint>(_argv);

            public void Dispose() {
                if (_entries != null) {
                    foreach (IntPtr entry in _entries) {
                        if (entry != IntPtr.Zero) Marshal.FreeCoTaskMem(entry);
                    }
                }

                if (_argv != IntPtr.Zero) Marshal.FreeCoTaskMem(_argv);
            }
        }
    }
}
