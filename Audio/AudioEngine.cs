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


namespace WaveTracker.Audio
{
    public class AudioEngine
    {
        public const ResamplingModes RESAMPLING_MODE = ResamplingModes.NoInterpolation;
        public const int bufferLengthMilliseconds = 17;
        public const int sampleRate = 44100;
        public static int samplesPerTick => (sampleRate / tickSpeed);
        public static AudioEngine instance;
        public int _tickCounter;

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

        private float[,] _workingBuffer;
        private byte[] _xnaBuffer;

        public float[,] currentBuffer => _workingBuffer;

        public DynamicSoundEffectInstance _instance;

        public ChannelManager channelManager;

        public void Initialize(ChannelManager channelMan)
        {
            instance = this;
            _instance = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Stereo);

            _instance.Play();
            channelManager = channelMan;
        }

        public int SamplesPerBuffer => (int)(bufferLengthMilliseconds / 1000f * sampleRate);

        public void Update()
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (_instance.PendingBufferCount < 4)
                SubmitBuffer();
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
        }

        private static void ConvertBuffer(float[,] from, byte[] to)
        {
            const int bytesPerSample = 2;
            int channels = from.GetLength(0);
            int samplesPerBuffer = from.GetLength(1);

            // Make sure the buffer sizes are correct
            System.Diagnostics.Debug.Assert(to.Length == samplesPerBuffer * channels * bytesPerSample, "Buffer sizes are mismatched.");

            for (int i = 0; i < samplesPerBuffer; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    // First clamp the value to the [-1.0..1.0] range
                    float floatSample = MathHelper.Clamp(from[c, i], -1.0f, 1.0f);

                    // Convert it to the 16 bit [short.MinValue..short.MaxValue] range
                    short shortSample = (short)(floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

                    // Calculate the right index based on the PCM format of interleaved samples per channel [L-R-L-R]
                    int index = i * channels * bytesPerSample + c * bytesPerSample;

                    // Store the 16 bit sample as two consecutive 8 bit values in the buffer with regard to endian-ness
                    if (!BitConverter.IsLittleEndian)
                    {
                        to[index] = (byte)(shortSample >> 8);
                        to[index + 1] = (byte)shortSample;
                    }
                    else
                    {
                        to[index] = (byte)shortSample;
                        to[index + 1] = (byte)(shortSample >> 8);
                    }
                }
            }
        }

        private void SubmitBuffer()
        {
            _workingBuffer = new float[2, SamplesPerBuffer];
            const int bytesPerSample = 2;
            _xnaBuffer = new byte[2 * SamplesPerBuffer * bytesPerSample];
            ClearWorkingBuffer(); // Fill buffer with zeros
            FillWorkingBuffer();
            ConvertBuffer(_workingBuffer, _xnaBuffer); // Same method used in part II
            try
            {
                _instance.SubmitBuffer(_xnaBuffer);
            }
            catch { }
        }

        private void FillWorkingBuffer()
        {

            foreach (Channel c in channelManager.channels)
            {
                c._sampleVolume = 0;
            }
            for (int i = 0; i < _workingBuffer.GetLength(1); ++i)
            {
                for (int c = 0; c < channelManager.channels.Count; ++c)
                {
                    float l, r;
                    channelManager.channels[c].ProcessSingleSample(out l, out r);
                    _workingBuffer[0, i] += l;
                    _workingBuffer[1, i] += r;
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
                if (Game1.VisualizerMode)
                    if (_tickCounter % (samplesPerTick / Rendering.Visualization.PianoSpeed) == 0)
                    {
                        Game1.visualization.Update();
                    }
            }
        }
        void ClearWorkingBuffer()
        {
            for (int c = 0; c < _workingBuffer.GetLength(0); ++c)
            {
                for (int i = 0; i < _workingBuffer.GetLength(1); ++i)
                {
                    _workingBuffer[c, i] = 0;
                }
            }
        }
    }
    public enum ResamplingModes
    {
        NoInterpolation,
        LinearInterpolation,
        Average,
    }
}

