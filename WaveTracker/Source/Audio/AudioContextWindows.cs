using System.Collections.Generic;
using System.Runtime.Versioning;
using WaveTracker.Audio.Interop.Windows;

namespace WaveTracker.Audio {
    [SupportedOSPlatform("Windows")]
    internal class AudioContextWindows : IAudioContext {
        private nint _hWaveOut;
        private bool isOpen;

        public int SampleRate { get; private set; } = 48000;
        public int Latency { get; private set; } = 15000;

        public bool Open(AudioDevice device) {
            Winmm.WaveFormatEx waveFormat = new Winmm.WaveFormatEx {
                wFormatTag = Winmm.WaveFormatEncoding.IeeeFloat,
                nChannels = 2,
                nSamplesPerSec = SampleRate,
                nAvgBytesPerSec = SampleRate * 2 * 4,
                nBlockAlign = 1,
                wBitsPerSample = 32,
                cbSize = 0,
            };

            MmException.Try(Winmm.waveOutOpen(out nint hWaveOut, device.DeviceNumber, ref waveFormat, null, 0, Winmm.WaveInOutOpenFlags.CallbackNull), "waveOutOpen");

            // ...

            _hWaveOut = hWaveOut;
            isOpen = true;

            return true;
        }

        public List<AudioDevice> EnumerateAudioDevices() {
            List<AudioDevice> devices = new List<AudioDevice>();

            uint deviceCount = Winmm.waveOutGetNumDevs();

            for (uint deviceNumber = 0; deviceNumber < deviceCount; deviceNumber++) {
                unsafe {
                    Winmm.WaveOutCaps caps = new Winmm.WaveOutCaps();
                    Winmm.waveOutGetDevCaps(deviceNumber, ref caps, (uint)sizeof(Winmm.WaveOutCaps));

                    devices.Add(new AudioDevice {
                        Name = new string(caps.szPname),
                        DeviceNumber = deviceNumber,
                    });
                }
            }

            return new List<AudioDevice>();
        }

        public bool SetSampleRate(int sampleRate) {
            SampleRate = sampleRate;
            return true;
        }

        public void SetLatency(int latency) {
            Latency = latency;
        }

        public bool IsAvailable() {
            // TODO
            return false;
        }

        public void Write(float[] buffer) {
            // TODO
        }

        public void Close() {
            if (isOpen) {
                Winmm.waveOutClose(_hWaveOut);
                isOpen = false;
            }
        }
    };
}
