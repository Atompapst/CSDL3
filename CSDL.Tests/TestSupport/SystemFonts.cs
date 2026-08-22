// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSDL3.Tests.TestSupport {
    /// <summary>
    /// Finds a TrueType font on the host so the SDL_ttf tests have something real to shape.
    /// The repository intentionally ships no font binary, so the suite borrows one from the
    /// operating system.
    /// </summary>
    public static class SystemFonts {
        /// <summary>Directories that hold fonts on the platforms this suite runs on.</summary>
        private static readonly string[] SearchRoots = {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"),
            "/usr/share/fonts",
            "/usr/local/share/fonts",
            "/Library/Fonts",
            "/System/Library/Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
        };

        /// <summary>
        /// Fonts tried first, in order. These are plain, well-behaved Latin faces - starting from
        /// a variable or emoji face would make the glyph-metric assertions needlessly fragile.
        /// </summary>
        private static readonly string[] PreferredFiles = {
            "DejaVuSans.ttf",
            "truetype/dejavu/DejaVuSans.ttf",
            "truetype/liberation/LiberationSans-Regular.ttf",
            "truetype/freefont/FreeSans.ttf",
            "arial.ttf",
            "segoeui.ttf",
            "verdana.ttf",
            "tahoma.ttf",
            "Supplemental/Arial.ttf",
        };

        private static readonly Lazy<string> Resolved = new Lazy<string>(Locate);

        /// <summary>
        /// The absolute path of a usable font file.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// No font was found under any of <see cref="SearchRoots"/>. Install any TrueType font
        /// (on Debian/Ubuntu: <c>apt-get install fonts-dejavu-core</c>) to run the SDL_ttf
        /// rendering tests.
        /// </exception>
        public static string FirstAvailable => Resolved.Value;

        private static string Locate() {
            List<string> roots = SearchRoots
                .Where(root => !string.IsNullOrEmpty(root) && Directory.Exists(root))
                .ToList();

            foreach (string root in roots) {
                foreach (string candidate in PreferredFiles) {
                    string exact = Path.Combine(root, candidate);
                    if (File.Exists(exact)) {
                        return exact;
                    }
                }
            }

            // Nothing preferred is installed - take the first ordinary .ttf we can find so a bare
            // host still exercises the native library instead of reporting a false negative.
            foreach (string root in roots) {
                string found = FirstFontUnder(root);
                if (found != null) {
                    return found;
                }
            }

            throw new InvalidOperationException(
                "No TrueType font found under any of: " + string.Join(", ", SearchRoots) +
                ". Install a font package (e.g. fonts-dejavu-core) to run the SDL_ttf rendering tests.");
        }

        private static string FirstFontUnder(string root) {
            EnumerationOptions options = new EnumerationOptions {
                RecurseSubdirectories = true,
                // Unreadable font directories are common on locked-down hosts; walking past them
                // is far better than aborting the search.
                IgnoreInaccessible = true,
            };

            try {
                return Directory.EnumerateFiles(root, "*.ttf", options).FirstOrDefault();
            } catch (IOException) {
                return null;
            } catch (UnauthorizedAccessException) {
                return null;
            }
        }
    }
}
