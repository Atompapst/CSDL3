// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.Video {
    public partial class Renderer {
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTexture"/>
        public bool RenderTexture(Texture gpuTexture, FRect? srcRect, FRect? dstRect) {
            ref readonly FRect srcRef = ref srcRect.AsRef(out FRect srcVal);
            ref readonly FRect dstRef = ref dstRect.AsRef(out FRect dstVal);
            return SDL.RenderTexture(Handle, gpuTexture.Handle, in srcRef, in dstRef).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTexture9Grid"/>
        public bool Render9Grid(Texture gpuTexture, FRect? srcRect, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale, FRect? dstRect) {
            ref readonly FRect srcRef = ref srcRect.AsRef(out FRect srcVal);
            ref readonly FRect dstRef = ref dstRect.AsRef(out FRect dstVal);
            return SDL.RenderTexture9Grid(Handle, gpuTexture.Handle, in srcRef, leftWidth, rightWidth, topHeight, bottomHeight, scale, in dstRef).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTexture9GridTiled"/>
        public bool Render9GridTiled(Texture gpuTexture, FRect? srcRect, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale, FRect? dstRect, float tileScale) {
            ref readonly FRect srcRef = ref srcRect.AsRef(out FRect srcVal);
            ref readonly FRect dstRef = ref dstRect.AsRef(out FRect dstVal);
            return SDL.RenderTexture9GridTiled(Handle, gpuTexture.Handle, in srcRef, leftWidth, rightWidth, topHeight, bottomHeight, scale, in dstRef, tileScale).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTextureAffine"/>
        public bool RenderAffine(Texture texture, FRect? srcRect, FPoint origin, FPoint right, FPoint down) {
            ref readonly FRect srcRef = ref srcRect.AsRef(out FRect srcVal);
            return SDL.RenderTextureAffine(Handle, texture.Handle, in srcRef, origin, right, down).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTextureRotated"/>
        public bool RenderRotated(Texture gpuTexture, FRect srcRect, FRect dstRect, double angle, FPoint center, FlipMode flip) {
            return SDL.RenderTextureRotated(Handle, gpuTexture.Handle, srcRect, dstRect, angle, center, flip).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTextureRotated"/>
        public bool RenderRotated(Texture gpuTexture, FRect? srcRect, FRect? dstRect, double angle, FPoint center, FlipMode flip) {
            ref readonly FRect srcRef = ref srcRect.AsRef(out FRect srcVal);
            ref readonly FRect dstRef = ref dstRect.AsRef(out FRect dstVal);
            return SDL.RenderTextureRotated(Handle, gpuTexture.Handle, in srcRef, in dstRef, angle, center, flip).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTextureTiled"/>
        public bool RenderTiled(Texture gpuTexture, FRect srcRect, float scale, FRect dstRect) {
            return SDL.RenderTextureTiled(Handle, gpuTexture.Handle, srcRect, scale, dstRect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.RenderTextureTiled"/>
        public bool RenderTiled(Texture gpuTexture, FRect? srcRect, float scale, FRect? dstRect) {
            ref readonly FRect srcRef = ref srcRect.AsRef(out FRect srcVal);
            ref readonly FRect dstRef = ref dstRect.AsRef(out FRect dstVal);
            return SDL.RenderTextureTiled(Handle, gpuTexture.Handle, in srcRef, scale, in dstRef).LogIfFalse();
        }
    }
}
