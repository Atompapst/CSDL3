// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.Video {
    public sealed partial class Window {
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowSizeInPixels"/>
        public Point SizeInPixels => GetWindowSizeInPixels();
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowPixelDensity"/>
        public float PixelDensity => GetWindowPixelDensity();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowAspectRatio"/>
        public (float Min, float Max) AspectRatio {
            get => GetWindowAspectRatio();
            set => SetWindowAspectRatio(value.Min, value.Max);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowBordersSize"/>
        public (int Top, int Left, int Bottom, int Right) BordersSize => GetWindowBordersSize();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMinimumSize"/>
        public Point MinimumSize {
            get => GetWindowMinimumSize();
            set => SetWindowMinimumSize(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMaximumSize"/>
        public Point MaximumSize {
            get => GetWindowMaximumSize();
            set => SetWindowMaximumSize(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowFullscreenMode"/>
        public DisplayMode? FullscreenMode {
            get => GetWindowFullscreenMode();
            set => SetWindowFullscreenMode(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowID"/>
        public uint Id => GetId();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowProperties"/>
        public WindowProperties Properties => new WindowProperties(GetProperties());

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowSize"/>
        public Point Size {
            get => GetWindowSize();
            set => SetWindowSize(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.SetWindowRelativeMouseMode"/>
        public bool RelativeMouseMode {
            get => GetWindowRelativeMouseMode();
            set => SetWindowRelativeMouseMode(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowSafeArea"/>
        public Rect SafeArea => GetWindowSafeArea();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowTitle"/>
        public string Title {
            get => GetTitle();
            set => SetTitle(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowPixelFormat"/>
        public PixelFormat PixelFormat => GetWindowPixelFormat();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowPosition"/>
        public Point Position {
            get => GetWindowPosition();
            set => SetWindowPosition(value);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposCenteredMask"/>
        public const uint CenteredMask = Macros.WindowposCenteredMask;

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposUndefinedMask"/>
        public const uint UndefinedMask = Macros.WindowposUndefinedMask;

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposCentered"/>
        public static readonly Point Centered = new Point((int)Macros.WindowposCentered, (int)Macros.WindowposCentered);

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposUndefined"/>
        public static readonly Point Undefined = new Point((int)Macros.WindowposUndefined, (int)Macros.WindowposUndefined);

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposCenteredDisplay"/>
        public static Point CenteredOn(DisplayID display) {
            int pos = (int)Macros.WindowposCenteredDisplay(display);
            return new Point(pos, pos);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposUndefinedDisplay"/>
        public static Point UndefinedOn(DisplayID display) {
            int pos = (int)Macros.WindowposUndefinedDisplay(display);
            return new Point(pos, pos);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposIscentered"/>
        public static bool IsCentered(Point position) {
            return Macros.WindowposIscentered((uint)position.X) && Macros.WindowposIscentered((uint)position.Y);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.WindowposIsundefined"/>
        public static bool IsUndefined(Point position) {
            return Macros.WindowposIsundefined((uint)position.X) && Macros.WindowposIsundefined((uint)position.Y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowDisplayScale"/>
        public float DisplayScale => GetWindowDisplayScale();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowOpacity"/>
        public float Opacity {
            get => GetWindowOpacity();
            set => SetWindowOpacity(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMouseRect"/>
        public Rect? MouseRect {
            get {
                NativePtr<Rect> ptr = SDL.GetWindowMouseRect(Handle);
                return ptr.IsNull ? null : ptr.Read();
            }
            set {
                ref readonly Rect rectRef = ref value.AsRef(out Rect rectVal);
                SDL.SetWindowMouseRect(Handle, in rectRef).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowProgressState"/>
        public ProgressState ProgressState {
            get => SDL.GetWindowProgressState(Handle);
            set => SDL.SetWindowProgressState(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowProgressValue"/>
        public float ProgressValue {
            get => SDL.GetWindowProgressValue(Handle);
            set => SDL.SetWindowProgressValue(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowTitle"/>
        private string GetTitle() {
            return SDL.GetWindowTitle(Handle).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowTitle"/>
        private bool SetTitle(string title) {
            return SDL.SetWindowTitle(Handle, title).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowIcon"/>
        public bool SetIcon(Surface icon) {
            return SDL.SetWindowIcon(Handle, icon.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowPosition"/>
        private bool SetWindowPosition(Point position) {
            return SDL.SetWindowPosition(Handle, position.X, position.Y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowPosition"/>
        private Point GetWindowPosition() {
            SDL.GetWindowPosition(Handle, out int x, out int y).LogIfFalse();
            return new Point(x, y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowSize"/>
        private bool SetWindowSize(Point size) {
            bool resized = SDL.SetWindowSize(Handle, size.X, size.Y).LogIfFalse();
            if (resized) {
                InvalidateWindowSurfaces();
            }
            return resized;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowSize"/>
        private Point GetWindowSize() {
            SDL.GetWindowSize(Handle, out int x, out int y).LogIfFalse();
            return new Point(x, y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowSafeArea"/>
        private Rect GetWindowSafeArea() {
            SDL.GetWindowSafeArea(Handle, out Rect rect).LogIfFalse();
            return rect;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowFullscreenMode"/>
        private DisplayMode? GetWindowFullscreenMode() {
            NativePtr<DisplayMode> ptr = SDL.GetWindowFullscreenMode(Handle);
            return ptr.IsNull ? null : ptr.Read();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowFullscreenMode"/>
        private bool SetWindowFullscreenMode(DisplayMode? mode) {
            ref readonly DisplayMode modeRef = ref mode.AsRef(out DisplayMode modeVal);
            return SDL.SetWindowFullscreenMode(Handle, in modeRef).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetWindowRelativeMouseMode"/>
        private bool GetWindowRelativeMouseMode() {
            return Input.SDL.GetWindowRelativeMouseMode(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.SetWindowRelativeMouseMode"/>
        private bool SetWindowRelativeMouseMode(bool relativeMouseMode) {
            return Input.SDL.SetWindowRelativeMouseMode(Handle, relativeMouseMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowID"/>
        private uint GetId() {
            uint id = SDL.GetWindowID(Handle);
            if (id == 0) {
                Error.LogError(nameof(GetId));
            }
            return id;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowProperties"/>
        private uint GetProperties() {
            uint res = SDL.GetWindowProperties(Handle);
            if (res == 0) {
                Error.LogError(nameof(GetProperties));
            }
            return res;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowParent"/>
        public Window? Parent => GetParent();

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowParent"/>
        private Window? GetParent() {
            // NULL means "this window has no parent"
            NativePtr<CSDL.Opaque.SdlWindow> ptr = SDL.GetWindowParent(Handle);
            return ptr.IsNull ? null : new Window(ptr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowSizeInPixels"/>
        private Point GetWindowSizeInPixels() {
            int w = 0, h = 0;
            SDL.GetWindowSizeInPixels(Handle, NativePtr<int>.FromRef(ref w), NativePtr<int>.FromRef(ref h)).LogIfFalse();
            return new Point(w, h);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowDisplayScale"/>
        private float GetWindowDisplayScale() {
            return SDL.GetWindowDisplayScale(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowPixelDensity"/>
        private float GetWindowPixelDensity() {
            return SDL.GetWindowPixelDensity(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowPixelFormat"/>
        private PixelFormat GetWindowPixelFormat() {
            return SDL.GetWindowPixelFormat(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowOpacity"/>
        private float GetWindowOpacity() {
            return SDL.GetWindowOpacity(Handle).LogIfInvalid(-1.0f);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowOpacity"/>
        private bool SetWindowOpacity(float opacity) {
            return SDL.SetWindowOpacity(Handle, opacity).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowAspectRatio"/>
        private bool SetWindowAspectRatio(float min, float max) {
            return SDL.SetWindowAspectRatio(Handle, min, max).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowAspectRatio"/>
        private (float Min, float Max) GetWindowAspectRatio() {
            SDL.GetWindowAspectRatio(Handle, out float min, out float max).LogIfFalse();
            return (min, max);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowBordersSize"/>
        private (int Top, int Left, int Bottom, int Right) GetWindowBordersSize() {
            int top = 0, left = 0, bottom = 0, right = 0;
            SDL.GetWindowBordersSize(
                Handle,
                NativePtr<int>.FromRef(ref top),
                NativePtr<int>.FromRef(ref left),
                NativePtr<int>.FromRef(ref bottom),
                NativePtr<int>.FromRef(ref right)).LogIfFalse();
            return (top, left, bottom, right);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMinimumSize"/>
        private bool SetWindowMinimumSize(Point size) {
            return SDL.SetWindowMinimumSize(Handle, size.X, size.Y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowMinimumSize"/>
        private Point GetWindowMinimumSize() {
            SDL.GetWindowMinimumSize(Handle, out int w, out int h).LogIfFalse();
            return new Point(w, h);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.SetWindowMaximumSize"/>
        private bool SetWindowMaximumSize(Point size) {
            return SDL.SetWindowMaximumSize(Handle, size.X, size.Y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GetWindowMaximumSize"/>
        private Point GetWindowMaximumSize() {
            SDL.GetWindowMaximumSize(Handle, out int w, out int h).LogIfFalse();
            return new Point(w, h);
        }
    }
}
