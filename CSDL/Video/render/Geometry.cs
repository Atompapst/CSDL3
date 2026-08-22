// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;

namespace CSDL.Video {
    /// <summary>
    /// Encapsulates geometry data (vertices and indices) for rendering.
    /// </summary>
    public class Geometry {
        public Vertex[] Vertices { get; }
        public int[]? Indices { get; }
        public int VertexCount => Vertices?.Length ?? 0;
        public int IndexCount => Indices?.Length ?? 0;
        public bool HasIndices => Indices != null && Indices.Length > 0;

        public Geometry(Vertex[] vertices, int[]? indices = null) {
            Vertices = vertices ?? throw new SDLException(nameof(vertices));
            Indices = indices;
        }

        /// <summary>
        /// Create geometry from vertices only (no indices).
        /// </summary>
        public static Geometry FromVertices(params Vertex[] vertices) {
            return new Geometry(vertices);
        }

        /// <summary>
        /// Create indexed geometry.
        /// </summary>
        public static Geometry FromIndexed(Vertex[] vertices, int[] indices) {
            return new Geometry(vertices, indices);
        }

        /// <summary>
        /// Create a quad (2 triangles).
        /// </summary>
        public static Geometry CreateQuad(FRect rect, FColor color) {
            Vertex[] vertices = new[] {
                new Vertex(new FPoint(rect.X, rect.Y), color, new FPoint(0, 0)),
                new Vertex(new FPoint(rect.X + rect.W, rect.Y), color, new FPoint(1, 0)),
                new Vertex(new FPoint(rect.X + rect.W, rect.Y + rect.H), color, new FPoint(1, 1)),
                new Vertex(new FPoint(rect.X, rect.Y + rect.H), color, new FPoint(0, 1)),
            };
            int[] indices = new[] { 0, 1, 2, 0, 2, 3 };
            return new Geometry(vertices, indices);
        }

        /// <summary>
        /// Create a simple triangle.
        /// </summary>
        public static Geometry CreateTriangle(FPoint p0, FPoint p1, FPoint p2, FColor c0, FColor c1, FColor c2) {
            return new Geometry(new[] {
                new Vertex(p0, c0, new FPoint(0, 0)),
                new Vertex(p1, c1, new FPoint(1, 0)),
                new Vertex(p2, c2, new FPoint(0.5f, 1)),
            }, new[] { 0, 1, 2 });
        }

        /// <summary>
        /// Create a colored diamond.
        /// </summary>
        public static Geometry CreateDiamond(FPoint center, float width, float height, FColor color) {
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            Vertex[] vertices = new[] {
                new Vertex(new FPoint(center.X, center.Y - hh), color, new FPoint(0.5f, 0)),
                new Vertex(new FPoint(center.X + hw, center.Y), color, new FPoint(1, 0.5f)),
                new Vertex(new FPoint(center.X, center.Y + hh), color, new FPoint(0.5f, 1)),
                new Vertex(new FPoint(center.X - hw, center.Y), color, new FPoint(0, 0.5f)),
            };

            int[] indices = new[] { 0, 1, 2, 0, 2, 3 };
            return new Geometry(vertices, indices);
        }

        /// <summary>
        /// Create a simple house shape. :)
        /// </summary>
        public static Geometry CreateHouse(FRect rect, FColor wallColor, FColor roofColor, FColor doorColor) {
            float x = rect.X;
            float y = rect.Y;
            float w = rect.W;
            float h = rect.H;

            Vertex[] vertices = new[] {
                new Vertex(new FPoint(x, y + h * 0.35f), wallColor),
                new Vertex(new FPoint(x + w, y + h * 0.35f), wallColor),
                new Vertex(new FPoint(x + w, y + h), wallColor),
                new Vertex(new FPoint(x, y + h), wallColor),

                new Vertex(new FPoint(x, y + h * 0.35f), roofColor),
                new Vertex(new FPoint(x + w * 0.5f, y), roofColor),
                new Vertex(new FPoint(x + w, y + h * 0.35f), roofColor),

                new Vertex(new FPoint(x + w * 0.4f, y + h * 0.65f), doorColor),
                new Vertex(new FPoint(x + w * 0.6f, y + h * 0.65f), doorColor),
                new Vertex(new FPoint(x + w * 0.6f, y + h), doorColor),
                new Vertex(new FPoint(x + w * 0.4f, y + h), doorColor),
            };

            int[] indices = new[] {
                0, 1, 2, 0, 2, 3,
                4, 5, 6,
                7, 8, 9, 7, 9, 10,
            };

            return new Geometry(vertices, indices);
        }

        /// <summary>
        /// Create a star-like polygon.
        /// </summary>
        public static Geometry CreateStar(FPoint center, float outerRadius, float innerRadius, FColor color) {
            Vertex[] vertices = new[] {
                new Vertex(new FPoint(center.X, center.Y - outerRadius), color),
                new Vertex(new FPoint(center.X + innerRadius, center.Y - innerRadius), color),
                new Vertex(new FPoint(center.X + outerRadius, center.Y), color),
                new Vertex(new FPoint(center.X + innerRadius, center.Y + innerRadius), color),
                new Vertex(new FPoint(center.X, center.Y + outerRadius), color),
                new Vertex(new FPoint(center.X - innerRadius, center.Y + innerRadius), color),
                new Vertex(new FPoint(center.X - outerRadius, center.Y), color),
                new Vertex(new FPoint(center.X - innerRadius, center.Y - innerRadius), color),
            };

            int[] indices = new[] {
                0, 1, 7,
                1, 2, 3,
                1, 3, 7,
                7, 3, 5,
                3, 4, 5,
                5, 6, 7,
            };

            return new Geometry(vertices, indices);
        }

        public class Builder {
            private readonly List<Vertex> _vertices = new List<Vertex>();
            private readonly List<int> _indices = new List<int>();

            public int VertexCount => _vertices.Count;

            public Builder Clear() {
                _vertices.Clear();
                _indices.Clear();
                return this;
            }

            public Builder AddVertex(FPoint position, FColor color, FPoint texCoord = default) {
                _vertices.Add(new Vertex(position, color, texCoord));
                return this;
            }

            public Builder AddTriangle(int i0, int i1, int i2) {
                _indices.Add(i0);
                _indices.Add(i1);
                _indices.Add(i2);
                return this;
            }

            public Builder AddQuad(int i0, int i1, int i2, int i3) {
                AddTriangle(i0, i1, i2);
                AddTriangle(i0, i2, i3);
                return this;
            }

            public Geometry Build() {
                return new Geometry(_vertices.ToArray(), _indices.Count > 0 ? _indices.ToArray() : null);
            }
        }

        public static Builder Create() {
            return new Builder();
        }
    }
}
