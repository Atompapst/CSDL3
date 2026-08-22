// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.File;
using CSDL.Image;
using CSDL.Video;
using CSDL3.Tests.TestSupport;
using ImageApi = CSDL.Image.Image;

namespace CSDL3.Tests.Image {
    /// <summary>
    /// Proves the configured SDL3_image runtime works: it loads, resolves
    /// its own dependency chain (libpng, libwebp, tiff, ...), and actually encodes and decodes
    /// pixels rather than merely answering version queries.
    /// </summary>
    [Collection(SdlCollection.Name)]
    public class ImageNativeTests {
        private const int Width = 8;
        private const int Height = 4;

        private readonly SdlFixture _sdl;

        public ImageNativeTests(SdlFixture sdl) {
            _sdl = sdl;
        }

        [Fact]
        public void Version_FromNativeLibrary_ReportsAnSdlImage3Runtime() {
            SdlVersionNumber version = new SdlVersionNumber(ImageApi.Version);

            Assert.Equal(3, version.Major);
            Assert.Equal((int)ImageApi.MajorVersion, version.Major);
        }

        [Theory]
        // One case per codec, so a single missing dependency binary names itself in the failure.
        [InlineData(ImageType.PNG)]
        [InlineData(ImageType.BMP)]
        [InlineData(ImageType.TGA)]
        [InlineData(ImageType.WEBP)]
        public void Save_ThenLoad_RoundTripsImageDimensionsThroughTheNativeCodec(ImageType type) {
            string path = _sdl.ScratchPath($"roundtrip-{type}.{Extension(type)}");

            using (Surface original = CreateTestPattern()) {
                Assert.True(SaveAs(type, original, path), $"encoding {type} failed: {CSDL.Error.GetError()}");
            }

            Assert.True(new System.IO.FileInfo(path).Length > 0, $"the {type} encoder wrote an empty file");

            using (Surface reloaded = ImageApi.Load(path)) {
                Assert.Equal(Width, reloaded.Width);
                Assert.Equal(Height, reloaded.Height);
            }
        }

        [Theory]
        // Lossless codecs only: JPEG and lossy WebP legitimately shift these values.
        [InlineData(ImageType.PNG)]
        [InlineData(ImageType.BMP)]
        [InlineData(ImageType.TGA)]
        public void Save_ThenLoad_PreservesExactPixelValuesForLosslessCodecs(ImageType type) {
            string path = _sdl.ScratchPath($"lossless-{type}.{Extension(type)}");

            using (Surface original = CreateTestPattern()) {
                Assert.True(SaveAs(type, original, path), $"encoding {type} failed: {CSDL.Error.GetError()}");
            }

            using (Surface reloaded = ImageApi.Load(path)) {
                // The corner pixels CreateTestPattern wrote.
                AssertPixel(reloaded, 0, 0, 1f, 0f, 0f);
                AssertPixel(reloaded, Width - 1, 0, 0f, 1f, 0f);
                AssertPixel(reloaded, 0, Height - 1, 0f, 0f, 1f);
                AssertPixel(reloaded, Width - 1, Height - 1, 1f, 1f, 1f);
            }
        }

        [Fact]
        public void SaveJPG_ThenLoad_DecodesThroughTheBundledJpegCodec() {
            // JPEG is lossy, so only the geometry and a coarse hue check are meaningful here.
            string path = _sdl.ScratchPath("roundtrip.jpg");

            using (Surface original = CreateTestPattern()) {
                Assert.True(ImageApi.SaveJPG(original, path, 90), $"encoding JPEG failed: {CSDL.Error.GetError()}");
            }

            using (Surface reloaded = ImageApi.Load(path)) {
                Assert.Equal(Width, reloaded.Width);
                Assert.Equal(Height, reloaded.Height);

                Assert.True(reloaded.ReadPixelFloat(0, 0, out float r, out float g, out float b, out _));
                Assert.True(r > g && r > b, $"the top-left pixel should stay predominantly red, got ({r}, {g}, {b})");
            }
        }

        [Fact]
        public void Surface_WriteRgb332Palette_ThenSavePNG_WritesAndPreservesAll256Colours() {
            const int paletteSize = 16;
            string path = _sdl.ScratchPath("palette-256.png");

            using (Surface palette = new Surface(paletteSize, paletteSize, PixelFormats.RGBA32)) {
                for (int i = 0; i < paletteSize * paletteSize; i++) {
                    int x = i % paletteSize;
                    int y = i / paletteSize;
                    byte r = (byte)(((i >> 5) & 0x07) * 255 / 7);
                    byte g = (byte)(((i >> 2) & 0x07) * 255 / 7);
                    byte b = (byte)((i & 0x03) * 255 / 3);

                    Assert.True(palette.WritePixel(x, y, r, g, b, 255), CSDL.Error.GetError());
                }

                Assert.True(palette.SavePNG(path), CSDL.Error.GetError());
            }

            Assert.True(new System.IO.FileInfo(path).Length > 0, "the palette PNG was empty");

            using (Surface reloaded = ImageApi.Load(path)) {
                Assert.Equal(paletteSize, reloaded.Width);
                Assert.Equal(paletteSize, reloaded.Height);

                for (int i = 0; i < paletteSize * paletteSize; i++) {
                    int x = i % paletteSize;
                    int y = i / paletteSize;
                    float r = ((i >> 5) & 0x07) / 7f;
                    float g = ((i >> 2) & 0x07) / 7f;
                    float b = (i & 0x03) / 3f;

                    AssertPixel(reloaded, x, y, r, g, b);
                }
            }
        }

        [Fact]
        public void LoadPNG_FromAnInMemoryStream_DecodesWithoutTouchingTheFilesystem() {
            // Exercises the IOStream path and the format-specific IMG_LoadPNG_IO entry point.
            byte[] encoded = EncodeToPngBytes();
            GCHandle pin = GCHandle.Alloc(encoded, GCHandleType.Pinned);

            try {
                using (IOStream source = IOStream.FromConstMem(pin.AddrOfPinnedObject(), (UIntPtr)encoded.Length))
                using (Surface decoded = ImageApi.LoadPNG(source)) {
                    Assert.Equal(Width, decoded.Width);
                    Assert.Equal(Height, decoded.Height);
                    AssertPixel(decoded, 0, 0, 1f, 0f, 0f);
                }
            }
            finally {
                pin.Free();
            }
        }

        [Fact]
        public void Load_FromAnInMemoryStream_SniffsThePngFormatWithoutAFileExtension() {
            // IMG_Load_IO has no filename to go on, so a successful decode proves the native
            // magic-byte detection ran.
            byte[] encoded = EncodeToPngBytes();
            GCHandle pin = GCHandle.Alloc(encoded, GCHandleType.Pinned);

            try {
                using (IOStream source = IOStream.FromConstMem(pin.AddrOfPinnedObject(), (UIntPtr)encoded.Length))
                using (Surface decoded = ImageApi.Load(source)) {
                    Assert.Equal(Width, decoded.Width);
                    Assert.Equal(Height, decoded.Height);
                }
            }
            finally {
                pin.Free();
            }
        }

        [Fact]
        public void Load_OfAMissingFile_ThrowsCarryingTheNativeErrorMessage() {
            string missing = _sdl.ScratchPath("does-not-exist.png");

            CSDL.SDLException error = Assert.Throws<CSDL.SDLException>(() => ImageApi.Load(missing));

            Assert.False(string.IsNullOrWhiteSpace(error.SdlError));
        }

        [Fact]
        public void Load_OfDataThatIsNotAnImage_ThrowsRatherThanReturningAnInvalidSurface() {
            string path = _sdl.ScratchPath("garbage.png");
            System.IO.File.WriteAllBytes(path, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });

            Assert.Throws<CSDL.SDLException>(() => ImageApi.Load(path));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Load_WithCloseAfter_InvalidatesTheInputStreamAfterASuccessfulDecode(bool typed) {
            string path = _sdl.ScratchPath($"close-after-success-{typed}.png");
            using (Surface original = CreateTestPattern()) {
                Assert.True(ImageApi.SavePNG(original, path));
            }

            IOStream source = IOStream.OpenRead(path);
            try {
                using Surface decoded = typed
                    ? ImageApi.Load(source, ImageType.PNG, closeAfter: true)
                    : ImageApi.Load(source, closeAfter: true);

                Assert.Equal(Width, decoded.Width);
                AssertClosed(source);
            } finally {
                source.Dispose();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Load_WithCloseAfter_InvalidatesTheInputStreamAfterAFailedDecode(bool typed) {
            string path = _sdl.ScratchPath($"close-after-failure-{typed}.png");
            System.IO.File.WriteAllBytes(path, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });

            IOStream source = IOStream.OpenRead(path);
            try {
                if (typed) {
                    Assert.Throws<CSDL.SDLException>(() => ImageApi.Load(source, ImageType.PNG, closeAfter: true));
                } else {
                    Assert.Throws<CSDL.SDLException>(() => ImageApi.Load(source, closeAfter: true));
                }

                AssertClosed(source);
            } finally {
                source.Dispose();
            }
        }

        [Fact]
        public void AnimationEncoder_WithCloseIO_TransfersTheDestinationStreamOwnership() {
            string path = _sdl.ScratchPath("closeio-encoder.gif");
            IOStream destination = IOStream.OpenWrite(path);

            try {
                using Surface frame = CreateTestPattern();
                using (AnimationEncoder encoder = new AnimationEncoder(destination, ImageType.GIF, closeio: true)) {
                    AssertClosed(destination);
                    Assert.True(encoder.AddFrame(frame, 100));
                }

                Assert.True(new System.IO.FileInfo(path).Length > 0);
            } finally {
                destination.Dispose();
            }
        }

        [Fact]
        public void AnimationEncoder_WithCloseIO_InvalidatesTheDestinationStreamWhenCreationFails() {
            string path = _sdl.ScratchPath("closeio-encoder-failure.bin");
            IOStream destination = IOStream.OpenWrite(path);

            try {
                Assert.Throws<CSDL.SDLException>(() => new AnimationEncoder(destination, ImageType.Unknown, closeio: true));
                AssertClosed(destination);
            } finally {
                destination.Dispose();
            }
        }

        [Fact]
        public void AnimationDecoder_WithCloseIO_TransfersTheSourceStreamOwnershipAndStillDecodes() {
            string path = CreateAnimationGif();
            IOStream source = IOStream.OpenRead(path);

            try {
                using AnimationDecoder decoder = new AnimationDecoder(source, ImageType.GIF, closeio: true);
                AssertClosed(source);

                Assert.True(decoder.GetFrame(out Surface frame, out ulong duration));
                Assert.NotNull(frame);
                Assert.Equal((ulong)100, duration);
                using (frame) {
                    Assert.Equal(Width, frame.Width);
                    Assert.Equal(Height, frame.Height);
                }
            } finally {
                source.Dispose();
            }
        }

        [Fact]
        public void AnimationEncoder_WithoutCloseIO_LeavesTheDestinationStreamUsable() {
            string path = _sdl.ScratchPath("borrowed-encoder.gif");
            using IOStream destination = IOStream.OpenWrite(path);
            using Surface frame = CreateTestPattern();

            using (AnimationEncoder encoder = new AnimationEncoder(destination, ImageType.GIF)) {
                Assert.True(encoder.AddFrame(frame, 100));
            }

            Assert.NotEqual(IntPtr.Zero, destination.NativePointer);
            Assert.True(destination.Flush());
            Assert.True(new System.IO.FileInfo(path).Length > 0);
        }

        [Fact]
        public void AnimationDecoder_WithoutCloseIO_LeavesTheSourceStreamUsableAndReturnsDisposableFrames() {
            string path = CreateAnimationGif();
            using IOStream source = IOStream.OpenRead(path);

            using (AnimationDecoder decoder = new AnimationDecoder(source, ImageType.GIF)) {
                Assert.True(decoder.GetFrame(out Surface firstFrame, out ulong firstDuration));
                Assert.NotNull(firstFrame);
                Assert.Equal((ulong)100, firstDuration);
                using (firstFrame) {
                    AssertPixel(firstFrame, 0, 0, 1f, 0f, 0f);
                }

                Assert.True(decoder.GetFrame(out Surface secondFrame, out ulong secondDuration));
                Assert.NotNull(secondFrame);
                Assert.Equal((ulong)200, secondDuration);
                using (secondFrame) {
                    AssertPixel(secondFrame, 0, 0, 0f, 1f, 0f);
                }
            }

            Assert.NotEqual(IntPtr.Zero, source.NativePointer);
            Assert.Equal(0, source.Seek(0, IOWhence.Set));
        }

        /// <summary>
        /// An 8x4 RGBA surface on a black field with the four corners set to red, green, blue and
        /// white, so the round-trip tests have known pixels to assert on.
        /// </summary>
        private static Surface CreateTestPattern() {
            Surface surface = new Surface(Width, Height, PixelFormats.RGBA32);
            try {
                Assert.True(surface.Clear(0f, 0f, 0f, 1f));
                Assert.True(surface.WritePixel(0, 0, 255, 0, 0, 255));
                Assert.True(surface.WritePixel(Width - 1, 0, 0, 255, 0, 255));
                Assert.True(surface.WritePixel(0, Height - 1, 0, 0, 255, 255));
                Assert.True(surface.WritePixel(Width - 1, Height - 1, 255, 255, 255, 255));
                return surface;
            } catch {
                surface.Dispose();
                throw;
            }
        }

        private static bool SaveAs(ImageType type, Surface surface, string path) {
            switch (type) {
                case ImageType.PNG: return ImageApi.SavePNG(surface, path);
                case ImageType.BMP: return ImageApi.SaveBMP(surface, path);
                case ImageType.TGA: return ImageApi.SaveTGA(surface, path);
                case ImageType.WEBP: return ImageApi.SaveWEBP(surface, path, 100f);
                case ImageType.GIF: return ImageApi.SaveGIF(surface, path);
                default: throw new ArgumentOutOfRangeException(nameof(type), type, "no encoder is wired up for this type");
            }
        }

        private static string Extension(ImageType type) {
            return type.ToString().ToLowerInvariant();
        }

        private byte[] EncodeToPngBytes() {
            string path = _sdl.ScratchPath($"memsource-{Guid.NewGuid():N}.png");
            using (Surface surface = CreateTestPattern()) {
                Assert.True(ImageApi.SavePNG(surface, path), CSDL.Error.GetError());
            }
            return System.IO.File.ReadAllBytes(path);
        }

        private string CreateAnimationGif() {
            string path = _sdl.ScratchPath($"animation-{Guid.NewGuid():N}.gif");
            using Surface first = CreateTestPattern();
            using Surface second = new Surface(Width, Height, PixelFormats.RGBA32);
            Assert.True(second.Clear(0f, 0f, 0f, 1f));
            Assert.True(second.WritePixel(0, 0, 0, 255, 0, 255));

            using (AnimationEncoder encoder = new AnimationEncoder(path)) {
                Assert.True(encoder.AddFrame(first, 100));
                Assert.True(encoder.AddFrame(second, 200));
            }

            return path;
        }

        private static void AssertClosed(IOStream stream) {
            Assert.Equal(IntPtr.Zero, stream.NativePointer);
            Assert.Throws<ObjectDisposedException>(() => _ = stream.Size);
        }

        private static void AssertPixel(Surface surface, int x, int y, float expectedR, float expectedG, float expectedB) {
            Assert.True(
                surface.ReadPixelFloat(x, y, out float r, out float g, out float b, out _),
                $"could not read pixel ({x}, {y}): {CSDL.Error.GetError()}");

            const float tolerance = 1f / 255f;
            Assert.Equal(expectedR, r, tolerance);
            Assert.Equal(expectedG, g, tolerance);
            Assert.Equal(expectedB, b, tolerance);
        }
    }
}
