// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Video {
    public static class Drivers {
        /// <summary>
        /// Gets the names of the built-in SDL 2D rendering drivers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A render driver implements drawing and texture management for an SDL renderer.
        /// Typical names include <c>opengl</c>, <c>direct3d12</c>, and <c>metal</c>.
        /// This is distinct from a <see cref="VideoDrivers">video driver</see>, which
        /// connects SDL to the operating system's window and display system.
        /// </para>
        /// <para>This property is safe to access from any thread.</para>
        /// </remarks>
        /// <value>A new array containing the render driver identifiers.</value>
        /// <seealso cref="VideoDrivers"/>
        public static string[] RenderDrivers => GetRenderDrivers();

        /// <summary>
        /// Gets the names of the video drivers compiled into SDL.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A video driver connects SDL to a platform's window and display system.
        /// Typical names include <c>windows</c>, <c>x11</c>, and <c>cocoa</c>.
        /// </para>
        /// <para>
        /// Drivers are ordered as SDL normally checks them during video subsystem
        /// initialization. Use <see cref="CurrentVideoDriver"/> to identify the
        /// driver that SDL actually initialized.
        /// </para>
        /// <para>This property must be accessed on the main thread.</para>
        /// </remarks>
        /// <value>A new array containing the video driver identifiers.</value>
        /// <seealso cref="RenderDrivers"/>
        /// <seealso cref="CurrentVideoDriver"/>
        public static string[] VideoDrivers => GetVideoDrivers();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetCurrentVideoDriver"/>
        public static string CurrentVideoDriver => SDL.GetCurrentVideoDriver().ToUtf8String() ?? string.Empty;

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetSystemTheme"/>
        public static SystemTheme SystemTheme => SDL.GetSystemTheme();

        /// <seealso cref="CSDL.Internal.Docs.Render.GetNumRenderDrivers">GetNumRenderDrivers</seealso>
        /// <seealso cref="CSDL.Internal.Docs.Render.GetRenderDriver">GetRenderDriver</seealso>
        private static string[] GetRenderDrivers() {
            int count = SDL.GetNumRenderDrivers();
            string[] drivers = new string[count];
            for (int i = 0; i < count; i++) {
                drivers[i] = SDL.GetRenderDriver(i).ToUtf8String() ?? string.Empty;
            }
            return drivers;
        }

        /// <seealso cref="CSDL.Internal.Docs.Video.GetNumVideoDrivers">GetNumVideoDrivers</seealso>
        /// <seealso cref="CSDL.Internal.Docs.Video.GetVideoDriver">GetVideoDriver</seealso>
        private static string[] GetVideoDrivers() {
            int count = SDL.GetNumVideoDrivers();
            string[] drivers = new string[count];
            for (int i = 0; i < count; i++) {
                drivers[i] = SDL.GetVideoDriver(i).ToUtf8String() ?? string.Empty;
            }
            return drivers;
        }
    }
}
