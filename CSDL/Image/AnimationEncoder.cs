// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.File;
using CSDL.Video;
namespace CSDL.Image {
    /// <summary>
    /// Create an encoder to save a series of images to a file.
    /// </summary>
    public class AnimationEncoder : NativeHandle<Opaque.SdlAnimationEncoder> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationEncoder"/>
        public AnimationEncoder(string path) {
            CreateAnimationEncoder(path);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationEncoder_IO"/>
        public AnimationEncoder(IOStream stream, ImageType type, bool closeio = false) {
            CreateAnimationEncoderIO(stream, type, closeio);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationEncoderWithProperties"/>
        public AnimationEncoder(AnimationEncoderProperties props) {
            CreateAnimationEncoderWithProperties(props);
        }

        internal AnimationEncoder(NativePtr<Opaque.SdlAnimationEncoder> handle) {
            Handle = handle;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationEncoder"/>
        private void CreateAnimationEncoder(string path) {
            Handle = SDL.CreateAnimationEncoder(path).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationEncoder_IO"/>
        private void CreateAnimationEncoderIO(IOStream stream, ImageType type, bool closeio) {
            NativePtr<Opaque.SdlAnimationEncoder> encoder = SDL.CreateAnimationEncoder_IO(stream.Handle, closeio, type.ToString());
            if (closeio) {
                stream.Invalidate();
            }
            Handle = encoder.ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationEncoderWithProperties"/>
        private void CreateAnimationEncoderWithProperties(AnimationEncoderProperties props) {
            Handle = SDL.CreateAnimationEncoderWithProperties(props.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.AddAnimationEncoderFrame"/>
        public bool AddFrame(Surface surface, ulong durationMs) {
            return SDL.AddAnimationEncoderFrame(Handle, surface.Handle, durationMs).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CloseAnimationEncoder"/>
        protected override void DisposeResource() {
            SDL.CloseAnimationEncoder(Handle).LogIfFalse();
        }
    }
}
