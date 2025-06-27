﻿using Microsoft.Xna.Framework;
using ProtoBuf;
using System;
using System.Runtime.CompilerServices;
using WaveTracker.Audio;

namespace WaveTracker.Tracker {
    [ProtoContract(SkipConstructor = true)]
    [Serializable]
    public class Wave {
        public const byte MinSampleValue = byte.MinValue;
        public const byte MaxSampleValue = 31;

        [ProtoMember(31)]
        public ResamplingMode resamplingMode;
        [ProtoMember(32)]
        public byte[] samples = new byte[64];

        public Wave() {
            for (int i = 0; i < 64; i++) {
                samples[i] = 16;
            }
            resamplingMode = App.Settings.SamplesWaves.DefaultResampleModeWave;
        }

        public Wave Clone() {
            Wave clonedWave = new Wave();
            clonedWave.resamplingMode = resamplingMode;
            for (int i = 0; i < samples.Length; ++i) {
                clonedWave.samples[i] = samples[i];
            }
            return clonedWave;
        }

        public Wave(string initialWaveString) {
            resamplingMode = App.Settings.SamplesWaves.DefaultResampleModeWave;
            SetWaveformFromString(initialWaveString);
        }
        public Wave(string initialWaveString, ResamplingMode resampling) {
            resamplingMode = resampling;
            SetWaveformFromString(initialWaveString);
        }

        public static Wave Sine {
            get { return new Wave("HJKMNOQRSTUUVVVVVVVUUTSRQONMKJHGECB9875432110000000112345789BCEF"); }
        }
        public static Wave Triangle {
            get { return new Wave("GHIJKLMNOPQRSTUVVUTSRQPONMLKJIHGFEDCBA98765432100123456789ABCDEF"); }
        }
        public static Wave Saw {
            get { return new Wave("GGHHIIJJKKLLMMNNOOPPQQRRSSTTUUVV00112233445566778899AABBCCDDEEFF"); }
        }
        public static Wave Pulse50 {
            get { return new Wave("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV00000000000000000000000000000000"); }
        }
        public static Wave Pulse25 {
            get { return new Wave("VVVVVVVVVVVVVVVV000000000000000000000000000000000000000000000000"); }
        }
        public static Wave Pulse12pt5 {
            get { return new Wave("VVVVVVVV00000000000000000000000000000000000000000000000000000000"); }
        }

        /// <summary>
        /// Smooth the wave shape
        /// </summary>
        /// <param name="window"></param>
        public void Smooth(int window, int amt) {
            while (amt > 0) {
                byte[] ret = new byte[64];
                for (int i = 0; i < 64; ++i) {
                    int sum = 0;
                    for (int j = -window; j <= window; j++) {
                        sum += GetSample(j + i);
                    }
                    ret[i] = (byte)Math.Round(sum / (window * 2 + 1f));
                }
                for (int i = 0; i < ret.Length; i++) {
                    samples[i] = ret[i];
                }
                amt--;
            }
        }

        /// <summary>
        /// Offset the phase <c>amt</c> steps left or right
        /// </summary>
        /// <param name="amt"></param>
        public void ShiftPhase(int amt) {
            byte[] ret = new byte[64];
            for (int i = 0; i < 64; ++i) {
                ret[i] = GetSample(i + amt);
            }
            for (int i = 0; i < ret.Length; i++) {
                samples[i] = ret[i];
            }
        }

        /// <summary>
        /// Returns true if the wave is equal to the default wave
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() {
            for (int i = 0; i < 64; i++) {
                if (samples[i] != 16) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Move the wave up or down by <c>amt</c> steps
        /// </summary>
        /// <param name="amt"></param>
        public void Move(int amt) {
            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Math.Clamp(samples[i] + amt, MinSampleValue, MaxSampleValue);
            }
        }

        /// <summary>
        /// Reverse the polarity of the wave
        /// </summary>
        public void Invert() {
            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Math.Clamp(MaxSampleValue - samples[i], MinSampleValue, MaxSampleValue);
            }
        }

        /// <summary>
        /// Make the wave repeat itself <c>factor</c> times
        /// </summary>
        /// <param name="factor"></param>
        public void Sync(float factor) {
            byte[] newWave = new byte[64];

            for (int i = 0; i < newWave.Length; i++) {
                newWave[i] = samples[(int)(i * factor) % 64];
            }
            samples = newWave;
        }

        /// <summary>
        /// Randomize the wave slightly
        /// </summary>
        public void MutateHarmonics() {
            Random r = new Random();
            float[] mod = new float[64];

            for (int harmonic = 1; harmonic <= 16; ++harmonic) {
                float intensity = 1f / harmonic * (float)r.NextDouble();
                float phase = (float)r.NextDouble();
                for (int i = 0; i < 64; ++i) {
                    float t = i / 64f + phase;
                    mod[i] += MathF.Sin(t * MathF.PI * 2 * harmonic) * intensity * 2;
                    //AddHarmonic(harmonic, 1 / (harmonic + 1f) * (float)r.NextDouble());
                }
            }

            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Math.Clamp(samples[i] + mod[i] + 0.5f, MinSampleValue, MaxSampleValue);
            }

        }

        /// <summary>
        /// Hold the wave's position every <c>amt</c> samples
        /// </summary>
        /// <param name="amt"></param>
        public void SampleAndHold(int amt) {
            for (int i = 0; i < samples.Length; i++) {
                samples[i] = samples[i / amt * amt];
            }
        }

        /// <summary>
        /// Make the wave fill the whole vertical range
        /// </summary>
        public void Normalize() {
            float max = MinSampleValue;
            float min = MaxSampleValue;
            foreach (byte sample in samples) {
                if (sample > max) {
                    max = sample;
                }

                if (sample < min) {
                    min = sample;
                }
            }

            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Helpers.MapClamped(samples[i], min, max, MinSampleValue, MaxSampleValue);
            }
        }

        /// <summary>
        /// Fill the wave with random values
        /// </summary>
        public void Randomize() {
            Random r = new Random();
            for (int i = 0; i < 64; i++) {
                samples[i] = (byte)r.Next(32);
            }
        }

        public void SetWaveformFromString(string s) {
            for (int i = 0; i < s.Length && i < 64; i++) {
                samples[i] = ConvertCharToDecimal(s[i]);
            }
        }
        /// <summary>
        /// Converts a string with each number separated by a space into this wave's samples
        /// </summary>
        /// <returns></returns>
        public void SetFromNumberString(string s) {
            string[] nums = s.Split(' ');
            for (int i = 0; i < nums.Length && i < 64; i++) {
                if (byte.TryParse(nums[i], out byte num)) {
                    samples[i] = Math.Min(num, MaxSampleValue);
                }
            }
        }

        public override string ToString() {
            string s = "";
            for (int i = 0; i < samples.Length; i++) {
                s += ConvertDecimalToChar(samples[i]);
            }
            return s;
        }

        /// <summary>
        /// Converts the samples of this wave to a string with each number separated by a space
        /// </summary>
        /// <returns></returns>
        public string ToNumberString() {
            string s = "";
            for (int i = 0; i < samples.Length - 1; i++) {
                s += samples[i] + " ";
            }
            s += samples[samples.Length - 1];
            return s;
        }

        public byte GetSample(int index) {
            return index < 0 ? samples[(int)Helpers.Mod(index, 64)] : samples[index % 64];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public float GetSampleMorphed(float t, Wave other, float interpolationAmt, float bendAmt) {
            if (t is < 0 or >= 1) {
                t = Helpers.Mod(t, 1);
            }

            if (bendAmt > 0.001f && t != 0) {
                t = GetBentTime(t, bendAmt); // faster bend algorithm
            }
            return interpolationAmt > 0.001f
                ? MathHelper.Lerp(GetSampleAtPosition(t), other.GetSampleAtPosition(t), interpolationAmt)
                : GetSampleAtPosition(t);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetBentTime(float t, float bendAmt) {
            bendAmt = -25 * bendAmt * bendAmt;
            float exp = 1 - 0.8f * bendAmt;
            if (t > 0.5f) {
                t = 2 * t % 1;
                return (float)Helpers.MoreAccurateFasterPower(t, exp) / 2f + 0.5f;
            }
            else {
                t = 2 * (1 - t) % 1;
                return 1 - (float)Helpers.MoreAccurateFasterPower(t, exp) / 2f - 0.5f;
            }
        }

        /// <summary>
        /// Gets sample at the position from 0.0-1.0
        /// <br/>
        /// 0.0 is the beginning of the waveform
        /// <br/>
        /// 1.0 is the end of the waveform, one full cycle
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSampleAtPosition(float t) {
            if (t is < 0 or >= 1) {
                t = Helpers.Mod(t, 1);
            }

            int index1 = (int)(t * samples.Length);
            float sample1 = samples[index1] / 16f - 1f;

            if (resamplingMode == ResamplingMode.None) {
                return sample1;
            }

            int index2 = (index1 + 1) % 64;
            float sample2 = samples[index2] / 16f - 1f;
            float betweenSamplesLerp = t * samples.Length - index1;
            float lerpedSample = MathHelper.Lerp(sample1, sample2, betweenSamplesLerp);
            if (resamplingMode == ResamplingMode.Linear) {
                return lerpedSample;
            }

            float nearestSample = betweenSamplesLerp > 0.5f ? sample2 : sample1;

            float sampDifference = MathF.Abs(samples[index1] - samples[index2]);

            return MathHelper.Lerp(lerpedSample, nearestSample, sampDifference / MaxSampleValue);
        }

        private static byte ConvertCharToDecimal(char c) { return (byte)"0123456789ABCDEFGHIJKLMNOPQRSTUV".IndexOf(c); }

        private static char ConvertDecimalToChar(int i) { return "0123456789ABCDEFGHIJKLMNOPQRSTUV"[Math.Clamp(i, MinSampleValue, MaxSampleValue)]; }
    }
}
