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
    public enum MacroType
    {
        Wave,
        Sample
    }
    public class Macro
    {
        public string name;
        public MacroType macroType;
        public Envelope volumeEnvelope;
        public Envelope arpEnvelope;
        public Envelope pitchEnvelope;
        public Envelope waveEnvelope;
        public Sample sample;
        public const char delimiter = '%';

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

            volumeEnvelope.isActive = true;
            volumeEnvelope.values.Add(90);
            volumeEnvelope.values.Add(84);
            volumeEnvelope.values.Add(78);
            volumeEnvelope.values.Add(60);
            volumeEnvelope.releaseIndex = 4;
            volumeEnvelope.values.Add(60);
            volumeEnvelope.values.Add(12);
            volumeEnvelope.values.Add(12);
            volumeEnvelope.values.Add(12);
            volumeEnvelope.values.Add(6);
            volumeEnvelope.values.Add(6);
            volumeEnvelope.values.Add(6);
            volumeEnvelope.values.Add(0);

            sample = new Sample();
            //sample.resampleMode = Audio.ResamplingModes.Average;


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
            if (sample.sampleBaseKey != other.sample.sampleBaseKey)
                return false;
            if (sample.sampleDetune != other.sample.sampleDetune)
                return false;
            return true;
        }

        public Macro Clone()
        {
            Debug.WriteLine("cloned " + name);
            Macro m = new Macro(macroType);
            //sample.CreateString();
            //m.Unpack(Pack() + sample.stringBuild.ToString());
            m.name = name;
            m.volumeEnvelope = volumeEnvelope.Clone();
            m.pitchEnvelope = pitchEnvelope.Clone();
            m.waveEnvelope = waveEnvelope.Clone();
            m.sample.sampleDataLeft.Clear();
            m.sample.sampleDataRight.Clear();
            for (int i = 0; i < sample.sampleDataLeft.Count; i++)
            {
                m.sample.sampleDataLeft.Add(sample.sampleDataLeft[i]);
                m.sample.sampleDataRight.Add(sample.sampleDataRight[i]);
            }
            m.sample.sampleLoopType = sample.sampleLoopType;
            m.sample.sampleLoopIndex = sample.sampleLoopIndex;
            m.sample.sampleBaseKey = sample.sampleBaseKey;
            m.sample.sampleDetune = sample.sampleDetune;
            m.sample.CreateString();
            Debug.WriteLine("equals" + m.IsEqualTo(this));
            return m;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string Pack()
        {
            string s = "";
            s += name + delimiter;
            s += (int)macroType + "" + delimiter;
            s += volumeEnvelope.Pack();
            s += arpEnvelope.Pack();
            s += pitchEnvelope.Pack();
            s += waveEnvelope.Pack();
            //Debug.WriteLine("packing " + s + sample.stringBuild.ToString());
            return s;
        }

        public void Unpack(string a)
        {

            string[] elements = a.Split(delimiter);
            // System.Diagnostics.Debug.WriteLine("unpacking " + a);

            name = elements[0];
            macroType = (MacroType)int.Parse(elements[1]);
            volumeEnvelope.Unpack(elements[2]);
            arpEnvelope.Unpack(elements[3]);
            pitchEnvelope.Unpack(elements[4]);
            waveEnvelope.Unpack(elements[5]);
            sample.sampleBaseKey = int.Parse(elements[6]);
            sample.sampleDetune = int.Parse(elements[7]);
            sample.sampleLoopType = (SampleLoopType)int.Parse(elements[8]);
            sample.sampleLoopIndex = int.Parse(elements[9]);
            sample.DataFromString(elements[10]);

        }
    }


    public class Envelope
    {
        public bool isActive = false;
        public int defaultValue;
        public List<int> values;
        public int releaseIndex = -1;
        public int loopIndex = -1;


        public string Pack()
        {
            string ret = "";
            ret += isActive ? "1&" : "0&";
            ret += releaseIndex + "&";
            ret += loopIndex + "&";
            foreach (int i in values)
            {
                ret += i + "&";
            }
            return ret + Macro.delimiter;
        }

        public void Unpack(string str)
        {
            string[] items = str.Split('&');
            isActive = items[0] == "1";
            releaseIndex = int.Parse(items[1]);
            loopIndex = int.Parse(items[2]);
            values.Clear();
            for (int i = 3; i < items.Length - 1; i++)
            {
                values.Add(int.Parse(items[i]));
            }
        }

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

            for (int i = 0; i < values.Count; i++)
            {
                if (releaseIndex == i) s += "/ ";
                if (loopIndex == i) s += "| ";
                s += values[i] + " ";
            }

            return s;
        }

        public void loadFromString(string input)
        {
            string[] parts = input.Split(' ');
            int i = 0;
            foreach (string part in parts)
            {
                if (part == "/") { releaseIndex = i; --i; }
                if (part == "|") { loopIndex = i; --i; }
                int val;
                int.TryParse(part, out val);
                values[i] = val;
                ++i;
            }
        }
    }

    public class Sample
    {
        public float SAMPLE_OUTPUT_L;
        public float SAMPLE_OUTPUT_R;
        public Audio.ResamplingModes resampleMode;
        public SampleLoopType sampleLoopType;
        public int sampleLoopIndex;
        public int sampleDetune;
        public int sampleBaseKey;
        public StringBuilder stringBuild;
        public List<float> sampleDataLeft, sampleDataRight;

        public Sample()
        {

            sampleLoopIndex = 0;

            sampleDataLeft = new List<float>();
            sampleDataRight = new List<float>();
            CreateString();
            sampleLoopType = SampleLoopType.OneShot;
            resampleMode = Audio.ResamplingModes.LinearInterpolation;
            sampleBaseKey = 60;
            sampleDetune = 0;
        }

        public void Normalize()
        {
            float max = 0;
            foreach (float sample in sampleDataLeft)
            {
                if (Math.Abs(sample) > max)
                    max = MathF.Abs(sample);
            }
            foreach (float sample in sampleDataRight)
            {
                if (Math.Abs(sample) > max)
                    max = MathF.Abs(sample);
            }
            for (int i = 0; i < sampleDataLeft.Count; i++)
            {
                sampleDataLeft[i] /= max;
                sampleDataRight[i] /= max;
            }
            CreateString();
        }

        public void Reverse()
        {
            sampleDataLeft.Reverse();
            sampleDataRight.Reverse();
            CreateString();
        }

        public void CreateString()
        {

            stringBuild = new StringBuilder();
            stringBuild.Append(sampleBaseKey + "" + Macro.delimiter);
            stringBuild.Append(sampleDetune + "" + Macro.delimiter);
            stringBuild.Append((int)sampleLoopType + "" + Macro.delimiter);
            stringBuild.Append(sampleLoopIndex + "" + Macro.delimiter);
            for (int i = 0; i < sampleDataLeft.Count; ++i)
            {
                ushort sampleL = (ushort)((this.sampleDataLeft[i] * 0.5f + 0.5f) * ushort.MaxValue / 4);
                ushort sampleR = (ushort)((this.sampleDataRight[i] * 0.5f + 0.5f) * ushort.MaxValue / 4);
                char a = (char)(sampleL);
                //char b = (char)(sampleL % 256);
                char c = (char)(sampleR);
                //char d = (char)(sampleR % 256);

                stringBuild.Append("" + a + c);
                //sampleDataLeft[i] = sampleL / ((float)ushort.MaxValue / 4) * 2 - 1;
                //sampleDataRight[i] = sampleR / ((float)ushort.MaxValue / 4) * 2 - 1;
            }
            stringBuild.Append(Macro.delimiter);
            //System.Diagnostics.Debug.WriteLine(stringBuild.ToString());
        }

        public void TrimSilence()
        {
            for (int i = sampleDataLeft.Count - 1; i >= 0; --i)
            {
                if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f)
                {
                    return;
                }
                sampleDataLeft.RemoveAt(i);
                sampleDataRight.RemoveAt(i);
            }
            CreateString();
        }

        public void DataFromString(string st)
        {
            sampleDataLeft.Clear();
            sampleDataRight.Clear();
            for (int i = 0; i < st.Length; i += 2)
            {
                int sampleL = st[i];
                int sampleR = st[i + 1];
                sampleDataLeft.Add(sampleL / ((float)ushort.MaxValue / 4) * 2 - 1);
                sampleDataRight.Add(sampleR / ((float)ushort.MaxValue / 4) * 2 - 1);
            }
        }

        public void SampleTick(decimal time, int stereoPhase)
        {
            decimal sampleIndex = (time * (decimal)(Audio.AudioEngine.sampleRate / Helpers.NoteToFrequency(sampleBaseKey - 12 + (sampleDetune / 100f))));
            int len = sampleDataLeft.Count - 1;
            if (sampleLoopType == SampleLoopType.OneShot)
            {

            }
            else if (sampleLoopType == SampleLoopType.PingPong && sampleDataLeft.Count > 2)
            {
                sampleIndex = Math.Abs((sampleIndex + (len - 1) - 1) % ((len - 1) * 2) - len);
            }
            else
            {
                sampleIndex = sampleIndex % len;
            }
            if (resampleMode == Audio.ResamplingModes.NoInterpolation)
            {
                SAMPLE_OUTPUT_L = getSample(0, (int)(sampleIndex));
                SAMPLE_OUTPUT_R = getSample(1, (int)(sampleIndex));
            }
            else if (resampleMode == Audio.ResamplingModes.LinearInterpolation)
            {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1m);
                SAMPLE_OUTPUT_L = MathHelper.Lerp(getSample(0, one), getSample(0, two), by);
                SAMPLE_OUTPUT_R = MathHelper.Lerp(getSample(1, one), getSample(1, two), by);
            }
            else
            {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1m);
                SAMPLE_OUTPUT_L = MathHelper.Lerp(getSample(0, one), getSample(0, two), by);
                SAMPLE_OUTPUT_R = MathHelper.Lerp(getSample(1, one), getSample(1, two), by);

                SAMPLE_OUTPUT_L += getSample(0, (int)(sampleIndex));
                SAMPLE_OUTPUT_R += getSample(1, (int)(sampleIndex));
                SAMPLE_OUTPUT_L /= 2;
                SAMPLE_OUTPUT_R /= 2;
            }
        }

        float getSample(int chan, int index)
        {
            if (index >= sampleDataLeft.Count)
                return 0;
            if (chan == 0)
                return sampleDataLeft[index];
            else
                return sampleDataRight[index];
        }


    }
    public enum SampleLoopType
    {
        OneShot,
        Forward,
        PingPong
    }
}
