// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.GPU {
    public class GPUTransferBuffer : NativeHandle<Opaque.SdlGPUTransferBuffer> {
        private readonly GPUDevice _gpuDevice;
        private bool _isMapped;

        /// <summary>
        /// The size in bytes of this transfer buffer, as given at creation time.
        /// </summary>
        public uint SizeInBytes { get; }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUTransferBuffer"/>
        public GPUTransferBuffer(GPUDevice gpuDevice, GPUTransferBufferCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            SizeInBytes = createInfo.Size;
            Handle = SDL.CreateGPUTransferBuffer(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.MapGPUTransferBuffer"/>
        public IntPtr Map(bool cycle = false) {
            if (_isMapped) {
                throw new InvalidOperationException("The transfer buffer is already mapped.");
            }

            IntPtr pointer = SDL.MapGPUTransferBuffer(_gpuDevice.Handle, Handle, cycle).ThrowIfInvalid();
            _isMapped = true;
            return pointer;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.UnmapGPUTransferBuffer"/>
        public void Unmap() {
            if (!_isMapped) {
                throw new InvalidOperationException("The transfer buffer is not mapped.");
            }

            SDL.UnmapGPUTransferBuffer(_gpuDevice.Handle, Handle);
            _isMapped = false;
        }

        /// <summary>
        /// Maps this transfer buffer and returns its contents as a writable <see cref="Span{T}"/>,
        /// without copying. The span is only valid until <see cref="Unmap"/> is called.
        /// </summary>
        /// <typeparam name="T">An unmanaged element type.</typeparam>
        /// <param name="cycle">See <see cref="CSDL.Internal.Docs.GPU.MapGPUTransferBuffer"/>.</param>
        public unsafe Span<T> MapAsSpan<T>(bool cycle = false) where T : unmanaged {
            IntPtr ptr = Map(cycle);
            return new Span<T>((void*)ptr, (int)(SizeInBytes / (uint)sizeof(T)));
        }

        /// <summary>
        /// Copies <paramref name="data"/> into this transfer buffer, handling the map/unmap
        /// bookkeeping. This is the Span-based equivalent of calling <see cref="Map"/>,
        /// writing through the returned pointer, and calling <see cref="Unmap"/>.
        /// </summary>
        /// <typeparam name="T">An unmanaged element type.</typeparam>
        /// <param name="data">The source data to copy in.</param>
        /// <param name="offsetBytes">The byte offset into the transfer buffer to start writing at.</param>
        /// <param name="cycle">See <see cref="CSDL.Internal.Docs.GPU.MapGPUTransferBuffer"/>.</param>
        public unsafe void SetData<T>(ReadOnlySpan<T> data, uint offsetBytes = 0, bool cycle = false) where T : unmanaged {
            long dataBytes = (long)data.Length * sizeof(T);
            if (offsetBytes + dataBytes > SizeInBytes) {
                throw new ArgumentOutOfRangeException(nameof(data), "Data does not fit in the transfer buffer at the given offset.");
            }

            IntPtr ptr = Map(cycle);
            try {
                Span<T> destination = new Span<T>((void*)(ptr + offsetBytes), data.Length);
                data.CopyTo(destination);
            } finally {
                Unmap();
            }
        }

        /// <summary>
        /// Copies out of this transfer buffer into <paramref name="destination"/>, handling the
        /// map/unmap bookkeeping. Typically used after a GPU-to-transfer-buffer download.
        /// </summary>
        /// <typeparam name="T">An unmanaged element type.</typeparam>
        /// <param name="destination">The managed buffer to copy the data into.</param>
        /// <param name="offsetBytes">The byte offset into the transfer buffer to start reading from.</param>
        public unsafe void GetData<T>(Span<T> destination, uint offsetBytes = 0) where T : unmanaged {
            long dataBytes = (long)destination.Length * sizeof(T);
            if (offsetBytes + dataBytes > SizeInBytes) {
                throw new ArgumentOutOfRangeException(nameof(destination), "Requested range exceeds the transfer buffer's size.");
            }

            IntPtr ptr = Map(false);
            try {
                Span<T> source = new Span<T>((void*)(ptr + offsetBytes), destination.Length);
                source.CopyTo(destination);
            } finally {
                Unmap();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUTransferBuffer"/>
        protected override void DisposeResource() {
            if (_isMapped) {
                SDL.UnmapGPUTransferBuffer(_gpuDevice.Handle, Handle);
                _isMapped = false;
            }
            SDL.ReleaseGPUTransferBuffer(_gpuDevice.Handle, Handle);
        }
    }
}
