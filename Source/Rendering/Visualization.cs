using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Tracker;
using WaveTracker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Audio;

namespace WaveTracker.Rendering {
    public class Visualization : Element {
        public List<List<ChannelState>> states;
        public List<List<ChannelState>> statesPrev;
        static Color[] waveColors;
        //FrameRenderer frameRenderer;
        Color currRowEmptyText = new Color(20, 24, 46);
        Color rowEmptyText = new Color(56, 64, 102);
        //int oscilloscopeHeight = 48;
        int oscilloscopePanelHeight;
        public static void GetWaveColors() {
            waveColors = new Color[100];
            int i = 0;
            foreach (Wave w in App.CurrentModule.WaveBank) {
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
            difference = w.GetSample(1) - w.GetSample(0);
            for (int i = 0; i < 64; ++i) {
                sampAmp = w.GetSample(i) - 15.5f;
                lastdifference = difference;
                difference = w.GetSample(i + 1) - w.GetSample(i);
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
        public Visualization() {
            //frameRenderer = fr;
            x = 0;
            y = 10;
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
            Fillstates(states);
        }

        void Fillstates(List<List<ChannelState>> states) {
            List<ChannelState> rowOfStates = new List<ChannelState>();
            for (int c = 0; c < ChannelManager.channels.Count; c++) {
                Channel chan = ChannelManager.channels[c];

                if ((chan.currentInstrument is WaveInstrument || (chan.currentInstrument is SampleInstrument inst && inst.sample.useInVisualization)) && chan.CurrentAmplitude > 0.01f && chan.CurrentPitch >= 12 && chan.CurrentPitch < 132 && ChannelManager.IsChannelOn(c)) {
                    ChannelState state = new ChannelState(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentInstrument is WaveInstrument ? GetColorOfWaveFromTable(chan.WaveIndex, chan.WaveMorphPosition) : Color.White, chan.IsPlaying);
                    rowOfStates.Add(state);
                }
            }
            rowOfStates.Sort((s1, s2) => s1.CompareTo(s2));
            if (states.Count > 0)
                states.Insert(0, rowOfStates);
            else
                states.Add(rowOfStates);
            int lastIndex = App.WindowHeight * 2 - 340;
            while (states.Count > lastIndex && lastIndex > 0)
                states.RemoveAt(lastIndex);
        }


        public void Draw() {

            Fillstates(statesPrev);
            for (int i = 0; i < 10; ++i) {
                if (App.Settings.Visualizer.HighlightPressedKeys) {
                    DrawSprite(20 + i * 60, 24, 60, 24, new Rectangle(0, 104, 60, 24), new Color(128, 128, 128, 128));
                }
                else {
                    DrawSprite(20 + i * 60, 24, 60, 24, new Rectangle(0, 104, 60, 24));
                }
            }
            oscilloscopePanelHeight = 40;
            //oscilloscopeHeight = 5;
            int py;// = oscilloscopeHeight * 8 + 40;
            int distFromBottomOfScreen = App.WindowHeight - oscilloscopePanelHeight - 40 - 18 - 15;
            int numVisibleRows = distFromBottomOfScreen / 7;
            while (numVisibleRows > 11) {
                oscilloscopePanelHeight++;
                distFromBottomOfScreen = App.WindowHeight - oscilloscopePanelHeight - 40 - 18 - 15;
                numVisibleRows = distFromBottomOfScreen / 7;
            }
            py = oscilloscopePanelHeight + 40;

            int px = 2;

            if (numVisibleRows < 3) numVisibleRows = 3;
            int currRow = numVisibleRows / 2;
            // draw channelheaders
            int channelWidth = App.CurrentModule.ChannelCount <= 13 ? 65 : 35;
            int trackerWidth = App.CurrentModule.ChannelCount * channelWidth;
            px = (App.WindowWidth - trackerWidth) / 2;


            // draw background
            //DrawRect(tx - 1, py, 841, numVisibleRows * 7 + 22, App.Settings.Appearance.Theme.background);
            DrawRect(0, py, 960, numVisibleRows * 7 + 90, App.Settings.Appearance.Theme["Row background"]);

            int rowSeparatorHeight = numVisibleRows * 7 + 19;

            // draw first row separator
            DrawRect(px - 1, py, 1, rowSeparatorHeight, App.Settings.Appearance.Theme["Channel separator"]);

            DrawBubbleRect(-1, py, px, 18, Color.White);
            DrawRect(0, py + 18, 960, 1, App.Settings.Appearance.Theme["Channel separator"]);
            for (int i = 0; i < App.CurrentModule.ChannelCount; ++i) {
                DrawBubbleRect(px + i * channelWidth, py, channelWidth - 1, 18, Color.White);

                string str;
                if (channelWidth == 35) {
                    str = "Ch " + (i + 1).ToString("D2");
                }
                else {
                    str = "Channel " + (i + 1).ToString("D2");
                }
                Write(str, px + i * channelWidth + channelWidth / 2 - Helpers.GetWidthOfText(str) / 2 - 1, py + 3, UIColors.label);
                int volumeStartX = px + i * channelWidth + 2;
                int maxVolumeWidth = channelWidth - 1 - 4;
                DrawRect(volumeStartX, py + 12, maxVolumeWidth, 3, UIColors.panel);

                // draw volume amp
                int amp = (int)(maxVolumeWidth / 2 * App.PatternEditor.ChannelHeaders[i].Amplitude);
                DrawRect(volumeStartX + maxVolumeWidth / 2 - amp, py + 12, amp * 2, 3, new Color(63, 215, 52));


                // draw row separator
                DrawRect(px + (i + 1) * channelWidth - 1, py, 1, rowSeparatorHeight, App.Settings.Appearance.Theme["Channel separator"]);
            }
            DrawBubbleRect(px + trackerWidth, py, App.WindowWidth - (px + trackerWidth) + 1, 18, Color.White);
            //DrawRect(tx + 2, py + 12, 80, 3, UIColors.panel);
            py += 19;

            // draw rows
            for (int i = 0; i < numVisibleRows; ++i) {
                int rowY = py + i * 7;
                int thisRow = Playback.position.Row + i - numVisibleRows / 2;
                if (thisRow == Playback.position.Row) {
                    DrawRect(px - 1, rowY, trackerWidth + 1, 7, App.Settings.Appearance.Theme["Channel separator"]);
                    DrawRect(px - 1, rowY, trackerWidth + 1, 7, Helpers.Alpha(App.Settings.Appearance.Theme["Cursor"], 90));
                }

                if (thisRow >= 0 && thisRow < Playback.Frame.GetLength())
                    DrawRow(px, rowY, i, Playback.position.Frame, thisRow, 0, thisRow == Playback.position.Row, channelWidth);
            }

        }








        void DrawRow(int x, int y, int line, int frame, int row, int frameWrap, bool currRow, int channelWidth) {
            // get the row color
            Color rowTextColor = App.Settings.Appearance.Theme["Row text"];
            if (row % App.CurrentSong.RowHighlightPrimary == 0)
                rowTextColor = App.Settings.Appearance.Theme["Row text (primary highlight)"];
            else if (row % App.CurrentSong.RowHighlightSecondary == 0)
                rowTextColor = App.Settings.Appearance.Theme["Row text (secondary highlight)"];
            Color c = Helpers.Alpha(App.Settings.Appearance.Theme["Row text"], currRow ? 255 : 120);

            // draw pattern events
            for (int channel = 0; channel < App.CurrentModule.ChannelCount; ++channel) {
                if (channelWidth == 35) {
                    DrawPatternEventCompact(frame, row, channel, frameWrap, x + channel * channelWidth + 1, y, App.CurrentSong.NumEffectColumns[channel], currRow ? 255 : 120);
                }
                else {
                    DrawPatternEventExpanded(frame, row, channel, frameWrap, x + channel * channelWidth + 1, y, App.CurrentSong.NumEffectColumns[channel], currRow ? 255 : 120);
                }
            }
        }

        void DrawPatternEventCompact(int frame, int row, int channel, int frameWrap, int x, int y, int effectColumns, int alpha) {

            int noteValue = App.CurrentSong[frame][row, channel, CellType.Note];
            int instrumentValue = App.CurrentSong[frame][row, channel, CellType.Instrument];
            int volumeValue = App.CurrentSong[frame][row, channel, CellType.Volume];

            Color emptyColor = App.Settings.Appearance.Theme["Row text"].MultiplyWith(App.Settings.Appearance.Theme["Empty dashes tint"]);
            Color noteColor = Helpers.Alpha(App.Settings.Appearance.Theme["Row text"], alpha);
            // draw note

            if (noteValue == WTPattern.EVENT_NOTE_CUT) {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText)
                    Write("OFF", x + 2, y, noteColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 2, noteColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_NOTE_RELEASE) {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText)
                    Write("REL", x + 2, y, noteColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 1, noteColor);
                    DrawRect(x + 3, y + 4, 13, 1, noteColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_EMPTY) {
                bool wroteAnEffect = false;
                for (int i = 0; i < effectColumns; ++i) {
                    int thisEffectType = App.CurrentSong[frame][row, channel, CellType.Effect1 + i * 2];
                    int thisEffectParameter = App.CurrentSong[frame][row, channel, CellType.Effect1Parameter + i * 2];

                    if (thisEffectType != WTPattern.EVENT_EMPTY) {
                        wroteAnEffect = true;
                        Write(Helpers.FlushString((char)thisEffectType + ""), x + 2, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect"], alpha));
                        if (Helpers.IsEffectHex((char)thisEffectType))
                            WriteMonospaced(thisEffectParameter.ToString("X2"), x + 7, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect parameter"], alpha), 4);
                        else
                            WriteMonospaced(thisEffectParameter.ToString("D2"), x + 7, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect parameter"], alpha), 4);
                        break;
                    }
                }
                if (!wroteAnEffect)
                    WriteMonospaced("···", x + 3, y, emptyColor, 4);
            }
            else {
                string noteName = Helpers.MIDINoteToText(noteValue);
                if (noteName.Contains('#')) {
                    Write(noteName, x + 2, y, noteColor);
                }
                else {
                    WriteMonospaced(noteName[0] + "-", x + 2, y, noteColor, 5);
                    Write(noteName[2] + "", x + 13, y, noteColor);
                }
            }

            // draw instrument column
            if (instrumentValue == WTPattern.EVENT_EMPTY) {
                if (volumeValue == WTPattern.EVENT_EMPTY) {
                    WriteMonospaced("··", x + 22, y, emptyColor, 4);
                }
                else {
                    WriteMonospaced(volumeValue.ToString("D2"), x + 21, y, Helpers.Alpha(App.Settings.Appearance.Theme["Volume"], alpha), 4);
                }
            }
            else {
                Color instrumentColor;
                if (instrumentValue < App.CurrentModule.Instruments.Count) {
                    if (App.CurrentModule.Instruments[instrumentValue] is WaveInstrument)
                        instrumentColor = App.Settings.Appearance.Theme["Instrument (wave)"];
                    else
                        instrumentColor = App.Settings.Appearance.Theme["Instrument (sample)"];
                }
                else {
                    instrumentColor = Color.Red;
                }
                WriteMonospaced(instrumentValue.ToString("D2"), x + 21, y, Helpers.Alpha(instrumentColor, alpha), 4);
            }
        }

        void DrawPatternEventExpanded(int frame, int row, int channel, int frameWrap, int x, int y, int effectColumns, int alpha) {

            int noteValue = App.CurrentSong[frame][row, channel, CellType.Note];
            int instrumentValue = App.CurrentSong[frame][row, channel, CellType.Instrument];
            int volumeValue = App.CurrentSong[frame][row, channel, CellType.Volume];

            Color emptyColor = App.Settings.Appearance.Theme["Row text"].MultiplyWith(App.Settings.Appearance.Theme["Empty dashes tint"]);
            Color noteColor = Helpers.Alpha(App.Settings.Appearance.Theme["Row text"], alpha);
            // draw note

            if (noteValue == WTPattern.EVENT_NOTE_CUT) {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText)
                    Write("OFF", x + 2, y, noteColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 2, noteColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_NOTE_RELEASE) {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText)
                    Write("REL", x + 2, y, noteColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 1, noteColor);
                    DrawRect(x + 3, y + 4, 13, 1, noteColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_EMPTY) {
                WriteMonospaced("···", x + 3, y, emptyColor, 4);
            }
            else {
                string noteName = Helpers.MIDINoteToText(noteValue);
                if (noteName.Contains('#')) {
                    Write(noteName, x + 2, y, noteColor);
                }
                else {
                    WriteMonospaced(noteName[0] + "-", x + 2, y, noteColor, 5);
                    Write(noteName[2] + "", x + 13, y, noteColor);
                }
            }

            // draw instrument column
            if (instrumentValue == WTPattern.EVENT_EMPTY) {
                WriteMonospaced("··", x + 22, y, emptyColor, 4);
            }
            else {
                Color instrumentColor;
                if (instrumentValue < App.CurrentModule.Instruments.Count) {
                    if (App.CurrentModule.Instruments[instrumentValue] is WaveInstrument)
                        instrumentColor = App.Settings.Appearance.Theme["Instrument (wave)"];
                    else
                        instrumentColor = App.Settings.Appearance.Theme["Instrument (sample)"];
                }
                else {
                    instrumentColor = Color.Red;
                }
                WriteMonospaced(instrumentValue.ToString("D2"), x + 21, y, Helpers.Alpha(instrumentColor, alpha), 4);
            }

            if (volumeValue == WTPattern.EVENT_EMPTY) {
                WriteMonospaced("··", x + 35, y, emptyColor, 4);
            }
            else {
                WriteMonospaced(volumeValue.ToString("D2"), x + 34, y, Helpers.Alpha(App.Settings.Appearance.Theme["Volume"], alpha), 4);
            }

            for (int i = 0; i < effectColumns; ++i) {
                int thisEffectType = App.CurrentSong[frame][row, channel, CellType.Effect1 + i * 2];
                int thisEffectParameter = App.CurrentSong[frame][row, channel, CellType.Effect1Parameter + i * 2];


                if (thisEffectType != WTPattern.EVENT_EMPTY) {
                    Write(Helpers.FlushString((char)thisEffectType + ""), x + 47, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect"], alpha));
                    if (Helpers.IsEffectHex((char)thisEffectType))
                        WriteMonospaced(thisEffectParameter.ToString("X2"), x + 52, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect parameter"], alpha), 4);
                    else
                        WriteMonospaced(thisEffectParameter.ToString("D2"), x + 52, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect parameter"], alpha), 4);
                    break;
                }
                else if (i == effectColumns - 1) {
                    WriteMonospaced("···", x + 48, y, emptyColor, 4);
                }
            }
        }

        void WriteNote(int value, int x, int y, bool currRow) {
            int alpha = currRow ? 255 : 120;
            Color c = Helpers.Alpha(App.Settings.Appearance.Theme["Row text"], currRow ? 255 : 120);
            if (value == WTPattern.EVENT_NOTE_CUT) // off
            {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText)
                    Write("OFF", x, y, c);
                else {
                    DrawRect(x + 1, y + 2, 13, 2, c);
                }
            }
            else if (value == WTPattern.EVENT_NOTE_RELEASE) // release 
              {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText)
                    Write("REL", x, y, c);
                else {
                    DrawRect(x + 1, y + 2, 13, 1, c);
                    DrawRect(x + 1, y + 4, 13, 1, c);
                }
            }
            else if (value == WTPattern.EVENT_EMPTY) // empty
            {
                WriteMonospaced("···", x + 1, y, currRow ? currRowEmptyText : App.Settings.Appearance.Theme["Row text"].MultiplyWith(App.Settings.Appearance.Theme["Empty dashes tint"]), 4);
            }
            else {
                string val = Helpers.MIDINoteToText(value);
                if (val.Contains('#')) {
                    Write(val, x, y, c);
                }
                else {
                    WriteMonospaced(val[0] + "-", x, y, c, 5);
                    Write(val[2] + "", x + 11, y, c);
                }

            }
        }

        void WriteInstrument(int value, int x, int y, bool currRow) {
            int alpha = currRow ? 255 : 120;
            if (value < 0) {
                WriteMonospaced("··", x + 1, y, currRow ? currRowEmptyText : App.Settings.Appearance.Theme["Row text"].MultiplyWith(App.Settings.Appearance.Theme["Empty dashes tint"]), 4);
            }
            else {
                if (value >= App.CurrentModule.Instruments.Count)
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Color.Red, alpha), 4);
                else if (App.CurrentModule.Instruments[value] is SampleInstrument)
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(App.Settings.Appearance.Theme["Instrument (sample)"], alpha), 4);
                else
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(App.Settings.Appearance.Theme["Instrument (wave)"], alpha), 4);

            }
        }

        void WriteVolume(int value, int x, int y, bool currRow) {
            if (value < 0) {
                WriteMonospaced("··", x + 1, y, currRow ? currRowEmptyText : App.Settings.Appearance.Theme["Row text"].MultiplyWith(App.Settings.Appearance.Theme["Empty dashes tint"]), 4);
            }
            else {
                int alpha = currRow ? 255 : 100;

                WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(App.Settings.Appearance.Theme["Volume"], alpha), 4);
            }
        }

        void WriteEffect(int value, int param, int x, int y, bool currRow) {
            int alpha = currRow ? 255 : 120;

            if (value < 0) {
                Write("·", x + 1, y, currRow ? currRowEmptyText : App.Settings.Appearance.Theme["Row text"].MultiplyWith(App.Settings.Appearance.Theme["Empty dashes tint"]));
            }
            else {
                Write("" + Helpers.GetEffectCharacter(value), x, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect"], alpha));
            }

            if (param < 0) {
                WriteMonospaced("··", x + 1 + 5, y, currRow ? currRowEmptyText : App.Settings.Appearance.Theme["Empty dashes"], 4);
            }
            else {
                if (Helpers.IsEffectHex((char)value))
                    WriteMonospaced(param.ToString("X2"), x + 5, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect parameter"], alpha), 4);
                else
                    WriteMonospaced(param.ToString("D2"), x + 5, y, Helpers.Alpha(App.Settings.Appearance.Theme["Effect parameter"], alpha), 4);
            }
        }

        void DrawBubbleRect(int x, int y, int w, int h, Color c) {
            DrawRect(x + 1, y, w - 2, h, c);
            DrawRect(x, y + 1, w, h - 1, c);
        }











        public void DrawOscilloscopes() {
            int numOscsX = 3;
            if (App.CurrentModule.ChannelCount <= 12)
                numOscsX = 2;
            if (App.CurrentModule.ChannelCount <= 6)
                numOscsX = 1;
            int numOscsY = (int)Math.Ceiling(App.CurrentModule.ChannelCount / (float)numOscsX);
            int oscsX = 628 * 2;
            int oscsY = 20 * 2 + 16;
            int oscsW = 636 / numOscsX;
            int oscsH = 2 * oscilloscopePanelHeight / numOscsY;




            //DrawRect(oscsX, oscsY, (oscsW + 2) * numOscsX + 2, (oscsH + 2) * numOscsY + 2, Color.White);
            for (int y = 0; y < numOscsY; ++y) {
                for (int x = 0; x < numOscsX; ++x) {
                    int channelNum = x + y * numOscsX + 1;
                    if (channelNum > App.CurrentModule.ChannelCount)
                        break;
                    DrawOscilloscope(channelNum, oscsX + x * (oscsW + 2) + 2, oscsY + y * (oscsH + 2) + 2, oscsW, oscsH);
                }
            }
        }

        public void DrawOscilloscope(int channelNum, int px, int py, int w, int h) {

            if (App.Settings.Visualizer.OscilloscopeBorders)
                DrawRect(px - 2, py - 2, w + 4, h + 4, Color.White);
            Color crossColor = new Color(44, 53, 77);
            DrawRect(px, py, w, h, new Color(20, 24, 46));

            if (App.Settings.Visualizer.OscilloscopeCrosshairs > 0) {
                DrawRect(px, py + h / 2, w, 1, crossColor);
                if (App.Settings.Visualizer.OscilloscopeCrosshairs > 1)
                    DrawRect(px + w / 2, py, 1, h, crossColor);
            }
            if (App.Settings.Visualizer.OscilloscopeBorders)
                WriteTwiceAsBig("" + channelNum, px + 2, py - 4, new Color(126, 133, 168));

            Channel channel = ChannelManager.channels[channelNum - 1];
            float samp1 = 0;
            float lastSamp = 0;
            float scopezoom = 40f / (App.Settings.Visualizer.OscilloscopeZoom / 100f);
            if (ChannelManager.IsChannelOn(channelNum - 1)) {
                if (channel.currentInstrument is SampleInstrument instrument) {
                    Sample samp = instrument.sample;

                    for (int i = -w / 2; i < w / 2 - 1; ++i) {
                        lastSamp = samp1;

                        // time per base note cycle
                        // quantized 

                        samp1 = -samp.GetMonoSample((i / (float)w * channel.CurrentFrequency / scopezoom) + (int)channel.SampleTime, channel.SampleStartOffset / 100f) * (h / 2f) * channel.CurrentAmplitudeAsWave / 1.5f + (h / 2f);
                        if (i > -w / 2)
                            DrawOscCol(px + i + w / 2, py - 2, samp1, lastSamp, Color.White, App.Settings.Visualizer.OscilloscopeThickness + 1);
                    }
                }
                else {
                    // WAVE
                    for (int i = -w / 2; i < w / 2 - 1; ++i) {
                        lastSamp = samp1;
                        float position = (i / (float)w * channel.CurrentFrequency / scopezoom);

                        samp1 = -channel.EvaluateWave(position + 5) * (h / 2f) * channel.CurrentAmplitude + (h / 2f);
                        if (i > -w / 2)
                            DrawOscCol(px + i + w / 2, py - 2, samp1, lastSamp, App.Settings.Visualizer.OscilloscopeColorfulWaves ? GetColorOfWaveFromTable(channel.WaveIndex, channel.WaveMorphPosition) : Color.White, App.Settings.Visualizer.OscilloscopeThickness + 1);
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
            int px = -80;
            int py = 24 * 2;

            if (App.Settings.Visualizer.HighlightPressedKeys && states.Count > 0) {
                for (int i = states[0].Count - 1; i >= 0; i--) {
                    if (states[0][i].isPlaying) {
                        int psy = y + py;
                        int pitch = (int)(states[0][i].pitch + 0.5f);
                        int psheight = 31;
                        int pswidth = 8;
                        int wo = 1;
                        int alpha = (int)states[0][i].volume.Map(0, 1, 60, 255);
                        if (!App.Settings.Visualizer.ChangeNoteOpacityByVolume)
                            alpha = 255;
                        if (!Helpers.IsNoteBlackKey(pitch)) {
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
                    if (!App.Settings.Visualizer.ChangeNoteWidthByVolume)
                        width = 10;
                    int alpha = (int)state.volume.Map(0, 1, 10, 255);
                    if (!App.Settings.Visualizer.ChangeNoteOpacityByVolume)
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
