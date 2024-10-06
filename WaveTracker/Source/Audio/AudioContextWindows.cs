using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using WaveTracker.Audio.Interop.Windows;

namespace WaveTracker.Audio {
    [SupportedOSPlatform("Windows")]
    internal class AudioContextWindows : IAudioContext {
        private nint _hWaveOut;
        private bool isOpen;

        private Winmm.WaveHeader[] waveHeaders = new Winmm.WaveHeader[2];
        private GCHandle waveHeadersHandle;
        private Queue<float> queueBuffer;
        private short[][] buffers = new short[2][];
        private GCHandle[] bufferHandles = new GCHandle[2];

        public int SampleRate { get; private set; } = 48000;
        public int Latency { get; private set; } = 15000;

        private int bufferLength;

        public AudioContextWindows() {
            waveHeadersHandle = GCHandle.Alloc(waveHeaders, GCHandleType.Pinned);
            reset();
        }

        public bool Open(AudioDevice device) {
            short nChannels = 2;
            short wBitsPerSample = 16;
            short nBlockAlign = (short)(nChannels * wBitsPerSample / 8);
            int nAvgBytesPerSec = SampleRate * nBlockAlign;

            Winmm.WaveFormatEx waveFormat = new Winmm.WaveFormatEx {
                wFormatTag = Winmm.WaveFormatEncoding.Pcm,
                nChannels = nChannels,
                nSamplesPerSec = SampleRate,
                nAvgBytesPerSec = nAvgBytesPerSec,
                nBlockAlign = nBlockAlign,
                wBitsPerSample = wBitsPerSample,
                cbSize = 0,
            };

            MmException.Try(
                Winmm.waveOutOpen(out nint hWaveOut, device.DeviceNumber, ref waveFormat, null, 0,
                    Winmm.WaveInOutOpenFlags.CallbackNull), "waveOutOpen");

            for (int i = 0; i < 2; i++) {
                waveHeaders[i].dataBuffer = bufferHandles[i].AddrOfPinnedObject();
                waveHeaders[i].bufferLength = buffers[i].Length * sizeof(short);
                MmException.Try(
                    Winmm.waveOutPrepareHeader(hWaveOut, ref waveHeaders[i], Marshal.SizeOf(typeof(Winmm.WaveHeader))),
                    "waveOutPrepareHeader");
                MmException.Try(
                    Winmm.waveOutWrite(hWaveOut, ref waveHeaders[i], Marshal.SizeOf(typeof(Winmm.WaveHeader))),
                    "waveOutWrite");
            }

            _hWaveOut = hWaveOut;
            isOpen = true;

            return true;
        }

        public List<AudioDevice> EnumerateAudioDevices() {
            List<AudioDevice> devices = new List<AudioDevice>();

            uint deviceCount = Winmm.waveOutGetNumDevs();

            for (int deviceNumber = 0; deviceNumber < deviceCount; deviceNumber++) {
                unsafe {
                    Winmm.WaveOutCaps caps = new Winmm.WaveOutCaps();
                    MmException.Try(
                        Winmm.waveOutGetDevCaps((uint)deviceNumber, ref caps,
                            (uint)Marshal.SizeOf(typeof(Winmm.WaveOutCaps))), "waveOutGetDevCaps");

                    devices.Add(new AudioDevice {
                        Name = new string(caps.szPname),
                        DeviceNumber = (uint)deviceNumber,
                    });
                }
            }

            return devices;
        }

        public bool SetSampleRate(int sampleRate) {
            SampleRate = sampleRate;
            reset();
            return true;
        }

        public void SetLatency(int latency) {
            Latency = latency;
            reset();
        }

        private void reset() {
            bufferLength = Math.Max(256, (int)((SampleRate / 1000.0) * (Latency / 1000.0)));
            queueBuffer = new Queue<float>(bufferLength);

            for (int i = 0; i < bufferHandles.Length; i++) {
                if (bufferHandles[i].IsAllocated)
                    bufferHandles[i].Free();
                buffers[i] = new short[bufferLength];
                bufferHandles[i] = GCHandle.Alloc(buffers[i], GCHandleType.Pinned);
            }
        }

        public int AvailableFrames() {
            for (int i = 0; i < 2; i++) {
                if (waveHeaders[i].flags.HasFlag(Winmm.WaveHeaderFlags.Done))
                    return bufferLength;
            }

            return 0;
        }

        public void Write(float[] buffer) {
            if (bufferLength == 0)
                return;

            foreach (var sample in buffer)
                queueBuffer.Enqueue(sample);

            while (queueBuffer.Count >= bufferLength) {
                for (int bufferIdx = 0; bufferIdx < 2; bufferIdx++) {
                    if (waveHeaders[bufferIdx].flags.HasFlag(Winmm.WaveHeaderFlags.Done)) {
                        for (int i = 0; i < buffers[bufferIdx].Length; i++)
                            buffers[bufferIdx][i] = (short)(queueBuffer.Dequeue() * 32767);

                        Winmm.waveOutWrite(_hWaveOut, ref waveHeaders[bufferIdx], Marshal.SizeOf(typeof(Winmm.WaveHeader)));
                        Thread.Sleep(1);
                        break;
                    }
                }
            }
        }

        public void Close() {
            if (isOpen) {
                for (int bufferIdx = 0; bufferIdx < waveHeaders.Length; bufferIdx++) {
                    waveHeaders[bufferIdx].dataBuffer = 0;
                    waveHeaders[bufferIdx].bufferLength = 0;

                    while (true) {
                        MmResult result = Winmm.waveOutUnprepareHeader(_hWaveOut, ref waveHeaders[bufferIdx],
                            Marshal.SizeOf(typeof(Winmm.WaveHeader)));
                        if (result == MmResult.NoError)
                            break;
                        else
                            Thread.Sleep(1);
                    }
                }

                Winmm.waveOutClose(_hWaveOut);
                isOpen = false;
            }
        }

        ~AudioContextWindows() {
            waveHeadersHandle.Free();
            for (int i = 0; i < bufferHandles.Length; i++) {
                bufferHandles[i].Free();
            }
        }
    };
}