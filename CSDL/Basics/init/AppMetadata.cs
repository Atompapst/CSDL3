// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL {
    public static class AppMetadata {
        /// <inheritdoc cref="CSDL.Props.AppMetadataNameString"/>
        public static string Name {
            get => GetAppMetadataProperty(Props.AppMetadataNameString);
            set => SetAppMetadataProperty(Props.AppMetadataNameString, value);
        }

        /// <inheritdoc cref="CSDL.Props.AppMetadataVersionString"/>
        public static string Version {
            get => GetAppMetadataProperty(Props.AppMetadataVersionString);
            set => SetAppMetadataProperty(Props.AppMetadataVersionString, value);
        }

        /// <inheritdoc cref="CSDL.Props.AppMetadataIdentifierString"/>
        public static string Identifier {
            get => GetAppMetadataProperty(Props.AppMetadataIdentifierString);
            set => SetAppMetadataProperty(Props.AppMetadataIdentifierString, value);
        }

        /// <inheritdoc cref="CSDL.Props.AppMetadataCreatorString"/>
        public static string Creator {
            get => GetAppMetadataProperty(Props.AppMetadataCreatorString);
            set => SetAppMetadataProperty(Props.AppMetadataCreatorString, value);
        }

        /// <inheritdoc cref="CSDL.Props.AppMetadataCopyrightString"/>
        public static string Copyright {
            get => GetAppMetadataProperty(Props.AppMetadataCopyrightString);
            set => SetAppMetadataProperty(Props.AppMetadataCopyrightString, value);
        }

        /// <inheritdoc cref="CSDL.Props.AppMetadataURLString"/>
        public static string URL {
            get => GetAppMetadataProperty(Props.AppMetadataURLString);
            set => SetAppMetadataProperty(Props.AppMetadataURLString, value);
        }

        /// <inheritdoc cref="CSDL.Props.AppMetadataTypeString"/>
        public static string Type {
            get => GetAppMetadataProperty(Props.AppMetadataTypeString);
            set => SetAppMetadataProperty(Props.AppMetadataTypeString, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Init.SetAppMetadataProperty"/>
        private static bool SetAppMetadataProperty(string name, string? value = null) {
            return SDL.SetAppMetadataProperty(name, value).LogIfFalse();
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Init.GetAppMetadataProperty"/>
        private static string GetAppMetadataProperty(string name) {
            return SDL.GetAppMetadataProperty(name).ToUtf8String() ?? string.Empty;
        }
    }
}
