// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Extensions {
    internal static class NativeHandleExtensions {
        /// <summary>
        ///     Rejects a handle that cannot be used, naming the parameter it arrived in.
        /// </summary>
        /// <exception cref="ArgumentNullException">A class-shaped handle was <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">The handle is disposed, retired, or uninitialised.</exception>
        internal static void ThrowIfInvalid<THandle>(this THandle handle, string parameterName)
            where THandle : INativeHandle {
            if (handle is null) {
                throw new ArgumentNullException(parameterName);
            }

            if (!handle.IsValid) {
                throw new ArgumentException(
                    $"The {typeof(THandle).Name} has been disposed, was retired by its owner, or was never initialised.",
                    parameterName);
            }
        }
    }
}
