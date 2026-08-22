using System;
using CSDL3.Tests.TestSupport;
using Sdl = CSDL;

namespace CSDL3.Tests.Events {
    [Collection(SdlCollection.Name)]
    public sealed class EventDispatchNativeTests {
        [Fact]
        public void DispatchEvent_UsesTheCorrectSpecializedUnionArms() {
            Sdl.GamepadSensorEvent? sensorEvent = null;
            Sdl.JoyBatteryEvent? batteryEvent = null;
            Sdl.PenButtonEvent? buttonEvent = null;

            void OnSensorUpdated(Sdl.GamepadSensorEvent e) => sensorEvent = e;
            void OnBatteryUpdated(Sdl.JoyBatteryEvent e) => batteryEvent = e;
            void OnButtonChanged(Sdl.PenButtonEvent e) => buttonEvent = e;

            Sdl.Events.Gamepad.SensorUpdated += OnSensorUpdated;
            Sdl.Events.Joystick.BatteryUpdated += OnBatteryUpdated;
            Sdl.Events.Pen.ButtonChanged += OnButtonChanged;
            try {
                Sdl.Event sensor = new Sdl.Event {
                    Gsensor = new Sdl.GamepadSensorEvent { Type = Sdl.EventType.GamepadSensorUpdate }
                };
                Sdl.Event battery = new Sdl.Event {
                    Jbattery = new Sdl.JoyBatteryEvent { Type = Sdl.EventType.JoystickBatteryUpdated }
                };
                Sdl.Event button = new Sdl.Event {
                    Pbutton = new Sdl.PenButtonEvent { Type = Sdl.EventType.PenButtonUp, Button = 2 }
                };

                Sdl.Events.DispatchEvent(sensor);
                Sdl.Events.DispatchEvent(battery);
                Sdl.Events.DispatchEvent(button);

                Assert.Equal(Sdl.EventType.GamepadSensorUpdate, sensorEvent?.Type);
                Assert.Equal(Sdl.EventType.JoystickBatteryUpdated, batteryEvent?.Type);
                Assert.Equal(Sdl.EventType.PenButtonUp, buttonEvent?.Type);
                Assert.Equal((byte)2, buttonEvent?.Button);
            }
            finally {
                Sdl.Events.Gamepad.SensorUpdated -= OnSensorUpdated;
                Sdl.Events.Joystick.BatteryUpdated -= OnBatteryUpdated;
                Sdl.Events.Pen.ButtonChanged -= OnButtonChanged;
            }
        }

        [Fact]
        public void DispatchEvent_DoesNotTreatKeymapChangesAsKeyInput() {
            int keyEvents = 0;
            Sdl.CommonEvent? keymapEvent = null;

            void OnKeyChanged(Sdl.KeyboardEvent _) => keyEvents++;
            void OnKeymapChanged(Sdl.CommonEvent e) => keymapEvent = e;

            Sdl.Events.Keyboard.KeyChanged += OnKeyChanged;
            Sdl.Events.Keyboard.KeymapChanged += OnKeymapChanged;
            try {
                Sdl.Event @event = new Sdl.Event {
                    Common = new Sdl.CommonEvent { Type = (uint)Sdl.EventType.KeymapChanged }
                };

                Sdl.Events.DispatchEvent(@event);

                Assert.Equal(0, keyEvents);
                Assert.Equal((uint)Sdl.EventType.KeymapChanged, keymapEvent?.Type);
            }
            finally {
                Sdl.Events.Keyboard.KeyChanged -= OnKeyChanged;
                Sdl.Events.Keyboard.KeymapChanged -= OnKeymapChanged;
            }
        }

        [Fact]
        public void DispatchEvent_HandlesTheWholeUserEventRange() {
            Sdl.UserEvent? received = null;
            Action<Sdl.UserEvent> previous = Sdl.Events.User.OnUserEvent;
            Sdl.Events.User.OnUserEvent = e => received = e;
            try {
                uint type = (uint)Sdl.EventType.User + 1;
                Sdl.Event @event = new Sdl.Event {
                    User = new Sdl.UserEvent { Type = type, Code = 42 }
                };

                Sdl.Events.DispatchEvent(@event);

                Assert.Equal(type, received?.Type);
                Assert.Equal(42, received?.Code);
            }
            finally {
                Sdl.Events.User.OnUserEvent = previous;
            }
        }

        [Fact]
        public void DispatchEvent_ExposesNotificationActions() {
            Sdl.NotificationEvent? received = null;

            void OnActionInvoked(Sdl.NotificationEvent e) => received = e;

            Sdl.Events.Notification.ActionInvoked += OnActionInvoked;
            try {
                Sdl.Event @event = new Sdl.Event {
                    Notification = new Sdl.NotificationEvent { Type = Sdl.EventType.NotificationActionInvoked }
                };

                Sdl.Events.DispatchEvent(@event);

                Assert.Equal(Sdl.EventType.NotificationActionInvoked, received?.Type);
            }
            finally {
                Sdl.Events.Notification.ActionInvoked -= OnActionInvoked;
            }
        }

        [Fact]
        public void PushEvent_DeliversLifecycleEventsThroughTheApplicationWatch() {
            int eventCount = 0;

            void OnApplicationEvent(Sdl.CommonEvent _) => eventCount++;

            Sdl.Events.Common.Any += OnApplicationEvent;
            try {
                Sdl.Event @event = new Sdl.Event {
                    Common = new Sdl.CommonEvent { Type = (uint)Sdl.EventType.LowMemory }
                };

                Assert.True(Sdl.Events.PushEvent(ref @event));
                Assert.Equal(1, eventCount);
            }
            finally {
                Sdl.Events.Common.Any -= OnApplicationEvent;
                Sdl.Events.Flush(Sdl.EventType.LowMemory);
            }
        }
    }
}
