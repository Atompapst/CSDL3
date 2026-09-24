# HandleTable

Wrappers like `Texture`, `Renderer` or `Surface` are `readonly struct`s. They do **not** hold the SDL pointer —
they hold a `long` id into `HandleTable`. That buys three things a raw pointer cannot:

- a copy of a disposed wrapper throws instead of using freed memory,
- a child dies with its parent (`Texture` after `Renderer.Dispose()`), without the child knowing,
- the same SDL pointer handed back twice is the same identity (`==` works).

Files: `HandleId.cs` · `OwnerId.cs` · `HandleKind.cs` · `HandleTable.cs` (API) ·
`HandleTable.Storage.cs` (slots) · `HandleTable.Resolution.cs` (lookup) · `HandleTable.Diagnostics.cs` (leaks).

## The id, bit by bit

```
HandleId<T>.Value : long

 63                              32   31   30                             0
┌──────────────────────────────────┬──────┬──────────────────────────────────┐
│            Generation            │ View │               Slot               │
│             32 bits              │ 1 bit│             31 bits              │
└──────────────────────────────────┴──────┴──────────────────────────────────┘
                                     0x8000_0000       SlotMask = 0x7FFF_FFFF
```

```csharp
Slot       => (int)((uint)Value & 0x7FFF_FFFF)   // which row in the table
Generation => (uint)(Value >> 32)                // how often that row was recycled
IsView     => ((uint)Value & 0x8000_0000) != 0   // non-owning: release does nothing
IsDefault  => Value == 0                         // never live (generations start at 1)
```

## Worked example

```csharp
var window  = new Window("demo", 640, 480);   // slot 0
var texture = renderer.CreateTexture(...);    // slot 2, owner = renderer
```

| step                                   | `Value` (hex)         | `ToString()` | meaning                     |
|----------------------------------------|-----------------------|--------------|-----------------------------|
| first handle ever                      | `0000_0001_0000_0000` | `#0.1`       | slot 0, gen 1, owning       |
| `window.GetWindowSurface()` (SDL owns it)    | `0000_0001_8000_0001` | `#1.1&`      | slot 1, gen 1, **view**     |
| same surface asked for a second time   | `0000_0001_8000_0001` | `#1.1&`      | interned → identical id     |
| `texture`                              | `0000_0001_0000_0002` | `#2.1`       | slot 2, gen 1, owning       |
| after `texture.Dispose()`, slot 2 now  | `gen = 2`             | —            | old id's gen 1 ≠ 2 → stale  |
| next resource reusing slot 2           | `0000_0002_0000_0002` | `#2.2`       | same slot, new generation   |

Note `default(Texture)` is `0x0000_0000_0000_0000` — slot 0, generation 0. Live slots start at generation **1**,
so the zeroed struct can never accidentally address slot 0.

`IsDefault` and `IsValid` are different questions: `default` means *"no argument"* (SDL takes `NULL`), a disposed
handle means *"this is gone"* and must throw.

## Equality ignores the view bit

```csharp
0000_0001_0000_0001   // owning handle to surface #1
0000_0001_8000_0001   // view of the same surface
                ^ only this bit differs → Equals == true
```

Both address one resource, so `Equals`/`GetHashCode` mask `ViewFlag` off. `Release` does not: a view returns
`false` immediately and never frees anything.

## Slot → storage address

Slots live in chunks of 1024 (`SegmentShift = 10`), so the table grows without ever moving existing rows —
readers can resolve lock-free while another thread allocates.

```
slot 1234 = 0b 0000_0000_0000_0000_0000_01 00_1101_0010
               └──────────── 22 bit ──────┘└── 10 bit ─┘
                 1234 >> 10   =   1          1234 & 1023  =  210
                 which segment               index inside it

_segments[1][210]
```

## Acquire: three doors, one table

```csharp
Acquire(ptr, HandleKind.Owned,    owner)   // we must free it   → SDL.DestroyTexture etc.
Acquire(ptr, HandleKind.Borrowed, owner)   // SDL frees it      → window surface, camera frame, tray entry
AcquireShared(ptr, owner)                  // n acquisitions, n releases (SDL_net addresses)
```

What happens when the pointer is **already interned**:

| call                    | existing owner alive | result                                       |
|-------------------------|----------------------|----------------------------------------------|
| `Acquire(…, Owned)`     | yes or no            | retire old slot, **new** slot (address reuse)|
| `Acquire(…, Borrowed)`  | yes                  | **view** of the existing slot, refcount kept |
| `Acquire(…, Borrowed)`  | no                   | retire old slot, new slot                    |
| `AcquireShared`         | yes                  | `RefCount++`, same slot + generation         |
| `AcquireShared`         | no                   | retire old slot, new slot                    |

The `Owned` row matters: malloc reuses addresses. If SDL hands back a pointer we already know as owned, the old
slot is stale by definition — its resource was freed.

## Ownership: `OwnerId`

```csharp
OwnerId AsOwner => new OwnerId(Value & ~ViewFlag);   // same bits, view flag stripped
```

A child slot stores its parent's id. Every resolve walks that chain (up to `MaxOwnerDepth = 8`) and fails if any
ancestor's generation no longer matches:

```
texture #2.1 ──owner──▶ renderer #1.1 ──owner──▶ window #0.1
                                                       │
renderer.Dispose()  →  slot 1 becomes gen 2            │
texture resolve: lookup #1.1 → slot 1 is gen 2 ≠ 1 ────┘  ObjectDisposedException
```

Nothing has to be propagated to children — the generation mismatch does it. `Texture.Dispose()` after that point
is a silent no-op, not a double free.

## Moving a resource to a new owner

`Reparent` writes a new owner into the slot and leaves the generation alone, so nothing goes stale and
every copy of the handle follows. Only SDL_ttf needs it: `TTF_SetTextEngine` re-homes a text under a
different engine, and the engine that currently draws a text is the one whose destruction has to take it
down.

That it works on the slot rather than the handle is the point. A wrapper keeping its owner in a field
could only ever have re-homed the one copy it was called on, and every other copy would have gone on
waiting for the wrong engine to die.

## Release vs. Invalidate

```csharp
Release(id, out ptr, out owned)   // refcount--, hands the pointer to exactly ONE caller
Invalidate(id)                    // SDL already reclaimed it: kill the slot, ignore refcount
```

`Release` returns `true` only to the last reference, and only then does the wrapper call `SDL.DestroyX`. A view
always gets `false`. `Invalidate` is for the cases where SDL consumes a resource behind our back
(`SavePNG(stream, closeAfter: true)` closing the `IOStream`, `GetWindowSurface()` retiring the surface SDL swapped out on a resize).

## One exception: destroyed is not freed

A dead owner normally ends the story — SDL freed the child, so there is nothing to hand over and `Release` says
`false`. A renderer is the exception SDL documents:

```c
SDL_DestroyWindow(window)  ->  SDL_DestroyRendererWithoutFreeing(renderer)   // stops it working
SDL_DestroyRenderer(renderer)  ->  SDL_free(renderer)                        // frees it, in either order
```

So a window's renderer is acquired with `survivesOwner: true`, which puts one bit in its slot:

| question                          | answer for such a slot                    |
|-----------------------------------|-------------------------------------------|
| `Resolve` / `IsLive` after owner   | stale, exactly like any other child       |
| `AddRef` after owner               | `false` — nothing new may point at it     |
| `Release` after owner              | **`true`, once** — `SDL_DestroyRenderer`  |
| `CountLeaks` after owner           | **counted** — the missing `Dispose` is real |

That is the whole of it: dead to every reader, alive to exactly one `Dispose`. Textures below the renderer are
*not* marked, because `SDL_DestroyWindow` really does `SDL_free` every one of them.

A software renderer is deliberately not marked either. Nothing in SDL ties it to the surface it draws into, and
`SDL_DestroyRenderer` flushes pending commands into that surface on the way out — so it has to be disposed
*before* its surface, and afterwards it is an ordinary no-op.

## Reclaiming slots nobody releases

`Release` is what retires a slot, and it refuses a handle whose owner is gone — correctly, because SDL
already freed that resource and there is nothing to hand over. But then *nothing* retires it: the slot keeps a
pointer no reader may follow and keeps its address in `Interned`, for the life of the process. One slot per
texture that died with its renderer, every level reload.

Retiring children the moment their owner dies would mean walking the table on every teardown — disposing a
renderer with ten thousand textures must stay O(1). So the table takes them back lazily instead, in
`RentLocked`, at the one moment the scan pays for itself:

```
RentLocked()
  free stack has one?      -> pop it, done
  about to grow?           -> sweep orphans first, and pop one of those instead
```

The sweep is guarded twice, and both guards matter:

- **something was retired since the last sweep** — otherwise nothing can have been orphaned, which is what
  stops a table that is only ever filled (every start-up) from scanning itself on every slot;
- **about `SlotsUsed` rents have happened since the last sweep** — the scan is O(`SlotsUsed`), so this makes
  it amortised constant per rent instead of quadratic for a caller that creates faster than it destroys.

Nothing native happens in a sweep: it is bookkeeping catching up with what SDL already did. Slots marked
`survivesOwner` are skipped — those still owe SDL a call, and sweeping one would turn a reported leak into an
unreported one.

## Generation overflow

`RetireLocked` bumps the generation and pushes the slot back onto the free stack — **unless** it wrapped to 0.
A slot that has been recycled 2³² times is burned forever, so no old id can ever match again.

## Diagnostics

```csharp
HandleTable.SlotsUsed      // slots ever handed out — tests assert this stops growing
HandleTable.CountLeaks()   // live Owned slots with a live owner chain, plus the `survivesOwner` ones
HandleTable.ReportLeaks()  // the same, logged per type name
HandleTable.Reset()        // only valid after SDL itself is down
```

Each slot carries a `TypeTag` (an index into `TypeNames`, assigned once per `T` by the static
`TypeTag<T>` initializer), which is why a leak report can name the type. It is process-local — never persist it.

## Threading

Every mutation (`Acquire`, `Release`, `Invalidate`, …) takes the `Gate` lock. `Resolve` / `IsLive` /
`PointerOrZero` take **no lock**: they `Volatile.Read` the segment array, read the slot and compare the
generation. Worst case a racing resolve sees a retired slot and reports the handle as stale — which it is.
