// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL {
    public static class Error {
        // SDL's error state is per-thread
        [System.ThreadStatic]
        private static byte[]? _lastErrorBuffer;
        [System.ThreadStatic]
        private static int _lastErrorLength;

        /// <summary>
        /// Raised whenever <see cref="LogError"/> or one of the <c>Throw*</c> methods observes
        /// a real SDL error, with the message that was reported.
        /// </summary>
        /// <remarks>
        /// Not part of the SDL API - a addition for reacting to failures without
        /// having to poll <see cref="LastError"/> yourself.
        /// </remarks>
        public static event System.Action<string>? OnError;

        /// <summary>
        /// The most recent SDL error message observed on this thread via <see cref="LogError"/>
        /// or one of the <c>Throw*</c> methods, or empty if none has occurred yet.
        /// </summary>
        /// <remarks>
        /// Not part of the SDL API. In plain C, <c>SDL_GetError()</c> is enough on its own - you call it
        /// right after a failing call, before anything else touches that thread's error state. This
        /// wrapper already made that call for you as part of the <c>bool</c>-returning call convention
        /// (see e.g. <see cref="CBoolExtensions.LogIfFalse"/>), so by the time you see a returned
        /// <see langword="false"/> there's nothing left to read via <see cref="GetError"/> unless SDL's
        /// error state happens to still say the same thing - this keeps a copy around regardless, so
        /// you always have somewhere to look. If you threw instead of logged, the exception already
        /// carries its own copy - see <see cref="SDLException.SdlError"/>.
        /// <para>
        /// Note that unlike <c>SDL_ClearError</c>, nothing here clears SDL's own error state - see
        /// <see cref="LogError"/>.
        /// </para>
        /// </remarks>
        public static string LastError {
            get {
                byte[]? buffer = _lastErrorBuffer;
                return buffer == null || _lastErrorLength == 0
                    ? string.Empty
                    : System.Text.Encoding.UTF8.GetString(buffer, 0, _lastErrorLength);
            }
        }

        /// <remarks>
        /// Reads SDL's current thread-local error state directly. For errors already observed
        /// by this wrapper, prefer <see cref="LastError"/>, which keeps a stable managed copy.
        /// </remarks>
        /// <seealso cref="CSDL.Internal.Docs.Error.GetError">GetError</seealso>
        public static string GetError() {
            return SDL.GetError().ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Error.SetError"/>
        /// <remarks>
        /// <paramref name="message"/> is treated as literal text, not as a printf-style format string:
        /// any <c>%</c> it contains is escaped before being forwarded to SDL. Natively, <c>SDL_SetError</c>
        /// is variadic and interprets its format argument with printf semantics; since this overload never
        /// supplies substitution arguments, passing a message straight through would let a stray <c>%</c>
        /// (e.g. from dynamic or user-supplied text) make SDL read nonexistent varargs.
        /// </remarks>
        public static bool SetError(string fmt) {
            return SDL.SetError(fmt.Replace("%", "%%"));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Error.SetErrorV"/>
        /// <remarks>
        /// This replaces <c>%s</c> placeholders in <paramref name="fmt"/> with the corresponding
        /// <paramref name="args"/> in C# before setting the error message. Since C's <c>va_list</c>
        /// cannot be safely marshaled from C#, this method performs the formatting in managed code
        /// instead of calling SDL's native <c>SDL_SetErrorV</c>.
        /// </remarks>
        public static void SetErrorV(string fmt, params string[] args) {
            string formatted = FormatV(fmt, args);
            SDL.SetError(formatted.Replace("%", "%%"));
        }

        private static string FormatV(string format, string[] args) {
            if (string.IsNullOrEmpty(format) || args is null || args.Length == 0) {
                return format ?? string.Empty;
            }

            System.Text.StringBuilder result = new System.Text.StringBuilder(format.Length);
            int argIndex = 0;

            for (int i = 0; i < format.Length; i++) {
                char c = format[i];
                if (c == '%' && i + 1 < format.Length) {
                    char next = format[i + 1];
                    if (next == 's' && argIndex < args.Length) {
                        result.Append(args[argIndex++]);
                        i++;
                        continue;
                    }
                    if (next == '%') {
                        result.Append('%');
                        i++;
                        continue;
                    }
                }
                result.Append(c);
            }
            return result.ToString();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Error.OutOfMemory"/>
        public static void OutOfMemory() {
            SDL.OutOfMemory();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Error.ClearError"/>
        public static bool ClearError() {
            return SDL.ClearError();
        }

        /// <summary>
        /// Throws an <see cref="SDLException"/> with the current SDL error if there is one.
        /// </summary>
        /// <remarks>
        /// Also records the message to <see cref="LastError"/> and raises <see cref="OnError"/> before
        /// throwing, same as <see cref="LogError"/> does for the non-throwing path - though here
        /// the exception's own <see cref="SDLException.SdlError"/> already carries the message too.
        /// </remarks>
        public static void ThrowIfError(string operation = "SDL operation") {
            string error = GetError();
            if (!string.IsNullOrWhiteSpace(error)) {
                RecordError(error);
                throw new SDLException(operation, error);
            }
        }

        /// <summary>
        /// Throws an <see cref="SDLException"/> using the current SDL error, or a fallback message if none exists.
        /// </summary>
        public static void Throw(string operation = "SDL operation") {
            string error = GetError();
            string message = string.IsNullOrWhiteSpace(error) ? "Unknown SDL error" : error;
            RecordError(message);
            throw new SDLException(operation, message);
        }

        /// <summary>
        /// Gets the current error and clears it.
        /// </summary>
        public static string GetErrorAndClear() {
            string error = GetError();
            ClearError();
            return error;
        }

        /// <inheritdoc cref="CSDL.Macros.InvalidParamError"/>
        public static void SetInvalidParamError(string paramName) {
            Macros.InvalidParamError(paramName);
        }

        /// <inheritdoc cref="CSDL.Macros.Unsupported"/>
        public static void SetUnsupported() {
            Macros.Unsupported();
        }

        /// <summary>
        /// Sets SDL's standardized invalid-parameter error and throws an <see cref="SDLInvalidParamException"/>.
        /// </summary>
        public static void ThrowInvalidParam(string paramName, string operation = "SDL operation") {
            SetInvalidParamError(paramName);
            string error = GetError();
            RecordError(error);
            throw new SDLInvalidParamException(operation, paramName, error);
        }

        /// <summary>
        /// Throws an <see cref="SDLNullParamException"/> when the value is null.
        /// </summary>
        public static T ThrowIfNull<T>(T value, string paramName, string operation = "SDL operation") where T : class {
            if (value == null) {
                SetInvalidParamError(paramName);
                string error = GetError();
                RecordError(error);
                throw new SDLNullParamException(operation, paramName, error);
            }

            return value;
        }

        /// <summary>
        /// Throws an <see cref="SDLInvalidParamException"/> when the value is null, empty, or whitespace.
        /// </summary>
        public static string ThrowIfNullOrWhiteSpace(string value, string paramName, string operation = "SDL operation") {
            if (string.IsNullOrWhiteSpace(value)) {
                ThrowInvalidParam(paramName, operation);
            }
            return value;
        }

        /// <summary>
        /// Logs the current SDL error, if any, recording it to <see cref="LastError"/> and raising
        /// <see cref="OnError"/> along the way.
        /// </summary>
        internal static void LogError(string operation = "SDL operation") {
            NativePtr<byte> error = SDL.GetError();
            if (!error.IsNull) {
                RecordError(error);
                Log.Error($"{operation} failed: {LastError}");
            }
        }

        /// <summary>
        /// Records <paramref name="message"/> to <see cref="LastError"/> and raises
        /// <see cref="OnError"/>. For callers that already had to materialize a managed string anyway
        /// (the <c>Throw*</c> methods build an exception message from it regardless), so there's no
        /// extra allocation to avoid here.
        /// </summary>
        private static void RecordError(string message) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            _lastErrorBuffer = bytes;
            _lastErrorLength = bytes.Length;

            OnError?.Invoke(message);
        }

        private static void RecordError(NativePtr<byte> error) {
            int length = 0;
            while (error[length] != 0) {
                length++;
            }

            byte[]? buffer = _lastErrorBuffer;
            if (buffer == null || buffer.Length < length) {
                buffer = new byte[length];
                _lastErrorBuffer = buffer;
            }
            System.Runtime.InteropServices.Marshal.Copy(error.Ptr, buffer, 0, length);
            _lastErrorLength = length;

            if (OnError != null) {
                OnError(LastError);
            }
        }
    }
}
