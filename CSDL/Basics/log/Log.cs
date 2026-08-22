// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using CSDL.Extensions;
using CSDL.Internal.Logging;
namespace CSDL {
    public static partial class Log {
        private static readonly Dictionary<LogPriority, Action<LogCategory, string>> SdlDispatch =
            new Dictionary<LogPriority, Action<LogCategory, string>> {
                [LogPriority.Trace] = (c, m) => SDL.LogTrace((int)c, m),
                [LogPriority.Verbose] = (c, m) => SDL.LogVerbose((int)c, m),
                [LogPriority.Debug] = (c, m) => SDL.LogDebug((int)c, m),
                [LogPriority.Info] = (c, m) => SDL.LogInfo((int)c, m),
                [LogPriority.Warn] = (c, m) => SDL.LogWarn((int)c, m),
                [LogPriority.Error] = (c, m) => SDL.LogError((int)c, m),
                [LogPriority.Critical] = (c, m) => SDL.LogCritical((int)c, m),
            };




        private static readonly LogFormatter Formatter = new LogFormatter(GetPriorityPrefix);
        private static readonly SinkPipeline Pipeline = new SinkPipeline();
        private static readonly LogRouter Router = new LogRouter(Pipeline);
        private static readonly CustomConsoleSink ConsoleSink = new CustomConsoleSink(Formatter);
        private static readonly SDL_LogOutputFunctionNative NativeLogOutputDelegate = RoutedOutputNative;

        static Log() {
            SDL.SetLogOutputFunction(NativeLogOutputDelegate, IntPtr.Zero);
        }

        /// <summary>
        /// Default category for convenience methods
        /// </summary>
        public static LogCategory DefaultCategory { get; set; } = LogCategory.Application;

        /// <summary>
        /// Gets the console colors used by the custom log output for each log priority.
        /// </summary>
        /// <remarks>
        /// These colors are only used when <see cref="UseCustomOutput"/> is active.
        /// </remarks>
        public static IDictionary<LogPriority, ConsoleColor> CustomLogColors => ConsoleSink.CustomLogColors;

        /// <summary>
        /// Gets a value indicating whether file logging is currently enabled.
        /// </summary>
        public static bool FileLoggingEnabled => Pipeline.Contains<FileLogSink>();

        /// <summary>
        /// Enables file logging that writes to the specified file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is null, empty, or whitespace.
        /// </exception>
        /// <remarks>
        /// If file logging is already enabled, the existing is replaced with a new one
        /// using the specified path.
        /// </remarks>
        public static void EnableFileLogging(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException("Log file path cannot be empty.", nameof(path));
            }

            Pipeline.RemoveSink<FileLogSink>();
            Pipeline.AddSink(new FileLogSink(path, Formatter));
        }

        /// <summary>
        /// Disables file logging.
        /// </summary>
        public static void DisableFileLogging() {
            Pipeline.RemoveSink<FileLogSink>();
        }

        /// <summary>
        /// Switches logging to SDL's default output behavior.
        /// </summary>
        public static void UseDefaultOutput() {
            Router.Mode = LogOutputMode.Default;
            Pipeline.RemoveSink<CustomConsoleSink>();
        }

        /// <summary>
        /// Enables the custom CSDL log output.
        /// </summary>
        public static void UseCustomOutput() {
            Router.Mode = LogOutputMode.Custom;

            if (!Pipeline.Contains<CustomConsoleSink>()) {
                Pipeline.AddSink(ConsoleSink);
            }
        }

        private static void RoutedOutputNative(nint userdata, int category, LogPriority priority, nint message) {
            Router.Handle((LogCategory)category, priority, (NativePtr<byte>)message);
        }

        /// <summary>
        /// Registers a custom sink that receives every dispatched log entry.
        /// </summary>
        /// <param name="sink">The sink to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink"/> is null.</exception>
        public static void AddSink(ILogSink sink) {
            Pipeline.AddSink(sink);
        }

        /// <summary>
        /// Removes a previously registered sink.
        /// </summary>
        /// <param name="sink">The sink to remove.</param>
        /// <returns><see langword="true"/> if the sink was found and removed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink"/> is null.</exception>
        public static bool RemoveSink(ILogSink sink) {
            return Pipeline.RemoveSink(sink);
        }

        private static string GetPriorityPrefix(LogPriority priority) {
            if (Prefixes.TryGet(priority, out PriorityPrefixItem? props) && props.Prefix != null) {
                return props.Prefix;
            }

            return priority.ToString().ToUpperInvariant();
        }

        private static void LogException(LogPriority p, Exception e, object? o = null) {
            LogException(DefaultCategory, p, e, o);
        }

        private static void LogException(LogCategory c, LogPriority p, Exception e, object? o = null) {
            if (e == null) {
                return;
            }
            LogInternal(c, p, FormatException(e), o);
        }

        private static void LogInternal(LogCategory category, LogPriority priority, string message, object? context) {
            if (priority < CategoryPriority[category]) {
                return;
            }

            if (context != null) {
                message += $" [Context: {JsonSerializer.Serialize(context)}]";
            }

            string format = EscapeFormatString(message);
            if (!SdlDispatch.TryGetValue(priority, out Action<LogCategory, string>? fn)) {
                SDL.LogMessage((int)category, priority, format);
            } else {
                fn(category, format);
            }
        }

        private static string EscapeFormatString(string message) {
            return message.Replace("%", "%%");
        }

        private static string FormatException(Exception? ex) {
            StringBuilder sb = new StringBuilder();
            int level = 0;

            while (ex != null) {
                string indent = new string(' ', level * 2);
                sb.AppendLine($"{indent}Exception: {ex.GetType().Name}");
                sb.AppendLine($"{indent}Message  : {ex.Message}");

                if (ex.StackTrace != null) {
                    sb.AppendLine($"{indent}Stack    :");
                    sb.AppendLine($"{indent}{ex.StackTrace.Replace("\n", "\n" + indent)}");
                }

                ex = ex.InnerException;
                if (ex != null) {
                    sb.AppendLine($"{indent}Inner Exception:");
                }

                level++;
            }

            return sb.ToString();
        }

        #region Message Logging

        /// <summary>
        /// Writes a trace message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Trace(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Trace, message, context);
        }

        /// <summary>
        /// Writes a trace message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Trace(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Trace, message, context);
        }

        /// <summary>
        /// Writes a verbose message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Verbose(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Verbose, message, context);
        }

        /// <summary>
        /// Writes a verbose message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Verbose(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Verbose, message, context);
        }

        /// <summary>
        /// Writes a debug message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Debug(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Debug, message, context);
        }

        /// <summary>
        /// Writes a debug message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Debug(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Debug, message, context);
        }

        /// <summary>
        /// Writes an informational message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Info(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Info, message, context);
        }

        /// <summary>
        /// Writes an informational message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Info(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Info, message, context);
        }

        /// <summary>
        /// Writes a warning message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Warn(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Warn, message, context);
        }

        /// <summary>
        /// Writes a warning message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Warn(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Warn, message, context);
        }

        /// <summary>
        /// Writes an error message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Error(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Error, message, context);
        }

        /// <summary>
        /// Writes an error message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Error(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Error, message, context);
        }

        /// <summary>
        /// Writes a critical message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Critical(string message, object? context = null) {
            LogInternal(DefaultCategory, LogPriority.Critical, message, context);
        }

        /// <summary>
        /// Writes a critical message to the specified SDL category.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="context">
        /// Optional contextual data that will be serialized and appended to the message.
        /// </param>
        public static void Critical(LogCategory category, string message, object? context = null) {
            LogInternal(category, LogPriority.Critical, message, context);
        }

        #endregion

        #region Raw Logging

        /// <summary>
        /// Logs a message using a native UTF-8 buffer pointer.
        /// </summary>
        /// <param name="category">The SDL log category.</param>
        /// <param name="priority">The log priority level.</param>
        /// <param name="buffer">Pointer to a null-terminated UTF-8 string.</param>
        /// <remarks>The buffer must be null-terminated and contain valid UTF-8 text.</remarks>
        public static void Message(LogCategory category, LogPriority priority, NativePtr<byte> buffer) {
            if (priority < CategoryPriority[category]) {
                return;
            }
            SDL.LogMessage((int)category, priority, EscapeFormatString(buffer.ToUtf8String() ?? string.Empty));
        }

        /// <summary>
        /// Logs a message using a native UTF-8 buffer pointer to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="priority">The log priority level.</param>
        /// <param name="buffer">Pointer to a null-terminated UTF-8 string.</param>
        /// <remarks>The buffer must be null-terminated and contain valid UTF-8 text.</remarks>
        public static void Message(LogPriority priority, NativePtr<byte> buffer) {
            Message(DefaultCategory, priority, buffer);
        }

        #endregion

        #region Exception Logging

        /// <summary>
        /// Writes an exception as a trace message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Trace(Exception ex, object? o = null) {
            LogException(LogPriority.Trace, ex, o);
        }

        /// <summary>
        /// Writes an exception as a trace message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Trace(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Trace, ex, o);
        }

        /// <summary>
        /// Writes an exception as a verbose message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Verbose(Exception ex, object? o = null) {
            LogException(LogPriority.Verbose, ex, o);
        }

        /// <summary>
        /// Writes an exception as a verbose message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Verbose(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Verbose, ex, o);
        }

        /// <summary>
        /// Writes an exception as a debug message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Debug(Exception ex, object? o = null) {
            LogException(LogPriority.Debug, ex, o);
        }

        /// <summary>
        /// Writes an exception as a debug message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Debug(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Debug, ex, o);
        }

        /// <summary>
        /// Writes an exception as an informational message <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Info(Exception ex, object? o = null) {
            LogException(LogPriority.Info, ex, o);
        }

        /// <summary>
        /// Writes an exception as an informational message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Info(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Info, ex, o);
        }

        /// <summary>
        /// Writes an exception as a warning message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Warn(Exception ex, object? o = null) {
            LogException(LogPriority.Warn, ex, o);
        }

        /// <summary>
        /// Writes an exception as a warning message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Warn(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Warn, ex, o);
        }

        /// <summary>
        /// Writes an exception as an error message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Error(Exception ex, object? o = null) {
            LogException(LogPriority.Error, ex, o);
        }

        /// <summary>
        /// Writes an exception as an error message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Error(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Error, ex, o);
        }

        /// <summary>
        /// Writes an exception as a critical message to the <see cref="DefaultCategory"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Critical(Exception ex, object? o = null) {
            LogException(LogPriority.Critical, ex, o);
        }

        /// <summary>
        /// Writes an exception as a critical message to the specified SDL category.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="c">The SDL log category.</param>
        /// <param name="o">Optional contextual data to append to the log entry.</param>
        public static void Critical(Exception ex, LogCategory c, object? o = null) {
            LogException(c, LogPriority.Critical, ex, o);
        }

        #endregion
    }
}
