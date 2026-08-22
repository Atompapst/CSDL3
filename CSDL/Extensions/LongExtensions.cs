// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.CompilerServices;

namespace CSDL.Extensions {
    internal static class LongExtensions {
        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the value is <c>-1</c>.
        /// </summary>
        /// <remarks>
        ///     Use this for <c>Sint64</c>-returning SDL calls whose documentation says <c>-1</c>
        ///     signals a real failure (e.g. <c>SDL_SeekIO</c>, <c>SDL_TellIO</c>).
        /// </remarks>
        internal static long ThrowIfInvalid(this long value, long check, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == check) {
                Error.ThrowIfError(operation ?? "SDL operation");
            }

            return value;
        }

        /// <summary>
        ///     Logs the current SDL error (via <see cref="Error.LogError"/>) if the value is
        ///     <c>-1</c>, without throwing.
        /// </summary>
        internal static long LogIfInvalid(this long value, long check, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == check) {
                Error.LogError(operation ?? "SDL operation");
            }

            return value;
        }
    }
}
