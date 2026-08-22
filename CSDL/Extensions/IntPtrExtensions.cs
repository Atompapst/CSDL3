// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;

namespace CSDL.Extensions {
    internal static class IntPtrExtensions {
        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the pointer is <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        ///     For raw <see cref="IntPtr"/> results from SDL calls that don't go through the
        ///     <c>NativePtr&lt;T&gt;</c> marshalling (e.g. <c>SDL_MapGPUTransferBuffer</c>), where NULL
        ///     signals a real failure.
        /// </remarks>
        internal static IntPtr ThrowIfInvalid(this IntPtr ptr, [CallerArgumentExpression(nameof(ptr))] string? operation = null) {
            if (ptr == IntPtr.Zero) {
                Error.ThrowIfError(operation ?? "SDL operation");
            }

            return ptr;
        }

        /// <summary>
        ///     Logs the current SDL error (via <see cref="Error.LogError"/>) if the pointer is
        ///     <see cref="IntPtr.Zero"/>, without throwing.
        /// </summary>
        internal static IntPtr LogIfInvalid(this IntPtr ptr, [CallerArgumentExpression(nameof(ptr))] string? operation = null) {
            if (ptr == IntPtr.Zero) {
                Error.LogError(operation ?? "SDL operation");
            }

            return ptr;
        }
    }
}
