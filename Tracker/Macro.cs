using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
using System.Diagnostics;

namespace WaveTracker.Tracker
{
    [Serializable]
    public enum MacroType
    {
        Wave,
        Sample
    }
    [Serializable]
    public partial class Macro
    {
        public string name;
        public MacroType macroType;
        public Envelope volumeEnvelope;
        public Envelope arpEnvelope;
        public Envelope pitchEnvelope;
        public Envelope waveEnvelope;
        public Sample sample;
        public const char delimiter = '%';

        public Macro()
        {
            macroType = MacroType.Wave;
            name = "New Instrument";
            volumeEnvelope = new Envelope(99);
            arpEnvelope = new Envelope(0);
            pitchEnvelope = new Envelope(0);
            waveEnvelope = new Envelope(0);


            sample = new Sample();
        }
        public Macro(MacroType type)
        {
            macroType = type;
            if (type == MacroType.Wave)
                name = "New Wave Instrument";
            else if (type == MacroType.Sample)
                name = "New Sample Instrument";
            else
                name = "New Instrument";
            volumeEnvelope = new Envelope(99);
            arpEnvelope = new Envelope(0);
            pitchEnvelope = new Envelope(0);
            waveEnvelope = new Envelope(0);

            //volumeEnvelope.isActive = true;
            //volumeEnvelope.values.Add(90);
            //volumeEnvelope.values.Add(84);
            //volumeEnvelope.values.Add(78);
            //volumeEnvelope.values.Add(60);
            //volumeEnvelope.releaseIndex = 4;
            //volumeEnvelope.values.Add(60);
            //volumeEnvelope.values.Add(12);
            //volumeEnvelope.values.Add(12);
            //volumeEnvelope.values.Add(12);
            //volumeEnvelope.values.Add(6);
            //volumeEnvelope.values.Add(6);
            //volumeEnvelope.values.Add(6);
            //volumeEnvelope.values.Add(0);

            sample = new Sample();
        }

        public bool IsEqualTo(Macro other)
        {
            if (name != other.name)
                return false;

            if (macroType != other.macroType)
                return false;

            if (!volumeEnvelope.IsEqualTo(other.volumeEnvelope))
                return false;
            if (!arpEnvelope.IsEqualTo(other.arpEnvelope))
                return false;
            if (!pitchEnvelope.IsEqualTo(other.pitchEnvelope))
                return false;
            if (!waveEnvelope.IsEqualTo(other.waveEnvelope))
                return false;
            if (sample.sampleDataLeft.Count != other.sample.sampleDataLeft.Count)
                return false;
            if (sample.sampleLoopType != other.sample.sampleLoopType)
                return false;
            if (sample.sampleLoopIndex != other.sample.sampleLoopIndex)
                return false;
            if (sample.BaseKey != other.sample.BaseKey)
                return false;
            if (sample.Detune != other.sample.Detune)
                return false;
            return true;
        }

        public Macro Clone()
        {
            Macro m = new Macro(macroType);
            //sample.CreateString();
            //m.Unpack(Pack() + sample.stringBuild.ToString());
            m.name = name;
            m.volumeEnvelope = volumeEnvelope.Clone();
            m.pitchEnvelope = pitchEnvelope.Clone();
            m.arpEnvelope = arpEnvelope.Clone();
            m.waveEnvelope = waveEnvelope.Clone();
            m.sample.sampleDataLeft.Clear();
            m.sample.sampleDataRight.Clear();
            for (int i = 0; i < sample.sampleDataLeft.Count; i++)
            {
                m.sample.sampleDataLeft.Add(sample.sampleDataLeft[i]);
                if (sample.sampleDataRight.Count != 0)
                    m.sample.sampleDataRight.Add(sample.sampleDataRight[i]);
            }
            m.sample.sampleDataAccessL = m.sample.sampleDataLeft.ToArray();
            m.sample.sampleDataAccessR = m.sample.sampleDataRight.ToArray();
            m.sample.sampleLoopType = sample.sampleLoopType;
            m.sample.sampleLoopIndex = sample.sampleLoopIndex;
            m.sample.SetBaseKey(sample.BaseKey);
            m.sample.SetDetune(sample.Detune);
            return m;
        }

        public void SetName(string name)
        {
            this.name = name;
        }
    }

    [Serializable]
    public partial class Envelope
    {
        public bool isActive = false;
        public int defaultValue;
        public List<int> values;
        public int releaseIndex = -1;
        public int loopIndex = -1;

        public Envelope Clone()
        {
            Envelope ret = new Envelope(defaultValue);
            ret.isActive = isActive;
            ret.values = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                ret.values.Add(values[i]);
            }
            ret.loopIndex = loopIndex;
            ret.releaseIndex = releaseIndex;

            return ret;

        }

        public Envelope(int defaultValue)
        {
            this.defaultValue = defaultValue;
            isActive = false;
            values = new List<int>();
            releaseIndex = -1;
            loopIndex = -1;
        }
        public bool IsEqualTo(Envelope other)
        {
            if (other.isActive != isActive)
                return false;

            if (other.loopIndex != loopIndex)
                return false;
            if (other.releaseIndex != releaseIndex)
                return false;
            if (values.Count != other.values.Count)
                return false;
            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] != other.values[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasRelease { get { return releaseIndex >= 0; } }
        public bool HasLoop { get { return loopIndex >= 0; } }

        public override string ToString()
        {
            string s = "";
            if (values.Count > 0)
            {
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (loopIndex == i) s += "| ";
                    if (releaseIndex + 1 == i && releaseIndex >= 0) s += "/ ";
                    s += values[i] + " ";
                }
                int j = values.Count - 1;
                if (loopIndex == j) s += "| ";
                if (releaseIndex + 1 == j && releaseIndex >= 0) s += "/ ";
                s += values[j];
            }
            return s;
        }

        public void loadFromString(string input)
        {
            string[] parts = input.Split(' ');
            int i = 0;
            releaseIndex = -1;
            loopIndex = -1;
            values.Clear();
            foreach (string part in parts)
            {
                if (part == "/")
                    releaseIndex = --i;
                if (part.Contains('|'))
                    loopIndex = i--;
                int val = 0;
                if (int.TryParse(part, out val))
                    values.Add(val);
                i++;
            }
        }
    }
    [Serializable]
    public partial class Sample
    {
        public Audio.ResamplingModes resampleMode;
        public SampleLoopType sampleLoopType;
        public int sampleLoopIndex;
        public int Detune { get; private set; }
        public int BaseKey { get; private set; }
        public int currentPlaybackPosition;
        public float _baseFrequency { get; private set; }
        public List<float> sampleDataLeft, sampleDataRight;
        public float[] sampleDataAccessL, sampleDataAccessR;

        public Sample()
        {

            sampleLoopIndex = 0;

            sampleDataLeft = new List<float>();
            sampleDataRight = new List<float>();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
            sampleLoopType = SampleLoopType.OneShot;
            resampleMode = Audio.ResamplingModes.Linear;
            BaseKey = 48;
            Detune = 0;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void SetDetune(int value)
        {
            Detune = value;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void SetBaseKey(int value)
        {
            BaseKey = value;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }


        public void Normalize()
        {

            float max = 0;
            foreach (float sample in sampleDataLeft)
            {
                if (Math.Abs(sample) > max)
                    max = MathF.Abs(sample);
            }
            if (sampleDataRight.Count != 0)
            {
                foreach (float sample in sampleDataRight)
                {
                    if (Math.Abs(sample) > max)
                        max = MathF.Abs(sample);
                }
            }
            for (int i = 0; i < sampleDataLeft.Count; i++)
            {
                sampleDataLeft[i] /= max;
                if (sampleDataRight.Count != 0)
                    sampleDataRight[i] /= max;
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Reverse()
        {
            sampleDataLeft.Reverse();
            if (sampleDataRight.Count != 0)
                sampleDataRight.Reverse();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }



        public void TrimSilence()
        {
            if (sampleDataLeft.Count > 1000)
            {
                if (sampleDataRight.Count == 0)
                {
                    for (int i = 0; i < sampleDataLeft.Count; ++i)
                    {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f)
                        {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i)
                    {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f)
                        {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                }
                else
                {
                    for (int i = 0; i < sampleDataLeft.Count; ++i)
                    {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f)
                        {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i)
                    {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f)
                        {
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

        public float getMonoSample(float time)
        {
            float sampleIndex = 0;
            float x = (time * (Audio.AudioEngine.sampleRate / _baseFrequency));
            long l = sampleDataAccessL.Length;
            long p = sampleLoopIndex;
            if (sampleLoopType == SampleLoopType.OneShot || x <= l)
            {
                sampleIndex = x;
            }
            else if (sampleLoopType == SampleLoopType.PingPong)
            {
                long b = (long)((x - p) % ((l - p) * 2));
                if (b < l - p)
                {
                    sampleIndex = b + p;
                }
                else if (b >= l - p)
                {
                    sampleIndex = l - (b + p - l);
                }
            }
            else if (sampleLoopType == SampleLoopType.Forward)
            {
                sampleIndex = ((x - p) % (l - p)) + p;
            }
            if (sampleIndex < 0)
                return 0;
            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == Audio.ResamplingModes.None)
            {
                return (getSample(0, (int)(sampleIndex)) + getSample(1, (int)(sampleIndex))) * 0.5f;
            }
            else if (resampleMode == Audio.ResamplingModes.Linear)
            {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                return (MathHelper.Lerp(getSample(0, one), getSample(0, two), by) + MathHelper.Lerp(getSample(1, one), getSample(1, two), by)) * 0.5f;
            }
            else
            {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                float outputL, outputR = 0;
                outputL = MathHelper.Lerp(getSample(0, one), getSample(0, two), by);
                outputR = MathHelper.Lerp(getSample(1, one), getSample(1, two), by);

                outputL += getSample(0, (int)(sampleIndex));
                outputR += getSample(1, (int)(sampleIndex));
                outputL /= 2;
                outputR /= 2;
                return (outputL + outputR) / 2;
            }
        }

        public void SampleTick(float time, int stereoPhase, out float outputL, out float outputR)
        {
            float sampleIndex = 0;
            float x = (time * (Audio.AudioEngine.sampleRate / _baseFrequency));
            long l = sampleDataAccessL.Length;
            long p = sampleLoopIndex;
            if (sampleLoopType == SampleLoopType.OneShot || x <= l)
            {
                sampleIndex = x;
            }
            else if (sampleLoopType == SampleLoopType.PingPong)
            {
                long b = (long)((x - p) % ((l - p) * 2));
                if (b < l - p)
                {
                    sampleIndex = b + p;
                }
                else if (b >= l - p)
                {
                    sampleIndex = l - (b + p - l);
                }
                //sampleIndex = Math.Abs((sampleIndex - sampleLoopIndex + (len - 1) - 1) % ((len - 1) * 2) - len) + sampleLoopIndex;

            }
            else if (sampleLoopType == SampleLoopType.Forward)
            {
                sampleIndex = ((x - p) % (l - p)) + p;
            }
            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == Audio.ResamplingModes.None)
            {
                outputL = getSample(0, (int)(sampleIndex)) * 1.5f;
                outputR = getSample(1, (int)(sampleIndex)) * 1.5f;
            }
            else if (resampleMode == Audio.ResamplingModes.Linear)
            {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(getSample(0, one), getSample(0, two), by) * 1.5f;
                outputR = MathHelper.Lerp(getSample(1, one), getSample(1, two), by) * 1.5f;
            }
            else
            {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(getSample(0, one), getSample(0, two), by);
                outputR = MathHelper.Lerp(getSample(1, one), getSample(1, two), by);

                outputL += getSample(0, (int)(sampleIndex));
                outputR += getSample(1, (int)(sampleIndex));
                outputL /= 1.33333333f;
                outputR /= 1.33333333f;
            }
        }

        public void MixToMono()
        {
            for (int i = 0; i < sampleDataLeft.Count; ++i)
            {
                sampleDataLeft[i] += sampleDataRight[i];
                sampleDataLeft[i] /= 2f;
            }
            sampleDataRight.Clear();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        float getSample(int chan, int index)
        {
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
    public enum SampleLoopType
    {
        OneShot,
        Forward,
        PingPong
    }
}
