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
        public static int _tickCounter;
        public WasapiOut wasapiOut;
        public List<MMDevice> devices;
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


        public void RenderTo(string filepath, int maxloops, bool individualStems)
        {
            //filepath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            wasapiOut.Stop();
            Thread.Sleep(100);
            _tickCounter = 0;
            Tracker.Playback.rendering = true;
            Tracker.Playback.stopRendering = false;
            Tracker.Playback.loops = 0;
            Tracker.Playback.maxLoops = maxloops + 1;
            System.Windows.Forms.ProgressBar progressBar = new System.Windows.Forms.ProgressBar();
            progressBar.Show();
            progressBar.Visible = true;

            progressBar.BringToFront();

            filepath = "C:\\Users\\elias\\Desktop\\waveoutput";
            WaveFileWriter[] waveFileWriters = new WaveFileWriter[24];

            if (individualStems)
            {
                for (int c = 0; c < channelManager.channels.Count; c++)
                {
                    waveFileWriters[c] = new WaveFileWriter(filepath + "_ch_" + (c + 1).ToString("D2") + ".wav", audioProvider.WaveFormat);
                }
            }
            else
            {
                waveFileWriters[0] = new WaveFileWriter(filepath + ".wav", audioProvider.WaveFormat);
            }
            Tracker.Playback.RenderInitialize();
            Tracker.Playback.RenderStart();
            Tracker.Playback.RenderStep();
            foreach (Channel c in channelManager.channels)
            {
                c.NextTick();
            }

            float delta = (tickSpeed / 60f) / sampleRate;
            progressBar.Minimum = 0;
            progressBar.Step = 1;
            progressBar.Value = 0;

            for (long n = 0; n < sampleRate * 600; n++)
            {



                float sampleL = 0, sampleR = 0;
                float l = 0, r = 0;
                for (int c = 0; c < channelManager.channels.Count; ++c)
                {
                    if (FrameEditor.channelToggles[c])
                    {
                        channelManager.channels[c].ProcessSingleSample(out l, out r, true, delta);
                        sampleL += l;
                        sampleR += r;
                    }
                    if (individualStems)
                    {
                        waveFileWriters[c].WriteSample(l);
                        waveFileWriters[c].WriteSample(r);
                    }

                }
                if (!individualStems)
                {
                    waveFileWriters[0].WriteSample(sampleL);
                    waveFileWriters[0].WriteSample(sampleR);
                }
                _tickCounter++;
                if (_tickCounter >= samplesPerTick)
                {
                    _tickCounter = 0;
                    if (!Tracker.Playback.RenderStep())
                        break;
                    foreach (Channel c in channelManager.channels)
                    {
                        c.NextTick();
                    }
                }
                progressBar.PerformStep();
            }
            progressBar.Hide();
            if (individualStems)
            {
                for (int c = 0; c < channelManager.channels.Count; c++)
                {
                    waveFileWriters[c].Flush();
                    waveFileWriters[c].Close();
                    waveFileWriters[c].Dispose();
                }
            }
            else
            {
                waveFileWriters[0].Flush();
                waveFileWriters[0].Close();
            }
            Tracker.Playback.rendering = false;
            Tracker.Playback.stopRendering = false;
            Tracker.Playback.loops = 0;
            Tracker.Playback.maxLoops = -1;
            ChannelManager.instance.Reset();
            Thread.Sleep(100);
            wasapiOut.Play();
        }


        public class Provider : WaveProvider32
        {
            public override int Read(float[] buffer, int offset, int sampleCount)
            {
                if (Tracker.Playback.rendering)
                    return sampleCount;
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

                    currentBuffer[0, currBufferPosition] = buffer[n + offset];
                    currentBuffer[1, currBufferPosition] = buffer[n + offset + 1];
                    currBufferPosition++;
                    if (currBufferPosition >= currentBuffer.Length / 2)
                        currBufferPosition = 0;

                    if (Game1.VisualizerMode)
                        if (_tickCounter % (samplesPerTick / Preferences.visualizerPianoSpeed) == 0)
                        {
                            Game1.visualization.Update();
                        }

                    _tickCounter++;
                    if (_tickCounter >= samplesPerTick)
                    {
                        _tickCounter = 0;
                        Tracker.Playback.Step();
                        if (Tracker.Playback.rendering && Tracker.Playback.stopRendering)
                        {
                            Tracker.Playback.isPlaying = false;
                            Tracker.Playback.rendering = false;
                            Tracker.Playback.stopRendering = false;
                            return 0;
                        }
                        foreach (Channel c in channelManager.channels)
                        {
                            c.NextTick();
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

