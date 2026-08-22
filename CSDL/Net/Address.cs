// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.Extensions;

namespace CSDL.Net {
    /// <summary>
    /// A network address, obtained by resolving a hostname or IP string. Reference-counted
    /// internally by SDL_net; disposing this drops CSDL's reference.
    /// </summary>
    public sealed class Address : NativeHandle<Opaque.SdlAddress>, IComparable<Address> {
        static Address() {
            Net.EnsureInitialized();
        }

        internal Address(NativePtr<Opaque.SdlAddress> handle, bool ownsHandle) : base(handle, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetAddressStatus"/>
        public Status Status => SDL.GetAddressStatus(Handle);

        /// <summary>
        /// Begins resolving <paramref name="hostname"/> (or a literal IP string). Resolution
        /// happens asynchronously; check <see cref="Status"/>, or call
        /// <see cref="WaitUntilResolved"/> to block until it completes.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Net.ResolveHostname"/>
        public static Address Resolve(string hostname) {
            return new Address(SDL.ResolveHostname(hostname).ThrowIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.WaitUntilResolved"/>
        public bool WaitUntilResolved(int timeoutMs = -1) {
            Status status = SDL.WaitUntilResolved(Handle, timeoutMs);
            if (status == Status.Failure) {
                Error.LogError(nameof(WaitUntilResolved));
            }
            return status == Status.Success;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetAddressBytes"/>
        public byte[] GetBytes() {
            IntPtr ptr = SDL.GetAddressBytes(Handle, out int numBytes);
            if (ptr == IntPtr.Zero || numBytes <= 0) {
                return Array.Empty<byte>();
            }

            byte[] result = new byte[numBytes];
            Marshal.Copy(ptr, result, 0, numBytes);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.CompareAddresses"/>
        public int CompareTo(Address? other) {
            if (other is null) return 1;
            return SDL.CompareAddresses(Handle, other.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetAddressString"/>
        public override string? ToString() {
            return SDL.GetAddressString(Handle).ToUtf8String() ?? base.ToString();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.UnrefAddress"/>
        protected override void DisposeResource() {
            SDL.UnrefAddress(Handle);
        }
    }
}
