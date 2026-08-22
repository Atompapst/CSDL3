// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.File;
using CSDL.Video;

namespace CSDL.TTF {
    /// <summary>
    /// A loaded TrueType/OpenType font, used to measure and render text to
    /// <see cref="Surface"/>s.
    /// </summary>
    public sealed class Font : NativeHandle<Opaque.SdlFont> {
        private IOStream? _streamClosedWithFont;

        static Font() {
            TTF.EnsureInitialized();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.OpenFont"/>
        public Font(string file, float pointSize) {
            Handle = SDL.OpenFont(file, pointSize).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.OpenFontIO"/>
        public Font(IOStream stream, float pointSize, bool closeStream = false) {
            Handle = SDL.OpenFontIO(stream.Handle, closeStream, pointSize).ThrowIfInvalid();
            _streamClosedWithFont = closeStream ? stream : null;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.OpenFontWithProperties"/>
        public Font(FontCreateProperties properties) {
            Handle = SDL.OpenFontWithProperties(properties.Handle).ThrowIfInvalid();
        }

        internal Font(NativePtr<Opaque.SdlFont> handle, bool ownsHandle = true) : base(handle, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontProperties"/>
        public FontProperties Properties => new FontProperties(GetProperties());

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CopyFont"/>
        public Font Copy() {
            NativePtr<Opaque.SdlFont> copy = SDL.CopyFont(Handle).ThrowIfInvalid();
            return new Font(copy);
        }

        /// <summary>
        /// Gets or sets the font's point size.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontSize"/>
        public float Size {
            get => SDL.GetFontSize(Handle);
            set => SDL.SetFontSize(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontHeight"/>
        public int Height => SDL.GetFontHeight(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontAscent"/>
        public int Ascent => SDL.GetFontAscent(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontDescent"/>
        public int Descent => SDL.GetFontDescent(Handle);

        /// <summary>
        /// Gets or sets the spacing between lines of text for the font.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontLineSkip"/>
        public int LineSkip {
            get => SDL.GetFontLineSkip(Handle);
            set => SDL.SetFontLineSkip(Handle, value);
        }

        /// <summary>
        /// Gets or sets the spacing between individual characters of the font.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontCharSpacing"/>
        public int CharSpacing {
            get => SDL.GetFontCharSpacing(Handle);
            set => SDL.SetFontCharSpacing(Handle, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the font's current style (bold, italic, underline, strikethrough).
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontStyle"/>
        public FontStyleFlags Style {
            get => SDL.GetFontStyle(Handle);
            set => SDL.SetFontStyle(Handle, value);
        }

        /// <summary>
        /// Gets the font's family name (e.g. "Arial"), or <see langword="null"/> if unavailable.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontFamilyName"/>
        public string? FamilyName => SDL.GetFontFamilyName(Handle).ToUtf8String();

        /// <summary>
        /// Gets the font's current style name (e.g. "Bold Italic"), or <see langword="null"/> if unavailable.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontStyleName"/>
        public string? StyleName => SDL.GetFontStyleName(Handle).ToUtf8String();

        /// <summary>
        /// Gets or sets the font's outline thickness in pixels. Zero disables outlining.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontOutline"/>
        public int Outline {
            get => SDL.GetFontOutline(Handle);
            set => SDL.SetFontOutline(Handle, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the font's hinting mode.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontHinting"/>
        public HintingFlags Hinting {
            get => SDL.GetFontHinting(Handle);
            set => SDL.SetFontHinting(Handle, value);
        }

        /// <summary>
        /// Gets or sets whether kerning is enabled for the font.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontKerning"/>
        public bool Kerning {
            get => SDL.GetFontKerning(Handle);
            set => SDL.SetFontKerning(Handle, value);
        }

        /// <summary>
        /// Gets or sets the horizontal alignment used when wrapping text.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontWrapAlignment"/>
        public HorizontalAlignment WrapAlignment {
            get => SDL.GetFontWrapAlignment(Handle);
            set => SDL.SetFontWrapAlignment(Handle, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.FontIsFixedWidth"/>
        public bool IsFixedWidth => SDL.FontIsFixedWidth(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.FontIsScalable"/>
        public bool IsScalable => SDL.FontIsScalable(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetNumFontFaces"/>
        public int NumFaces => SDL.GetNumFontFaces(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.FontHasGlyph"/>
        public bool HasGlyph(uint codepoint) {
            return SDL.FontHasGlyph(Handle, codepoint);
        }

        /// <summary>
        /// Gets or sets the direction to be used for text shaping by the font.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontDirection"/>
        public Direction Direction {
            get => SDL.GetFontDirection(Handle);
            set => SDL.SetFontDirection(Handle, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the script used for text shaping by the font, as an
        /// <a href="https://unicode.org/iso15924/iso15924-codes.html">ISO 15924 code</a>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontScript"/>
        public uint Script {
            get => SDL.GetFontScript(Handle);
            set => SDL.SetFontScript(Handle, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets whether Signed Distance Field rendering is enabled for the font.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontSDF"/>
        public bool SDFEnabled {
            get => SDL.GetFontSDF(Handle);
            set => SDL.SetFontSDF(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontWeight"/>
        public int Weight => SDL.GetFontWeight(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontGeneration"/>
        public uint Generation => SDL.GetFontGeneration(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetFontLanguage"/>
        public bool SetLanguage(string? languageBcp47) {
            return SDL.SetFontLanguage(Handle, languageBcp47).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontDPI"/>
        public bool GetDPI(out int hdpi, out int vdpi) {
            return SDL.GetFontDPI(Handle, out hdpi, out vdpi).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetFontSizeDPI"/>
        public bool SetSizeDPI(float pointSize, int hdpi, int vdpi) {
            return SDL.SetFontSizeDPI(Handle, pointSize, hdpi, vdpi).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.AddFallbackFont"/>
        public bool AddFallbackFont(Font fallback) {
            return SDL.AddFallbackFont(Handle, fallback.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RemoveFallbackFont"/>
        public void RemoveFallbackFont(Font fallback) {
            SDL.RemoveFallbackFont(Handle, fallback.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.ClearFallbackFonts"/>
        public void ClearFallbackFonts() {
            SDL.ClearFallbackFonts(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGlyphKerning"/>
        public bool GetGlyphKerning(uint previousCodepoint, uint codepoint, out int kerning) {
            return SDL.GetGlyphKerning(Handle, previousCodepoint, codepoint, out kerning).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGlyphMetrics"/>
        public bool GetGlyphMetrics(uint codepoint, out int minX, out int maxX, out int minY, out int maxY, out int advance) {
            return SDL.GetGlyphMetrics(Handle, codepoint, out minX, out maxX, out minY, out maxY, out advance).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGlyphScript"/>
        public static uint GetGlyphScript(uint codepoint) {
            return SDL.GetGlyphScript(codepoint);
        }

        /// <summary>
        /// Measures the pixel size a rendered (unwrapped) string of text would occupy, without
        /// rendering it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetStringSize"/>
        public bool GetStringSize(string text, out int width, out int height) {
            return SDL.GetStringSize(Handle, text, 0, out width, out height).LogIfFalse();
        }

        /// <summary>
        /// Measures the pixel size a word-wrapped string of text would occupy, without rendering it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetStringSizeWrapped"/>
        public bool GetStringSizeWrapped(string text, int wrapWidth, out int width, out int height) {
            return SDL.GetStringSizeWrapped(Handle, text, 0, wrapWidth, out width, out height).LogIfFalse();
        }

        /// <summary>
        /// Measures how much of <paramref name="text"/> fits within <paramref name="maxWidth"/>
        /// pixels, without rendering it.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.MeasureString"/>
        public bool MeasureString(string text, int maxWidth, out int measuredWidth, out int measuredLength) {
            bool ok = SDL.MeasureString(Handle, text, 0, maxWidth, out measuredWidth, out nuint length).LogIfFalse();
            measuredLength = (int)length;
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_Solid"/>
        public Surface RenderTextSolid(string text, Color fg) {
            return WrapSurface(SDL.RenderText_Solid(Handle, text, 0, fg), nameof(RenderTextSolid));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_Solid_Wrapped"/>
        public Surface RenderTextSolidWrapped(string text, Color fg, int wrapWidth) {
            return WrapSurface(SDL.RenderText_Solid_Wrapped(Handle, text, 0, fg, wrapWidth), nameof(RenderTextSolidWrapped));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_Shaded"/>
        public Surface RenderTextShaded(string text, Color fg, Color bg) {
            return WrapSurface(SDL.RenderText_Shaded(Handle, text, 0, fg, bg), nameof(RenderTextShaded));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_Shaded_Wrapped"/>
        public Surface RenderTextShadedWrapped(string text, Color fg, Color bg, int wrapWidth) {
            return WrapSurface(SDL.RenderText_Shaded_Wrapped(Handle, text, 0, fg, bg, wrapWidth), nameof(RenderTextShadedWrapped));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_Blended"/>
        public Surface RenderTextBlended(string text, Color fg) {
            return WrapSurface(SDL.RenderText_Blended(Handle, text, 0, fg), nameof(RenderTextBlended));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_Blended_Wrapped"/>
        public Surface RenderTextBlendedWrapped(string text, Color fg, int wrapWidth) {
            return WrapSurface(SDL.RenderText_Blended_Wrapped(Handle, text, 0, fg, wrapWidth), nameof(RenderTextBlendedWrapped));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_LCD"/>
        public Surface RenderTextLCD(string text, Color fg, Color bg) {
            return WrapSurface(SDL.RenderText_LCD(Handle, text, 0, fg, bg), nameof(RenderTextLCD));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderText_LCD_Wrapped"/>
        public Surface RenderTextLCDWrapped(string text, Color fg, Color bg, int wrapWidth) {
            return WrapSurface(SDL.RenderText_LCD_Wrapped(Handle, text, 0, fg, bg, wrapWidth), nameof(RenderTextLCDWrapped));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderGlyph_Solid"/>
        public Surface RenderGlyphSolid(uint codepoint, Color fg) {
            return WrapSurface(SDL.RenderGlyph_Solid(Handle, codepoint, fg), nameof(RenderGlyphSolid));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderGlyph_Shaded"/>
        public Surface RenderGlyphShaded(uint codepoint, Color fg, Color bg) {
            return WrapSurface(SDL.RenderGlyph_Shaded(Handle, codepoint, fg, bg), nameof(RenderGlyphShaded));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderGlyph_Blended"/>
        public Surface RenderGlyphBlended(uint codepoint, Color fg) {
            return WrapSurface(SDL.RenderGlyph_Blended(Handle, codepoint, fg), nameof(RenderGlyphBlended));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.RenderGlyph_LCD"/>
        public Surface RenderGlyphLCD(uint codepoint, Color fg, Color bg) {
            return WrapSurface(SDL.RenderGlyph_LCD(Handle, codepoint, fg, bg), nameof(RenderGlyphLCD));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGlyphImage"/>
        public Surface GetGlyphImage(uint codepoint, out ImageType imageType) {
            return WrapSurface(SDL.GetGlyphImage(Handle, codepoint, out imageType), nameof(GetGlyphImage));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGlyphImageForIndex"/>
        public Surface GetGlyphImageForIndex(uint glyphIndex, out ImageType imageType) {
            return WrapSurface(SDL.GetGlyphImageForIndex(Handle, glyphIndex, out imageType), nameof(GetGlyphImageForIndex));
        }

        private static Surface WrapSurface(NativePtr<SurfaceData> surface, string operation) {
            return new Surface(surface.ThrowIfInvalid(operation), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFontProperties"/>
        private uint GetProperties() {
            PropertiesID id = SDL.GetFontProperties(Handle);
            if (id.Value == 0) {
                Error.LogError(nameof(GetProperties));
            }
            return id.Value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CloseFont"/>
        protected override void DisposeResource() {
            try {
                SDL.CloseFont(Handle);
            } finally {
                // SDL_ttf closes a closeStream source together with the font, so its wrapper must
                // stop exposing the now-invalid native handle.
                _streamClosedWithFont?.Invalidate();
                _streamClosedWithFont = null;
            }
        }
    }
}
