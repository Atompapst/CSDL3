// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL {
    /// <seealso cref="CSDL.File.IOStream.Properties"/>
    public sealed class IOStreamProperties : PropertyGroup {
        internal IOStreamProperties(uint handle) : base(handle) { }

        // Set by IOFromFile
        /// <inheritdoc cref="CSDL.Props.IostreamFileDescriptorNumber"/>
        public NumberProperty FileDescriptor => PropNumber(Props.IostreamFileDescriptorNumber);

        /// <inheritdoc cref="CSDL.Props.IostreamStdioFilePointer"/>
        public PointerProperty StdioFile => PropPointer(Props.IostreamStdioFilePointer);

        /// <inheritdoc cref="CSDL.Props.IostreamWindowsHandlePointer"/>
        public PointerProperty WindowsHandle => PropPointer(Props.IostreamWindowsHandlePointer);

        /// <inheritdoc cref="CSDL.Props.IostreamAndroidAassetPointer"/>
        public PointerProperty AndroidAsset => PropPointer(Props.IostreamAndroidAassetPointer);

        /// <inheritdoc cref="CSDL.Props.IostreamOpenharmonyRAWFILE64Pointer"/>
        public PointerProperty OpenHarmonyRawFile64 => PropPointer(Props.IostreamOpenharmonyRAWFILE64Pointer);

        // Set by IOFromMem / IOFromConstMem
        /// <inheritdoc cref="CSDL.Props.IostreamMemoryPointer"/>
        public PointerProperty Memory => PropPointer(Props.IostreamMemoryPointer);

        /// <inheritdoc cref="CSDL.Props.IostreamMemorySizeNumber"/>
        public NumberProperty MemorySize => PropNumber(Props.IostreamMemorySizeNumber);

        /// <inheritdoc cref="CSDL.Props.IostreamMemoryFreeFuncPointer"/>
        public PointerProperty MemoryFreeFunc => PropPointer(Props.IostreamMemoryFreeFuncPointer);

        // Set by IOFromDynamicMem
        /// <inheritdoc cref="CSDL.Props.IostreamDynamicMemoryPointer"/>
        /// <remarks>Setting it to null hands the buffer over to the app (free it with <seealso cref="CSDL.Memory.Free"/>); the next call must be close.</remarks>
        public PointerProperty DynamicMemory => PropPointer(Props.IostreamDynamicMemoryPointer);

        /// <inheritdoc cref="CSDL.Props.IostreamDynamicChunksizeNumber"/>
        public NumberProperty DynamicChunkSize => PropNumber(Props.IostreamDynamicChunksizeNumber);
    }
}
