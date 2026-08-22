// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Threads {
    public class ThreadProperties : PropertyGroup {
        public ThreadProperties(uint handle) : base(handle) { }
        public ThreadProperties() : base() { }
        /// <inheritdoc cref="CSDL.Props.ThreadCreateEntryFunctionPointer"/>
        public PointerProperty EntryFunction => PropPointer(Props.ThreadCreateEntryFunctionPointer);
        /// <inheritdoc cref="CSDL.Props.ThreadCreateNameString"/>
        public StringProperty Name => PropString(Props.ThreadCreateNameString);
        /// <inheritdoc cref="CSDL.Props.ThreadCreateUserdataPointer"/>
        public PointerProperty Userdata => PropPointer(Props.ThreadCreateUserdataPointer);
        /// <inheritdoc cref="CSDL.Props.ThreadCreateStacksizeNumber"/>
        public NumberProperty StackSize => PropNumber(Props.ThreadCreateStacksizeNumber);

    }
}
