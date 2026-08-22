// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    public struct Line {
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }

        public FPoint Start {
            get => new FPoint(X1, Y1);
            set {
                X1 = value.X;
                Y1 = value.Y;
            }
        }

        public FPoint End {
            get => new FPoint(X2, Y2);
            set {
                X2 = value.X;
                Y2 = value.Y;
            }
        }

        public Line(FPoint start, FPoint end) {
            Start = start;
            End = end;
        }

        public Line(float x1, float y1, float x2, float y2) {
            Start = new FPoint(x1, y1);
            End = new FPoint(x2, y2);
        }

        public float Length => PointMath.Magnitude(X2 - X1, Y2 - Y1);

        public FPoint[] ToPointArray() {
            return new FPoint[] { Start, End };
        }

        public override string ToString() {
            return $"Line(({X1}, {Y1}) -> ({X2}, {Y2}))";
        }

        public static explicit operator (FPoint, FPoint)(Line l) {
            return (l.Start, l.End);
        }
        public static explicit operator Line((FPoint, FPoint) tuple) {
            return new Line(tuple.Item1, tuple.Item2);
        }

    }

    public static class LineUtils {
        /// <summary>
        /// Clips <paramref name="line"/> to <paramref name="rect"/>, writing the clipped segment to
        /// <paramref name="res"/> and returning whether it intersects at all.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Rect.GetRectAndLineIntersection"/>
        public static bool GetIntersection(this Rect rect, Line line, out Line res) {
            int x1 = (int)line.X1, y1 = (int)line.Y1, x2 = (int)line.X2, y2 = (int)line.Y2;
            bool intersects = SDL.GetRectAndLineIntersection(in rect, ref x1, ref y1, ref x2, ref y2);
            res = intersects ? new Line(x1, y1, x2, y2) : default;
            return intersects;
        }

        /// <inheritdoc cref="GetIntersection(Rect,Line,out Line)"/>
        public static bool GetIntersection(this FRect rect, Line line, out Line res) {
            float x1 = line.X1, y1 = line.Y1, x2 = line.X2, y2 = line.Y2;
            bool intersects = SDL.GetRectAndLineIntersectionFloat(in rect, ref x1, ref y1, ref x2, ref y2);
            res = intersects ? new Line(x1, y1, x2, y2) : default;
            return intersects;
        }
    }
}
