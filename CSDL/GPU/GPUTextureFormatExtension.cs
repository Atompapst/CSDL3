using CSDL.Video;
namespace CSDL.GPU {
    public static class GPUTextureFormatExtension {
        // GPUTextureFormat is enum

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CalculateGPUTextureFormatSize"/>
        public static uint CalculateGPUTextureFormatSize(this GPUTextureFormat format, uint width, uint height, uint depthOrLayerCount) {
            return SDL.CalculateGPUTextureFormatSize(format, width, height, depthOrLayerCount);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GPUTextureFormatTexelBlockSize"/>
        public static uint TexelBlockSize(this GPUTextureFormat format) {
            return SDL.GPUTextureFormatTexelBlockSize(format);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GetPixelFormatFromGPUTextureFormat"/>
        public static PixelFormat ToPixelFormat(this GPUTextureFormat format) {
            return SDL.GetPixelFormatFromGPUTextureFormat(format);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GetGPUTextureFormatFromPixelFormat"/>
        public static GPUTextureFormat ToGPUTextureFormat(this PixelFormat format) {
            return SDL.GetGPUTextureFormatFromPixelFormat(format);
        }
    }
}
