// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Audio {
    public class AudioDeviceProperties : PropertyGroup {

        internal AudioDeviceProperties(uint handle) : base(handle) { }

        /// <inheritdoc cref="CSDL.Props.AudioDeviceUniqueIDString"/>
        public StringProperty UniqueId => PropString(Props.AudioDeviceUniqueIDString);

    }
}
