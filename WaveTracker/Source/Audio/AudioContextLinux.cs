using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WaveTracker.Audio.Interop;

namespace WaveTracker.Audio {
    [SupportedOSPlatform("Linux")]
    internal class AudioContextLinux : IAudioContext {
        private nint _pcm = 0;
        public Alsa.snd_pcm_format_t sample_format = Alsa.snd_pcm_format_t.FLOAT_LE;
        public Alsa.snd_pcm_access_t access = Alsa.snd_pcm_access_t.RW_INTERLEAVED;
        public uint channels = 2;
        public uint sample_rate = 48000;
        public bool soft_resample = true;

        /// required overall latency in us 
        public uint latency = 10_000;

        private bool isOpen;

        public bool Open(AudioDevice device) {
            if (isOpen)
                Close();

            nint pcm;
            unsafe {
                int status = Alsa.snd_pcm_open(&pcm, device.ID, Alsa.snd_pcm_stream_t.PLAYBACK, 0);
                if (status != 0)
                    throw new IOException($"Cannot open PCM audio device ({device.Name})");

                _pcm = pcm;
                isOpen = true;

                return Alsa.snd_pcm_set_params(pcm, sample_format, access, channels,
                                          sample_rate, soft_resample ? 1 : 0, latency) == 0;
            }
        }

        public List<AudioDevice> EnumerateAudioDevices() {
            List<AudioDevice> devices = [];

            unsafe {
                // Enumerate sound devices
                sbyte** deviceNameHints;
                int err = Alsa.snd_device_name_hint(-1, "pcm", &deviceNameHints);
                if (err != 0)
                    return devices;

                sbyte** hint = deviceNameHints;
                while (*hint != null) {
                    sbyte* namePtr = Alsa.snd_device_name_get_hint(*hint, "NAME");
                    sbyte* descPtr = Alsa.snd_device_name_get_hint(*hint, "DESC");
                    sbyte* ioidPtr = Alsa.snd_device_name_get_hint(*hint, "IOID");

                    string name = new string(namePtr);
                    string desc = new string(descPtr);
                    string ioid = new string(ioidPtr);

                    if (namePtr != null)
                        Marshal.FreeHGlobal((nint)namePtr);
                    if (descPtr != null)
                        Marshal.FreeHGlobal((nint)descPtr);
                    if (ioidPtr != null)
                        Marshal.FreeHGlobal((nint)ioidPtr);

                    if (name == "null")
                        desc = "Null (discard all samples)";

                    // IOID is either Input, Output or a null string. Null means both.
                    // Output-only devices seem to be duplicates.
                    // Devices that start with "sysdefault:" are also duplicating another listed device.
                    if (ioid == "" && !name.StartsWith("sysdefault:")) {
                        devices.Add(new AudioDevice {
                            ID = name,
                            Name = desc.Split('\n')[0],
                        });
                    }

                    hint++;
                }

                // Free hint buffer
                Alsa.snd_device_name_free_hint(deviceNameHints);
            }

            return devices;
        }

        public bool SetSampleRate(int sampleRate) {
            sample_rate = (uint)sampleRate;

            if (isOpen) {
                return Alsa.snd_pcm_set_params(_pcm, sample_format, access, channels,
                                              sample_rate, soft_resample ? 1 : 0, latency) == 0;
            }
            else {
                return true;
            }
        }

        public void SetLatency(int latency) {
            this.latency = (uint)latency;
        }

        public int AvailableFrames() {
            if (isOpen) {
                int frame_count = (int)Alsa.snd_pcm_avail(_pcm);
                if (frame_count < 0)
                    Alsa.snd_pcm_recover(_pcm, frame_count, 0);
                return frame_count;
            }
            else {
                return 0;
            }
        }

        public void Write(float[] buffer) {
            if (!isOpen)
                return;

            unsafe {
                fixed (float* bufferPtr = buffer) {
                    long frame_count = Alsa.snd_pcm_writei(_pcm, bufferPtr, (ulong)(buffer.Length / channels));
                    if (frame_count < 0)
                        Alsa.snd_pcm_recover(_pcm, (int)frame_count, 0);
                }
            }
        }

        public void Close() {
            if (!isOpen)
                return;

            Alsa.snd_pcm_close(_pcm);
            isOpen = false;
        }
    };
}
