// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.Net {
    /// <summary>
    /// Listens for and accepts incoming <see cref="StreamSocket"/> connections from clients.
    /// </summary>
    public sealed class Server : NativeHandle<Opaque.SdlServer> {
        static Server() {
            Net.EnsureInitialized();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.CreateServer"/>
        public Server(Address? addr = null, ushort port = 0, PropertiesID props = default) {
            Handle = SDL.CreateServer(addr?.Handle ?? default, port, props).ThrowIfInvalid();
        }

        /// <summary>
        /// Checks for a pending client connection, without blocking, and accepts it if one is
        /// waiting. Call this in a loop (or after <see cref="Net.WaitUntilInputAvailable"/>
        /// reports this server as ready) to accept all pending connections.
        /// </summary>
        /// <param name="client">The newly-accepted client, or <see langword="null"/> if none were pending.</param>
        /// <returns><see langword="true"/> unless a real error occurred (a pending-but-empty check still returns <see langword="true"/>).</returns>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.AcceptClient"/>
        public bool TryAcceptClient(out StreamSocket? client) {
            if (!SDL.AcceptClient(Handle, out NativePtr<Opaque.SdlStreamSocket> clientHandle).LogIfFalse()) {
                client = null;
                return false;
            }

            client = clientHandle.IsNull ? null : new StreamSocket(clientHandle);
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.DestroyServer"/>
        protected override void DisposeResource() {
            SDL.DestroyServer(Handle);
        }
    }
}
