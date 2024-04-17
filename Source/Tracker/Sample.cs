using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using WaveTracker.Audio;
using NAudio.Wave;

namespace WaveTracker.Tracker {
    [Serializable]
    [ProtoContract(SkipConstructor = false)]
    public class Sample {
        public enum LoopType { OneShot, Forward, PingPong }

        [ProtoMember(1)]
        public ResamplingMode resampleMode;
        [ProtoMember(2)]
        public LoopType loopType;
        [ProtoMember(3)]
        public int loopPoint;
        [ProtoMember(4)]
        public int Detune { get; private set; }
        [ProtoMember(5)]
        public int BaseKey { get; private set; }
        public int currentPlaybackPosition;
        [ProtoMember(7)]
        public bool useInVisualization;

        public float _baseFrequency { get; private set; }
        [ProtoMember(8)]
        public short[] sampleDataAccessL = new short[0];
        [ProtoMember(9)]
        public short[] sampleDataAccessR = new short[0];
        public int Length { get { return sampleDataAccessL.Length; } }
        public bool IsStereo { get { return sampleDataAccessR.Length > 0; } }
        public Sample() {
            loopPoint = 0;

            useInVisualization = false;
            sampleDataAccessL = new short[0];
            sampleDataAccessR = new short[0];
            loopType = LoopType.OneShot;
            resampleMode = (ResamplingMode)Preferences.profile.defaultResampleSample;
            BaseKey = Preferences.profile.defaultBaseKey;
            SetDetune(0);
        }

        [ProtoAfterDeserialization]
        void AfterDeserialized() {
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void SetDetune(int value) {
            Detune = value;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void SetBaseKey(int value) {
            BaseKey = value;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void Normalize() {
            Normalize(0, Length - 1);
        }

        public void Normalize(int start, int end) {
            float maxAmplitude = 0;
            for (int i = start; i <= end; ++i) {
                short sample = sampleDataAccessL[i];
                float val = Math.Abs(sample / (float)short.MaxValue);
                if (val > maxAmplitude)
                    maxAmplitude = val;
                if (IsStereo) {
                    sample = sampleDataAccessR[i];
                    val = Math.Abs(sample / (float)short.MaxValue);
                    if (val > maxAmplitude)
                        maxAmplitude = val;
                }
            }

            for (int i = start; i <= end; i++) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] / maxAmplitude);
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)(sampleDataAccessR[i] / maxAmplitude);
                }
            }
        }
        public void Reverse() {
            Reverse(0, Length - 1);
        }
        public void Reverse(int start, int end) {
            Array.Reverse(sampleDataAccessL, start, end - start + 1);
            if (IsStereo)
                Array.Reverse(sampleDataAccessR, start, end - start + 1);
        }
        public void FadeIn() {
            FadeIn(0, Length - 1);
        }
        public void FadeIn(int start, int end) {
            int length = end - start + 1;
            for (int i = start; i <= end; i++) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] * (float)(i - start) / length);
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)(sampleDataAccessR[i] * (float)(i - start) / length);
                }
            }
        }
        public void FadeOut() {
            FadeOut(0, Length - 1);
        }
        public void FadeOut(int start, int end) {
            int length = end - start + 1;
            for (int i = start; i <= end; i++) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] * (1 - (float)(i - start) / length));
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)(sampleDataAccessR[i] * (1 - (float)(i - start) / length));
                }
            }
        }

        public void Invert() {
            Invert(0, Length - 1);
        }
        public void Invert(int start, int end) {
            for (int i = start; i <= end; i++) {
                sampleDataAccessL[i] *= -1;
                if (IsStereo) {
                    sampleDataAccessR[i] *= -1;
                }
            }
        }

        public void Amplify(float factor) {
            Amplify(factor, 0, Length - 1);
        }

        public void Amplify(float factor, int start, int end) {
            for (int i = start; i <= end; i++) {
                sampleDataAccessL[i] = (short)Math.Clamp(sampleDataAccessL[i] * factor, short.MinValue, short.MaxValue);
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)Math.Clamp(sampleDataAccessR[i] * factor, short.MinValue, short.MaxValue);
                }
            }
        }

        public void MixToMono() {
            for (int i = 0; i < Length; ++i) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] / 2 + sampleDataAccessR[i] / 2);
            }
            sampleDataAccessR = new short[0];
        }

        public void Cut(int start, int end) {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = end; i >= start; i--) {
                sampleDataLeft.RemoveAt(i);
                if (IsStereo)
                    sampleDataRight.RemoveAt(i);
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void TrimSilence() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            if (sampleDataLeft.Count > 1000) {
                if (IsStereo) {
                    for (int i = 0; i < sampleDataLeft.Count; ++i) {
                        if (Math.Abs(sampleDataLeft[i] / (float)short.MaxValue) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i) {
                        if (Math.Abs(sampleDataLeft[i] / (float)short.MaxValue) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
                    }
                }
                else {
                    for (int i = 0; i < sampleDataLeft.Count; ++i) {
                        if (Math.Abs(sampleDataLeft[i] / (float)short.MaxValue) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i) {
                        if (Math.Abs(sampleDataLeft[i] / (float)short.MaxValue) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public float GetMonoSample(float time, float startPercentage) {
            SampleTick(time, 0, startPercentage, out float l, out float r);
            return (l + r) / 2f;
        }

        public void SampleTick(float time, float stereoPhase, float startPercentage, out float outputL, out float outputR) {
            float sampleIndex = 0;
            float x = (time * (AudioEngine.SAMPLE_RATE / _baseFrequency));
            x += startPercentage * Length;
            long l = Length;
            long p = loopPoint;
            if (loopType == LoopType.OneShot || x <= l) {
                sampleIndex = x;
            }
            else if (loopType == LoopType.PingPong) {
                float b = ((x - p) % ((l - p) * 2));
                if (b < l - p) {
                    sampleIndex = b + p;
                }
                else if (b >= l - p) {
                    sampleIndex = l - (b + p - l);
                }
            }
            else if (loopType == LoopType.Forward) {
                sampleIndex = ((x - p) % (l - p)) + p;
            }
            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == ResamplingMode.None) {
                outputL = GetSampleAt(0, (int)(sampleIndex + stereoPhase * 100));
                outputR = GetSampleAt(1, (int)(sampleIndex - stereoPhase * 100));
            }
            else if (resampleMode == ResamplingMode.Linear) {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(GetSampleAt(0, one + (int)(stereoPhase * 100)), GetSampleAt(0, two + (int)(stereoPhase * 100)), by);
                outputR = MathHelper.Lerp(GetSampleAt(1, one - (int)(stereoPhase * 100)), GetSampleAt(1, two - (int)(stereoPhase * 100)), by);
            }
            else {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(GetSampleAt(0, one + (int)(stereoPhase * 100)), GetSampleAt(0, two + (int)(stereoPhase * 100)), by);
                outputR = MathHelper.Lerp(GetSampleAt(1, one - (int)(stereoPhase * 100)), GetSampleAt(1, two - (int)(stereoPhase * 100)), by);

                outputL += GetSampleAt(0, (int)(sampleIndex + stereoPhase * 100));
                outputR += GetSampleAt(1, (int)(sampleIndex - stereoPhase * 100));
                outputL /= 2f;
                outputR /= 2f;
            }
            outputL *= 1.5f;
            outputR *= 1.5f;
        }


        float GetSampleAt(int chan, int index) {
            if (sampleDataAccessL.Length == 0) {
                return 0;
            }
            if (index < 0)
                return 0;
            if (index >= sampleDataAccessL.Length)
                return 0;
            if (index >= sampleDataAccessL.Length)
                index = sampleDataAccessL.Length - 1;
            if (chan == 0 || sampleDataAccessL.Length != sampleDataAccessR.Length)
                return sampleDataAccessL[index] / (float)short.MaxValue;
            else
                return sampleDataAccessR[index] / (float)short.MaxValue;
        }

        public void SaveToDisk() {
            string filepath;
            if (!SaveLoad.ChooseExportPath(out filepath)) {
                return;
            }
            int channels = IsStereo ? 2 : 1;
            WaveFormat format = new WaveFormat(44100, 16, channels);
            using (WaveFileWriter writer = new WaveFileWriter(filepath, format)) {
                for (int i = 0; i < Length; ++i) {
                    writer.WriteSample(sampleDataAccessL[i]);
                    if (IsStereo) {
                        writer.WriteSample(sampleDataAccessR[i]);
                    }
                }
            }
        }
    }
}

