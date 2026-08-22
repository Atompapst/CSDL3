// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.TTF;
using CSDL.Video;
using CSDL3.Tests.TestSupport;
using TtfApi = CSDL.TTF.TTF;

namespace CSDL3.Tests.TTF {
    /// <summary>
     /// Proves the configured SDL3_ttf runtime works: it loads together with
    /// the FreeType and HarfBuzz it was built against, opens a real font file, and rasterises
    /// glyphs into an SDL surface.
    /// <para>
    /// The rendering tests borrow a font from the host - see <see cref="SystemFonts"/> - because
    /// the repository ships no font binary of its own.
    /// </para>
    /// </summary>
    [Collection(SdlCollection.Name)]
    public class TtfNativeTests {
        private const float PointSize = 24f;

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
        public void OpenFont_WithASystemFont_ExposesMetricsFromTheNativeFace() {
            using (Font font = OpenTestFont()) {
                Assert.Equal(PointSize, font.Size, 1e-3f);
                Assert.True(font.Height > 0, "the font reported a non-positive line height");
                Assert.True(font.Ascent > 0, "the font reported a non-positive ascent");
                // FreeType reports descent below the baseline as a negative number.
                Assert.True(font.Descent <= 0, $"expected a non-positive descent, got {font.Descent}");
                Assert.False(string.IsNullOrWhiteSpace(font.FamilyName));
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
        public void OpenFont_OfAMissingFile_ThrowsCarryingTheNativeErrorMessage() {
            CSDL.SDLException error = Assert.Throws<CSDL.SDLException>(
                () => new Font("this-font-does-not-exist.ttf", PointSize));

            Assert.False(string.IsNullOrWhiteSpace(error.SdlError));
        }

        [Fact]
        public void RenderTextBlended_WithASystemFont_RasterisesGlyphsIntoASurface() {
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
        public void RenderTextShaded_WithASystemFont_FillsTheBackgroundColour() {
            // Shaded rendering must produce an opaque box, unlike the blended mode above.
            using (Font font = OpenTestFont())
            using (Surface rendered = font.RenderTextShaded("CSDL3", new Color(255, 255, 255), new Color(0, 0, 255))) {
                Assert.True(rendered.ReadPixelFloat(0, 0, out float r, out float g, out float b, out float a));

                Assert.True(a > 0.99f, $"expected an opaque background, got alpha {a}");
                Assert.True(b > r && b > g, $"expected the blue background to dominate, got ({r}, {g}, {b})");
            }
        }

        [Fact]
        public void GetStringSize_WithASystemFont_MatchesTheRenderedSurface() {
            using (Font font = OpenTestFont()) {
                Assert.True(font.GetStringSize("CSDL3", out int width, out int height), CSDL.Error.GetError());

                using (Surface rendered = font.RenderTextBlended("CSDL3", new Color(255, 255, 255))) {
                    Assert.Equal(width, rendered.Width);
                    Assert.Equal(height, rendered.Height);
                }
            }
        }

        private static Font OpenTestFont() {
            return new Font(SystemFonts.FirstAvailable, PointSize);
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
