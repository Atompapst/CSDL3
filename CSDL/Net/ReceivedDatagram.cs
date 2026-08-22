// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Net {
    /// <summary>
    /// A packet received via <see cref="DatagramSocket.TryReceive"/>, copied out of SDL_net's
    /// native buffer into managed memory so it outlives the native <c>NET_Datagram</c>.
    /// </summary>
    /// <param name="Sender">The address that sent this packet, or <see langword="null"/> if SDL_net
    /// couldn't determine one. Dispose it when done, or ignore it - it's a fresh reference owned by
    /// this record.</param>
    /// <param name="Port">The port the packet was sent from.</param>
    /// <param name="Payload">The packet's payload bytes.</param>
    public readonly record struct ReceivedDatagram(Address? Sender, ushort Port, byte[] Payload);
}
