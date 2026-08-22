# CSDL3 Tests

Unit and integration tests for CSDL3, written with xunit.

## Native Libraries

CSDL3 itself ships no native binaries, and neither does the test project. The tests require SDL, SDL_image, SDL_mixer,
SDL_net, and SDL_ttf to be installed on the machine.

GitHub Actions runs the test suite on a self-hosted Linux runner that provides all required native SDL binaries.
Desktop integration tests also require Xvfb and use its isolated X11 display in CI.

## Running Tests

```bash
dotnet test                            # all tests
dotnet test --filter "ClassName"       # one test class
dotnet test --verbosity detailed       # verbose output
```

## Test Structure

Tests are organized by subsystem:

- `Basics/` — Init, version, error, timer, platform
- `Video/` — Window, Renderer, GPU
- `Input/` — Keyboard, Mouse, Gamepad, Touch
- `Audio/` — Audio device and mixer
- `Image/` — SDL_image loading, saving, codecs
- `TTF/` — Font loading and text rendering
- `Events/` — Event system
- `Utilities/` — Timer, Random, File I/O
- `TestSupport/` — Shared fixtures and helpers, not tests

## Writing Tests

1. Put the file in the matching subsystem folder.
2. Name tests `Method_Scenario_ExpectedOutcome`.
3. One behaviour per test.
4. If the test touches SDL, join `SdlCollection` and take `SdlFixture` through the constructor — never call
   `Init.Quit()` yourself. SDL init/quit is process-global, and quitting also tears down SDL_ttf and SDL_mixer for every
   later test.

```csharp
[Collection(SdlCollection.Name)]
public class WindowTests {
    [Fact]
    public void CreateWindow_WithValidParameters_CreatesWindowSuccessfully() {
        Point size = new Point(800, 600);

        using (Window window = new Window("Test", size.X, size.Y)) {
            Assert.Equal(size, window.Size);
        }
    }
}
```

## Known Limitations

- The SDL_ttf tests borrow a TrueType font from the host — no font ships with the repo. On a bare Linux image, install
  one first: `apt-get install fonts-dejavu-core`.
- A native runtime must provide every SDL entry point your application calls. The self-hosted CI source checkouts define
  the revisions validated by this repository.

## Contributing Tests

Tests are a great way to contribute! They help catch regressions and document expected behavior. If you're adding a new
feature or fixing a bug, please include tests.

AI-generated tests are ok, but do not submit them blindly—ensure they are relevant 
to the feature or bug and that you can justify their behavior.
