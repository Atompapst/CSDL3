// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.File;
using CSDL.Properties;

namespace CSDL {
    /// <summary>
    ///     The property set understood by <see cref="Process(ProcessCreateProperties)"/>.
    /// </summary>
    /// <remarks>
    ///     <see cref="Args"/> is the only required property; set it through <see cref="SetArgs"/> so the
    ///     native argv stays alive for as long as this group does.
    /// </remarks>
    /// <seealso cref="CSDL.Internal.Docs.Process.CreateProcessWithProperties">SDL_CreateProcessWithProperties</seealso>
    public sealed class ProcessCreateProperties : PropertyGroup {
        private NativeStringArray.Native? _args;

        /// <inheritdoc cref="CSDL.Props.ProcessCreateArgsPointer"/>
        public PointerProperty Args => PropPointer(Props.ProcessCreateArgsPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateEnvironmentPointer"/>
        public PointerProperty Environment => PropPointer(Props.ProcessCreateEnvironmentPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateWorkingDirectoryString"/>
        public StringProperty WorkingDirectory => PropString(Props.ProcessCreateWorkingDirectoryString);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStdinNumber"/>
        public NumberProperty StdinOption => PropNumber(Props.ProcessCreateStdinNumber);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStdinPointer"/>
        public PointerProperty StdinSource => PropPointer(Props.ProcessCreateStdinPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStdoutNumber"/>
        public NumberProperty StdoutOption => PropNumber(Props.ProcessCreateStdoutNumber);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStdoutPointer"/>
        public PointerProperty StdoutSource => PropPointer(Props.ProcessCreateStdoutPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStderrNumber"/>
        public NumberProperty StderrOption => PropNumber(Props.ProcessCreateStderrNumber);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStderrPointer"/>
        public PointerProperty StderrSource => PropPointer(Props.ProcessCreateStderrPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateStderrToStdoutBoolean"/>
        public BooleanProperty StderrToStdout => PropBool(Props.ProcessCreateStderrToStdoutBoolean);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateBackgroundBoolean"/>
        public BooleanProperty Background => PropBool(Props.ProcessCreateBackgroundBoolean);

        /// <inheritdoc cref="CSDL.Props.ProcessCreateCmdlineString"/>
        public StringProperty CommandLine => PropString(Props.ProcessCreateCmdlineString);

        /// <summary>
        ///     Sets the program and its arguments, allocating the NULL-terminated native argv SDL wants.
        /// </summary>
        /// <param name="args">the executable in <c>args[0]</c>, followed by its arguments.</param>
        /// <remarks>The allocation is owned by this group and released on <see cref="Dispose"/>.</remarks>
        public void SetArgs(params string[] args) {
            ArgumentNullException.ThrowIfNull(args);
            if (args.Length == 0) {
                throw new ArgumentException("args must at least contain the path to the executable.", nameof(args));
            }

            _args?.Dispose();
            NativeStringArray.Native argv = NativeStringArray.AllocateNullTerminated(args);
            _args = argv;
            Args.Set(argv.Ptr);
        }

        /// <summary>Routes standard input for the process.</summary>
        /// <param name="io">where input comes from.</param>
        /// <param name="redirect">the stream to read from, required when <paramref name="io"/> is <see cref="ProcessIO.Redirect"/>.</param>
        public void SetStdin(ProcessIO io, IOStream? redirect = null) {
            StdinOption.Set((long)io);
            if (redirect is not null) {
                StdinSource.Set(redirect.NativePointer);
            }
        }

        /// <summary>Routes standard output for the process.</summary>
        /// <inheritdoc cref="SetStdin" path="/param"/>
        public void SetStdout(ProcessIO io, IOStream? redirect = null) {
            StdoutOption.Set((long)io);
            if (redirect is not null) {
                StdoutSource.Set(redirect.NativePointer);
            }
        }

        /// <summary>Routes standard error for the process.</summary>
        /// <inheritdoc cref="SetStdin" path="/param"/>
        public void SetStderr(ProcessIO io, IOStream? redirect = null) {
            StderrOption.Set((long)io);
            if (redirect is not null) {
                StderrSource.Set(redirect.NativePointer);
            }
        }

        /// <summary>
        ///     Destroys the property group and frees the argv allocated by <see cref="SetArgs"/>.
        /// </summary>
        public override void Dispose() {
            _args?.Dispose();
            _args = null;
            base.Dispose();
        }
    }
}
