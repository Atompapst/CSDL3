// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {

    public partial struct Rect {

        #region CSDL_IMPL SDL_PointInRect : SDL_rect#SDL_PointInRect
        //TODO generate Docs for inline methods
        /// <summary>
        /// <c>true</c> if <paramref name="point"/> lies within this rect (matches SDL_PointInRect's
        /// half-open semantics: the right/bottom edge is excluded).
        /// </summary>
        /// <seealso><c>SDL_PointInRect</c></seealso>
        public bool Contains(Point point) {
            return point.X >= X && point.X < X + W && point.Y >= Y && point.Y < Y + H;
        }

        #endregion

        #region CSDL_IMPL SDL_RectEmpty : SDL_rect#SDL_RectEmpty

        /// <summary>
        /// <c>true</c> if this rect has no area, i.e. width or height is zero or negative.
        /// </summary>
        /// <seealso><c>SDL_RectEmpty</c></seealso>
        public bool IsEmpty => W <= 0 || H <= 0;

        #endregion

        #region CSDL_IMPL SDL_RectsEqual : SDL_rect#SDL_RectsEqual

        public bool Equals(Rect other) {
            return X == other.X && Y == other.Y && W == other.W && H == other.H;
        }

        #endregion

        #region CSDL_IMPL SDL_RECT_CAN_OVERFLOW : SDL_rect_impl#SDL_RECT_CAN_OVERFLOW

        /// <summary>Shared with <see cref="FRect"/> - mirrors SDL_RECT_CAN_OVERFLOW.</summary>
        internal static bool RectCanOverflow(in Rect r) {
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
            if (RectCanOverflow(in this) || RectCanOverflow(in other)) {
                return false;
            }
            return HasRectIntersection(in this, in other);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersection"/>
        public bool HasIntersectionUnchecked(in Rect other) {
            return HasRectIntersection(in this, in other);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.HasRectIntersection"/>
        public static bool HasIntersection(in Rect a, in Rect b) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                return false;
            }
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
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
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
            if (aMax - 1 < aMin) {
                return false;
            }
            return true;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectIntersection : SDL_rect_impl#SDL_INTERSECTRECT, SDL_RECT_CAN_OVERFLOW, SDL_RectEmpty

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectIntersection"/>
        public static bool GetRectIntersection(in Rect a, in Rect b, out Rect result) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                result = default(Rect);
                return false;
            }

            // Horizontal intersection
            int aMin = a.X;
            int aMax = aMin + a.W;
            int bMin = b.X;
            int bMax = bMin + b.W;
            if (bMin > aMin) {
                aMin = bMin;
            }
            int x = aMin;
            if (bMax < aMax) {
                aMax = bMax;
            }
            int w = aMax - aMin;

            // Vertical intersection
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) {
                aMin = bMin;
            }
            int y = aMin;
            if (bMax < aMax) {
                aMax = bMax;
            }
            int h = aMax - aMin;

            result = new Rect(x, y, w, h);
            return !result.IsEmpty;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectUnion : SDL_rect_impl#SDL_UNIONRECT, SDL_RECT_CAN_OVERFLOW, SDL_RectEmpty

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectUnion"/>
        public static bool TryUnion(Rect a, Rect b, out Rect result) {
            if (RectCanOverflow(in a) || RectCanOverflow(in b)) {
                result = default(Rect);
                return false;
            }

            // Special cases for empty rects
            if (a.IsEmpty) {
                result = b.IsEmpty ? default(Rect) : b;
                return true;
            }
            if (b.IsEmpty) {
                result = a;
                return true;
            }

            // Horizontal union
            int aMin = a.X;
            int aMax = aMin + a.W;
            int bMin = b.X;
            int bMax = bMin + b.W;
            if (bMin < aMin) {
                aMin = bMin;
            }
            int x = aMin;
            if (bMax > aMax) {
                aMax = bMax;
            }
            int w = aMax - aMin;

            // Vertical union
            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin < aMin) {
                aMin = bMin;
            }
            int y = aMin;
            if (bMax > aMax) {
                aMax = bMax;
            }
            int h = aMax - aMin;

            result = new Rect(x, y, w, h);
            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectUnion"/>
        public static Rect Union(Rect a, Rect b) {
            TryUnion(a, b, out Rect result);
            return result;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectEnclosingPoints : SDL_rect_impl#SDL_ENCLOSEPOINTS, SDL_RectEmpty

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectEnclosingPoints"/>
        public static bool TryGetEnclosingPoints(Point[] points, Rect? clip, out Rect result) {
            result = default(Rect);
            if (points.Length == 0) {
                return false;
            }

            int minX, minY, maxX, maxY;

            if (clip.HasValue) {
                Rect c = clip.Value;
                // Special case for empty rectangle
                if (c.IsEmpty) {
                    return false;
                }

                int clipMinX = c.X;
                int clipMinY = c.Y;
                int clipMaxX = c.X + c.W - 1;
                int clipMaxY = c.Y + c.H - 1;

                bool added = false;
                minX = minY = maxX = maxY = 0;

                foreach (Point p in points) {
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
                    Point p = points[i];
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

            result = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            return true;
        }

        #endregion

        #region CSDL_IMPL SDL_GetSpanEnclosingRect : SDL_rect#SDL_GetSpanEnclosingRect

        /// <summary>
        /// Merges <paramref name="rects"/> into the single vertical span - x=0, width=<paramref name="width"/> -
        /// that encloses all of their y-ranges, clamped to [0, <paramref name="height"/>]. Not part of the
        /// public SDL API; SDL uses this internally to compute damage/update spans for a window.
        /// </summary>
        /// <seealso><c>SDL_GetSpanEnclosingRect</c></seealso>
        public static bool TryGetEnclosingSpan(int width, int height, Rect[] rects, out Rect span) {
            span = default(Rect);
            if (width < 1 || height < 1 || rects.Length < 1) {
                return false;
            }

            int spanY1 = height;
            int spanY2 = 0;

            foreach (Rect rect in rects) {
                int rectY1 = rect.Y;
                int rectY2 = rectY1 + rect.H;

                if (rectY1 < 0) {
                    spanY1 = 0;
                } else if (rectY1 < spanY1) {
                    spanY1 = rectY1;
                }

                if (rectY2 > height) {
                    spanY2 = height;
                } else if (rectY2 > spanY2) {
                    spanY2 = rectY2;
                }
            }

            if (spanY2 <= spanY1) {
                return false;
            }

            span = new Rect(0, spanY1, width, spanY2 - spanY1);
            return true;
        }

        #endregion

        #region CSDL_IMPL SDL_GetRectAndLineIntersection : SDL_rect_impl#SDL_INTERSECTRECTANDLINE, SDL_rect_impl#COMPUTEOUTCODE, SDL_RECT_CAN_OVERFLOW, SDL_RectEmpty

        private const int CodeBottom = 1;
        private const int CodeTop = 2;
        private const int CodeLeft = 4;
        private const int CodeRight = 8;

        private static int ComputeOutCode(in Rect rect, int x, int y) {
            int code = 0;
            if (y < rect.Y) {
                code |= CodeTop;
            } else if (y > rect.Y + rect.H - 1) {
                code |= CodeBottom;
            }
            if (x < rect.X) {
                code |= CodeLeft;
            } else if (x > rect.X + rect.W - 1) {
                code |= CodeRight;
            }
            return code;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectAndLineIntersection"/>
        public static bool GetRectAndLineIntersection(in Rect rect, ref int x1, ref int y1, ref int x2, ref int y2) {
            if (RectCanOverflow(in rect)) {
                return false;
            }
            if (rect.IsEmpty) {
                return false;
            }

            int rectX1 = rect.X;
            int rectY1 = rect.Y;
            int rectX2 = rect.X + rect.W - 1;
            int rectY2 = rect.Y + rect.H - 1;

            // Check to see if entire line is inside rect
            if (x1 >= rectX1 && x1 <= rectX2 && x2 >= rectX1 && x2 <= rectX2 &&
                y1 >= rectY1 && y1 <= rectY2 && y2 >= rectY1 && y2 <= rectY2) {
                return true;
            }

            // Check to see if entire line is to one side of rect
            if (x1 < rectX1 && x2 < rectX1 || x1 > rectX2 && x2 > rectX2 ||
                y1 < rectY1 && y2 < rectY1 || y1 > rectY2 && y2 > rectY2) {
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

                int x = 0, y = 0;
                if (outcode1 != 0) {
                    if ((outcode1 & CodeTop) != 0) {
                        y = rectY1;
                        x = x1 + (int)((long)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode1 & CodeBottom) != 0) {
                        y = rectY2;
                        x = x1 + (int)((long)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode1 & CodeLeft) != 0) {
                        x = rectX1;
                        y = y1 + (int)((long)(y2 - y1) * (x - x1) / (x2 - x1));
                    } else if ((outcode1 & CodeRight) != 0) {
                        x = rectX2;
                        y = y1 + (int)((long)(y2 - y1) * (x - x1) / (x2 - x1));
                    }
                    x1 = x;
                    y1 = y;
                    outcode1 = ComputeOutCode(in rect, x, y);
                } else {
                    if ((outcode2 & CodeTop) != 0) {
                        y = rectY1;
                        x = x1 + (int)((long)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode2 & CodeBottom) != 0) {
                        y = rectY2;
                        x = x1 + (int)((long)(x2 - x1) * (y - y1) / (y2 - y1));
                    } else if ((outcode2 & CodeLeft) != 0) {
                        x = rectX1;
                        y = y1 + (int)((long)(y2 - y1) * (x - x1) / (x2 - x1));
                    } else if ((outcode2 & CodeRight) != 0) {
                        x = rectX2;
                        y = y1 + (int)((long)(y2 - y1) * (x - x1) / (x2 - x1));
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
