// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Globalization;
using CSDL.Extensions;
namespace CSDL.Properties {
    public readonly struct NumberProperty(uint handle, string name) {
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetNumberProperty"/>
        public long Get(long defaultValue = 0) {
            return SDL.GetNumberProperty(handle, name, defaultValue);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.SetNumberProperty"/>
        public bool Set(long value) {
            return SDL.SetNumberProperty(handle, name, value).LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.ClearProperty"/>
        public bool Clear() {
            return SDL.ClearProperty(handle, name).LogIfFalse();
        }
        public static implicit operator long(NumberProperty prop) {
            return prop.Get();
        }
        public override string ToString() {
            return Get().ToString(CultureInfo.InvariantCulture);
        }
    }
}
