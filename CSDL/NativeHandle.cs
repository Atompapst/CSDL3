// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Internal;
namespace CSDL {
    public interface INativeHandle {
        nint NativePointer { get; }
    }
    /// <summary>
    /// A high-stakes wrapper around a raw unmanaged resource pointer.
    /// </summary>
    /// <remarks>
    /// This is C#, but you are playing with loaded C guns here. Treat this with respect.
    /// If you fail to <see cref="Dispose"/> of this object, 
    /// you are actively creating a production-grade memory leak. Wrap this in a 'using' block or 
    /// die by a thousand memory dumps.
    /// </remarks>
    /// <seealso cref="NativePointer"/>
    public abstract class NativeHandle<T> : IDisposable, INativeHandle where T : unmanaged {
        private NativePtr<T> _handle;
        private readonly bool _ownsHandle;
        private readonly InvalidationRegistration _invalidation;
        private int _disposed;

        protected NativeHandle() {
            _ownsHandle = true;
            _invalidation = new InvalidationRegistration(Invalidate);
        }

        protected NativeHandle(NativePtr<T> handle, bool ownsHandle) {
            _handle = handle;
            _ownsHandle = ownsHandle;
            _invalidation = new InvalidationRegistration(Invalidate);
        }

        internal InvalidationRegistration Invalidation => _invalidation;

        /// <summary>
        /// Gets or sets the typed handle/pointer
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        internal NativePtr<T> Handle {
            get {
                if (System.Threading.Volatile.Read(ref _disposed) != 0 && _handle.IsNull) {
                    throw new ObjectDisposedException(GetType().Name);
                }
                return _handle;
            }
            set => _handle = value;
        }

        /// <summary>
        /// The raw native pointer backing this handle, for interop with structs that take it directly (e.g. GPU create-info structs).
        /// </summary>
        public nint NativePointer => _handle.Ptr;

        /// <summary>
        /// Provides direct access to the structure reference of the resource managed by the handle.
        /// </summary>
        internal ref T Ref => ref _handle.AsRef();

        /// <summary>
        /// Checks if the handle is valid (not null)
        /// </summary>
        protected bool IsValid => !_handle.IsNull;

        /// <summary>
        /// Releases any unmanaged resources of the derived class.
        /// This is meant to be overridden to perform specific cleanup tasks.
        /// </summary>
        protected virtual void DisposeResource() { }

        /// <summary>
        /// Releases managed references after a native parent destroyed this handle.
        /// </summary>
        protected virtual void InvalidateResource() { }


        /// <summary>
        /// Explicitly frees the unmanaged memory. 
        /// The <see cref="GC">Garbage Collector</see> <b>CANNOT</b> clean up unmanaged resources automatically 
        /// without a massive performance penalty. Call this immediately when done, 
        /// or wrap this object in a 'using' statement. No exceptions.
        /// Subsequent calls to this method will have no effect.
        /// </summary>
        /// <seealso cref="DisposeResource"/>
        public void Dispose() {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 0) return;

            try {
                if (_ownsHandle && !_handle.IsNull) {
                    DisposeResource(); // Calls SDL_Destroy* which frees memory internally
                }
            } finally {
                // A failed native cleanup must not leave a disposed wrapper exposing a stale handle.
                _handle = NativePtr<T>.Zero;
                GC.SuppressFinalize(this);
            }
        }

        // The native owner destroyed this handle as part of its own teardown.
        internal void Invalidate() {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 0) return;
            InvalidateResource();
            _handle = NativePtr<T>.Zero;
            GC.SuppressFinalize(this);
        }

#if DEBUG
        ~NativeHandle() {
            if (_ownsHandle && System.Threading.Volatile.Read(ref _disposed) == 0 && !_handle.IsNull) {
                // Punish them
                System.Diagnostics.Debug.WriteLine(
                    $"[CSDL LEAK DETECTOR]: Memory Leak in '{GetType().Name}'! " +
                    $"An object was garbage collected without being properly Disposed. " +
                    $"This resource cannot be safely freed from the finalizer thread, so it is being " +
                    $"leaked instead of risking a native crash. FIX YOUR CODE."
                );
            }
            // Intentionally does NOT call Dispose()/DisposeResource() here. Some Destroy* functions
            // (SDL_DestroyWindow, SDL_DestroyRenderer, SDL_GL_DestroyContext, ...) are documented as
            // main-thread-only, and the finalizer thread is never the main thread. But even for the
            // ones that are documented safe from any thread (e.g. SDL_DestroyMutex), the finalizer
            // runs at a GC-chosen time with no guaranteed order relative to other finalizers or to
            // Init.Quit() - by then the subsystem backing this handle may already be torn down, or
            // the handle may reference memory SDL has since reused. Leaking is the safe failure mode.
        }
#endif
    }
}
