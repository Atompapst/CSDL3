// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL {
    /// <summary>
    /// Exception thrown for SDL-specific failures.
    /// </summary>
    public class SDLException : Exception {
        public string Operation { get; }
        public string SdlError { get; }

        public SDLException(string operation, string sdlError)
            : base($"{operation} failed: {sdlError}") {
            Operation = operation ?? string.Empty;
            SdlError = sdlError ?? string.Empty;
        }

        public SDLException(string message)
            : base(message) {
            Operation = string.Empty;
            SdlError = string.Empty;
        }

        public SDLException(string message, Exception innerException)
            : base(message, innerException) {
            Operation = string.Empty;
            SdlError = string.Empty;
        }

    }

    /// <summary>
    /// Exception thrown when SDL reports an invalid parameter.
    /// </summary>
    public class SDLInvalidParamException : SDLException {
        public string ParamName { get; }

        public SDLInvalidParamException(string operation, string paramName, string sdlError)
            : base(operation, sdlError) {
            ParamName = paramName ?? string.Empty;
        }

        public SDLInvalidParamException(string paramName)
            : this("SDL operation", paramName, $"Parameter '{paramName}' is invalid") { }
    }

    /// <summary>
    /// Exception thrown when SDL reports a null parameter as invalid.
    /// </summary>
    public sealed class SDLNullParamException : SDLInvalidParamException {
        public SDLNullParamException(string operation, string paramName, string sdlError)
            : base(operation, paramName, sdlError) { }

        public SDLNullParamException(string paramName)
            : this("SDL operation", paramName, $"Parameter '{paramName}' is invalid") { }
    }
}
