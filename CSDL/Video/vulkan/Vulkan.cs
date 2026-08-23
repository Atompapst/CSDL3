// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Video {
    public static class Vulkan {
        static Vulkan() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_LoadLibrary"/>
        public static bool LoadLibrary(string? path = null) {
            return SDL.Vulkan_LoadLibrary(path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_UnloadLibrary"/>
        public static void UnloadLibrary() {
            SDL.Vulkan_UnloadLibrary();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_GetVkGetInstanceProcAddr"/>
        public static IntPtr GetVkGetInstanceProcAddr() {
            IntPtr address = SDL.Vulkan_GetVkGetInstanceProcAddr();
            if (address == IntPtr.Zero) {
                Error.LogError(nameof(SDL.Vulkan_GetVkGetInstanceProcAddr));
            }
            return address;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_GetInstanceExtensions"/>
        public static string[] GetInstanceExtensions() {
            IntPtr ptr = SDL.Vulkan_GetInstanceExtensions(out uint count);
            if (ptr == IntPtr.Zero || count == 0) {
                return Array.Empty<string>();
            }
            return NativeStringArray.ToArray(ptr, (int)count);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_GetPresentationSupport"/>
        public static bool GetPresentationSupport(IntPtr instance, IntPtr physicalDevice, uint queueFamilyIndex) {
            return SDL.Vulkan_GetPresentationSupport(instance, physicalDevice, queueFamilyIndex);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_CreateSurface"/>
        public static bool CreateSurface(Window window, IntPtr instance, IntPtr allocator, out IntPtr surface) {
            ArgumentNullException.ThrowIfNull(window);
            IntPtr result = IntPtr.Zero;
            bool ok;
            unsafe {
                ok = SDL.Vulkan_CreateSurface(window.Handle, instance, allocator, (IntPtr)(&result)).LogIfFalse();
            }
            surface = result;
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Vulkan.Vulkan_DestroySurface"/>
        public static void DestroySurface(IntPtr instance, IntPtr surface, IntPtr allocator = default) {
            SDL.Vulkan_DestroySurface(instance, surface, allocator);
        }
    }
}
