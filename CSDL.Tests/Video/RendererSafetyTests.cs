// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSDL.Video;

namespace CSDL3.Tests.Video {
    public sealed class RendererSafetyTests {
        [Fact]
        public void RenderGeometry_RejectsVertexCountBeyondArrayLength() {
            Renderer renderer = CreateUninitializedRenderer();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                renderer.RenderGeometry(null, new Vertex[1], 2));
        }

        [Fact]
        public void RenderGeometryRaw_RejectsBuffersTooSmallForStride() {
            Renderer renderer = CreateUninitializedRenderer();

            Assert.Throws<ArgumentException>(() => renderer.RenderGeometryRaw(
                null,
                new float[2], sizeof(float) * 2,
                new FColor[1], Unsafe.SizeOf<FColor>(),
                new float[2], sizeof(float) * 2,
                2,
                indices: Array.Empty<int>()));
        }

        [Fact]
        public void RenderGeometryRaw_RejectsIndexCountBeyondArrayLength() {
            Renderer renderer = CreateUninitializedRenderer();

            Assert.Throws<ArgumentOutOfRangeException>(() => renderer.RenderGeometryRaw(
                null,
                new float[4], sizeof(float) * 2,
                new FColor[2], Unsafe.SizeOf<FColor>(),
                new float[4], sizeof(float) * 2,
                2,
                new int[1],
                2));
        }

        [Fact]
        public void ProcAddressApis_ReturnNullableNativeAddresses() {
            Assert.Equal(typeof(IntPtr?), GetReturnType(typeof(GL)));
            Assert.Equal(typeof(IntPtr?), GetReturnType(typeof(EGL)));
        }

        private static Renderer CreateUninitializedRenderer() {
            return (Renderer)RuntimeHelpers.GetUninitializedObject(typeof(Renderer));
        }

        private static Type GetReturnType(Type type) {
            MethodInfo method = type.GetMethod(nameof(GL.GetProcAddress), BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            return method.ReturnType;
        }
    }
}
