// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    public partial struct Line {
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

        /// <inheritdoc cref="Rect.GetIntersection(Line,out Line)"/>
        public bool GetIntersection(Rect rect, out Line res) {
            return rect.GetIntersection(this, out res);
        }

        /// <inheritdoc cref="FRect.GetIntersection(Line,out Line)"/>
        public bool GetIntersection(FRect rect, out Line res) {
            return rect.GetIntersection(this, out res);
        }
    }
}
