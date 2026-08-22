// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;

namespace CSDL.TTF {
    /// <summary>
    /// The property set an application fills in and hands to <see cref="Font(FontCreateProperties)"/>.
    /// </summary>
    /// <remarks>
    /// One of <see cref="FileName"/>, <see cref="IOStream"/> or <see cref="ExistingFont"/> has to be
    /// set - everything else is optional.
    /// </remarks>
    public sealed class FontCreateProperties : PropertyGroup {
        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateFilenameString"/>
        public StringProperty FileName => PropString(Props.FontCreateFilenameString);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateIostreamPointer"/>
        public PointerProperty IOStream => PropPointer(Props.FontCreateIostreamPointer);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateIostreamOffsetNumber"/>
        public NumberProperty IOStreamOffset => PropNumber(Props.FontCreateIostreamOffsetNumber);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateIostreamAutocloseBoolean"/>
        public BooleanProperty IOStreamAutoClose => PropBool(Props.FontCreateIostreamAutocloseBoolean);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateSizeFloat"/>
        public FloatProperty Size => PropFloat(Props.FontCreateSizeFloat);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateFaceNumber"/>
        public NumberProperty Face => PropNumber(Props.FontCreateFaceNumber);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateHorizontalDpiNumber"/>
        public NumberProperty HorizontalDPI => PropNumber(Props.FontCreateHorizontalDpiNumber);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateVerticalDpiNumber"/>
        public NumberProperty VerticalDPI => PropNumber(Props.FontCreateVerticalDpiNumber);

        /// <inheritdoc cref="CSDL.TTF.Props.FontCreateExistingFontPointer"/>
        public PointerProperty ExistingFont => PropPointer(Props.FontCreateExistingFontPointer);
    }
}
