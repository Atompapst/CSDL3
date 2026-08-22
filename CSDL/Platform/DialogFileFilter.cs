// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSDL {
    public partial struct DialogFileFilter {
        /// <summary>
        ///     Builds a filter from already-allocated native UTF-8 strings.
        /// </summary>
        /// <remarks>
        ///     Kept internal on purpose: the struct only points at the two strings, so whoever allocated
        ///     them has to outlive the dialog. Use <see cref="FileDialog"/>'s filter overloads or
        ///     <see cref="FileDialogProperties.SetFilters"/> instead of doing that by hand.
        /// </remarks>
        internal DialogFileFilter(nint name, nint pattern) {
            _name = name;
            _pattern = pattern;
        }
    }

    /// <summary>
    ///     A native <c>SDL_DialogFileFilter[]</c> owning the UTF-8 strings it points at.
    /// </summary>
    /// <remarks>
    ///     SDL requires the filter array to stay valid at least until the dialog's callback runs, which
    ///     is why this is a separate, explicitly-disposed allocation rather than a pinned managed array.
    /// </remarks>
    internal readonly struct NativeFileFilters : IDisposable {
        private readonly nint _filters;
        private readonly nint[] _strings;

        private NativeFileFilters(nint filters, nint[] strings, int count) {
            _filters = filters;
            _strings = strings;
            Count = count;
        }

        /// <summary>The number of filters in the array.</summary>
        public int Count { get; }

        /// <summary>The native array pointer, or null if there are no filters.</summary>
        public NativePtr<DialogFileFilter> Ptr => new NativePtr<DialogFileFilter>(_filters);

        public static NativeFileFilters Allocate(IReadOnlyList<(string Name, string Pattern)>? filters) {
            if (filters == null || filters.Count == 0) {
                return new NativeFileFilters(nint.Zero, Array.Empty<nint>(), 0);
            }

            int count = filters.Count;
            nint[] strings = new nint[count * 2];
            nint block = Marshal.AllocCoTaskMem(count * Marshal.SizeOf<DialogFileFilter>());
            NativePtr<DialogFileFilter> array = new NativePtr<DialogFileFilter>(block);

            for (int i = 0; i < count; i++) {
                nint name = Marshal.StringToCoTaskMemUTF8(filters[i].Name ?? string.Empty);
                nint pattern = Marshal.StringToCoTaskMemUTF8(filters[i].Pattern ?? string.Empty);
                strings[i * 2] = name;
                strings[i * 2 + 1] = pattern;
                array[i] = new DialogFileFilter(name, pattern);
            }

            return new NativeFileFilters(block, strings, count);
        }

        public void Dispose() {
            if (_strings != null) {
                foreach (nint str in _strings) {
                    if (str != nint.Zero) Marshal.FreeCoTaskMem(str);
                }
            }

            if (_filters != nint.Zero) Marshal.FreeCoTaskMem(_filters);
        }
    }
}
