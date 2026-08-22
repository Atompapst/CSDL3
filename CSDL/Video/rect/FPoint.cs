// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CSDL.Video {
    public partial struct FPoint : IEquatable<FPoint> {
        public static FPoint Zero => new FPoint(0, 0);
        public static FPoint One => new FPoint(1, 1);
        public static FPoint UnitX => new FPoint(1, 0);
        public static FPoint UnitY => new FPoint(0, 1);
        public static FPoint Up => new FPoint(0, -1);
        public static FPoint Down => new FPoint(0, 1);
        public static FPoint Left => new FPoint(-1, 0);
        public static FPoint Right => new FPoint(1, 0);

        public static FPoint operator +(FPoint a, FPoint b) {
            return new FPoint(a.X + b.X, a.Y + b.Y);
        }
        public static FPoint operator -(FPoint a, FPoint b) {
            return new FPoint(a.X - b.X, a.Y - b.Y);
        }
        public static FPoint operator *(FPoint p, float s) {
            return new FPoint(p.X * s, p.Y * s);
        }
        public static FPoint operator /(FPoint p, float s) {
            return new FPoint(p.X / s, p.Y / s);
        }

        public static float Dot(FPoint a, FPoint b) {
            return PointMath.Dot(a.X, a.Y, b.X, b.Y);
        }

        public double Distance(FPoint b) {
            return PointMath.Distance(X, Y, b.X, b.Y);
        }

        public float Magnitude() {
            return PointMath.Magnitude(X, Y);
        }

        public static double Distance(FPoint a, FPoint b) {
            return PointMath.Distance(a.X, a.Y, b.X, b.Y);
        }

        public FPoint Normalize() {
            float mag = Magnitude();
            return mag == 0 ? new FPoint(0, 0) : this / mag;
        }

        public static explicit operator FPoint(Point p) {
            return new FPoint(p.X, p.Y);
        }

        /// <summary>
        ///     Reinterprets the bits of a <see cref="Vector2" /> as an <see cref="FPoint" /> - both are two
        ///     sequential <see cref="float" /> fields, so this is a bit cast, not a numeric conversion.
        /// </summary>
        public static explicit operator FPoint(Vector2 v) {
            return Unsafe.BitCast<Vector2, FPoint>(v);
        }

        /// <summary>
        ///     Reinterprets the bits of an <see cref="FPoint" /> as a <see cref="Vector2" /> - both are two
        ///     sequential <see cref="float" /> fields, so this is a bit cast, not a numeric conversion.
        /// </summary>
        public static explicit operator Vector2(FPoint p) {
            return Unsafe.BitCast<FPoint, Vector2>(p);
        }

        public override string ToString() {
            return $"FPoint(X: {X}, Y: {Y})";
        }
        public bool Equals(FPoint other) {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }
        public override bool Equals(object? obj) {
            return obj is FPoint other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(X, Y);
        }
    }
}
