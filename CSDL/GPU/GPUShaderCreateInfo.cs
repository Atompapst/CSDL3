// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;

namespace CSDL.GPU {
    public partial struct GPUShaderCreateInfo {
        /// <summary>
        /// Sets the entry point function name for the shader, allocating a native UTF-8 copy.
        /// The caller is responsible for freeing the previous pointer (if any) via <see cref="FreeEntrypoint"/>.
        /// </summary>
        public void SetEntrypoint(string entrypoint) {
            _entrypoint = entrypoint != null ? Marshal.StringToCoTaskMemUTF8(entrypoint) : IntPtr.Zero;
        }

        /// <summary>
        /// Frees the native UTF-8 string previously allocated by <see cref="SetEntrypoint"/>.
        /// </summary>
        public void FreeEntrypoint() {
            if (_entrypoint != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(_entrypoint);
                _entrypoint = IntPtr.Zero;
            }
        }
    }
}
