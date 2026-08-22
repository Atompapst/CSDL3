// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Diagnostics.CodeAnalysis;
namespace CSDL.Video {

    /// <summary>
    /// Defines a renderable vertex with position, color, and texture coordinates.
    /// </summary>
    public partial struct Vertex : IEquatable<Vertex> {

        public Vertex(FPoint position, FColor color) : this(position, color, default) { }


        public override string ToString() {
            return $"Vertex(Pos:{Position}, Col:{Color}, UV:{TexCoord})";
        }

        public static bool operator ==(Vertex left, Vertex right) {
            return left.Equals(right);
        }

        public static bool operator !=(Vertex left, Vertex right) {
            return !(left == right);
        }

        public override bool Equals([NotNullWhen(true)] object? obj) {
            return base.Equals(obj);
        }
        public bool Equals(Vertex other) {
            return Position.Equals(other.Position) && Color.Equals(other.Color) && TexCoord.Equals(other.TexCoord);
            
        }
        public override int GetHashCode() {
            return HashCode.Combine(Position, Color, TexCoord);
        }

        /// <summary>
        /// Fluent builder for vertex arrays and optional index buffers.
        /// </summary>
        public sealed class Builder {
            private readonly System.Collections.Generic.List<Vertex> _vertices = new System.Collections.Generic.List<Vertex>();
            private readonly System.Collections.Generic.List<int> _indices = new System.Collections.Generic.List<int>();

            /// <summary>
            /// Number of vertices currently in the builder.
            /// </summary>
            public int VertexCount => _vertices.Count;

            /// <summary>
            /// Number of indices currently in the builder.
            /// </summary>
            public int IndexCount => _indices.Count;

            /// <summary>
            /// Remove all vertices and indices.
            /// </summary>
            public Builder Clear() {
                _vertices.Clear();
                _indices.Clear();
                return this;
            }

            /// <summary>
            /// Append a vertex and return the builder for chaining.
            /// </summary>
            public Builder AddVertex(FPoint position, FColor color, FPoint texCoord = default) {
                _vertices.Add(new Vertex(position, color, texCoord));
                return this;
            }

            /// <summary>
            /// Append a triangle index triplet.
            /// </summary>
            public Builder AddTriangle(int i0, int i1, int i2) {
                _indices.Add(i0);
                _indices.Add(i1);
                _indices.Add(i2);
                return this;
            }

            /// <summary>
            /// Append two triangles forming a quad.
            /// </summary>
            public Builder AddQuad(int i0, int i1, int i2, int i3) {
                AddTriangle(i0, i1, i2);
                AddTriangle(i0, i2, i3);
                return this;
            }

            /// <summary>
            /// Build vertices and indices arrays for renderer submission.
            /// </summary>
            public void Build(out Vertex[] vertices, out int[]? indices) {
                vertices = _vertices.ToArray();
                indices = _indices.Count > 0 ? _indices.ToArray() : null;
            }
        }
    }
}
