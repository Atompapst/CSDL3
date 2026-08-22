// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
namespace CSDL.Video {
    /// <summary>
    /// Provides utility methods to retrieve resolution information, aspect ratios, and resolutions filtered by aspect ratio.
    /// </summary>
    public static class ResolutionHelper {
        private static readonly Dictionary<Resolution, ResolutionInfo> Resolutions = new Dictionary<Resolution, ResolutionInfo> {
            // 16:9  
            { Resolution.HD_720p, new ResolutionInfo(1280, 720, AspectRatio.Ratio_16_9) },
            { Resolution.HDPlus_900p, new ResolutionInfo(1600, 900, AspectRatio.Ratio_16_9) },
            { Resolution.FullHD_1080p, new ResolutionInfo(1920, 1080, AspectRatio.Ratio_16_9) },
            { Resolution.QHD_1440p, new ResolutionInfo(2560, 1440, AspectRatio.Ratio_16_9) },
            { Resolution.QHDPlus_1800p, new ResolutionInfo(3200, 1800, AspectRatio.Ratio_16_9) },
            { Resolution.UHD_4K, new ResolutionInfo(3840, 2160, AspectRatio.Ratio_16_9) },
            { Resolution.UHD_8K, new ResolutionInfo(7680, 4320, AspectRatio.Ratio_16_9) },

            // 4:3  
            { Resolution.VGA, new ResolutionInfo(640, 480, AspectRatio.Ratio_4_3) },
            { Resolution.SVGA, new ResolutionInfo(800, 600, AspectRatio.Ratio_4_3) },
            { Resolution.XGA, new ResolutionInfo(1024, 768, AspectRatio.Ratio_4_3) },
            { Resolution.SXGA, new ResolutionInfo(1280, 960, AspectRatio.Ratio_4_3) },
            { Resolution.UXGA, new ResolutionInfo(1600, 1200, AspectRatio.Ratio_4_3) },

            // 16:10  
            { Resolution.WXGA, new ResolutionInfo(1280, 800, AspectRatio.Ratio_16_10) },
            { Resolution.WSXGA, new ResolutionInfo(1440, 900, AspectRatio.Ratio_16_10) },
            { Resolution.WUXGA, new ResolutionInfo(1920, 1200, AspectRatio.Ratio_16_10) },
            { Resolution.WQXGA, new ResolutionInfo(2560, 1600, AspectRatio.Ratio_16_10) },

            // Ultrawide (21:9)  
            { Resolution.UWHD, new ResolutionInfo(2560, 1080, AspectRatio.Ratio_21_9) },
            { Resolution.UWQHD, new ResolutionInfo(3440, 1440, AspectRatio.Ratio_21_9) },
            { Resolution.UW4K, new ResolutionInfo(5120, 2160, AspectRatio.Ratio_21_9) },

            // Mobile  
            { Resolution.qHD, new ResolutionInfo(960, 540, AspectRatio.Mobile) },
            { Resolution.FWVGA, new ResolutionInfo(854, 480, AspectRatio.Mobile) },
            { Resolution.HVGA, new ResolutionInfo(480, 320, AspectRatio.Mobile) },
        };

        /// <summary>
        /// Retrieves resolution information for a given resolution.
        /// </summary>
        /// <param name="resolution">The resolution enumeration value for which to retrieve information.</param>
        /// <returns>A <see cref="ResolutionInfo"/> object containing details about the specified resolution.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided resolution is unsupported or not found.</exception>
        public static ResolutionInfo GetResolutionInfo(Resolution resolution) {
            if (Resolutions.TryGetValue(resolution, out ResolutionInfo? info)) {
                return info;
            }
            throw new ArgumentException($"Unsupported resolution: {resolution}");
        }
        /// <summary>
        /// Retrieves resolution information based on the specified width and height.
        /// </summary>
        /// <param name="width">The width of the resolution to look up.</param>
        /// <param name="height">The height of the resolution to look up.</param>
        /// <returns>A <see cref="ResolutionInfo"/> object containing details about the resolution with the specified width and height.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified resolution width and height are unsupported or not found.</exception>
        public static ResolutionInfo GetResolutionInfo(int width, int height) {
            foreach (KeyValuePair<Resolution, ResolutionInfo> pair in Resolutions) {
                if (pair.Value.Width == width && pair.Value.Height == height) {
                    return pair.Value;
                }
            }
            throw new ArgumentException($"Unsupported resolution: {width}x{height}");
        }

        /// <summary>
        /// Retrieves the aspect ratio for a given resolution.
        /// </summary>
        /// <param name="resolution">The resolution enumeration value for which to retrieve the aspect ratio.</param>
        /// <returns>An <see cref="AspectRatio"/> value representing the aspect ratio of the specified resolution.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided resolution is unsupported or not found.</exception>
        public static AspectRatio GetAspectRatio(Resolution resolution) {
            if (Resolutions.TryGetValue(resolution, out ResolutionInfo? info)) {
                return info.Aspect;
            }
            throw new ArgumentException($"Unsupported resolution: {resolution}");
        }

        /// <summary>
        /// Retrieves a collection of resolutions that match the specified aspect ratio.
        /// </summary>
        /// <param name="aspect">The <see cref="AspectRatio"/> to filter resolutions by.</param>
        /// <returns>An <see cref="IEnumerable{Res}"/> containing all resolutions that correspond to the specified aspect ratio.</returns>
        /// <exception cref="ArgumentException">Thrown when no resolutions match the provided aspect ratio.</exception>
        public static IEnumerable<Resolution> GetResolutionsByAspect(AspectRatio aspect) {
            List<Resolution> list = new List<Resolution>();
            foreach (KeyValuePair<Resolution, ResolutionInfo> pair in Resolutions) {
                if (pair.Value.Aspect == aspect) {
                    list.Add(pair.Key);
                }
            }
            if (list.Count == 0) {
                throw new ArgumentException($"Unsupported aspect ratio: {aspect}");
            }
            return list;
        }


        /// <summary>
        /// Represents information about common screen resolution, including width, height, and aspect ratio.
        /// </summary>
        public class ResolutionInfo {
            public int Width { get; }
            public int Height { get; }
            public AspectRatio Aspect { get; }
            public ResolutionInfo(int width, int height, AspectRatio aspect) {
                Width = width;
                Height = height;
                Aspect = aspect;
            }
            public override string ToString() {
                return $"{Width}x{Height} ({Aspect})";
            }
        }
    }
}
