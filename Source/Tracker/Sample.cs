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
        [ProtoMember(1)]
        public ResamplingMode resampleMode;
        [ProtoMember(2)]
        public SampleLoopType sampleLoopType;
        [ProtoMember(3)]
        public int sampleLoopIndex;
        [ProtoMember(4)]
        public int Detune { get; private set; }
        [ProtoMember(5)]
        public int BaseKey { get; private set; }
        [ProtoMember(6)]
        public int currentPlaybackPosition;
        [ProtoMember(7)]
        public bool useInVisualization;

        [ProtoMember(8)]
        public float _baseFrequency { get; private set; }
        [ProtoMember(9)]
        public float[] sampleDataAccessL = new float[0];
        [ProtoMember(10)]
        public float[] sampleDataAccessR = new float[0];

        public Sample() {
            sampleLoopIndex = 0;

            useInVisualization = false;
            sampleDataAccessL = new float[0];
            sampleDataAccessR = new float[0];
            sampleLoopType = SampleLoopType.OneShot;
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
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            float max = 0;
            foreach (float sample in sampleDataLeft) {
                if (Math.Abs(sample) > max)
                    max = MathF.Abs(sample);
            }
            if (sampleDataRight.Count != 0) {
                foreach (float sample in sampleDataRight) {
                    if (Math.Abs(sample) > max)
                        max = MathF.Abs(sample);
                }
            }
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] /= max;
                if (sampleDataRight.Count != 0)
                    sampleDataRight[i] /= max;
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Reverse() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            sampleDataLeft.Reverse();
            if (sampleDataRight.Count != 0)
                sampleDataRight.Reverse();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void FadeIn() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= (float)i / sampleDataLeft.Count;
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] *= (float)i / sampleDataLeft.Count;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void FadeOut() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= 1 - (float)i / sampleDataLeft.Count;
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] *= 1 - (float)i / sampleDataLeft.Count;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void Invert() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= -1;
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] *= -1;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Amplify(float val) {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] = Math.Clamp(sampleDataLeft[i] * val, -1f, 1f);
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] = Math.Clamp(sampleDataRight[i] * val, -1f, 1f);
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void TrimSilence() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            if (sampleDataLeft.Count > 1000) {
                if (sampleDataRight.Count == 0) {
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
            float x = (time * (AudioEngine.sampleRate / _baseFrequency));
            x += startPercentage * sampleDataAccessL.Length;
            long l = sampleDataAccessL.Length;
            long p = sampleLoopIndex;
            if (sampleLoopType == SampleLoopType.OneShot || x <= l) {
                sampleIndex = x;
            }
            else if (sampleLoopType == SampleLoopType.PingPong) {
                float b = ((x - p) % ((l - p) * 2));
                if (b < l - p) {
                    sampleIndex = b + p;
                }
                else if (b >= l - p) {
                    sampleIndex = l - (b + p - l);
                }
                //sampleIndex = Math.Abs((sampleIndex - sampleLoopIndex + (len - 1) - 1) % ((len - 1) * 2) - len) + sampleLoopIndex;

            }
            else if (sampleLoopType == SampleLoopType.Forward) {
                sampleIndex = ((x - p) % (l - p)) + p;
            }
            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == ResamplingMode.None) {
                outputL = getSample(0, (int)(sampleIndex + stereoPhase * 100)) * 1.5f;
                outputR = getSample(1, (int)(sampleIndex - stereoPhase * 100)) * 1.5f;
            }
            else if (resampleMode == ResamplingMode.Linear) {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(getSample(0, one + (int)(stereoPhase * 100)), getSample(0, two + (int)(stereoPhase * 100)), by) * 1.5f;
                outputR = MathHelper.Lerp(getSample(1, one - (int)(stereoPhase * 100)), getSample(1, two - (int)(stereoPhase * 100)), by) * 1.5f;
            }
            else {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(getSample(0, one + (int)(stereoPhase * 100)), getSample(0, two + (int)(stereoPhase * 100)), by);
                outputR = MathHelper.Lerp(getSample(1, one - (int)(stereoPhase * 100)), getSample(1, two - (int)(stereoPhase * 100)), by);

                outputL += getSample(0, (int)(sampleIndex + stereoPhase * 100));
                outputR += getSample(1, (int)(sampleIndex - stereoPhase * 100));
                outputL /= 1.33333333f;
                outputR /= 1.33333333f;
            }
        }

        public void MixToMono() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; ++i) {
                sampleDataLeft[i] += sampleDataRight[i];
                sampleDataLeft[i] /= 2f;
            }
            sampleDataRight.Clear();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        float getSample(int chan, int index) {
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
                return sampleDataAccessL[index];
            else
                return sampleDataAccessR[index];
        }


    }
}
