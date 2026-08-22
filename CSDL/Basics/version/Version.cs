// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL {
    public static class Version {
        /// <inheritdoc cref="CSDL.Internal.Docs.Version.GetVersion"/>
        public static int SdlVersion => SDL.GetVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.Version.GetRevision"/>
        public static string Revision => SDL.GetRevision().ToUtf8String() ?? string.Empty;

        /// <inheritdoc cref="Macros.Version"/>
        public static uint VersionCompiled => Macros.Version;

        /// <inheritdoc cref="Macros.MajorVersion"/>
        public const uint Major = Macros.MajorVersion;

        /// <inheritdoc cref="Macros.MinorVersion"/>
        public const uint Minor = Macros.MinorVersion;

        /// <inheritdoc cref="Macros.MicroVersion"/>
        public const uint Micro = Macros.MicroVersion;

        /// <inheritdoc cref="Macros.Versionnum"/>
        public static uint Num(uint major, uint minor, uint patch) {
            return Macros.Versionnum(major, minor, patch);
        }

        /// <inheritdoc cref="Macros.VersionnumMajor"/>
        public static uint NumMajor(uint version) {
            return Macros.VersionnumMajor(version);
        }

        /// <inheritdoc cref="Macros.VersionnumMinor"/>
        public static uint NnumMinor(uint version) {
            return Macros.VersionnumMinor(version);
        }

        /// <inheritdoc cref="Macros.VersionnumMicro"/>
        public static uint NumMicro(uint version) {
            return Macros.VersionnumMicro(version);
        }

        /// <inheritdoc cref="Macros.VersionAtleast"/>
        public static bool Atleast(uint x, uint y, uint z) {
            return Macros.VersionAtleast(x, y, z);
        }
    }
}
