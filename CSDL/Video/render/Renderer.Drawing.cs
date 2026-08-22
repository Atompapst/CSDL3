// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;
using CSDL.Extensions;

namespace CSDL.Video {
    public partial class Renderer {
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderFillRect"/>
        public bool RenderFillRect(FRect rect) {
            return SDL.RenderFillRect(Handle, in rect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderFillRects"/>
        public bool RenderFillRects(FRect[] rects) {
            if (rects == null || rects.Length <= 0) return true;
            unsafe {
                fixed (FRect* fr = rects) {
                    return SDL.RenderFillRects(Handle, fr, rects.Length).LogIfFalse();
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderRect"/>
        public bool RenderRect(FRect rect) {
            return SDL.RenderRect(Handle.Ptr, rect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderRects"/>
        public bool RenderRects(FRect[] rects) {
            if (rects == null || rects.Length <= 0) return true;
            unsafe {
                fixed (FRect* fr = rects) {
                    return SDL.RenderRects(Handle, fr, rects.Length).LogIfFalse();
                }
            }
        }

        /// <summary>
        /// Render geometry using a Geometry object.
        /// </summary>
        public bool RenderGeometry(Texture texture, Geometry geometry) {
            ArgumentNullException.ThrowIfNull(geometry);
            if (geometry.Vertices == null || geometry.VertexCount == 0) return true;

            return RenderGeometry(texture, geometry.Vertices, geometry.VertexCount, geometry.Indices, geometry.HasIndices ? geometry.IndexCount : 0);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderGeometry"/>
        public bool RenderGeometry(Texture? texture, Vertex[] vertices, int[]? indices = null) {
            if (vertices == null || vertices.Length == 0) return true;

            int numIndices = indices?.Length ?? 0;
            return RenderGeometry(texture, vertices, vertices.Length, indices, numIndices);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderGeometry"/>
        public bool RenderGeometry(Texture? texture, Vertex[] vertices, int numVertices, int[]? indices = null, int numIndices = 0) {
            ArgumentNullException.ThrowIfNull(vertices);
            ArgumentOutOfRangeException.ThrowIfNegative(numVertices);
            ArgumentOutOfRangeException.ThrowIfNegative(numIndices);
            if (numVertices == 0) return true;
            if (numVertices > vertices.Length) {
                throw new ArgumentOutOfRangeException(nameof(numVertices), "The requested vertex count exceeds the vertex array length.");
            }
            if (indices == null && numIndices != 0) {
                throw new ArgumentException("An index array is required when numIndices is non-zero.", nameof(indices));
            }
            if (indices != null && numIndices > indices.Length) {
                throw new ArgumentOutOfRangeException(nameof(numIndices), "The requested index count exceeds the index array length.");
            }
            unsafe {
                fixed (Vertex* v = vertices) {
                    if (indices != null && numIndices > 0) {
                        fixed (int* i = indices) {
                            return SDL.RenderGeometry(Handle, texture?.Handle ?? IntPtr.Zero, v, numVertices, i, numIndices).LogIfFalse();
                        }
                    } else {
                        return SDL.RenderGeometry(Handle, texture?.Handle ?? IntPtr.Zero, v, numVertices, null, 0).LogIfFalse();
                    }
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderGeometryRaw"/>
        public bool RenderGeometryRaw(Texture? gpuTexture, float[] xy, int xyStride, FColor[] colors, int colorStride, float[] uv, int uvStride, int numVertices, byte[]? indices = null, int numIndices = 0, int sizeIndices = 4) {
            ValidateRawGeometryInput(xy, xyStride, colors, colorStride, uv, uvStride, numVertices);
            ValidateIndexBuffer(indices, numIndices, sizeIndices);
            if (numVertices == 0) return true;
            unsafe {
                fixed (byte* pIndices = indices) {
                    return RenderGeometryRawCore(gpuTexture, xy, xyStride, colors, colorStride, uv, uvStride, numVertices, (IntPtr)pIndices, numIndices, sizeIndices);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderGeometryRaw"/>
        public bool RenderGeometryRaw(Texture? gpuTexture, float[] xy, int xyStride, FColor[] colors, int colorStride, float[] uv, int uvStride, int numVertices, short[] indices, int numIndices) {
            ValidateRawGeometryInput(xy, xyStride, colors, colorStride, uv, uvStride, numVertices);
            ValidateIndexBuffer(indices, numIndices, sizeof(short));
            if (numVertices == 0) return true;
            unsafe {
                fixed (short* pIndices = indices) {
                    return RenderGeometryRawCore(gpuTexture, xy, xyStride, colors, colorStride, uv, uvStride, numVertices, (IntPtr)pIndices, numIndices, 2);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderGeometryRaw"/>
        public bool RenderGeometryRaw(Texture? gpuTexture, float[] xy, int xyStride, FColor[] colors, int colorStride, float[] uv, int uvStride, int numVertices, int[]? indices = null, int numIndices = 0) {
            ValidateRawGeometryInput(xy, xyStride, colors, colorStride, uv, uvStride, numVertices);
            ValidateIndexBuffer(indices, numIndices, sizeof(int));
            if (numVertices == 0) return true;

            if (indices != null && numIndices > 0) {
                unsafe {
                    fixed (int* pIndices = indices) {
                        return RenderGeometryRawCore(gpuTexture, xy, xyStride, colors, colorStride, uv, uvStride, numVertices, (IntPtr)pIndices, numIndices, 4);
                    }
                }
            } else {
                return RenderGeometryRawCore(gpuTexture, xy, xyStride, colors, colorStride, uv, uvStride, numVertices, IntPtr.Zero, 0, 4);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderGeometryRaw"/>
        private bool RenderGeometryRawCore(Texture? gpuTexture, float[] xy, int xyStride, FColor[] colors, int colorStride, float[] uv, int uvStride, int numVertices, IntPtr pIndices, int numIndices, int sizeIndices) {
            unsafe {
                fixed (float* f = xy) {
                    fixed (FColor* c = colors) {
                        fixed (float* u = uv) {
                            return SDL.RenderGeometryRaw(Handle.Ptr, gpuTexture?.Handle.Ptr ?? IntPtr.Zero, f, xyStride, c, colorStride, u, uvStride, numVertices, pIndices, numIndices, sizeIndices).LogIfFalse();
                        }
                    }
                }
            }
        }

        private static void ValidateRawGeometryInput(float[] xy, int xyStride, FColor[] colors, int colorStride, float[] uv, int uvStride, int numVertices) {
            ArgumentNullException.ThrowIfNull(xy);
            ArgumentNullException.ThrowIfNull(colors);
            ArgumentNullException.ThrowIfNull(uv);
            ArgumentOutOfRangeException.ThrowIfNegative(numVertices);
            if (numVertices == 0) return;

            ValidateVertexBuffer(xy, xyStride, numVertices, Unsafe.SizeOf<float>() * 2, nameof(xy));
            ValidateVertexBuffer(colors, colorStride, numVertices, Unsafe.SizeOf<FColor>(), nameof(colors));
            ValidateVertexBuffer(uv, uvStride, numVertices, Unsafe.SizeOf<float>() * 2, nameof(uv));
        }

        private static void ValidateVertexBuffer<T>(T[] values, int stride, int count, int elementSize, string parameterName) where T : unmanaged {
            if (stride < elementSize) {
                throw new ArgumentOutOfRangeException(parameterName, "The stride is smaller than one vertex element.");
            }

            long requiredBytes = ((long)count - 1) * stride + elementSize;
            long availableBytes = (long)values.Length * Unsafe.SizeOf<T>();
            if (requiredBytes > availableBytes) {
                throw new ArgumentException("The array is too small for the requested count and stride.", parameterName);
            }
        }

        private static void ValidateIndexBuffer<T>(T[]? indices, int count, int indexSize) where T : unmanaged {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (indexSize is not (1 or 2 or 4)) {
                throw new ArgumentOutOfRangeException(nameof(indexSize), "SDL only supports index sizes of 1, 2, or 4 bytes.");
            }
            if (indices == null) {
                if (count != 0) {
                    throw new ArgumentException("An index array is required when numIndices is non-zero.", nameof(indices));
                }
                return;
            }

            if ((long)count * indexSize > (long)indices.Length * Unsafe.SizeOf<T>()) {
                throw new ArgumentOutOfRangeException(nameof(count), "The requested index count exceeds the index array length.");
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderLine"/>
        public bool RenderLine(Line line) {
            return SDL.RenderLine(Handle, line.X1, line.Y1, line.X2, line.Y2).LogIfFalse();
        }

        /// <summary>
        /// Renders a batch of independent line segments. Unlike <see cref="RenderLineStrip"/>,
        /// consecutive lines are not connected to one another.
        /// </summary>
        public bool RenderLines(Line[] lines) {
            if (lines == null || lines.Length == 0) return true;
            bool ok = true;
            for (int i = 0; i < lines.Length; i++) {
                ok &= RenderLine(lines[i]);
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderLines"/>
        public bool RenderLineStrip(FPoint[] points) {
            if (points == null || points.Length == 0) return true;
            unsafe {
                fixed (FPoint* p = points) {
                    return SDL.RenderLines(Handle, p, points.Length).LogIfFalse();
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderPoint"/>
        public bool RenderPoint(Point point) {
            return SDL.RenderPoint(Handle, point.X, point.Y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderPoint"/>
        public bool RenderPoint(FPoint point) {
            return SDL.RenderPoint(Handle, point.X, point.Y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderPoints"/>
        public bool Render(Point[] points) {
            if (points == null || points.Length <= 0) return true;
            FPoint[] fp = new FPoint[points.Length];
            for (int i = 0; i < points.Length; i++) {
                fp[i] = new FPoint(points[i].X, points[i].Y);
            }
            return RenderPoints(fp, fp.Length);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderPoints"/>
        public bool RenderPoints(FPoint[] points) {
            if (points == null || points.Length <= 0) return true;
            return RenderPoints(points, points.Length);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderPoints"/>
        private bool RenderPoints(FPoint[] points, int count) {
            unsafe {
                fixed (FPoint* p = points) {
                    return SDL.RenderPoints(Handle, p, count).LogIfFalse();
                }
            }
        }
    }
}
