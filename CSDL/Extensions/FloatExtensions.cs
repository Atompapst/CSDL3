// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.CompilerServices;
namespace CSDL.Extensions {
    internal static class FloatExtensions {
        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the value is the SDL failure sentinel
        ///     (-1.0f).
        /// </summary>
        internal static float ThrowIfInvalid(this float value, float check, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == check) {
                Error.ThrowIfError(operation ?? "SDL operation");
            }

            return value;
        }

        /// <summary>
        ///     Logs the current SDL error and clears it (via <see cref="Error.LogError"/>) if the
        ///     value is the SDL failure sentinel (-1.0f), without throwing.
        /// </summary>
        internal static float LogIfInvalid(this float value, float check, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == check) {
                Error.LogError(operation ?? "SDL operation");
            }

            return value;
        }
    }
}
