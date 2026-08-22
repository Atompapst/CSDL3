// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CSDL.Extensions;

namespace CSDL.Video {
    public class PixelGrid {
        private readonly Surface _surface;
        private PixelData[,] _pixels;

        public PixelGrid(Surface surface) {
            _surface = surface;
            InitializePixelData();
        }

        [MemberNotNull(nameof(_pixels))]
        private void InitializePixelData() {
            int width = _surface.Width;
            int height = _surface.Height;
            _pixels = new PixelData[width, height];
            _surface.Lock();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    byte r, g, b, a;
                    ReadSurfacePixel(x, y, out r, out g, out b, out a);
                    _pixels[x, y] = new PixelData(new FPoint(x, y), new Color(r, g, b, a));
                }

            }
            _surface.Unlock();
        }

        public void SetColor(FPoint point, Color color) {
            ValidateCoordinates(point);
            int x = (int)point.X;
            int y = (int)point.Y;
            _surface.WritePixel(x, y, color.R, color.G, color.B, color.A);
            _pixels[x, y].Color = color;
        }

        public Color GetColor(FPoint point) {
            ValidateCoordinates(point);
            return _pixels[(int)point.X, (int)point.Y]?.Color ?? Color.Black;
        }

        public PixelData GetPixelData(FPoint point) {
            ValidateCoordinates(point);
            return _pixels[(int)point.X, (int)point.Y];
        }

        // Use instance method so we can access _canvas and its surface pointer.
        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ReadSurfacePixel"/>
        private bool ReadSurfacePixel(int x, int y, out byte r, out byte g, out byte b, out byte a) {
            r = g = b = a = 0;
            return SDL.ReadSurfacePixel(_surface.Handle, x, y, out r, out g, out b, out a).LogIfFalse();
        }

        private void ValidateCoordinates(FPoint point) {
            if (point.X < 0 || point.X >= _surface.Width) {
                throw new ArgumentOutOfRangeException(nameof(point.X), "X-coordinate is out of bounds.");
            }
            if (point.Y < 0 || point.Y >= _surface.Height) {
                throw new ArgumentOutOfRangeException(nameof(point.Y), "Y-coordinate is out of bounds.");
            }
        }

        public IEnumerable<PixelData> GetPixelData() {
            foreach ((int x, int y) in IterateOverPixels()) {
                yield return _pixels[x, y];
            }
        }

        private IEnumerable<(int X, int Y)> IterateOverPixels() {
            for (int y = 0; y < _surface.Height; y++) {
                for (int x = 0; x < _surface.Width; x++) {
                    yield return (x, y);
                }
            }
        }
    }

    public class PixelData {
        public FPoint Position { get; }
        public Color Color { get; set; }

        public PixelData(FPoint position, Color color) {
            Position = position;
            Color = color;
        }

        public override string ToString() {
            return $"PixelData(Position: {Position}, Color: {Color})";
        }
    }
}
