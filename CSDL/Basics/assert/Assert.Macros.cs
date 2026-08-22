// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace CSDL {
    public static partial class Assert {
        /// <inheritdoc cref="CSDL.Macros.AssertLevel"/>
#if ASSERT_LEVEL_3_OR_GREATER
            public const int AssertLevel = 3;
#elif ASSERT_LEVEL_2_OR_GREATER || DEBUG
        public const int AssertLevel = 2;
#elif ASSERT_LEVEL_1_OR_GREATER
            public const int AssertLevel = 1;
#else
            public const int AssertLevel = 0;
#endif

        /// <inheritdoc cref="CSDL.Macros.NullWhileLoopCondition"/>
        /// <remarks>
        /// Kept only for name parity with <c>SDL_NULL_WHILE_LOOP_CONDITION</c>. It has no effect in C#:
        /// the do/while(0) trick it supports exists to avoid dangling-else problems in C macros, which
        /// don't apply to real C# methods.
        /// </remarks>
        public const bool NullWhileLoopCondition = false;

        /// <inheritdoc cref="CSDL.Macros.AssertFile"/>
        public static string AssertFile([CallerFilePath] string? file = null) {
            return File(file);
        }

        /// <inheritdoc cref="CSDL.Macros.File"/>
        public static string File([CallerFilePath] string? file = null) {
            return System.IO.Path.GetFileName(file) ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Macros.Line"/>
        public static int Line([CallerLineNumber] int line = 0) {
            return line;
        }

        /// <inheritdoc cref="CSDL.Macros.Function"/>
        public static string Function([CallerMemberName] string function = "") {
            return string.IsNullOrEmpty(function) ? "???" : function;
        }

        /// <inheritdoc cref="CSDL.Macros.TriggerBreakpoint"/>
        public static void TriggerBreakpoint() {
            Debugger.Break();
        }

        /// <inheritdoc cref="CSDL.Macros.AssertBreakpoint"/>
        public static void AssertBreakpoint() {
            TriggerBreakpoint();
        }

        /// <inheritdoc cref="CSDL.Macros.DisabledAssert"/>
        public static void DisabledAssert(Func<bool> condition) {
            // Absichtlich nicht ausführen.
            // Die Lambda wird kompiliert, aber nicht aufgerufen.
            _ = condition;
        }

        /// <inheritdoc cref="CSDL.Macros.AssertRelease"/>
        [Conditional("ASSERT_LEVEL_1_OR_GREATER")]
        public static void Release(
            bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null,
            [CallerMemberName] string? function = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0
        ) {
            AssertImpl(condition, expression, function, file, line);
        }

        /// CSDL-Note: Renamed to Check() for avoiding conflict with class name.
        /// <inheritdoc cref="CSDL.Macros.Assert"/>
        [Conditional("ASSERT_LEVEL_2_OR_GREATER"), Conditional("DEBUG")]
        public static void Check(
            bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null,
            [CallerMemberName] string? function = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0
        ) {
            AssertImpl(condition, expression, function, file, line);
        }

        /// <inheritdoc cref="CSDL.Macros.AssertParanoid"/>
        [Conditional("ASSERT_LEVEL_3_OR_GREATER")]
        public static void Paranoid(
            bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null,
            [CallerMemberName] string? function = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0
        ) {
            AssertImpl(condition, expression, function, file, line);
        }

        /// <inheritdoc cref="CSDL.Macros.AssertAlways"/>
        public static void Always(
            bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null,
            [CallerMemberName] string? function = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0
        ) {
            AssertImpl(condition, expression, function, file, line);
        }

        // Shared body for Release/Check/Paranoid/Always
        private static void AssertImpl(bool condition, string? expression, string? function, string? file, int line) {
            if (!condition) {
                HandleState(Report(expression, function, file, line));
            }
        }
    }
}
