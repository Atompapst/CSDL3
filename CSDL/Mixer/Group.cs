// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// An optional mixing bucket: <see cref="Track"/>s assigned to a group (see
    /// <see cref="Track.SetGroup"/>) are mixed together first, letting the app inspect that combined
    /// data via <see cref="SetPostMixCallback"/> before it joins the rest of a <see cref="Mixer"/>'s
    /// final mix.
    /// </summary>
    public sealed class Group : NativeHandle<Opaque.SdlGroup> {
        private readonly object _callbackLock = new object();
        private string? _postMixCallbackId;

        internal Group(NativePtr<Opaque.SdlGroup> handle, Mixer owner) : base(handle, true) {
            owner.RegisterChild(Invalidation);
        }

        /// <summary>
        /// The mixer that was passed to <see cref="Mixer.CreateGroup"/> to create this group. The
        /// returned wrapper is a borrowed handle - do not dispose it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetGroupMixer"/>
        public Mixer? Mixer {
            get {
                NativePtr<Opaque.SdlMixer> mixer = SDL.GetGroupMixer(Handle);
                if (mixer.IsNull) {
                    Error.LogError(nameof(SDL.GetGroupMixer));
                    return null;
                }
                return new Mixer(mixer, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetGroupProperties"/>
        public GroupProperties? Properties {
            get {
                uint id = SDL.GetGroupProperties(Handle);
                if (id == 0) {
                    Error.LogError(nameof(SDL.GetGroupProperties));
                    return null;
                }
                return new GroupProperties(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetGroupPostMixCallback"/>
        public bool SetPostMixCallback(GroupMixCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);
            string id = $"GroupPostMix:{Guid.NewGuid()}";
            MIX_GroupMixCallbackNative native = GroupMixCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(id, callback, native, userData);
            lock (_callbackLock) {
                bool ok = SDL.SetGroupPostMixCallback(Handle, native, reg.userdataPtr).LogIfFalse();
                if (!ok) {
                    CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(id);
                    return false;
                }
                if (_postMixCallbackId is not null) {
                    CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(_postMixCallbackId);
                }
                _postMixCallbackId = id;
                return true;
            }
        }

        /// <summary>Removes the group's post-mix callback.</summary>
        public bool ClearPostMixCallback() {
            lock (_callbackLock) {
                bool ok = SDL.SetGroupPostMixCallback(Handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok && _postMixCallbackId is not null) {
                    CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(_postMixCallbackId);
                    _postMixCallbackId = null;
                }
                return ok;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyGroup"/>
        protected override void DisposeResource() {
            ClearPostMixCallback();
            SDL.DestroyGroup(Handle);
        }

        protected override void InvalidateResource() {
            if (_postMixCallbackId is not null) {
                CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(_postMixCallbackId);
                _postMixCallbackId = null;
            }
        }
    }
}
