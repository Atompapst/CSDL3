// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Video;
using CSDL3.Tests.TestSupport;

namespace CSDL3.Tests.Video {
    [Collection(SdlCollection.Name)]
    public sealed class WindowRendererNativeTests {
        [Fact]
        public void CreateWindowAndRenderer_ClearThenReadPixels_ReturnsTheClearColour() {
            const int width = 32;
            const int height = 24;
            Color clearColor = new Color(19, 87, 163);

            Window.CreateWindowAndRenderer(
                "CSDL3 Window Renderer Test",
                width,
                height,
                WindowFlags.AlwaysOnTop,
                out Window window,
                out Renderer renderer);
            using (window)
            using (renderer) {
                Assert.Equal(new Point(width, height), window.Size);
                Assert.Equal(window.Id, renderer.GetWindow().Id);

                renderer.ClearColor = clearColor;
                Assert.True(renderer.Clear(), CSDL.Error.GetError());

                using (Surface pixel = renderer.ReadPixels(Rect.One)) {
                    Assert.True(pixel.ReadPixelFloat(0, 0, out float r, out float g, out float b, out float a), CSDL.Error.GetError());
                    const float tolerance = 1f / 255f;
                    Assert.Equal(clearColor.R / 255f, r, tolerance);
                    Assert.Equal(clearColor.G / 255f, g, tolerance);
                    Assert.Equal(clearColor.B / 255f, b, tolerance);
                    Assert.Equal(1f, a, tolerance);
                }

                Assert.True(renderer.Present(), CSDL.Error.GetError());
            }
        }
    }
}
