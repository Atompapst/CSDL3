// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL.Video {
    public partial struct FColor : IEquatable<FColor> {
        private const float Min = 0.0f;
        private const float Max = 1.0f;
        /// <summary>
        /// A new instance of <see cref="FColor"/> with alpha component set to 1.Min.
        /// </summary>
        public FColor(float r, float g, float b) {
            R = r;
            G = g;
            B = b;
            A = Macros.AlphaOpaqueFloat;
        }

        /// <summary>
        /// R: 1, G: 0, B: 0
        /// </summary>
        public static FColor Red => new FColor(Max, Min, Min, Max);
        /// <summary>
        /// R: 0, G: 1, B: 0
        /// </summary>
        public static FColor Green => new FColor(Min, Max, Min, Max);
        /// <summary>
        /// R: 0, G: 0, B: 1
        /// </summary>
        public static FColor Blue => new FColor(Min, Min, Max, Max);
        /// <summary>
        /// R: 1, G: 1, B: 1
        /// </summary>
        public static FColor White => new FColor(Max, Max, Max, Max);
        /// <summary>
        /// R: 0, G: 0, B: 0
        /// </summary>
        public static FColor Black => new FColor(Min, Min, Min, Max);
        /// <summary>
        /// R: 1, G: 1, B: 0
        /// </summary>
        public static FColor Yellow => new FColor(Max, Max, Min, Max);
        /// <summary>
        /// R: 0, G: 1, B: 1
        /// </summary>
        public static FColor Cyan => new FColor(Min, Max, Max, Max);
        /// <summary>
        /// R: 1, G: 0, B: 1
        /// </summary>
        public static FColor Magenta => new FColor(Max, Min, Max, Max);
        /// <summary>
        /// R: 0.5, G: 0.5, B: 0.5
        /// </summary>
        public static FColor DarkGrey => new FColor(0.5f, 0.5f, 0.5f, Max);
        /// <summary>
        /// R: 0.75, G: 0.75, B: 0.75
        /// </summary>
        public static FColor IntermediateGrey => new FColor(0.75f, 0.75f, 0.75f, Max);
        /// <summary>
        /// R: 0.9, G: 0.9, B: 0.9
        /// </summary>
        public static FColor BrightGrey => new FColor(0.9f, 0.9f, 0.9f, Max);
        /// <summary>
        /// R: 0, G: 0, B: 0, A: 0
        /// </summary>
        public static FColor Transparent => new FColor(Min, Min, Min, Macros.AlphaTransparentFloat);

        /// <summary>
        /// Generates a random color with random red, green, and blue components.
        /// The alpha component is always set to 1.Min.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="FColor"/> with randomly generated red, green, and blue components.
        /// </returns>
        public static FColor Random() {
            return new FColor(Rand.Global.Next(Min, Max),
                Rand.Global.Next(Min, Max),
                Rand.Global.Next(Min, Max),
                Max);
        }

        /// <summary>
        /// Converts the current <see cref="FColor"/> instance to a <see cref="Color"/> structure.
        /// The resulting Color uses 8-bit per channel representation by rounding the RGB and Alpha
        /// components of FColor, scaled from 0.0–1.Min to 0–255.
        /// </summary>
        /// <returns>Returns a <see cref="Color"/> based on the corresponding values of the FColor instance.</returns>
        public Color ToColor() {
            return new Color(
                (byte)Math.Round(R * 255f),
                (byte)Math.Round(G * 255f),
                (byte)Math.Round(B * 255f),
                (byte)Math.Round(A * 255f));
        }


        public static bool operator ==(FColor l, FColor r) {
            return l.R == r.R && l.G == r.G && l.B == r.B && l.A == r.A;
        }

        public static bool operator !=(FColor l, FColor r) {
            return !(l == r);
        }

        public override bool Equals(object? obj) {
            if (obj is FColor other) {
                return this == other;
            }
            return false;
        }

        public bool Equals(FColor other) {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public override int GetHashCode() {
            return HashCode.Combine(R, G, B, A);
        }

        public override string ToString() {
            return $"FColor(R:{R:0.###}, G:{G:0.###}, B:{B:0.###}, A:{A:0.###})";
        }
    }
}
