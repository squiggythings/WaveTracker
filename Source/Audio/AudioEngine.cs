using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        public static int SamplesPerTick {
            get {
                return SampleRate / TickSpeed;
            }
        }

        public static float[,] CurrentBuffer { get; private set; }
        public const int PREVIEW_BUFFER_LENGTH = 1000;
        private static int currBufferPosition;
        public static int RenderProcessedRows { get; set; }
        public static int RenderTotalRows { get; private set; }

        private static int _tickCounter;
        private static WasapiOut wasapiOut;
        public static bool IsRendering { get; private set; }
        public static bool CancelRenderFlag { get; set; }
        public static MMDeviceCollection OutputDevices { get; private set; }
        public static string[] OutputDeviceNames { get; private set; }

        private static AudioProvider audioProvider;

        public static void ResetTicks() {
            _tickCounter = 0;
        }

        public static void Initialize() {
            Dialogs.exportingDialog = new ExportingDialog();
            CurrentBuffer = new float[2, PREVIEW_BUFFER_LENGTH];
            audioProvider = new AudioProvider();
            SetSampleRate(App.Settings.Audio.SampleRate, App.Settings.Audio.Oversampling);
            GetAudioOutputDevices();
            int index = Array.IndexOf(OutputDeviceNames, App.Settings.Audio.OutputDevice);
            wasapiOut = index < 1 ? new WasapiOut() : new WasapiOut(OutputDevices[index], AudioClientShareMode.Shared, false, 0);
            wasapiOut.Init(audioProvider);
            wasapiOut.Play();
        }

        /// <summary>
        /// Sets the sample rate and oversampling factor. Updates antialiasing filters accordingly.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="oversampling"></param>
        public static void SetSampleRate(SampleRate rate, int oversampling) {
            SampleRate = SampleRateToInt(rate);
            TrueSampleRate = SampleRate * oversampling;
            audioProvider.SetWaveFormat(SampleRate, 2);
            foreach (Channel chan in ChannelManager.Channels) {
                chan.UpdateFilter();
            }
            ChannelManager.PreviewChannel.UpdateFilter();
        }

        /// <summary>
        /// Populates OutputDevices and OutputDeviceNames with all the connected audio devices. <br></br>
        /// This is an expensive operation, only call when needed.
        /// </summary>
        public static void GetAudioOutputDevices() {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            OutputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            OutputDeviceNames = new string[OutputDevices.Count];
            List<string> names = [];
            foreach (MMDevice device in OutputDevices) {
                names.Add(device.FriendlyName);
            }
            OutputDeviceNames = names.ToArray();
        }

        /// <summary>
        /// Stops the audio output connection
        /// </summary>
        public static void Stop() {
            if (File.Exists(Dialogs.exportingDialog.Path + ".temp")) {
                File.Delete(Dialogs.exportingDialog.Path + ".temp");
            }

            wasapiOut.Stop();
        }

        public static void Reset() {
            PianoInput.ClearAllNotes();
            wasapiOut.Stop();
            SetSampleRate(App.Settings.Audio.SampleRate, App.Settings.Audio.Oversampling);
            Thread.Sleep(1);
            int index = Array.IndexOf(OutputDeviceNames, App.Settings.Audio.OutputDevice);
            wasapiOut = index < 1 ? new WasapiOut() : new WasapiOut(OutputDevices[index], AudioClientShareMode.Shared, false, 0);
            wasapiOut.Init(audioProvider);
            wasapiOut.Play();
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

            bool b = await Task.Run(() => {
                return WriteToWaveFile(filepath + ".temp", audioProvider);
            });
            Debug.WriteLine("Exported!");
            if (b) {
                if (overwriting) {
                    File.Delete(filepath);
                }
                File.Copy(filepath + ".temp", filepath);
            }
            File.Delete(filepath + ".temp");
        }

        private static bool WriteToWaveFile(string path, IWaveProvider source) {
            wasapiOut.Stop();
            IsRendering = true;
            CancelRenderFlag = false;
            _tickCounter = 0;
            RenderProcessedRows = 0;

            ChannelManager.Reset();
            Playback.PlayFromBeginning();
            WaveFileWriter.CreateWaveFile(path, source);
            wasapiOut.Play();
            return !CancelRenderFlag;
        }

        public class AudioProvider : WaveProvider32 {
            public override int Read(float[] buffer, int offset, int sampleCount) {
                if (IsRendering) {
                    if (RenderProcessedRows >= RenderTotalRows || CancelRenderFlag) {
                        Playback.Stop();
                        IsRendering = false;
                        return 0;
                    }
                }

                int OVERSAMPLE = App.Settings.Audio.Oversampling;
                float delta = 1f / TrueSampleRate * (TickSpeed / 60f);

                for (int n = 0; n < sampleCount; n += 2) {
                    buffer[n + offset] = buffer[n + offset + 1] = 0;
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

                        ChannelManager.PreviewChannel.ProcessSingleSample(out l, out r, delta);
                        leftSum += l;
                        rightSum += r;
                        buffer[n + offset] = leftSum;
                        buffer[n + offset + 1] = rightSum;
                    }

                    buffer[n + offset] = Math.Clamp(buffer[n + offset] * (App.Settings.Audio.MasterVolume / 100f), -1, 1);
                    buffer[n + offset + 1] = Math.Clamp(buffer[n + offset + 1] * (App.Settings.Audio.MasterVolume / 100f), -1, 1);
                    if (!IsRendering) {
                        CurrentBuffer[0, currBufferPosition] = buffer[n + offset];
                        CurrentBuffer[1, currBufferPosition] = buffer[n + offset + 1];
                        currBufferPosition++;
                        if (currBufferPosition >= CurrentBuffer.Length / 2) {
                            currBufferPosition = 0;
                        }
                    }

                    if (App.VisualizerMode && !IsRendering) {
                        if (_tickCounter % (int)(SamplesPerTick / ((float)App.Settings.Visualizer.PianoSpeed / (App.Settings.Visualizer.DrawInHighResolution ? 1 : App.Settings.General.ScreenScale))) == 0) {
                            App.Visualizer.RecordChannelStates();
                            //App.visualization.Update();
                        }
                    }

                    _tickCounter++;
                    if (_tickCounter >= SamplesPerTick) {
                        _tickCounter = 0;
                        Playback.Tick();
                        foreach (Channel c in ChannelManager.Channels) {
                            c.NextTick();
                        }
                        ChannelManager.PreviewChannel.NextTick();
                    }
                    if (IsRendering) {
                        if (RenderProcessedRows >= RenderTotalRows || CancelRenderFlag) {
                            return n;
                        }
                    }
                }
                return sampleCount;
            }
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

