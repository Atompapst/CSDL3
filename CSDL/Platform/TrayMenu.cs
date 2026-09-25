// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    /// <summary>
    ///     A menu (or submenu) belonging to a <see cref="Tray"/>.
    /// </summary>
    /// <remarks>
    ///     Unlike most wrappers here this is not an <see cref="IDisposable"/> handle: SDL has no
    ///     "destroy menu" call - a menu lives and dies with the <see cref="Tray"/> (or the
    ///     <see cref="TrayEntry"/>) it was created from.
    /// </remarks>
    public sealed class TrayMenu {
        internal TrayMenu(NativePtr<Opaque.SdlTrayMenu> handle) {
            Handle = handle;
        }

        private NativePtr<Opaque.SdlTrayMenu> Handle { get; }

        /// <summary>
        ///     The tray this menu ultimately hangs off, walking up through any submenus.
        /// </summary>
        /// <remarks>
        ///     SDL destroys a tray's whole menu tree with the tray, so every entry created here records
        ///     that tray as its owner. Then a destroyed tray makes every entry handle below it
        ///     stale at once, without this class having kept a list of them.
        /// </remarks>
        private OwnerId OwningTray {
            get {
                NativePtr<Opaque.SdlTrayMenu> menu = Handle;
                for (int depth = 0; !menu.IsNull && depth < 16; depth++) {
                    NativePtr<Opaque.SdlTray> tray = SDL.GetTrayMenuParentTray(menu);
                    if (!tray.IsNull) {
                        return new Tray(tray, HandleKind.Borrowed).AsOwner;
                    }

                    NativePtr<Opaque.SdlTrayEntry> entry = SDL.GetTrayMenuParentEntry(menu);
                    if (entry.IsNull) break;
                    menu = SDL.GetTrayEntryParent(entry);
                }

                return OwnerId.None;
            }
        }

        /// <summary>The raw native pointer backing this menu.</summary>
        public nint NativePointer => Handle.Ptr;

        /// <summary>
        ///     Whether SDL handed out a menu at all.
        /// </summary>
        public bool IsValid => !Handle.IsNull;

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.InsertTrayEntryAt"/>
        /// <param name="pos">the index to insert at; -1 appends to the end.</param>
        /// <param name="label">the entry's label, or <see langword="null"/> for a separator.</param>
        /// <param name="flags">what kind of entry this is.</param>
        public TrayEntry Insert(int pos, string? label, TrayEntryFlags flags = TrayEntryFlags.Button) {
            NativePtr<Opaque.SdlTrayEntry> entry = SDL.InsertTrayEntryAt(Handle, pos, label, flags)
                .ThrowIfInvalid(nameof(SDL.InsertTrayEntryAt));
            return new TrayEntry(entry, HandleKind.Owned, OwningTray);
        }

        /// <summary>Appends an entry to the end of this menu.</summary>
        /// <seealso cref="Insert"/>
        public TrayEntry Add(string? label, TrayEntryFlags flags = TrayEntryFlags.Button) {
            return Insert(-1, label, flags);
        }

        /// <summary>Appends a separator (an entry without a label) to the end of this menu.</summary>
        /// <seealso cref="Insert"/>
        public TrayEntry AddSeparator() {
            return Insert(-1, null);
        }

        /// <summary>Appends a checkbox entry to the end of this menu.</summary>
        /// <seealso cref="Insert"/>
        public TrayEntry AddCheckbox(string label, bool @checked = false) {
            TrayEntryFlags flags = TrayEntryFlags.Checkbox;
            if (@checked) {
                flags |= TrayEntryFlags.Checked;
            }
            return Insert(-1, label, flags);
        }

        /// <summary>Appends a submenu entry to the end of this menu and creates its submenu.</summary>
        /// <seealso cref="Insert"/>
        /// <seealso cref="TrayEntry.CreateSubmenu"/>
        public TrayMenu AddSubmenu(string label) {
            TrayEntry entry = Insert(-1, label, TrayEntryFlags.Submenu);
            return entry.CreateSubmenu();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.GetTrayEntries"/>
        /// <remarks>
        ///     The returned entries are views onto SDL's own list - disposing one of them removes the
        ///     entry from the menu, so treat them as borrowed unless that is what you want.
        /// </remarks>
        public TrayEntry[] Entries {
            get {
                int count = 0;
                nint entries = SDL.GetTrayEntries(Handle, NativePtr<int>.FromRef(ref count));
                if (entries == nint.Zero || count <= 0) {
                    return Array.Empty<TrayEntry>();
                }

                NativePtr<nint> list = entries;
                TrayEntry[] result = new TrayEntry[count];
                for (int i = 0; i < count; i++) {
                    result[i] = new TrayEntry(list[i], HandleKind.Borrowed, OwningTray);
                }
                return result;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.GetTrayMenuParentTray"/>
        /// <value>The tray this menu belongs to, or <see langword="default"/> if it is a submenu.</value>
        public Tray ParentTray => new Tray(SDL.GetTrayMenuParentTray(Handle), HandleKind.Borrowed);

        /// <inheritdoc cref="CSDL.Internal.Docs.Tray.GetTrayMenuParentEntry"/>
        /// <value>The entry this submenu hangs off, or <see langword="default"/> if it is a tray's top-level menu.</value>
        public TrayEntry ParentEntry =>
            new TrayEntry(SDL.GetTrayMenuParentEntry(Handle), HandleKind.Borrowed, OwningTray);
    }
}
