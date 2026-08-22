// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Properties;
namespace CSDL.Video {
    public class WindowProperties : PropertyGroup {

        internal WindowProperties(uint handle) : base(handle) { }
        /// <inheritdoc cref="CSDL.Props.WindowShapePointer"/>
        public PointerProperty Shape => PropPointer(Props.WindowShapePointer);

        /// <inheritdoc cref="CSDL.Props.WindowHDREnabledBoolean"/>
        public BooleanProperty HDREnabled => PropBool(Props.WindowHDREnabledBoolean);

        /// <inheritdoc cref="CSDL.Props.WindowSdrWhiteLevelFloat"/>
        public NumberProperty SDRWhiteLevel => PropNumber(Props.WindowSdrWhiteLevelFloat);

        /// <inheritdoc cref="CSDL.Props.WindowHDRHeadroomFloat"/>
        public NumberProperty HDRHeadroom => PropNumber(Props.WindowHDRHeadroomFloat);

        /// <inheritdoc cref="CSDL.Props.WindowAndroidWindowPointer"/>
        public PointerProperty AndroidWindow => PropPointer(Props.WindowAndroidWindowPointer);

        /// <inheritdoc cref="CSDL.Props.WindowAndroidSurfacePointer"/>
        public PointerProperty AndroidSurface => PropPointer(Props.WindowAndroidSurfacePointer);

        /// <inheritdoc cref="CSDL.Props.WindowUikitWindowPointer"/>
        public PointerProperty UIKitWindow => PropPointer(Props.WindowUikitWindowPointer);

        /// <inheritdoc cref="CSDL.Props.WindowUikitMetalViewTagNumber"/>
        public NumberProperty UIKitMetalViewTag => PropNumber(Props.WindowUikitMetalViewTagNumber);

        /// <inheritdoc cref="CSDL.Props.WindowUikitOpenglFramebufferNumber"/>
        public NumberProperty UIKitOpenGLFramebuffer => PropNumber(Props.WindowUikitOpenglFramebufferNumber);

        /// <inheritdoc cref="CSDL.Props.WindowUikitOpenglRenderbufferNumber"/>
        public NumberProperty UIKitOpenGLRenderbuffer => PropNumber(Props.WindowUikitOpenglRenderbufferNumber);

        /// <inheritdoc cref="CSDL.Props.WindowUikitOpenglResolveFramebufferNumber"/>
        public NumberProperty UIKitOpenGLResolveFramebuffer => PropNumber(Props.WindowUikitOpenglResolveFramebufferNumber);

        /// <inheritdoc cref="CSDL.Props.WindowKMSDRMDeviceIndexNumber"/>
        public NumberProperty KMSDRMDeviceIndex => PropNumber(Props.WindowKMSDRMDeviceIndexNumber);

        /// <inheritdoc cref="CSDL.Props.WindowKMSDRMDrmFdNumber"/>
        public NumberProperty KMSDRMDrmFD => PropNumber(Props.WindowKMSDRMDrmFdNumber);

        /// <inheritdoc cref="CSDL.Props.WindowKMSDRMGbmDevicePointer"/>
        public PointerProperty KMSDRMGBMDevice => PropPointer(Props.WindowKMSDRMGbmDevicePointer);

        /// <inheritdoc cref="CSDL.Props.WindowCocoaWindowPointer"/>
        public PointerProperty CocoaWindow => PropPointer(Props.WindowCocoaWindowPointer);

        /// <inheritdoc cref="CSDL.Props.WindowCocoaMetalViewTagNumber"/>
        public NumberProperty CocoaMetalViewTag => PropNumber(Props.WindowCocoaMetalViewTagNumber);

        /// <inheritdoc cref="CSDL.Props.WindowOpenvrOverlayIDNumber"/>
        public NumberProperty OpenVROverlayId => PropNumber(Props.WindowOpenvrOverlayIDNumber);

        /// <inheritdoc cref="CSDL.Props.WindowQnxWindowPointer"/>
        public PointerProperty QNXWindow => PropPointer(Props.WindowQnxWindowPointer);

        /// <inheritdoc cref="CSDL.Props.WindowQnxSurfacePointer"/>
        public PointerProperty QNXSurface => PropPointer(Props.WindowQnxSurfacePointer);

        /// <inheritdoc cref="CSDL.Props.WindowVivanteDisplayPointer"/>
        public PointerProperty VivanteDisplay => PropPointer(Props.WindowVivanteDisplayPointer);

        /// <inheritdoc cref="CSDL.Props.WindowVivanteWindowPointer"/>
        public PointerProperty VivanteWindow => PropPointer(Props.WindowVivanteWindowPointer);

        /// <inheritdoc cref="CSDL.Props.WindowVivanteSurfacePointer"/>
        public PointerProperty VivanteSurface => PropPointer(Props.WindowVivanteSurfacePointer);

        /// <inheritdoc cref="CSDL.Props.WindowWIN32HwndPointer"/>
        public PointerProperty Win32HWND => PropPointer(Props.WindowWIN32HwndPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWIN32HdcPointer"/>
        public PointerProperty Win32HDC => PropPointer(Props.WindowWIN32HdcPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWIN32InstancePointer"/>
        public PointerProperty Win32Instance => PropPointer(Props.WindowWIN32InstancePointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandDisplayPointer"/>
        public PointerProperty WaylandDisplay => PropPointer(Props.WindowWaylandDisplayPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandSurfacePointer"/>
        public PointerProperty WaylandSurface => PropPointer(Props.WindowWaylandSurfacePointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandViewportPointer"/>
        public PointerProperty WaylandViewport => PropPointer(Props.WindowWaylandViewportPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandEGLWindowPointer"/>
        public PointerProperty WaylandEGLWindow => PropPointer(Props.WindowWaylandEGLWindowPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandXdgSurfacePointer"/>
        public PointerProperty WaylandXDgSurface => PropPointer(Props.WindowWaylandXdgSurfacePointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandXdgToplevelPointer"/>
        public PointerProperty WaylandXDgToplevel => PropPointer(Props.WindowWaylandXdgToplevelPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandXdgToplevelExportHandleString"/>
        public StringProperty WaylandXDgToplevelExportHandle => PropString(Props.WindowWaylandXdgToplevelExportHandleString);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandXdgPopupPointer"/>
        public PointerProperty WaylandXDgPopup => PropPointer(Props.WindowWaylandXdgPopupPointer);

        /// <inheritdoc cref="CSDL.Props.WindowWaylandXdgPositionerPointer"/>
        public PointerProperty WaylandXDgPositioner => PropPointer(Props.WindowWaylandXdgPositionerPointer);

        /// <inheritdoc cref="CSDL.Props.WindowX11DisplayPointer"/>
        public PointerProperty X11Display => PropPointer(Props.WindowX11DisplayPointer);

        /// <inheritdoc cref="CSDL.Props.WindowX11ScreenNumber"/>
        public NumberProperty X11Screen => PropNumber(Props.WindowX11ScreenNumber);

        /// <inheritdoc cref="CSDL.Props.WindowX11WindowNumber"/>
        public NumberProperty X11Window => PropNumber(Props.WindowX11WindowNumber);

        /// <inheritdoc cref="CSDL.Props.WindowEmscriptenCanvasIDString"/>
        public StringProperty EmscriptenCanvasID => PropString(Props.WindowEmscriptenCanvasIDString);

        /// <inheritdoc cref="CSDL.Props.WindowEmscriptenKeyboardElementString"/>
        public StringProperty EmscriptenKeyboardElement => PropString(Props.WindowEmscriptenKeyboardElementString);
    }
}
