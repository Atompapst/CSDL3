// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Internal {
    /// <summary>
    /// Lets a native owner invalidate a dependent handle without depending on its wrapper type.
    /// </summary>
    internal sealed class InvalidationRegistration {
        private readonly Action _invalidate;

        internal InvalidationRegistration(Action invalidate) {
            _invalidate = invalidate;
        }

        internal void Invalidate() => _invalidate();
    }
}
