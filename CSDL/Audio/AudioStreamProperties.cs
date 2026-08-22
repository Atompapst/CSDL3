// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Audio {
    public class AudioStreamProperties : PropertyGroup {

        internal AudioStreamProperties(uint handle) : base(handle) { }

        /// <inheritdoc cref="CSDL.Props.AudiostreamAutoCleanupBoolean"/>
        public BooleanProperty AutoCleanup => PropBool(Props.AudiostreamAutoCleanupBoolean);

    }
}
