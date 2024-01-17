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

namespace WaveTracker.Rendering {
    public class Visualization : Element {
        public List<List<ChannelState>> states;
        public List<List<ChannelState>> statesPrev;
        static Color[] waveColors;
        FrameRenderer frameRenderer;
        Color currRowEmptyText = new Color(20, 24, 46);
        Color rowEmptyText = new Color(56, 64, 102);
        int oscilloscopeHeight = 48;
        public static void GetWaveColors() {
            waveColors = new Color[100];
            int i = 0;
            foreach (Wave w in Song.currentSong.waves) {
                bool isEmpty = true;
                foreach (int sample in w.samples) {
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

        static Color GetColorOfWaveFromTable(int index, float morph) {
            return waveColors[index].Lerp(waveColors[(index + 1) % 100], morph);
        }

        static Color ColorFromWave(Wave w) {
            double dcOffset = 0;
            float directionChanges = 0;
            double brightness = 0;
            float amp = 0;
            int difference, lastdifference;
            float sampAmp;
            float h, l;
            difference = w.getSample(1) - w.getSample(0);
            for (int i = 0; i < 64; ++i) {
                sampAmp = w.getSample(i) - 15.5f;
                lastdifference = difference;
                difference = w.getSample(i + 1) - w.getSample(i);
                brightness += difference * difference;
                amp += Math.Abs(sampAmp);
                dcOffset += sampAmp;
                if (!(difference == 0 || lastdifference == 0)) {
                    if (difference < lastdifference) {
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
        static float MathMod(float a, float b) {
            while (a <= 0) {
                a += b;
            }
            return (a + b) % b;
        }
        public Visualization(FrameRenderer fr) {
            frameRenderer = fr;
            x = 0; y = 0;
            states = new List<List<ChannelState>>();
            states.Add(new List<ChannelState> { new ChannelState(0, 0, Color.Red, false) });
            statesPrev = new List<List<ChannelState>>();
            statesPrev.Add(new List<ChannelState> { new ChannelState(0, 0, Color.Red, false) });
            waveColors = new Color[100];
            for (int i = 0; i < 100; ++i) {
                waveColors[i] = Color.White;
            }
        }

        public void Update() {
            fillstates(states);
        }

        public void fillstates(List<List<ChannelState>> states) {
            List<ChannelState> rowOfStates = new List<ChannelState>();
            for (int c = 0; c < ChannelManager.channels.Count; c++) {
                Channel chan = ChannelManager.channels[c];

                if ((chan.currentMacro.macroType == MacroType.Wave || chan.currentMacro.sample.useInVisualization) && chan.CurrentAmplitude > 0.01f && chan.CurrentPitch >= 0 && chan.CurrentPitch < 120 && FrameEditor.channelToggles[c]) {
                    ChannelState state = new ChannelState(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentMacro.macroType == MacroType.Wave ? GetColorOfWaveFromTable(chan.waveIndex, chan.waveMorphPosition) : Color.White, chan.isPlaying);
                    rowOfStates.Add(state);
                }
            }
            rowOfStates.Sort((s1, s2) => s1.CompareTo(s2));
            if (states.Count > 0)
                states.Insert(0, rowOfStates);
            else
                states.Add(rowOfStates);
            int lastIndex = Game1.WindowHeight * 2 - 340;
            while (states.Count > lastIndex && lastIndex > 0)
                states.RemoveAt(lastIndex);
        }


        public void Draw() {
            fillstates(statesPrev);
            if (Preferences.profile.visualizerHighlightKeys)
                DrawSprite(InstrumentEditor.tex, 20, 20, 600, 24, new Rectangle(16, 688, 600, 24), new Color(128, 128, 128, 128));
            else
                DrawSprite(InstrumentEditor.tex, 20, 20, 600, 24, new Rectangle(16, 688, 600, 24));
            oscilloscopeHeight = 5;
            int py = oscilloscopeHeight * 8 + 40;
            int dist = Game1.WindowHeight - py - 18 - 15;
            int numVisibleRows = dist / 7;
            while (numVisibleRows > 11) {
                oscilloscopeHeight++;
                py = oscilloscopeHeight * 8 + 40;
                dist = Game1.WindowHeight - py - 18 - 15;
                numVisibleRows = dist / 7;
            }

            int px = 2;

            if (numVisibleRows < 3) numVisibleRows = 3;
            int currRow = numVisibleRows / 2;
            // draw channelheaders
            int id = 0;
            int tx = px + 58;


            // draw background
            //DrawRect(tx - 1, py, 841, numVisibleRows * 7 + 22, Colors.theme.background);
            DrawRect(0, py, 960, numVisibleRows * 7 + 90, Colors.theme.background);

            int rowSeparatorHeight = numVisibleRows * 7 + 90;

            // draw first row separator
            DrawRect(tx - 1, py, 1, rowSeparatorHeight, Colors.theme.rowSeparator);

            DrawBubbleRect(tx - 81, py, 80, 18, Color.White);
            DrawRect(0, py + 18, 960, 1, Colors.theme.rowSeparator);
            foreach (Channel ch in ChannelManager.channels) {
                DrawBubbleRect(tx, py, 34, 18, Color.White);

                string str = "Ch " + (id + 1).ToString("D2");
                Write(str, tx + 16 - Helpers.getWidthOfText(str) / 2, py + 3, UIColors.label);
                DrawRect(tx + 2, py + 12, 30, 3, UIColors.panel);

                // draw volume amp
                int amp = (int)Math.Round(frameRenderer.GetChannelHeaderAmp(id) * 15);
                DrawRect(tx + 17 - amp, py + 12, amp * 2, 3, new Color(63, 215, 52));

                id++;
                tx += 35;

                // draw row separator
                DrawRect(tx - 1, py, 1, rowSeparatorHeight, Colors.theme.rowSeparator);
            }
            DrawBubbleRect(tx, py, 150, 18, Color.White);
            //DrawRect(tx + 2, py + 12, 80, 3, UIColors.panel);
            px += 58;
            py += 19;
            for (int i = 0; i < numVisibleRows; ++i) {
                int rowY = py + i * 7;
                int thisRow = Playback.playbackRow + i - numVisibleRows / 2;
                if (thisRow == Playback.playbackRow) {
                    DrawRect(px, rowY, 35 * Song.CHANNEL_COUNT, 7, Colors.theme.rowSeparator);
                    DrawRect(px, rowY, 35 * Song.CHANNEL_COUNT, 7, Helpers.Alpha(Colors.theme.cursor, 90));
                }

                if (thisRow >= 0 && thisRow <= Playback.frame.GetLastRow())
                    for (int channel = 0; channel < Song.CHANNEL_COUNT * 5; channel += 5) {
                        int rowX = px + (channel / 5) * 35;
                        bool hasNote = Playback.frame.pattern[thisRow][channel + 0] != -1;
                        bool hasInstrument = Playback.frame.pattern[thisRow][channel + 1] != -1;
                        if (hasNote) {
                            WriteNote(Playback.frame.pattern[thisRow][channel + 0], rowX + 3, rowY, thisRow == Playback.playbackRow);
                        } else {
                            WriteEffect(Playback.frame.pattern[thisRow][channel + 3], Playback.frame.pattern[thisRow][channel + 4], rowX + 3, rowY, thisRow == Playback.playbackRow);
                        }
                        if (hasInstrument) {
                            WriteInstrument(Playback.frame.pattern[thisRow][channel + 1], rowX + 22, rowY, thisRow == Playback.playbackRow);
                        } else {
                            WriteVolume(Playback.frame.pattern[thisRow][channel + 2], rowX + 22, rowY, thisRow == Playback.playbackRow);
                        }
                    }
            }
        }
















        void WriteNote(int value, int x, int y, bool currRow) {
            int alpha = currRow ? 255 : 120;
            Color c = Helpers.Alpha(Colors.theme.patternText, alpha);
            if (value == Frame.NOTE_CUT_VALUE) // off
            {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("OFF", x, y, c);
                else {
                    DrawRect(x + 1, y + 2, 13, 2, c);
                }
            } else if (value == Frame.NOTE_RELEASE_VALUE) // release 
              {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("REL", x, y, c);
                else {
                    DrawRect(x + 1, y + 2, 13, 1, c);
                    DrawRect(x + 1, y + 4, 13, 1, c);
                }
            } else if (value == Frame.NOTE_EMPTY_VALUE) // empty
              {
                WriteMonospaced("···", x + 1, y, currRow ? currRowEmptyText : Helpers.Alpha(Colors.theme.patternText, Colors.theme.patternEmptyTextAlpha), 4);
            } else {
                string val = Helpers.GetNoteName(value);
                if (val.Contains('#')) {
                    Write(val, x, y, c);
                } else {
                    WriteMonospaced(val[0] + "-", x, y, c, 5);
                    Write(val[2] + "", x + 11, y, c);
                }

            }
        }

        void WriteInstrument(int value, int x, int y, bool currRow) {
            int alpha = currRow ? 255 : 120;
            if (value < 0) {
                WriteMonospaced("··", x + 1, y, currRow ? currRowEmptyText : Helpers.Alpha(Colors.theme.patternText, Colors.theme.patternEmptyTextAlpha), 4);
            } else {
                if (value >= Song.currentSong.instruments.Count)
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Color.Red, alpha), 4);
                else if (Song.currentSong.instruments[value].macroType == MacroType.Sample)
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.theme.instrumentColumnSample, alpha), 4);
                else
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.theme.instrumentColumnWave, alpha), 4);

            }
        }

        void WriteVolume(int value, int x, int y, bool currRow) {
            if (value < 0) {
                WriteMonospaced("··", x + 1, y, currRow ? currRowEmptyText : Helpers.Alpha(Colors.theme.patternText, Colors.theme.patternEmptyTextAlpha), 4);
            } else {
                int alpha = currRow ? 255 : 100;

                WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.theme.volumeColumn, alpha), 4);
            }
        }

        void WriteEffect(int value, int param, int x, int y, bool currRow) {
            int alpha = currRow ? 255 : 120;

            if (value < 0) {
                Write("·", x + 1, y, currRow ? currRowEmptyText : Helpers.Alpha(Colors.theme.patternText, Colors.theme.patternEmptyTextAlpha));
            } else {
                Write(Helpers.GetEffectCharacter(value), x, y, Helpers.Alpha(Colors.theme.effectColumn, alpha));
            }

            if (param < 0) {
                WriteMonospaced("··", x + 1 + 5, y, currRow ? currRowEmptyText : Helpers.Alpha(Colors.theme.patternText, Colors.theme.patternEmptyTextAlpha), 4);
            } else {
                if (Helpers.isEffectHexadecimal(value))
                    WriteMonospaced(param.ToString("X2"), x + 5, y, Helpers.Alpha(Colors.theme.effectColumnParameter, alpha), 4);
                else
                    WriteMonospaced(param.ToString("D2"), x + 5, y, Helpers.Alpha(Colors.theme.effectColumnParameter, alpha), 4);
            }
        }






















        void DrawBubbleRect(int x, int y, int w, int h, Color c) {
            DrawRect(x + 1, y, w - 2, h, c);
            DrawRect(x, y + 1, w, h - 1, c);
        }

        public void DrawOscilloscopes() {
            int oscsX = 628 * 2;
            int oscsY = 20 * 2;
            int oscsW = 106 * 2;
            int oscsH = oscilloscopeHeight * 2;
            int numOscsX = 3, numOscsY = 8;
            DrawRect(oscsX, oscsY, (oscsW + 2) * numOscsX + 2, (oscsH + 2) * numOscsY + 2, Color.White);
            for (int y = 0; y < numOscsY; ++y) {
                for (int x = 0; x < numOscsX; ++x) {
                    DrawOscilloscope(x + y * numOscsX + 1, oscsX + x * (oscsW + 2) + 2, oscsY + y * (oscsH + 2) + 2, oscsW, oscsH);
                }
            }
        }

        public void DrawOscilloscope(int channelNum, int px, int py, int w, int h) {

            Color crossColor = new Color(44, 53, 77);
            DrawRect(px, py, w, h, new Color(20, 24, 46));

            if (Preferences.profile.visualizerScopeCrosshairs > 0) {
                DrawRect(px, py + h / 2, w, 1, crossColor);
                if (Preferences.profile.visualizerScopeCrosshairs > 1)
                    DrawRect(px + w / 2, py, 1, h, crossColor);
            }
            WriteTwiceAsBig("" + channelNum, px + 2, py - 4, new Color(126, 133, 168));

            Channel ch = ChannelManager.channels[channelNum - 1];
            float samp1 = 0;
            float lastSamp = 0;
            float scopezoom = 40f / (Preferences.profile.visualizerScopeZoom / 100f);
            if (FrameEditor.channelToggles[channelNum - 1]) {
                if (ch.currentMacro.macroType == MacroType.Wave) {
                    // WAVE
                    Wave wv = ch.currentWave;
                    for (int i = -w / 2; i < w / 2 - 1; ++i) {
                        lastSamp = samp1;
                        float position = (i / (float)w * ch.CurrentFrequency / scopezoom);

                        samp1 = -ch.EvaluateWave(position) * (h / 2f) * ch.CurrentAmplitude + (h / 2f);
                        if (i > -w / 2)
                            DrawOscCol(px + i + w / 2, py - 2, samp1, lastSamp, Preferences.profile.visualizerScopeColors ? GetColorOfWaveFromTable(ch.waveIndex, ch.waveMorphPosition) : Color.White, Preferences.profile.visualizerScopeThickness + 1);
                    }
                } else // SAMPLE
                  {
                    Sample samp = ch.currentMacro.sample;

                    for (int i = -w / 2; i < w / 2 - 1; ++i) {
                        lastSamp = samp1;

                        // time per base note cycle
                        // quantized 


                        samp1 = -samp.getMonoSample((i / (float)w * ch.CurrentFrequency / scopezoom) + (int)ch.sampleTime) * (h / 2f) * ch.CurrentAmplitudeAsWave / 1.5f + (h / 2f);
                        if (i > -w / 2)
                            DrawOscCol(px + i + w / 2, py - 2, samp1, lastSamp, Color.White, Preferences.profile.visualizerScopeThickness + 1);
                    }
                }
            }

        }

        void DrawOscCol(int x, int y, float min, float max, Color c, int size) {
            if (min < max)
                DrawRect(x, y + (int)min, size, (int)(max - min) + size, c);
            else
                DrawRect(x, y + (int)max, size, (int)(min - max) + size, c);
        }

        public void DrawPiano(List<List<ChannelState>> states) {
            int px = 40;
            int py = 20 * 2;

            if (Preferences.profile.visualizerHighlightKeys && states.Count > 0) {
                for (int i = states[0].Count - 1; i >= 0; i--) {
                    if (states[0][i].isPlaying) {
                        int psy = y + py;
                        int pitch = (int)(states[0][i].pitch + 0.5f);
                        int psheight = 31;
                        int pswidth = 8;
                        int wo = 1;
                        int alpha = (int)states[0][i].volume.Map(0, 1, 60, 255);
                        if (!Preferences.profile.visualizerPianoFade)
                            alpha = 255;
                        if (!Helpers.isNoteBlackKey(pitch)) {
                            wo = 1;
                            psheight += 14;
                            pswidth = 8;
                        }
                        DrawRect((px + pitch * 10 + wo), psy, pswidth, psheight, Helpers.Alpha(states[0][i].color, alpha));
                    }
                }
            }
            for (int i = 0; i < states.Count; i++) {
                if (states.Count <= i)
                    return;
                foreach (ChannelState state in states[i]) {
                    int width = (int)state.volume.Map(0, 1, 1, 15);
                    if (!Preferences.profile.visualizerPianoChangeWidth)
                        width = 10;
                    int alpha = (int)state.volume.Map(0, 1, 10, 255);
                    if (!Preferences.profile.visualizerPianoFade)
                        alpha = 255;
                    DrawRect((int)(px + state.pitch * 10 + 4) - width / 2 + 1, y + py + 24 * 2, width, 1, Helpers.Alpha(state.color, alpha));
                }
                py++;
            }
        }

        public struct ChannelState {
            public float pitch { get; private set; }
            public float volume { get; private set; }
            public Color color { get; private set; }
            public bool isPlaying { get; private set; }
            public ChannelState(float p, float v, Color c, bool ip) {
                pitch = p;
                volume = v;
                color = c;
                isPlaying = ip;
            }

            public override string ToString() {
                return "(" + pitch + ", " + volume + ")";
            }

            public int CompareTo(ChannelState other) {
                if (this.volume > other.volume)
                    return -1;
                if (this.volume < other.volume)
                    return 1;
                return 0;
            }

        }
    }

}
