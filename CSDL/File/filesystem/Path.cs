// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.File {
    public static class Path {
        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.GetBasePath"/>
        public static string? BasePath => SDL.GetBasePath().ToUtf8StringOrLog();

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.GetCurrentDirectory"/>
        public static string? CurrentDirectory => SDL.GetCurrentDirectory().ToUtf8StringAndFree();

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.GetPrefPath"/>
        public static string? GetPrefPath(string org, string app) {
            return SDL.GetPrefPath(org, app).ToUtf8StringAndFree();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.GetUserFolder"/>
        public static string? GetUserFolder(Folder folder) {
            return SDL.GetUserFolder(folder).ToUtf8StringOrLog();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.CreateDirectory"/>
        public static bool CreateDirectory(string path) {
            return SDL.CreateDirectory(path);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.RemovePath"/>
        public static bool RemovePath(string path) {
            return SDL.RemovePath(path);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.RenamePath"/>
        public static bool RenamePath(string oldPath, string newPath) {
            return SDL.RenamePath(oldPath, newPath);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.EnumerateDirectory"/>
        public static bool EnumerateDirectory(string path, EnumerateDirectoryCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            string id = Guid.NewGuid().ToString("N");
            SDL_EnumerateDirectoryCallbackNative cb = EnumerateDirectoryCallbackWrapper.Create(callback);
            (IntPtr _, IntPtr userdataPtr) = CallbackRegistry.Register(id, callback, cb, userdata);
            try {
                return SDL.EnumerateDirectory(path, cb, userdataPtr).LogIfFalse();
            } finally {
                CallbackRegistry.Unregister<EnumerateDirectoryCallback, SDL_EnumerateDirectoryCallbackNative>(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.CopyFile"/>
        public static bool CopyFile(string oldPath, string newPath) {
            return SDL.CopyFile(oldPath, newPath);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.GetPathInfo"/>
        public static bool GetInfo(string path, out PathInfo info) {
            return SDL.GetPathInfo(path, out info).LogIfFalse();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Filesystem.GlobDirectory"/>
        public static string[] GlobDirectory(string path, string pattern, GlobFlags flags, out int count) {
            NativePtr<nint> results = SDL.GlobDirectory(path, pattern, flags, out count);
            if (results.IsNull) {
                Error.LogError();
                return Array.Empty<string>();
            }
            string[] strings = NativeStringArray.ToArray(results, count);
            Memory.Free(results);
            return strings;
        }
    }
}
