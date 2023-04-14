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



namespace WaveTracker.Audio
{
    public class AudioEngine
    {
        public const ResamplingModes RESAMPLING_MODE = ResamplingModes.None;
        public const int sampleRate = 44100;
        public static int samplesPerTick => (sampleRate / tickSpeed);
        public static int SamplesPerBuffer => 1200;
        static int currBufferPosition;
        public static AudioEngine instance;
        public static int _tickCounter;
        WasapiOut wasapiOut;

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
            var sineWaveProvider = new Provider();
            sineWaveProvider.SetWaveFormat(AudioEngine.sampleRate, 2); // 16kHz mono
            wasapiOut = new WasapiOut();
            wasapiOut.Init(sineWaveProvider);
            wasapiOut.Play();
            instance = this;

        }

        public void Stop()
        {
            wasapiOut.Stop();
        }


        public class Provider : WaveProvider32
        {
            public override int Read(float[] buffer, int offset, int sampleCount)
            {
                int sampleRate = WaveFormat.SampleRate;
                float delta = (1f / sampleRate) * (tickSpeed / 60f);

                for (int n = 0; n < sampleCount; n += 2)
                {
                    buffer[n + offset] = buffer[n + offset + 1] = 0;

                    for (int c = 0; c < channelManager.channels.Count; ++c)
                    {
                        float l, r;

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

