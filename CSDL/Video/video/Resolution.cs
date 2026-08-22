// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    /// <summary>
    /// Represents a collection of predefined screen resolutions across various aspect ratios
    /// and formats, including standard, widescreen, ultrawide, and mobile-specific resolutions.
    /// </summary>
    public enum Resolution {
        /// 1280x720 (16:9)
        HD_720p = 0,
        /// 1600x900 (16:9)
        HDPlus_900p = 1,
        /// 1920x1080 (16:9)
        FullHD_1080p = 2,
        /// 2560x1440 (16:9)
        QHD_1440p = 3,
        /// 3200x1800 (16:9)
        QHDPlus_1800p = 4,
        /// 3840x2160 (16:9)
        UHD_4K = 5,
        /// 7680x4320 (16:9)
        UHD_8K = 6,
        /// 640x480 (4:3)
        VGA = 100,
        /// 800x600 (4:3)
        SVGA = 101,
        /// 1024x768 (4:3)
        XGA = 102,
        /// 1280x960 (4:3)
        SXGA = 103,
        /// 1600x1200 (4:3)
        UXGA = 104,
        /// 1280x800 (16:10)
        WXGA = 200,
        /// 1440x900 (16:10)
        WSXGA = 201,
        /// 1920x1200 (16:10)
        WUXGA = 202,
        /// 2560x1600 (16:10)
        WQXGA = 203,
        /// 2560x1080 Ultrawide (21:9)
        UWHD = 300,
        /// 3440x1440 Ultrawide (21:9)
        UWQHD = 301,
        /// 5120x2160 Ultrawide (21:9)
        UW4K = 302,
        /// 960x540 Mobile-Friendly / Small Formats
        qHD = 400,
        /// 854x480 Mobile-Friendly / Small Formats
        FWVGA = 401,
        /// 480x320 Mobile-Friendly / Small Formats
        HVGA = 402,
    }
}
