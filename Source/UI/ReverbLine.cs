using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Audio;

namespace WaveTracker.UI {
    public class ReverbLine {

        float Feedback { get; set; }
        float WetMix { get; set; }

        DelayLine[] delayLines;

        public ReverbLine(float feedback, float wetMix) {
            WetMix = wetMix;
            delayLines = new DelayLine[10];
            Random rand = new Random();
            for (int i = 0; i < delayLines.Length; i++) {
                int length = rand.Next(500, 3000);
                delayLines[i] = new DelayLine(length, feedback * length / 3000f, 1f);
            }
        }


        public float Transform(float sample) {
            float sum = 0;
            for (int i = 0; i < delayLines.Length; ++i) {
                sum += delayLines[i].Transform(sample);
            }
            sum /= delayLines.Length;
            return sample + sum * WetMix;
        }
    }
}
