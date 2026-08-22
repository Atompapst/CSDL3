// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSDL.Extensions {
    internal static class NativePtrExtensions {
        /// <summary>
        ///     Reads a native UTF-8 <c>const char*</c> as a managed string, without freeing it.
        /// </summary>
        /// <remarks>
        ///     Use this for pointers returned by SDL - they point to memory SDL owns (static buffers,
        ///     thread-locals) and must never be released through .NET's marshaller. Returns <c>null</c>
        ///     if the pointer is null.
        /// </remarks>
        internal static string? ToUtf8String(this NativePtr<byte> ptr) {
            return ptr.IsNull ? null : Marshal.PtrToStringUTF8(ptr.Ptr);
        }

        /// <summary>
        ///     Reads a native UTF-8 <c>const char*</c> as a managed string, then frees the pointer with
        ///     <c>SDL_free</c>.
        /// </summary>
        /// <remarks>
        ///     Use this only for the handful of SDL functions whose documentation says the returned
        ///     string must be freed by the caller (e.g. <c>SDL_GetClipboardText</c>). Calling this on a
        ///     pointer SDL still owns internally - the common case, see <see cref="ToUtf8String"/> -
        ///     would free memory SDL is still using.
        /// </remarks>
        internal static string? ToUtf8StringAndFree(this NativePtr<byte> ptr) {
            string? result = ptr.ToUtf8String();
            ptr.Free();
            return result;
        }

        /// <summary>
        ///     Reads a native UTF-8 <c>const char*</c> as a managed string; if the pointer is null, logs
        ///     the current SDL error via <see cref="Error.LogError"/> and returns <see langword="null"/>.
        /// </summary>
        /// <remarks>
        ///     Use this for string-returning SDL calls whose documentation says NULL signals a real
        ///     failure (e.g. <c>SDL_GetBasePath</c>), as opposed to ones that never return NULL (e.g.
        ///     <c>SDL_GetAudioFormatName</c> falls back to "SDL_AUDIO_UNKNOWN") - for those, plain
        ///     <see cref="ToUtf8String"/> is enough, the null check here would never fire.
        /// </remarks>
        internal static string? ToUtf8StringOrLog(this NativePtr<byte> ptr, [CallerArgumentExpression(nameof(ptr))] string? operation = null) {
            if (ptr.IsNull) {
                Error.LogError(operation ?? "SDL operation");
                return null;
            }

            return ptr.ToUtf8String();
        }

        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the pointer is null.
        /// </summary>
        /// <remarks>
        ///     Use this after an SDL call that hands back a new handle/instance, where a null pointer
        ///     means construction failed and there is nothing sensible left to do but abort.
        /// </remarks>
        internal static NativePtr<T> ThrowIfInvalid<T>(
            this NativePtr<T> ptr,
            [CallerArgumentExpression(nameof(ptr))] string? operation = null) where T : unmanaged {
            if (ptr.IsNull) {
                Error.ThrowIfError(operation ?? "SDL operation");
            }

            return ptr;
        }

        /// <summary>
        ///     Logs the current SDL error (via <see cref="Error.LogError"/>) if the pointer is null,
        ///     without throwing.
        /// </summary>
        /// <remarks>
        ///     Use this where a null result shouldn't abort the caller - e.g. a query that can be
        ///     retried or corrected later some other way (a device list refreshed again once hotplug
        ///     events arrive), as opposed to <see cref="ThrowIfInvalid{T}"/>'s construction-failure case.
        /// </remarks>
        internal static NativePtr<T> LogIfInvalid<T>(
            this NativePtr<T> ptr,
            [CallerArgumentExpression(nameof(ptr))] string? operation = null) where T : unmanaged {
            if (ptr.IsNull) {
                Error.LogError(operation ?? "SDL operation");
            }

            return ptr;
        }
    }
}
