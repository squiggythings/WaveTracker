﻿
namespace WaveTracker.Audio {
    public class StereoBiQuadFilter {
        // private BiQuadFilter filterL;
        // private BiQuadFilter filterR;

        public StereoBiQuadFilter() {
            // filterL = BiQuadFilter.LowPassFilter(AudioEngine.TrueSampleRate, AudioEngine.SampleRate / 2f, 1);
            // filterR = BiQuadFilter.LowPassFilter(AudioEngine.TrueSampleRate, AudioEngine.SampleRate / 2f, 1);
        }

        public void SetLowpassFilter(float sampleRate, float cutoffFrequency, float q) {
            // filterL.SetLowPassFilter(sampleRate, cutoffFrequency, q);
            // filterR.SetLowPassFilter(sampleRate, cutoffFrequency, q);
        }

        public void Transform(float inputL, float inputR, out float outputL, out float outputR) {
            outputL = inputL; //filterL.Transform(inputL);
            outputR = inputR; //filterR.Transform(inputR);
        }

        public float TransformL(float input) {
            return input; // filterL.Transform(input);
        }
        public float TransformR(float input) {
            return input; // filterR.Transform(input);
        }
    }
}
