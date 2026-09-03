// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CSDL.Extensions {
    internal static class NullableExtensions {
        /// <summary>
        ///     Gets a <c>ref readonly</c> to <paramref name="nullable"/>'s value, or a null reference if it
        ///     has none.
        /// </summary>
        /// <remarks>
        ///     Use this to pass an optional struct to a P/Invoke import that takes it by <c>in</c> - SDL
        ///     treats a null pointer there as "unset"/"use the default".
        /// </remarks>
        internal static ref readonly T AsRef<T>(this T? nullable, [UnscopedRef] out T storage) where T : struct {
            storage = nullable.GetValueOrDefault();
            return ref nullable.HasValue ? ref storage : ref Unsafe.NullRef<T>();
        }
    }
}
