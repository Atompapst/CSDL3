// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Audio;

namespace CSDL.EventHandlers.Interfaces {
    public interface IAudioDeviceEvents {
        event Action<PlaybackDeviceInfo>? PlaybackDeviceAdded;
        event Action<RecordingDeviceInfo>? RecordingDeviceAdded;
        event Action<PlaybackDeviceInfo>? PlaybackDeviceRemoved;
        event Action<RecordingDeviceInfo>? RecordingDeviceRemoved;
        event Action<PlaybackDeviceInfo>? PlaybackDeviceFormatChanged;
        event Action<RecordingDeviceInfo>? RecordingDeviceFormatChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class AudioDevice : Interfaces.IAudioDeviceEvents {
        public event Action<PlaybackDeviceInfo>? PlaybackDeviceAdded;
        public event Action<RecordingDeviceInfo>? RecordingDeviceAdded;
        public event Action<PlaybackDeviceInfo>? PlaybackDeviceRemoved;
        public event Action<RecordingDeviceInfo>? RecordingDeviceRemoved;
        public event Action<PlaybackDeviceInfo>? PlaybackDeviceFormatChanged;
        public event Action<RecordingDeviceInfo>? RecordingDeviceFormatChanged;

        internal void Handle(AudioDeviceEvent e) {
            switch (e.Type) {
                case EventType.AudioDeviceAdded:
                {
                    if (e.Recording) {
                        AudioDevices.OnRecordingAdded(e.Which, e.Timestamp);
                        RecordingDeviceInfo? info = AudioDevices.GetRecordingInfo(e.Which);
                        if (info != null) RecordingDeviceAdded?.Invoke(info);
                    } else {
                        AudioDevices.OnPlaybackAdded(e.Which, e.Timestamp);
                        PlaybackDeviceInfo? info = AudioDevices.GetPlaybackInfo(e.Which);
                        if (info != null) PlaybackDeviceAdded?.Invoke(info);
                    }
                    break;
                }
                case EventType.AudioDeviceRemoved:
                {
                    if (e.Recording) {
                        RecordingDeviceInfo? info = AudioDevices.GetRecordingInfo(e.Which);
                        AudioDevices.OnRecordingRemoved(e.Which);
                        if (info != null) RecordingDeviceRemoved?.Invoke(info);
                    } else {
                        PlaybackDeviceInfo? info = AudioDevices.GetPlaybackInfo(e.Which);
                        AudioDevices.OnPlaybackRemoved(e.Which);
                        if (info != null) PlaybackDeviceRemoved?.Invoke(info);
                    }
                    break;
                }
                case EventType.AudioDeviceFormatChanged:
                {
                    if (e.Recording) {
                        AudioDevices.OnRecordingUpdated(e.Which, e.Timestamp);
                        RecordingDeviceInfo? info = AudioDevices.GetRecordingInfo(e.Which);
                        if (info != null) RecordingDeviceFormatChanged?.Invoke(info);
                    } else {
                        AudioDevices.OnPlaybackUpdated(e.Which, e.Timestamp);
                        PlaybackDeviceInfo? info = AudioDevices.GetPlaybackInfo(e.Which);
                        if (info != null) PlaybackDeviceFormatChanged?.Invoke(info);
                    }
                    break;
                }
            }
        }
    }
}
