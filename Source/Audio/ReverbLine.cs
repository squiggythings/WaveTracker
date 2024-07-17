using NAudio.Dsp;
using SharpDX;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Audio {
    //public class ReverbLine {

    //    public float Feedback { get; set; }
    //    public float DryWetMix { get; set; }

    //    BiQuadFilter[] filters;
    //    BiQuadFilter[] filters2;
    //    StereoDelayLine[] delays;
    //    StereoDelayLine d;

    //    float[,] mixingMatrix;
    //    float[] mixingMatrixOutputL;
    //    float[] mixingMatrixOutputR;
    //    Random rand = new Random();
    //    float attenuationAmt;

    //    public ReverbLine() {
    //        Feedback = 0.5f;
    //        DryWetMix = 0.1f;
    //        delays = new StereoDelayLine[8];
    //        int min = 1000;
    //        int max = 20000;
    //        for (int i = 0; i < delays.Length; i++) {
    //            delays[i] = new StereoDelayLine((int)(min + (max - min) * ((float)i / delays.Length)) + rand.Next(-100, 100), Feedback, 1f);
    //            //if (i % 2 == 1)
    //            //    delays[i].PingPong = true;
    //        }
    //        attenuationAmt = (float)Math.Sqrt(delays.Length);
    //        mixingMatrix = new float[delays.Length, delays.Length];
    //        for (int i = 0; i < delays.Length; i++) {
    //            for (int j = 0; j < delays.Length; j++) {
    //                mixingMatrix[i, j] = i == j ? 1 : -1;
    //            }
    //        }
    //        filters = new BiQuadFilter[delays.Length * 2];
    //        filters2 = new BiQuadFilter[delays.Length * 2];
    //        for (int i = 0; i < filters.Length; i++) {
    //            filters[i] = BiQuadFilter.AllPassFilter(44100, rand.NextFloat(200, 20000), rand.NextFloat(0, 99f));
    //            filters2[i] = BiQuadFilter.LowPassFilter(44100, rand.NextFloat(2000, 10000), 0.9f);
    //        }
    //        mixingMatrixOutputL = new float[delays.Length];
    //        mixingMatrixOutputR = new float[delays.Length];
    //        //mixingMatrixOutput = new float[delays.Length, delays.Length, 2];
    //        d = new StereoDelayLine(10000, 0.9f, 1);
    //    }

    //    public void Transform(ref float l, ref float r) {
    //        //d.Transform(ref l, ref r);
    //        //d._ResetCurrent(l + d._PeekWetOutput(0), r + d._PeekWetOutput(1));
    //        //d._Send(l, r);
    //        //d._Step(1);
    //        //l += d._PeekWetOutput(0) * 0.2f;
    //        //r += d._PeekWetOutput(1) * 0.2f;

    //        for (int i = 0; i < delays.Length; i++) {
    //            mixingMatrixOutputL[i] = 0;
    //            mixingMatrixOutputR[i] = 0;
    //            for (int j = 0; j < delays.Length; j++) {
    //                mixingMatrixOutputL[i] += delays[j]._PeekWetOutput(0) * mixingMatrix[i, j];
    //                mixingMatrixOutputR[i] += delays[j]._PeekWetOutput(1) * mixingMatrix[i, j];
    //            }

    //        }
    //        for (int i = 0; i < delays.Length; i++) {
    //            mixingMatrixOutputL[i] = filters[i * 2].Transform(mixingMatrixOutputL[i]);
    //            mixingMatrixOutputR[i] = filters[i * 2 + 1].Transform(mixingMatrixOutputR[i]);
    //            // mixingMatrixOutputL[i] = filters2[i * 2].Transform(mixingMatrixOutputL[i]);
    //            //mixingMatrixOutputR[i] = filters2[i * 2 + 1].Transform(mixingMatrixOutputR[i]);
    //            delays[i]._ResetCurrent(l + mixingMatrixOutputL[i] * Feedback / attenuationAmt, r + mixingMatrixOutputR[i] * Feedback / attenuationAmt);
    //        }
    //        float wetOutputL = 0;
    //        float wetOutputR = 0;
    //        foreach (StereoDelayLine delay in delays) {
    //            delay._Step(1);
    //            wetOutputL += delay._PeekWetOutput(0);
    //            wetOutputR += delay._PeekWetOutput(1);
    //        }
    //        wetOutputL /= delays.Length;
    //        wetOutputR /= delays.Length;

    //        //if (wetOutputL != 0) {
    //        //    int a = 1;
    //        //}
    //        l = l * (1 - DryWetMix) + wetOutputL * DryWetMix;
    //        r = r * (1 - DryWetMix) + wetOutputR * DryWetMix;
    //    }
    //}


    ////public class AllpassFilter {
    ////    public float Gain { get; set; }
    ////    StereoDelayLine StereoDelay { get; set; }
    ////    public AllpassFilter(int delayTime, float gain) {
    ////        StereoDelay = new StereoDelayLine(delayTime, Gain, 1f);
    ////    }

    ////    public void Transform(ref float sampleL, ref float sampleR) {
    ////        float outputL = 0;
    ////        float outputR = 0;
    ////        outputL = sampleL * Gain;
    ////        outputR = sampleR * Gain;
    ////        float delayInL = sampleL;
    ////        float delayInR = sampleR;
    ////        StereoDelay.Transform(ref sampleL, ref sampleR);
    ////        sampleL += outputL;
    ////        sampleR += outputR;

    ////    }
    ////}
}
