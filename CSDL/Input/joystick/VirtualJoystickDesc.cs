// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;

namespace CSDL.Input {
    public partial struct VirtualJoystickDesc {
        /// <summary>Called when the joystick state should be updated.</summary>
        public delegate void UpdateDelegate(object? userdata);

        /// <summary>Called when the player index is set.</summary>
        public delegate void SetPlayerIndexDelegate(object? userdata, int playerIndex);

        /// <summary>Implements <c>SDL_RumbleJoystick</c>.</summary>
        public delegate bool RumbleDelegate(object? userdata, ushort lowFrequencyRumble, ushort highFrequencyRumble);

        /// <summary>Implements <c>SDL_RumbleJoystickTriggers</c>.</summary>
        public delegate bool RumbleTriggersDelegate(object? userdata, ushort leftRumble, ushort rightRumble);

        /// <summary>Implements <c>SDL_SetJoystickLED</c>.</summary>
        public delegate bool SetLedDelegate(object? userdata, byte red, byte green, byte blue);

        /// <summary>Implements <c>SDL_SendJoystickEffect</c>. <paramref name="data"/> points to <paramref name="size"/> bytes.</summary>
        public delegate bool SendEffectDelegate(object? userdata, nint data, int size);

        /// <summary>Implements <c>SDL_SetGamepadSensorEnabled</c>.</summary>
        public delegate bool SetSensorsEnabledDelegate(object? userdata, bool enabled);

        /// <summary>Called by SDL to clean up <paramref name="userdata"/> when the joystick is detached - runs before this struct's own <c>Detach</c> cleanup.</summary>
        public delegate void CleanupDelegate(object? userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UpdateNative(nint userdata);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetPlayerIndexNative(nint userdata, int playerIndex);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool RumbleNative(nint userdata, ushort lowFrequencyRumble, ushort highFrequencyRumble);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool RumbleTriggersNative(nint userdata, ushort leftRumble, ushort rightRumble);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool SetLedNative(nint userdata, byte red, byte green, byte blue);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool SendEffectNative(nint userdata, nint data, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate CBool SetSensorsEnabledNative(nint userdata, CBool enabled);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CleanupNative(nint userdata);

        private sealed class Callbacks {
            public UpdateDelegate? Update { get; init; }
            public SetPlayerIndexDelegate? SetPlayerIndex { get; init; }
            public RumbleDelegate? Rumble { get; init; }
            public RumbleTriggersDelegate? RumbleTriggers { get; init; }
            public SetLedDelegate? SetLed { get; init; }
            public SendEffectDelegate? SendEffect { get; init; }
            public SetSensorsEnabledDelegate? SetSensorsEnabled { get; init; }
            public CleanupDelegate? Cleanup { get; init; }
            public object? UserData { get; init; }
            public nint NamePtr;
            public nint TouchpadsPtr;
            public nint SensorsPtr;
        }

        private static Callbacks Resolve(nint userdata) {
            return (Callbacks)CallbackRegistry.GetUserdata(userdata)!;
        }

        private static void UpdateTrampoline(nint userdata) {
            try {
                Callbacks cb = Resolve(userdata);
                cb.Update?.Invoke(cb.UserData);
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick update callback failed.");
            }
        }

        private static void SetPlayerIndexTrampoline(nint userdata, int playerIndex) {
            try {
                Callbacks cb = Resolve(userdata);
                cb.SetPlayerIndex?.Invoke(cb.UserData, playerIndex);
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick player-index callback failed.");
            }
        }

        private static CBool RumbleTrampoline(nint userdata, ushort low, ushort high) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.Rumble?.Invoke(cb.UserData, low, high) ?? false;
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick rumble callback failed.");
                return false;
            }
        }

        private static CBool RumbleTriggersTrampoline(nint userdata, ushort left, ushort right) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.RumbleTriggers?.Invoke(cb.UserData, left, right) ?? false;
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick trigger-rumble callback failed.");
                return false;
            }
        }

        private static CBool SetLedTrampoline(nint userdata, byte red, byte green, byte blue) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.SetLed?.Invoke(cb.UserData, red, green, blue) ?? false;
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick LED callback failed.");
                return false;
            }
        }

        private static CBool SendEffectTrampoline(nint userdata, nint data, int size) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.SendEffect?.Invoke(cb.UserData, data, size) ?? false;
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick effect callback failed.");
                return false;
            }
        }

        private static CBool SetSensorsEnabledTrampoline(nint userdata, CBool enabled) {
            try {
                Callbacks cb = Resolve(userdata);
                return cb.SetSensorsEnabled?.Invoke(cb.UserData, enabled) ?? false;
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick sensor callback failed.");
                return false;
            }
        }

        private static void CleanupTrampoline(nint userdata) {
            try {
                Callbacks cb = Resolve(userdata);
                cb.Cleanup?.Invoke(cb.UserData);
            }
            catch (Exception ex) {
                Log.Error(ex, "Virtual joystick cleanup callback failed.");
            }
        }

        /// <summary>
        /// Reproduces the <c>SDL_INIT_INTERFACE</c> macro's <c>version</c> assignment (<c>desc-&gt;version = sizeof(*desc)</c>).
        /// </summary>
        public void InitVersion() {
            Version = (uint)Marshal.SizeOf<VirtualJoystickDesc>();
        }

        /// <summary>
        /// Registers every provided callback with <see cref="CallbackRegistry"/>, allocates
        /// <paramref name="name"/>/<paramref name="touchpads"/>/<paramref name="sensors"/> as unmanaged
        /// memory, and fills in this struct's <see cref="UserData"/> and callback fields. Every
        /// callback parameter is optional (<see langword="null"/>) - matching SDL's own "all elements
        /// of this structure are optional" contract - and simply left at its zeroed default (a null
        /// function pointer) when omitted. Call <see cref="Detach"/> with the returned id once
        /// <c>SDL_DetachVirtualJoystick</c> has run, to free everything allocated here.
        /// </summary>
        internal string Attach(
            string? name, VirtualJoystickTouchpadDesc[]? touchpads, VirtualJoystickSensorDesc[]? sensors,
            UpdateDelegate? update, SetPlayerIndexDelegate? setPlayerIndex, RumbleDelegate? rumble,
            RumbleTriggersDelegate? rumbleTriggers, SetLedDelegate? setLed, SendEffectDelegate? sendEffect,
            SetSensorsEnabledDelegate? setSensorsEnabled, CleanupDelegate? cleanup, object? userData) {
            string id = Guid.NewGuid().ToString("N");
            Callbacks callbacks = new Callbacks {
                Update = update, SetPlayerIndex = setPlayerIndex, Rumble = rumble, RumbleTriggers = rumbleTriggers,
                SetLed = setLed, SendEffect = sendEffect, SetSensorsEnabled = setSensorsEnabled, Cleanup = cleanup,
                UserData = userData,
            };

            if (!string.IsNullOrEmpty(name)) {
                callbacks.NamePtr = Marshal.StringToCoTaskMemUTF8(name);
                _name = callbacks.NamePtr;
            }

            if (touchpads is { Length: > 0 }) {
                callbacks.TouchpadsPtr = AllocArray(touchpads);
                Touchpads = callbacks.TouchpadsPtr;
                Ntouchpads = (ushort)touchpads.Length;
            }

            if (sensors is { Length: > 0 }) {
                callbacks.SensorsPtr = AllocArray(sensors);
                Sensors = callbacks.SensorsPtr;
                Nsensors = (ushort)sensors.Length;
            }

            // Only this first registration actually allocates a GCHandle, same reasoning as
            // IOStreamInterface.Attach - unlike SDL_OpenIO/SDL_OpenStorage, SDL_AttachVirtualJoystick has
            // no separate userData parameter, so the resulting pointer goes straight into this struct's
            // own UserData field instead.
            (IntPtr updatePtr, IntPtr userdataPtr) = CallbackRegistry.Register<UpdateDelegate, UpdateNative>(id + ":update", update ?? (_ => { }), UpdateTrampoline, callbacks);
            Update = updatePtr;
            UserData = userdataPtr;

            if (setPlayerIndex != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<SetPlayerIndexDelegate, SetPlayerIndexNative>(id + ":setPlayerIndex", setPlayerIndex, SetPlayerIndexTrampoline);
                SetPlayerIndex = ptr;
            }
            if (rumble != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<RumbleDelegate, RumbleNative>(id + ":rumble", rumble, RumbleTrampoline);
                Rumble = ptr;
            }
            if (rumbleTriggers != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<RumbleTriggersDelegate, RumbleTriggersNative>(id + ":rumbleTriggers", rumbleTriggers, RumbleTriggersTrampoline);
                RumbleTriggers = ptr;
            }
            if (setLed != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<SetLedDelegate, SetLedNative>(id + ":setLed", setLed, SetLedTrampoline);
                SetLed = ptr;
            }
            if (sendEffect != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<SendEffectDelegate, SendEffectNative>(id + ":sendEffect", sendEffect, SendEffectTrampoline);
                SendEffect = ptr;
            }
            if (setSensorsEnabled != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<SetSensorsEnabledDelegate, SetSensorsEnabledNative>(id + ":setSensorsEnabled", setSensorsEnabled, SetSensorsEnabledTrampoline);
                SetSensorsEnabled = ptr;
            }
            if (cleanup != null) {
                (IntPtr ptr, _) = CallbackRegistry.Register<CleanupDelegate, CleanupNative>(id + ":cleanup", cleanup, CleanupTrampoline);
                Cleanup = ptr;
            }

            return id;
        }

        private static nint AllocArray<T>(T[] items) where T : unmanaged {
            int elementSize = Marshal.SizeOf<T>();
            nint ptr = Marshal.AllocHGlobal(elementSize * items.Length);
            for (int i = 0; i < items.Length; i++) {
                Marshal.StructureToPtr(items[i], ptr + i * elementSize, false);
            }
            return ptr;
        }

        /// <summary>
        /// Unregisters everything <see cref="Attach"/> registered under <paramref name="id"/> and frees
        /// its unmanaged allocations. Call only after <c>SDL_DetachVirtualJoystick</c> has returned -
        /// SDL calls the app's <c>Cleanup</c> callback (still reachable through the registrations this
        /// removes) during that call, not after it.
        /// </summary>
        internal static void Detach(string id) {
            // The "update" registration is always present (Attach registers a no-op if the app didn't
            // supply one) and carries the shared Callbacks instance as its userdata - fetch it before
            // Unregister below removes the entry.
            Callbacks? callbacks = CallbackRegistry.TryGetUserdata<UpdateDelegate, UpdateNative>(id + ":update", out object? userdata)
                ? userdata as Callbacks
                : null;

            CallbackRegistry.Unregister<UpdateDelegate, UpdateNative>(id + ":update");
            CallbackRegistry.Unregister<SetPlayerIndexDelegate, SetPlayerIndexNative>(id + ":setPlayerIndex");
            CallbackRegistry.Unregister<RumbleDelegate, RumbleNative>(id + ":rumble");
            CallbackRegistry.Unregister<RumbleTriggersDelegate, RumbleTriggersNative>(id + ":rumbleTriggers");
            CallbackRegistry.Unregister<SetLedDelegate, SetLedNative>(id + ":setLed");
            CallbackRegistry.Unregister<SendEffectDelegate, SendEffectNative>(id + ":sendEffect");
            CallbackRegistry.Unregister<SetSensorsEnabledDelegate, SetSensorsEnabledNative>(id + ":setSensorsEnabled");
            CallbackRegistry.Unregister<CleanupDelegate, CleanupNative>(id + ":cleanup");

            if (callbacks is null) return;
            if (callbacks.NamePtr != 0) Marshal.FreeCoTaskMem(callbacks.NamePtr);
            if (callbacks.TouchpadsPtr != 0) Marshal.FreeHGlobal(callbacks.TouchpadsPtr);
            if (callbacks.SensorsPtr != 0) Marshal.FreeHGlobal(callbacks.SensorsPtr);
        }
    }
}
