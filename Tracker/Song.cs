using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Tracker
{
    public class Song
    {
        public const int CHANNEL_COUNT = 24;
        public List<Frame> frames;
        public int rowsPerFrame;
        public int[] ticksPerRow;
        public string name;
        public string author;
        public string year;
        public string comment;
        public List<Macro> instruments;
        public Wave[] waves;
        public int tickRate;
        public bool quantizeChannelAmplitude;

        public Song()
        {
            name = "";
            author = "";
            year = "";
            comment = "";
            ticksPerRow = new int[] { 4 };
            rowsPerFrame = 64;
            frames = new List<Frame>();
            frames.Add(new Frame());

            waves = new Wave[100];
            for (int i = 0; i < 100; i++)
            {
                waves[i] = new Wave();
            }
            waves[0] = Wave.Sine;
            waves[1] = Wave.Triangle;
            waves[2] = Wave.Saw;
            waves[3] = Wave.Pulse50;
            waves[4] = Wave.Pulse25;
            waves[5] = Wave.Pulse12pt5;
            //waves[6] = new Wave("0VG04MVVVM40GV60IK402AIQUVUQIA204KI00VG04MVVVM40GV60IK402AIQUVUQIA204KI0", Audio.ResamplingModes.LinearInterpolation);

            //waves[7] = new Wave("0MMQQUUUUUUUUSSMMEE662200000000224488AACCEEGGGGIIIIIIIIIIGGGGEEE", Audio.ResamplingModes.LinearInterpolation);
            //waves[8].Randomize();
            //waves[9].Randomize();
            //waves[10].Randomize();

            instruments = new List<Macro>();
            instruments.Add(new Macro(MacroType.Wave));

            tickRate = 60;
            quantizeChannelAmplitude = false;
        }

        public bool Equals(Song other)
        {
            if (other.name != name)
                return false;
            if (other.author != author)
                return false;
            if (other.comment != comment)
                return false;
            if (other.ticksPerRow != ticksPerRow)
                return false;
            if (other.rowsPerFrame != rowsPerFrame)
                return false;
            if (other.frames.Count != frames.Count)
                return false;
            for (int i = 0; i < 100; ++i)
            {
                if (!waves[i].isEqualTo(other.waves[i]))
                    return false;
            }
            for (int i = 0; i < frames.Count; i++)
            {
                if (!frames[i].IsEqualTo(other.frames[i]))
                    return false;
            }

            if (other.instruments.Count != instruments.Count)
                return false;
            for (int i = 0; i < instruments.Count; i++)
            {
                if (!instruments[i].IsEqualTo(other.instruments[i]))
                    return false;
            }

            return true;
        }

        public Song Clone()
        {
            Song s = new Song();
            s.frames = CloneSequence();
            s.name = name;
            s.author = author;
            s.year = year;
            s.comment = comment;
            s.ticksPerRow = ticksPerRow;
            s.rowsPerFrame = rowsPerFrame;
            s.waves = new Wave[100];
            s.tickRate = tickRate;
            s.quantizeChannelAmplitude = quantizeChannelAmplitude;
            for (int i = 0; i < 100; i++)
            {
                s.waves[i] = waves[i].Clone();
            }
            s.instruments = new List<Macro>();
            for (int i = 0; i < instruments.Count; i++)
            {
                s.instruments.Add(instruments[i].Clone());
            }
            return s;
        }

        public List<Frame> CloneSequence()
        {
            List<Frame> ret = new List<Frame>();
            for (int i = 0; i < frames.Count; i++)
            {
                ret.Add(frames[i].Clone());
            }
            return ret;
        }

        public void LoadSequenceFrom(List<Frame> otherFrames)
        {
            frames.Clear();
            for (int i = 0; i < otherFrames.Count; i++)
            {
                frames.Add(otherFrames[i].Clone());
            }
        }
        public string GetTicksAsString()
        {
            string ret = "";
            for (int i = 0; i < ticksPerRow.Length - 1; i++)
            {
                ret += ticksPerRow[i] + " ";
            }
            ret += ticksPerRow[(int)ticksPerRow.Length - 1];
            return ret;
        }
        public void LoadTicksFromString(string text)
        {
            List<int> ticks = new List<int>();
            foreach (string word in text.Split(' '))
            {
                if (checkTickString(word))
                    ticks.Add(int.Parse(word));
            }
            if (ticks.Count == 0)
                ticks.Add(1);
            ticksPerRow = new int[ticks.Count];
            int n = 0;
            foreach (int i in ticks)
            {
                ticksPerRow[n] = i;
                n++;
            }

        }

        public bool checkTickString(string st)
        {
            foreach (char c in st)
            {
                if (!"0123456789".Contains(c))
                    return false;
            }
            return true;
        }
    }
}
