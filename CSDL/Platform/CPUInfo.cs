// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    public static class CPUInfo {
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.GetNumLogicalCPUCores"/>
        public static int NumLogicalCores => SDL.GetNumLogicalCPUCores();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.GetCPUCacheLineSize"/>
        public static int CpuCacheLineSize => SDL.GetCPUCacheLineSize();
        public static long SystemRamGib => SystemRamBytes / (1024L * 1024L * 1024L);
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.GetSystemRAM"/>
        public static int SystemRamMiB => SDL.GetSystemRAM();
        public static long SystemRamBytes => (long)SystemRamMiB * 1024L * 1024L;
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.GetSIMDAlignment"/>
        public static nuint SimdAlignment => (nuint)SDL.GetSIMDAlignment();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.GetSystemPageSize"/>
        public static int SystemPageSize => SDL.GetSystemPageSize();
        public static int SystemPageSizeOrDefault => SystemPageSize > 0 ? SystemPageSize : 4096;

        /// <inheritdoc cref="CSDL.Macros.CachelineSize"/>
        /// <remarks>
        ///     A compile-time constant SDL uses for cache-line alignment, not a measurement - see
        ///     <see cref="CpuCacheLineSize"/> for what this machine actually reports.
        /// </remarks>
        public static int CachelineSizeHint => (int)Macros.CachelineSize;

        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasAltiVec"/>
        public static bool HasAltiVec => SDL.HasAltiVec();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasMMX"/>
        public static bool HasMMX => SDL.HasMMX();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasSSE"/>
        public static bool HasSSE => SDL.HasSSE();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasSSE2"/>
        public static bool HasSSE2 => SDL.HasSSE2();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasSSE3"/>
        public static bool HasSSE3 => SDL.HasSSE3();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasSSE41"/>
        public static bool HasSSE41 => SDL.HasSSE41();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasSSE42"/>
        public static bool HasSSE42 => SDL.HasSSE42();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasAVX"/>
        public static bool HasAVX => SDL.HasAVX();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasAVX2"/>
        public static bool HasAVX2 => SDL.HasAVX2();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasAVX512F"/>
        public static bool HasAVX512F => SDL.HasAVX512F();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasARMSIMD"/>
        public static bool HasARMSIMD => SDL.HasARMSIMD();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasNEON"/>
        public static bool HasNEON => SDL.HasNEON();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasLSX"/>
        public static bool HasLSX => SDL.HasLSX();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasLASX"/>
        public static bool HasLASX => SDL.HasLASX();
        /// <inheritdoc cref="CSDL.Internal.Docs.CPUInfo.HasSVE2"/>
        public static bool HasSVE2 => SDL.HasSVE2();

        public static bool HasX86Simd => HasMMX || HasSSE || HasSSE2 || HasSSE3 || HasSSE41 || HasSSE42 || HasAVX || HasAVX2 || HasAVX512F;
        public static bool HasArmSimd => HasARMSIMD || HasNEON || HasSVE2;
        public static bool HasLoongArchSimd => HasLSX || HasLASX;
        public static bool HasAnySimd => HasX86Simd || HasArmSimd || HasLoongArchSimd || HasAltiVec;
    }
}
