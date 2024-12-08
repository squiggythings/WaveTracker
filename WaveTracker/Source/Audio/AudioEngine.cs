using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WaveTracker.Audio.Native;
using WaveTracker.Tracker;
using WaveTracker.UI;

namespace WaveTracker.Audio {
    public static class AudioEngine {
        public static int SampleRate { get; private set; }
        public static int TrueSampleRate { get; private set; }

        private static int TickSpeed {
            get {
                return App.CurrentModule == null ? 60 : App.CurrentModule.TickRate;
            }
        }
        public static double SamplesPerTick {
            get {
                return SampleRate / (double)TickSpeed;
            }
        }

        public static float[,] CurrentBuffer { get; private set; }
        public const int PREVIEW_BUFFER_LENGTH = 1000;
        private static int currBufferPosition;
        public static int RenderProcessedRows { get; set; }
        public static int RenderTotalRows { get; private set; }

        private static double _tickCounter;
        public static bool IsRendering { get; private set; }
        public static bool CancelRenderFlag { get; set; }
        public static AudioDevice CurrentOutputDevice { get; private set; }
        public static List<AudioDevice> OutputDevices { get; private set; }
        public static string[] OutputDeviceNames { get; private set; }

        private static IWaveStream previewStream = null;
        public static IWaveStream PreviewStream {
            get => previewStream;
            set {
                if (value != null)
                    SetSampleRate(value.SampleRate, App.Settings.Audio.Oversampling);
                else
                    SetSampleRate(SampleRateToInt(App.Settings.Audio.SampleRate), App.Settings.Audio.Oversampling);

                previewStream = value;
            }
        }

        private static IAudioContext audioCtx;
        private static Thread audioOutThread;
        private static bool doPauseAudioThread = false;
        private static bool doStopAudioThread = false;

        public static void ResetTicks() {
            _tickCounter = 0;
        }

        public static void Initialize() {
            Dialogs.exportingDialog = new ExportingDialog();
            CurrentBuffer = new float[2, PREVIEW_BUFFER_LENGTH];

            if (OperatingSystem.IsWindows()) {
                audioCtx = new AudioContextWindows();
            }
            else if (OperatingSystem.IsLinux()) {
                audioCtx = new AudioContextLinux();
            }
            else if (OperatingSystem.IsMacOS()) {
                throw new NotImplementedException();
            }
            else
                throw new PlatformNotSupportedException("This platform has no audio context");

            Reset();
        }

        /// <summary>
        /// Sets the sample rate and oversampling factor. Updates antialiasing filters accordingly.
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="oversampling"></param>
        public static void SetSampleRate(int sampleRate, int oversampling) {
            SampleRate = sampleRate;
            TrueSampleRate = SampleRate * oversampling;
            audioCtx.SetSampleRate(SampleRate);
            foreach (Channel chan in ChannelManager.Channels) {
                chan.UpdateFilter();
            }
            ChannelManager.PreviewChannel.UpdateFilter();
        }

        /// <summary>
        /// Populates OutputDevices and OutputDeviceNames with all the connected
        /// audio devices, and updates the CurrentOutputDevice.
        /// <br></br>This is an expensive operation, only call when needed.
        /// </summary>
        public static void UpdateAudioOutputDevices() {
            OutputDevices = audioCtx.EnumerateAudioDevices();
            OutputDevices.Insert(0, AudioDevice.DefaultOutputDevice);

            OutputDeviceNames = new string[OutputDevices.Count];
            for (int i = 0; i < OutputDeviceNames.Length; i++)
                OutputDeviceNames[i] = OutputDevices[i].Name;

            int index = Array.IndexOf(OutputDeviceNames, App.Settings.Audio.OutputDevice);
            CurrentOutputDevice = index < 0 ? AudioDevice.DefaultOutputDevice : OutputDevices[index];
        }

        /// <summary>
        /// Resume outputting sound to the audio thread
        /// </summary>
        private static void resumeAudioThread() {
            doPauseAudioThread = false;
        }

        /// <summary>
        /// Pause outputting sound to the audio thread
        /// </summary>
        private static void pauseAudioThread() {
            doPauseAudioThread = true;
        }

        /// <summary>
        /// Open the audio thread and start outputting sound to it
        /// </summary>
        private static void startAudioThread() {
            doStopAudioThread = false;
            audioOutThread = new Thread(audioOutLoop);
            audioOutThread.Name = "Audio Output";
            audioOutThread.Start();
        }

        /// <summary>
        /// Stop outputting sound to the audio thread and close it
        /// </summary>
        private static void stopAudioThread() {
            doStopAudioThread = true;
            audioOutThread?.Join();
        }

        /// <summary>
        /// Stops the audio output connection
        /// </summary>
        public static void Stop() {
            if (File.Exists(Dialogs.exportingDialog.Path + ".temp")) {
                File.Delete(Dialogs.exportingDialog.Path + ".temp");
            }

            stopAudioThread();
        }

        public static void Reset() {
            PianoInput.ClearAllNotes();
            stopAudioThread();

            SetSampleRate(SampleRateToInt(App.Settings.Audio.SampleRate), App.Settings.Audio.Oversampling);
            audioCtx.SetLatency(App.Settings.Audio.DesiredLatency * 1000);
            Thread.Sleep(1);

            UpdateAudioOutputDevices();

            startAudioThread();
        }

        public static async void RenderTo(string filepath, int maxloops) {
            RenderTotalRows = App.CurrentSong.GetNumberOfRows(maxloops);
            if (!SaveLoad.ChooseExportPath(out filepath)) {
                return;
            }
            Dialogs.exportingDialog.Open();
            Dialogs.exportingDialog.Path = filepath;
            Dialogs.exportingDialog.TotalRows = RenderTotalRows;
            bool overwriting = File.Exists(filepath);

            bool b = await Task.Run(() => WriteToWaveFile(filepath + ".temp"));
            Debug.WriteLine("Exported!");
            if (b) {
                if (overwriting) {
                    File.Delete(filepath);
                }
                File.Copy(filepath + ".temp", filepath);
            }
            File.Delete(filepath + ".temp");
        }

        private static bool WriteToWaveFile(string path) {
            pauseAudioThread();

            IsRendering = true;
            CancelRenderFlag = false;
            ResetTicks();
            RenderProcessedRows = 0;

            ChannelManager.Reset();
            Playback.PlayFromBeginning();

            List<float> wavSamples = new List<float>();

            float[] buffer = new float[4096];
            while (true) {
                int count = readSamples(buffer, 0, buffer.Length);
                if (count == 0)
                    break;

                wavSamples.AddRange(buffer[..count]);
            }

            Wav wav = new Wav(Wav.FloatToPCM16(wavSamples.ToArray()), 2, (uint)SampleRate);

            using (var file = File.OpenWrite(path))
                wav.Write(file);

            resumeAudioThread();
            return !CancelRenderFlag;
        }

        private static void audioOutLoop() {
            float[] buffer = new float[256];
            float[] previewBuffer = new float[256];

            try {
                audioCtx.Open(CurrentOutputDevice);

                while (!doStopAudioThread) {
                    if (doPauseAudioThread) {
                        Thread.Sleep(10);
                    }
                    else {
                        int samplesCount;

                        if (PreviewStream != null) {
                            float previewVolume = 0.75f * App.Settings.Audio.MasterVolume / 100f;

                            if (PreviewStream.NumChannels == 1) {
                                int previewSamplesCount = PreviewStream.ReadSamples(previewBuffer, previewBuffer.Length / 2);

                                for (int i = 0; i < previewSamplesCount; i++) {
                                    buffer[2 * i] = previewVolume * previewBuffer[i];
                                    buffer[2 * i + 1] = previewVolume * previewBuffer[i];
                                }

                                samplesCount = previewSamplesCount * 2;
                            }
                            else {
                                samplesCount = PreviewStream.ReadSamples(previewBuffer, previewBuffer.Length);
                                for (int i = 0; i < samplesCount; i++)
                                    buffer[i] = previewVolume * previewBuffer[i];
                            }
                        }
                        else {
                            samplesCount = readSamples(buffer, 0, buffer.Length);
                            if (samplesCount == 0) {
                                PreviewStream = null;
                            }
                        }

                        audioCtx.Write(buffer[..samplesCount]);
                    }
                }

                audioCtx.Close();
            } catch (Exception e) {
                Debug.WriteLine("Audio context error: " + e);

                // make sure audio is still read even when audio context is not present
                while (!doStopAudioThread) {
                    if (doPauseAudioThread) {
                        Thread.Sleep(10);
                    }
                    else {
                        readSamples(buffer, 0, buffer.Length);
                        Thread.Sleep(10);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessSingleSample(out float left, out float right) {
            int OVERSAMPLE = App.Settings.Audio.Oversampling;
            float delta = 1f / TrueSampleRate * (TickSpeed / 60f);

            left = right = 0;
            float leftSum;
            float rightSum;
            for (int j = 0; j < OVERSAMPLE; ++j) {
                float l;
                float r;
                leftSum = 0;
                rightSum = 0;
                for (int c = 0; c < ChannelManager.Channels.Count; ++c) {
                    ChannelManager.Channels[c].ProcessSingleSample(out l, out r, delta);
                    leftSum += l;
                    rightSum += r;
                }

                if (!IsRendering) {
                    ChannelManager.PreviewChannel.ProcessSingleSample(out l, out r, delta);
                    leftSum += l;
                    rightSum += r;
                }
                left = leftSum;
                right = rightSum;
            }

            if (Dialogs.currentSampleModifyDialog != null && Dialogs.currentSampleModifyDialog.WindowIsOpen) {
                Dialogs.currentSampleModifyDialog.GetPreviewSample(out leftSum, out rightSum);
                left += leftSum;
                right += rightSum;
            }

            left = Math.Clamp(left * (App.Settings.Audio.MasterVolume / 100f), -1, 1);
            right = Math.Clamp(right * (App.Settings.Audio.MasterVolume / 100f), -1, 1);
            if (!IsRendering) {
                CurrentBuffer[0, currBufferPosition] = left;
                CurrentBuffer[1, currBufferPosition] = right;
                currBufferPosition++;
                if (currBufferPosition >= CurrentBuffer.Length / 2) {
                    currBufferPosition = 0;
                }
            }

            if (App.VisualizerMode && !IsRendering) {
                if ((int)_tickCounter % (int)(SamplesPerTick / ((float)App.Settings.Visualizer.PianoSpeed / (App.Settings.Visualizer.DrawInHighResolution ? 1 : App.Settings.General.ScreenScale))) == 0) {
                    App.Visualizer.RecordChannelStates();
                }
            }

            _tickCounter++;
            if (_tickCounter > SamplesPerTick) {
                _tickCounter -= SamplesPerTick;
                Playback.Tick();
                foreach (Channel c in ChannelManager.Channels) {
                    c.NextTick();
                }
                ChannelManager.PreviewChannel.NextTick();
            }

        }

        private static int readSamples(float[] buffer, int offset, int sampleCount) {
            if (IsRendering) {
                if (RenderProcessedRows >= RenderTotalRows || CancelRenderFlag) {
                    Playback.Stop();
                    IsRendering = false;
                    return 0;
                }
            }
            for (int n = 0; n < sampleCount; n += 2) {
                ProcessSingleSample(out buffer[n + offset], out buffer[n + offset + 1]);

                if (IsRendering) {
                    if (RenderProcessedRows >= RenderTotalRows || CancelRenderFlag) {
                        return n;
                    }
                }
            }

            return sampleCount;
        }

        /// <summary>
        /// Converts a sample rate enum into its actual numerical sample rate in Hz.
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static int SampleRateToInt(SampleRate rate) {
            return rate switch {
                Audio.SampleRate._11025 => 11025,
                Audio.SampleRate._22050 => 22050,
                Audio.SampleRate._44100 => 44100,
                Audio.SampleRate._48000 => 48000,
                Audio.SampleRate._96000 => 96000,
                _ => 0,
            };
        }
    }

    [ProtoContract]
    public enum ResamplingMode {
        None,
        Linear,
        Mix,
    }
    public enum SampleRate {
        _11025,
        _22050,
        _44100,
        _48000,
        _96000,
    }
}

