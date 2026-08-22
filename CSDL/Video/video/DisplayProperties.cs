// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Video {
    public class DisplayProperties : PropertyGroup {
        internal DisplayProperties(uint handle) : base(handle) { }

        /// <inheritdoc cref="CSDL.Props.DisplayHDREnabledBoolean"/>
        public BooleanProperty HDREnabled => PropBool(Props.DisplayHDREnabledBoolean);

        /// <inheritdoc cref="CSDL.Props.DisplayKMSDRMPanelOrientationNumber"/>
        public NumberProperty KMSDRMPanelOrientation => PropNumber(Props.DisplayKMSDRMPanelOrientationNumber);

        /// <inheritdoc cref="CSDL.Props.DisplayWaylandWlOutputPointer"/>
        public PointerProperty WaylandWLOutput => PropPointer(Props.DisplayWaylandWlOutputPointer);

        /// <inheritdoc cref="CSDL.Props.DisplayWindowsHmonitorPointer"/>
        public PointerProperty WindowsHMonitor => PropPointer(Props.DisplayWindowsHmonitorPointer);
    }
}
