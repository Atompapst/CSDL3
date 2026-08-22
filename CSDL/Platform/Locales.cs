// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    /// <summary>
    ///     One entry of the user's preferred locale list.
    /// </summary>
    /// <param name="Language">the ISO-639 language code, e.g. <c>en</c>.</param>
    /// <param name="Country">the ISO-3166 country code, e.g. <c>US</c>, or <see langword="null"/> if the locale does not name one.</param>
    public readonly record struct LocaleInfo(string Language, string? Country) {
        public override string ToString() {
            return string.IsNullOrEmpty(Country) ? Language : $"{Language}_{Country}";
        }
    }

    /// <summary>
    ///     The locales the user prefers, most-preferred first.
    /// </summary>
    public static class Locales {
        /// <inheritdoc cref="CSDL.Internal.Docs.Locale.GetPreferredLocales"/>
        /// <remarks>
        ///     Queried from the OS on every access - SDL also pushes an
        ///     <see cref="EventType.LocaleChanged"/> event when this list changes, so there is no need to
        ///     poll it.
        /// </remarks>
        public static LocaleInfo[] Preferred {
            get {
                nint locales = SDL.GetPreferredLocales(out int count);
                if (locales == nint.Zero) {
                    Error.LogError(nameof(SDL.GetPreferredLocales));
                    return Array.Empty<LocaleInfo>();
                }

                try {
                    if (count <= 0) {
                        return Array.Empty<LocaleInfo>();
                    }

                    NativePtr<nint> list = new NativePtr<nint>(locales);
                    LocaleInfo[] result = new LocaleInfo[count];
                    for (int i = 0; i < count; i++) {
                        NativePtr<Locale> entry = new NativePtr<Locale>(list[i]);
                        if (entry.IsNull) {
                            result[i] = new LocaleInfo(string.Empty, null);
                            continue;
                        }

                        Locale locale = entry.AsRef();
                        string? country = locale.Country;
                        result[i] = new LocaleInfo(locale.Language ?? string.Empty, string.IsNullOrEmpty(country) ? null : country);
                    }

                    return result;
                }
                finally {
                    Memory.Free(locales);
                }
            }
        }
    }
}
