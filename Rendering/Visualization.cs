using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Audio;
using WaveTracker.Tracker;
using WaveTracker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Rendering
{
    public class Visualization : Element
    {
        public static int PianoSpeed = 8;
        public List<List<ChannelState>> states;
        static Color[] waveColors;

        public static void GetWaveColors()
        {
            waveColors = new Color[100];
            int i = 0;
            foreach (Wave w in Game1.currentSong.waves)
            {
                bool isEmpty = true;
                foreach (int sample in w.samples)
                {
                    if (sample != 16)
                        isEmpty = false;
                }
                if (isEmpty)
                    waveColors[i] = Color.Transparent;
                else
                    waveColors[i] = ColorFromWave(w);
                ++i;
            }
        }
        static Color ColorFromWave(Wave w)
        {
            double dcOffset = 0;
            float directionChanges = 0;
            double brightness = 0;
            float amp = 0;
            int difference, lastdifference;
            float sampAmp;
            float h, l;
            difference = w.getSample(1) - w.getSample(0);
            for (int i = 0; i < 64; ++i)
            {
                sampAmp = w.getSample(i) - 15.5f;
                lastdifference = difference;
                difference = w.getSample(i + 1) - w.getSample(i);
                brightness += difference * difference;
                amp += Math.Abs(sampAmp);
                dcOffset += sampAmp;
                if (!(difference == 0 || lastdifference == 0))
                {
                    if (difference < lastdifference)
                    {
                        if (Math.Abs(difference) > 1)
                            directionChanges += Math.Abs(difference);
                    }
                }
            }
            dcOffset *= 0.0961290323;
            brightness = Math.Sqrt(brightness);
            h = (float)(dcOffset * 0.8 - 55 + ((brightness * 5) % 100));
            l = (float)Math.Clamp(100 - ((brightness + directionChanges) / 1.2) - 2, 50, 100) / 100f;
            h = MathMod((h * 3.6f), 360);

            return Helpers.HSLtoRGB((int)h, 1 - (l - 0.5f) * 2, l);
        }
        static float MathMod(float a, float b)
        {
            while (a <= 0)
            {
                a += b;
            }
            return (a + b) % b;
        }
        public Visualization()
        {
            x = 0; y = 0;
            states = new List<List<ChannelState>>();
            states.Add(new List<ChannelState> { new ChannelState(0, 0, Color.Red) });
            waveColors = new Color[100];
            for (int i = 0; i < 100; ++i)
            {
                waveColors[i] = Color.White;
            }
        }

        public void Update()
        {
            List<ChannelState> rowOfStates = new List<ChannelState>();
            for (int c = 0; c < ChannelManager.instance.channels.Count; c++)
            {
                Channel chan = ChannelManager.instance.channels[c];
                if ((chan.currentMacro.macroType == MacroType.Wave || Preferences.visualizerShowSamplesInPianoRoll) && chan.CurrentAmplitude > 0.01f && chan.CurrentPitch > 0 && chan.CurrentPitch < 120 && FrameEditor.channelToggles[c])
                {
                    ChannelState state = new ChannelState(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentMacro.macroType == MacroType.Wave ? waveColors[chan.waveIndex] : Color.White);
                    rowOfStates.Add(state);
                }
            }
            rowOfStates.Sort((s1, s2) => s1.CompareTo(s2));
            if (states.Count > 0)
                states.Insert(0, rowOfStates);
            else
                states.Add(rowOfStates);
            while (states.Count > 380 * 2)
                states.RemoveAt(380 * 2);


        }


        public void Draw()
        {
            //DrawPiano(20, 50);
        }
        public void DrawPiano(int px, int py, SpriteBatch batch)
        {
            DrawSprite(InstrumentEditor.tex, px, py - 24 * 2, 1200, 48, new Rectangle(16, 688, 600, 24));
            foreach (List<ChannelState> row in states)
            {
                foreach (ChannelState state in row)
                {
                    int width = (int)state.volume.Map(0, 1, 1, 15);
                    DrawRect((int)(px + state.pitch * 10 + 4) - width / 2 + 1, y + py, width, 1, Helpers.Alpha(state.color, (int)state.volume.Map(0, 1, 10, 255)));
                }
                py++;
            }
        }

        public struct ChannelState
        {
            public float pitch { get; private set; }
            public float volume { get; private set; }
            public Color color { get; private set; }
            public ChannelState(float p, float v, Color c)
            {
                pitch = p;
                volume = v;
                color = c;
            }

            public override string ToString()
            {
                return "(" + pitch + ", " + volume + ")";
            }

            public int CompareTo(ChannelState other)
            {
                if (this.volume > other.volume)
                    return -1;
                if (this.volume < other.volume)
                    return 1;
                return 0;
            }

        }
    }

}
