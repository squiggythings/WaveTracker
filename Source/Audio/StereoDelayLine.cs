using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Audio {
    public class StereoDelayLine {
        float[] bufferL;
        float[] bufferR;


        public float Feedback { get; set; }
        public float WetMix { get; set; }

        public bool PingPong { get; set; }
        /// <summary>
        /// The length of the delay in samples
        /// </summary>
        int DelayLength => bufferL.Length;
        int thisIndex;
        int lastIndex => thisIndex > 1 ? thisIndex - 1 : bufferL.Length - 1;

        public enum OutputType {
            Mono,

        }
        public StereoDelayLine(int length, float feedback, float wetMix = 1f) {
            bufferL = new float[length];
            bufferR = new float[length];
            Feedback = feedback;
            WetMix = wetMix;
        }

        public void _Send(float sampleL, float sampleR, float mixAmount = 1f) {
            bufferL[thisIndex] += sampleL * mixAmount;
            bufferR[thisIndex] += sampleR * mixAmount;
        }

        public void _ResetCurrent(float sampleL, float sampleR) {
            bufferL[thisIndex] = sampleL;
            bufferR[thisIndex] = sampleR;
        }

        public void _Step(float feedback) {
            bufferL[thisIndex] *= feedback;
            bufferR[thisIndex] *= feedback;
            thisIndex++;
            if (thisIndex >= bufferL.Length) {
                thisIndex = 0;
            }
        }
        //public void Flush(out float l, out float r) {
        //    // if (thisIndex % (DelaySamples / skipAmt2) != 0)
        //    Step();
        //    l = bufferL[lastIndex];
        //    r = bufferR[lastIndex];
        //}

        public float _PeekWetOutput(int channel) {
            if (channel == 0) {
                return bufferL[lastIndex] * WetMix;
            }
            else if (channel == 1) {
                if (PingPong) {
                    return bufferR[(lastIndex + DelayLength / 2) % DelayLength] * WetMix;
                }
                else {
                    return bufferR[lastIndex] * WetMix;
                }
            }
            return 0;
        }

        public void ChangeLength(int newLength) {
            bufferL = new float[newLength];
            bufferR = new float[newLength];
        }

        public void Clear() {
            for (int i = 0; i < bufferL.Length; i++) {
                bufferL[i] = bufferR[i] = 0;
            }
        }

        public void Transform(ref float sampleL, ref float sampleR) {
            bufferL[thisIndex] += sampleL;
            bufferR[thisIndex] += sampleR;
            bufferL[thisIndex] *= Feedback;
            bufferR[thisIndex] *= Feedback;

            // step
            thisIndex++;
            if (thisIndex >= bufferL.Length) {
                thisIndex = 0;
            }

            // output
            sampleL = sampleL + bufferL[lastIndex] * WetMix;
            if (PingPong) {
                sampleR = sampleR + bufferR[(lastIndex + DelayLength / 2) % DelayLength] * WetMix;
            }
            else {
                sampleR = sampleR + bufferR[lastIndex] * WetMix;
            }
        }
    }
}
