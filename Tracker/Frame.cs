using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;
namespace WaveTracker.Tracker
{
    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class Frame
    {
        [ProtoMember(15)]
        public Row[] rows;
        public short[][] pattern;

        public void SetRows()
        {
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < rows[i].values.Length; j++)
                {
                    if (j % 5 == 0) // note
                        rows[i].values[j] = (byte)(pattern[i][j] + 4);
                    if (j % 5 == 1) // instrument
                        rows[i].values[j] = (byte)(pattern[i][j] + 4);
                    if (j % 5 == 2) // volume
                        rows[i].values[j] = (byte)(pattern[i][j] + 4);
                    if (j % 5 == 3) // effect
                        rows[i].values[j] = (byte)(pattern[i][j] + 4);
                    if (j % 5 == 4) // effect parameter
                        rows[i].values[j] = (byte)(pattern[i][j] + 0);
                }
            }
        }

        public void ReadRows()
        {
            pattern = new short[256][];
            for (int i = 0; i < 256; i++)
            {
                pattern[i] = new short[Song.CHANNEL_COUNT * 5];
                for (int j = 0; j < pattern[i].Length; j++)
                {
                    if (j % 5 == 0) // note
                        pattern[i][j] = (short)(rows[i].values[j] - 4);
                    if (j % 5 == 1) // instrument
                        pattern[i][j] = (short)(rows[i].values[j] - 4);
                    if (j % 5 == 2) // volume
                        pattern[i][j] = (short)(rows[i].values[j] - 4);
                    if (j % 5 == 3) // effect
                        pattern[i][j] = (short)(rows[i].values[j] - 4);
                    if (j % 5 == 4)
                    {  // effect parameter
                        pattern[i][j] = (short)(rows[i].values[j] - 0);
                        if (pattern[i][j - 1] == -1)
                        {
                            pattern[i][j] = -1;
                        }
                    }
                }
            }
        }

        public string Pack()
        {
            StringBuilder sb = new StringBuilder();
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
                        sb.Append((char)(count + 33));
                        sb.Append((char)(value + 33));
                        value = pattern[y][x];
                        count = 1;
                    }
                }
                sb.Append((char)(count + 33));
                sb.Append((char)(value + 33) + "" + (char)16);
            }
            return sb.ToString();
        }

        public void Unpack(string str)
        {
            string[] elements = str.Split((char)16);
            int x = 0;
            int y = 0;
            foreach (string element in elements)
            {
                y = 0;
                for (int i = 0; i < element.Length; i += 2)
                {
                    short count = (short)(element[i] - 33);
                    short value = (short)(element[i + 1] - 33);
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
            rows = new Row[256];
            for (int i = 0; i < 256; i++)
            {
                pattern[i] = new short[Song.CHANNEL_COUNT * 5];
                rows[i] = new Row();
                rows[i].values = new byte[Song.CHANNEL_COUNT * 5];
                for (int j = 0; j < pattern[i].Length; j++)
                {
                    pattern[i][j] = -1;
                }
            }
        }

        public int GetLastRow()
        {
            for (int y = 0; y < pattern.Length; y++)
            {
                if (y == Game1.currentSong.rowsPerFrame)
                    return y - 1;
                for (int x = 3; x < pattern[y].Length; x += 5)
                {
                    if (pattern[y][x] == 22 || pattern[y][x] == 21 || pattern[y][x] == 20)
                        return y;
                }
            }
            return 255;
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

    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class Row
    {
        [ProtoMember(16)]
        public byte[] values = new byte[Song.CHANNEL_COUNT * 5];
    }
}
