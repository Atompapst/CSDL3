// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSDL {
    /// <summary>
    /// Provides SDL-style assertion helpers for C#.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It allows you to write assertions that can be enabled or disabled at compile time.
    /// </para>
    ///
    /// <para>
    /// Assertion levels are controlled by compile-time symbols:
    /// </para>
    ///
    /// <list type="table">
    ///   <listheader>
    ///     <term>Symbol</term>
    ///     <description>Effect</description>
    ///   </listheader>
    ///   <item>
    ///     <term>ASSERT_LEVEL_1_OR_GREATER</term>
    ///     <description>Enables level 1 assertions, usually release-safe checks.</description>
    ///   </item>
    ///   <item>
    ///     <term>ASSERT_LEVEL_2_OR_GREATER</term>
    ///     <description>Enables level 2 assertions, usually normal debug checks.</description>
    ///   </item>
    ///   <item>
    ///     <term>ASSERT_LEVEL_3_OR_GREATER</term>
    ///     <description>Enables level 3 assertions, usually very expensive/paranoid checks.</description>
    ///   </item>
    ///   <item>
    ///     <term>DEBUG</term>
    ///     <description>Also enables level 2 assertions by default.</description>
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// Important: These symbols are checked at compile time, not at runtime.
    /// This means assertion calls may be completely removed by the C# compiler
    /// if the required symbol is not defined.
    /// </para>
    ///
    /// <para>
    /// If you enable a higher level, you should also define all lower levels.
    /// For example, for level 3 define:
    /// <c>ASSERT_LEVEL_1_OR_GREATER;ASSERT_LEVEL_2_OR_GREATER;ASSERT_LEVEL_3_OR_GREATER</c>.
    /// </para>
    ///
    /// <para>
    /// Example usage:
    /// </para>
    ///
    /// <code>
    /// Assert.Release(handle != IntPtr.Zero);
    /// Assert.Check(index >= 0);
    /// Assert.Paranoid(value.IsValid());
    /// </code>
    /// </remarks>
    public static partial class Assert {
        private static readonly Lock Sync = new Lock();

        private static SDL_AssertionHandlerNative? _currentNativeAssertionHandler;

        /*
         * SDL's C assertion macros use a static SDL_AssertData per assertion site.
         * We must do something similar in C#, because SDL keeps pointers to these
         * records in the assertion report.
         */
        private static readonly Dictionary<(string File, int Line, string Function, string Condition), IntPtr> AssertionSites = new Dictionary<(string File, int Line, string Function, string Condition), IntPtr>();

        /// <inheritdoc cref="CSDL.Internal.Docs.Assert.ReportAssertion"/>
        /// <remarks>
        /// Each distinct (file, line, function, condition) combination allocates a small, permanently
        /// cached unmanaged record, mirroring SDL's static per-callsite <c>SDL_AssertData</c>. This is fine
        /// for the fixed call sites produced by <see cref="Release"/>, <see cref="Check"/>, <see cref="Paranoid"/>
        /// and <see cref="Always"/>, but callers invoking this method directly should avoid passing
        /// dynamically generated strings, since the cache is never freed for the lifetime of the process.
        /// </remarks>
        public static AssertState Report(
            string? condition,
            string? function,
            string? file,
            int line
        ) {
            condition ??= "unknown condition";
            function ??= "unknown function";
            file ??= "unknown file";

            IntPtr data = GetOrCreateAssertionSite(condition, function, file, line);

            return SDL.ReportAssertion(data, function, file, line);
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Assert.SetAssertionHandler"/>
        public static void SetHandler(AssertionHandler handler, object? userdata = null) {
            lock (Sync) {
                CallbackRegistry.UnregisterSingle<AssertionHandler, SDL_AssertionHandlerNative>();

                if (handler == null) {
                    SDL.SetAssertionHandler(null, IntPtr.Zero);
                    _currentNativeAssertionHandler = null;
                    return;
                }

                _currentNativeAssertionHandler = AssertionHandlerWrapper.Create(handler);

                (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.RegisterSingle(handler, _currentNativeAssertionHandler, userdata);
                SDL.SetAssertionHandler(_currentNativeAssertionHandler, reg.userdataPtr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Assert.GetAssertionHandler"/>
        public static AssertionHandler? GetHandler(out object? userdata) {
            lock (Sync) {
                SDL_AssertionHandlerNative nativeHandler = SDL.GetAssertionHandler(out IntPtr userdataPtr);

                // Check if we have a registered managed handler
                if (CallbackRegistry.TryGet<AssertionHandler, SDL_AssertionHandlerNative>(
                        out AssertionHandler? publicHandler,
                        out SDL_AssertionHandlerNative? registeredNative,
                        out object? registeredUserdata)) {

                    // Verify the native handler matches what SDL reports
                    if (nativeHandler != null) {
                        userdata = registeredUserdata;
                        return publicHandler;
                    }
                }

                // No managed handler registered
                userdata = userdataPtr != IntPtr.Zero ? CallbackRegistry.GetUserdata(userdataPtr) : null;
                return null;
            }
        }

        /// <summary>
        /// Restore SDL's default assertion handler.
        /// </summary>
        public static void UseDefaultHandler() {
            lock (Sync) {
                CallbackRegistry.UnregisterSingle<AssertionHandler, SDL_AssertionHandlerNative>();

                _currentNativeAssertionHandler = SDL.GetDefaultAssertionHandler();
                SDL.SetAssertionHandler(_currentNativeAssertionHandler, IntPtr.Zero);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Assert.GetAssertionReport"/>
        public static IReadOnlyList<AssertReportItem> GetReport() {
            lock (Sync) {
                List<AssertReportItem> result = new List<AssertReportItem>();

                NativePtr<AssertData> current = SDL.GetAssertionReport();

                while (!current.IsNull) {
                    AssertData data = Marshal.PtrToStructure<AssertData>(current.Ptr);

                    result.Add(new AssertReportItem(
                        data.AlwaysIgnore,
                        data.TriggerCount,
                        data.Condition,
                        data.Filename,
                        data.Linenum,
                        data.Function
                    ));

                    current = data.Next;
                }
                return result;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Assert.ResetAssertionReport"/>
        public static void ResetReport() {
            lock (Sync) {
                SDL.ResetAssertionReport();
            }
        }

        private static IntPtr GetOrCreateAssertionSite(
            string condition,
            string function,
            string file,
            int line
        ) {
            (string, int, string, string) key = (file, line, function, condition);

            lock (Sync) {
                if (AssertionSites.TryGetValue(key, out IntPtr existing)) {
                    return existing;
                }

                AssertData data = new AssertData(false, 0u, Marshal.StringToCoTaskMemUTF8(condition), Marshal.StringToCoTaskMemUTF8(file), line, Marshal.StringToCoTaskMemUTF8(function));

                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<AssertData>());
                Marshal.StructureToPtr(data, ptr, false);

                AssertionSites[key] = ptr;
                return ptr;
            }
        }

        /// <remarks>
        /// <see cref="AssertState.Retry"/> behaves like <see cref="AssertState.Ignore"/> here: SDL's C macro
        /// re-evaluates the original expression on retry, but <see cref="Release"/>/<see cref="Check"/>/
        /// <see cref="Paranoid"/>/<see cref="Always"/> only receive the already-evaluated <c>bool</c> result,
        /// so there is nothing left to re-run.
        /// </remarks>
        private static void HandleState(AssertState state) {
            switch (state) {
                case AssertState.Retry:
                    break;

                case AssertState.Break:
                    Debugger.Break();
                    break;

                case AssertState.Abort:
                    Environment.FailFast("SDL assertion requested abort.");
                    break;

                case AssertState.Ignore:
                case AssertState.AlwaysIgnore:
                default:
                    break;
            }
        }
    }
}
