// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.CompilerServices;
namespace CSDL.Extensions {
    internal static class CBoolExtensions {
        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the result is <see langword="false"/>.
        /// </summary>
        /// <remarks>
        ///     Use this for calls whose failure should abort the operation - typically construction,
        ///     where there is nothing sensible left to do but abort.
        /// </remarks>
        internal static bool ThrowIfFalse(this CBool result, [CallerArgumentExpression(nameof(result))] string? operation = null) {
            if (!result) {
                Error.ThrowIfError(operation ?? "SDL operation");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Logs the current SDL error (via <see cref="Error.LogError"/>) if the result is
        ///     <see langword="false"/>, without throwing.
        /// </summary>
        /// <remarks>
        ///     Use this for calls where failure shouldn't abort the caller - either the operation is
        ///     genuinely non-critical, or SDL's documented failure conditions turn out to be broader
        ///     than what actually warrants aborting.
        /// </remarks>
        internal static bool LogIfFalse(this CBool result, [CallerArgumentExpression(nameof(result))] string? operation = null) {
            if (!result) {
                Error.LogError(operation ?? "SDL operation");
                return false;
            }

            return true;
        }
    }
}
