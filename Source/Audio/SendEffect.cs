using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Audio {
    public abstract class SendEffect {
        protected float InputL { get; private set; }
        protected float InputR { get; private set; }
        public float OutputL { get; protected set; }
        public float OutputR { get; protected set; }
        public float WetMix { get; set; }

        public void ResetInput() {
            InputL = 0;
            InputR = 0;
        }

        public void Receive(float l, float r) {
            InputL += l;
            InputR += r;
        }

        public abstract void UpdateSampleRate();

        public abstract void Transform();

        //public abstract void Initialize(int sampleRate);
        //public void Transform(ref float sampleL, ref float sampleR);
    }

    public class StereoDistortion : SendEffect {
        //BiQuadFilter filterL, filterR;

        public float Gain { get; set; }
        public float Threshold { get; set; }
        public float Ratio { get; set; }

        public StereoDistortion() {
        }
        public override void UpdateSampleRate() {
            //filterL = BiQuadFilter.LowPassFilter(AudioEngine.SampleRate, AudioEngine.SampleRate / 2 * (1 - Damping / 2f), 1);
            //filterR = BiQuadFilter.LowPassFilter(AudioEngine.SampleRate, AudioEngine.SampleRate / 2 * (1 - Damping / 2f), 1);
        }
        public override void Transform() {
            float l = InputL;
            float r = InputR;

            if (l > Threshold)
                l = Threshold + l / Ratio;
            if (l < -Threshold)
                l = -Threshold - l / Ratio;
            if (r > Threshold)
                r = Threshold + r / Ratio;
            if (r < -Threshold)
                r = -Threshold - r / Ratio;
            OutputL = InputL + l * WetMix;
            OutputR = InputR + r * WetMix;
        }
    }
    public class StereoDelayLine : SendEffect {
        float[] bufferL;
        float[] bufferR;
        BiQuadFilter filterL, filterR;
        public int Length { get; private set; }
        public float Feedback { get; set; }
        public float Damping { get; set; }
        public bool PingPong { get; set; }
        int index;
        int delayTicks;

        public StereoDelayLine() {
            Length = 1;
            UpdateSampleRate();
        }

        public override void UpdateSampleRate() {
            filterL = BiQuadFilter.LowPassFilter(AudioEngine.SampleRate, AudioEngine.SampleRate / 2 * (1 - Damping / 2f), 1);
            filterR = BiQuadFilter.LowPassFilter(AudioEngine.SampleRate, AudioEngine.SampleRate / 2 * (1 - Damping / 2f), 1);
            SetLength(Length);
        }

        public void SetLength(int ticks) {
            Length = ticks;
            bufferL = new float[ticks * AudioEngine.SamplesPerTick];
            bufferR = new float[ticks * AudioEngine.SamplesPerTick];
        }

        public override void Transform() {
            bufferL[index] = InputL + filterL.Transform(bufferL[index]) * Feedback;
            bufferR[index] = InputR + filterR.Transform(bufferL[index]) * Feedback;
            index++;
            if (index > bufferL.Length) {
                index = 0;
            }
            OutputL = InputL + bufferL[index] * WetMix;
            OutputR = InputL + bufferR[index] * WetMix;
        }
    }
}
