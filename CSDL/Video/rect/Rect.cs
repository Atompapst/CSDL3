// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
            Rect res = Unsafe.NullRef<Rect>();
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
        
        #region CSDL_IMPL SDL_RECT_CAN_OVERFLOW : SDL_rect_impl#SDL_RECT_CAN_OVERFLOW
        
        /// <summary>Shared with <see cref="FRect"/> and <see cref="LineUtils"/> - mirrors SDL_RECT_CAN_OVERFLOW.</summary>
        private static bool RectCanOverflow(in Rect r) {
            const int halfMax = int.MaxValue / 2;
            const int halfMin = int.MinValue / 2;
            return r.X <= halfMin || r.X >= halfMax ||
                   r.Y <= halfMin || r.Y >= halfMax ||
                   r.W >= halfMax || r.H >= halfMax;
        }
        
        #endregion

        #region CSDL_IMPL SDL_HasRectIntersection : SDL_rect_impl#SDL_HASINTERSECTION, SDL_RECT_CAN_OVERFLOW
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersection"/>
        public bool HasIntersection(in Rect other) {
            if (RectCanOverflow(in this) || RectCanOverflow(in other)) return false;
            return HasRectIntersection(in this, in other);
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersection"/>
        public bool HasIntersectionUnchecked(in Rect other) {
            return HasRectIntersection(in this, in other);
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersection"/>
        public static bool HasIntersection(in Rect a, in Rect b) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) return false;
            return HasRectIntersection(in a, in b);
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersection"/>
        public static bool IntersectsUnchecked(in Rect a, in Rect b) {
            return HasRectIntersection(in a, in b);
        }

        private static bool HasRectIntersection(in Rect a, in Rect b) {
            // Horizontal intersection
            int aMin = a.X;
            int aMax = aMin + a.W;
            int bMin = b.X;
            int bMax = bMin + b.W;
            if (bMin > aMin) aMin = bMin;
            if (bMax < aMax) aMax = bMax;
            if (aMax - 1 < aMin) return false;

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) aMin = bMin;
            if (bMax < aMax) aMax = bMax;
            if (aMax - 1 < aMin) return false;
            return true;
        }
        #endregion

        #region CSDL_IMPL SDL_GetRectIntersection : SDL_rect_impl#SDL_INTERSECTRECT, SDL_RECT_CAN_OVERFLOW

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectIntersection"/>
        public static bool GetRectIntersection(in Rect a, in Rect b, out Rect result) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                result = default;
                return false;
            }

            // Horizontal intersection
            int aMin = a.X;
            int aMax = aMin + a.W;
            int bMin = b.X;
            int bMax = bMin + b.W;
            if (bMin > aMin) aMin = bMin;
            int x = aMin;
            if (bMax < aMax) aMax = bMax;
            int w = aMax - aMin;

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) aMin = bMin;
            int y = aMin;
            if (bMax < aMax) aMax = bMax;
            int h = aMax - aMin;

            result = new Rect(x, y, w, h);
            return !result.IsEmpty;
        }
        #endregion

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectUnion"/>
        public static Rect Union(Rect a, Rect b) {
            SDL.GetRectUnion(a, b, out Rect r).LogIfFalse();
            return (Rect)r;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectIntersection"/>
        public static bool GetRectIntersection(Rect a, Rect b, out Rect result) {
            return SDL.GetRectIntersection(a, b, out result);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectEnclosingPoints"/>
        public static bool TryGetEnclosingPoints(Point[] points, Rect? clip, out Rect result) {
            if (points == null || points.Length == 0) {
                result = default;
                return false;
            }

            NativePtr<Point> raw = points.ToUnmanaged();
            try {
                // false here just means "all points were outside the clip rect" - not an SDL error.
                if (clip.HasValue) {
                    Rect clipValue = clip.Value;
                    return SDL.GetRectEnclosingPoints(raw, points.Length, NativePtr<Rect>.FromRef(ref clipValue), out result);
                }
                return SDL.GetRectEnclosingPoints(raw, points.Length, NativePtr<Rect>.Zero, out result);
            }
            finally {
                raw.Free();
            }
        }

        // public static explicit operator Rect(FRect r) {
        //     return new Rect((int)r.X, (int)r.Y, (int)r.W, (int)r.H);
        // }

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
