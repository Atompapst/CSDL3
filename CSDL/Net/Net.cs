// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Net {
    /// <summary>
    /// Entry point for the SDL_net subsystem: initialization, versioning, and system-wide queries
    /// that aren't tied to a particular socket or address.
    /// </summary>
    public static class Net {
        private static bool _initialized;

        static Net() {
            Init.OnQuit += Quit;
        }

        /// <summary>
        /// Initializes SDL_net if it hasn't been already. Called automatically by
        /// <see cref="Address"/>, <see cref="StreamSocket"/>, <see cref="Server"/>, and
        /// <see cref="DatagramSocket"/> before they touch the network.
        /// </summary>
        internal static void EnsureInitialized() {
            if (_initialized) return;
            SDL.Init().ThrowIfFalse();
            _initialized = true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.Quit"/>
        internal static void Quit() {
            if (!_initialized) return;
            SDL.Quit();
            _initialized = false;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.Version"/>
        public static int Version => SDL.Version();

        /// <inheritdoc cref="Macros.NetMajorVersion"/>
        public static uint MajorVersion => Macros.NetMajorVersion;
        /// <inheritdoc cref="Macros.NetMinorVersion"/>
        public static uint MinorVersion => Macros.NetMinorVersion;
        /// <inheritdoc cref="Macros.NetMicroVersion"/>
        public static uint MicroVersion => Macros.NetMicroVersion;

        /// <inheritdoc cref="Macros.NetVersionAtleast"/>
        public static bool VersionAtleast(uint x, uint y, uint z) {
            return Macros.NetVersionAtleast(x, y, z);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.GetLocalAddresses"/>
        public static Address[] GetLocalAddresses() {
            EnsureInitialized();

            NativePtr<nint> arrayPtr = SDL.GetLocalAddresses(out int numAddresses);
            arrayPtr.ThrowIfInvalid();

            try {
                Address[] result = new Address[numAddresses];
                for (int i = 0; i < numAddresses; i++) {
                    // The array only holds a reference for as long as SDL.FreeLocalAddresses
                    // hasn't run yet, so ref each address we want to keep before that happens.
                    NativePtr<Opaque.SdlAddress> refed = SDL.RefAddress(arrayPtr[i]);
                    result[i] = new Address(refed, true);
                }
                return result;
            }
            finally {
                SDL.FreeLocalAddresses(arrayPtr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.SimulateAddressResolutionLoss"/>
        public static void SimulateAddressResolutionLoss(int percentLoss) {
            SDL.SimulateAddressResolutionLoss(percentLoss);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Net.WaitUntilInputAvailable"/>
        public static int WaitUntilInputAvailable(INativeHandle[] sockets, int timeoutMs = -1) {
            EnsureInitialized();
            ArgumentNullException.ThrowIfNull(sockets);
            if (sockets.Length == 0) return 0;

            nint[] pointers = new nint[sockets.Length];
            for (int i = 0; i < sockets.Length; i++) {
                pointers[i] = sockets[i].NativePointer;
            }

            unsafe {
                fixed (nint* ptr = pointers) {
                    return SDL.WaitUntilInputAvailable((nint)ptr, pointers.Length, timeoutMs).LogIfInvalid(-1);
                }
            }
        }
    }
}
