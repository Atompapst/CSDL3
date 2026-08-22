// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;

namespace CSDL.File {
    public partial struct StorageInterface {
        /// <inheritdoc cref="CSDL.Internal.Docs.Storage.CloseStorage"/>
        public delegate bool CloseDelegate(object? userdata);

        /// <summary>Returns whether the storage is currently ready for access. Optional - a storage that's always ready doesn't need to implement this.</summary>
        public delegate bool ReadyDelegate(object? userdata);

        /// <summary>
        /// Enumerates <paramref name="path"/>, calling <paramref name="reportEntry"/> once per directory
        /// entry with its directory name and filename - this already wraps the raw native callback SDL
        /// passed in, so no pointer/marshaling code is needed here. Optional for write-only storage.
        /// </summary>
        public delegate bool EnumerateDelegate(object? userdata, string path, Func<string, string, EnumerationResult> reportEntry);

        /// <summary>Gets path information. Optional for write-only storage.</summary>
        public delegate bool InfoDelegate(object? userdata, string path, out PathInfo info);

        /// <summary>Reads exactly <paramref name="length"/> bytes from <paramref name="path"/> into <paramref name="destination"/>. Optional for write-only storage.</summary>
        public delegate bool ReadFileDelegate(object? userdata, string path, nint destination, ulong length);

        /// <summary>Writes exactly <paramref name="length"/> bytes from <paramref name="source"/> to <paramref name="path"/>. Optional for read-only storage.</summary>
        public delegate bool WriteFileDelegate(object? userdata, string path, nint source, ulong length);

        /// <summary>Creates a directory. Optional for read-only storage.</summary>
        public delegate bool MkdirDelegate(object? userdata, string path);

        /// <summary>Removes a file or empty directory. Optional for read-only storage.</summary>
        public delegate bool RemoveDelegate(object? userdata, string path);

        /// <summary>Renames a path. Optional for read-only storage.</summary>
        public delegate bool RenameDelegate(object? userdata, string oldpath, string newpath);

        /// <summary>Copies a file. Optional for read-only storage.</summary>
        public delegate bool CopyDelegate(object? userdata, string oldpath, string newpath);

        /// <summary>Gets the space remaining, in bytes. Optional for read-only storage.</summary>
        public delegate ulong SpaceRemainingDelegate(object? userdata);

        // The actual native ABI shape SDL calls - see IOStreamInterface.cs for why these stay internal
        // (so CBool works) while the delegates above are public and app-facing.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool CloseNative(nint userdata);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool ReadyNative(nint userdata);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool EnumerateNative(nint userdata, nint path, nint callback, nint callbackUserdata);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool InfoNative(nint userdata, nint path, out PathInfo info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool ReadFileNative(nint userdata, nint path, nint destination, ulong length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool WriteFileNative(nint userdata, nint path, nint source, ulong length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool MkdirNative(nint userdata, nint path);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool RemoveNative(nint userdata, nint path);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool RenameNative(nint userdata, nint oldpath, nint newpath);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool CopyNative(nint userdata, nint oldpath, nint newpath);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ulong SpaceRemainingNative(nint userdata);

        private sealed class Callbacks {
            public CloseDelegate? Close { get; init; }
            public ReadyDelegate? Ready { get; init; }
            public EnumerateDelegate? Enumerate { get; init; }
            public InfoDelegate? Info { get; init; }
            public ReadFileDelegate? ReadFile { get; init; }
            public WriteFileDelegate? WriteFile { get; init; }
            public MkdirDelegate? Mkdir { get; init; }
            public RemoveDelegate? Remove { get; init; }
            public RenameDelegate? Rename { get; init; }
            public CopyDelegate? Copy { get; init; }
            public SpaceRemainingDelegate? SpaceRemaining { get; init; }
            public object? UserData { get; init; }
        }

        private static Callbacks Resolve(nint userdata) {
            return (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
        }

        private static CBool CloseTrampoline(nint userdata) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.Close!(cb.UserData);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool ReadyTrampoline(nint userdata) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.Ready?.Invoke(cb.UserData) ?? true;
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool EnumerateTrampoline(nint userdata, nint path, nint callback, nint callbackUserdata) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.Enumerate is null) return false;

                string pathString = Marshal.PtrToStringUTF8(path) ?? string.Empty;
                SDL_EnumerateDirectoryCallbackNative nativeCallback = Marshal.GetDelegateForFunctionPointer<SDL_EnumerateDirectoryCallbackNative>(callback);

                EnumerationResult ReportEntry(string dirname, string fname) {
                    nint dirnamePtr = Marshal.StringToCoTaskMemUTF8(dirname);
                    nint fnamePtr = Marshal.StringToCoTaskMemUTF8(fname);
                    try {
                        return nativeCallback(callbackUserdata, dirnamePtr, fnamePtr);
                    }
                    finally {
                        Marshal.FreeCoTaskMem(dirnamePtr);
                        Marshal.FreeCoTaskMem(fnamePtr);
                    }
                }

                return cb.Enumerate(cb.UserData, pathString, ReportEntry);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool InfoTrampoline(nint userdata, nint path, out PathInfo info) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.Info is null) {
                    info = default;
                    return false;
                }
                return cb.Info(cb.UserData, Marshal.PtrToStringUTF8(path) ?? string.Empty, out info);
            } catch {
                info = default;
                SetCallbackError();
                return false;
            }
        }

        private static CBool ReadFileTrampoline(nint userdata, nint path, nint destination, ulong length) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.ReadFile is null) return false;
                return cb.ReadFile(cb.UserData, Marshal.PtrToStringUTF8(path) ?? string.Empty, destination, length);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool WriteFileTrampoline(nint userdata, nint path, nint source, ulong length) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.WriteFile is null) return false;
                return cb.WriteFile(cb.UserData, Marshal.PtrToStringUTF8(path) ?? string.Empty, source, length);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool MkdirTrampoline(nint userdata, nint path) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.Mkdir is null) return false;
                return cb.Mkdir(cb.UserData, Marshal.PtrToStringUTF8(path) ?? string.Empty);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool RemoveTrampoline(nint userdata, nint path) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.Remove is null) return false;
                return cb.Remove(cb.UserData, Marshal.PtrToStringUTF8(path) ?? string.Empty);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool RenameTrampoline(nint userdata, nint oldpath, nint newpath) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.Rename is null) return false;
                return cb.Rename(cb.UserData, Marshal.PtrToStringUTF8(oldpath) ?? string.Empty, Marshal.PtrToStringUTF8(newpath) ?? string.Empty);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static CBool CopyTrampoline(nint userdata, nint oldpath, nint newpath) {
            try {
                Callbacks cb = Resolve(userdata);
                if (cb.Copy is null) return false;
                return cb.Copy(cb.UserData, Marshal.PtrToStringUTF8(oldpath) ?? string.Empty, Marshal.PtrToStringUTF8(newpath) ?? string.Empty);
            } catch {
                SetCallbackError();
                return false;
            }
        }

        private static ulong SpaceRemainingTrampoline(nint userdata) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.SpaceRemaining?.Invoke(cb.UserData) ?? 0;
            } catch {
                SetCallbackError();
                return 0;
            }
        }

        private static void SetCallbackError() {
            try {
                Error.SetError("Managed storage callback threw an exception.");
            } catch {
                // Never allow error reporting to escape a native callback either.
            }
        }

        /// <summary>
        /// Reproduces the <c>SDL_INIT_INTERFACE</c> macro's <c>version</c> assignment (<c>iface-&gt;version = sizeof(*iface)</c>).
        /// </summary>
        /// <remarks>
        /// Call this on a freshly zeroed (<see langword="default"/>) instance before calling
        /// <see cref="Attach"/>.
        /// </remarks>
        public void InitVersion() {
            Version = (uint)Marshal.SizeOf<StorageInterface>();
        }

        /// <summary>
        /// Registers every provided operation with <see cref="CallbackRegistry"/>, assigns the resulting
        /// native function pointers, and returns the id (pass to <see cref="Detach"/> once the storage
        /// closes) and the single userdata pointer to pass as <c>SDL_OpenStorage</c>'s <c>userData</c>.
        /// Every parameter but <paramref name="close"/> is optional (<see langword="null"/>), matching
        /// SDL's own "several of these are optional depending on read/write direction" contract - the
        /// generated field is simply left at its zeroed default (a null function pointer) when omitted.
        /// </summary>
        internal string Attach(
            CloseDelegate close, ReadyDelegate? ready, EnumerateDelegate? enumerate, InfoDelegate? info,
            ReadFileDelegate? readFile, WriteFileDelegate? writeFile, MkdirDelegate? mkdir, RemoveDelegate? remove,
            RenameDelegate? rename, CopyDelegate? copy, SpaceRemainingDelegate? spaceRemaining,
            object? userData, out IntPtr userdataPtr) {
            string id = Guid.NewGuid().ToString("N");
            Callbacks callbacks = new Callbacks {
                Close = close, Ready = ready, Enumerate = enumerate, Info = info, ReadFile = readFile,
                WriteFile = writeFile, Mkdir = mkdir, Remove = remove, Rename = rename, Copy = copy,
                SpaceRemaining = spaceRemaining, UserData = userData,
            };

            // Only the first registration actually allocates a GCHandle, same reasoning as
            // IOStreamInterface.Attach - SDL only ever hands back the one pointer this call returns.
            (IntPtr closePtr, IntPtr sharedUserdataPtr) = CallbackRegistry.Register<CloseDelegate, CloseNative>(id + ":close", close, CloseTrampoline, callbacks);
            Close = closePtr;

            if (ready != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<ReadyDelegate, ReadyNative>(id + ":ready", ready, ReadyTrampoline);
                Ready = ptr;
            }
            if (enumerate != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<EnumerateDelegate, EnumerateNative>(id + ":enumerate", enumerate, EnumerateTrampoline);
                Enumerate = ptr;
            }
            if (info != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<InfoDelegate, InfoNative>(id + ":info", info, InfoTrampoline);
                Info = ptr;
            }
            if (readFile != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<ReadFileDelegate, ReadFileNative>(id + ":readFile", readFile, ReadFileTrampoline);
                ReadFile = ptr;
            }
            if (writeFile != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<WriteFileDelegate, WriteFileNative>(id + ":writeFile", writeFile, WriteFileTrampoline);
                WriteFile = ptr;
            }
            if (mkdir != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<MkdirDelegate, MkdirNative>(id + ":mkdir", mkdir, MkdirTrampoline);
                Mkdir = ptr;
            }
            if (remove != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<RemoveDelegate, RemoveNative>(id + ":remove", remove, RemoveTrampoline);
                Remove = ptr;
            }
            if (rename != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<RenameDelegate, RenameNative>(id + ":rename", rename, RenameTrampoline);
                Rename = ptr;
            }
            if (copy != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<CopyDelegate, CopyNative>(id + ":copy", copy, CopyTrampoline);
                Copy = ptr;
            }
            if (spaceRemaining != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<SpaceRemainingDelegate, SpaceRemainingNative>(id + ":spaceRemaining", spaceRemaining, SpaceRemainingTrampoline);
                SpaceRemaining = ptr;
            }

            userdataPtr = sharedUserdataPtr;
            return id;
        }

        /// <summary>Unregisters everything <see cref="Attach"/> registered under <paramref name="id"/>, freeing the shared userdata GCHandle.</summary>
        internal static void Detach(string id) {
            CallbackRegistry.Unregister<CloseDelegate, CloseNative>(id + ":close");
            CallbackRegistry.Unregister<ReadyDelegate, ReadyNative>(id + ":ready");
            CallbackRegistry.Unregister<EnumerateDelegate, EnumerateNative>(id + ":enumerate");
            CallbackRegistry.Unregister<InfoDelegate, InfoNative>(id + ":info");
            CallbackRegistry.Unregister<ReadFileDelegate, ReadFileNative>(id + ":readFile");
            CallbackRegistry.Unregister<WriteFileDelegate, WriteFileNative>(id + ":writeFile");
            CallbackRegistry.Unregister<MkdirDelegate, MkdirNative>(id + ":mkdir");
            CallbackRegistry.Unregister<RemoveDelegate, RemoveNative>(id + ":remove");
            CallbackRegistry.Unregister<RenameDelegate, RenameNative>(id + ":rename");
            CallbackRegistry.Unregister<CopyDelegate, CopyNative>(id + ":copy");
            CallbackRegistry.Unregister<SpaceRemainingDelegate, SpaceRemainingNative>(id + ":spaceRemaining");
        }
    }
}
