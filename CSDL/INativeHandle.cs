// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    /// <summary>
    /// A wrapper around a native SDL resource that this process has to release again.
    /// </summary>
    public interface INativeHandle : IDisposable {
        /// <summary>The raw native pointer, or <c>0</c> once the resource is gone.</summary>
        nint NativePointer { get; }

        /// <summary>Whether the underlying resource is still alive and usable.</summary>
        bool IsValid { get; }
    }
}
