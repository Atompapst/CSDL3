// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CSDL.Video {
    public partial struct FRect : IEquatable<FRect> {
        public static FRect operator +(FRect a, FRect b) {
            return new FRect(a.X + b.X, a.Y + b.Y, a.W + b.W, a.H + b.H);
        }
        public static FRect operator -(FRect a, FRect b) {
            return new FRect(a.X - b.X, a.Y - b.Y, a.W - b.W, a.H - b.H);
        }
        public static FRect operator *(FRect p, float scalar) {
            return new FRect(p.X * scalar, p.Y * scalar, p.W * scalar, p.H * scalar);
        }
        public static FRect operator /(FRect p, float scalar) {
            return new FRect(p.X / scalar, p.Y / scalar, p.W / scalar, p.H / scalar);
        }

        public FRect(FPoint topLeft, FPoint size) {
            X = topLeft.X;
            Y = topLeft.Y;
            W = size.X;
            H = size.Y;
        }

        public static FRect One => new FRect(0, 0, 1, 1);
        public FPoint Position => new FPoint(X, Y);

        /// <summary>
        /// Clips <paramref name="line"/> to this rect, writing the clipped segment to
        /// <paramref name="res"/> and returning whether it intersects at all.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectAndLineIntersectionFloat"/>
        public bool GetIntersection(Line line, out Line res) {
            float x1 = line.X1, y1 = line.Y1, x2 = line.X2, y2 = line.Y2;
            bool intersects = GetRectAndLineIntersection(this, ref x1, ref y1, ref x2, ref y2);
            res = intersects ? new Line(x1, y1, x2, y2) : default(Line);
            return intersects;
        }

        /// <summary>
        ///     Reinterprets the bits of a <see cref="Vector4" /> as an <see cref="FRect" /> - both are four
        ///     sequential <see cref="float" /> fields, so this is a bit cast, not a numeric conversion.
        ///     <see cref="Vector4.Z" /> and <see cref="Vector4.W" /> map to <see cref="W" /> and <see cref="H" /> respectively.
        /// </summary>
        public static explicit operator FRect(Vector4 v) {
            return Unsafe.BitCast<Vector4, FRect>(v);
        }

        /// <summary>
        ///     Reinterprets the bits of an <see cref="FRect" /> as a <see cref="Vector4" /> - both are four
        ///     sequential <see cref="float" /> fields, so this is a bit cast, not a numeric conversion.
        ///     <see cref="W" /> and <see cref="H" /> map to <see cref="Vector4.Z" /> and <see cref="Vector4.W" /> respectively.
        /// </summary>
        public static explicit operator Vector4(FRect r) {
            return Unsafe.BitCast<FRect, Vector4>(r);
        }

        public static bool operator ==(FRect? left, FRect? right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }
            if (left is null || right is null) {
                return false;
            }

            return left.Value.X == right.Value.X &&
                   left.Value.Y == right.Value.Y &&
                   left.Value.W == right.Value.W &&
                   left.Value.H == right.Value.H;
        }

        public static bool operator !=(FRect? left, FRect? right) {
            return !(left == right);
        }

        public override bool Equals([NotNullWhen(true)] object? obj) {
            return obj is FRect other && Equals(other);
        }
        public bool Equals(FRect other) {
            return X.Equals(other.X) && Y.Equals(other.Y) && W.Equals(other.W) && H.Equals(other.H);
        }
        public override int GetHashCode() {
            return HashCode.Combine(X, Y, W, H);
        }
    }
}
