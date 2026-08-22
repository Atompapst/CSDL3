// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Image {
    public class AnimationDecoderProperties : PropertyGroup {
        public AnimationDecoderProperties() { }

        /// <inheritdoc cref="Props.AnimationDecoderCreateFilenameString"/>
        public StringProperty FileName => PropString(Props.AnimationDecoderCreateFilenameString);

        /// <inheritdoc cref="Props.AnimationDecoderCreateIostreamPointer"/>
        public PointerProperty IOStream => PropPointer(Props.AnimationDecoderCreateIostreamPointer);

        /// <inheritdoc cref="Props.AnimationDecoderCreateIostreamAutocloseBoolean"/>
        public BooleanProperty IOStreamAutoClose => PropBool(Props.AnimationDecoderCreateIostreamAutocloseBoolean);

        /// <inheritdoc cref="Props.AnimationDecoderCreateTypeString"/>
        public StringProperty Type => PropString(Props.AnimationDecoderCreateTypeString);

        /// <inheritdoc cref="Props.AnimationDecoderCreateTimebaseNumeratorNumber"/>
        public NumberProperty Numerator => PropNumber(Props.AnimationDecoderCreateTimebaseNumeratorNumber);

        /// <inheritdoc cref="Props.AnimationDecoderCreateTimebaseDenominatorNumber"/>
        public NumberProperty Denominator => PropNumber(Props.AnimationDecoderCreateTimebaseDenominatorNumber);

        /// <inheritdoc cref="Props.AnimationDecoderCreateAvifMaxThreadsNumber"/>
        public NumberProperty AvifMaxThreads => PropNumber(Props.AnimationDecoderCreateAvifMaxThreadsNumber);

        /// <inheritdoc cref="Props.AnimationDecoderCreateAvifAllowIncrementalBoolean"/>
        public BooleanProperty AvifAllowIncremental => PropBool(Props.AnimationDecoderCreateAvifAllowIncrementalBoolean);

        /// <inheritdoc cref="Props.AnimationDecoderCreateAvifAllowProgressiveBoolean"/>
        public BooleanProperty AvifAllowProgressive => PropBool(Props.AnimationDecoderCreateAvifAllowProgressiveBoolean);

        /// <inheritdoc cref="Props.AnimationDecoderCreateGifTransparentColorIndexNumber"/>
        public NumberProperty GifTransparentColorIndex => PropNumber(Props.AnimationDecoderCreateGifTransparentColorIndexNumber);

        /// <inheritdoc cref="Props.AnimationDecoderCreateGifNumColorsNumber"/>
        public NumberProperty GifNumColors => PropNumber(Props.AnimationDecoderCreateGifNumColorsNumber);
    }
}
