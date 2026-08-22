// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.File {
    public partial class IOStream {
        private string? _customCallbackId;

        /// <summary>
        /// Creates an <see cref="IOStream"/> backed by application-provided callbacks, matching
        /// <c>SDL_OpenIO</c>. <paramref name="userData"/> (if any) is handed back to every one of the
        /// callbacks unchanged - it does not need to be pinned or otherwise kept alive by the caller,
        /// this instance already does that for as long as the stream is open.
        /// </summary>
        public static IOStream FromCustom(
            IOStreamInterface.SizeDelegate size,
            IOStreamInterface.SeekDelegate seek,
            IOStreamInterface.ReadDelegate read,
            IOStreamInterface.WriteDelegate write,
            IOStreamInterface.FlushDelegate flush,
            IOStreamInterface.CloseDelegate close,
            object? userData = null) {
            IOStreamInterface iface = default;
            iface.InitVersion();
            string id = iface.Attach(size, seek, read, write, flush, close, userData, out IntPtr userdataPtr);

            try {
                IOStream stream = new IOStream(SDL.OpenIO(in iface, userdataPtr).ThrowIfInvalid());
                stream._customCallbackId = id;
                return stream;
            } catch {
                IOStreamInterface.Detach(id);
                throw;
            }
        }

        partial void OnCustomIOStreamClosed() {
            if (_customCallbackId != null) {
                IOStreamInterface.Detach(_customCallbackId);
                _customCallbackId = null;
            }
        }
    }
}
