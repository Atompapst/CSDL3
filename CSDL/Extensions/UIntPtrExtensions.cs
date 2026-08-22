// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;

namespace CSDL.Extensions {
    internal static class UIntPtrExtensions {
        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the value is <see cref="UIntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        ///     Use this for <c>size_t</c>-returning SDL calls whose documentation says 0 signals a
        ///     real failure (e.g. <c>SDL_IOprintf</c>), as opposed to ones where 0 is also a valid,
        ///     non-error result (e.g. <c>SDL_ReadIO</c>/<c>SDL_WriteIO</c> with a zero-length request) -
        ///     for those, leave the raw value alone and let the caller check <c>SDL_GetIOStatus</c>.
        /// </remarks>
        internal static UIntPtr ThrowIfInvalid(this UIntPtr value, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == UIntPtr.Zero) {
                Error.ThrowIfError(operation ?? "SDL operation");
            }

            return value;
        }

        /// <summary>
        ///     Logs the current SDL error (via <see cref="Error.LogError"/>) if the value is
        ///     <see cref="UIntPtr.Zero"/>, without throwing.
        /// </summary>
        internal static UIntPtr LogIfInvalid(this UIntPtr value, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == UIntPtr.Zero) {
                Error.LogError(operation ?? "SDL operation");
            }

            return value;
        }
    }
}
