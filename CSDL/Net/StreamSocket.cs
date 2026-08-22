// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Net {
    /// <summary>
    /// A reliable, ordered, bidirectional byte stream connected to a single remote peer -
    /// either an outgoing connection created with <see cref="StreamSocket(Address,ushort,PropertiesID)"/>,
    /// or an incoming one accepted via <see cref="Server.TryAcceptClient"/>.
    /// </summary>
    public sealed class StreamSocket : NativeHandle<Opaque.SdlStreamSocket> {
        static StreamSocket() {
            Net.EnsureInitialized();
        }

        /// <summary>
        /// Begins connecting to <paramref name="address"/> as a client. Connecting is
        /// asynchronous; check <see cref="ConnectionStatus"/>, or call
        /// <see cref="WaitUntilConnected"/> to block until it completes.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.CreateClient"/>
        public StreamSocket(Address address, ushort port, PropertiesID props = default) {
            ArgumentNullException.ThrowIfNull(address);
            Handle = SDL.CreateClient(address.Handle, port, props).ThrowIfInvalid();
        }

        internal StreamSocket(NativePtr<Opaque.SdlStreamSocket> handle) : base(handle, true) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetConnectionStatus"/>
        public Status ConnectionStatus => SDL.GetConnectionStatus(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetStreamSocketAddress"/>
        public Address? RemoteAddress {
            get {
                NativePtr<Opaque.SdlAddress> address = SDL.GetStreamSocketAddress(Handle).LogIfInvalid();
                return address.IsNull ? null : new Address(address, true);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetStreamSocketPendingWrites"/>
        public int PendingWrites => SDL.GetStreamSocketPendingWrites(Handle).LogIfInvalid(-1);

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.WaitUntilConnected"/>
        public bool WaitUntilConnected(int timeoutMs = -1) {
            Status status = SDL.WaitUntilConnected(Handle, timeoutMs);
            if (status == Status.Failure) {
                Error.LogError(nameof(WaitUntilConnected));
            }
            return status == Status.Success;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.WaitUntilStreamSocketDrained"/>
        public int WaitUntilDrained(int timeoutMs = -1) {
            return SDL.WaitUntilStreamSocketDrained(Handle, timeoutMs).LogIfInvalid(-1);
        }

        /// <summary>
        /// Reads bytes that have arrived from the remote peer into <paramref name="buffer"/>,
        /// without blocking.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.ReadFromStreamSocket"/>
        public int Read(NativePtr<byte> buffer, int length) {
            if (buffer.IsNull) return 0;
            return SDL.ReadFromStreamSocket(Handle, buffer, length).LogIfInvalid(-1);
        }

        /// <summary>
        /// Queues <paramref name="data"/> for sending to the remote peer, without blocking.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.WriteToStreamSocket"/>
        public unsafe bool Write(ReadOnlySpan<byte> data) {
            if (data.IsEmpty) return true;
            fixed (byte* ptr = data) {
                return SDL.WriteToStreamSocket(Handle, (IntPtr)ptr, data.Length).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.SimulateStreamPacketLoss"/>
        public void SimulatePacketLoss(int percentLoss) {
            SDL.SimulateStreamPacketLoss(Handle, percentLoss);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.DestroyStreamSocket"/>
        protected override void DisposeResource() {
            SDL.DestroyStreamSocket(Handle);
        }
    }
}
