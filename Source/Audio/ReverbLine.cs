using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Audio {
    public class ReverbLine {

        public float Feedback { get; set; }
        public float WetMix { get; set; }


        StereoDelayLine[] delays;
        StereoDelayLine d;

        float[,] mixingMatrix;
        float[] mixingMatrixOutputL;
        float[] mixingMatrixOutputR;

        public ReverbLine() {
            Feedback = 0.9f;
            WetMix = 1.0f;
            delays = new StereoDelayLine[4];
            Random rand = new Random();
            for (int i = 0; i < delays.Length; i++) {
                delays[i] = new StereoDelayLine(rand.Next(1000, 3500), Feedback, 1f);
            }
            mixingMatrix = new float[delays.Length, delays.Length];
            for (int i = 0; i < delays.Length; i++) {
                for (int j = 0; j < delays.Length; j++) {
                    mixingMatrix[i, j] = 1;
                }
            }
            mixingMatrixOutputL = new float[delays.Length];
            mixingMatrixOutputR = new float[delays.Length];
            //mixingMatrixOutput = new float[delays.Length, delays.Length, 2];
            d = new StereoDelayLine(10000, 0.9f, 1);
        }

        public void Transform(ref float l, ref float r) {
            d.Transform(ref l, ref r);
            d._ResetCurrent(l + d._PeekWetOutput]);
            d._Send(l, r);
            d._Step(1);
            l += d._PeekWetOutput(0) * 0.2f;
            r += d._PeekWetOutput(1) * 0.2f;
            foreach (StereoDelayLine delay in delays) {
                delay._ResetCurrent(l + delay._PeekWetOutput(0) * Feedback, r);
            }
            for (int i = 0; i < delays.Length; i++) {
                mixingMatrixOutputL[i] = 0;
                mixingMatrixOutputR[i] = 0;
                for (int j = 0; j < delays.Length; j++) {
                    mixingMatrixOutputL
                    delays[j]._Send(delays[i]._PeekWetOutput(0) * mixingMatrix[i, j], delays[i]._PeekWetOutput(1) * mixingMatrix[i, j]);
                }
            }
            float wetOutputL = 0;
            float wetOutputR = 0;
            foreach (StereoDelayLine delay in delays) {
                delay._Step(1);
                wetOutputL += delay._PeekWetOutput(0);
                wetOutputR += delay._PeekWetOutput(1);
            }
            //if (wetOutputL != 0) {
            //    int a = 1;
            //}
            l = l + wetOutputL * WetMix;
            r = r + wetOutputR * WetMix;




        }
    }


    //public class AllpassFilter {
    //    public float Gain { get; set; }
    //    StereoDelayLine StereoDelay { get; set; }
    //    public AllpassFilter(int delayTime, float gain) {
    //        StereoDelay = new StereoDelayLine(delayTime, Gain, 1f);
    //    }

    //    public void Transform(ref float sampleL, ref float sampleR) {
    //        float outputL = 0;
    //        float outputR = 0;
    //        outputL = sampleL * Gain;
    //        outputR = sampleR * Gain;
    //        float delayInL = sampleL;
    //        float delayInR = sampleR;
    //        StereoDelay.Transform(ref sampleL, ref sampleR);
    //        sampleL += outputL;
    //        sampleR += outputR;

    //    }
    //}
}
