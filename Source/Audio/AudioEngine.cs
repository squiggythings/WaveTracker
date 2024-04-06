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
    public class AudioEngine {
        public const ResamplingMode RESAMPLING_MODE = ResamplingMode.None;
        public const int SAMPLE_RATE = 44100;
        public static int samplesPerTick => (SAMPLE_RATE / tickSpeed);
        public static int SamplesPerBuffer => 1000;

        public const bool quantizeAmplitude = false;
        static int currBufferPosition;
        public static AudioEngine instance;
        public static int totalRows;
        public static int processedRows;
        public static int _tickCounter;
        public static long samplesRead;
        public static WaveFileWriter waveFileWriter;
        public WasapiOut wasapiOut;
        public static bool rendering;
        public static bool cancelRender;
        public List<MMDevice> devices;
        Provider audioProvider;


        static int tickSpeed {
            get {
                if (App.CurrentModule == null)
                    return 60;
                else return App.CurrentModule.TickRate;
            }
        }

        public static float[,] currentBuffer;

        public static void ResetTicks() {
            _tickCounter = 0;
        }

        public void Initialize() {
            Dialogs.exportingDialog = new ExportingDialog();
            currentBuffer = new float[2, SamplesPerBuffer];
            audioProvider = new Provider();
            audioProvider.SetWaveFormat(AudioEngine.SAMPLE_RATE, 2); // 44.1khz stereo
            wasapiOut = new WasapiOut();
            wasapiOut.Init(audioProvider);
            wasapiOut.Play();
            instance = this;

        }

        public void Stop() {
            if (File.Exists(Dialogs.exportingDialog.Path + ".temp"))
                File.Delete(Dialogs.exportingDialog.Path + ".temp");
            wasapiOut.Stop();
        }

        public void Reset() {
            wasapiOut.Stop();
            Thread.Sleep(10);
            wasapiOut = new WasapiOut();
            wasapiOut.Init(audioProvider);
            wasapiOut.Play();
        }


        public async void RenderTo(string filepath, int maxloops, bool individualStems) {
            //Debug.WriteLine("Total Rows with " + maxloops + " loops: " + Song.currentSong.GetNumberOfRows(maxloops));
            totalRows = App.CurrentSong.GetNumberOfRows(maxloops);
            if (!SaveLoad.ChooseExportPath(out filepath)) {
                return;
            }
            Dialogs.exportingDialog.Open();
            Dialogs.exportingDialog.Path = filepath;
            Dialogs.exportingDialog.TotalRows = totalRows;
            bool overwriting = File.Exists(filepath);

            bool b = await Task.Run(() => WriteToWaveFile(filepath + ".temp", audioProvider));
            Debug.WriteLine("Exported!");
            if (b) {
                if (overwriting) {
                    File.Delete(filepath);
                }
                File.Copy(filepath + ".temp", filepath);
            }
            File.Delete(filepath + ".temp");
            //rendering = false;
            //waveFileWriter.Close();
            //Tracker.Playback.Stop();
            //rendering = false;

        }


        bool WriteToWaveFile(string path, IWaveProvider source) {
            wasapiOut.Stop();
            rendering = true;
            cancelRender = false;
            _tickCounter = 0;
            samplesRead = 0;
            processedRows = 0;

            ChannelManager.Reset();
            Tracker.Playback.PlayFromBeginning();
            WaveFileWriter.CreateWaveFile(path, source);
            wasapiOut.Play();
            return !cancelRender;
        }

        public class Provider : WaveProvider32 {
            public override int Read(float[] buffer, int offset, int sampleCount) {
                if (rendering) {
                    if (processedRows >= totalRows || cancelRender) {
                        Playback.Stop();
                        rendering = false;
                        return 0;
                    }
                }
                int sampleRate = WaveFormat.SampleRate;
                float delta = (1f / sampleRate) * (tickSpeed / 60f);

                for (int n = 0; n < sampleCount; n += 2) {
                    buffer[n + offset] = buffer[n + offset + 1] = 0;
                    float l = 0, r = 0;
                    for (int c = 0; c < ChannelManager.channels.Count; ++c) {
                        l = r = 0;
                        ChannelManager.channels[c].ProcessSingleSample(out l, out r, true, delta);
                        buffer[n + offset] += l;
                        buffer[n + offset + 1] += r;
                    }

                    l = r = 0;
                    ChannelManager.previewChannel.ProcessSingleSample(out l, out r, true, delta);
                    buffer[n + offset] += l;
                    buffer[n + offset + 1] += r;

                    if (!rendering) {
                        buffer[n + offset] = Math.Clamp(buffer[n + offset], -1, 1);
                        buffer[n + offset + 1] = Math.Clamp(buffer[n + offset + 1], -1, 1);
                        currentBuffer[0, currBufferPosition] = buffer[n + offset];
                        currentBuffer[1, currBufferPosition] = buffer[n + offset + 1];
                        currBufferPosition++;
                        if (currBufferPosition >= currentBuffer.Length / 2)
                            currBufferPosition = 0;
                    }
                    buffer[n + offset] *= Preferences.profile.master_volume;
                    buffer[n + offset + 1] *= Preferences.profile.master_volume;

                    if (App.VisualizerMode && !rendering)
                        if (_tickCounter % (samplesPerTick / Preferences.profile.visualizerPianoSpeed) == 0) {
                            App.visualization.Update();
                        }

                    _tickCounter++;
                    if (_tickCounter >= samplesPerTick) {
                        _tickCounter = 0;
                        Tracker.Playback.Tick();
                        foreach (Channel c in ChannelManager.channels) {
                            c.NextTick();
                        }
                        ChannelManager.previewChannel.NextTick();
                    }
                    if (rendering) {
                        if (processedRows >= totalRows || cancelRender) {
                            return n;
                        }
                    }
                }
                return sampleCount;
            }
        }


    }
    public enum ResamplingMode {
        None,
        Linear,
        Mix,
    }
}

