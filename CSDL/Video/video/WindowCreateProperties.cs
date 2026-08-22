// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Video {
    public class WindowCreateProperties : PropertyGroup {
        /// <inheritdoc cref="CSDL.Props.WindowCreateAlwaysOnTopBoolean"/>
        public BooleanProperty AlwaysOnTop => PropBool(Props.WindowCreateAlwaysOnTopBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateBorderlessBoolean"/>
        public BooleanProperty Borderless => PropBool(Props.WindowCreateBorderlessBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateConstrainPopupBoolean"/>
        public BooleanProperty ConstrainPopup => PropBool(Props.WindowCreateConstrainPopupBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateFocusableBoolean"/>
        public BooleanProperty Focusable => PropBool(Props.WindowCreateFocusableBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateExternalGraphicsContextBoolean"/>
        public BooleanProperty ExternalGraphicsContext => PropBool(Props.WindowCreateExternalGraphicsContextBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateFlagsNumber"/>
        public NumberProperty Flags => PropNumber(Props.WindowCreateFlagsNumber);
        /// <inheritdoc cref="CSDL.Props.WindowCreateFullscreenBoolean"/>
        public BooleanProperty Fullscreen => PropBool(Props.WindowCreateFullscreenBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateHeightNumber"/>
        public NumberProperty Height => PropNumber(Props.WindowCreateHeightNumber);
        /// <inheritdoc cref="CSDL.Props.WindowCreateHiddenBoolean"/>
        public BooleanProperty Hidden => PropBool(Props.WindowCreateHiddenBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateHighPixelDensityBoolean"/>
        public BooleanProperty HighPixelDensity => PropBool(Props.WindowCreateHighPixelDensityBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateMaximizedBoolean"/>
        public BooleanProperty Maximized => PropBool(Props.WindowCreateMaximizedBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateMenuBoolean"/>
        public BooleanProperty Menu => PropBool(Props.WindowCreateMenuBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateMetalBoolean"/>
        public BooleanProperty Metal => PropBool(Props.WindowCreateMetalBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateMinimizedBoolean"/>
        public BooleanProperty Minimized => PropBool(Props.WindowCreateMinimizedBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateModalBoolean"/>
        public BooleanProperty Modal => PropBool(Props.WindowCreateModalBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateMouseGrabbedBoolean"/>
        public BooleanProperty MouseGrabbed => PropBool(Props.WindowCreateMouseGrabbedBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateOpenglBoolean"/>
        public BooleanProperty OpenGL => PropBool(Props.WindowCreateOpenglBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateParentPointer"/>
        public PointerProperty Parent => PropPointer(Props.WindowCreateParentPointer);
        /// <inheritdoc cref="CSDL.Props.WindowCreateResizableBoolean"/>
        public BooleanProperty Resizable => PropBool(Props.WindowCreateResizableBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateTitleString"/>
        public StringProperty Title => PropString(Props.WindowCreateTitleString);
        /// <inheritdoc cref="CSDL.Props.WindowCreateTransparentBoolean"/>
        public BooleanProperty Transparent => PropBool(Props.WindowCreateTransparentBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateTooltipBoolean"/>
        public BooleanProperty Tooltip => PropBool(Props.WindowCreateTooltipBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateUtilityBoolean"/>
        public BooleanProperty Utility => PropBool(Props.WindowCreateUtilityBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateVulkanBoolean"/>
        public BooleanProperty Vulkan => PropBool(Props.WindowCreateVulkanBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateWidthNumber"/>
        public NumberProperty Width => PropNumber(Props.WindowCreateWidthNumber);
        /// <inheritdoc cref="CSDL.Props.WindowCreateXNumber"/>
        public NumberProperty X => PropNumber(Props.WindowCreateXNumber);
        /// <inheritdoc cref="CSDL.Props.WindowCreateYNumber"/>
        public NumberProperty Y => PropNumber(Props.WindowCreateYNumber);
        /// <inheritdoc cref="CSDL.Props.WindowCreateCocoaWindowPointer"/>
        public PointerProperty CocoaWindow => PropPointer(Props.WindowCreateCocoaWindowPointer);
        /// <inheritdoc cref="CSDL.Props.WindowCreateCocoaViewPointer"/>
        public PointerProperty CocoaView => PropPointer(Props.WindowCreateCocoaViewPointer);
        /// <inheritdoc cref="CSDL.Props.WindowCreateWindowscenePointer"/>
        public PointerProperty WindowScene => PropPointer(Props.WindowCreateWindowscenePointer);
        /// <inheritdoc cref="CSDL.Props.WindowCreateWaylandSurfaceRoleCustomBoolean"/>
        public BooleanProperty WaylandSurfaceRoleCustom => PropBool(Props.WindowCreateWaylandSurfaceRoleCustomBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateWaylandCreateEGLWindowBoolean"/>
        public BooleanProperty WaylandCreateEGLWindow => PropBool(Props.WindowCreateWaylandCreateEGLWindowBoolean);
        /// <inheritdoc cref="CSDL.Props.WindowCreateWaylandWlSurfacePointer"/>
        public PointerProperty WaylandWLSurface => PropPointer(Props.WindowCreateWaylandWlSurfacePointer);
        /// <inheritdoc cref="CSDL.Props.WindowWIN32HwndPointer"/>
        public PointerProperty Win32HWND => PropPointer(Props.WindowWIN32HwndPointer);
        /// <inheritdoc cref="CSDL.Props.WindowCreateWIN32PixelFormatHwndPointer"/>
        public PointerProperty Win32PixelFormatHWND => PropPointer(Props.WindowCreateWIN32PixelFormatHwndPointer);
        /// <inheritdoc cref="CSDL.Props.WindowCreateX11WindowNumber"/>
        public NumberProperty X11Window => PropNumber(Props.WindowCreateX11WindowNumber);
        /// <inheritdoc cref="CSDL.Props.WindowCreateEmscriptenCanvasIDString"/>
        public StringProperty EmscriptenCanvasID => PropString(Props.WindowCreateEmscriptenCanvasIDString);
        /// <inheritdoc cref="CSDL.Props.WindowCreateEmscriptenKeyboardElementString"/>
        public StringProperty EmscriptenKeyboardElement => PropString(Props.WindowCreateEmscriptenKeyboardElementString);
    }
}
