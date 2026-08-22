// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.File;

namespace CSDL {
    /// <summary>
    ///     A child process spawned through SDL.
    /// </summary>
    /// <remarks>
    ///     Disposing this object only releases SDL's bookkeeping - it does not stop the process. Use
    ///     <see cref="Kill"/> for that, and <see cref="Wait"/> to collect the exit code.
    /// </remarks>
    public sealed class Process : NativeHandle<Opaque.SdlProcess> {

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.CreateProcess"/>
        /// <param name="args">the executable in <c>args[0]</c>, followed by its arguments. The trailing NULL C wants is added here.</param>
        /// <param name="pipeStdio"><see langword="true"/> to pipe the process's stdin/stdout so <see cref="Input"/>, <see cref="Output"/> and <see cref="ReadAll"/> can be used.</param>
        public Process(string[] args, bool pipeStdio = false) {
            ArgumentNullException.ThrowIfNull(args);
            if (args.Length == 0) {
                throw new ArgumentException("args must at least contain the path to the executable.", nameof(args));
            }

            using NativeStringArray.Native argv = NativeStringArray.AllocateNullTerminated(args);
            Handle = SDL.CreateProcess(argv.Ptr, pipeStdio).ThrowIfInvalid(nameof(SDL.CreateProcess));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.CreateProcessWithProperties"/>
        public Process(ProcessCreateProperties properties) {
            ArgumentNullException.ThrowIfNull(properties);
            Handle = SDL.CreateProcessWithProperties(properties.Handle).ThrowIfInvalid(nameof(SDL.CreateProcessWithProperties));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.GetProcessProperties"/>
        public ProcessProperties Properties => new ProcessProperties(SDL.GetProcessProperties(Handle));

        /// <inheritdoc cref="CSDL.Props.ProcessPidNumber"/>
        public long Id => Properties.Pid.Get();

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.GetProcessInput"/>
        /// <remarks>
        ///     The stream belongs to the process and is closed when this object is disposed, so the
        ///     returned view must not be closed by the caller.
        /// </remarks>
        public IOStream? Input {
            get {
                NativePtr<Opaque.SdlIOStream> stream = SDL.GetProcessInput(Handle);
                return stream.IsNull ? null : new IOStream(stream, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.GetProcessOutput"/>
        /// <inheritdoc cref="Input" path="/remarks"/>
        public IOStream? Output {
            get {
                NativePtr<Opaque.SdlIOStream> stream = SDL.GetProcessOutput(Handle);
                return stream.IsNull ? null : new IOStream(stream, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.KillProcess"/>
        public bool Kill(bool force = false) {
            return SDL.KillProcess(Handle, force).LogIfFalse(nameof(SDL.KillProcess));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.WaitProcess"/>
        /// <returns><see langword="true"/> if the process has exited, in which case <paramref name="exitCode"/> holds its exit code.</returns>
        public bool TryWait(out int exitCode) {
            return SDL.WaitProcess(Handle, false, out exitCode);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.WaitProcess"/>
        /// <returns>The process's exit code, once it has exited.</returns>
        /// <exception cref="SDLException">The process could not be waited on.</exception>
        public int Wait() {
            SDL.WaitProcess(Handle, true, out int exitCode).ThrowIfFalse(nameof(SDL.WaitProcess));
            return exitCode;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.ReadProcess"/>
        /// <returns>Everything the process wrote to stdout, or an empty array if it wrote nothing.</returns>
        /// <exception cref="SDLException">Reading failed - the process was not created with piped stdout, or it is still running.</exception>
        public byte[] ReadAll(out int exitCode) {
            nint data = SDL.ReadProcess(Handle, out nuint size, out exitCode);
            if (data == nint.Zero) {
                Error.Throw(nameof(SDL.ReadProcess));
            }

            try {
                if (size == 0) {
                    return Array.Empty<byte>();
                }

                byte[] result = new byte[(int)size];
                System.Runtime.InteropServices.Marshal.Copy(data, result, 0, result.Length);
                return result;
            }
            finally {
                Memory.Free(data);
            }
        }

        /// <summary>
        ///     Reads everything the process wrote to stdout and decodes it as UTF-8.
        /// </summary>
        /// <seealso cref="ReadAll"/>
        public string ReadAllText(out int exitCode) {
            byte[] data = ReadAll(out exitCode);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Process.DestroyProcess"/>
        protected override void DisposeResource() {
            SDL.DestroyProcess(Handle);
        }
    }
}
