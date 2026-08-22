// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.Extensions;

namespace CSDL.File {
    public partial class IOStream : NativeHandle<Opaque.SdlIOStream> {
        private int _trackLeaseCount;
        private NativePtr<Opaque.SdlIOStream> _deferredHandle;

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOFromFile"/>
        public IOStream(string file, string mode) {
            Handle = SDL.IOFromFile(file, mode).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOFromFile"/>
        public IOStream(string file, IOStreamMode mode) {
            Handle = SDL.IOFromFile(file, ModeToString(mode)).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOFromMem"/>
        public IOStream(IntPtr mem, UIntPtr size) {
            Handle = SDL.IOFromMem(mem, size).ThrowIfInvalid();
        }

        public static IOStream OpenRead(string file) {
            return new IOStream(file, IOStreamMode.Read);
        }

        public static IOStream OpenReadWrite(string file) {
            return new IOStream(file, IOStreamMode.Read | IOStreamMode.Plus);
        }

        public static IOStream OpenWrite(string file) {
            return new IOStream(file, IOStreamMode.Write);
        }

        public static IOStream OpenWriteExclusive(string file) {
            return new IOStream(file, IOStreamMode.Write | IOStreamMode.Exclusive);
        }

        public static IOStream OpenAppend(string file) {
            return new IOStream(file, IOStreamMode.Append);
        }

        internal IOStream(NativePtr<Opaque.SdlIOStream> handle) {
            Handle = handle;
        }

        /// <summary>
        ///     Wraps a stream that something else owns (e.g. a <see cref="CSDL.Process"/>'s stdio), so
        ///     disposing this view does not close the underlying stream.
        /// </summary>
        internal IOStream(NativePtr<Opaque.SdlIOStream> handle, bool ownsHandle) : base(handle, ownsHandle) {
            Handle = handle;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOFromConstMem"/>
        public static IOStream FromConstMem(IntPtr mem, UIntPtr size) {
            return new IOStream(SDL.IOFromConstMem(mem, size));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOFromDynamicMem"/>
        public static IOStream FromDynamicMem() {
            return new IOStream(SDL.IOFromDynamicMem());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.GetIOSize"/>
        public long Size => SDL.GetIOSize(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.GetIOStatus"/>
        public IOStatus Status => SDL.GetIOStatus(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.GetIOProperties"/>
        public uint Properties => SDL.GetIOProperties(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.SeekIO"/>
        public long Seek(long offset, IOWhence whence) {
            return SDL.SeekIO(Handle, offset, whence).LogIfInvalid(-1);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.TellIO"/>
        public long Tell() {
            return SDL.TellIO(Handle).LogIfInvalid(-1);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadIO"/>
        public ulong Read(IntPtr ptr, ulong size) {
            return SDL.ReadIO(Handle, ptr, (UIntPtr)size);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteIO"/>
        public ulong Write(IntPtr ptr, ulong size) {
            return SDL.WriteIO(Handle, ptr, (UIntPtr)size);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.FlushIO"/>
        public bool Flush() {
            return SDL.FlushIO(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOprintf"/>
        /// <remarks>
        /// SDL treats <paramref name="text"/> as a printf format string, so any <c>%</c> it contains
        /// is escaped to <c>%%</c> before the call - there is no managed varargs list to back it up.
        /// </remarks>
        public ulong Printf(string text) {
            return SDL.IOprintf(Handle, text.Replace("%", "%%")).LogIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.IOvprintf"/>
        /// <remarks>
        /// A C <c>va_list</c> cannot be constructed from managed code, so this writes
        /// <paramref name="text"/> the same way <see cref="Printf"/> does.
        /// </remarks>
        public ulong VPrintf(string text) {
            return SDL.IOprintf(Handle, text.Replace("%", "%%")).LogIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU8"/>
        public byte ReadU8() {
            SDL.ReadU8(Handle, out byte value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU8"/>
        public bool WriteU8(byte value) {
            return SDL.WriteU8(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS8"/>
        public sbyte ReadS8() {
            SDL.ReadS8(Handle, out sbyte value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS8"/>
        public bool WriteS8(sbyte value) {
            return SDL.WriteS8(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU16LE"/>
        public ushort ReadU16LE() {
            SDL.ReadU16LE(Handle, out ushort value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU16LE"/>
        public bool WriteU16LE(ushort value) {
            return SDL.WriteU16LE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU16BE"/>
        public ushort ReadU16BE() {
            SDL.ReadU16BE(Handle, out ushort value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU16BE"/>
        public bool WriteU16BE(ushort value) {
            return SDL.WriteU16BE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS16LE"/>
        public short ReadS16LE() {
            SDL.ReadS16LE(Handle, out short value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS16LE"/>
        public bool WriteS16LE(short value) {
            return SDL.WriteS16LE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS16BE"/>
        public short ReadS16BE() {
            SDL.ReadS16BE(Handle, out short value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS16BE"/>
        public bool WriteS16BE(short value) {
            return SDL.WriteS16BE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU32LE"/>
        public uint ReadU32LE() {
            SDL.ReadU32LE(Handle, out uint value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU32LE"/>
        public bool WriteU32LE(uint value) {
            return SDL.WriteU32LE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU32BE"/>
        public uint ReadU32BE() {
            SDL.ReadU32BE(Handle, out uint value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU32BE"/>
        public bool WriteU32BE(uint value) {
            return SDL.WriteU32BE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS32LE"/>
        public int ReadS32LE() {
            SDL.ReadS32LE(Handle, out int value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS32LE"/>
        public bool WriteS32LE(int value) {
            return SDL.WriteS32LE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS32BE"/>
        public int ReadS32BE() {
            SDL.ReadS32BE(Handle, out int value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS32BE"/>
        public bool WriteS32BE(int value) {
            return SDL.WriteS32BE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU64LE"/>
        public ulong ReadU64LE() {
            SDL.ReadU64LE(Handle, out ulong value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU64LE"/>
        public bool WriteU64LE(ulong value) {
            return SDL.WriteU64LE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadU64BE"/>
        public ulong ReadU64BE() {
            SDL.ReadU64BE(Handle, out ulong value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteU64BE"/>
        public bool WriteU64BE(ulong value) {
            return SDL.WriteU64BE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS64LE"/>
        public long ReadS64LE() {
            SDL.ReadS64LE(Handle, out long value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS64LE"/>
        public bool WriteS64LE(long value) {
            return SDL.WriteS64LE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.ReadS64BE"/>
        public long ReadS64BE() {
            SDL.ReadS64BE(Handle, out long value).LogIfFalse();
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.WriteS64BE"/>
        public bool WriteS64BE(long value) {
            return SDL.WriteS64BE(Handle, value).LogIfFalse();
        }

        /// <param name="file">The path of the file to load.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.LoadFile"/>
        /// <returns>The file contents, or <c>null</c> if the file could not be loaded.</returns>
        public static byte[]? LoadFile(string file) {
            IntPtr ptr = SDL.LoadFile(file, out nuint size);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(LoadFile));
                return null;
            }

            byte[] data = new byte[size];
            Marshal.Copy(ptr, data, 0, (int)size);
            CSDL.SDL.free(ptr);
            return data;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.SaveFile"/>
        public static bool SaveFile(string file, byte[] data) {
            unsafe {
                fixed (byte* pData = data) {
                    return SDL.SaveFile(file, (IntPtr)pData, (UIntPtr)data.Length).LogIfFalse();
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.LoadFile_IO"/>
        public byte[]? Load(bool closeIO = false) {
            IntPtr ptr = SDL.LoadFile_IO(Handle, out nuint size, closeIO);
            if (closeIO) {
                Invalidate();
            }

            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(Load));
                return null;
            }

            byte[] data = new byte[size];
            Marshal.Copy(ptr, data, 0, (int)size);
            CSDL.SDL.free(ptr);
            return data;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.SaveFile_IO"/>
        /// <param name="data">The data to write.</param>
        /// <param name="closeIO">If true, this stream is closed by SDL before the call returns, even on failure.</param>
        public bool Save(byte[] data, bool closeIO = false) {
            bool result;
            unsafe {
                fixed (byte* pData = data) {
                    result = SDL.SaveFile_IO(Handle, (IntPtr)pData, (UIntPtr)data.Length, closeIO).LogIfFalse();
                }
            }

            if (closeIO) {
                Invalidate();
            }

            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Iostream.CloseIO"/>
        protected override void DisposeResource() {
            if (System.Threading.Volatile.Read(ref _trackLeaseCount) != 0) {
                _deferredHandle = Handle;
                return;
            }
            SDL.CloseIO(Handle).LogIfFalse();
            OnCustomIOStreamClosed();
        }

        protected override void InvalidateResource() {
            OnCustomIOStreamClosed();
        }

        internal void AcquireTrackLease() {
            System.Threading.Interlocked.Increment(ref _trackLeaseCount);
        }

        internal void ReleaseTrackLease() {
            if (System.Threading.Interlocked.Decrement(ref _trackLeaseCount) != 0 || _deferredHandle.IsNull) return;
            SDL.CloseIO(_deferredHandle).LogIfFalse();
            _deferredHandle = NativePtr<Opaque.SdlIOStream>.Zero;
            OnCustomIOStreamClosed();
        }

        partial void OnCustomIOStreamClosed();
    }
}
