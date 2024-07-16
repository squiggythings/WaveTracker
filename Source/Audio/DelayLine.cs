using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Audio {
    public class DelayLine {
        float[] buffer;
        float Feedback { get; set; }
        float WetMix { get; set; }
        int DelaySamples { get; set; }
        int thisIndex;
        int lastIndex => thisIndex > 1 ? thisIndex - 1 : buffer.Length - 1;

        Random rand = new Random();

        public DelayLine(int length, float feedback, float wetMix) {
            buffer = new float[length];
            DelaySamples = length;
            Feedback = feedback;
            WetMix = wetMix;
        }

        public void Add(float sample, float mixAmount = 1f) {
            buffer[thisIndex] += sample * mixAmount;

        }

        void Step() {
            thisIndex++;
            if (thisIndex >= buffer.Length) {
                thisIndex = 0;
            }
        }
        public float Flush() {
            // if (thisIndex % (DelaySamples / skipAmt2) != 0)
            Step();
            return buffer[lastIndex] * WetMix;
        }

        public float Transform(float sample) {
            //thisIndex++;
            //if (thisIndex >= buffer.Length) {
            //    thisIndex = 0;
            //}
            //buffer[thisIndex] = sample + buffer[thisIndex] * Feedback;
            Add(sample);
            double chance = rand.NextDouble();
            if (chance < 0.001)
                Step();
            if (chance < 0.999)
                Step();
            buffer[thisIndex] *= Feedback;
            return sample + buffer[lastIndex] * WetMix;
        }
    }
}
