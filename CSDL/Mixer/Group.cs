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
    public readonly partial struct Group {
        internal Group(NativePtr<Opaque.SdlGroup> handle, Mixer owner)
            : this(handle, HandleKind.Owned, owner.AsOwner) { }

        /// <summary>
        /// The mixer that was passed to <see cref="Mixer.CreateGroup"/> to create this group. The
        /// returned wrapper is a borrowed handle - do not dispose it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetGroupMixer"/>
        /// <returns>A borrowed view of the owning mixer, or <see langword="default"/> if there is none.</returns>
        public Mixer Mixer {
            get {
                NativePtr<Opaque.SdlMixer> mixer = SDL.GetGroupMixer(Handle);
                if (mixer.IsNull) {
                    Error.LogError(nameof(SDL.GetGroupMixer));
                }
                return new Mixer(mixer, HandleKind.Borrowed);
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

        /// <summary>The <see cref="CallbackRegistry"/> key for this Group's post-mix callback.</summary>
        /// <remarks>
        ///     Derived from the native pointer rather than stored in a field: every copy of this value
        ///     has to mean the same registration, and <see cref="Dispose"/> needs the key after the
        ///     handle has already been retired.
        /// </remarks>
        private static string PostMixCallbackId(nint handle) {
            return $"GroupPostMix:{handle}";
        }

        /// <summary>Drops the post-mix callback registered for this Group, if any.</summary>
        private static void ReleaseCallbacks(nint handle) {
            CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(PostMixCallbackId(handle));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.SetGroupPostMixCallback"/>
        public bool SetPostMixCallback(GroupMixCallback callback, object? userData = null) {
            ArgumentNullException.ThrowIfNull(callback);

            nint handle = Handle.Ptr;
            string id = PostMixCallbackId(handle);

            // Replace rather than stack: one callback per Group, so the previous registration under
            // this key goes first - which also frees the userdata it pinned.
            CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(id);

            MIX_GroupMixCallbackNative native = GroupMixCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) reg = CallbackRegistry.Register(id, callback, native, userData);
            if (!SDL.SetGroupPostMixCallback(Handle, native, reg.userdataPtr).LogIfFalse()) {
                CallbackRegistry.Unregister<GroupMixCallback, MIX_GroupMixCallbackNative>(id);
                return false;
            }

            return true;
        }

        /// <summary>Removes the Group's post-mix callback.</summary>
        public bool ClearPostMixCallback() {
            nint handle = Handle.Ptr;
            if (!SDL.SetGroupPostMixCallback(Handle, null!, IntPtr.Zero).LogIfFalse()) {
                return false;
            }

            ReleaseCallbacks(handle);
            return true;
        }

    }
}
