// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Properties {
    public readonly struct StringProperty(uint handle, string name) {
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetStringProperty"/>
        public string? Get(string? defaultValue = null) {
            return SDL.GetStringProperty(handle, name, defaultValue).ToUtf8String();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.SetStringProperty"/>
        public bool Set(string value) {
            return SDL.SetStringProperty(handle, name, value).LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.ClearProperty"/>
        public bool Clear() {
            return SDL.ClearProperty(handle, name).LogIfFalse();
        }
        public override string ToString() {
            return Get() ?? string.Empty;
        }
    }
}
