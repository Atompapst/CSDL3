// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL.Video {
    public partial struct Color : IEquatable<Color> {
        private const byte MinValue = 0;
        private const byte MaxValue = 255;

        /// <summary>
        /// A new instance of <see cref="Color"/> with alpha component set to 255.
        /// </summary>
        public Color(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
            A = (byte)Macros.AlphaOpaque;
        }
        /// <summary>
        /// R: 255, G: 0, B: 0
        /// </summary>
        public static Color Red => new Color(MaxValue, MinValue, MinValue);
        /// <summary>
        /// R: 0, G: 255, B: 0
        /// </summary>
        public static Color Green => new Color(MinValue, MaxValue, MinValue);
        /// <summary>
        /// R: 0, G: 0, B: 255
        /// </summary>
        public static Color Blue => new Color(MinValue, MinValue, MaxValue);
        /// <summary>
        /// R: 255, G: 255, B: 255
        /// </summary>
        public static Color White => new Color(MaxValue, MaxValue, MaxValue);
        /// <summary>
        /// R: 0, G: 0, B: 0
        /// </summary>
        public static Color Black => new Color(MinValue, MinValue, MinValue);
        /// <summary>
        /// R: 255, G: 255, B: 0
        /// </summary>
        public static Color Yellow => new Color(MaxValue, MaxValue, MinValue);
        /// <summary>
        /// R: 0, G: 255, B: 255
        /// </summary>
        public static Color Cyan => new Color(MinValue, MaxValue, MaxValue);
        /// <summary>
        /// R: 255, G: 0, B: 255
        /// </summary>
        public static Color Magenta => new Color(MaxValue, MinValue, MaxValue);
        /// <summary>
        /// R: 64, G: 64, B: 64
        /// </summary>
        public static Color DarkGrey => new Color(64, 64, 64);
        /// <summary>
        /// R: 128, G: 128, B: 128
        /// </summary>
        public static Color IntermediateGrey => new Color(128, 128, 128);
        /// <summary>
        /// R: 192, G: 192, B: 192
        /// </summary>
        public static Color BrightGrey => new Color(192, 192, 192);
        /// <summary>
        /// R: 0, G: 0, B: 0, A: 0
        /// </summary>
        public static Color Transparent => new Color(MinValue, MinValue, MinValue, (byte)Macros.AlphaTransparent);

        /// <summary>
        /// Generates a random color with random red, green, and blue components.
        /// The alpha component is always set to 1.0.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Color"/> with randomly generated red, green, and blue components.
        /// </returns>
        public static Color Random() {
            return new Color(
                (byte)Rand.Global.Next(MinValue, MaxValue + 1),
                (byte)Rand.Global.Next(MinValue, MaxValue + 1),
                (byte)Rand.Global.Next(MinValue, MaxValue + 1),
                MaxValue
            );
        }
        /// <summary>
        /// Converts a <see cref="Color"/> to an <see cref="FColor"/> by normalizing its components to a range of 0 to 1.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> instance to be converted.</param>
        /// <returns>A new instance of <see cref="FColor"/> with normalized RGBA components.</returns>
        public static FColor ToFColor(Color color) {
            return new FColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }


        public static Color FromHex(string hex) {
            if (hex.StartsWith('#')) {
                hex = hex.Substring(1);
            }
            if (hex.Length != 6 && hex.Length != 8) {
                throw new ArgumentException("Hex color must be in the format RRGGBB or RRGGBBAA.");
            }
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = hex.Length == 8 ? Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;

            return new Color(r, g, b, a);
        }

        public static bool operator ==(Color left, Color right) {
            return left.R == right.R &&
                   left.G == right.G &&
                   left.B == right.B &&
                   left.A == right.A;
        }

        public static bool operator !=(Color left, Color right) {
            return !(left == right);
        }

        public override bool Equals(object? obj) {
            if (obj is Color other) {
                return this == other;
            }
            return false;
        }

        public bool Equals(Color other) {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public override int GetHashCode() {
            return HashCode.Combine(R, G, B, A);
        }

        public override string ToString() {
            return $"Color(R: {R}, G: {G}, B: {B}, A: {A})";
        }
    }
}
