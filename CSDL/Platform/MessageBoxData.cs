// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.InteropServices;

namespace CSDL {
    public partial struct MessageBoxData {
        /// <summary>
        ///     Fills in the native message box description.
        /// </summary>
        /// <remarks>
        ///     Internal because every pointer here has to stay valid for the duration of the
        ///     <c>SDL_ShowMessageBox</c> call - see <see cref="MessageBox.Show"/>, which owns those
        ///     allocations.
        /// </remarks>
        internal MessageBoxData(MessageBoxFlags flags, nint window, nint title, nint message, int numbuttons, nint buttons, nint colorScheme) {
            Flags = flags;
            Window = window;
            _title = title;
            _message = message;
            Numbuttons = numbuttons;
            Buttons = buttons;
            ColorScheme = colorScheme;
        }
    }

    public partial struct MessageBoxButtonData {
        /// <inheritdoc cref="MessageBoxData(MessageBoxFlags, nint, nint, nint, int, nint, nint)"/>
        internal MessageBoxButtonData(MessageBoxButtonFlags flags, int buttonID, nint text) {
            Flags = flags;
            ButtonID = buttonID;
            _text = text;
        }
    }

    /// <summary>
    ///     The five colors a message box can be tinted with, in the order
    ///     <see cref="MessageBoxColorType"/> lists them.
    /// </summary>
    /// <remarks>
    ///     This mirrors <c>SDL_MessageBoxColorScheme</c>. The generated <see cref="MessageBoxColorScheme"/>
    ///     lost the array in translation (it carries a single color), so this is what
    ///     <see cref="MessageBox.Show"/> hands to SDL.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageBoxColors {
        /// <inheritdoc cref="MessageBoxColorType.Background"/>
        public MessageBoxColor Background;

        /// <inheritdoc cref="MessageBoxColorType.Text"/>
        public MessageBoxColor Text;

        /// <inheritdoc cref="MessageBoxColorType.ButtonBorder"/>
        public MessageBoxColor ButtonBorder;

        /// <inheritdoc cref="MessageBoxColorType.ButtonBackground"/>
        public MessageBoxColor ButtonBackground;

        /// <inheritdoc cref="MessageBoxColorType.ButtonSelected"/>
        public MessageBoxColor ButtonSelected;
    }
}
