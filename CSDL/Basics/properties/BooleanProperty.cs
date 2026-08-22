// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Properties {
    public readonly struct BooleanProperty(uint handle, string name) {
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetBooleanProperty"/>
        public bool Get(bool defaultValue = false) {
            return SDL.GetBooleanProperty(handle, name, defaultValue);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.SetBooleanProperty"/>
        public bool Set(bool value) {
            return SDL.SetBooleanProperty(handle, name, value).LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.ClearProperty"/>
        public bool Clear() {
            return SDL.ClearProperty(handle, name).LogIfFalse();
        }
        public static implicit operator bool(BooleanProperty prop) {
            return prop.Get();
        }
        public override string ToString() {
            return Get() ? "true" : "false";
        }
    }
}
