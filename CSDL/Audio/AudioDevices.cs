// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
using System.Linq;
using CSDL.Extensions;
namespace CSDL.Audio {
    public static class AudioDevices {
        private static readonly Dictionary<uint, PlaybackDeviceInfo> _playbackDevices = new Dictionary<uint, PlaybackDeviceInfo>();
        private static readonly Dictionary<uint, RecordingDeviceInfo> _recordingDevices = new Dictionary<uint, RecordingDeviceInfo>();
        private static readonly object DevicesLock = new object();

        static AudioDevices() {
            Init.InitSubSystem(InitFlags.Audio);
            Refresh();
        }

        public static IReadOnlyCollection<PlaybackDeviceInfo> Playback { get { lock (DevicesLock) return _playbackDevices.Values.ToArray(); } }
        public static IReadOnlyCollection<RecordingDeviceInfo> Recording { get { lock (DevicesLock) return _recordingDevices.Values.ToArray(); } }

        public static int PlaybackCount { get { lock (DevicesLock) return _playbackDevices.Count; } }
        public static int RecordingCount { get { lock (DevicesLock) return _recordingDevices.Count; } }

        internal static void OnPlaybackAdded(uint id, ulong timestamp) {
            lock (DevicesLock) {
                if (!_playbackDevices.ContainsKey(id)) {
                    _playbackDevices[id] = new PlaybackDeviceInfo(id, timestamp);
                }
            }
        }

        internal static void OnRecordingAdded(uint id, ulong timestamp) {
            lock (DevicesLock) {
                if (!_recordingDevices.ContainsKey(id)) {
                    _recordingDevices[id] = new RecordingDeviceInfo(id, timestamp);
                }
            }
        }

        internal static void OnPlaybackRemoved(uint id) {
            lock (DevicesLock) _playbackDevices.Remove(id);
        }

        internal static void OnRecordingRemoved(uint id) {
            lock (DevicesLock) _recordingDevices.Remove(id);
        }

        internal static void OnPlaybackUpdated(uint id, ulong timestamp) {
            lock (DevicesLock) {
                if (_playbackDevices.ContainsKey(id)) {
                    _playbackDevices[id] = new PlaybackDeviceInfo(id, timestamp);
                }
            }
        }

        internal static void OnRecordingUpdated(uint id, ulong timestamp) {
            lock (DevicesLock) {
                if (_recordingDevices.ContainsKey(id)) {
                    _recordingDevices[id] = new RecordingDeviceInfo(id, timestamp);
                }
            }
        }


        /// <seealso cref="CSDL.Internal.Docs.Audio.GetAudioPlaybackDevices">GetAudioPlaybackDevices</seealso>
        /// <seealso cref="CSDL.Internal.Docs.Audio.GetAudioRecordingDevices">GetAudioRecordingDevices</seealso>
        public static void Refresh() {
            lock (DevicesLock) {
                _playbackDevices.Clear();
                _recordingDevices.Clear();

                NativePtr<AudioDeviceID> playbackIds = SDL.GetAudioPlaybackDevices(out int playbackCount).LogIfInvalid();
                if (!playbackIds.IsNull) {
                    for (int i = 0; i < playbackCount; i++) {
                        uint id = playbackIds[i];
                        if (!_playbackDevices.ContainsKey(id)) {
                            _playbackDevices[id] = new PlaybackDeviceInfo(id, 0);
                        }
                    }
                }
                playbackIds.Free();

                NativePtr<AudioDeviceID> recordingIds = SDL.GetAudioRecordingDevices(out int recordingCount).LogIfInvalid();
                if (!recordingIds.IsNull) {
                    for (int i = 0; i < recordingCount; i++) {
                        uint id = recordingIds[i];
                        if (!_recordingDevices.ContainsKey(id)) {
                            _recordingDevices[id] = new RecordingDeviceInfo(id, 0);
                        }
                    }
                }
                recordingIds.Free();
            }
        }

        public static bool IsPlaybackPresent(uint id) {
            lock (DevicesLock) return _playbackDevices.ContainsKey(id);
        }
        public static bool IsRecordingPresent(uint id) {
            lock (DevicesLock) return _recordingDevices.ContainsKey(id);
        }

        public static PlaybackDeviceInfo? GetPlaybackInfo(uint id) {
            lock (DevicesLock) return _playbackDevices.GetValueOrDefault(id);
        }

        public static RecordingDeviceInfo? GetRecordingInfo(uint id) {
            lock (DevicesLock) return _recordingDevices.GetValueOrDefault(id);
        }

        public static PlaybackDevice OpenDefaultPlayback(AudioSpec? spec = null) {
            return PlaybackDevice.OpenDefault(spec);
        }

        public static PlaybackDevice OpenDefaultPlayback(AudioSpec? spec, AudioStreamCallback callback, object? userdata = null) {
            return PlaybackDevice.OpenDefault(spec, callback, userdata);
        }

        public static RecordingDevice OpenDefaultRecording(AudioSpec? spec = null) {
            return RecordingDevice.OpenDefault(spec);
        }

        public static RecordingDevice OpenDefaultRecording(AudioSpec? spec, AudioStreamCallback callback, object? userdata = null) {
            return RecordingDevice.OpenDefault(spec, callback, userdata);
        }
    }
}
