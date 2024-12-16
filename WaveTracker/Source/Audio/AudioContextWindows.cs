using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using WaveTracker.Audio.Interop.Windows;

namespace WaveTracker.Audio {
    [SupportedOSPlatform("Windows")]
    internal class AudioContextWindows : IAudioContext {
        private nint _hWaveOut;
        private bool isOpen;
        private AudioDevice currentDevice = null;

        private const int N_CHANNELS = 2;

        // Having only one buffer would work, but 2 buffers prevents temporary clipping.
        private const int N_BUFFERS = 2;
        private int currBufferIdx = 0;

        private Winmm.WaveHeader[] waveHeaders = new Winmm.WaveHeader[N_BUFFERS];
        private GCHandle waveHeadersHandle;
        private Queue<float> queueBuffer;
        private short[][] buffers = new short[N_BUFFERS][];
        private GCHandle[] bufferHandles = new GCHandle[N_BUFFERS];

        private Stopwatch watch = new Stopwatch();

        public int SampleRate { get; private set; } = 48000;
        public int Latency { get; private set; } = 100_000;

        private int bufferLength;

        public AudioContextWindows() {
            waveHeadersHandle = GCHandle.Alloc(waveHeaders, GCHandleType.Pinned);
            reset();
        }

        /// <summary>
        /// Open this audio device and set it as current.
        /// </summary>
        /// <returns>Whether it successfully opened.</returns>
        public bool Open(AudioDevice device) {
            currentDevice = device;
            return open();
        }

        /// <summary>
        /// Open the current audio device.
        /// Does nothing if there is no current device.
        /// </summary>
        /// <returns>Whether it successfully opened.</returns>
        private bool open() {
            if (currentDevice == null)
                return false;

            short wBitsPerSample = 16;
            short nBlockAlign = (short)(N_CHANNELS * wBitsPerSample / 8);
            int nAvgBytesPerSec = SampleRate * nBlockAlign;

            Winmm.WaveFormatEx waveFormat = new Winmm.WaveFormatEx {
                wFormatTag = Winmm.WaveFormatEncoding.Pcm,
                nChannels = N_CHANNELS,
                nSamplesPerSec = SampleRate,
                nAvgBytesPerSec = nAvgBytesPerSec,
                nBlockAlign = nBlockAlign,
                wBitsPerSample = wBitsPerSample,
                cbSize = 0,
            };

            MmException.Try(Winmm.waveOutOpen(out nint hWaveOut, currentDevice.DeviceNumber, ref waveFormat, null, 0, Winmm.WaveInOutOpenFlags.CallbackNull), "waveOutOpen");

            for (int i = 0; i < N_BUFFERS; i++) {
                waveHeaders[i].dataBuffer = bufferHandles[i].AddrOfPinnedObject();
                waveHeaders[i].bufferLength = buffers[i].Length * sizeof(short);
                MmException.Try(Winmm.waveOutPrepareHeader(hWaveOut, ref waveHeaders[i], Marshal.SizeOf(typeof(Winmm.WaveHeader))), "waveOutPrepareHeader");
                MmException.Try(Winmm.waveOutWrite(hWaveOut, ref waveHeaders[i], Marshal.SizeOf(typeof(Winmm.WaveHeader))), "waveOutWrite");
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
                    MmException.Try(Winmm.waveOutGetDevCaps((uint)deviceNumber, ref caps, (uint)Marshal.SizeOf(typeof(Winmm.WaveOutCaps))), "waveOutGetDevCaps");

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

        /// <summary>
        /// Close, reset, and reopen the current device.
        /// Does nothing if there is no current device.
        /// It's easier to close and reopen the device than to manage its individual resources when setting config like sample rate and latency.
        /// </summary>
        private void reset() {
            close();

            bufferLength = Math.Max(256, (int)((SampleRate / 1000.0) * (Latency / 1000.0)));
            queueBuffer = new Queue<float>(bufferLength);

            for (int i = 0; i < N_BUFFERS; i++) {
                if (bufferHandles[i].IsAllocated)
                    bufferHandles[i].Free();
                buffers[i] = new short[bufferLength];
                bufferHandles[i] = GCHandle.Alloc(buffers[i], GCHandleType.Pinned);
            }

            open();
        }

        public void Write(float[] buffer) {
            if (bufferLength == 0)
                return;

            foreach (var sample in buffer)
                queueBuffer.Enqueue(sample);

            bool hasWritten = false;

            while (queueBuffer.Count >= bufferLength) {
                if (waveHeaders[currBufferIdx].flags.HasFlag(Winmm.WaveHeaderFlags.Done)) {
                    for (int i = 0; i < buffers[currBufferIdx].Length; i++)
                        buffers[currBufferIdx][i] = (short)(queueBuffer.Dequeue() * 32767);

                    Winmm.waveOutWrite(_hWaveOut, ref waveHeaders[currBufferIdx], Marshal.SizeOf(typeof(Winmm.WaveHeader)));
                    currBufferIdx = (currBufferIdx + 1) % N_BUFFERS;
                    hasWritten = true;
                    break;
                }
            }

            // We have to guarantee that the time it spends on an audio frame is not instantaneous,
            // otherwise it will create staggering later while waiting for one buffer to finish.
            double currAudioFrameTimeMs = 1000.0 * buffer.Length / (SampleRate * N_CHANNELS);
            TimeSpan timeToWait = TimeSpan.FromMilliseconds(currAudioFrameTimeMs) - watch.Elapsed;

            if (hasWritten)
                timeToWait -= TimeSpan.FromMilliseconds(5);

            if (timeToWait > TimeSpan.Zero)
                Thread.Sleep(timeToWait);

            watch.Restart();
        }

        /// <summary>
        /// Close the current device and unset it as current.
        /// </summary>
        public void Close() {
            close();
            currentDevice = null;
        }

        /// <summary>
        /// Close the current device. It is still left set as current so that we can reopen it privately in other methods like reset().
        /// Does nothing if there is no current device.
        /// </summary>
        private void close() {
            if (currentDevice == null)
                return;

            if (isOpen) {
                for (int bufferIdx = 0; bufferIdx < N_BUFFERS; bufferIdx++) {
                    waveHeaders[bufferIdx].dataBuffer = 0;
                    waveHeaders[bufferIdx].bufferLength = 0;

                    while (true) {
                        MmResult result = Winmm.waveOutUnprepareHeader(_hWaveOut, ref waveHeaders[bufferIdx], Marshal.SizeOf(typeof(Winmm.WaveHeader)));
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
            for (int i = 0; i < N_BUFFERS; i++) {
                bufferHandles[i].Free();
            }
        }
    };
}