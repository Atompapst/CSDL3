// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Numerics;

namespace CSDL.Video {

    public partial struct Point : IEquatable<Point> {
        public static Point Zero => new Point(0, 0);
        public static Point One => new Point(1, 1);
        public static Point UnitX => new Point(1, 0);
        public static Point UnitY => new Point(0, 1);
        public static Point Up => new Point(0, -1);
        public static Point Down => new Point(0, 1);
        public static Point Left => new Point(-1, 0);
        public static Point Right => new Point(1, 0);

        public static Point operator +(Point a, Point b) {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        public static Point operator -(Point a, Point b) {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
        public static Point operator *(Point p, int s) {
            return new Point(p.X * s, p.Y * s);
        }
        public static Point operator /(Point p, int s) {
            return new Point(p.X / s, p.Y / s);
        }

        public static int Dot(Point a, Point b) {
            return PointMath.Dot(a.X, a.Y, b.X, b.Y);
        }

        public double Distance(Point b) {
            return PointMath.Distance(X, Y, b.X, b.Y);
        }
        public double Magnitude() {
            return PointMath.Magnitude(X, Y);
        }
        public static double Distance(Point a, Point b) {
            return PointMath.Distance(a.X, a.Y, b.X, b.Y);
        }

        /// <summary>
        ///     Converts a <see cref="Vector2" /> to a <see cref="Point" />, truncating each component to
        ///     an <see cref="int" />.
        /// </summary>
        public static explicit operator Point(Vector2 v) {
            return new Point((int)v.X, (int)v.Y);
        }

        /// <summary>
        ///     Converts a <see cref="Point" /> to a <see cref="Vector2" />.
        /// </summary>
        public static explicit operator Vector2(Point p) {
            return new Vector2(p.X, p.Y);
        }

        public override string ToString() {
            return $"Point(X: {X}, Y: {Y})";
        }

        public bool Equals(Point other) {
            return X == other.X && Y == other.Y;
        }
        public override bool Equals(object? obj) {
            return obj is Point other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(X, Y);
        }
        public static bool operator ==(Point left, Point right) {
            return left.Equals(right);
        }
        public static bool operator !=(Point left, Point right) {
            return !left.Equals(right);
        }
    }

    internal static class PointMath {
        public static float Magnitude(float x, float y) {
            return (float)Math.Sqrt(x * x + y * y);
        }
        public static double Magnitude(int x, int y) {
            // Widen to double before squaring - x*x/y*y in 32-bit int overflows (and can wrap
            // negative) for |x| or |y| beyond ~46340, which would otherwise feed Math.Sqrt a
            // corrupted or negative sum and silently produce NaN.
            return Math.Sqrt((double)x * x + (double)y * y);
        }

        public static float Distance(float x1, float y1, float x2, float y2) {
            return Magnitude(x1 - x2, y1 - y2);
        }

        public static double Distance(int x1, int y1, int x2, int y2) {
            return Magnitude(x1 - x2, y1 - y2);
        }

        public static float Dot(float x1, float y1, float x2, float y2) {
            return x1 * x2 + y1 * y2;
        }

        public static int Dot(int x1, int y1, int x2, int y2) {
            return x1 * x2 + y1 * y2;
        }
    }
}
