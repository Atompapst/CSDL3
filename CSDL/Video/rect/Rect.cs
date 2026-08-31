// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Numerics;

namespace CSDL.Video {

    public partial struct Rect : IEquatable<Rect> {

        public static Rect operator +(Rect a, Rect b) {
            return new Rect(a.X + b.X, a.Y + b.Y, a.W + b.W, a.H + b.H);
        }
        public static Rect operator -(Rect a, Rect b) {
            return new Rect(a.X - b.X, a.Y - b.Y, a.W - b.W, a.H - b.H);
        }
        public static Rect operator *(Rect p, int scalar) {
            return new Rect(p.X * scalar, p.Y * scalar, p.W * scalar, p.H * scalar);
        }
        public static Rect operator /(Rect p, int scalar) {
            return new Rect(p.X / scalar, p.Y / scalar, p.W / scalar, p.H / scalar);
        }

        public Rect(Point topLeft, Point size) {
            X = topLeft.X;
            Y = topLeft.Y;
            W = size.X;
            H = size.Y;
        }

        public static Rect One => new Rect(0, 0, 1, 1);
        public Point Position => new Point(X, Y);
        public Point Size => new Point(W, H);

        /// <summary>
        /// <c>true</c> if <paramref name="point"/> lies within this rect (matches SDL_PointInRect's
        /// half-open semantics: the right/bottom edge is excluded).
        /// </summary>
        /// <seealso><c>SDL_PointInRect</c></seealso>
        public bool Contains(Point point) {
            return point.X >= X && point.X < X + W && point.Y >= Y && point.Y < Y + H;
        }

        /// <summary>
        /// <c>true</c> if this rect has no area, i.e. width or height is zero or negative.
        /// </summary>
        /// <seealso><c>SDL_RectEmpty</c></seealso>
        public bool IsEmpty => W <= 0 || H <= 0;

        /// <summary>
        /// Clips <paramref name="line"/> to this rect, writing the clipped segment to
        /// <paramref name="res"/> and returning whether it intersects at all.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectAndLineIntersection"/>
        public bool GetIntersection(Line line, out Line res) {
            // Round rather than truncate - a plain (int) cast biases towards zero (-1.9f -> -1
            // instead of -2), which skews clipping results for lines with negative coordinates.
            int x1 = (int)MathF.Round(line.X1), y1 = (int)MathF.Round(line.Y1);
            int x2 = (int)MathF.Round(line.X2), y2 = (int)MathF.Round(line.Y2);
            bool intersects = GetRectAndLineIntersection(in this, ref x1, ref y1, ref x2, ref y2);
            res = intersects ? new Line(x1, y1, x2, y2) : default(Line);
            return intersects;
        }

        /// <summary>
        ///     Converts a <see cref="Vector4" /> to a <see cref="Rect" />, truncating each component to
        ///     an <see cref="int" />. <see cref="Vector4.Z" /> and <see cref="Vector4.W" /> map to
        ///     <see cref="W" /> and <see cref="H" /> respectively.
        /// </summary>
        public static explicit operator Rect(Vector4 v) {
            return new Rect((int)v.X, (int)v.Y, (int)v.Z, (int)v.W);
        }

        /// <summary>
        ///     Converts a <see cref="Rect" /> to a <see cref="Vector4" />. <see cref="W" /> and
        ///     <see cref="H" /> map to <see cref="Vector4.Z" /> and <see cref="Vector4.W" /> respectively.
        /// </summary>
        public static explicit operator Vector4(Rect r) {
            return new Vector4(r.X, r.Y, r.W, r.H);
        }

        /// <seealso><c>SDL_RectsEqual</c></seealso>
        public bool Equals(Rect other) {
            return X == other.X && Y == other.Y && W == other.W && H == other.H;
        }
        public override bool Equals(object? obj) {
            return obj is Rect other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(X, Y, W, H);
        }
        public static bool operator ==(Rect left, Rect right) {
            return left.Equals(right);
        }
        public static bool operator !=(Rect left, Rect right) {
            return !left.Equals(right);
        }
    }
}
