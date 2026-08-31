// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Video {

    public partial struct FRect {

        #region CSDL_IMPL SDL_PointInRectFloat : SDL_rect#SDL_PointInRectFloat

        /// <summary>
        /// <c>true</c> if <paramref name="point"/> lies within this rect (matches
        /// SDL_PointInRectFloat's closed semantics: the right/bottom edge is included).
        /// </summary>
        /// <seealso><c>SDL_PointInRectFloat</c></seealso>
        public bool Contains(FPoint point) {
            return point.X >= X && point.X <= X + W && point.Y >= Y && point.Y <= Y + H;
        }

        #endregion

        #region CSDL_IMPL SDL_RectEmptyFloat : SDL_rect#SDL_RectEmptyFloat

        /// <summary>
        /// <c>true</c> if this rect has no area, i.e. width or height is negative. Unlike
        /// <see cref="Rect.IsEmpty"/>, a zero-sized float rect is NOT considered empty
        /// (matches SDL_RectEmptyFloat).
        /// </summary>
        /// <seealso><c>SDL_RectEmptyFloat</c></seealso>
        public bool IsEmpty => W < 0f || H < 0f;

        #endregion

        #region CSDL_IMPL SDL_RectsEqualEpsilon : SDL_rect#SDL_RectsEqualEpsilon

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

        #endregion

        #region CSDL_IMPL SDL_RectsEqualFloat : SDL_rect#SDL_RectsEqualFloat, SDL_RectsEqualEpsilon

        /// <summary>
        /// Compares this rect to <paramref name="other"/> within <see cref="CSDL.Macros.FltEpsilon"/>,
        /// SDL's default tolerance for floating point rect comparisons.
        /// </summary>
        /// <seealso><c>SDL_RectsEqualFloat</c></seealso>
        public bool EqualsApprox(FRect other) {
            return EqualsEpsilon(other, CSDL.Macros.FltEpsilon);
        }

        #endregion

        #region CSDL_IMPL SDL_RectToFRect : SDL_rect#SDL_RectToFRect

        /// <summary>
        ///     Converts a <see cref="Rect" /> to an <see cref="FRect" /> by widening each integer
        ///     component to <see cref="float" />.
        /// </summary>
        public static explicit operator FRect(Rect r) {
            return new FRect(r.X, r.Y, r.W, r.H);
        }

        #endregion

        #region CSDL_IMPL SDL_RECT_CAN_OVERFLOW : SDL_rect_impl#SDL_RECT_CAN_OVERFLOW

        /// <summary>Shared with <see cref="Rect"/> - mirrors SDL_RECT_CAN_OVERFLOW.</summary>
        private static bool RectCanOverflow(in FRect r) {
            const float halfMax = (float)(int.MaxValue / 2);
            const float halfMin = (float)(int.MinValue / 2);
            return r.X <= halfMin ||
                   r.X >= halfMax ||
                   r.Y <= halfMin ||
                   r.Y >= halfMax ||
                   r.W >= halfMax ||
                   r.H >= halfMax;
        }

        #endregion

        #region CSDL_IMPL SDL_HasRectIntersectionFloat : SDL_rect_impl#SDL_HASINTERSECTION, SDL_RECT_CAN_OVERFLOW

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersectionFloat"/>
        public bool Intersects(FRect other) {
            return Intersects(this, other);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersectionFloat"/>
        public static bool Intersects(FRect a, FRect b) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                return false;
            }
            return HasRectIntersection(in a, in b);
        }

        private static bool HasRectIntersection(in FRect a, in FRect b) {
            // Horizontal intersection - float's ENCLOSEPOINTS_EPSILON is 0, unlike Rect's 1.
            float aMin = a.X;
            float aMax = aMin + a.W;
            float bMin = b.X;
            float bMax = bMin + b.W;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax < aMin) {
                return false;
            }

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax < aMin) {
                return false;
            }
            return true;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectIntersectionFloat : SDL_rect_impl#SDL_INTERSECTRECT, SDL_rect_impl#SDL_RECT_CAN_OVERFLOW, SDL_RectEmptyFloat

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectIntersectionFloat"/>
        public static bool GetRectIntersection(FRect a, FRect b, out FRect result) {
            // false just means "these rects don't overlap" (or overflow) - not an SDL error.
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                result = default(FRect);
                return false;
            }

            // Horizontal intersection
            float aMin = a.X;
            float aMax = aMin + a.W;
            float bMin = b.X;
            float bMax = bMin + b.W;
            if (bMin > aMin) {
                aMin = bMin;
            }
            float x = aMin;
            if (bMax < aMax) {
                aMax = bMax;
            }
            float w = aMax - aMin;

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) {
                aMin = bMin;
            }
            float y = aMin;
            if (bMax < aMax) {
                aMax = bMax;
            }
            float h = aMax - aMin;

            result = new FRect(x, y, w, h);
            return !result.IsEmpty;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectUnionFloat : SDL_rect_impl#SDL_UNIONRECT, SDL_rect_impl#SDL_RECT_CAN_OVERFLOW, SDL_RectEmptyFloat

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectUnionFloat"/>
        public static bool Union(FRect a, FRect b, out FRect result) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                result = default(FRect);
                return false;
            }

            // Special cases for empty rects
            if (a.IsEmpty) {
                result = b.IsEmpty ? default(FRect) : b;
                return true;
            }
            if (b.IsEmpty) {
                result = a;
                return true;
            }

            // Horizontal union
            float aMin = a.X;
            float aMax = aMin + a.W;
            float bMin = b.X;
            float bMax = bMin + b.W;
            if (bMin < aMin) {
                aMin = bMin;
            }
            float x = aMin;
            if (bMax > aMax) {
                aMax = bMax;
            }
            float w = aMax - aMin;

            // Vertical union
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin < aMin) {
                aMin = bMin;
            }
            float y = aMin;
            if (bMax > aMax) {
                aMax = bMax;
            }
            float h = aMax - aMin;

            result = new FRect(x, y, w, h);
            return true;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectEnclosingPointsFloat : SDL_rect_impl#SDL_ENCLOSEPOINTS, SDL_RectEmptyFloat

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectEnclosingPointsFloat"/>
        public static bool TryGetEnclosingPoints(FPoint[] points, FRect? clip, out FRect result) {
            result = default(FRect);
            if (points.Length == 0) {
                return false;
            }

            float minX, minY, maxX, maxY;

            if (clip.HasValue) {
                FRect c = clip.Value;
                // Special case for empty rectangle
                if (c.IsEmpty) {
                    return false;
                }

                // Float's ENCLOSEPOINTS_EPSILON is 0, so no "-1" here unlike the int version.
                float clipMinX = c.X;
                float clipMinY = c.Y;
                float clipMaxX = c.X + c.W;
                float clipMaxY = c.Y + c.H;

                bool added = false;
                minX = minY = maxX = maxY = 0f;

                foreach (FPoint p in points) {
                    if (p.X < clipMinX || p.X > clipMaxX || p.Y < clipMinY || p.Y > clipMaxY) {
                        continue;
                    }

                    if (!added) {
                        // First point added
                        minX = maxX = p.X;
                        minY = maxY = p.Y;
                        added = true;
                        continue;
                    }

                    if (p.X < minX) {
                        minX = p.X;
                    } else if (p.X > maxX) {
                        maxX = p.X;
                    }
                    if (p.Y < minY) {
                        minY = p.Y;
                    } else if (p.Y > maxY) {
                        maxY = p.Y;
                    }
                }

                if (!added) {
                    return false;
                }
            } else {
                // No clipping, always add the first point
                minX = maxX = points[0].X;
                minY = maxY = points[0].Y;

                for (int i = 1; i < points.Length; i++) {
                    FPoint p = points[i];
                    if (p.X < minX) {
                        minX = p.X;
                    } else if (p.X > maxX) {
                        maxX = p.X;
                    }
                    if (p.Y < minY) {
                        minY = p.Y;
                    } else if (p.Y > maxY) {
                        maxY = p.Y;
                    }
                }
            }

            result = new FRect(minX, minY, maxX - minX, maxY - minY);
            return true;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectAndLineIntersectionFloat : SDL_rect_impl#SDL_INTERSECTRECTANDLINE, SDL_rect_impl#COMPUTEOUTCODE, SDL_RECT_CAN_OVERFLOW, SDL_RectEmptyFloat

        private const int CodeBottom = 1;
        private const int CodeTop = 2;
        private const int CodeLeft = 4;
        private const int CodeRight = 8;

        private static int ComputeOutCode(in FRect rect, float x, float y) {
            int code = 0;
            if (y < rect.Y) {
                code |= CodeTop;
            } else if (y > rect.Y + rect.H) {
                code |= CodeBottom;
            }
            if (x < rect.X) {
                code |= CodeLeft;
            } else if (x > rect.X + rect.W) {
                code |= CodeRight;
            }
            return code;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectAndLineIntersectionFloat"/>
        public static bool GetRectAndLineIntersection(FRect rect, ref float x1, ref float y1, ref float x2, ref float y2) {
            if (RectCanOverflow(in rect)) {
                return false;
            }
            if (rect.IsEmpty) {
                return false;
            }

            float rectX1 = rect.X;
            float rectY1 = rect.Y;
            float rectX2 = rect.X + rect.W;
            float rectY2 = rect.Y + rect.H;

            // Check to see if entire line is inside rect
            if (x1 >= rectX1 && x1 <= rectX2 && x2 >= rectX1 && x2 <= rectX2 &&
                y1 >= rectY1 && y1 <= rectY2 && y2 >= rectY1 && y2 <= rectY2) {
                return true;
            }

            // Check to see if entire line is to one side of rect
            if (x1 < rectX1 && x2 < rectX1 ||
                x1 > rectX2 && x2 > rectX2 ||
                y1 < rectY1 && y2 < rectY1 ||
                y1 > rectY2 && y2 > rectY2) {
                return false;
            }

            if (y1 == y2) { // Horizontal line, easy to clip
                if (x1 < rectX1) {
                    x1 = rectX1;
                } else if (x1 > rectX2) {
                    x1 = rectX2;
                }
                if (x2 < rectX1) {
                    x2 = rectX1;
                } else if (x2 > rectX2) {
                    x2 = rectX2;
                }
                return true;
            }

            if (x1 == x2) { // Vertical line, easy to clip
                if (y1 < rectY1) {
                    y1 = rectY1;
                } else if (y1 > rectY2) {
                    y1 = rectY2;
                }
                if (y2 < rectY1) {
                    y2 = rectY1;
                } else if (y2 > rectY2) {
                    y2 = rectY2;
                }
                return true;
            }

            // More complicated Cohen-Sutherland algorithm
            int outcode1 = ComputeOutCode(in rect, x1, y1);
            int outcode2 = ComputeOutCode(in rect, x2, y2);
            while (outcode1 != 0 || outcode2 != 0) {
                if ((outcode1 & outcode2) != 0) {
                    return false;
                }

                float x = 0f, y = 0f;
                if (outcode1 != 0) {
                    if ((outcode1 & CodeTop) != 0) {
                        y = rectY1;
                        x = x1 + (float)((double)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode1 & CodeBottom) != 0) {
                        y = rectY2;
                        x = x1 + (float)((double)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode1 & CodeLeft) != 0) {
                        x = rectX1;
                        y = y1 + (float)((double)(y2 - y1) * (x - x1) / (x2 - x1));
                    } else if ((outcode1 & CodeRight) != 0) {
                        x = rectX2;
                        y = y1 + (float)((double)(y2 - y1) * (x - x1) / (x2 - x1));
                    }
                    x1 = x;
                    y1 = y;
                    outcode1 = ComputeOutCode(in rect, x, y);
                } else {
                    if ((outcode2 & CodeTop) != 0) {
                        y = rectY1;
                        x = x1 + (float)((double)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode2 & CodeBottom) != 0) {
                        y = rectY2;
                        x = x1 + (float)((double)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode2 & CodeLeft) != 0) {
                        x = rectX1;
                        y = y1 + (float)((double)(y2 - y1) * (x - x1) / (x2 - x1));
                    } else if ((outcode2 & CodeRight) != 0) {
                        x = rectX2;
                        y = y1 + (float)((double)(y2 - y1) * (x - x1) / (x2 - x1));
                    }
                    x2 = x;
                    y2 = y;
                    outcode2 = ComputeOutCode(in rect, x, y);
                }
            }
            return true;
        }

        #endregion
    }
}
