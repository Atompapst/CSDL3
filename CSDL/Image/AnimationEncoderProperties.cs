// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Image {
    public class AnimationEncoderProperties : PropertyGroup {
        public AnimationEncoderProperties() { }

        /// <inheritdoc cref="Props.AnimationEncoderCreateFilenameString"/>
        public StringProperty FileName => PropString(Props.AnimationEncoderCreateFilenameString);

        /// <inheritdoc cref="Props.AnimationEncoderCreateIostreamPointer"/>
        public PointerProperty IOStream => PropPointer(Props.AnimationEncoderCreateIostreamPointer);

        /// <inheritdoc cref="Props.AnimationEncoderCreateIostreamAutocloseBoolean"/>
        public BooleanProperty IOStreamAutoClose => PropBool(Props.AnimationEncoderCreateIostreamAutocloseBoolean);

        /// <inheritdoc cref="Props.AnimationEncoderCreateTypeString"/>
        public StringProperty Type => PropString(Props.AnimationEncoderCreateTypeString);

        /// <inheritdoc cref="Props.AnimationEncoderCreateQualityNumber"/>
        public NumberProperty Quality => PropNumber(Props.AnimationEncoderCreateQualityNumber);

        /// <inheritdoc cref="Props.AnimationEncoderCreateTimebaseNumeratorNumber"/>
        public NumberProperty TimebaseNumerator => PropNumber(Props.AnimationEncoderCreateTimebaseNumeratorNumber);

        /// <inheritdoc cref="Props.AnimationEncoderCreateTimebaseDenominatorNumber"/>
        public NumberProperty TimebaseDenominator => PropNumber(Props.AnimationEncoderCreateTimebaseDenominatorNumber);

        /// <inheritdoc cref="Props.AnimationEncoderCreateAvifMaxThreadsNumber"/>
        public NumberProperty AvifMaxThreads => PropNumber(Props.AnimationEncoderCreateAvifMaxThreadsNumber);

        /// <inheritdoc cref="Props.AnimationEncoderCreateAvifKeyframeIntervalNumber"/>
        public NumberProperty AvifKeyFrameInterval => PropNumber(Props.AnimationEncoderCreateAvifKeyframeIntervalNumber);

        /// <inheritdoc cref="Props.AnimationEncoderCreateGifUseLutBoolean"/>
        public BooleanProperty GifUseLut => PropBool(Props.AnimationEncoderCreateGifUseLutBoolean);
    }
}
