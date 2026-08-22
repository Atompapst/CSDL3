// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;
using CSDL.Extensions;
using CSDL.File;

namespace CSDL.Video {
    public partial class Surface : NativeHandle<SurfaceData> {
        public Surface(int width, int height, PixelFormat format) {
            CreateSurface(width, height, format);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.CreateSurfaceFrom"/>
        /// <remarks>
        ///     No copy is made of <paramref name="pixels"/>: the caller owns that memory and must keep it
        ///     alive until this surface has been disposed.
        /// </remarks>
        public Surface(int width, int height, PixelFormat format, IntPtr pixels, int pitch) {
            CreateSurfaceFrom(width, height, format, pixels, pitch);
        }

        internal Surface(NativePtr<SurfaceData> canvas, bool ownsHandle = false) : base(canvas, ownsHandle) {
            Handle = canvas;
        }
        
        /// <inheritdoc cref="CSDL.Video.SurfaceData.W"/>
        public int Width => Ref.W;

        /// <inheritdoc cref="CSDL.Video.SurfaceData.H"/>
        public int Height => Ref.H;

        /// <inheritdoc cref="CSDL.Video.SurfaceData.Pitch"/>
        public int Pitch  => Ref.Pitch;

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceBlendMode"/>
        public BlendMode BlendMode {
            get => GetBlendMode();
            set => SetBlendMode(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceAlphaMod"/>
        public byte AlphaMod {
            get => GetAlphaMod();
            set => SetAlphaMod(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceColorMod"/>
        public (byte R, byte G, byte B) ColorMod {
            get {
                SDL.GetSurfaceColorMod(Handle, out byte r, out byte g, out byte b).LogIfFalse();
                return (r, g, b);
            }
            set => SDL.SetSurfaceColorMod(Handle, value.R, value.G, value.B).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceColorKey"/>
        public uint? ColorKey {
            get {
                if (!HasColorKey) return null;
                SDL.GetSurfaceColorKey(Handle, out uint key).LogIfFalse();
                return key;
            }
            set => SDL.SetSurfaceColorKey(Handle, value.HasValue, value.GetValueOrDefault()).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfaceColorspace"/>
        public Colorspace Colorspace {
            get => SDL.GetSurfaceColorspace(Handle);
            set => SetColorspace(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfacePalette"/>
        public Palette? Palette {
            get => GetPalette();
            set => SetPalette(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfaceProperties"/>
        public SurfaceProperties Properties => new SurfaceProperties(GetProperties());

        /// <summary>
        ///     Gets the pixel data pointer.
        /// </summary>
        public IntPtr Pixels => Ref.Pixels;

        /// <summary>
        ///     Gets the pixel format of the surface.
        /// </summary>
        public PixelFormat Format => Ref.Format;

        public SurfaceFlags Flags => Ref.Flags;

        /// <summary>
        ///     Gets or sets the reference count.
        /// </summary>
        public int RefCount {
            get => Handle.AsRef().Refcount;
            set => Handle.AsRef().Refcount = value;
        }

        /// <summary>
        ///     Gets the size of the surface in pixels.
        /// </summary>
        public Point Size => new Point(Width, Height);

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.CreateSurface"/>
        private void CreateSurface(int width, int height, PixelFormat format) {
            Handle = SDL.CreateSurface(width, height, format).LogIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.CreateSurfaceFrom"/>
        private void CreateSurfaceFrom(int width, int height, PixelFormat format, IntPtr pixels, int pitch) {
            Handle = SDL.CreateSurfaceFrom(width, height, format, pixels, pitch).LogIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Mustlock"/>
        private bool MustLock() {
            return Macros.Mustlock(ref Ref);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.LockSurface"/>
        public bool Lock() {
            if (!MustLock()) {
                Log.Debug("Surface does not require locking.");
                return true;
            }
            return SDL.LockSurface(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.UnlockSurface"/>
        public void Unlock() {
            SDL.UnlockSurface(Handle);
        }

        /// <summary>
        ///     Performs a fast surface copy to a destination surface.
        /// </summary>
        /// <param name="dst">The destination surface.</param>
        public bool Blit(Surface dst) {
            ref readonly Rect nullRect = ref Unsafe.NullRef<Rect>();
            return SDL.BlitSurface(Handle, in nullRect, dst.Handle, in nullRect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurfaceUnchecked"/>
        public bool BlitUnchecked(Rect srcRect, Surface dst, Rect dstRect) {
            return SDL.BlitSurfaceUnchecked(Handle, in srcRect, dst.Handle, in dstRect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurfaceUncheckedScaled"/>
        public bool BlitUncheckedScaled(Rect srcRect, Surface dst, Rect dstRect, ScaleMode scaleMode) {
            return SDL.BlitSurfaceUncheckedScaled(Handle, in srcRect, dst.Handle, in dstRect, scaleMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurfaceTiled"/>
        public bool BlitTiled(Rect srcRect, Surface dst, Rect dstRect) {
            return SDL.BlitSurfaceTiled(Handle, in srcRect, dst.Handle, in dstRect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurfaceTiledWithScale"/>
        public bool BlitTiled(Rect srcRect, float scale, ScaleMode scaleMode, Surface dst, Rect dstRect) {
            return SDL.BlitSurfaceTiledWithScale(Handle, in srcRect, scale, scaleMode, dst.Handle, in dstRect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurface9Grid"/>
        public bool Blit9Grid(Rect srcRect, int leftWidth, int rightWidth, int topHeight, int bottomHeight, float scale, ScaleMode scaleMode, Surface dst, Rect dstRect) {
            return SDL.BlitSurface9Grid(Handle, in srcRect, leftWidth, rightWidth, topHeight, bottomHeight, scale, scaleMode, dst.Handle, in dstRect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.StretchSurface"/>
        public bool Stretch(Rect srcRect, Surface dst, Rect dstRect, ScaleMode scaleMode) {
            return SDL.StretchSurface(Handle, in srcRect, dst.Handle, in dstRect, scaleMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SurfaceHasAlternateImages"/>
        public bool HasAlternateImages => SDL.SurfaceHasAlternateImages(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.AddSurfaceAlternateImage"/>
        public bool AddAlternateImage(Surface image) {
            return SDL.AddSurfaceAlternateImage(Handle, image.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfaceImages"/>
        public Surface[] GetAlternateImages() {
            IntPtr ptr = SDL.GetSurfaceImages(Handle, out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetAlternateImages));
                return Array.Empty<Surface>();
            }

            NativePtr<NativePtr<SurfaceData>> images = ptr;
            Surface[] result = new Surface[count];
            for (int i = 0; i < count; i++) {
                result[i] = new Surface(images[i]);
            }
            Memory.Free(ptr);
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.RemoveSurfaceAlternateImages"/>
        public void RemoveAlternateImages() {
            SDL.RemoveSurfaceAlternateImages(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SurfaceHasColorKey"/>
        public bool HasColorKey => SDL.SurfaceHasColorKey(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceRLE"/>
        public bool RLE {
            get => SDL.SurfaceHasRLE(Handle);
            set => SDL.SetSurfaceRLE(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceColorspace"/>
        public bool SetColorspace(Colorspace colorspace) {
            return SDL.SetSurfaceColorspace(Handle, colorspace).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfacePalette"/>
        public bool SetPalette(Palette? palette) {
            return SDL.SetSurfacePalette(Handle, palette?.Handle ?? NativePtr<PaletteData>.Zero).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfacePalette"/>
        private Palette? GetPalette() {
            NativePtr<PaletteData> palette = SDL.GetSurfacePalette(Handle);
            return palette.IsNull ? null : new Palette(palette);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.CreateSurfacePalette"/>
        public Palette CreatePalette() {
            // SDL keeps a reference of its own and frees the palette with the surface, so the wrapper
            // returned here deliberately doesn't own the handle.
            return new Palette(SDL.CreateSurfacePalette(Handle).ThrowIfInvalid());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfaceProperties"/>
        private uint GetProperties() {
            PropertiesID id = SDL.GetSurfaceProperties(Handle);
            if (id.Value == 0) {
                Error.LogError(nameof(GetProperties));
            }
            return id.Value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.PremultiplySurfaceAlpha"/>
        public bool PremultiplyAlpha(bool linear) {
            return SDL.PremultiplySurfaceAlpha(Handle, linear).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ReadSurfacePixelFloat"/>
        public bool ReadPixelFloat(int x, int y, out float r, out float g, out float b, out float a) {
            return SDL.ReadSurfacePixelFloat(Handle, x, y, out r, out g, out b, out a).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.WriteSurfacePixel"/>
        public bool WritePixel(int x, int y, byte r, byte g, byte b, byte a) {
            return SDL.WriteSurfacePixel(Handle, x, y, r, g, b, a).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.WriteSurfacePixelFloat"/>
        public bool WritePixelFloat(int x, int y, float r, float g, float b, float a) {
            return SDL.WriteSurfacePixelFloat(Handle, x, y, r, g, b, a).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SaveBMP"/>
        public bool SaveBMP(string file) {
            return SDL.SaveBMP(Handle, file).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SaveBMP_IO"/>
        public bool SaveBMP(IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SaveBMP_IO(Handle, destination.Handle, closeAfter));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SavePNG"/>
        public bool SavePNG(string file) {
            return SDL.SavePNG(Handle, file).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SavePNG_IO"/>
        public bool SavePNG(IOStream destination, bool closeAfter = false) {
            return CompleteStreamSave(destination, closeAfter, SDL.SavePNG_IO(Handle, destination.Handle, closeAfter));
        }

        // SDL closes a stream passed with closeio=true, so prevent IOStream.Dispose from closing it again.
        private static bool CompleteStreamSave(IOStream destination, bool closeAfter, CBool result) {
            if (closeAfter) {
                destination.Handle = NativePtr<CSDL.Opaque.SdlIOStream>.Zero;
            }

            return result.LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ConvertPixels"/>
        public static bool ConvertPixels(int width, int height, PixelFormat srcFormat, IntPtr src, int srcPitch, PixelFormat dstFormat, IntPtr dst, int dstPitch) {
            return SDL.ConvertPixels(width, height, srcFormat, src, srcPitch, dstFormat, dst, dstPitch).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ConvertPixelsAndColorspace"/>
        public static bool ConvertPixelsAndColorspace(int width, int height, PixelFormat srcFormat, Colorspace srcColorspace, PropertiesID srcProperties, IntPtr src, int srcPitch, PixelFormat dstFormat, Colorspace dstColorspace, PropertiesID dstProperties, IntPtr dst, int dstPitch) {
            return SDL.ConvertPixelsAndColorspace(width, height, srcFormat, srcColorspace, srcProperties, src, srcPitch, dstFormat, dstColorspace, dstProperties, dst, dstPitch).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.PremultiplyAlpha"/>
        public static bool PremultiplyAlpha(int width, int height, PixelFormat srcFormat, IntPtr src, int srcPitch, PixelFormat dstFormat, IntPtr dst, int dstPitch, bool linear) {
            return SDL.PremultiplyAlpha(width, height, srcFormat, src, srcPitch, dstFormat, dst, dstPitch, linear).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurfaceScaled"/>
        public bool BlitScaled(Surface dst) {
            return BlitScaled(null, dst, null);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.BlitSurfaceScaled"/>
        public bool BlitScaled(Rect? srcRect, Surface dst, Rect? dstRect, ScaleMode scaleMode = ScaleMode.Nearest) {
            Rect srcVal = srcRect.GetValueOrDefault();
            Rect dstVal = dstRect.GetValueOrDefault();
            ref readonly Rect srcRef = ref srcRect.HasValue ? ref srcVal : ref Unsafe.NullRef<Rect>();
            ref readonly Rect dstRef = ref dstRect.HasValue ? ref dstVal : ref Unsafe.NullRef<Rect>();
            return SDL.BlitSurfaceScaled(Handle, in srcRef, dst.Handle, in dstRef, scaleMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.DuplicateSurface"/>
        public Surface Duplicate() {
            return new Surface(SDL.DuplicateSurface(Handle).LogIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ConvertSurface"/>
        public Surface Convert(PixelFormat format) {
            return new Surface(SDL.ConvertSurface(Handle, format).LogIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ConvertSurfaceAndColorspace"/>
        public Surface Convert(PixelFormat format, Colorspace colorspace, Palette? palette = null, PropertiesID properties = default) {
            NativePtr<SurfaceData> surface = SDL.ConvertSurfaceAndColorspace(Handle, format, palette?.Handle ?? NativePtr<PaletteData>.Zero, colorspace, properties).LogIfInvalid();
            return new Surface(surface, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.RotateSurface"/>
        public Surface Rotate(float angle) {
            return new Surface(SDL.RotateSurface(Handle, angle).LogIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ScaleSurface"/>
        public Surface Scale(int width, int height, ScaleMode scaleMode = ScaleMode.Nearest) {
            return new Surface(SDL.ScaleSurface(Handle, width, height, scaleMode).LogIfInvalid(), true);
        }

        private BlendMode GetBlendMode() {
            SDL.GetSurfaceBlendMode(Handle, out BlendMode blendMode).LogIfFalse();
            return blendMode;
        }

        private void SetBlendMode(BlendMode blendMode) {
            SDL.SetSurfaceBlendMode(Handle, blendMode).LogIfFalse();
        }

        private byte GetAlphaMod() {
            SDL.GetSurfaceAlphaMod(Handle, out byte alpha).LogIfFalse();
            return alpha;
        }

        private void SetAlphaMod(byte alpha) {
            SDL.SetSurfaceAlphaMod(Handle, alpha).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.SetSurfaceClipRect"/>
        public bool SetClipRect(Rect? rect) {
            Rect rectVal = rect.GetValueOrDefault();
            ref readonly Rect rectRef = ref rect.HasValue ? ref rectVal : ref Unsafe.NullRef<Rect>();
            return SDL.SetSurfaceClipRect(Handle, in rectRef).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.GetSurfaceClipRect"/>
        public Rect GetClipRect() {
            SDL.GetSurfaceClipRect(Handle, out Rect rect).LogIfFalse();
            return rect;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.FlipSurface"/>
        public bool Flip(FlipMode flip) {
            return SDL.FlipSurface(Handle, flip).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.ClearSurface"/>
        public bool Clear(float r, float g, float b, float a) {
            return SDL.ClearSurface(Handle, r, g, b, a).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.FillSurfaceRect"/>
        public bool FillRect(Rect? rect, uint color) {
            Rect rectVal = rect.GetValueOrDefault();
            ref readonly Rect rectRef = ref rect.HasValue ? ref rectVal : ref Unsafe.NullRef<Rect>();
            return SDL.FillSurfaceRect(Handle, in rectRef, color).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.FillSurfaceRects"/>
        public bool FillRects(Rect[] rects, uint color) {
            return SDL.FillSurfaceRects(Handle, rects, rects.Length, color).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Surface.DestroySurface"/>
        protected override void DisposeResource() {
            SDL.DestroySurface(Handle);
        }
    }
}
