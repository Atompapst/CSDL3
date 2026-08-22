// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;
using CSDL.Extensions;
namespace CSDL.Video {


    public class Texture : NativeHandle<TextureData> {


        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureSize"/>
        public FPoint Size => GetSize();

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureProperties"/>
        public TextureProperties Properties => new TextureProperties(GetProperties());

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureBlendMode"/>
        public BlendMode BlendMode {
            get => GetBlendMode();
            set => SetBlendMode(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureScaleMode"/>
        public ScaleMode ScaleMode {
            get => GetScaleMode();
            set => SetScaleMode(value);
        }

        /// <summary>
        /// Gets or Sets the AlphaMod
        /// </summary>
        /// <seealso cref="GetTextureAlphaMod"/>
        /// <seealso cref="SetTextureAlphaMod"/>
        public byte AlphaMod {
            get => GetTextureAlphaMod();
            set => SetTextureAlphaMod(value);
        }
        
        /// <summary>
        /// Gets or Sets the AlphaModFloat
        /// </summary>
        /// <seealso cref="GetTextureAlphaModFloat"/>
        /// <seealso cref="SetTextureAlphaModFloat"/>
        public float AlphaModFloat {
            get => GetTextureAlphaModFloat();
            set => SetTextureAlphaModFloat(value);
        }

        /// <summary>
        /// Gets or Sets the ColorMod
        /// </summary>
        /// <seealso cref="GetTextureColorMod"/>
        /// <seealso cref="SetTextureColorMod"/>
        public Color ColorMod {
            get => GetTextureColorMod();
            set => SetTextureColorMod(value);
        }

        /// <summary>
        /// Gets or Sets the ColorModFloat
        /// </summary>
        /// <seealso cref="GetTextureColorModFloat"/>
        /// <seealso cref="SetTextureColorModFloat"/>
        public FColor ColorModFloat {
            get => GetTextureColorModFloat();
            set => SetTextureColorModFloat(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTexture"/>
        public Texture(Renderer renderer, int width, int height, PixelFormat format, TextureAccess access) {
            CreateTexture(renderer, width, height, format, access);
            renderer.RegisterChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTextureFromSurface"/>
        public Texture(Renderer renderer, Surface surface) {
            CreateTextureFromSurface(renderer, surface);
            renderer.RegisterChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTextureWithProperties"/>
        public Texture(Renderer renderer, TextureCreateProperties properties) {
            CreateTexture(renderer, properties);
            renderer.RegisterChild(Invalidation);
        }

        internal Texture(NativePtr<TextureData> texture, bool ownsHandle = false) : base(texture, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTexture"/>
        private void CreateTexture(Renderer renderer, int width, int height, PixelFormat format, TextureAccess access) {
            Handle = SDL.CreateTexture(renderer.Handle, format, access, width, height).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTextureWithProperties"/>
        private void CreateTexture(Renderer renderer, TextureCreateProperties properties) {
            Handle = SDL.CreateTextureWithProperties(renderer.Handle, properties.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateTextureFromSurface"/>
        private void CreateTextureFromSurface(Renderer renderer, Surface surface) {
            Handle = SDL.CreateTextureFromSurface(renderer.Handle, surface.Handle.Ptr).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureSize"/>
        private FPoint GetSize() {
            SDL.GetTextureSize(Handle, out float w, out float h).LogIfFalse();
            return new FPoint(w, h);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureProperties"/>
        private uint GetProperties() {
            PropertiesID id = SDL.GetTextureProperties(Handle);
            if (id.Value == 0) {
                Error.LogError(nameof(GetProperties));
            }
            return id.Value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureBlendMode"/>
        private BlendMode GetBlendMode() {
            SDL.GetTextureBlendMode(Handle, out BlendMode mode).LogIfFalse();
            return mode;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureBlendMode"/>
        private bool SetBlendMode(BlendMode mode) {
            return SDL.SetTextureBlendMode(Handle, mode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureScaleMode"/>
        private ScaleMode GetScaleMode() {
            SDL.GetTextureScaleMode(Handle, out ScaleMode mode).LogIfFalse();
            return mode;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureScaleMode"/>
        private bool SetScaleMode(ScaleMode mode) {
            return SDL.SetTextureScaleMode(Handle, mode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureAlphaMod"/>
        private byte GetTextureAlphaMod() {
            SDL.GetTextureAlphaMod(Handle, out byte alphaMod).LogIfFalse();
            return alphaMod;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureAlphaMod"/>
        private bool SetTextureAlphaMod(byte alphaMod) {
            return SDL.SetTextureAlphaMod(Handle, alphaMod).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureAlphaModFloat"/>
        private float GetTextureAlphaModFloat() {
            SDL.GetTextureAlphaModFloat(Handle, out float alphaMod).LogIfFalse();
            return alphaMod;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureAlphaModFloat"/>
        private bool SetTextureAlphaModFloat(float alphaMod) {
            return SDL.SetTextureAlphaModFloat(Handle, alphaMod).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureColorMod"/>
        private Color GetTextureColorMod() {
            SDL.GetTextureColorMod(Handle, out byte r, out byte g, out byte b).LogIfFalse();
            return new Color(r, g, b);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureColorMod"/>
        private bool SetTextureColorMod(Color color) {
            return SDL.SetTextureColorMod(Handle, color.R, color.G, color.B).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTextureColorModFloat"/>
        private FColor GetTextureColorModFloat() {
            SDL.GetTextureColorModFloat(Handle, out float r, out float g, out float b).LogIfFalse();
            return new FColor(r, g, b);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTextureColorModFloat"/>
        private bool SetTextureColorModFloat(FColor color) {
            return SDL.SetTextureColorModFloat(Handle, color.R, color.G, color.B).LogIfFalse();
        }

        /// <summary>
        /// The palette used with this texture, for palettized (indexed) pixel formats.
        /// </summary>
        public Palette? Palette {
            get => GetPalette();
            set => SetPalette(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetTexturePalette"/>
        private Palette? GetPalette() {
            NativePtr<PaletteData> palette = SDL.GetTexturePalette(Handle);
            return palette.IsNull ? null : new Palette(palette);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetTexturePalette"/>
        private bool SetPalette(Palette? palette) {
            return SDL.SetTexturePalette(Handle, palette?.Handle ?? NativePtr<PaletteData>.Zero).LogIfFalse();
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Render.LockTexture"/>
        public bool Lock(Rect? rect, out IntPtr pixels, out int pitch) {
            Rect rectVal = rect.GetValueOrDefault();
            ref readonly Rect rectRef = ref rect.HasValue ? ref rectVal : ref Unsafe.NullRef<Rect>();
            return SDL.LockTexture(Handle, in rectRef, out pixels, out pitch).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.LockTextureToSurface"/>
        public Surface? LockToSurface(Rect? rect) {
            Rect rectVal = rect.GetValueOrDefault();
            ref readonly Rect rectRef = ref rect.HasValue ? ref rectVal : ref Unsafe.NullRef<Rect>();
            nint surface = 0;
            if (!SDL.LockTextureToSurface(Handle, in rectRef, NativePtr<nint>.FromRef(ref surface)).LogIfFalse()) {
                return null;
            }
            return new Surface(surface);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.UnlockTexture"/>
        public void Unlock() {
            SDL.UnlockTexture(Handle);
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Render.UpdateTexture"/>
        public bool Update(Rect? rect, IntPtr pixels, int pitch) {
            Rect rectVal = rect.GetValueOrDefault();
            ref readonly Rect rectRef = ref rect.HasValue ? ref rectVal : ref Unsafe.NullRef<Rect>();
            return SDL.UpdateTexture(Handle, in rectRef, pixels, pitch).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.UpdateNVTexture"/>
        public bool UpdateNV(Rect? rect, IntPtr yPlane, int yPitch, IntPtr uvPlane, int uvPitch) {
            Rect rectVal = rect.GetValueOrDefault();
            NativePtr<Rect> rectPtr = rect.HasValue ? NativePtr<Rect>.FromRef(ref rectVal) : NativePtr<Rect>.Zero;
            return SDL.UpdateNVTexture(Handle, rectPtr, yPlane, yPitch, uvPlane, uvPitch).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.UpdateYUVTexture"/>
        public bool UpdateYUV(Rect? rect, IntPtr yPlane, int yPitch, IntPtr uPlane, int uPitch, IntPtr vPlane, int vPitch) {
            Rect rectVal = rect.GetValueOrDefault();
            NativePtr<Rect> rectPtr = rect.HasValue ? NativePtr<Rect>.FromRef(ref rectVal) : NativePtr<Rect>.Zero;
            return SDL.UpdateYUVTexture(Handle, rectPtr, yPlane, yPitch, uPlane, uPitch, vPlane, vPitch).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRendererFromTexture"/>
        public Renderer? GetRenderer() {
            NativePtr<CSDL.Opaque.SdlRenderer> renderer = SDL.GetRendererFromTexture(Handle).LogIfInvalid();
            return renderer.IsNull ? null : new Renderer(renderer);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.DestroyTexture"/>
        protected override void DisposeResource() {
            SDL.DestroyTexture(Handle);
        }
    }
}
