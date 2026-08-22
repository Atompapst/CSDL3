// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Globalization;
using CSDL.Extensions;
namespace CSDL.Properties {
    public readonly struct FloatProperty(uint handle, string name) {
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetFloatProperty"/>
        public float Get(float defaultValue = 0.0f) {
            return SDL.GetFloatProperty(handle, name, defaultValue);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.SetFloatProperty"/>
        public bool Set(float value) {
            return SDL.SetFloatProperty(handle, name, value).LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.ClearProperty"/>
        public bool Clear() {
            return SDL.ClearProperty(handle, name).LogIfFalse();
        }
        public static implicit operator float(FloatProperty prop) {
            return prop.Get();
        }
        public override string ToString() {
            return Get().ToString(CultureInfo.InvariantCulture);
        }
    }
}
