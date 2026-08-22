// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;

namespace CSDL {
    /// <summary>
    ///     The read-only properties SDL keeps for a running <see cref="Process"/>.
    /// </summary>
    /// <remarks>
    ///     This group is created and owned by SDL and lives as long as the process object does, so it
    ///     must not be disposed - the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Process.Properties"/>
    public sealed class ProcessProperties : PropertyGroup {
        internal ProcessProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.Props.ProcessPidNumber"/>
        public NumberProperty Pid => PropNumber(Props.ProcessPidNumber);

        /// <inheritdoc cref="CSDL.Props.ProcessStdinPointer"/>
        public PointerProperty Stdin => PropPointer(Props.ProcessStdinPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessStdoutPointer"/>
        public PointerProperty Stdout => PropPointer(Props.ProcessStdoutPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessStderrPointer"/>
        public PointerProperty Stderr => PropPointer(Props.ProcessStderrPointer);

        /// <inheritdoc cref="CSDL.Props.ProcessBackgroundBoolean"/>
        public BooleanProperty Background => PropBool(Props.ProcessBackgroundBoolean);
    }
}
