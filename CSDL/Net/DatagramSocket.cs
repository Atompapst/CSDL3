// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Net {
    /// <summary>
    /// An unreliable, unordered packet socket: sends and receives whole packets to/from any
    /// address, with no notion of a persistent connection.
    /// </summary>
    public sealed class DatagramSocket : NativeHandle<Opaque.SdlDatagramSocket> {
        static DatagramSocket() {
            Net.EnsureInitialized();
        }

        /// <param name="bindAddress">The local address to listen on, or <see langword="null"/> to listen on all available local addresses.</param>
        /// <param name="port">The local port to bind to, or 0 to let the system pick one.</param>
        /// <param name="properties">Properties of the new socket. Defaults to zero.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.CreateDatagramSocket"/>
        public DatagramSocket(Address? bindAddress = null, ushort port = 0, PropertiesID properties = default) {
            Handle = SDL.CreateDatagramSocket(bindAddress?.Handle ?? default, port, properties).ThrowIfInvalid();
        }

        /// <summary>
        /// Sends <paramref name="data"/> as a single packet. Pass <see langword="null"/> for
        /// <paramref name="destination"/> to broadcast (only if this socket allows it).
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.SendDatagram"/>
        public unsafe bool Send(Address? destination, ushort port, ReadOnlySpan<byte> data) {
            fixed (byte* ptr = data) {
                return SDL.SendDatagram(Handle, destination?.Handle ?? default, port, (IntPtr)ptr, data.Length).LogIfFalse();
            }
        }

        /// <summary>
        /// Checks for a new packet, without blocking, and copies it into a managed
        /// <see cref="ReceivedDatagram"/> if one is available.
        /// </summary>
        /// <param name="datagram">The received packet, or <see langword="default"/> if none was pending.</param>
        /// <returns><see langword="true"/> unless a real error occurred (a pending-but-empty check still returns <see langword="true"/>).</returns>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.ReceiveDatagram"/>
        public unsafe bool TryReceive(out ReceivedDatagram datagram) {
            // NET_ReceiveDatagram takes a NET_Datagram** (pointer-to-pointer): the generated
            // binding models the out-parameter as a plain NativePtr<nint> rather than an 'out'
            // parameter, so the double indirection is built by hand here.
            nint dgramPtrValue;
            if (!SDL.ReceiveDatagram(Handle, (nint)(&dgramPtrValue)).LogIfFalse()) {
                datagram = default;
                return false;
            }

            if (dgramPtrValue == 0) {
                datagram = default;
                return true;
            }

            Datagram* raw = (Datagram*)dgramPtrValue;
            Address? sender = raw->Addr == 0 ? null : new Address(SDL.RefAddress(raw->Addr), true);
            byte[] payload = raw->Buflen > 0
                ? new ReadOnlySpan<byte>((void*)raw->Buf, raw->Buflen).ToArray()
                : Array.Empty<byte>();
            datagram = new ReceivedDatagram(sender, raw->Port, payload);

            SDL.DestroyDatagram(dgramPtrValue);
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.SimulateDatagramPacketLoss"/>
        public void SimulatePacketLoss(int percentLoss) {
            SDL.SimulateDatagramPacketLoss(Handle, percentLoss);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.DestroyDatagramSocket"/>
        protected override void DisposeResource() {
            SDL.DestroyDatagramSocket(Handle);
        }
    }
}
