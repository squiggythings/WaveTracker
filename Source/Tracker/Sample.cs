using Microsoft.Xna.Framework;
using NAudio.Wave;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using WaveTracker.Audio;

namespace WaveTracker.Tracker {
    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class Sample {
        public enum LoopType { OneShot, Forward, PingPong }

        [ProtoMember(1)]
        private int resampleInt;
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

        public float BaseFrequency { get; private set; }
        [ProtoMember(8)]
        public short[] sampleDataAccessL = [];
        [ProtoMember(9)]
        public short[] sampleDataAccessR = [];
        public int Length { get { return sampleDataAccessL.Length; } }
        public bool IsStereo { get { return sampleDataAccessR.Length > 0; } }
        [ProtoMember(10)]
        public int sampleRate;
        [ProtoMember(11)]
        public string name;

        public Sample() {
            loopPoint = 0;

            useInVisualization = false;
            sampleDataAccessL = [];
            sampleDataAccessR = [];
            loopType = LoopType.OneShot;
            resampleMode = App.Settings.SamplesWaves.DefaultResampleModeSample;
            BaseKey = App.Settings.SamplesWaves.DefaultSampleBaseKey;
            sampleRate = 44100;
            SetDetune(0);
        }

        [ProtoBeforeSerialization]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void BeforeDeserialized() {
            resampleInt = (int)resampleMode;
        }

        [ProtoAfterDeserialization]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void AfterDeserialized() {
            SetBaseFrequency();
            resampleMode = (ResamplingMode)resampleInt;
        }

        public void SetDetune(int value) {
            Detune = value;
            SetBaseFrequency();
        }

        public void SetBaseKey(int value) {
            BaseKey = value;
            SetBaseFrequency();
        }

        private void SetBaseFrequency() {
            BaseFrequency = Helpers.NoteToFrequency(BaseKey - Detune / 100f);
        }

        public void Normalize() {
            Normalize(0, Length);
        }

        public void Normalize(int start, int end) {
            float maxAmplitude = 0;
            for (int i = start; i < end; ++i) {
                short sample = sampleDataAccessL[i];
                float val = Math.Abs(sample / (float)short.MaxValue);
                if (val > maxAmplitude) {
                    maxAmplitude = val;
                }

                if (IsStereo) {
                    sample = sampleDataAccessR[i];
                    val = Math.Abs(sample / (float)short.MaxValue);
                    if (val > maxAmplitude) {
                        maxAmplitude = val;
                    }
                }
            }

            for (int i = start; i < end; i++) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] / maxAmplitude);
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)(sampleDataAccessR[i] / maxAmplitude);
                }
            }
        }
        public void Reverse() {
            Reverse(0, Length);
        }
        public void Reverse(int start, int end) {
            Array.Reverse(sampleDataAccessL, start, end - start);
            if (IsStereo) {
                Array.Reverse(sampleDataAccessR, start, end - start);
            }
        }
        public void FadeIn() {
            FadeIn(0, Length);
        }
        public void FadeIn(int start, int end) {
            int length = end - start + 1;
            for (int i = start; i < end; i++) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] * (float)(i - start) / length);
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)(sampleDataAccessR[i] * (float)(i - start) / length);
                }
            }
        }
        public void FadeOut() {
            FadeOut(0, Length);
        }
        public void FadeOut(int start, int end) {
            int length = end - start + 1;
            for (int i = start; i < end; i++) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] * (1 - (float)(i - start) / length));
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)(sampleDataAccessR[i] * (1 - (float)(i - start) / length));
                }
            }
        }

        public void Invert() {
            Invert(0, Length);
        }
        public void Invert(int start, int end) {
            for (int i = start; i < end; i++) {
                sampleDataAccessL[i] *= -1;
                if (IsStereo) {
                    sampleDataAccessR[i] *= -1;
                }
            }
        }

        public void Amplify(float factor) {
            Amplify(factor, 0, Length);
        }

        public void Amplify(float factor, int start, int end) {
            for (int i = start; i < end; i++) {
                sampleDataAccessL[i] = (short)Math.Clamp(sampleDataAccessL[i] * factor, short.MinValue, short.MaxValue);
                if (IsStereo) {
                    sampleDataAccessR[i] = (short)Math.Clamp(sampleDataAccessR[i] * factor, short.MinValue, short.MaxValue);
                }
            }
        }

        public void RemoveDC() {
            if(Length > 0) {
                // Calculate and subtract left channel avg
                short avgL = (short)sampleDataAccessL.Average(x => {
                    return x;
                });

                for (int i = 0; i < Length; i++) {
                    sampleDataAccessL[i] -= avgL;
                }

                // Calculate and subtract right channel avg if stereo
                if (IsStereo) {
                    short avgR = (short)sampleDataAccessR.Average(x => {
                        return x;
                    });

                    for (int i = 0; i < Length; i++) {
                        sampleDataAccessR[i] -= avgR;
                    }
                }

            }
        }

        public void MixToMono() {
            for (int i = 0; i < Length; ++i) {
                sampleDataAccessL[i] = (short)(sampleDataAccessL[i] / 2 + sampleDataAccessR[i] / 2);
            }

            sampleDataAccessR = [];
        }

        public void Cut(int start, int end) {
            short[] newSampleDataL = new short[Length - (end - start)];
            short[] newSampleDataR = new short[IsStereo ? Length - (end - start) : 0];

            int index = 0;
            int newIndex = 0;
            while (index < start) {
                newSampleDataL[newIndex] = sampleDataAccessL[index];
                if (IsStereo) {
                    newSampleDataR[newIndex] = sampleDataAccessR[index];
                }

                index++;
                newIndex++;
            }

            index = end;
            while (index < Length) {
                newSampleDataL[newIndex] = sampleDataAccessL[index];
                if (IsStereo) {
                    newSampleDataR[newIndex] = sampleDataAccessR[index];
                }

                index++;
                newIndex++;
            }

            sampleDataAccessL = newSampleDataL;
            sampleDataAccessR = newSampleDataR;
            if (loopPoint >= Length) {
                loopPoint = 0;
                loopType = LoopType.OneShot;
            }
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
            int stereo = (int)(stereoPhase * 1000);
            float x = time * (sampleRate / BaseFrequency);
            x += startPercentage * Length;
            long l = Length;
            long p = loopPoint;
            if (loopType == LoopType.OneShot || x <= l) {
                sampleIndex = x;
            }
            else if (loopType == LoopType.PingPong) {
                float b = (x - p) % ((l - p) * 2);
                if (b < l - p) {
                    sampleIndex = b + p;
                }
                else if (b >= l - p) {
                    sampleIndex = l - (b + p - l);
                }
            }
            else if (loopType == LoopType.Forward) {
                sampleIndex = (x - p) % (l - p) + p;
            }

            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == ResamplingMode.None) {
                outputL = GetSampleAt(0, (int)(sampleIndex + stereo));
                outputR = GetSampleAt(1, (int)(sampleIndex - stereo));
            }
            else if (resampleMode == ResamplingMode.Linear) {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(GetSampleAt(0, one + stereo), GetSampleAt(0, two + stereo), by);
                outputR = MathHelper.Lerp(GetSampleAt(1, one - stereo), GetSampleAt(1, two - stereo), by);
            }
            else {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(GetSampleAt(0, one + stereo), GetSampleAt(0, two + stereo), by);
                outputR = MathHelper.Lerp(GetSampleAt(1, one - stereo), GetSampleAt(1, two - stereo), by);

                outputL += GetSampleAt(0, (int)sampleIndex + stereo);
                outputR += GetSampleAt(1, (int)sampleIndex - stereo);
                outputL /= 2f;
                outputR /= 2f;
            }

            outputL *= 1.5f;
            outputR *= 1.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetSampleAt(int chan, int index) {
            if (sampleDataAccessL.Length == 0) {
                return 0;
            }

            if (index < 0) {
                return 0;
            }

            if (index >= sampleDataAccessL.Length) {
                return 0;
            }

            if (index >= sampleDataAccessL.Length) {
                index = sampleDataAccessL.Length - 1;
            }

            return chan == 0 || sampleDataAccessL.Length != sampleDataAccessR.Length
                ? sampleDataAccessL[index] / (float)short.MaxValue
                : sampleDataAccessR[index] / (float)short.MaxValue;
        }

        public void SaveToDisk() {
            if (!SaveLoad.ChooseSampleExportPath(out string filepath)) {
                return;
            }

            int channels = IsStereo ? 2 : 1;
            WaveFormat format = new WaveFormat(sampleRate, channels);
            using (WaveFileWriter writer = new WaveFileWriter(filepath, format)) {
                for (int i = 0; i < Length; ++i) {
                    writer.WriteSample(sampleDataAccessL[i] / (float)short.MaxValue);
                    if (IsStereo) {
                        writer.WriteSample(sampleDataAccessR[i] / (float)short.MaxValue);
                    }
                }
            }
        }
    }
}

