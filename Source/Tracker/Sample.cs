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
        public int sampleLoopIndex;
        [ProtoMember(4)]
        public int Detune { get; private set; }
        [ProtoMember(5)]
        public int BaseKey { get; private set; }
        public int currentPlaybackPosition;
        [ProtoMember(7)]
        public bool useInVisualization;

        [ProtoMember(8)]
        public float _baseFrequency { get; private set; }
        [ProtoMember(9)]
        public short[] sampleDataAccessL = new short[0];
        [ProtoMember(10)]
        public short[] sampleDataAccessR = new short[0];
        public long Length { get { return sampleDataAccessL.LongLength; } }
        public bool IsStereo { get { return sampleDataAccessR.Length > 0; } }
        public Sample() {
            sampleLoopIndex = 0;

            useInVisualization = false;
            sampleDataAccessL = new short[0];
            sampleDataAccessR = new short[0];
            loopType = LoopType.OneShot;
            resampleMode = (ResamplingMode)Preferences.profile.defaultResampleSample;
            BaseKey = Preferences.profile.defaultBaseKey;
            Detune = 0;
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
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            short max = 0;
            foreach (short sample in sampleDataLeft) {
                if (Math.Abs(sample) > max)
                    max = Math.Abs(sample);
            }
            if (IsStereo) {
                foreach (short sample in sampleDataRight) {
                    if (Math.Abs(sample) > max)
                        max = Math.Abs(sample);
                }
            }
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] /= max;
                if (IsStereo)
                    sampleDataRight[i] /= max;
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Reverse() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            sampleDataLeft.Reverse();
            if (sampleDataRight.Count != 0)
                sampleDataRight.Reverse();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void FadeIn() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] = (short)(sampleDataLeft[i] * (float)i / sampleDataLeft.Count);
                if (IsStereo) {
                    sampleDataRight[i] = (short)(sampleDataRight[i] * (float)i / sampleDataLeft.Count);
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void FadeOut() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] = (short)(sampleDataLeft[i] * (1 - (float)i / sampleDataLeft.Count));
                if (IsStereo) {
                    sampleDataRight[i] = (short)(sampleDataRight[i] * (1 - (float)i / sampleDataLeft.Count));
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void Invert() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= -1;
                if (IsStereo) {
                    sampleDataRight[i] *= -1;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Amplify(float val) {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] = (short)Math.Clamp(sampleDataLeft[i] * val, -1f, 1f);
                if (IsStereo) {
                    sampleDataRight[i] = (short)Math.Clamp(sampleDataRight[i] * val, -1f, 1f);
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void MixToMono() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; ++i) {
                sampleDataLeft[i] = (short)(sampleDataLeft[i] / 2 + sampleDataRight[i] / 2);
            }
            sampleDataRight.Clear();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void TrimSilence() {
            List<short> sampleDataLeft = sampleDataAccessL.ToList();
            List<short> sampleDataRight = sampleDataAccessR.ToList();
            if (sampleDataLeft.Count > 1000) {
                if (IsStereo) {
                    for (int i = 0; i < sampleDataLeft.Count; ++i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                }
                else {
                    for (int i = 0; i < sampleDataLeft.Count; ++i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
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
            x += startPercentage * sampleDataAccessL.Length;
            long l = sampleDataAccessL.Length;
            long p = sampleLoopIndex;
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
                //sampleIndex = Math.Abs((sampleIndex - sampleLoopIndex + (len - 1) - 1) % ((len - 1) * 2) - len) + sampleLoopIndex;

            }
            else if (loopType == LoopType.Forward) {
                sampleIndex = ((x - p) % (l - p)) + p;
            }
            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == ResamplingMode.None) {
                outputL = GetSampleAt(0, (int)(sampleIndex + stereoPhase * 100)) * 1.5f;
                outputR = GetSampleAt(1, (int)(sampleIndex - stereoPhase * 100)) * 1.5f;
            }
            else if (resampleMode == ResamplingMode.Linear) {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(GetSampleAt(0, one + (int)(stereoPhase * 100)), GetSampleAt(0, two + (int)(stereoPhase * 100)), by) * 1.5f;
                outputR = MathHelper.Lerp(GetSampleAt(1, one - (int)(stereoPhase * 100)), GetSampleAt(1, two - (int)(stereoPhase * 100)), by) * 1.5f;
            }
            else {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(GetSampleAt(0, one + (int)(stereoPhase * 100)), GetSampleAt(0, two + (int)(stereoPhase * 100)), by);
                outputR = MathHelper.Lerp(GetSampleAt(1, one - (int)(stereoPhase * 100)), GetSampleAt(1, two - (int)(stereoPhase * 100)), by);

                outputL += GetSampleAt(0, (int)(sampleIndex + stereoPhase * 100));
                outputR += GetSampleAt(1, (int)(sampleIndex - stereoPhase * 100));
                outputL /= 1.33333333f;
                outputR /= 1.33333333f;
            }
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

    }
}

