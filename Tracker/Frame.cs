using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WaveTracker.Tracker
{
    public class Frame
    {
        //public List<List<short>> pattern;
        public short[][] pattern;
        StringBuilder sb;
        public string Pack()
        {
            sb = new StringBuilder();
            for (int x = 0; x < Song.CHANNEL_COUNT * 5; ++x)
            {
                short value = pattern[0][x];
                int count = 1;
                for (int y = 1; y < 256; ++y)
                {
                    if (value == pattern[y][x])
                    {
                        count++;
                    }
                    else
                    {
                        sb.Append((char)count + "%");
                        sb.Append((char)(value) + "%");
                        value = pattern[y][x];
                        count = 1;
                    }
                }
                sb.Append(count + "%");
                sb.Append(value + "%&");
            }
            return sb.ToString();
        }

        public void Unpack(string str)
        {
            string[] elements = str.Split('&');
            int x = 0;
            int y = 0;
            foreach (string element in elements)
            {
                string[] columnData = element.Split('%');
                y = 0;
                for (int i = 0; i < columnData.Length - 1; i += 2)
                {
                    short count = short.Parse(columnData[i]);
                    short value = short.Parse(columnData[i + 1]);
                    for (int j = 0; j < count; j++)
                    {
                        pattern[y][x] = value;
                        y++;
                    }
                }
                x++;
            }
        }
        public Frame()
        {
            pattern = new short[256][];
            for (int i = 0; i < 256; i++)
            {
                pattern[i] = new short[Song.CHANNEL_COUNT * 5];
                for (int j = 0; j < pattern[i].Length; j++)
                {
                    pattern[i][j] = -1;

                }
            }
        }
        public void CopyInto(Frame other)
        {
            int rownum = 0;
            foreach (short[] row in this.pattern)
            {
                int i = 0;
                foreach (short val in row)
                {
                    other.pattern[rownum][i] = val;
                    i++;
                }
                rownum++;
            }
        }

        public void LoadFrom(Frame fr)
        {
            int i = 0;
            foreach (short[] row in fr.pattern)
            {
                int j = 0;
                foreach (short val in row)
                {
                    this.pattern[i][j] = val;
                    j++;
                }
                i++;
            }
        }

        public bool IsEqualTo(Frame other)
        {
            int i = 0;
            foreach (short[] row in pattern)
            {
                int j = 0;
                foreach (short val in row)
                {
                    if (other.pattern[i][j] != val)
                        return false;
                    j++;
                }
                i++;
            }
            return true;
        }

        public Frame Clone()
        {
            Frame ret = new Frame();
            int i = 0;
            foreach (short[] row in this.pattern)
            {
                int j = 0;
                foreach (short val in row)
                {
                    ret.pattern[i][j] = val;
                    j++;
                }
                i++;
            }
            return ret;
        }
    }
}
