// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
namespace CSDL {
    /// <summary>
    ///     Static Memory methods using SDL's Stdinc Functions.
    /// </summary>
    public static unsafe class Memory {
        private static readonly object MemoryFunctionsLock = new object();
        // SDL may be concurrently executing a previous allocator when the caller replaces it.
        // Keep every successful managed installation rooted for the process lifetime.
        private static readonly List<Delegate> ManagedMemoryFunctions = new List<Delegate>();
        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.malloc"/>
        public static NativePtr<IntPtr> Malloc(nuint size) {
            IntPtr pointer = SDL.malloc(size);
            if (pointer == IntPtr.Zero) {
                Error.ThrowIfError(nameof(Malloc));
            }
            return new NativePtr<IntPtr>(pointer);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.calloc"/>
        public static NativePtr<IntPtr> Calloc(nuint nmemb, nuint size) {
            IntPtr pointer = SDL.calloc(nmemb, size);
            if (pointer == IntPtr.Zero) {
                Error.ThrowIfError(nameof(Calloc));
            }
            return new NativePtr<IntPtr>(pointer);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.aligned_alloc"/>
        public static NativePtr<IntPtr> AlignedAlloc(UIntPtr alignment, UIntPtr size) {
            IntPtr pointer = SDL.aligned_alloc(alignment, size);
            if (pointer == IntPtr.Zero) {
                Error.ThrowIfError(nameof(AlignedAlloc));
            }
            return new NativePtr<IntPtr>(pointer);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.realloc"/>
        public static NativePtr<IntPtr> Realloc(IntPtr mem, nuint size) {
            IntPtr pointer = SDL.realloc(mem, size);
            if (pointer == IntPtr.Zero) {
                Error.ThrowIfError(nameof(Realloc));
            }
            return new NativePtr<IntPtr>(pointer);
        }

        /// <summary>
        /// Reallocates memory referenced by a <see cref="NativePtr{T}"/>.
        /// </summary>
        /// <seealso cref="Realloc(IntPtr, nuint)"/>
        public static NativePtr<IntPtr> Realloc(NativePtr<IntPtr> mem, nuint size) {
            return Realloc(mem.Ptr, size);
        }

        /// <summary>
        /// Allocates a zero-initialized array of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="count">The number of elements to allocate.</param>
        /// <seealso cref="Calloc(nuint, nuint)"/>
        public static NativePtr<T> CallocArray<T>(int count) where T : unmanaged {
            IntPtr ptr = SDL.calloc((nuint)count, (nuint)sizeof(T));
            if (ptr == IntPtr.Zero) {
                Error.ThrowIfError(nameof(CallocArray));
            }
            return new NativePtr<T>(ptr);
        }

        /// <summary>
        /// Allocates an array of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="count">The number of elements to allocate.</param>
        /// <seealso cref="Malloc(nuint)"/>
        public static NativePtr<T> MallocArray<T>(int count) where T : unmanaged {
            IntPtr ptr = SDL.malloc((nuint)(count * sizeof(T)));
            if (ptr == IntPtr.Zero) {
                Error.ThrowIfError(nameof(MallocArray));
            }
            return new NativePtr<T>(ptr);
        }

        /// <summary>
        /// Allocates single Instance of <typeparamref name="T"/>.
        /// </summary>
        /// <seealso cref="Malloc(nuint)"/>
        public static NativePtr<T> Malloc<T>() where T : unmanaged {
            IntPtr ptr = SDL.malloc((nuint)sizeof(T));
            if (ptr == IntPtr.Zero) {
                Error.ThrowIfError(nameof(Malloc));
            }
            return new NativePtr<T>(ptr);
        }

        /// <summary>
        /// Allocates a zero-initialized single instance of <typeparamref name="T"/>.
        /// </summary>
        /// <seealso cref="Calloc(nuint, nuint)"/>
        public static NativePtr<T> Calloc<T>() where T : unmanaged {
            IntPtr ptr = SDL.calloc(1, (nuint)sizeof(T));
            if (ptr == IntPtr.Zero) {
                Error.ThrowIfError(nameof(Calloc));
            }
            return new NativePtr<T>(ptr);
        }

        /// <summary>
        /// Reallocates an array of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="ptr">The pointer to the memory to reallocate.</param>
        /// <param name="count">The new number of elements.</param>
        /// <seealso cref="Realloc(IntPtr, nuint)"/>
        public static NativePtr<T> ReallocArray<T>(NativePtr<T> ptr, int count) where T : unmanaged {
            IntPtr newPtr = SDL.realloc(ptr.Ptr, (nuint)(count * sizeof(T)));
            if (newPtr == IntPtr.Zero) {
                Error.ThrowIfError(nameof(ReallocArray));
            }
            return new NativePtr<T>(newPtr);
        }

        /// <summary>
        /// Frees memory referenced by <paramref name="ptr"/> and sets the pointer to zero.
        /// </summary>
        /// <param name="ptr">The pointer to the allocated memory.</param>
        /// <seealso cref="Free(IntPtr)"/>
        public static void Free<T>(this ref NativePtr<T> ptr) where T : unmanaged {
            if (!ptr.IsNull) {
                SDL.free(ptr.Ptr);
                ptr = NativePtr<T>.Zero;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.free"/>
        public static void Free(IntPtr mem) {
            if (mem != IntPtr.Zero) {
                SDL.free(mem);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.aligned_free"/>
        public static void AlignedFree(IntPtr mem) {
            if (mem != nint.Zero) {
                SDL.aligned_free(mem);
            }
        }

        /// <summary>
        /// Frees aligned memory referenced by a <see cref="NativePtr{T}"/>.
        /// </summary>
        /// <param name="mem">The pointer to memory previously allocated by <see cref="AlignedAlloc(UIntPtr, UIntPtr)"/>.</param>
        /// <seealso cref="AlignedFree(IntPtr)"/>
        public static void AlignedFree(NativePtr<IntPtr> mem) {
            if (!mem.IsNull) {
                SDL.aligned_free(mem.Ptr);
            }
        }


        /// <summary>
        /// Replaces SDL's memory allocation functions with managed callbacks.
        /// </summary>
        /// <param name="malloc">The custom allocation callback.</param>
        /// <param name="calloc">The custom zero-initialized allocation callback.</param>
        /// <param name="realloc">The custom reallocation callback.</param>
        /// <param name="free">The custom deallocation callback.</param>
        /// <remarks>
        /// All four callbacks are required here (unlike the native function, which also accepts all-NULL
        /// to mean "restore the defaults") - use <see cref="SetOriginalMemoryFunctions"/> for that instead.
        /// One should not replace the memory functions once any allocations have already been made.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Any of the four callbacks is <see langword="null"/>.</exception>
        /// <exception cref="SDLException">
        /// SDL rejected the call. Its own docs say this "will not set an error message", so unlike the rest of this wrapper this does <b>not</b> go through
        /// <see cref="Error.GetError"/>/<see cref="Error.LastError"/> - the message here is our own.
        /// </exception>
        /// <seealso cref="SetOriginalMemoryFunctions"/>
        /// <seealso cref="CSDL.Internal.Docs.Stdinc.SetMemoryFunctions">SDL_SetMemoryFunctions</seealso>
        public static void SetMemoryFunctions(MallocFunc malloc, CallocFunc calloc, ReallocFunc realloc, FreeFunc free) {
            ArgumentNullException.ThrowIfNull(malloc);
            ArgumentNullException.ThrowIfNull(calloc);
            ArgumentNullException.ThrowIfNull(realloc);
            ArgumentNullException.ThrowIfNull(free);

            SDL_malloc_funcNative mallocNative = MallocFuncWrapper.Create(malloc);
            SDL_calloc_funcNative callocNative = CallocFuncWrapper.Create(calloc);
            SDL_realloc_funcNative reallocNative = ReallocFuncWrapper.Create(realloc);
            SDL_free_funcNative freeNative = FreeFuncWrapper.Create(free);

            lock (MemoryFunctionsLock) {
                if (!SDL.SetMemoryFunctions(mallocNative, callocNative, reallocNative, freeNative)) {
                    throw new SDLException(nameof(SetMemoryFunctions), "SDL_SetMemoryFunctions failed - either mix NULL and non-NULL callbacks (not possible through this overload) or memory was already allocated before this call.");
                }

                ManagedMemoryFunctions.Add(mallocNative);
                ManagedMemoryFunctions.Add(callocNative);
                ManagedMemoryFunctions.Add(reallocNative);
                ManagedMemoryFunctions.Add(freeNative);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.GetOriginalMemoryFunctions"/>
        /// <remarks>
        /// Restores the default allocator SDL started with, undoing any prior
        /// <see cref="SetMemoryFunctions"/> call. The native functions this reads back are already
        /// SDL-owned function pointers (not managed closures), so - unlike <see cref="SetMemoryFunctions"/> -
        /// there's no GC lifetime concern here to root them against.
        /// </remarks>
        public static void SetOriginalMemoryFunctions() {
            SDL.GetOriginalMemoryFunctions(
                out SDL_malloc_funcNative malloc,
                out SDL_calloc_funcNative calloc,
                out SDL_realloc_funcNative realloc,
                out SDL_free_funcNative free
            );

            lock (MemoryFunctionsLock) {
                if (!SDL.SetMemoryFunctions(malloc, calloc, realloc, free)) {
                    throw new SDLException(nameof(SetOriginalMemoryFunctions), "SDL_SetMemoryFunctions failed while restoring the original allocator - this should not normally happen, since GetOriginalMemoryFunctions always returns valid callbacks.");
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Stdinc.GetMemoryFunctions"/>
        /// <returns>
        /// Managed delegates forwarding to the allocator SDL is using right now - either its own, or
        /// whatever <see cref="SetMemoryFunctions"/> installed.
        /// </returns>
        /// <remarks>
        /// The delegates are bound to the function pointers read at the time of this call: installing a
        /// different allocator afterwards does not change where they route to, so re-read them after any
        /// <see cref="SetMemoryFunctions"/>/<see cref="SetOriginalMemoryFunctions"/> call. Memory taken
        /// from one allocator must be returned to the same one.
        /// </remarks>
        public static (MallocFunc Malloc, CallocFunc Calloc, ReallocFunc Realloc, FreeFunc Free) GetMemoryFunctions() {
            SDL.GetMemoryFunctions(
                out SDL_malloc_funcNative malloc,
                out SDL_calloc_funcNative calloc,
                out SDL_realloc_funcNative realloc,
                out SDL_free_funcNative free
            );

            return (
                size => malloc(size),
                (nmemb, size) => calloc(nmemb, size),
                (mem, size) => realloc(mem, size),
                mem => free(mem)
            );
        }

        /// <summary>
        /// <para>Attempts to release free memory from the GNU C library heap back to the operating system.</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method invokes <see href="https://man7.org/linux/man-pages/man3/malloc_trim.3.html">malloc_trim(3)</see><c></c>, which attempts to release free heap memory by calling
        /// <see href="https://man7.org/linux/man-pages/man2/sbrk.2.html">sbrk(2)</see> or
        /// <see href="https://man7.org/linux/man-pages/man2/madvise.2.html">madvise(2)</see> with suitable arguments. Releasing memory is best-effort;
        /// calling this method does not guarantee that any memory will be returned to the system.
        /// </para>
        /// <para>
        /// A <paramref name="pad"/> value of zero retains only the minimum amount of free memory at the top
        /// of the heap (one page or less). A nonzero value requests that amount of trailing free space be
        /// retained for future allocations that would otherwise require extending the heap.
        /// </para>
        /// <para>
        /// Only the main heap honors <paramref name="pad"/>; thread heaps do not. Since glibc 2.8,
        /// <c>malloc_trim</c> can release whole free pages in all arenas. Earlier versions only released
        /// memory at the top of the main arena.
        /// </para>
        /// <para>
        /// This wrapper is Linux-specific. If glibc or <c>malloc_trim</c> is unavailable, the native call
        /// fails silently. The native return value is not exposed, so callers cannot determine whether
        /// memory was actually released.
        /// </para>
        /// </remarks>
        /// <param name="pad">The amount of free space, in bytes, to leave untrimmed at the top of the main heap.</param>
        /// <threadsafety>It is safe to call this function from any thread (MT-Safe).</threadsafety>
        public static void MallocTrim(nuint pad = 0) {
            try { malloc_trim(pad); } catch { }
        }

        // ONLY LINUX
        [System.Runtime.InteropServices.DllImport("libc.so.6")]
        private static extern int malloc_trim(nuint pad);
    }
}
