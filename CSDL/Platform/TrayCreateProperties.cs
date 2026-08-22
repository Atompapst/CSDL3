// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CSDL.Properties;
using CSDL.Video;

namespace CSDL {
    /// <summary>
    ///     The property set understood by <see cref="Tray(TrayCreateProperties)"/>.
    /// </summary>
    /// <seealso cref="CSDL.Internal.Docs.Tray.CreateTrayWithProperties">SDL_CreateTrayWithProperties</seealso>
    public sealed class TrayCreateProperties : PropertyGroup {
        // The generated Props.TrayCreateDoubleclickCallbackPointer carries the macro's *name* instead of
        // its value, so the real property string is spelled out here.
        private const string DoubleclickCallbackPointer = "SDL.tray.create.doubleclick_callback";
        private readonly HashSet<string> _clickCallbackIds = new HashSet<string>();
        private readonly List<GCHandle> _userdataHandles = new List<GCHandle>();

        /// <inheritdoc cref="CSDL.Props.TrayCreateIconPointer"/>
        public PointerProperty Icon => PropPointer(Props.TrayCreateIconPointer);

        /// <inheritdoc cref="CSDL.Props.TrayCreateTooltipString"/>
        public StringProperty Tooltip => PropString(Props.TrayCreateTooltipString);

        /// <inheritdoc cref="CSDL.Props.TrayCreateLeftclickCallbackPointer"/>
        public PointerProperty LeftClickCallback => PropPointer(Props.TrayCreateLeftclickCallbackPointer);

        /// <inheritdoc cref="CSDL.Props.TrayCreateMiddleclickCallbackPointer"/>
        public PointerProperty MiddleClickCallback => PropPointer(Props.TrayCreateMiddleclickCallbackPointer);

        /// <inheritdoc cref="CSDL.Props.TrayCreateRightclickCallbackPointer"/>
        public PointerProperty RightClickCallback => PropPointer(Props.TrayCreateRightclickCallbackPointer);

        /// <inheritdoc cref="CSDL.Props.TrayCreateDoubleclickCallbackPointer"/>
        public PointerProperty DoubleClickCallback => PropPointer(DoubleclickCallbackPointer);

        /// <inheritdoc cref="CSDL.Props.TrayCreateUserdataPointer"/>
        public PointerProperty Userdata => PropPointer(Props.TrayCreateUserdataPointer);

        /// <summary>Sets the tray icon from a surface.</summary>
        /// <remarks>SDL only reads the surface while the tray is being created.</remarks>
        public void SetIcon(Surface icon) {
            ArgumentNullException.ThrowIfNull(icon);
            Icon.Set(icon.NativePointer);
        }

        /// <inheritdoc cref="SetClickCallback"/>
        public void SetLeftClickCallback(TrayClickCallback callback, object? userdata = null) {
            SetClickCallback(Props.TrayCreateLeftclickCallbackPointer, nameof(LeftClickCallback), callback, userdata);
        }

        /// <inheritdoc cref="SetClickCallback"/>
        public void SetMiddleClickCallback(TrayClickCallback callback, object? userdata = null) {
            SetClickCallback(Props.TrayCreateMiddleclickCallbackPointer, nameof(MiddleClickCallback), callback, userdata);
        }

        /// <inheritdoc cref="SetClickCallback"/>
        public void SetRightClickCallback(TrayClickCallback callback, object? userdata = null) {
            SetClickCallback(Props.TrayCreateRightclickCallbackPointer, nameof(RightClickCallback), callback, userdata);
        }

        /// <inheritdoc cref="SetClickCallback"/>
        public void SetDoubleClickCallback(TrayClickCallback callback, object? userdata = null) {
            SetClickCallback(DoubleclickCallbackPointer, nameof(DoubleClickCallback), callback, userdata);
        }

        /// <summary>
        ///     Installs a managed click callback: the delegate is rooted for as long as it stays
        ///     registered and its native function pointer is written to the matching property.
        /// </summary>
        /// <remarks>
        ///     SDL keeps a single userdata pointer per tray, shared by all four click callbacks, so
        ///     passing <paramref name="userdata"/> here changes what every installed callback receives.
        /// </remarks>
        private void SetClickCallback(string property, string slot, TrayClickCallback callback, object? userdata) {
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"TrayClick:{Handle}:{slot}";
            CallbackRegistry.Unregister<TrayClickCallback, SDL_TrayClickCallbackNative>(id);

            SDL_TrayClickCallbackNative native = TrayClickCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr _) cb = CallbackRegistry.Register(id, callback, native);

            PropPointer(property).Set(cb.functionPtr);
            _clickCallbackIds.Add(id);
            SetSharedUserdata(userdata);
        }

        // SDL stores one userdata pointer on the tray, not one per callback. Keep every pointer
        // issued to SDL alive until the tray is destroyed so an in-flight native callback is safe.
        private void SetSharedUserdata(object? userdata) {
            GCHandle handle = default;
            nint userdataPtr = nint.Zero;
            if (userdata is not null) {
                handle = GCHandle.Alloc(userdata);
                userdataPtr = GCHandle.ToIntPtr(handle);
            }

            if (!Userdata.Set(userdataPtr)) {
                if (handle.IsAllocated) {
                    handle.Free();
                }
                throw new SDLException(nameof(Userdata));
            }

            if (handle.IsAllocated) {
                _userdataHandles.Add(handle);
            }
        }

        internal (string[] CallbackIds, GCHandle[] UserdataHandles) TakeClickCallbackRegistrations() {
            string[] callbackIds = new string[_clickCallbackIds.Count];
            _clickCallbackIds.CopyTo(callbackIds);
            _clickCallbackIds.Clear();

            GCHandle[] userdataHandles = _userdataHandles.ToArray();
            _userdataHandles.Clear();
            return (callbackIds, userdataHandles);
        }

        public override void Dispose() {
            foreach (string id in _clickCallbackIds) {
                CallbackRegistry.Unregister<TrayClickCallback, SDL_TrayClickCallbackNative>(id);
            }
            _clickCallbackIds.Clear();

            foreach (GCHandle handle in _userdataHandles) {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
            _userdataHandles.Clear();
            base.Dispose();
        }
    }
}
