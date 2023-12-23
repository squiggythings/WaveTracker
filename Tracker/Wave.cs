using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;
using Microsoft.Xna.Framework;
using WaveTracker.Audio;
using ProtoBuf;

namespace WaveTracker.Tracker
{
    [ProtoContract(SkipConstructor = true)]
    [Serializable]
    public class Wave
    {
        [ProtoMember(31)]
        public ResamplingModes resamplingMode;
        [ProtoMember(32)]
        public byte[] samples = new byte[64];

        public Wave()
        {
            for (int i = 0; i < 64; i++)
            {
                samples[i] = 16;
            }
            this.resamplingMode = (ResamplingModes)Preferences.profile.defaultResampleWave;
        }


        public Wave Clone()
        {
            Wave clonedWave = new Wave();
            clonedWave.resamplingMode = resamplingMode;
            for (int i = 0; i < samples.Length; ++i)
            {
                clonedWave.samples[i] = samples[i];
            }
            return clonedWave;
        }

        public Wave(string initialWaveString)
        {
            this.resamplingMode = (ResamplingModes)Preferences.profile.defaultResampleWave;
            SetWaveformFromString(initialWaveString);
        }
        public Wave(string initialWaveString, ResamplingModes resampling)
        {
            this.resamplingMode = resampling;
            SetWaveformFromString(initialWaveString);
        }

        public static Wave Sine
        {
            get { return new Wave("HJKMNOQRSTUUVVVVVVVUUTSRQONMKJHGECB9875432110000000112345789BCEF"); }
        }
        public static Wave Triangle
        {
            get { return new Wave("GHIJKLMNOPQRSTUVVUTSRQPONMLKJIHGFEDCBA98765432100123456789ABCDEF"); }
        }
        public static Wave Saw
        {
            get { return new Wave("GGHHIIJJKKLLMMNNOOPPQQRRSSTTUUVV00112233445566778899AABBCCDDEEFF"); }
        }
        public static Wave Pulse50
        {
            get { return new Wave("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV00000000000000000000000000000000"); }
        }
        public static Wave Pulse25
        {
            get { return new Wave("VVVVVVVVVVVVVVVV000000000000000000000000000000000000000000000000"); }
        }
        public static Wave Pulse12pt5
        {
            get { return new Wave("VVVVVVVV00000000000000000000000000000000000000000000000000000000"); }
        }

        public void Smooth(int amt)
        {
            byte[] ret = new byte[64];
            for (int i = 0; i < 64; ++i)
            {
                int sum = 0;
                for (int j = -amt; j <= amt; j++)
                {
                    sum += getSample(j + i);
                }
                ret[i] = (byte)Math.Round((float)sum / (amt * 2 + 1f));
            }
            for (int i = 0; i < ret.Length; i++)
            {
                samples[i] = ret[i];
            }
        }

        public void ShiftPhase(int amt)
        {
            byte[] ret = new byte[64];
            for (int i = 0; i < 64; ++i)
            {
                ret[i] = (byte)getSample(i + amt);
            }
            for (int i = 0; i < ret.Length; i++)
            {
                samples[i] = ret[i];
            }
        }

        public void Move(int amt)
        {
            for (int i = 0; i < 64; ++i)
            {
                samples[i] = (byte)Math.Clamp(samples[i] + amt, 0, 31);
            }
        }

        public void Invert()
        {
            for (int i = 0; i < 64; ++i)
            {
                samples[i] = (byte)Math.Clamp(31 - samples[i], 0, 31);
            }
        }

        public void Mutate()
        {
            Random r = new Random();
            for (int i = 0; i < 64; ++i)
            {
                samples[i] = (byte)Math.Clamp(samples[i] + r.Next(3) - 1, 0, 31);
            }
        }

        public void Randomize()
        {
            Random r = new Random();
            for (int i = 0; i < 64; i++)
            {
                samples[i] = (byte)r.Next(32);
            }
        }

        public void SetWaveformFromString(string s)
        {
            for (int i = 0; i < s.Length && i < 64; i++)
            {
                samples[i] = convertCharToDecimal(s[i]);
            }
        }

        public void SetWaveformFromNumber(string s)
        {
            string[] nums = s.Split(',');
            byte num;
            for (int i = 0; i < nums.Length && i < 64; i++)
            {
                if (byte.TryParse(nums[i], out num))
                {
                    samples[i] = num;
                }
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < samples.Length; i++)
            {
                s += convertDecimalToChar(samples[i]);
            }
            return s;
        }

        public string Pack()
        {
            if (ToString() == "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG")
            {

                return "" + (int)resamplingMode;
            }
            string s = "";
            for (int i = 0; i < samples.Length; i += 2)
            {
                s += (char)(samples[i] * 32 + samples[i + 1] + 33);
            }
            return s + "" + (int)resamplingMode;
        }

        public void Unpack(string s)
        {
            if (s.Length == 1)
            {
                for (int i = 0; i < 64; i++)
                {
                    samples[i] = 16;
                }
                resamplingMode = (ResamplingModes)int.Parse(s[0] + "");

                return;
            }
            for (int i = 0; i < 32; ++i)
            {
                int c = s[i] - 33;
                samples[i * 2] = (byte)(c / 32);
                samples[i * 2 + 1] = (byte)(c % 32);
            }
            resamplingMode = (ResamplingModes)int.Parse(s[32] + "");
        }

        public bool isEqualTo(Wave other)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                if (other.samples[i] != samples[i])
                    return false;
            }
            if (other.resamplingMode != resamplingMode)
                return false;
            return true;
        }

        public string ToNumberString()
        {
            string s = "";
            for (int i = 0; i < samples.Length - 1; i++)
            {
                s += samples[i] + ",";
            }
            s += samples[samples.Length - 1];
            return s;
        }

        public byte getSample(int index)
        {
            if (index < 0)
            {
                return samples[(int)Helpers.Mod(index, 64)];
            }

            return samples[index % 64];
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
        public float GetSampleAtPosition(float t)
        {
            while (t < 0)
                t += 1;
            if (resamplingMode == ResamplingModes.None)
            {
                return getSample((int)(t * samples.Length)) / 16f - 1f;
            }
            else if (resamplingMode == ResamplingModes.Linear)
            {
                int index1 = (int)(t * samples.Length);
                int index2 = index1 + 1;
                float sample1 = getSample(index1) / 16f - 1f;
                float sample2 = getSample(index2) / 16f - 1f;
                return MathHelper.Lerp(sample1, sample2, (float)(Helpers.Mod(t, 0.015625)) * 64);
            }
            else
            {
                float lerpt = (float)(Helpers.Mod(t, 0.015625)) * 64;
                int index1 = (int)(t * samples.Length);
                int index2 = index1 + 1;
                float sample1 = getSample(index1) / 16f - 1f;
                float sample2 = getSample(index2) / 16f - 1f;

                // int index1 = (int)(t * samples.Length);
                // int index2 = index1 + 1;
                // float sample1 = getSample(index1) / 16f - 1f;
                // float sample2 = getSample(index2) / 16f - 1f;
                // float lerp = MathHelper.Lerp(sample1, sample2, (float)Helpers.Mod(t, 0.015625) * 64);
                // float reg = getSample((int)(t * samples.Length + 0.5f)) / 16f - 1f;
                // return (lerp + reg) * 0.5f;
                if (lerpt < 0.5f)
                {
                    if (sample1 < sample2)
                        return sample1 + 0.0625f * lerpt;
                    else if (sample1 > sample2)
                        return sample1 - 0.0625f * lerpt;
                    else
                        return sample1;
                }
                else
                {
                    if (sample1 < sample2)
                        return sample2 - 0.0625f * (1 - lerpt);
                    else if (sample1 > sample2)
                        return sample2 + 0.0625f * (1 - lerpt);
                    else
                        return sample2;
                }
            }
        }


        byte convertCharToDecimal(char c) { return (byte)"0123456789ABCDEFGHIJKLMNOPQRSTUV".IndexOf(c); }
        char convertDecimalToChar(int i) { return "0123456789ABCDEFGHIJKLMNOPQRSTUV"[Math.Clamp(i, 0, 31)]; }
    }
}
