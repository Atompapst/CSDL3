# Event Handlers

This folder contains the internal event handlers backing the public `Events.*` API (see `CSDL/Internal/Events/`).

## Structure

Each SDL event group has an `internal sealed class` implementing a public interface (`I...Events`) declared in
`CSDL.EventHandlers.Interfaces`. Handlers inherit `EventHandlerBase`, which provides frame-based dirty tracking:
`MarkDirty()` / `IncrementCounter()` flag the handler, and `ResetState()` is only called on frames where something
actually happened.

Handlers with a **simple** SDL event type (e.g. `Common`, `Window`, `Display`, `Drop`, `Clipboard`, `AudioDevice`,
`Camera`, `Render`, `User`, `Sensor`) stay as one file.

Handlers with **multiple event variants** SDL event types (e.g. `Mouse`, `Gamepad`, `Joystick`, `Keyboard`, `Pen`,
`Touch`) are split into `partial class` files, one per event type, using the naming convention:

```
<Handler>.cs                 // base file
<Handler>.<EventType>Event.cs  // one file per SDL event type
```

Example: `Gamepad.cs`, `Gamepad.ButtonEvent.cs`, `Gamepad.AxisEvent.cs`, ...

## Rules

**Base file (`<Handler>.cs`)**

- Class declaration: `internal sealed partial class X : EventHandlerBase, Interfaces.IXEvents`
- Public query/API methods only (`IsDown`, `GetAxis`, `HasAny...`, etc.) — no state fields that belong to a single event
  type
- `ResetState()` that calls one `partial void Reset<EventType>State()` per event-type file
- `partial void Reset<EventType>State();` declarations (no body) for every event-type file

**Event-type file (`<Handler>.<EventType>Event.cs`)**

- Everything specific to that one SDL event type: private state fields/dictionaries, `EventCounter`, `Last...Event`
  property, public `event Action<...>` fields, the `internal void Handle(...)` method, and
  `partial void Reset<EventType>State() { ... }` implementation
- Private helpers only used by this event type stay here too (e.g. `EnsureXStateContainers`, `RemoveXState`), even if
  another event-type file calls them (e.g. device-added/removed cleanup)
- If a `Handle` needs state owned by another event-type file, call that file's helper method directly rather than
  duplicating state

**Interface (`CSDL.EventHandlers.Interfaces.I...Events`)**

- Split the same way as the class: `public partial interface IXEvents` per file, containing only the members declared in
  that file

## Adding a new event type to an existing multi-event handler

1. Create `<Handler>.<NewEvent>Event.cs` with the `partial interface` block and `partial class` block as above.
2. Add the `partial void Reset<NewEvent>State();` declaration and call in the base file's `ResetState()`.
3. Wire up dispatch in `CSDL/Events/_Generated/EventDispatch.g.cs` if not already generated.
4. Build (`dotnet build`) to verify.

## Adding a brand new handler

1. Decide single-file vs multi-file based on how many distinct SDL event types it handles.
2. Follow the patterns above.
3. Register the lazy instance in `CSDL/Events/Events.cs` (`EventHandlerRegistry.MarkAccessed(new X())` if it has
   resettable per-frame state, plain `new X()` if stateless).
4. Add dispatch cases in `EventDispatch.g.cs`.
