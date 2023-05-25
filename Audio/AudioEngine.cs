using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading;
using NAudio.Wave;
using NAudio.Dsp;
using NAudio.Wasapi;
using NAudio.Wave.SampleProviders;
using NAudio.CoreAudioApi;
using System.IO;
using SharpDX.MediaFoundation.DirectX;

namespace WaveTracker.Audio
{
    public class AudioEngine
    {
        public const ResamplingModes RESAMPLING_MODE = ResamplingModes.None;
        public const int sampleRate = 44100;
        public static int samplesPerTick => (sampleRate / tickSpeed);
        public static int SamplesPerBuffer => 1000;
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
        public Rendering.ExportingDialog exportingDialog;
        Provider audioProvider;

        public static int tickSpeed
        {
            get
            {
                if (Game1.currentSong == null)
                    return 60;
                else return Game1.currentSong.tickRate;
            }
        }

        public static bool quantizeAmplitude
        {
            get
            {
                if (Game1.currentSong == null)
                    return true;
                else return Game1.currentSong.quantizeChannelAmplitude;
            }
        }

        public static float[,] currentBuffer;

        public static ChannelManager channelManager;

        public void Initialize(ChannelManager channelMan)
        {
            exportingDialog = new Rendering.ExportingDialog();
            currentBuffer = new float[2, SamplesPerBuffer];
            channelManager = channelMan;
            audioProvider = new Provider();
            audioProvider.SetWaveFormat(AudioEngine.sampleRate, 2); // 44.1khz stereo
            wasapiOut = new WasapiOut();
            wasapiOut.Init(audioProvider);
            wasapiOut.Play();
            instance = this;

        }

        public void Stop()
        {
            if (File.Exists(exportingDialog.Path + ".temp"))
                File.Delete(exportingDialog.Path + ".temp");
            wasapiOut.Stop();
        }

        public void Reset()
        {
            wasapiOut.Stop();
            Thread.Sleep(10);
            wasapiOut = new WasapiOut();
            wasapiOut.Init(audioProvider);
            wasapiOut.Play();
        }


        public async void RenderTo(string filepath, int maxloops, bool individualStems)
        {
            //Debug.WriteLine("Total Rows with " + maxloops + " loops: " + Game1.currentSong.GetNumberOfRows(maxloops));
            totalRows = Game1.currentSong.GetNumberOfRows(maxloops);
            if (!SaveLoad.ChooseExportPath(out filepath))
            {
                return;
            }
            exportingDialog.Open();
            exportingDialog.Path = filepath;
            exportingDialog.TotalRows = totalRows;
            bool overwriting = File.Exists(filepath);

            bool b = await Task.Run(() => WriteToWaveFile(filepath + ".temp", audioProvider));
            Debug.WriteLine("Exported!");
            if (b)
            {
                if (overwriting)
                {
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


        bool WriteToWaveFile(string path, IWaveProvider source)
        {
            wasapiOut.Stop();
            rendering = true;
            cancelRender = false;
            _tickCounter = 0;
            samplesRead = 0;
            processedRows = 0;

            ChannelManager.instance.Reset();
            Tracker.Playback.PlayFromBeginning();
            WaveFileWriter.CreateWaveFile(path, source);
            wasapiOut.Play();
            return !cancelRender;
        }

        public class Provider : WaveProvider32
        {
            public override int Read(float[] buffer, int offset, int sampleCount)
            {
                if (rendering)
                {
                    if (processedRows > totalRows || cancelRender)
                    {
                        Tracker.Playback.Stop();
                        rendering = false;
                        return 0;
                    }
                }
                int sampleRate = WaveFormat.SampleRate;
                float delta = (1f / sampleRate) * (tickSpeed / 60f);

                for (int n = 0; n < sampleCount; n += 2)
                {
                    buffer[n + offset] = buffer[n + offset + 1] = 0;

                    for (int c = 0; c < channelManager.channels.Count; ++c)
                    {
                        float l = 0, r = 0;
                        channelManager.channels[c].ProcessSingleSample(out l, out r, true, delta);
                        buffer[n + offset] += l;
                        buffer[n + offset + 1] += r;
                    }

                    if (!rendering)
                    {
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

                    if (Game1.VisualizerMode && !rendering)
                        if (_tickCounter % (samplesPerTick / Preferences.profile.visualizerPianoSpeed) == 0)
                        {
                            Game1.visualization.Update();
                        }

                    _tickCounter++;
                    if (_tickCounter >= samplesPerTick)
                    {
                        _tickCounter = 0;
                        Tracker.Playback.Tick();
                        foreach (Channel c in channelManager.channels)
                        {
                            c.NextTick();
                        }
                    }
                    if (rendering)
                    {
                        if (processedRows > totalRows || cancelRender)
                        {
                            return n;
                        }
                    }
                }
                return sampleCount;
            }
        }


    }
    public enum ResamplingModes
    {
        None,
        Linear,
        Mix,
    }
}

