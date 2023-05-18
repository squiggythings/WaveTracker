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
        public List<List<ChannelState>> states;
        public List<List<ChannelState>> statesPrev;
        static Color[] waveColors;
        FrameRenderer frameRenderer;
        Color currRowEmptyText = new Color(20, 24, 46);
        Color rowEmptyText = new Color(56, 64, 102);
        int oscilloscopeHeight = 48;
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
        public Visualization(FrameRenderer fr)
        {
            frameRenderer = fr;
            x = 0; y = 0;
            states = new List<List<ChannelState>>();
            states.Add(new List<ChannelState> { new ChannelState(0, 0, Color.Red) });
            statesPrev = new List<List<ChannelState>>();
            statesPrev.Add(new List<ChannelState> { new ChannelState(0, 0, Color.Red) });
            waveColors = new Color[100];
            for (int i = 0; i < 100; ++i)
            {
                waveColors[i] = Color.White;
            }
        }

        public void Update()
        {
            fillstates(states);
        }

        public void fillstates(List<List<ChannelState>> states)
        {
            List<ChannelState> rowOfStates = new List<ChannelState>();
            for (int c = 0; c < ChannelManager.instance.channels.Count; c++)
            {
                Channel chan = ChannelManager.instance.channels[c];

                if ((chan.currentMacro.macroType == MacroType.Wave || chan.currentMacro.sample.useInVisualization) && chan.CurrentAmplitude > 0.01f && chan.CurrentPitch >= 0 && chan.CurrentPitch < 120 && FrameEditor.channelToggles[c])
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
            int lastIndex = Game1.bottomOfScreen * 2 - 340;
            while (states.Count > lastIndex && lastIndex > 0)
                states.RemoveAt(lastIndex);
        }


        public void Draw()
        {
            fillstates(statesPrev);

            DrawSprite(InstrumentEditor.tex, 20, 20, 600, 24, new Rectangle(16, 688, 600, 24));
            oscilloscopeHeight = 5;
            int py = oscilloscopeHeight * 8 + 40;
            int dist = Game1.bottomOfScreen - py - 18 - 15;
            int numVisibleRows = dist / 7;
            while (numVisibleRows > 11)
            {
                oscilloscopeHeight++;
                py = oscilloscopeHeight * 8 + 40;
                dist = Game1.bottomOfScreen - py - 18 - 15;
                numVisibleRows = dist / 7;
            }

            int px = 2;

            if (numVisibleRows < 3) numVisibleRows = 3;
            int currRow = numVisibleRows / 2;
            // draw channelheaders
            int id = 0;
            int tx = px + 22;
            DrawRect(tx - 1, py, 1, numVisibleRows * 7 + 22, Colors.rowSeparatorColor);
            foreach (Channel ch in ChannelManager.instance.channels)
            {
                DrawBubbleRect(tx, py, 34, 18, Color.White);

                string str = "Ch " + (id + 1).ToString("D2");
                Write(str, tx + 16 - Helpers.getWidthOfText(str) / 2, py + 3, Colors.rowSeparatorColor);
                DrawRect(tx + 2, py + 12, 30, 3, new Color(223, 224, 232));
                int amp = (int)Math.Round(frameRenderer.GetChannelHeaderAmp(id) * 15);
                DrawRect(tx + 17 - amp, py + 12, amp * 2, 3, new Color(63, 215, 52));
                // ch._sampleVolume = -1;

                id++;
                tx += 35;
                DrawRect(tx - 1, py, 1, numVisibleRows * 7 + 22, Colors.rowSeparatorColor);
            }

            px += 22;
            py += 18;
            for (int i = 0; i < numVisibleRows; ++i)
            {
                int rowY = py + i * 7;
                int thisRow = Playback.playbackRow + i - numVisibleRows / 2;
                if (thisRow == Playback.playbackRow)
                    DrawRect(px, rowY, 35 * Song.CHANNEL_COUNT, 7, new Color(70, 80, 125));

                if (thisRow >= 0 && thisRow <= Playback.frame.GetLastRow())
                    for (int channel = 0; channel < Song.CHANNEL_COUNT * 5; channel += 5)
                    {
                        int rowX = px + (channel / 5) * 35;
                        bool hasNote = Playback.frame.pattern[thisRow][channel + 0] != -1;
                        bool hasInstrument = Playback.frame.pattern[thisRow][channel + 1] != -1;
                        if (hasNote)
                        {
                            WriteNote(Playback.frame.pattern[thisRow][channel + 0], rowX + 3, rowY, thisRow == Playback.playbackRow);
                        }
                        else
                        {
                            WriteEffect(Playback.frame.pattern[thisRow][channel + 3], Playback.frame.pattern[thisRow][channel + 4], rowX + 3, rowY, thisRow == Playback.playbackRow);
                        }
                        if (hasInstrument)
                        {
                            WriteInstrument(Playback.frame.pattern[thisRow][channel + 1], rowX + 22, rowY, thisRow == Playback.playbackRow);
                        }
                        else
                        {
                            WriteVolume(Playback.frame.pattern[thisRow][channel + 2], rowX + 22, rowY, thisRow == Playback.playbackRow);
                        }
                    }
            }
        }
















        void WriteNote(int value, int x, int y, bool currRow)
        {
            int alpha = currRow ? 255 : 120;
            Color c = Helpers.Alpha(Colors.rowText, alpha);
            if (value == -2) // off
            {
                if (!Preferences.showNoteCutAndReleaseAsSymbols)
                    Write("OFF", x, y, c);
                else
                {
                    DrawRect(x + 1, y + 2, 13, 2, c);
                }
            }
            else if (value == -3) // release 
            {
                if (!Preferences.showNoteCutAndReleaseAsSymbols)
                    Write("REL", x, y, c);
                else
                {
                    DrawRect(x + 1, y + 2, 13, 1, c);
                    DrawRect(x + 1, y + 4, 13, 1, c);
                }
            }
            else if (value < 0) // empty
            {
                WriteMonospaced("···", x + 1, y, currRow ? currRowEmptyText : Colors.rowTextEmpty, 4);
            }
            else
            {
                string val = Helpers.GetNoteName(value);
                if (val.Contains('#'))
                {
                    Write(val, x, y, c);
                }
                else
                {
                    WriteMonospaced(val[0] + "-", x, y, c, 5);
                    Write(val[2] + "", x + 11, y, c);
                }

            }
        }

        void WriteInstrument(int value, int x, int y, bool currRow)
        {
            int alpha = currRow ? 255 : 120;
            if (value < 0)
            {
                WriteMonospaced("··", x + 1, y, currRow ? currRowEmptyText : Colors.rowTextEmpty, 4);
            }
            else
            {
                if (value >= InstrumentBank.song.instruments.Count)
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Color.Red, alpha), 4);
                else if (InstrumentBank.song.instruments[value].macroType == MacroType.Sample)
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.instrumentSampleColumnText, alpha), 4);
                else
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.instrumentColumnText, alpha), 4);

            }
        }

        void WriteVolume(int value, int x, int y, bool currRow)
        {
            if (value < 0)
            {
                WriteMonospaced("··", x + 1, y, currRow ? currRowEmptyText : Colors.rowTextEmpty, 4);
            }
            else
            {
                int alpha = currRow ? 255 : 100;

                WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.volumeColumnText, alpha), 4);
            }
        }

        void WriteEffect(int value, int param, int x, int y, bool currRow)
        {
            int alpha = currRow ? 255 : 120;

            if (value < 0)
            {
                Write("·", x + 1, y, currRow ? currRowEmptyText : Colors.rowTextEmpty);
            }
            else
            {
                Write(Helpers.GetEffectCharacter(value), x, y, Helpers.Alpha(Colors.effectColumnText, alpha));
            }

            if (param < 0)
            {
                WriteMonospaced("··", x + 1 + 5, y, currRow ? currRowEmptyText : Colors.rowTextEmpty, 4);
            }
            else
            {
                if (Helpers.isEffectHexadecimal(value))
                    WriteMonospaced(param.ToString("X2"), x + 5, y, Helpers.Alpha(Colors.effectColumnParameterText, alpha), 4);
                else
                    WriteMonospaced(param.ToString("D2"), x + 5, y, Helpers.Alpha(Colors.effectColumnParameterText, alpha), 4);
            }
        }






















        void DrawBubbleRect(int x, int y, int w, int h, Color c)
        {
            DrawRect(x + 1, y, w - 2, h, c);
            DrawRect(x, y + 1, w, h - 1, c);
        }

        public void DrawOscilloscopes()
        {
            int oscsX = 628 * 2;
            int oscsY = 20 * 2;
            int oscsW = 106 * 2;
            int oscsH = oscilloscopeHeight * 2;
            int numOscsX = 3, numOscsY = 8;
            DrawRect(oscsX, oscsY, (oscsW + 2) * numOscsX + 2, (oscsH + 2) * numOscsY + 2, Color.White);
            for (int y = 0; y < numOscsY; ++y)
            {
                for (int x = 0; x < numOscsX; ++x)
                {
                    DrawOscilloscope(x + y * numOscsX + 1, oscsX + x * (oscsW + 2) + 2, oscsY + y * (oscsH + 2) + 2, oscsW, oscsH);
                }
            }
        }

        public void DrawOscilloscope(int channelNum, int px, int py, int w, int h)
        {

            Color crossColor = new Color(44, 53, 77);
            DrawRect(px, py, w, h, new Color(20, 24, 46));
            DrawRect(px, py + h / 2, w, 1, crossColor);
            DrawRect(px + w / 2, py, 1, h, crossColor);
            WriteTwiceAsBig("" + channelNum, px + 2, py - 4, Colors.cursorColor);

            Channel ch = ChannelManager.instance.channels[channelNum - 1];
            float samp1 = 0;
            float lastSamp = 0;
            if (FrameEditor.channelToggles[channelNum - 1])
            {
                if (ch.currentMacro.macroType == MacroType.Wave)
                {
                    // WAVE
                    Wave wv = ch.currentWave;
                    for (int i = -w / 2; i < w / 2 - 1; ++i)
                    {
                        lastSamp = samp1;
                        samp1 = -wv.getSampleAtPosition(i / (float)w * ch.CurrentFrequency / Preferences.visualizerScopeZoom) * (h / 2f) * ch.CurrentAmplitude + (h / 2f);
                        if (i > -w / 2)
                            DrawOscCol(px + i + w / 2, py - 2, samp1, lastSamp, waveColors[ch.waveIndex], 2);
                    }
                }
                else // SAMPLE
                {
                    Sample samp = ch.currentMacro.sample;

                    for (int i = -w / 2; i < w / 2 - 1; ++i)
                    {
                        lastSamp = samp1;

                        // time per base note cycle
                        // quantized 


                        samp1 = -samp.getMonoSample((i / (float)w * ch.CurrentFrequency / Preferences.visualizerScopeZoom) + (int)ch.sampleTime) * (h / 2f) * ch.CurrentAmplitudeAsWave + (h / 2f);
                        if (i > -w / 2)
                            DrawOscCol(px + i + w / 2, py - 2, samp1, lastSamp, Color.White, 2);
                    }
                }
            }

        }

        void DrawOscCol(int x, int y, float min, float max, Color c, int size)
        {
            if (min < max)
                DrawRect(x, y + (int)min, size, (int)(max - min) + size, c);
            else
                DrawRect(x, y + (int)max, size, (int)(min - max) + size, c);
        }

        public void DrawPiano(List<List<ChannelState>> states)
        {
            int px = 40;
            int py = 20 * 2;
            for (int i = 0; i < states.Count; i++)
            {
                foreach (ChannelState state in states[i])
                {
                    int width = (int)state.volume.Map(0, 1, 1, 15);
                    DrawRect((int)(px + state.pitch * 10 + 4) - width / 2 + 1, y + py + 24 * 2, width, 1, Helpers.Alpha(state.color, (int)state.volume.Map(0, 1, 10, 255)));
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
