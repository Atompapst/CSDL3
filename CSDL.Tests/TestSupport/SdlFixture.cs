// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL;

namespace CSDL3.Tests.TestSupport {
    /// <summary>
    /// Owns the process-wide SDL lifetime for the whole test run: SDL is initialized once
    /// before the first test in <see cref="SdlCollection"/> and torn down after the last one.
    /// <para>
    /// SDL init/quit is global mutable state, and <c>Init.Quit</c> also fires the
    /// <c>Init.OnQuit</c> handlers that shut SDL_ttf and SDL_mixer down, so no individual test
    /// may quit on its own. Every test class that touches a native library therefore joins
    /// <see cref="SdlCollection"/>, which both shares this fixture and serializes those classes.
    /// </para>
    /// </summary>
    public sealed class SdlFixture : IDisposable {
        /// <summary>
        /// Subsystems the suite needs. Deliberately excludes <c>Video</c>: none of the native
        /// smoke tests need a window or a display, and SDL wants video initialized on the
        /// process main thread - which an xunit test thread is not.
        /// </summary>
        public const InitFlags RequiredSubsystems = InitFlags.Events;

        public SdlFixture() {
            Init.SetAppMetadata("CSDL3.Tests", "1.0.0", "de.index.csdl3.tests");
            Init.Initialize(RequiredSubsystems);
        }

        /// <summary>A scratch directory that is deleted when the whole run finishes.</summary>
        public string ScratchDirectory { get; } = CreateScratchDirectory();

        /// <summary>Builds a path inside <see cref="ScratchDirectory"/>; the file need not exist.</summary>
        public string ScratchPath(string fileName) {
            return System.IO.Path.Combine(ScratchDirectory, fileName);
        }

        public void Dispose() {
            try {
                Init.Quit();
            }
            finally {
                try {
                    System.IO.Directory.Delete(ScratchDirectory, recursive: true);
                } catch (System.IO.IOException) {
                    // A leftover scratch file is not worth failing the run over.
                } catch (UnauthorizedAccessException) {
                }
            }
        }

        private static string CreateScratchDirectory() {
            string path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "CSDL3.Tests-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(path);
            return path;
        }
    }

    /// <summary>
    /// The single collection every native-library test class belongs to. xunit runs collections
    /// in parallel but the classes inside one collection sequentially, so this keeps all SDL
    /// init/quit and error-state manipulation on one thread at a time.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class SdlCollection : ICollectionFixture<SdlFixture> {
        public const string Name = "SDL native";
    }
}
