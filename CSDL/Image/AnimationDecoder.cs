// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.File;
using CSDL.Video;
namespace CSDL.Image {
    public class AnimationDecoder : NativeHandle<Opaque.SdlAnimationDecoder> {
        public Metadata Metadata => new Metadata(SDL.GetAnimationDecoderProperties(Handle));

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationDecoder"/>
        public AnimationDecoder(string path) {
            CreateAnimationDecoder(path);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationDecoder_IO"/>
        public AnimationDecoder(IOStream stream, ImageType type, bool closeio = false) {
            CreateAnimationDecoderIO(stream, type, closeio);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationDecoderWithProperties"/>
        public AnimationDecoder(AnimationDecoderProperties props) {
            CreateAnimationDecoderWithProperties(props);
        }

        internal AnimationDecoder(NativePtr<Opaque.SdlAnimationDecoder> handle) {
            Handle = handle;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationDecoder"/>
        private void CreateAnimationDecoder(string path) {
            Handle = SDL.CreateAnimationDecoder(path).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationDecoder_IO"/>
        private void CreateAnimationDecoderIO(IOStream stream, ImageType type, bool closeio) {
            NativePtr<Opaque.SdlAnimationDecoder> decoder = SDL.CreateAnimationDecoder_IO(stream.Handle, closeio, type.ToString());
            if (closeio) {
                stream.Invalidate();
            }
            Handle = decoder.ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CreateAnimationDecoderWithProperties"/>
        private void CreateAnimationDecoderWithProperties(AnimationDecoderProperties props) {
            Handle = SDL.CreateAnimationDecoderWithProperties(props.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.GetAnimationDecoderFrame"/>
        public bool GetFrame(out Surface? frame, out ulong durationMs) {
            ulong duration = 0;
            bool success = SDL.GetAnimationDecoderFrame(Handle, out NativePtr<SurfaceData> framePtr, NativePtr<ulong>.FromRef(ref duration)).LogIfFalse();
            durationMs = duration;
            if (success) {
                frame = framePtr.IsNull ? null : new Surface(framePtr, true);
                return true;
            }
            frame = null;
            return false;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.GetAnimationDecoderStatus"/>
        public AnimationDecoderStatus Status => SDL.GetAnimationDecoderStatus(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.ResetAnimationDecoder"/>
        public bool Reset() {
            return SDL.ResetAnimationDecoder(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Image.CloseAnimationDecoder"/>
        protected override void DisposeResource() {
            SDL.CloseAnimationDecoder(Handle).LogIfFalse();
        }
    }
}
