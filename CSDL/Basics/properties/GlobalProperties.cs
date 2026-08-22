// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL {
    public static class GlobalProperties {
        /// <inheritdoc cref="CSDL.Internal.Docs.Properties.GetGlobalProperties"/>
        private static uint Handle => SDL.GetGlobalProperties();
        public static StringProperty String(string name) {
            return new StringProperty(Handle, name);
        }
        public static NumberProperty Number(string name) {
            return new NumberProperty(Handle, name);
        }
        public static BooleanProperty Bool(string name) {
            return new BooleanProperty(Handle, name);
        }
        public static FloatProperty Float(string name) {
            return new FloatProperty(Handle, name);
        }
        public static PointerProperty Pointer(string name) {
            return new PointerProperty(Handle, name);
        }
    }
}
