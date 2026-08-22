// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Properties;

namespace CSDL {
    /// <summary>
    ///     The property set understood by <see cref="FileDialog.Show"/>.
    /// </summary>
    /// <seealso cref="CSDL.Internal.Docs.Dialog.ShowFileDialogWithProperties">SDL_ShowFileDialogWithProperties</seealso>
    public sealed class FileDialogProperties : PropertyGroup {
        private NativeFileFilters? _filters;

        /// <inheritdoc cref="CSDL.Props.FileDialogFiltersPointer"/>
        public PointerProperty Filters => PropPointer(Props.FileDialogFiltersPointer);

        /// <inheritdoc cref="CSDL.Props.FileDialogNfiltersNumber"/>
        public NumberProperty FilterCount => PropNumber(Props.FileDialogNfiltersNumber);

        /// <inheritdoc cref="CSDL.Props.FileDialogWindowPointer"/>
        public PointerProperty ParentWindow => PropPointer(Props.FileDialogWindowPointer);

        /// <inheritdoc cref="CSDL.Props.FileDialogLocationString"/>
        public StringProperty Location => PropString(Props.FileDialogLocationString);

        /// <inheritdoc cref="CSDL.Props.FileDialogManyBoolean"/>
        public BooleanProperty AllowMany => PropBool(Props.FileDialogManyBoolean);

        /// <inheritdoc cref="CSDL.Props.FileDialogTitleString"/>
        public StringProperty Title => PropString(Props.FileDialogTitleString);

        /// <inheritdoc cref="CSDL.Props.FileDialogAcceptString"/>
        public StringProperty AcceptLabel => PropString(Props.FileDialogAcceptString);

        /// <inheritdoc cref="CSDL.Props.FileDialogCancelString"/>
        public StringProperty CancelLabel => PropString(Props.FileDialogCancelString);

        /// <summary>Makes the dialog modal to <paramref name="window"/>.</summary>
        public void SetWindow(CSDL.Video.Window window) {
            ArgumentNullException.ThrowIfNull(window);
            ParentWindow.Set(window.NativePointer);
        }

        /// <summary>
        ///     Sets the file filters, allocating the native array SDL reads while the dialog is open.
        /// </summary>
        /// <param name="filters">the (name, pattern) filters to offer, e.g. <c>("PNG images", "png")</c>.</param>
        /// <remarks>
        ///     The allocation is owned by this group and released on <see cref="Dispose"/>, so keep the
        ///     group alive until the dialog callback has run.
        /// </remarks>
        public void SetFilters(IReadOnlyList<(string Name, string Pattern)> filters) {
            ArgumentNullException.ThrowIfNull(filters);

            _filters?.Dispose();
            NativeFileFilters native = NativeFileFilters.Allocate(filters);
            _filters = native;

            Filters.Set(native.Ptr);
            FilterCount.Set(native.Count);
        }

        /// <summary>
        ///     Destroys the property group and frees the filters allocated by <see cref="SetFilters"/>.
        /// </summary>
        public override void Dispose() {
            _filters?.Dispose();
            _filters = null;
            base.Dispose();
        }
    }
}
