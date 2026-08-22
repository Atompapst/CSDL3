// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.GPU;
using System;
using System.Buffers;
namespace CSDL.Extensions {
    internal static class ArrayExtensions {

        private const int StackLimit = 64;

        internal unsafe delegate void PointerArrayAction(nint* ptr, uint count);

        internal static void WithPointers<T>(this T[]? items, PointerArrayAction action)
            where T : class, INativeHandle {
            if (items == null || items.Length == 0) {
                return;
            }

            int count = items.Length;

            if (count <= StackLimit) {
                Span<nint> raw = stackalloc nint[count];

                FillPointers(items, raw);

                unsafe {
                    fixed (nint* ptr = raw) {
                        action(ptr, (uint)count);
                    }
                }
                return;
            }

            nint[] rented = ArrayPool<nint>.Shared.Rent(count);

            try {
                Span<nint> raw = rented.AsSpan(0, count);

                FillPointers(items, raw);

                unsafe {
                    fixed (nint* ptr = raw) {
                        action(ptr, (uint)count);
                    }
                }
            }
            finally {
                ArrayPool<nint>.Shared.Return(rented, false);
            }
        }

        private static void FillPointers<T>(T[] items, Span<nint> destination)
            where T : class, INativeHandle {
            for (int i = 0; i < items.Length; i++) {
                destination[i] = items[i].NativePointer;
            }
        }

        internal static NativePtr<T> ToUnmanaged<T>(this T item) where T : unmanaged {
            NativePtr<T> ptr = Memory.Malloc<T>();
            ptr.Write(item);
            return ptr;
        }

        internal static NativePtr<T> ToUnmanaged<T>(this T[]? items) where T : unmanaged {
            if (items == null || items.Length <= 0) {
                return NativePtr<T>.Zero;
            }

            NativePtr<T> ptr = Memory.MallocArray<T>(items.Length);
            items.AsSpan().CopyTo(ptr.AsSpan(items.Length));
            return ptr;
        }

        internal static T[] ToManaged<T>(this NativePtr<T> ptr, int length) where T : unmanaged {
            T[] managedArray = new T[length];
            ptr.AsReadOnlySpan(length).CopyTo(managedArray);
            return managedArray;
        }

        internal static IntPtr[] GetRaw(this GPUBuffer[] items) {
            return items.GetRaw(p => p.Handle.Ptr);
        }

        internal static IntPtr[] GetRaw(this GPUTexture[] items) {
            return items.GetRaw(p => p.Handle.Ptr);
        }

        internal static IntPtr[] GetRaw(this GPUFence[] items) {
            return items.GetRaw(p => p.Handle.Ptr);
        }

        internal static GPUTextureSamplerBinding[] Combine(this GPUTexture[] items, GPUSampler[] samplers) {
            return items.GetRaw(samplers, (t, s) => new GPUTextureSamplerBinding { Texture = t.Handle.Ptr, Sampler = s.Handle.Ptr });
        }

        internal static GPUTextureSamplerBinding[] Combine(this GPUSampler[] items, GPUTexture[] textures) {
            return textures.Combine(items);
        }

        private static TOut[] GetRaw<TIn, TOut>(this TIn[]? items, Func<TIn, TOut> converter) {
            if (items == null) return Array.Empty<TOut>();
            TOut[] result = new TOut[items.Length];
            for (int i = 0; i < items.Length; i++) {
                result[i] = converter(items[i]);
            }
            return result;
        }

        private static TOut[] GetRaw<TIn1, TIn2, TOut>(this TIn1[]? items, TIn2[] items2, Func<TIn1, TIn2, TOut> converter) {
            if (items == null) return Array.Empty<TOut>();
            TOut[] result = new TOut[items.Length];
            for (int i = 0; i < items.Length; i++) {
                result[i] = converter(items[i], items2[i]);
            }
            return result;
        }
    }
}
