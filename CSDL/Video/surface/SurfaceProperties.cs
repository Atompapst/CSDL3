// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Properties;
namespace CSDL.Video {
    /// <summary>
    /// The properties SDL keeps for an existing <see cref="Surface"/>.
    /// </summary>
    /// <remarks>
    /// This group is created and owned by SDL and lives as long as the surface does, so it must not be
    /// disposed - the finalizer that would otherwise destroy it is suppressed.
    /// </remarks>
    /// <seealso cref="Surface.Properties"/>
    public sealed class SurfaceProperties : PropertyGroup {
        internal SurfaceProperties(uint handle) : base(handle) {
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="CSDL.Props.SurfaceSdrWhitePointFloat"/>
        public FloatProperty SDRWhitePoint => PropFloat(Props.SurfaceSdrWhitePointFloat);

        /// <inheritdoc cref="CSDL.Props.SurfaceHDRHeadroomFloat"/>
        public FloatProperty HDRHeadroom => PropFloat(Props.SurfaceHDRHeadroomFloat);

        /// <inheritdoc cref="CSDL.Props.SurfaceTonemapOperatorString"/>
        public StringProperty TonemapOperator => PropString(Props.SurfaceTonemapOperatorString);

        /// <inheritdoc cref="CSDL.Props.SurfaceHotspotXNumber"/>
        public NumberProperty HotspotX => PropNumber(Props.SurfaceHotspotXNumber);

        /// <inheritdoc cref="CSDL.Props.SurfaceHotspotYNumber"/>
        public NumberProperty HotspotY => PropNumber(Props.SurfaceHotspotYNumber);

        /// <inheritdoc cref="CSDL.Props.SurfaceRotationFloat"/>
        public FloatProperty Rotation => PropFloat(Props.SurfaceRotationFloat);
    }
}
