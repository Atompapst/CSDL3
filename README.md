# CSDL3

**CSDL3 is an idiomatic, object-oriented C# wrapper for working with [SDL3](https://github.com/libsdl-org/SDL), making
it easier to build cross-platform games and multimedia applications in .NET.**

[![NuGet](https://img.shields.io/nuget/v/CSDL3)](https://www.nuget.org/packages/CSDL3)
[![License: zlib](https://img.shields.io/badge/license-zlib-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-purple)](https://dotnet.microsoft.com)

> **Status**: SDL3 is essentially fully implemented across the high-level wrappers.
> It hasn't been exhaustively tested yet though, so occasional bugs can still show
> up here and there. Issues and PRs are appreciated. Going forward, the primary focus is adding new convenience
> methods to the high-level wrappers.

## What's covered

CSDL wraps [SDL3](https://wiki.libsdl.org/SDL3/FrontPage) itself (video, audio, input, GPU, threads, file I/O, and
more), plus the official satellite libraries: [SDL_image](https://wiki.libsdl.org/SDL3_image/FrontPage),
[SDL_mixer](https://wiki.libsdl.org/SDL3_mixer/FrontPage), [SDL_ttf](https://wiki.libsdl.org/SDL3_ttf/FrontPage), and
[SDL_net](https://wiki.libsdl.org/SDL3_net/FrontPage).

[SDL_shadercross](https://github.com/libsdl-org/SDL_shadercross) is not covered yet - because there's no SDL wiki
documentation for it for now.

## Requirements

- **.NET 10.0**
- **SDL3 runtime** available on your system

## Compatibility

CI builds and tests on a self-hosted Linux runner. Before testing, it fast-forwards the local source checkouts for SDL,
SDL_image, SDL_mixer, SDL_net, and SDL_ttf, then builds and installs them into a job-local prefix. The workflows are the
source of truth for the validated native runtime setup.

## Getting Started

### 1. Installation

```bash
dotnet add package CSDL3
```

### 2. Get the SDL3 Native Libraries

**⚠️ Important**: CSDL3 is bindings only - it does not ship with native SDL3 binaries. You need to obtain them
separately and place them where your app can find them.

#### Recommended approach:

Use prebuilt native packages from [edwardgushchin/SDL3-CS](https://github.com/edwardgushchin/SDL3-CS) for SDL3,
SDL_image, SDL_mixer, and SDL_ttf:

```bash
dotnet add package SDL3-CS.Linux
dotnet add package SDL3-CS.Linux.Image
dotnet add package SDL3-CS.Linux.Mixer
dotnet add package SDL3-CS.Linux.TTF
```

Replace `Linux` with `Windows` or `macOS` for those platforms. Install SDL_net separately from
its [official releases](https://github.com/libsdl-org/SDL_net/releases).

#### Manual approach:

Download binaries directly from:

- [SDL3 releases](https://github.com/libsdl-org/SDL/releases/latest) - grab `SDL3.dll`/`.so`/`.dylib`
- [SDL_image releases](https://github.com/libsdl-org/SDL_image/releases/latest)
- [SDL_mixer releases](https://github.com/libsdl-org/SDL_mixer/releases/latest)
- [SDL_ttf releases](https://github.com/libsdl-org/SDL_ttf/releases/latest)
- [SDL_net releases](https://github.com/libsdl-org/SDL_net/releases/latest) (if needed)

Place the `.dll`/`.so`/`.dylib` files next to your executable.

### 3. Quick Start

Choose your preferred approach:

#### Option A: Manual Event Loop

```csharp
using CSDL;
using CSDL.Input;
using CSDL.Video;

namespace CSDL3.Example {
    public static class Program {
        public static void Main(string[] args) {
            Init.RunOnMainThread(Run, $"Everything works on {Platform.Name}!", true);
            Init.Quit(); // important!!!
        }

        private static void Run(object userdata) {
            // Performs automatically when specific resources accessed
            //if (!Init.Initialize(InitFlags.Video | InitFlags.Events)) throw; 
            
            //Log.UseCustomOutput();
            //Log.CategoryPriority.SetForAll(LogPriority.Debug);
            
            using (var window = new Window("My Application", 800, 600)) {
                using (var renderer = new Renderer(window)) {
                    string text = (string)userdata;
                    // DefaultClearColor Applied when Clear() is called across all Renderers; adjust to your liking
                    Renderer.ClearColor = Color.Random();

                    // Window queries must stay on the main thread, so cache the size once and
                    // hand it to the timer callback instead of touching `window` from its thread.
                    Point windowSize = window.Size;
                    int textX = CSDL.Rand.Global.Next(50, windowSize.X / 2);
                    int textY = CSDL.Rand.Global.Next(10, windowSize.Y / 2);

                    // CSDL.Timer + CSDL.Rand together: jump the text to a new random spot once a second
                    using (var repositionTimer = new CSDL.Timer(CSDL.Timer.MsPerSecond, (object ud, TimerID id, uint interval) => {
                        textX = CSDL.Rand.Global.Next(50, windowSize.X / 2);
                        textY = CSDL.Rand.Global.Next(10, windowSize.Y / 2);
                        return interval; // returning the interval keeps the timer repeating
                    })) {
                        bool running = true;
                        while (running) {
                            Events.PollAll();
                            if (Events.Common.QuitRequested()) running = false;
                            if (Events.Keyboard.IsDown(Scancode.Escape)) running = false;

                            renderer.Clear();
                            renderer.DrawColor = Color.Red;
                            renderer.RenderDebugText(textX, textY, text);
                            renderer.Present();
                            CSDL.Timer.Delay(10);
                        }
                    }
                }
            }
        }
    }
}
```

#### Option B: Game Lifecycle

Derive from `Game` and let SDL's native main-callback mechanism (`SDL_MAIN_USE_CALLBACKS`) handle your event loop:

```csharp
using CSDL;

public sealed class MyGame : CSDL.Game {
    protected override AppResult OnInit(string[] args) {
        // create your window/renderer here
        return AppResult.Continue;
    }

    protected override AppResult OnIterate() {
        // one frame of your game loop
        return AppResult.Continue;
    }

    protected override AppResult OnEvent(ref Event @event) {
        return AppResult.Continue;
    }

    protected override void OnQuit(AppResult result) {
        // dispose your resources here
    }
}

public static class Program {
    public static int Main() => new MyGame().Run();
}
```

Use `Game<TState>` if you'd prefer to keep your app's state in an explicit object rather than `this`.

## How the Bindings Are Generated

The low-level `_Generated` layer underneath the hand-written wrapper classes is produced by a private code generator
(not part of this repo) that reads the [SDL wiki](https://github.com/libsdl-org/sdlwiki) directly. Function signatures,
structs, enums, and documentation all come from there automatically.

### Properties

Properties are handled specially since the SDL wiki doesn't document them well. They're derived from reading the actual
SDL source code, parsing the surrounding syntax, and inferring return values from context.

## Contributing

Issues and PRs are appreciated! Please note:

- `_Generated/*.g.cs` files are auto-generated. Don't modify them
- For new features or fixes, target the high-level API in the main source directories

## License

Licensed under the [zlib License](LICENSE).
