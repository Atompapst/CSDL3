// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    /// <summary>
    /// Opaque structs for Vulkan
    /// </summary>
    public static class Opaque {
        /// <summary>
        /// <para>Opaque handle to an instance object</para>
        /// </summary>
        /// <VulkanDocs><seealso href="https://docs.vulkan.org/refpages/latest/refpages/source/VkInstance.html">VkInstance</seealso></VulkanDocs>
        public struct VkInstance;

        /// <summary>
        /// <para> Opaque handle to a surface object</para>
        /// </summary>
        /// <VulkanDocs><seealso href="https://docs.vulkan.org/refpages/latest/refpages/source/VkSurfaceKHR.html">VkSurfaceKHR</seealso></VulkanDocs>
        public struct VkSurfaceKHR;

        /// <summary>
        /// <para>Opaque handle to a physical device object</para>
        /// </summary>
        /// <VulkanDocs><seealso href="https://docs.vulkan.org/refpages/latest/refpages/source/VkPhysicalDevice.html">VkPhysicalDevice</seealso></VulkanDocs>
        public struct VkPhysicalDevice;
    }
}
