// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL.Input {
    /// <summary>A native cursor owned by the caller.</summary>
    public sealed class Cursor : NativeHandle<Opaque.SdlCursor> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CreateSystemCursor"/>
        public Cursor(SystemCursor cursor) {
            Handle = SDL.CreateSystemCursor(cursor).ThrowIfInvalid();
        }

        internal Cursor(NativePtr<Opaque.SdlCursor> handle, bool ownsHandle)
            : base(handle, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CreateCursor"/>
        /// <remarks>
        /// <paramref name="data"/> and <paramref name="mask"/> are 1-bit-per-pixel bitmaps, so both
        /// must hold <c>(w + 7) / 8 * h</c> bytes.
        /// </remarks>
        public static Cursor FromBitmap(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mask, int w, int h, int hotX, int hotY) {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(w);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(h);

            int expected = checked((w + 7) / 8 * h);
            if (data.Length < expected || mask.Length < expected) {
                throw new ArgumentException($"A {w}x{h} cursor needs {expected} bytes of data and mask, got {data.Length} and {mask.Length}.");
            }
            unsafe {
                fixed (byte* dataPtr = data)
                fixed (byte* maskPtr = mask) {
                    return new Cursor(
                        SDL.CreateCursor((NativePtr<byte>)dataPtr, (NativePtr<byte>)maskPtr, w, h, hotX, hotY).ThrowIfInvalid(),
                        true);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CreateColorCursor"/>
        public static Cursor FromSurface(Surface surface, int hotX, int hotY) {
            ArgumentNullException.ThrowIfNull(surface);
            return new Cursor(SDL.CreateColorCursor(surface.Handle, hotX, hotY).ThrowIfInvalid(), true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CreateAnimatedCursor"/>
        /// <param name="frames">The frames of the animation, each with its duration in milliseconds. A duration of 0 stops the animation on that frame.</param>
        /// <param name="hotX">The x position of the cursor hot spot, shared by all frames.</param>
        /// <param name="hotY">The y position of the cursor hot spot, shared by all frames.</param>
        public static Cursor FromAnimation(ReadOnlySpan<(Surface Surface, uint DurationMs)> frames, int hotX, int hotY) {
            if (frames.IsEmpty) {
                throw new ArgumentException("An animated cursor needs at least one frame.", nameof(frames));
            }

            CursorFrameInfo[] infos = new CursorFrameInfo[frames.Length];
            for (int i = 0; i < frames.Length; i++) {
                Surface surface = frames[i].Surface;
                ArgumentNullException.ThrowIfNull(surface, nameof(frames));
                infos[i].Surface = surface.NativePointer;
                infos[i].Duration = frames[i].DurationMs;
            }

            return FromAnimation(infos, hotX, hotY);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.CreateAnimatedCursor"/>
        public static Cursor FromAnimation(Span<CursorFrameInfo> frames, int hotX, int hotY) {
            if (frames.IsEmpty) {
                throw new ArgumentException("An animated cursor needs at least one frame.", nameof(frames));
            }

            return new Cursor(
                SDL.CreateAnimatedCursor(frames, frames.Length, hotX, hotY).ThrowIfInvalid(),
                true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetCursor"/>
        public static Cursor? Current {
            get {
                NativePtr<Opaque.SdlCursor> cursor = SDL.GetCursor();
                return cursor.IsNull ? null : new Cursor(cursor, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.GetDefaultCursor"/>
        public static Cursor? Default {
            get {
                NativePtr<Opaque.SdlCursor> cursor = SDL.GetDefaultCursor();
                return cursor.IsNull ? null : new Cursor(cursor, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mouse.DestroyCursor"/>
        protected override void DisposeResource() {
            SDL.DestroyCursor(Handle);
        }
    }
}
