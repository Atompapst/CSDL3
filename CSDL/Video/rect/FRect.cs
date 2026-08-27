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
        
        #region CSDL_IMPL SDL_RECT_CAN_OVERFLOW : SDL_rect_impl#SDL_RECT_CAN_OVERFLOW
        
        /// <summary>Shared with <see cref="Rect"/> and <see cref="LineUtils"/> - mirrors SDL_RECT_CAN_OVERFLOW.</summary>
        internal static bool RectCanOverflow(ref FRect r) {
            const float halfMax = (float)(int.MaxValue / 2);
            const float halfMin = (float)(int.MinValue / 2);
            return r.X <= halfMin || r.X >= halfMax ||
                   r.Y <= halfMin || r.Y >= halfMax ||
                   r.W >= halfMax || r.H >= halfMax;
        }
        #endregion
        
        #region CSDL_IMPL SDL_HasRectIntersectionFloat : SDL_rect_impl#SDL_HASINTERSECTION, SDL_RECT_CAN_OVERFLOW

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersectionFloat"/>
        public bool Intersects(FRect other) {
            return Intersects(this, other);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersectionFloat"/>
        public static bool Intersects(FRect a, FRect b) {
            if (RectCanOverflow(ref a) || RectCanOverflow(ref b)) return false;
            return HasRectIntersection(ref a, ref b);
        }

        private static bool HasRectIntersection(ref FRect a, ref FRect b) {
            // Horizontal intersection - float's ENCLOSEPOINTS_EPSILON is 0, unlike Rect's 1.
            float aMin = a.X;
            float aMax = aMin + a.W;
            float bMin = b.X;
            float bMax = bMin + b.W;
            if (bMin > aMin) aMin = bMin;
            if (bMax < aMax) aMax = bMax;
            if (aMax < aMin) return false;

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) aMin = bMin;
            if (bMax < aMax) aMax = bMax;
            if (aMax < aMin) return false;
            return true;
        }
        #endregion

        #region CSDL_IMPL SDL_GetRectIntersectionFloat : SDL_rect_impl#SDL_INTERSECTRECT, SDL_RECT_CAN_OVERFLOW

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectIntersectionFloat"/>
        public static bool GetRectIntersection(FRect a, FRect b, out FRect result) {
            // false just means "these rects don't overlap" (or overflow) - not an SDL error.
            if (RectCanOverflow(ref a) || RectCanOverflow(ref b)) {
                result = default;
                return false;
            }

            // Horizontal intersection
            float aMin = a.X;
            float aMax = aMin + a.W;
            float bMin = b.X;
            float bMax = bMin + b.W;
            if (bMin > aMin) aMin = bMin;
            float x = aMin;
            if (bMax < aMax) aMax = bMax;
            float w = aMax - aMin;

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) aMin = bMin;
            float y = aMin;
            if (bMax < aMax) aMax = bMax;
            float h = aMax - aMin;

            result = new FRect(x, y, w, h);
            return !result.IsEmpty;
        }
        #endregion


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
