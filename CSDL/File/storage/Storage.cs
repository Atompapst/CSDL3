// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.File {
    public partial class Storage : NativeHandle<Opaque.SdlStorage> {
        private string? _customCallbackId;

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.OpenTitleStorage"/>
        public static Storage OpenTitle(string? @override = null, PropertiesID props = default) {
            return new Storage(SDL.OpenTitleStorage(@override, props).ThrowIfInvalid());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.OpenUserStorage"/>
        public static Storage OpenUser(string org, string app, PropertiesID props = default) {
            return new Storage(SDL.OpenUserStorage(org, app, props).ThrowIfInvalid());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.OpenFileStorage"/>
        public static Storage OpenFile(string path) {
            return new Storage(SDL.OpenFileStorage(path).ThrowIfInvalid());
        }

        internal Storage(NativePtr<Opaque.SdlStorage> handle) {
            Handle = handle;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.StorageReady"/>
        public bool IsReady => SDL.StorageReady(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.GetStorageSpaceRemaining"/>
        public ulong SpaceRemaining => SDL.GetStorageSpaceRemaining(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.GetStorageFileSize"/>
        public bool GetFileSize(string path, out ulong length) {
            return SDL.GetStorageFileSize(Handle, path, out length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.ReadStorageFile"/>
        public bool ReadFile(string path, IntPtr destination, ulong length) {
            return SDL.ReadStorageFile(Handle, path, destination, length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.WriteStorageFile"/>
        public bool WriteFile(string path, IntPtr source, ulong length) {
            return SDL.WriteStorageFile(Handle, path, source, length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.CreateStorageDirectory"/>
        public bool CreateDirectory(string path) {
            return SDL.CreateStorageDirectory(Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.EnumerateStorageDirectory"/>
        public bool EnumerateDirectory(string path, EnumerateDirectoryCallback callback, object? userdata = null) {
            string id = Guid.NewGuid().ToString("N");
            SDL_EnumerateDirectoryCallbackNative nativeCallback = EnumerateDirectoryCallbackWrapper.Create(callback);
            
            (IntPtr _, IntPtr userdataPtr) = CallbackRegistry.Register(id, callback, nativeCallback, userdata);
            try {
                return SDL.EnumerateStorageDirectory(Handle, path, nativeCallback, userdataPtr).LogIfFalse();
            }
            finally {
                CallbackRegistry.Unregister<EnumerateDirectoryCallback, SDL_EnumerateDirectoryCallbackNative>(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.RemoveStoragePath"/>
        public bool RemovePath(string path) {
            return SDL.RemoveStoragePath(Handle, path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.RenameStoragePath"/>
        public bool RenamePath(string oldPath, string newPath) {
            return SDL.RenameStoragePath(Handle, oldPath, newPath).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.CopyStorageFile"/>
        public bool CopyFile(string oldPath, string newPath) {
            return SDL.CopyStorageFile(Handle, oldPath, newPath).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.GetStoragePathInfo"/>
        public bool GetPathInfo(string path, out PathInfo info) {
            return SDL.GetStoragePathInfo(Handle, path, out info).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.GlobStorageDirectory"/>
        public string[] GlobDirectory(string path, string pattern, GlobFlags flags) {
            NativePtr<nint> results = SDL.GlobStorageDirectory(Handle, path, pattern, flags, out int count);
            if (results.IsNull) {
                Error.LogError(nameof(GlobDirectory));
                return Array.Empty<string>();
            }
            string[] strings = NativeStringArray.ToArray(results, count);
            Memory.Free(results);
            return strings;
        }

        /// <summary>
        /// Creates a <see cref="Storage"/> backed by application-provided callbacks, matching
        /// <c>SDL_OpenStorage</c>. Every parameter but <paramref name="close"/> is optional
        /// (<see langword="null"/>), matching which operations SDL itself treats as optional depending
        /// on whether the storage is read-only, write-only, or both.
        /// </summary>
        /// <seealso cref="CSDL.Internal.Docs.Storage.OpenStorage">OpenStorage</seealso>
        public static Storage FromCustom(
            StorageInterface.CloseDelegate close,
            StorageInterface.ReadyDelegate? ready = null,
            StorageInterface.EnumerateDelegate? enumerate = null,
            StorageInterface.InfoDelegate? info = null,
            StorageInterface.ReadFileDelegate? readFile = null,
            StorageInterface.WriteFileDelegate? writeFile = null,
            StorageInterface.MkdirDelegate? mkdir = null,
            StorageInterface.RemoveDelegate? remove = null,
            StorageInterface.RenameDelegate? rename = null,
            StorageInterface.CopyDelegate? copy = null,
            StorageInterface.SpaceRemainingDelegate? spaceRemaining = null,
            object? userData = null) {
            StorageInterface iface = default;
            iface.InitVersion();
            string id = iface.Attach(close, ready, enumerate, info, readFile, writeFile, mkdir, remove, rename, copy, spaceRemaining, userData, out IntPtr userdataPtr);

            try {
                Storage storage = new Storage(SDL.OpenStorage(in iface, userdataPtr).ThrowIfInvalid());
                storage._customCallbackId = id;
                return storage;
            } catch {
                StorageInterface.Detach(id);
                throw;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.CloseStorage"/>
        protected override void DisposeResource() {
            SDL.CloseStorage(Handle).LogIfFalse();
            if (_customCallbackId != null) {
                StorageInterface.Detach(_customCallbackId);
                _customCallbackId = null;
            }
        }
    }
}
