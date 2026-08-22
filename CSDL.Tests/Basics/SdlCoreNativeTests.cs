// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

// CSDL is aliased rather than imported: the namespace also holds an Assert, a Version
// and a Timer that would collide with Xunit.Assert, System.Version and System.Threading.Timer.
using Sdl = CSDL;
using CSDL3.Tests.TestSupport;

namespace CSDL3.Tests.Basics {
    /// <summary>
    /// Proves the configured native SDL3 runtime is present,
    /// loadable and callable: entry points resolve, arguments and strings marshal in both
    /// directions, and real subsystems come up.
    /// </summary>
    [Collection(SdlCollection.Name)]
    public class SdlCoreNativeTests {
        private readonly SdlFixture _sdl;

        public SdlCoreNativeTests(SdlFixture sdl) {
            _sdl = sdl;
        }

        [Fact]
        public void GetVersion_FromNativeLibrary_ReportsAnSdl3Runtime() {
            SdlVersionNumber version = new SdlVersionNumber(Sdl.Version.SdlVersion);

            Assert.Equal(3, version.Major);
            Assert.True(version.Minor >= 2, $"expected at least SDL 3.2.0, got {version}");
        }

        [Fact]
        public void GetVersion_FromNativeLibrary_SharesTheMajorVersionOfTheHeadersTheBindingsUse() {
            // The generated P/Invoke surface targets Macros.Version. A different major version
            // would mean ABI breakage; within a major version SDL only adds entry points.
            //
            // The two are deliberately NOT asserted equal: the bindings can be generated from
            // newer headers than the configured native runtime, in which case the entry
            // points added by those newer headers will not resolve at runtime - see the note in
            // README.md. Tighten this to runtime >= compiled once the natives have caught up.
            SdlVersionNumber runtime = new SdlVersionNumber(Sdl.Version.SdlVersion);
            SdlVersionNumber compiled = new SdlVersionNumber((int)Sdl.Version.VersionCompiled);

            Assert.Equal(compiled.Major, runtime.Major);
            Assert.True(
                runtime.Packed >= 3004014,
                $"native SDL3 is {runtime}, below the validated 3.4.14 runtime floor");
        }

        [Fact]
        public void GetRevision_FromNativeLibrary_MarshalsANonEmptyUtf8String() {
            // Exercises the NativePtr<byte> -> string path against a real native buffer.
            string revision = Sdl.Version.Revision;

            Assert.False(string.IsNullOrWhiteSpace(revision));
        }

        [Fact]
        public void GetPlatform_FromNativeLibrary_MatchesTheHostOperatingSystem() {
            // The native binary that actually loaded must be the one built for this OS, so SDL's
            // own idea of the platform has to agree with the runtime's.
            if (System.OperatingSystem.IsWindows()) {
                Assert.True(Sdl.Platform.IsWindows, "SDL_GetPlatform did not report Windows");
            } else if (System.OperatingSystem.IsLinux()) {
                Assert.True(Sdl.Platform.IsLinux, "SDL_GetPlatform did not report Linux");
                Assert.True(Sdl.Platform.IsUnix, "SDL platform macros report Linux as Unix-like");
            } else if (System.OperatingSystem.IsMacOS()) {
                Assert.True(Sdl.Platform.IsMacOS, "SDL_GetPlatform did not report macOS");
            } else {
                // Nothing to cross-check against, but the call still has to survive the boundary.
                Assert.False(string.IsNullOrWhiteSpace(Sdl.Version.Revision));
            }
        }

        [Fact]
        public void DeviceFormFactorQueries_WorkWithTheConfiguredNativeRuntime() {
            Assert.False(string.IsNullOrWhiteSpace(Sdl.Platform.FormFactorName));
            _ = Sdl.Platform.IsPhone;
            _ = Sdl.Platform.IsUbuntuTouch;
        }

        [Fact]
        public void SetError_ThenGetError_RoundTripsThroughNativeErrorState() {
            // Managed string in, native storage, native string back out.
            const string message = "csdl3-native-smoke-test";
            try {
                Sdl.Error.SetError(message);

                Assert.Equal(message, Sdl.Error.GetError());
            }
            finally {
                Sdl.Error.ClearError();
            }
        }

        [Fact]
        public void ClearError_AfterSetError_LeavesNativeErrorStateEmpty() {
            Sdl.Error.SetError("something went wrong");

            Sdl.Error.ClearError();

            Assert.Equal(string.Empty, Sdl.Error.GetError());
        }

        [Fact]
        public void Initialize_EventsSubsystem_IsReportedAsInitializedByTheNativeLibrary() {
            // The fixture already brought this subsystem up; WasInit reads it back out of SDL.
            Assert.True(Sdl.Init.AreAllInitialized(SdlFixture.RequiredSubsystems));
            Assert.Equal(SdlFixture.RequiredSubsystems, Sdl.Init.WasInit(SdlFixture.RequiredSubsystems));
        }

        [Fact]
        public void InitSubSystem_ThenQuitSubSystem_TogglesTheSubsystemWithoutTearingDownSdl() {
            Assert.False(Sdl.Init.IsAnyInitialized(Sdl.InitFlags.Sensor));

            Sdl.Init.InitSubSystem(Sdl.InitFlags.Sensor);
            try {
                Assert.True(Sdl.Init.AreAllInitialized(Sdl.InitFlags.Sensor));
            }
            finally {
                Sdl.Init.QuitSubSystem(Sdl.InitFlags.Sensor);
            }

            Assert.False(Sdl.Init.IsAnyInitialized(Sdl.InitFlags.Sensor));
            // Quitting one subsystem must leave the rest of the run's SDL state intact.
            Assert.True(Sdl.Init.AreAllInitialized(SdlFixture.RequiredSubsystems));
        }

        [Fact]
        public void GetTicks_CalledTwiceAcrossASleep_AdvancesMonotonically() {
            // Returns a real 64-bit value from the native timer rather than a marshalling artefact.
            ulong first = Sdl.Timer.GetTicksNs();
            System.Threading.Thread.Sleep(20);
            ulong second = Sdl.Timer.GetTicksNs();

            Assert.True(second > first, $"SDL_GetTicksNS did not advance: {first} -> {second}");
        }

        [Fact]
        public void GetPerformanceFrequency_FromNativeLibrary_IsPositive() {
            Assert.True(Sdl.Timer.GetPerformanceFrequency() > 0);
        }

        [Fact]
        public void GetNumLogicalCpuCores_FromNativeLibrary_MatchesTheRuntimesView() {
            // Two independent probes of the same hardware fact - a decoding or calling-convention
            // mistake would show up as a nonsense core count.
            Assert.Equal(System.Environment.ProcessorCount, Sdl.CPUInfo.NumLogicalCores);
        }

        [Fact]
        public void ScratchDirectory_FromFixture_IsUsableForTheFileBasedTests() {
            // Guards the shared fixture the Image tests write their round-trip files into.
            string path = _sdl.ScratchPath("core-probe.bin");
            System.IO.File.WriteAllBytes(path, new byte[] { 1, 2, 3 });

            Assert.True(System.IO.File.Exists(path));
        }
    }
}
