// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Video {
    public partial class Renderer {
        private Color _clearColor = new Color(34, 54, 69);
        private Presentation? _logicalPresentation;

        /// <summary>
        /// Gets or sets the color used to clear this renderer's target in <see cref="Clear"/>.
        /// </summary>
        /// <seealso cref="Clear"/>
        public Color ClearColor {
            get => _clearColor;
            set => _clearColor = value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRendererName"/>
        public string Name => GetRendererName();

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRendererProperties"/>
        public RendererProperties Properties => new RendererProperties(GetProperties());

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetDefaultTextureScaleMode"/>
        public ScaleMode ScaleMode {
            get => GetDefaultTextureScaleMode();
            set => SetDefaultTextureScaleMode(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderViewport"/>
        public Rect Viewport {
            get => GetViewport();
            set => SetViewport(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderVSync"/>
        public int Vsync {
            get => GetVsync();
            set => SetVsync(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderClipRect"/>
        public Rect ClipRect {
            get => GetClipRect();
            set => SetClipRect(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderColorScale"/>
        public float ColorScale {
            get => GetColorScale();
            set => SetColorScale(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderDrawBlendMode"/>
        public BlendMode DrawBlendMode {
            get => GetDrawBlendMode();
            set => SetDrawBlendMode(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderDrawColor"/>
        public Color DrawColor {
            get => GetDrawColor();
            set => SetDrawColor(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderDrawColorFloat"/>
        public FColor DrawColorF {
            get => GetDrawColorFloat();
            set => SetDrawColorFloat(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderScale"/>
        public FPoint Scale {
            get => GetScale();
            set => SetScale(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderClipEnabled"/>
        public bool ClipEnabled => SDL.RenderClipEnabled(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderLogicalPresentation"/>
        public Presentation LogicalPresentation => _logicalPresentation ?? GetLogicalPresentation();

        /// <summary>
        /// <c>true</c> if this renderer is the GPU-backed renderer (see <see cref="Macros.GPURenderer"/>).
        /// </summary>
        public bool IsGpuRenderer => Name == Macros.GPURenderer;

        /// <summary>
        /// <c>true</c> if this renderer is the software renderer (see <see cref="Macros.SoftwareRenderer"/>).
        /// </summary>
        public bool IsSoftwareRenderer => Name == Macros.SoftwareRenderer;

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderDebugText"/>
        public static uint DebugTextCharacterSize => Macros.DebugTextFontCharacterSize;

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRendererName"/>
        private string GetRendererName() {
            return SDL.GetRendererName(Handle).ToUtf8StringOrLog() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRendererProperties"/>
        private uint GetProperties() {
            PropertiesID id = SDL.GetRendererProperties(Handle);
            if (id.Value == 0) {
                Error.LogError(nameof(GetProperties));
            }
            return id.Value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetDefaultTextureScaleMode"/>
        private ScaleMode GetDefaultTextureScaleMode() {
            SDL.GetDefaultTextureScaleMode(Handle, out ScaleMode mode).LogIfFalse();
            return mode;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetDefaultTextureScaleMode"/>
        private bool SetDefaultTextureScaleMode(ScaleMode mode) {
            return SDL.SetDefaultTextureScaleMode(Handle, mode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderViewport"/>
        private Rect GetViewport() {
            SDL.GetRenderViewport(Handle, out Rect rect).LogIfFalse();
            return rect;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderViewport"/>
        private bool SetViewport(Rect rect) {
            return SDL.SetRenderViewport(Handle, rect).LogIfFalse();
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderViewport"/>
        public bool SetViewport(Rect? rect) {
            ref readonly Rect rectRef = ref rect.AsRef(out Rect value);
            return SDL.SetRenderViewport(Handle, in rectRef).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderVSync"/>
        private int GetVsync() {
            SDL.GetRenderVSync(Handle, out int vsync).LogIfFalse();
            return vsync;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderVSync"/>
        private bool SetVsync(int vsync) {
            return SDL.SetRenderVSync(Handle, vsync).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderClipRect"/>
        private Rect GetClipRect() {
            SDL.GetRenderClipRect(Handle, out Rect rect).LogIfFalse();
            return rect;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderClipRect"/>
        private bool SetClipRect(Rect rect) {
            return SDL.SetRenderClipRect(Handle, rect).LogIfFalse();
        }

        /// <summary>
        /// Sets the clip rectangle, or disables clipping if <paramref name="rect"/> is <see langword="null"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderClipRect"/>
        public bool SetClipRect(Rect? rect) {
            ref readonly Rect rectRef = ref rect.AsRef(out Rect value);
            return SDL.SetRenderClipRect(Handle, in rectRef).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderColorScale"/>
        private float GetColorScale() {
            SDL.GetRenderColorScale(Handle, out float scale).LogIfFalse();
            return scale;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderColorScale"/>
        private bool SetColorScale(float scale) {
            return SDL.SetRenderColorScale(Handle, scale).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderDrawBlendMode"/>
        private BlendMode GetDrawBlendMode() {
            SDL.GetRenderDrawBlendMode(Handle, out BlendMode mode).LogIfFalse();
            return mode;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderDrawBlendMode"/>
        private bool SetDrawBlendMode(BlendMode mode) {
            return SDL.SetRenderDrawBlendMode(Handle, mode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderDrawColor"/>
        private Color GetDrawColor() {
            SDL.GetRenderDrawColor(Handle, out byte r, out byte g, out byte b, out byte a).LogIfFalse();
            return new Color(r, g, b, a);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderDrawColor"/>
        private bool SetDrawColor(Color color) {
            return SDL.SetRenderDrawColor(Handle, color.R, color.G, color.B, color.A).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderDrawColorFloat"/>
        private FColor GetDrawColorFloat() {
            SDL.GetRenderDrawColorFloat(Handle, out float r, out float g, out float b, out float a).LogIfFalse();
            return new FColor(r, g, b, a);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderDrawColorFloat"/>
        private bool SetDrawColorFloat(FColor color) {
            return SDL.SetRenderDrawColorFloat(Handle, color.R, color.G, color.B, color.A).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderLogicalPresentation"/>
        private Presentation GetLogicalPresentation() {
            SDL.GetRenderLogicalPresentation(Handle, out int w, out int h, out RendererLogicalPresentation presentation).LogIfFalse();
            return new Presentation(w, h, presentation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderLogicalPresentation"/>
        public bool SetLogicalPresentation(int width, int height, RendererLogicalPresentation presentation) {
            bool ok = SDL.SetRenderLogicalPresentation(Handle, width, height, presentation).LogIfFalse();
            if (ok) {
                _logicalPresentation = new Presentation(width, height, presentation);
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderScale"/>
        private FPoint GetScale() {
            SDL.GetRenderScale(Handle, out float x, out float y).LogIfFalse();
            return new FPoint(x, y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderScale"/>
        private bool SetScale(FPoint p) {
            return SDL.SetRenderScale(Handle, p.X, p.Y).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderLogicalPresentationRect"/>
        public FRect GetLogicalPresentationRect() {
            SDL.GetRenderLogicalPresentationRect(Handle, out FRect rect).LogIfFalse();
            return rect;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderOutputSize"/>
        public Point GetOutputSize() {
            SDL.GetRenderOutputSize(Handle, out int w, out int h).LogIfFalse();
            return new Point(w, h);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderSafeArea"/>
        public Rect GetSafeArea() {
            SDL.GetRenderSafeArea(Handle, out Rect rect).LogIfFalse();
            return rect;
        }

        public class Presentation {
            public int Height;
            public RendererLogicalPresentation RendererLogicalPresentation;
            public int Width;

            internal Presentation(int width, int height, RendererLogicalPresentation presentation) {
                Width = width;
                Height = height;
                RendererLogicalPresentation = presentation;
            }
        }
    }
}
