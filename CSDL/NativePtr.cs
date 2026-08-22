// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;

namespace CSDL {
    /// <summary>
    ///     A type-safe wrapper around a native pointer to an unmanaged structure.
    /// </summary>
    public readonly struct NativePtr<T> : IEquatable<NativePtr<T>> where T : unmanaged {

        /// <summary>
        ///     Wraps a native pointer as a typed <see cref="NativePtr{T}" />.
        /// </summary>
        public NativePtr(IntPtr ptr) {
            Ptr = ptr;
        }

        /// <summary>
        ///     The underlying native pointer.
        /// </summary>
        public IntPtr Ptr { get; }

        /// <summary>
        ///     A typed null pointer.
        /// </summary>
        public static NativePtr<T> Zero => default;

        /// <summary>
        ///     Returns true if the value represents a null pointer.
        /// </summary>
        public bool IsNull => Ptr == IntPtr.Zero;

        /// <summary>
        ///     Dereference the pointed data at the specified index.
        /// </summary>
        public unsafe T this[int index] {
            get => ((T*)Ptr)![index];
            set => ((T*)Ptr)![index] = value;
        }

        /// <summary>
        ///     Implicitly converts to <see cref="IntPtr" />.
        /// </summary>
        public static implicit operator IntPtr(NativePtr<T> other) {
            return other.Ptr;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="IntPtr" /> to a <see cref="NativePtr{T}" />.
        /// </summary>
        public static implicit operator NativePtr<T>(IntPtr other) {
            return new NativePtr<T>(other);
        }

        /// <summary>
        ///     Implicitly converts to an unmanaged <c>T*</c>.
        /// </summary>
        public static unsafe implicit operator T*(NativePtr<T> other) {
            return (T*)other.Ptr;
        }

        /// <summary>
        ///     Implicitly converts an unmanaged <c>T*</c> to a <see cref="NativePtr{T}" />.
        /// </summary>
        public static unsafe implicit operator NativePtr<T>(T* other) {
            return new NativePtr<T>((IntPtr)other);
        }

        /// <summary>
        ///     Creates a <see cref="NativePtr{T}" /> from a read-only reference.
        /// </summary>
        public static unsafe NativePtr<T> FromIn(scoped in T target) {
            // https://stackoverflow.com/a/1665570
            return new NativePtr<T>((IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef<T>(in target)));
        }

        /// <summary>
        ///     Creates a <see cref="NativePtr{T}" /> from a reference.
        /// </summary>
        public static unsafe NativePtr<T> FromRef(scoped ref T target) {
            return new NativePtr<T>((IntPtr)Unsafe.AsPointer<T>(ref target));
        }


        /// <summary>
        ///     Dereferences the pointer as a managed reference to <typeparamref name="T" />.
        /// </summary>
        public unsafe ref T AsRef() {
            return ref Unsafe.AsRef<T>((void*)Ptr);
        }

        /// <summary>
        ///     Dereferences the pointer at the given index as a managed reference to <typeparamref name="T" />.
        /// </summary>
        /// <param name="index">Zero-based element index.</param>
        public unsafe ref T AsRef(int index) {
            return ref ((T*)Ptr)![index];
        }

        /// <summary>
        ///     Dereferences the pointer as a read-only managed reference to <typeparamref name="T" />.
        /// </summary>
        public unsafe ref readonly T AsReadOnlyRef() {
            return ref Unsafe.AsRef<T>((void*)Ptr);
        }

        /// <summary>
        ///     Views the pointed-to memory as a <see cref="Span{T}" /> of <paramref name="length" /> elements.
        /// </summary>
        public unsafe Span<T> AsSpan(int length) {
            return new Span<T>((void*)Ptr, length);
        }

        /// <summary>
        ///     Views the pointed-to memory as a <see cref="ReadOnlySpan{T}" /> of <paramref name="length" /> elements.
        /// </summary>
        public unsafe ReadOnlySpan<T> AsReadOnlySpan(int length) {
            return new ReadOnlySpan<T>((void*)Ptr, length);
        }

        /// <summary>
        ///     Returns a pointer advanced by the given element offset.
        /// </summary>
        /// <param name="elementOffset">The number of elements of type <typeparamref name="T" /> to offset.</param>
        /// <returns>
        ///     An <see cref="IntPtr" /> pointing to the memory location at the specified element offset.
        /// </returns>
        public nint Offset(int elementOffset) {
            return Ptr + elementOffset * SizeOfInternal();
        }

        /// <summary>
        ///     Reads the value at the pointer location.
        /// </summary>
        public unsafe T Read() {
            return Unsafe.Read<T>((void*)Ptr);
        }

        /// <summary>
        ///     Reads the value at the given element index.
        /// </summary>
        public unsafe T Read(int index) {
            return ((T*)Ptr)![index];
        }

        /// <summary>
        ///     Writes a value to the pointer location.
        /// </summary>
        public unsafe void Write(T value) {
            Unsafe.Write<T>((void*)Ptr, value);
        }

        /// <summary>
        ///     Writes a value at the given element offset.
        /// </summary>
        public unsafe void Write(T value, int offset) {
            Unsafe.Write<T>((void*)(Ptr + offset * SizeOfInternal()), value);
        }

        /// <summary>
        ///     Returns true if both pointers point to the same memory location.
        /// </summary>
        public bool Equals(NativePtr<T> other) {
            return Ptr == other.Ptr;
        }

        private static int SizeOfInternal() {
            return Unsafe.SizeOf<T>();
        }
        public override bool Equals(object? obj) {
            return obj is NativePtr<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return Ptr.GetHashCode();
        }

        public static bool operator ==(NativePtr<T> left, NativePtr<T> right) {
            return left.Ptr == right.Ptr;
        }

        public static bool operator !=(NativePtr<T> left, NativePtr<T> right) {
            return left.Ptr != right.Ptr;
        }

        /// <summary>
        ///     Returns a pointer advanced by <paramref name="elementOffset" /> elements of
        ///     <typeparamref name="T" /> - matching the pointer arithmetic of a <c>T*</c>, not a raw byte
        ///     offset.
        /// </summary>
        public static NativePtr<T> operator +(NativePtr<T> ptr, int elementOffset) {
            return new NativePtr<T>((nint)ptr.Ptr + elementOffset * SizeOfInternal());
        }

        /// <summary>
        ///     Returns a pointer receded by <paramref name="elementOffset" /> elements of
        ///     <typeparamref name="T" /> - matching the pointer arithmetic of a <c>T*</c>, not a raw byte
        ///     offset.
        /// </summary>
        public static NativePtr<T> operator -(NativePtr<T> ptr, int elementOffset) {
            return new NativePtr<T>((nint)ptr.Ptr - elementOffset * SizeOfInternal());
        }

        public override string ToString() {
            return Ptr.ToString();
        }
    }
}
