// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using CSDL.Extensions;

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
        /// <c>true</c> if <paramref name="point"/> lies within this rect (matches
        /// SDL_PointInRectFloat's closed semantics: the right/bottom edge is included).
        /// </summary>
        /// <seealso><c>SDL_PointInRectFloat</c></seealso>
        public bool Contains(FPoint point) {
            return point.X >= X && point.X <= X + W && point.Y >= Y && point.Y <= Y + H;
        }

        /// <summary>
        /// <c>true</c> if this rect has no area, i.e. width or height is negative. Unlike
        /// <see cref="Rect.IsEmpty"/>, a zero-sized float rect is NOT considered empty
        /// (matches SDL_RectEmptyFloat).
        /// </summary>
        /// <seealso><c>SDL_RectEmptyFloat</c></seealso>
        public bool IsEmpty => W < 0f || H < 0f;

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersectionFloat"/>
        public bool Intersects(FRect other) {
            return SDL.HasRectIntersectionFloat(other, this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersectionFloat"/>
        public static bool Intersects(FRect a, FRect b) {
            return SDL.HasRectIntersectionFloat(a, b);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectIntersectionFloat"/>
        public static bool GetRectIntersection(FRect a, FRect b, out FRect result) {
            // false just means "these rects don't overlap" - not an SDL error, so no LogIfFalse.
            return SDL.GetRectIntersectionFloat(a, b, out result);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectUnionFloat"/>
        public static FRect Union(FRect a, FRect b) {
            SDL.GetRectUnionFloat(a, b, out FRect result).LogIfFalse();
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectEnclosingPointsFloat"/>
        public static bool TryGetEnclosingPoints(FPoint[] points, FRect? clip, out FRect result) {
            if (points == null || points.Length == 0) {
                result = default;
                return false;
            }

            NativePtr<FPoint> raw = points.ToUnmanaged();
            try {
                // false here just means "all points were outside the clip rect" - not an SDL error.
                if (clip.HasValue) {
                    FRect clipValue = clip.Value;
                    return SDL.GetRectEnclosingPointsFloat(raw, points.Length, NativePtr<FRect>.FromRef(ref clipValue), out result);
                }
                return SDL.GetRectEnclosingPointsFloat(raw, points.Length, NativePtr<FRect>.Zero, out result);
            }
            finally {
                raw.Free();
            }
        }

        // public static explicit operator FRect(Rect r) {
        //     SDL.RectToFRect(r, out FRect result);
        //     return result;
        // }

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
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

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

        /// <summary>
        /// Compares this rect to <paramref name="other"/> allowing each field to differ by up to
        /// <paramref name="epsilon"/>, to absorb floating point precision drift.
        /// </summary>
        /// <seealso><c>SDL_RectsEqualEpsilon</c></seealso>
        public bool EqualsEpsilon(FRect other, float epsilon) {
            return MathF.Abs(X - other.X) <= epsilon &&
                   MathF.Abs(Y - other.Y) <= epsilon &&
                   MathF.Abs(W - other.W) <= epsilon &&
                   MathF.Abs(H - other.H) <= epsilon;
        }

        /// <summary>
        /// Compares this rect to <paramref name="other"/> within <see cref="CSDL.Macros.FltEpsilon"/>,
        /// SDL's default tolerance for floating point rect comparisons.
        /// </summary>
        /// <seealso><c>SDL_RectsEqualFloat</c></seealso>
        public bool EqualsApprox(FRect other) {
            return EqualsEpsilon(other, CSDL.Macros.FltEpsilon);
        }
    }
}
