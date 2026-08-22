// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Video {
    public partial class Renderer {
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTexture"/>
        public Texture CreateTexture(PixelFormat format, TextureAccess access, int width, int height) {
            Texture gpuTexture = new Texture(SDL.CreateTexture(Handle, format, access, width, height).ThrowIfInvalid(), true);
            RegisterChild(gpuTexture.Invalidation);
            return gpuTexture;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTextureFromSurface"/>
        public Texture CreateTexture(Surface surface) {
            Texture texture = new Texture(SDL.CreateTextureFromSurface(Handle, surface.Handle).ThrowIfInvalid(), true);
            RegisterChild(texture.Invalidation);
            return texture;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTextureWithProperties"/>
        public Texture CreateTextureWithProperties(TextureCreateProperties props) {
            Texture texture = new Texture(SDL.CreateTextureWithProperties(Handle, props.Handle).ThrowIfInvalid(), true);
            RegisterChild(texture.Invalidation);
            return texture;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.FlushRenderer"/>
        public bool Flush() {
            return SDL.FlushRenderer(Handle).LogIfFalse();
        }

        /// <summary>
        /// Clears the current rendering target with <see cref="ClearColor"/>, without disturbing
        /// <see cref="DrawColor"/> for subsequent draw calls.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderClear"/>
        public bool Clear() {
            Color previousDrawColor = DrawColor;
            SetDrawColor(_clearColor);
            bool ok = SDL.RenderClear(Handle).LogIfFalse();
            SetDrawColor(previousDrawColor);
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetCurrentRenderOutputSize"/>
        public Point GetCurrentOutputSize() {
            SDL.GetCurrentRenderOutputSize(Handle, out int x, out int y).LogIfFalse();
            return new Point(x, y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderPresent"/>
        public bool Present() {
            return SDL.RenderPresent(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderTarget"/>
        public Texture? GetTarget() {
            NativePtr<TextureData> target = SDL.GetRenderTarget(Handle);
            if (target.IsNull) {
                return null;
            }
            return new Texture(target);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderTarget"/>
        public bool SetTarget(Texture? texture) {
            return SDL.SetRenderTarget(Handle, texture?.Handle ?? IntPtr.Zero).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderWindow"/>
        public Window GetWindow() {
            NativePtr<CSDL.Opaque.SdlWindow> w = SDL.GetRenderWindow(Handle);
            return new Window(w);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderTextureAddressMode"/>
        public bool GetTextureAddressMode(out TextureAddressMode uMode, out TextureAddressMode vMode) {
            return SDL.GetRenderTextureAddressMode(Handle, out uMode, out vMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetRenderTextureAddressMode"/>
        public bool SetTextureAddressMode(TextureAddressMode uMode, TextureAddressMode vMode) {
            return SDL.SetRenderTextureAddressMode(Handle, uMode, vMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.ConvertEventToRenderCoordinates"/>
        public bool ConvertEventToRenderCoordinates(ref Event @event) {
            return SDL.ConvertEventToRenderCoordinates(Handle, ref @event).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderCoordinatesFromWindow"/>
        public FPoint RenderCoordinatesFromWindow(float windowX, float windowY) {
            SDL.RenderCoordinatesFromWindow(Handle, windowX, windowY, out float x, out float y).LogIfFalse();
            return new FPoint(x, y);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderCoordinatesToWindow"/>
        public FPoint RenderCoordinatesToWindow(float x, float y) {
            SDL.RenderCoordinatesToWindow(Handle, x, y, out float windowX, out float windowY).LogIfFalse();
            return new FPoint(windowX, windowY);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderDebugText"/>
        public bool RenderDebugText(float x, float y, string str) {
            return SDL.RenderDebugText(Handle, x, y, str).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderDebugTextFormat"/>
        public bool RenderDebugTextFormat(float x, float y, string fmt) {
            return SDL.RenderDebugTextFormat(Handle, x, y, fmt).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderViewportSet"/>
        public bool ViewportSet() {
            return SDL.RenderViewportSet(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderReadPixels"/>
        public Surface ReadPixels(Rect area) {
            NativePtr<SurfaceData> surface = SDL.RenderReadPixels(Handle, area).ThrowIfInvalid();
            return new Surface(surface, true);
        }
    }
}
