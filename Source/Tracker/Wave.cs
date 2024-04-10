using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;
using Microsoft.Xna.Framework;
using ProtoBuf;
using WaveTracker.Audio;

namespace WaveTracker.Tracker {
    [ProtoContract(SkipConstructor = true)]
    [Serializable]
    public class Wave {
        [ProtoMember(31)]
        public ResamplingMode resamplingMode;
        [ProtoMember(32)]
        public byte[] samples = new byte[64];

        public Wave() {
            for (int i = 0; i < 64; i++) {
                samples[i] = 16;
            }
            this.resamplingMode = (ResamplingMode)Preferences.profile.defaultResampleWave;
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
            this.resamplingMode = (ResamplingMode)Preferences.profile.defaultResampleWave;
            SetWaveformFromString(initialWaveString);
        }
        public Wave(string initialWaveString, ResamplingMode resampling) {
            this.resamplingMode = resampling;
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
        /// <param name="amt"></param>
        public void Smooth(int amt) {
            byte[] ret = new byte[64];
            for (int i = 0; i < 64; ++i) {
                int sum = 0;
                for (int j = -amt; j <= amt; j++) {
                    sum += GetSample(j + i);
                }
                ret[i] = (byte)Math.Round(sum / (amt * 2 + 1f));
            }
            for (int i = 0; i < ret.Length; i++) {
                samples[i] = ret[i];
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
        /// Move the wave up or down by <c>amt</c> steps
        /// </summary>
        /// <param name="amt"></param>
        public void Move(int amt) {
            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Math.Clamp(samples[i] + amt, 0, 31);
            }
        }

        /// <summary>
        /// Reverse the polarity of the wave
        /// </summary>
        public void Invert() {
            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Math.Clamp(31 - samples[i], 0, 31);
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
        public void Mutate() {
            Random r = new Random();
            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Math.Clamp(samples[i] + r.Next(3) - 1, 0, 31);
            }
        }

        /// <summary>
        /// Hold the wave's position every <c>amt</c> samples
        /// </summary>
        /// <param name="amt"></param>
        public void Downsample(int amt) {
            for (int i = 0; i < samples.Length; i++) {
                samples[i] = samples[(i / amt) * amt];
            }
        }

        /// <summary>
        /// Make the wave fill the whole vertical range
        /// </summary>
        public void Normalize() {
            float max = 0;
            float min = 31;
            foreach (byte sample in samples) {
                if (sample > max)
                    max = sample;

                if (sample < min)
                    min = sample;
            }

            for (int i = 0; i < 64; ++i) {
                samples[i] = (byte)Helpers.MapClamped(samples[i], min, max, 0, 31);
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
                samples[i] = convertCharToDecimal(s[i]);
            }
        }

        public void SetWaveformFromNumber(string s) {
            string[] nums = s.Split(' ');
            byte num;
            for (int i = 0; i < nums.Length && i < 64; i++) {
                if (byte.TryParse(nums[i], out num)) {
                    samples[i] = num;
                }
            }
        }

        public override string ToString() {
            string s = "";
            for (int i = 0; i < samples.Length; i++) {
                s += convertDecimalToChar(samples[i]);
            }
            return s;
        }

        public string ToNumberString() {
            string s = "";
            for (int i = 0; i < samples.Length - 1; i++) {
                s += samples[i] + " ";
            }
            s += samples[samples.Length - 1];
            return s;
        }

        public byte GetSample(int index) {
            if (index < 0) {
                return samples[(int)Helpers.Mod(index, 64)];
            }

            return samples[index % 64];
        }

        public float GetSampleMorphed(float t, Wave other, float interpolationAmt, float bendAmt) {
            while (t < 0)
                t += 1;
            t %= 1;
            //t = Helpers.Mod(t, 1f);
            //if (bendAmt > 0) { // old bend code

            //    bendAmt = (bendAmt + 1) * (bendAmt + 1) * (bendAmt + 1);
            //    float tPow = MathF.Pow(t, bendAmt);
            //    float oneMinusTPow = MathF.Pow(1 - t, bendAmt);
            //    t = tPow / (tPow + oneMinusTPow);
            //    /*//float s1 = s(1, bendAmt + 1);
            //    //float s1 = s(1, bendAmt + 1);
            //    //float s2 = s(2 * t - 1, bendAmt + 1);
            //    //float s2 = s(2 * t - 1, bendAmt + 1);
            //    //t = (0.5f / s1) * s2 + 0.5f;
            //    //t = Math.Clamp(MathF.Sin((t - 0.5f) * MathF.PI / 2) * (bendAmt + 0.707f) + 0.5f, 0, 1f);*/
            //}
            if (bendAmt > 0.001f) { 
                t = GetBentTime(t, bendAmt) + 0.5f; // faster bend algorithm
            }
            if (interpolationAmt > 0) {
                return MathHelper.Lerp(GetSampleAtPosition(t), other.GetSampleAtPosition(t), interpolationAmt);
            }
            else {
                return GetSampleAtPosition(t);
            }
        }

        float GetBentTime(float t, float bendAmt) {
            if (t > 0.5f) {
                return MathF.Pow(2 * t % 1, 1 - 0.8f * (-25 * bendAmt * bendAmt)) / 2f;
            }
            else {
                return 1 - MathF.Pow(2 * (1 - t) % 1, 1 - 0.8f * (-25 * bendAmt * bendAmt)) / 2f;

            }
            //return MathF.Pow((t + 0.5f) % 1, 1 - 0.75f * bendAmt) + 0.5f;
        }


        /// <summary>
        /// Gets sample at the position from 0.0-1.0
        /// <br></br>
        /// 0.0 is the beginning of the waveform
        /// <br></br>
        /// 1.0 is the end of the waveform, one full cycle
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public float GetSampleAtPosition(float t) {
            while (t < 0)
                t += 1;
            t %= 1;
            int index1 = (int)(t * samples.Length);
            float sample1 = samples[index1] / 16f - 1f;

            if (resamplingMode == ResamplingMode.None) {
                return sample1;
            }

            int index2 = (index1 + 1) % 64;
            float sample2 = samples[index2] / 16f - 1f;
            float lerpt = (t * samples.Length) % 1;
            float lerpedSample = MathHelper.Lerp(sample1, sample2, lerpt);
            if (resamplingMode == ResamplingMode.Linear) {
                return lerpedSample;
            }


            float nearestSample = lerpt > 0.5f ? sample2 : sample1;

            float sampDifference = MathF.Abs(samples[index1] - samples[index2]);

            return MathHelper.Lerp(lerpedSample, nearestSample, sampDifference / 31f);
        }


        byte convertCharToDecimal(char c) { return (byte)"0123456789ABCDEFGHIJKLMNOPQRSTUV".IndexOf(c); }
        char convertDecimalToChar(int i) { return "0123456789ABCDEFGHIJKLMNOPQRSTUV"[Math.Clamp(i, 0, 31)]; }
    }
}
