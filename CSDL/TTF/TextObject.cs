// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL.TTF {
    /// <summary>
    /// A piece of UTF-8 text laid out with a <see cref="Font"/> through a <see cref="TextEngine"/>.
    /// </summary>
    /// <remarks>
    /// Wraps <c>TTF_Text</c>. The struct is named <see cref="Text"/> in this binding, so the
    /// wrapper class is named <see cref="TextObject"/> to avoid the collision.
    /// </remarks>
    public sealed partial class TextObject : NativeHandle<Text> {
        // The engine and font a text object runs on are plain pointers on the native side, and
        // neither TextEngine nor Font can be reconstructed from one - TextEngine is abstract, and a
        // Font wrapper built from a foreign handle would not know who owns it. So the instances
        // handed in here are kept around and returned by Engine and Font.
        private TextEngine? _engine;
        private Font? _font;

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateText"/>
        public TextObject(TextEngine? engine, Font font, string text) {
            Handle = SDL.CreateText(engine?.Handle ?? default, font.Handle, text, 0).ThrowIfInvalid();
            _engine = engine;
            _font = font;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextProperties"/>
        public TextProperties Properties => new TextProperties(GetProperties());

        /// <summary>
        /// Gets the <see cref="TextEngine"/> this text object is laid out with, or
        /// <see langword="null"/> if it has none - or if it runs on an engine this wrapper was never
        /// handed (see the remarks on <see cref="SetEngine"/>).
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextEngine"/>
        public TextEngine? Engine {
            get {
                NativePtr<Opaque.SdlTextEngine> engine = SDL.GetTextEngine(ref Ref).LogIfInvalid(nameof(Engine));
                return !engine.IsNull && _engine != null && _engine.Handle.Ptr == engine.Ptr ? _engine : null;
            }
        }

        /// <summary>
        /// Gets the <see cref="TTF.Font"/> this text object is laid out with, or
        /// <see langword="null"/> if it has none.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextFont"/>
        public Font? Font {
            get {
                NativePtr<Opaque.SdlFont> font = SDL.GetTextFont(ref Ref).LogIfInvalid(nameof(Font));
                if (font.IsNull) return null;
                if (_font != null && _font.Handle.Ptr == font.Ptr) return _font;

                // A handle this wrapper has not been handed - the font was set through some other
                // path. Whoever created it owns it, so this view never does.
                return new CSDL.TTF.Font(font, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.AppendTextString"/>
        public bool AppendString(string text) {
            return SDL.AppendTextString(ref Ref, text, 0).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.InsertTextString"/>
        public bool InsertString(int offset, string text) {
            return SDL.InsertTextString(ref Ref, offset, text, 0).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DeleteTextString"/>
        public bool DeleteString(int offset, int length) {
            return SDL.DeleteTextString(ref Ref, offset, length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetTextString"/>
        public bool SetString(string? text) {
            return SDL.SetTextString(ref Ref, text, 0).LogIfFalse();
        }

        /// <remarks>
        /// The engine passed in here is remembered, so <see cref="Engine"/> can hand it back.
        /// </remarks>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetTextEngine"/>
        public bool SetEngine(TextEngine engine) {
            if (!SDL.SetTextEngine(ref Ref, engine.Handle).LogIfFalse()) return false;
            _engine = engine;
            return true;
        }

        /// <remarks>
        /// The font passed in here is remembered, so <see cref="Font"/> can hand it back.
        /// </remarks>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetTextFont"/>
        public bool SetFont(Font? font) {
            if (!SDL.SetTextFont(ref Ref, font?.Handle ?? default).LogIfFalse()) return false;
            _font = font;
            return true;
        }

        /// <summary>
        /// Gets or sets the direction to be used for text shaping.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextDirection"/>
        public Direction Direction {
            get => SDL.GetTextDirection(ref Ref);
            set => SDL.SetTextDirection(ref Ref, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the script used for text shaping, as an
        /// <a href="https://unicode.org/iso15924/iso15924-codes.html">ISO 15924 code</a>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextScript"/>
        public uint Script {
            get => SDL.GetTextScript(ref Ref);
            set => SDL.SetTextScript(ref Ref, value).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextColor"/>
        public Color Color {
            get {
                SDL.GetTextColor(ref Ref, out byte r, out byte g, out byte b, out byte a).LogIfFalse();
                return new Color(r, g, b, a);
            }
            set => SDL.SetTextColor(ref Ref, value.R, value.G, value.B, value.A).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets the color of the text, as normalized floating-point components.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextColorFloat"/>
        public FColor FloatColor {
            get {
                SDL.GetTextColorFloat(ref Ref, out float r, out float g, out float b, out float a).LogIfFalse();
                return new FColor(r, g, b, a);
            }
            set => SDL.SetTextColorFloat(ref Ref, value.R, value.G, value.B, value.A).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextPosition"/>
        public bool GetPosition(out int x, out int y) {
            return SDL.GetTextPosition(ref Ref, out x, out y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetTextPosition"/>
        public bool SetPosition(int x, int y) {
            return SDL.SetTextPosition(ref Ref, x, y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextSize"/>
        public bool GetSize(out int width, out int height) {
            return SDL.GetTextSize(ref Ref, out width, out height).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextWrapWidth"/>
        public bool GetWrapWidth(out int wrapWidth) {
            return SDL.GetTextWrapWidth(ref Ref, out wrapWidth).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.SetTextWrapWidth"/>
        public bool SetWrapWidth(int wrapWidth) {
            return SDL.SetTextWrapWidth(ref Ref, wrapWidth).LogIfFalse();
        }

        /// <summary>
        /// Gets or sets whether whitespace is visible - and takes up space - when wrapping.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.TextWrapWhitespaceVisible"/>
        public bool WrapWhitespaceVisible {
            get => SDL.TextWrapWhitespaceVisible(ref Ref);
            set => SDL.SetTextWrapWhitespaceVisible(ref Ref, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.UpdateText"/>
        public bool Update() {
            return SDL.UpdateText(ref Ref).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DrawRendererText"/>
        public bool DrawViaRenderer(float x, float y) {
            return SDL.DrawRendererText(ref Ref, x, y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DrawSurfaceText"/>
        public bool DrawToSurface(int x, int y, Surface surface) {
            return SDL.DrawSurfaceText(ref Ref, x, y, surface.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetNextTextSubString"/>
        public bool GetNextSubString(in SubString substring, out SubString next) {
            return SDL.GetNextTextSubString(ref Ref, in substring, out next).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetPreviousTextSubString"/>
        public bool GetPreviousSubString(in SubString substring, out SubString previous) {
            return SDL.GetPreviousTextSubString(ref Ref, in substring, out previous).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextSubString"/>
        public bool GetSubString(int offset, out SubString substring) {
            return SDL.GetTextSubString(ref Ref, offset, out substring).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextSubStringForLine"/>
        public bool GetSubStringForLine(int line, out SubString substring) {
            return SDL.GetTextSubStringForLine(ref Ref, line, out substring).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextSubStringForPoint"/>
        public bool GetSubStringForPoint(int x, int y, out SubString substring) {
            return SDL.GetTextSubStringForPoint(ref Ref, x, y, out substring).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextSubStringsForRange"/>
        public SubString[] GetSubStringsForRange(int offset, int length) {
            IntPtr array = SDL.GetTextSubStringsForRange(ref Ref, offset, length, out int count);
            if (array == IntPtr.Zero) {
                Error.LogError(nameof(GetSubStringsForRange));
                return Array.Empty<SubString>();
            }

            NativePtr<IntPtr> pointers = array;
            SubString[] result = new SubString[count];
            for (int i = 0; i < count; i++) {
                result[i] = new NativePtr<SubString>(pointers[i]).Read();
            }

            Memory.Free(array);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetTextProperties"/>
        private uint GetProperties() {
            PropertiesID id = SDL.GetTextProperties(ref Ref);
            if (id.Value == 0) {
                Error.LogError(nameof(GetProperties));
            }
            return id.Value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DestroyText"/>
        protected override void DisposeResource() {
            SDL.DestroyText(ref Ref);
        }
    }
}
