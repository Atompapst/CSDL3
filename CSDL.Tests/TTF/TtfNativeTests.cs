// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.IO;
using CSDL.File;
using CSDL.TTF;
using CSDL.Video;
using CSDL3.Tests.TestSupport;
using TtfApi = CSDL.TTF.TTF;

namespace CSDL3.Tests.TTF {
    /// <summary>
     /// Proves the configured SDL3_ttf runtime works: it loads together with
    /// the FreeType and HarfBuzz it was built against, opens a real font file, and rasterises
    /// glyphs into an SDL surface. The test face is shipped with the repository so these checks do
    /// not depend on the host's installed fonts.
    /// </summary>
    [Collection(SdlCollection.Name)]
    public class TtfNativeTests {
        private const float PointSize = 24f;
        private static readonly string TestFontPath = System.IO.Path.Combine(
            AppContext.BaseDirectory, "Assets", "TTF", "pixelbasel.medium.ttf");

        [Fact]
        public void Version_FromNativeLibrary_ReportsAnSdlTtf3Runtime() {
            SdlVersionNumber version = new SdlVersionNumber(TtfApi.Version);

            Assert.Equal(3, version.Major);
            Assert.Equal((int)TtfApi.MajorVersion, version.Major);
        }

        [Fact]
        public void GetFreeTypeVersion_FromNativeLibrary_ReportsTheLinkedFreeType() {
            // SDL3_ttf statically links FreeType, so a plausible answer here proves the whole
            // native bundle - not just the SDL3_ttf shim - actually loaded.
            TtfApi.GetFreeTypeVersion(out int major, out int minor, out int patch);

            Assert.True(major >= 2, $"implausible FreeType version {major}.{minor}.{patch}");
            Assert.True(minor >= 0);
            Assert.True(patch >= 0);
        }

        [Fact]
        public void GetHarfBuzzVersion_FromNativeLibrary_ReportsEitherAVersionOrAnHonestZero() {
            // HarfBuzz is optional in an SDL3_ttf build. All zeroes means "not linked", which is a
            // valid answer; anything else has to be a sane version triple.
            TtfApi.GetHarfBuzzVersion(out int major, out int minor, out int patch);

            bool absent = major == 0 && minor == 0 && patch == 0;
            Assert.True(absent || major >= 1, $"implausible HarfBuzz version {major}.{minor}.{patch}");
        }

        [Theory]
        [InlineData("kern")]
        [InlineData("liga")]
        [InlineData("ss01")]
        public void StringToTag_ThenTagToString_RoundTripsThroughTheNativeTagEncoding(string tag) {
            // Needs no font, so it isolates the marshalling of the fixed-size native char buffer.
            uint encoded = TtfApi.StringToTag(tag);

            Assert.NotEqual(0u, encoded);
            Assert.Equal(tag, TtfApi.TagToString(encoded));
        }

        [Fact]
        public void OpenFont_WithTheBundledFont_ExposesMetricsAndMetadataFromTheNativeFace() {
            using (Font font = OpenTestFont()) {
                Assert.Equal(PointSize, font.Size, 1e-3f);
                Assert.True(font.Height > 0, "the font reported a non-positive line height");
                Assert.True(font.Ascent > 0, "the font reported a non-positive ascent");
                // FreeType reports descent below the baseline as a negative number.
                Assert.True(font.Descent <= 0, $"expected a non-positive descent, got {font.Descent}");
                Assert.Equal("Pixelbasel", font.FamilyName);
                Assert.Equal("Medium", font.StyleName);
                Assert.True(font.IsScalable);
                Assert.Equal(1, font.NumFaces);
            }
        }

        [Fact]
        public void OpenFont_ThenChangeThePointSize_IsReflectedInTheNativeMetrics() {
            using (Font font = OpenTestFont()) {
                int smallHeight = font.Height;

                font.Size = PointSize * 3f;

                Assert.True(
                    font.Height > smallHeight,
                    $"tripling the point size did not grow the line height ({smallHeight} -> {font.Height})");
            }
        }

        [Fact]
        public void OpenFontWithProperties_UsesTheBundledFontAndConfiguredPointSize() {
            using FontCreateProperties properties = new FontCreateProperties();
            Assert.True(properties.FileName.Set(TestFontPath));
            Assert.True(properties.Size.Set(PointSize * 2f));

            using Font font = new Font(properties);

            Assert.Equal(PointSize * 2f, font.Size, 1e-3f);
            Assert.Equal("Pixelbasel", font.FamilyName);
        }

        [Fact]
        public void OpenFontFromStream_RespectsTheRequestedStreamOwnership() {
            using (IOStream borrowed = IOStream.OpenRead(TestFontPath)) {
                using Font font = new Font(borrowed, PointSize);

                Assert.NotEqual(IntPtr.Zero, borrowed.NativePointer);
                Assert.True(borrowed.Size > 0);
            }

            IOStream owned = IOStream.OpenRead(TestFontPath);
            try {
                using (Font font = new Font(owned, PointSize, closeStream: true)) {
                    Assert.NotEqual(IntPtr.Zero, owned.NativePointer);
                    Assert.True(owned.Size > 0);
                }

                Assert.Equal(IntPtr.Zero, owned.NativePointer);
                Assert.Throws<ObjectDisposedException>(() => _ = owned.Size);
            } finally {
                owned.Dispose();
            }
        }

        [Fact]
        public void Font_SettingsAndCopy_RoundTripThroughTheNativeFace() {
            using Font font = OpenTestFont();
            font.Outline = 2;
            font.Hinting = HintingFlags.None;
            font.Kerning = false;
            font.WrapAlignment = HorizontalAlignment.Center;
            font.Style = FontStyleFlags.Bold | FontStyleFlags.Underline;
            font.LineSkip = font.Height + 5;

            Assert.Equal(font.Height + 5, font.LineSkip);
            Assert.Equal(2, font.Outline);
            Assert.Equal(HintingFlags.None, font.Hinting);
            Assert.False(font.Kerning);
            Assert.Equal(HorizontalAlignment.Center, font.WrapAlignment);
            Assert.Equal(FontStyleFlags.Bold | FontStyleFlags.Underline, font.Style);

            using Font copy = font.Copy();
            Assert.Equal(font.Size, copy.Size, 1e-3f);
            Assert.Equal(font.Style, copy.Style);
            Assert.Equal(font.Outline, copy.Outline);
        }

        [Fact]
        public void OpenFont_OfAMissingFile_ThrowsCarryingTheNativeErrorMessage() {
            CSDL.SDLException error = Assert.Throws<CSDL.SDLException>(
                () => new Font("this-font-does-not-exist.ttf", PointSize));

            Assert.False(string.IsNullOrWhiteSpace(error.SdlError));
        }

        [Fact]
        public void RenderTextBlended_WithTheBundledFont_RasterisesGlyphsIntoASurface() {
            // The end-to-end proof for SDL_ttf: FreeType rasterises, SDL_ttf composites, and the
            // result comes back as a surface this wrapper can read pixels out of.
            using (Font font = OpenTestFont())
            using (Surface rendered = font.RenderTextBlended("CSDL3", new Color(255, 255, 255))) {
                Assert.True(rendered.Width > 0, "the rendered surface has no width");
                Assert.True(rendered.Height > 0, "the rendered surface has no height");
                Assert.True(HasAnyOpaquePixel(rendered), "the rendered surface is fully transparent - nothing was rasterised");
            }
        }

        [Fact]
        public void RenderTextBlended_ForALongerString_ProducesAWiderSurface() {
            // Guards against a stub that returns a fixed-size surface regardless of the text.
            using (Font font = OpenTestFont())
            using (Surface shortText = font.RenderTextBlended("i", new Color(255, 255, 255)))
            using (Surface longText = font.RenderTextBlended("iiiiiiiiii", new Color(255, 255, 255))) {
                Assert.True(
                    longText.Width > shortText.Width,
                    $"ten glyphs ({longText.Width}px) were not wider than one ({shortText.Width}px)");
                Assert.Equal(shortText.Height, longText.Height);
            }
        }

        [Fact]
        public void RenderTextShaded_WithTheBundledFont_FillsTheBackgroundColour() {
            // Shaded rendering must produce an opaque box, unlike the blended mode above.
            using (Font font = OpenTestFont())
            using (Surface rendered = font.RenderTextShaded("CSDL3", new Color(255, 255, 255), new Color(0, 0, 255))) {
                Assert.True(rendered.ReadPixelFloat(0, 0, out float r, out float g, out float b, out float a));

                Assert.True(a > 0.99f, $"expected an opaque background, got alpha {a}");
                Assert.True(b > r && b > g, $"expected the blue background to dominate, got ({r}, {g}, {b})");
            }
        }

        [Fact]
        public void GetStringSize_WithTheBundledFont_MatchesTheRenderedSurface() {
            using (Font font = OpenTestFont()) {
                Assert.True(font.GetStringSize("CSDL3", out int width, out int height), CSDL.Error.GetError());

                using (Surface rendered = font.RenderTextBlended("CSDL3", new Color(255, 255, 255))) {
                    Assert.Equal(width, rendered.Width);
                    Assert.Equal(height, rendered.Height);
                }
            }
        }

        [Fact]
        public void GlyphOperations_WithTheBundledFont_ReturnMetricsAndRasterisedImages() {
            using Font font = OpenTestFont();

            Assert.True(font.HasGlyph('A'));
            Assert.True(font.GetGlyphMetrics('A', out int minX, out int maxX, out int minY, out int maxY, out int advance));
            Assert.True(maxX > minX, $"invalid horizontal bounds ({minX}, {maxX})");
            Assert.True(maxY > minY, $"invalid vertical bounds ({minY}, {maxY})");
            Assert.True(advance > 0, "the glyph has no advance");

            using (Surface glyph = font.RenderGlyphBlended('A', Color.White)) {
                Assert.True(glyph.Width > 0);
                Assert.True(glyph.Height > 0);
                Assert.True(HasAnyOpaquePixel(glyph));
            }

            using (Surface image = font.GetGlyphImage('A', out ImageType imageType)) {
                Assert.Equal(ImageType.Alpha, imageType);
                Assert.True(image.Width > 0);
                Assert.True(image.Height > 0);
            }
        }

        [Fact]
        public void MeasureAndWrapString_WithTheBundledFont_ReportBoundedLayout() {
            using Font font = OpenTestFont();
            Assert.True(font.GetStringSize("AAAA", out int fullWidth, out int singleLineHeight));
            Assert.True(font.MeasureString("AAAA", fullWidth / 2, out int measuredWidth, out int measuredLength));
            Assert.True(font.GetStringSizeWrapped("AAAA AAAA", fullWidth, out int wrappedWidth, out int wrappedHeight));

            Assert.InRange(measuredWidth, 1, fullWidth / 2);
            Assert.InRange(measuredLength, 1, "AAAA".Length - 1);
            Assert.InRange(wrappedWidth, 1, fullWidth);
            Assert.True(wrappedHeight > singleLineHeight, "a wrapped string did not produce multiple lines");
        }

        [Fact]
        public void SurfaceTextEngine_UpdatesAndDrawsTextOntoASurface() {
            using Font font = OpenTestFont();
            using SurfaceTextEngine engine = new SurfaceTextEngine();
            using TextObject text = new TextObject(engine, font, "A");

            Assert.Same(engine, text.Engine);
            Assert.Same(font, text.Font);
            text.Color = new Color(255, 0, 0, 255);
            Assert.Equal(new Color(255, 0, 0, 255), text.Color);
            text.FloatColor = new FColor(0f, 1f, 0f, 1f);
            Assert.Equal(new FColor(0f, 1f, 0f, 1f), text.FloatColor);
            Assert.True(text.SetPosition(7, 9));
            Assert.True(text.GetPosition(out int x, out int y));
            Assert.Equal(7, x);
            Assert.Equal(9, y);
            Assert.True(text.GetSize(out int initialWidth, out int initialHeight));
            Assert.True(initialWidth > 0);
            Assert.True(initialHeight > 0);

            Assert.True(text.AppendString("A"));
            Assert.True(text.InsertString(1, "A"));
            Assert.True(text.DeleteString(1, 1));
            Assert.True(text.Update());
            Assert.True(text.GetSize(out int updatedWidth, out _));
            Assert.True(updatedWidth > initialWidth);
            Assert.True(text.GetSubString(0, out SubString first));
            Assert.Equal(0, first.Offset);
            Assert.True(first.Length > 0);
            Assert.NotEmpty(text.GetSubStringsForRange(0, 1));

            using Surface target = new Surface(128, 64, PixelFormats.RGBA32);
            Assert.True(target.Clear(0f, 0f, 0f, 0f));
            Assert.True(text.DrawToSurface(3, 5, target));
            Assert.True(HasAnyOpaquePixel(target), "surface text drawing produced no visible pixels");
        }

        private static Font OpenTestFont() {
            Assert.True(File.Exists(TestFontPath), $"the bundled font was not copied to '{TestFontPath}'");
            return new Font(TestFontPath, PointSize);
        }

        /// <summary>
        /// True if any pixel in <paramref name="surface"/> is not fully transparent. Blended text
        /// is rendered onto a transparent field, so this is what distinguishes real glyph coverage
        /// from an empty surface of the right size.
        /// </summary>
        private static bool HasAnyOpaquePixel(Surface surface) {
            for (int y = 0; y < surface.Height; y++) {
                for (int x = 0; x < surface.Width; x++) {
                    if (surface.ReadPixelFloat(x, y, out _, out _, out _, out float alpha) && alpha > 0f) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
