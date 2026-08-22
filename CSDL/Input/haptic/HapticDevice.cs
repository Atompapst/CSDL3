// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Input {
    /// <summary>An opened SDL haptic device.</summary>
    public sealed class HapticDevice : NativeHandle<Opaque.SdlHaptic> {
        static HapticDevice() {
            Init.InitSubSystem(InitFlags.Haptic);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.OpenHaptic"/>
        public HapticDevice(HapticID id) {
            Handle = SDL.OpenHaptic(id).ThrowIfInvalid();
        }

        internal HapticDevice(NativePtr<Opaque.SdlHaptic> handle, bool ownsHandle = true) : base(handle, ownsHandle) { }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHapticFromID"/>
        /// <remarks>
        /// The device is still owned by whoever opened it, so disposing the returned wrapper does
        /// not close it.
        /// </remarks>
        public static HapticDevice? FromID(HapticID instanceID) {
            NativePtr<Opaque.SdlHaptic> haptic = SDL.GetHapticFromID(instanceID);
            return haptic.IsNull ? null : new HapticDevice(haptic, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHapticID"/>
        public HapticID Id => SDL.GetHapticID(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHapticName"/>
        public string Name => SDL.GetHapticName(Handle).ToUtf8String() ?? string.Empty;

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHapticFeatures"/>
        public uint Features => SDL.GetHapticFeatures(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetMaxHapticEffects"/>
        public int MaxEffects => SDL.GetMaxHapticEffects(Handle).LogIfInvalid(-1);

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetMaxHapticEffectsPlaying"/>
        public int MaxEffectsPlaying => SDL.GetMaxHapticEffectsPlaying(Handle).LogIfInvalid(-1);

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetNumHapticAxes"/>
        public int NumAxes => SDL.GetNumHapticAxes(Handle).LogIfInvalid(-1);

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.HapticEffectSupported"/>
        public bool IsEffectSupported(in HapticEffect effect) {
            return SDL.HapticEffectSupported(Handle, in effect);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.HapticRumbleSupported"/>
        public bool RumbleSupported => SDL.HapticRumbleSupported(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.CreateHapticEffect"/>
        public HapticEffectID CreateEffect(in HapticEffect effect) {
            HapticEffectID result = SDL.CreateHapticEffect(Handle, in effect);
            if (result.Value == -1) {
                Error.ThrowIfError(nameof(CreateEffect));
            }
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.DestroyHapticEffect"/>
        public void DestroyEffect(HapticEffectID effect) {
            SDL.DestroyHapticEffect(Handle, effect);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHapticEffectStatus"/>
        public bool GetEffectStatus(HapticEffectID effect) {
            return SDL.GetHapticEffectStatus(Handle, effect);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.UpdateHapticEffect"/>
        public bool UpdateEffect(HapticEffectID effect, in HapticEffect data) {
            return SDL.UpdateHapticEffect(Handle, effect, NativePtr<HapticEffect>.FromIn(in data)).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.RunHapticEffect"/>
        public bool RunEffect(HapticEffectID effect, uint iterations = 1) {
            return SDL.RunHapticEffect(Handle, effect, iterations).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.StopHapticEffect"/>
        public bool StopEffect(HapticEffectID effect) {
            return SDL.StopHapticEffect(Handle, effect).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.StopHapticEffects"/>
        public bool StopEffects() {
            return SDL.StopHapticEffects(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.InitHapticRumble"/>
        public bool InitializeRumble() {
            return SDL.InitHapticRumble(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.PlayHapticRumble"/>
        public bool PlayRumble(float strength, uint length) {
            return SDL.PlayHapticRumble(Handle, strength, length).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.StopHapticRumble"/>
        public bool StopRumble() {
            return SDL.StopHapticRumble(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.PauseHaptic"/>
        public bool Pause() {
            return SDL.PauseHaptic(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.ResumeHaptic"/>
        public bool Resume() {
            return SDL.ResumeHaptic(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.SetHapticAutocenter"/>
        public bool SetAutocenter(int value) {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);
            return SDL.SetHapticAutocenter(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.SetHapticGain"/>
        public bool SetGain(int value) {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);
            return SDL.SetHapticGain(Handle, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.CloseHaptic"/>
        protected override void DisposeResource() {
            SDL.CloseHaptic(Handle);
        }
    }
}
