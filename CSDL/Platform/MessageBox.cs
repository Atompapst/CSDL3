// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL {
    /// <summary>
    ///     A button offered by <see cref="MessageBox.Show"/>.
    /// </summary>
    /// <param name="Id">the value returned when this button is pressed.</param>
    /// <param name="Text">the button's label.</param>
    /// <param name="Flags">whether this button is the default for Return and/or Escape.</param>
    public readonly record struct MessageBoxButton(int Id, string Text, MessageBoxButtonFlags Flags = 0);

    /// <summary>
    ///     SDL's own message boxes - usable before (and without) a window or renderer exists.
    /// </summary>
    /// <remarks>
    ///     These block the calling thread until the user answers, and should be shown from the thread
    ///     that set up video.
    /// </remarks>
    public static class MessageBox {

        /// <inheritdoc cref="CSDL.Internal.Docs.Messagebox.ShowSimpleMessageBox"/>
        /// <param name="flags">the kind of box to show (error, warning, information).</param>
        /// <param name="title">the window title.</param>
        /// <param name="message">the text to show.</param>
        /// <param name="window">the parent window, or <see langword="null"/> for none.</param>
        public static bool ShowSimple(MessageBoxFlags flags, string title, string message, Window? window = null) {
            return SDL.ShowSimpleMessageBox(flags, title, message, WindowHandle(window))
                .LogIfFalse(nameof(SDL.ShowSimpleMessageBox));
        }

        /// <inheritdoc cref="ShowSimple"/>
        public static bool ShowError(string title, string message, Window? window = null) {
            return ShowSimple(MessageBoxFlags.Error, title, message, window);
        }

        /// <inheritdoc cref="ShowSimple"/>
        public static bool ShowWarning(string title, string message, Window? window = null) {
            return ShowSimple(MessageBoxFlags.Warning, title, message, window);
        }

        /// <inheritdoc cref="ShowSimple"/>
        public static bool ShowInformation(string title, string message, Window? window = null) {
            return ShowSimple(MessageBoxFlags.Information, title, message, window);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Messagebox.ShowMessageBox"/>
        /// <param name="flags">the kind of box to show, plus the button ordering.</param>
        /// <param name="title">the window title.</param>
        /// <param name="message">the text to show.</param>
        /// <param name="buttons">the buttons to offer; at least one is required.</param>
        /// <param name="window">the parent window, or <see langword="null"/> for none.</param>
        /// <param name="colors">an optional color scheme; platforms are free to ignore it.</param>
        /// <returns>The <see cref="MessageBoxButton.Id"/> of the button the user pressed.</returns>
        /// <exception cref="SDLException">The box could not be shown, or the user closed it without choosing.</exception>
        public static int Show(MessageBoxFlags flags, string title, string message,
            IReadOnlyList<MessageBoxButton> buttons, Window? window = null, MessageBoxColors? colors = null) {
            ArgumentNullException.ThrowIfNull(buttons);
            if (buttons.Count == 0) {
                throw new ArgumentException("A message box needs at least one button.", nameof(buttons));
            }

            nint titlePtr = Marshal.StringToCoTaskMemUTF8(title ?? string.Empty);
            nint messagePtr = Marshal.StringToCoTaskMemUTF8(message ?? string.Empty);
            nint[] labels = new nint[buttons.Count];
            nint buttonBlock = Marshal.AllocCoTaskMem(buttons.Count * Marshal.SizeOf<MessageBoxButtonData>());
            nint colorBlock = nint.Zero;

            try {
                NativePtr<MessageBoxButtonData> buttonArray = new NativePtr<MessageBoxButtonData>(buttonBlock);
                for (int i = 0; i < buttons.Count; i++) {
                    labels[i] = Marshal.StringToCoTaskMemUTF8(buttons[i].Text ?? string.Empty);
                    buttonArray[i] = new MessageBoxButtonData(buttons[i].Flags, buttons[i].Id, labels[i]);
                }

                if (colors.HasValue) {
                    colorBlock = Marshal.AllocCoTaskMem(Marshal.SizeOf<MessageBoxColors>());
                    NativePtr<MessageBoxColors> scheme = new NativePtr<MessageBoxColors>(colorBlock);
                    scheme[0] = colors.Value;
                }

                MessageBoxData data = new MessageBoxData(flags, WindowHandle(window).Ptr, titlePtr, messagePtr,
                    buttons.Count, buttonBlock, colorBlock);

                int buttonId = -1;
                SDL.ShowMessageBox(NativePtr<MessageBoxData>.FromRef(ref data), NativePtr<int>.FromRef(ref buttonId))
                    .ThrowIfFalse(nameof(SDL.ShowMessageBox));

                return buttonId;
            }
            finally {
                foreach (nint label in labels) {
                    if (label != nint.Zero) Marshal.FreeCoTaskMem(label);
                }
                if (buttonBlock != nint.Zero) Marshal.FreeCoTaskMem(buttonBlock);
                if (colorBlock != nint.Zero) Marshal.FreeCoTaskMem(colorBlock);
                if (titlePtr != nint.Zero) Marshal.FreeCoTaskMem(titlePtr);
                if (messagePtr != nint.Zero) Marshal.FreeCoTaskMem(messagePtr);
            }
        }

        private static NativePtr<Opaque.SdlWindow> WindowHandle(Window? window) {
            return window?.Handle ?? NativePtr<Opaque.SdlWindow>.Zero;
        }
    }
}
