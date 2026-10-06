// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Input {
    /// <seealso cref="CSDL.Input.Keyboards.StartTextInputWithProperties">StartTextInputWithProperties</seealso>
    public sealed class TextInputProperties : PropertyGroup {
        /// <inheritdoc cref="CSDL.Props.TextinputTypeNumber"/>
        /// <remarks>Holds a <see cref="TextInputType"/>.</remarks>
        public NumberProperty Type => PropNumber(Props.TextinputTypeNumber);

        /// <inheritdoc cref="CSDL.Props.TextinputCapitalizationNumber"/>
        /// <remarks>Holds a <see cref="Capitalization"/>.</remarks>
        public NumberProperty Capitalization => PropNumber(Props.TextinputCapitalizationNumber);

        /// <inheritdoc cref="CSDL.Props.TextinputAutocorrectBoolean"/>
        public BooleanProperty Autocorrect => PropBool(Props.TextinputAutocorrectBoolean);

        /// <inheritdoc cref="CSDL.Props.TextinputMultilineBoolean"/>
        public BooleanProperty Multiline => PropBool(Props.TextinputMultilineBoolean);

        /// <inheritdoc cref="CSDL.Props.TextinputMaxLengthNumber"/>
        public NumberProperty MaxLength => PropNumber(Props.TextinputMaxLengthNumber);

        /// <inheritdoc cref="CSDL.Props.TextinputTitleString"/>
        public StringProperty Title => PropString(Props.TextinputTitleString);

        /// <inheritdoc cref="CSDL.Props.TextinputPlaceholderString"/>
        public StringProperty Placeholder => PropString(Props.TextinputPlaceholderString);

        /// <inheritdoc cref="CSDL.Props.TextinputDefaultTextString"/>
        public StringProperty DefaultText => PropString(Props.TextinputDefaultTextString);

        /// <inheritdoc cref="CSDL.Props.TextinputAndroidInputtypeNumber"/>
        public NumberProperty AndroidInputType => PropNumber(Props.TextinputAndroidInputtypeNumber);

        /// <inheritdoc cref="CSDL.Props.TextinputOpenharmonyInputtypeNumber"/>
        public NumberProperty OpenHarmonyInputType => PropNumber(Props.TextinputOpenharmonyInputtypeNumber);
    }
}
