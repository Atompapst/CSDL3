// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.EventHandlers;
using CSDL.EventHandlers.Interfaces;
using CSDL.Extensions;

namespace CSDL {
    public static partial class Events {
        private static EventType _currentEventType;

        private static readonly Lazy<Common> _common = new Lazy<Common>(() => EventHandlerRegistry.MarkAccessed(new Common()));
        private static readonly Lazy<Display> _display = new Lazy<Display>(() => new Display());
        private static readonly Lazy<Window> _window = new Lazy<Window>(() => EventHandlerRegistry.MarkAccessed(new Window()));
        private static readonly Lazy<Keyboard> _keyboard = new Lazy<Keyboard>(() => EventHandlerRegistry.MarkAccessed(new Keyboard()));
        private static readonly Lazy<Mouse> _mouse = new Lazy<Mouse>(() => EventHandlerRegistry.MarkAccessed(new Mouse()));
        private static readonly Lazy<AudioDevice> _audioDevice = new Lazy<AudioDevice>(() => new AudioDevice());
        private static readonly Lazy<Clipboard> _clipboard = new Lazy<Clipboard>(() => new Clipboard());
        private static readonly Lazy<Joystick> _joystick = new Lazy<Joystick>(() => EventHandlerRegistry.MarkAccessed(new Joystick()));
        private static readonly Lazy<Gamepad> _gamepad = new Lazy<Gamepad>(() => EventHandlerRegistry.MarkAccessed(new Gamepad()));
        private static readonly Lazy<Touch> _touch = new Lazy<Touch>(() => EventHandlerRegistry.MarkAccessed(new Touch()));
        private static readonly Lazy<Drop> _drop = new Lazy<Drop>(() => new Drop());
        private static readonly Lazy<Sensor> _sensor = new Lazy<Sensor>(() => EventHandlerRegistry.MarkAccessed(new Sensor()));
        private static readonly Lazy<Pen> _pen = new Lazy<Pen>(() => EventHandlerRegistry.MarkAccessed(new Pen()));
        private static readonly Lazy<Camera> _camera = new Lazy<Camera>(() => new Camera());
        private static readonly Lazy<Render> _render = new Lazy<Render>(() => new Render());
        private static readonly Lazy<Notification> _notification = new Lazy<Notification>(() => new Notification());
        private static readonly Lazy<User> _user = new Lazy<User>(() => new User());
        private static readonly EventFilter _applicationEventWatch = HandleApplicationEventWatch;
        private static readonly SDL_EventFilterNative _nativeApplicationEventWatch = EventFilterWrapper.Create(_applicationEventWatch);

        static Events() {
            Init.InitSubSystem(InitFlags.Events);
            SDL.AddEventWatch(_nativeApplicationEventWatch, IntPtr.Zero).ThrowIfFalse(nameof(SDL.AddEventWatch));


            foreach (string typeName in Enum.GetNames<EventType>()) {
                if (typeName == nameof(EventType.EnumPadding) ||
                    typeName.EndsWith("First", StringComparison.Ordinal) ||
                    typeName.EndsWith("Last", StringComparison.Ordinal)) continue;
                EventType type = Enum.Parse<EventType>(typeName);
                EventTypes.Add(type, new EnabledEvents(type));
            }

            Init.OnQuit += () => {
                if (!_logBuffer.IsNull) {
                    Memory.Free(_logBuffer);
                }
            };
        }

        public static IApplicationEvents Common => _common.Value;
        public static IDisplayEvents Display => _display.Value;
        public static IWindowEvents Window => _window.Value;
        public static IKeyboardEvents Keyboard => _keyboard.Value;
        public static IMouseEvents Mouse => _mouse.Value;
        public static IAudioDeviceEvents AudioDevice => _audioDevice.Value;
        public static IClipboardEvents Clipboard => _clipboard.Value;
        public static IJoystickEvents Joystick => _joystick.Value;
        public static IGamepadEvents Gamepad => _gamepad.Value;
        public static ITouchEvents Touch => _touch.Value;
        public static IDropEvents Drop => _drop.Value;
        public static ISensorEvents Sensor => _sensor.Value;
        public static IPenEvents Pen => _pen.Value;
        public static ICameraEvents Camera => _camera.Value;
        public static IRenderEvents Render => _render.Value;
        public static INotificationEvents Notification => _notification.Value;
        public static IUserEvents User => _user.Value;

        public static Dictionary<EventType, EnabledEvents> EventTypes { get; } = new Dictionary<EventType, EnabledEvents>();

        private static NativePtr<byte> _logBuffer = NativePtr<byte>.Zero;
        private static int _logBufferLength = 512;
        private static LogPriority _logPriority = LogPriority.Verbose;

        /// <summary>
        /// Gets or sets whether event logging is enabled.
        /// When enabled, every dispatched event is logged with its description.
        /// </summary>
        public static bool LogEvents { get; set; }

        /// <summary>
        /// Enables logging of every dispatched event with its description.
        /// Only applies to <see cref="PollAll"/> and <see cref="DispatchEvent"/>.
        /// </summary>
        /// <param name="enable">Whether to enable event logging.</param>
        /// <param name="bufferSize">Size of the buffer for event descriptions (default: 512 bytes).</param>
        /// <param name="priority">Log priority level (default: Verbose).</param>
        /// <seealso cref="GetDescription(in Event, NativePtr{byte}, int)">GetDescription</seealso>
        public static void SetEventLogging(bool enable, int bufferSize = 512, LogPriority priority = LogPriority.Verbose) {
            LogEvents = enable;
            _logPriority = priority;

            if (enable && (_logBuffer.IsNull || bufferSize != _logBufferLength)) {
                if (!_logBuffer.IsNull) {
                    Memory.Free(_logBuffer);
                }
                _logBufferLength = bufferSize;
                _logBuffer = Memory.CallocArray<byte>(bufferSize);
            } else if (!enable && !_logBuffer.IsNull) {
                Memory.Free(_logBuffer);
                _logBuffer = NativePtr<byte>.Zero;
            }
        }

        /// <summary>
        ///     Drains the entire SDL event queue in one call, dispatching every pending event to its
        ///     corresponding handler instance. Call this once per frame before reading any handler state.
        /// </summary>
        /// <remarks>
        ///     Resets all handler states, pumps the OS event
        ///     queue via <see cref="Pump" />, then dispatches each event to the relevant handler:
        ///     <list type="bullet">
        ///         <item><see cref="Common" />      — quit, locale change, system theme, low memory, etc.</item>
        ///         <item><see cref="Display" />     — display connected/disconnected, orientation change.</item>
        ///         <item><see cref="Window" />      — focus, resize, move, close, hit-test, etc.</item>
        ///         <item><see cref="Keyboard" />    — key down/up, text input, text editing, keyboard add/remove.</item>
        ///         <item><see cref="Mouse" />       — motion, button down/up, wheel, mouse add/remove.</item>
        ///         <item><see cref="Gamepad" />     — axis, button, device add/remove, touchpad, sensor.</item>
        ///         <item><see cref="Joystick" />    — axis, ball, hat, button, device add/remove, battery.</item>
        ///         <item><see cref="Touch" />       — finger down/up/motion, pinch begin/update/end.</item>
        ///         <item><see cref="Pen" />         — proximity, touch, motion, button, axis.</item>
        ///         <item><see cref="Drop" />        — file/text drop begin, position, complete.</item>
        ///         <item><see cref="AudioDevice" /> — device added/removed/format-changed.</item>
        ///         <item><see cref="Clipboard" />   — clipboard update.</item>
        ///         <item><see cref="Sensor" />      — sensor data update.</item>
        ///         <item><see cref="Camera" />      — camera device added/removed/approved/denied.</item>
        ///         <item><see cref="Render" />      — render targets reset, device reset/lost.</item>
        ///     </list>
        ///     Every handler reflects the events that occurred during
        ///     the current frame. Handler state is valid until the next call.
        /// </remarks>
        /// <seealso cref="PollEvent" />
        public static void PollAll() {
            BeginCycle();
            Pump();
            while (PollInternal(out Event e)) {
                Dispatch(e);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.PollEvent"/>
        /// <seealso cref="PollAll" />
        /// <seealso cref="DispatchEvent"/>
        public static void PollEvent(Action<Event> action) {
            BeginCycle();
            Pump();
            while (PollInternal(out Event e)) {
                action(e);
            }
        }

        /// <summary>
        /// Additionally, dispatches the event to the appropriate handler for passed Event.
        /// </summary>
        /// <remarks>
        /// Check out <see cref="PollAll"/> for more information.
        /// </remarks>
        public static void DispatchEvent(this Event e) {
            Dispatch(e);
        }

        private static bool PollInternal(out Event @event) {
            return SDL.PollEvent(out @event);
        }

        private static void BeginCycle() {
            EventHandlerRegistry.BeginCycleForAccessedHandlers();
        }

        private static void Dispatch(Event se) {
            _currentEventType = (EventType)se.Type;
            if (LogEvents && !_logBuffer.IsNull) {
                int result = SDL.GetEventDescription(in se, _logBuffer, _logBufferLength);
                if (result > 0) {
                    Log.Message(LogCategory.Application, _logPriority, _logBuffer);
                }
            }
            DispatchGenerated(se);
        }

        private static bool HandleApplicationEventWatch(object? _, ref Event @event) {
            switch ((EventType)@event.Type) {
                case EventType.Terminating:
                case EventType.LowMemory:
                case EventType.WillEnterBackground:
                case EventType.DidEnterBackground:
                case EventType.WillEnterForeground:
                case EventType.DidEnterForeground:
                    _common.Value.Handle(@event.Common);
                    break;
            }

            return true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.PumpEvents"/>
        public static void Pump() {
            SDL.PumpEvents();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.PeepEvents"/>
        public static int Peep(Event[]? events, EventAction action, EventType minType = EventType.First, EventType maxType = EventType.Last) {
            if (events == null || events.Length == 0) {
                return SDL.PeepEvents(NativePtr<Event>.Zero, 0, action, (uint)minType, (uint)maxType).LogIfInvalid(-1);
            }
            unsafe {
                fixed (Event* p = events) {
                    return SDL.PeepEvents(p, events.Length, action, (uint)minType, (uint)maxType).LogIfInvalid(-1);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.HasEvent"/> 
        public static bool Has(EventType type) {
            return SDL.HasEvent((uint)type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.HasEvents"/> 
        public static bool Has(EventType minType, EventType maxType) {
            return SDL.HasEvents((uint)minType, (uint)maxType);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.WaitEvent"/>
        public static bool Wait(out Event @event) {
            return SDL.WaitEvent(out @event).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.WaitEventTimeout"/> 
        public static bool WaitTimeout(out Event @event, int timeoutMs) {
            return SDL.WaitEventTimeout(out @event, timeoutMs);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.PushEvent"/> 
        public static bool PushEvent(ref Event @event) {
            return SDL.PushEvent(ref @event);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.SetEventFilter"/> 
        public static void SetEventFilter(EventFilter filter, object? userdata = null) {
            SDL_EventFilterNative native = EventFilterWrapper.Create(filter);
            (IntPtr _, IntPtr userdataPtr) entry = CallbackRegistry.RegisterSingle(filter, native, userdata);
            SDL.SetEventFilter(native, entry.userdataPtr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.GetEventFilter"/>
        public static bool GetFilter(out EventFilter? filter, out object? userdata) {
            bool hasFilter = SDL.GetEventFilter(out SDL_EventFilterNative nativeFilter, out IntPtr userdataPtr);
            if (hasFilter && CallbackRegistry.TryGet<EventFilter, SDL_EventFilterNative>(out filter, out _, out userdata)) {
                return true;
            }
            filter = null;
            userdata = null;
            return false;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.AddEventWatch"/> 
        public static void AddWatch(EventFilter filter, object? userdata = null) {
            SDL_EventFilterNative native = EventFilterWrapper.Create(filter);
            string id = $"EventWatch:{Guid.NewGuid()}";
            (IntPtr _, IntPtr userdataPtr) entry = CallbackRegistry.Register(id, filter, native, userdata);

            CBool ok = SDL.AddEventWatch(native, entry.userdataPtr);
            if (!ok) {
                CallbackRegistry.Unregister<EventFilter, SDL_EventFilterNative>(id);
            }
            ok.ThrowIfFalse(nameof(SDL.AddEventWatch));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.RemoveEventWatch"/> 
        public static void RemoveWatch(EventFilter filter, object? userdata = null) {
            if (!CallbackRegistry.TryFindByManagedCallback<EventFilter, SDL_EventFilterNative>(filter, userdata, out string? id, out SDL_EventFilterNative? nativeFilter, out IntPtr userdataPtr)) {
                return;
            }
            SDL.RemoveEventWatch(nativeFilter, userdataPtr);
            CallbackRegistry.Unregister<EventFilter, SDL_EventFilterNative>(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.FilterEvents"/>
        public static void Filter(EventFilter filter, object? userdata = null) {
            string id = $"FilterEvents:{Guid.NewGuid()}";
            SDL_EventFilterNative native = EventFilterWrapper.Create(filter);
            (IntPtr _, IntPtr userdataPtr) entry = CallbackRegistry.Register(id, filter, native, userdata);
            try {
                SDL.FilterEvents(native, entry.userdataPtr);
            }
            finally {
                CallbackRegistry.Unregister<EventFilter, SDL_EventFilterNative>(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.RegisterEvents"/> 
        public static uint Register(int numevents) {
            return SDL.RegisterEvents(numevents);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.GetWindowFromEvent"/> 
        public static Video.Window? GetWindowFromEvent(in Event @event) {
            NativePtr<Opaque.SdlWindow> ptr = new NativePtr<Opaque.SdlWindow>(SDL.GetWindowFromEvent(in @event));
            return ptr.IsNull ? null : new Video.Window(ptr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.GetEventDescription"/>
        public static int GetDescription(in Event @event, byte[] buf) {
            unsafe {
                fixed (byte* b = buf) {
                    return SDL.GetEventDescription(in @event, b, buf.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.GetEventDescription"/>
        public static int GetDescription(in Event @event, NativePtr<byte> buf, int buflen) {
            return SDL.GetEventDescription(in @event, buf, buflen);
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Events.FlushEvent"/>
        public static void Flush(EventType type = EventType.PollSentinel) {
            Flush((uint)type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.FlushEvents"/>
        public static void Flush(EventType minType, EventType maxType) {
            Flush((uint)minType, (uint)maxType);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.FlushEvent"/>
        private static void Flush(uint type) {
            SDL.FlushEvent(type);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Events.FlushEvents"/>
        private static void Flush(uint minType, uint maxType) {
            SDL.FlushEvents(minType, maxType);
        }

        public readonly struct EnabledEvents {
            private readonly uint _type;
            public EnabledEvents(EventType type) {
                _type = (uint)type;
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Events.SetEventEnabled"/>
            public bool Enabled {
                get => EventEnabled();
                set => SetEnabled(value);
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Events.SetEventEnabled"/>
            private void SetEnabled(bool enabled) {
                SDL.SetEventEnabled(_type, enabled);
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Events.EventEnabled"/>
            private bool EventEnabled() {
                return SDL.EventEnabled(_type);
            }
        }
    }
    internal interface IEventCycle {
        internal void BeginCycle();
    }
}
