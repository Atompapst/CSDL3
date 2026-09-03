# Contributing to CSDL3

Thanks for considering a contribution! A few guidelines to keep things consistent.

## General

- `_Generated/*.g.cs` files are auto-generated from the [SDL wiki](https://github.com/libsdl-org/sdlwiki) by a private
  code generator. Don't modify them by hand - changes belong in the hand-written wrapper classes.
- Target the high-level API in the main source directories for new features or fixes.
- Match the existing style of the wrapper classes you're touching (naming, `using`/dispose patterns, docs).
- Keep PRs focused on one topic; unrelated cleanups make review harder.

## Managed implementations (`CSDL_IMPL`)

CSDL3 calls into native SDL through P/Invoke almost everywhere. The bar for a managed C# reimplementation is not the
size of the function, it's proof: benchmark it against the native call (e.g. with GenericBenchmark) and show the
managed port is actually faster. A `CSDL_IMPL` region is tracked against the SDL source it mirrors, so a port that
falls behind gets flagged for review automatically so that tracking is what makes it safe to take a managed port
even when it isn't a one-line helper.

This does **not** mean managed code is generally preferable to native SDL. If you're unsure whether a managed port is
worth it, benchmark it and include the numbers in your PR either way, since "I checked and it wasn't faster" is useful
information too.

### Marking a managed port

Wrap the managed implementation in a `CSDL_IMPL` region so it stays traceable against the SDL source it mirrors:

```
#region CSDL_IMPL <SDL_SYMBOL> : <SDL_FILE_ALIAS>#<SDL_ANCHOR>[, <DEPENDENCY>...]
```

- `<SDL_SYMBOL>` - the SDL function or macro this port stands in for.
- `<SDL_FILE_ALIAS>#<SDL_ANCHOR>` - a reference into the SDL source tree (file name without extension, plus the
  symbol/anchor to resolve inside it). A region can list more than one such reference.
- `<DEPENDENCY>` - the symbol of another `CSDL_IMPL` region this one relies on (no `#`). If that region needs review,
  this one is flagged too.
- Prefix any reference with `~` to exclude it from tracking (e.g. a loosely related mention that isn't a real
  dependency).

Example with a dependency:

```csharp
#region CSDL_IMPL SDL_RECT_CAN_OVERFLOW : SDL_rect_impl#SDL_RECT_CAN_OVERFLOW

private static bool RectCanOverflow(in Rect r) {
    const int halfMax = int.MaxValue / 2;
    const int halfMin = int.MinValue / 2;

    return r.X <= halfMin || r.X >= halfMax ||
           r.Y <= halfMin || r.Y >= halfMax ||
           r.W >= halfMax || r.H >= halfMax;
}

#endregion

#region CSDL_IMPL SDL_HasRectIntersection : SDL_rect_impl#SDL_HASINTERSECTION, SDL_RECT_CAN_OVERFLOW

public static bool HasIntersection(in Rect a, in Rect b) {
    if (RectCanOverflow(in a) || RectCanOverflow(in b)) return false;
    return HasRectIntersection(in a, in b);
}

private static bool HasRectIntersection(in Rect a, in Rect b) {
    // ...
}

#endregion
```

### How tracking works

A tool parses `CSDL_IMPL` regions in this repo and cross-references them against the SDL source tree. For each
tracked port it resolves the referenced SDL range and compares commit history to flag one of the states below. Live
results: [csdl.spieleins.de](https://csdl.spieleins.de/?lang=en).

- **Current** - the referenced SDL source isn't newer than the managed port.
- **Maintenance needed** - the referenced SDL source changed after the managed port was last touched.
- **Check required** - a declared dependency changed and needs review, even though the direct SDL reference didn't.
- **Unresolved** - the referenced SDL file/symbol couldn't be identified reliably (typo in the alias/anchor, moved
  file, renamed symbol, ...). Treat this the same as a build warning - fix the reference.

If your PR touches a `CSDL_IMPL` region (directly or through one of its dependencies), expect a reviewer to ask you to
double-check the managed port still matches upstream SDL behavior.

## Pull requests

- Make sure `dotnet build` and `dotnet test` pass locally before opening a PR.
- Keep the diff focused on the change described in the PR - avoid drive-by refactors.
