// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Image {
    public class Metadata : PropertyGroup {
        internal Metadata(uint handle) : base(handle) { }

        /// <inheritdoc cref="CSDL.Image.Props.MetadataIgnorePropsBoolean"/>
        public BooleanProperty IgnoreProps => PropBool(Props.MetadataIgnorePropsBoolean);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataDescriptionString"/>
        public StringProperty Description => PropString(Props.MetadataDescriptionString);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataCopyrightString"/>
        public StringProperty Copyright => PropString(Props.MetadataCopyrightString);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataTitleString"/>
        public StringProperty Title => PropString(Props.MetadataTitleString);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataAuthorString"/>
        public StringProperty Author => PropString(Props.MetadataAuthorString);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataCreationTimeString"/>
        public StringProperty CreationTime => PropString(Props.MetadataCreationTimeString);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataFrameCountNumber"/>
        public NumberProperty FrameCount => PropNumber(Props.MetadataFrameCountNumber);
        /// <inheritdoc cref="CSDL.Image.Props.MetadataLoopCountNumber"/>
        public NumberProperty LoopCount => PropNumber(Props.MetadataLoopCountNumber);
    }
}
