using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class Visualizer : Clickable {
        Piano piano;
        OscilloscopeGrid oscilloscopeGrid;
        MiniTrackerLayout miniTrackerLayout;
        static Color[] waveColors;
        public Visualizer() {
            x = 0;
            y = 0;
            piano = new Piano(20, 50, 120 * 2, 120 * 2, this);
            oscilloscopeGrid = new OscilloscopeGrid(this);
            miniTrackerLayout = new MiniTrackerLayout(null);
        }

        public static void GenerateWaveColors() {
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
                    waveColors[i] = GenerateColorFromWave(w);
                ++i;
            }
        }
        static Color GetColorOfWaveFromTable(int index, float morph) {
            if (morph > 0.001f)
                return waveColors[index].Lerp(waveColors[(index + 1) % 100], morph);
            else
                return waveColors[index];
        }
        static Color GenerateColorFromWave(Wave w) {
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
            h = (float)(dcOffset * 0.8 - 55 + (brightness * 5 % 100));
            l = (float)Math.Clamp(100 - ((brightness + directionChanges) / 1.2) - 2, 50, 100) / 100f;
            h = Helpers.Mod(h * 3.6f, 360);

            return Helpers.HSLtoRGB((int)h, 1 - (l - 0.5f) * 2, l);
        }
        public void Update() {

            float pianoHeightRatio = 0.78f;
            float pianoWidthRatio = 0.65f;

            int unscaledWidth = App.WindowWidth;
            int unscaledHeight = App.WindowHeight - (App.MENUSTRIP_HEIGHT + 15) - 9;
            miniTrackerLayout.x = 0;
            miniTrackerLayout.width = unscaledWidth;
            miniTrackerLayout.y = App.MENUSTRIP_HEIGHT + 35 + (int)Math.Ceiling((unscaledHeight - 20) * pianoHeightRatio);
            miniTrackerLayout.height = (int)Math.Ceiling((unscaledHeight - 18) * (1 - pianoHeightRatio));

            int scale = App.Settings.Visualizer.DrawInHighResolution ? App.Settings.General.ScreenScale : 1;
            x = 0;
            y = (App.MENUSTRIP_HEIGHT + 15) * scale;
            width = App.Settings.Visualizer.DrawInHighResolution ? App.ClientWindow.ClientBounds.Width : App.WindowWidth;
            height = (App.Settings.Visualizer.DrawInHighResolution ? App.ClientWindow.ClientBounds.Height : App.WindowHeight) - y - 9 * scale;
            piano.x = 10 * scale;
            piano.y = 10 * scale;
            piano.width = (int)((width - 20) * pianoWidthRatio);
            piano.height = (int)((height - 20 * scale) * pianoHeightRatio);
            oscilloscopeGrid.x = piano.BoundsRight + 5 * scale;
            oscilloscopeGrid.y = piano.y;
            oscilloscopeGrid.width = (width - piano.BoundsRight - 15 * scale);
            oscilloscopeGrid.height = piano.height;
            oscilloscopeGrid.Update();



        }

        public void DrawPianoAndScopes() {
            piano.Draw();
            oscilloscopeGrid.Draw();

        }

        public void DrawTracker() {
            miniTrackerLayout.Draw();
        }

        public void RecordChannelStates() {
            piano.RecordChannelStates();
        }



        public class Piano : Clickable {

            ChannelState[][] channelStates;
            int PianoNoteWidth => width / 120;
            int PianoHeight => 24 * PianoNoteWidth * 120 / 600;
            int writeIndex;

            public Piano(int x, int y, int width, int height, Element parent) {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;

                channelStates = new ChannelState[1000][];
                for (int i = 0; i < channelStates.Length; ++i) {
                    channelStates[i] = new ChannelState[25];
                    for (int j = 0; j < channelStates[i].Length; ++j) {
                        channelStates[i][j] = new ChannelState();
                    }
                }
                SetParent(parent);
            }

            /// <summary>
            /// Records the state of every channel into the buffer, and moves the write index forward 1 step
            /// </summary>
            public void RecordChannelStates() {
                Channel chan;
                for (int c = 0; c < ChannelManager.channels.Count; c++) {
                    chan = ChannelManager.channels[c];

                    if ((chan.currentInstrument is WaveInstrument || (chan.currentInstrument is SampleInstrument inst && inst.sample.useInVisualization)) && chan.CurrentAmplitude > 0.0001f && chan.CurrentPitch >= 12 && chan.CurrentPitch < 132 && ChannelManager.IsChannelOn(c)) {
                        channelStates[writeIndex][c].Set(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentInstrument is WaveInstrument ? GetColorOfWaveFromTable(chan.WaveIndex, chan.WaveMorphPosition) : Color.White);
                    }
                    else {
                        channelStates[writeIndex][c].Clear();
                    }
                }
                chan = ChannelManager.previewChannel;
                if (chan.CurrentAmplitude > 0.0001f) {
                    channelStates[writeIndex][24].Set(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentInstrument is WaveInstrument ? GetColorOfWaveFromTable(chan.WaveIndex, chan.WaveMorphPosition) : Color.White);
                }
                else {
                    channelStates[writeIndex][24].Clear();
                }
                Array.Sort(channelStates[writeIndex]);
                writeIndex++;
                if (writeIndex >= channelStates.GetLength(0)) {
                    writeIndex = 0;
                }

                //while (states.Count > height - 24)
                //    states.RemoveAt(states.Count - 1);
            }

            public void Draw() {
                // draw piano graphic
                for (int i = 0; i < 10; ++i) {
                    if (App.Settings.Visualizer.HighlightPressedKeys) {
                        DrawSprite(i * PianoNoteWidth * 12, 0, PianoNoteWidth * 12, PianoHeight, new Rectangle(0, 104, 60, 24), new Color(128, 128, 128, 128));
                    }
                    else {
                        DrawSprite(i * PianoNoteWidth * 12, 0, PianoNoteWidth * 12, PianoHeight, new Rectangle(0, 104, 60, 24));
                    }
                }

                // draw highlighted keys
                int drawIndex = writeIndex - 1;
                if (drawIndex < 0) {
                    drawIndex = channelStates.Length - 1;
                }
                if (App.Settings.Visualizer.HighlightPressedKeys) {
                    for (int c = 0; c < channelStates[drawIndex].Length; ++c) {
                        ChannelState state = channelStates[drawIndex][c];
                        if (state != null) {
                            int note = (int)(state.pitch + 0.5f);
                            int height = PianoHeight;
                            if (Helpers.IsNoteBlackKey(note)) {
                                height = (int)(height / 1.5f);
                            }
                            DrawRect((note - 12) * PianoNoteWidth, 0, PianoNoteWidth, height, Helpers.Alpha(state.GetColor(false), state.volume.Map(0, 1, 0.24f, 1f)));
                        }
                    }
                }

                // draw note cascade
                for (int i = 0; i < height - PianoHeight; ++i) {
                    for (int c = 0; c < channelStates[drawIndex].Length; ++c) {
                        ChannelState state = channelStates[drawIndex][c];
                        if (state != null) {
                            int noteWidth = PianoNoteWidth;
                            if (App.Settings.Visualizer.ChangeNoteWidthByVolume) {
                                noteWidth = (int)state.volume.Map(0, 1, 1, PianoNoteWidth + 2);
                            }
                            int noteXOffset = (PianoNoteWidth - noteWidth) / 2;
                            DrawRect((int)((state.pitch - 12) * PianoNoteWidth) + noteXOffset, i + PianoHeight + 1, noteWidth, 1, state.GetColor(App.Settings.Visualizer.ChangeNoteOpacityByVolume));
                        }
                    }
                    drawIndex--;
                    if (drawIndex < 0) {
                        drawIndex = channelStates.Length - 1;
                    }
                }
            }
            /// <summary>
            /// Holds basic information about a channels current state: pitch, volume, noteOn and its wave color
            /// </summary>
            public class ChannelState : IComparable {
                public float pitch;
                public float volume;
                Color color;

                /// <summary>
                /// Sets the state's values
                /// </summary>
                /// <param name="pitch"></param>
                /// <param name="volume"></param>
                /// <param name="color"></param>
                /// <param name="isPlaying"></param>
                public void Set(float pitch, float volume, Color color) {
                    this.pitch = pitch;
                    this.volume = volume;
                    this.color = color;
                }

                /// <summary>
                /// Clears the state
                /// </summary>
                public void Clear() {
                    this.pitch = 0;
                    this.volume = 0;
                }

                /// <summary>
                /// Gets the color of the wave
                /// </summary>
                /// <param name="withVolume">if enabled: will fade the color according to the volume</param>
                /// <returns></returns>
                public Color GetColor(bool withVolume) {
                    if (withVolume && color.A != 0) {
                        return Helpers.Alpha(color, (int)volume.Map(0, 1, 10, 255));
                    }
                    else {
                        return color;
                    }
                }

                public int CompareTo(object other) {
                    if (other is ChannelState state) {
                        if (this.volume > state.volume)
                            return -1;
                        if (this.volume < state.volume)
                            return 1;
                    }
                    return 0;
                }
            }

        }
        public class OscilloscopeGrid : Clickable {
            public Oscilloscope[] oscilloscopes;
            static int borderWidth = 1;

            public OscilloscopeGrid(Element parent) {
                SetParent(parent);
                oscilloscopes = new Oscilloscope[ChannelManager.channels.Count];
                for (int i = 0; i < ChannelManager.channels.Count; ++i) {
                    oscilloscopes[i] = new Oscilloscope(i, this);
                }
            }
            public void Update() {
                if (App.Settings.Visualizer.DrawInHighResolution)
                    borderWidth = 2;
                else
                    borderWidth = 1;
                int numOscsX = 3;
                if (App.CurrentModule.ChannelCount <= 12)
                    numOscsX = 2;
                if (App.CurrentModule.ChannelCount <= 6)
                    numOscsX = 1;
                int numOscsY = (int)Math.Ceiling(App.CurrentModule.ChannelCount / (float)numOscsX);

                int oscNum = 0;
                foreach (Oscilloscope osc in oscilloscopes) {
                    osc.enabled = false;
                }
                for (int y = 0; y < numOscsY; y++) {
                    for (int x = 0; x < numOscsX; x++) {
                        oscilloscopes[oscNum].x = (width / numOscsX) * x;
                        oscilloscopes[oscNum].width = width / numOscsX - borderWidth;
                        oscilloscopes[oscNum].y = (height / numOscsY) * y;
                        oscilloscopes[oscNum].height = height / numOscsY - borderWidth;
                        oscilloscopes[oscNum].enabled = oscNum < App.CurrentModule.ChannelCount;
                        oscNum++;
                    }
                }
            }
            public void Draw() {
                foreach (Oscilloscope oscilloscope in oscilloscopes) {
                    oscilloscope.Draw();
                }
            }


            public class Oscilloscope : Clickable {
                int channelID;
                Channel channel => ChannelManager.channels[channelID];
                public Oscilloscope(int channelID, Element parent) {
                    this.channelID = channelID;
                    width = 2;
                    height = 2;
                    SetParent(parent);
                }

                public void Draw() {
                    if (enabled) {
                        if (App.Settings.Visualizer.OscilloscopeBorders) {
                            DrawRect(-borderWidth, -borderWidth, width + borderWidth * 2, height + borderWidth * 2, Color.White);
                        }
                        DrawRect(0, 0, width, height, UIColors.black);
                        if (App.Settings.Visualizer.OscilloscopeCrosshairs > 0) {
                            DrawRect(0, height / 2, width, 1, new Color(44, 53, 77));
                            if (App.Settings.Visualizer.OscilloscopeCrosshairs > 1)
                                DrawRect(width / 2, 0, 1, height, new Color(44, 53, 77));
                        }
                        Color labelColor = channel.IsMuted ? new Color(126, 133, 168, 128) : new Color(126, 133, 168);
                        if (App.Settings.Visualizer.DrawInHighResolution) {
                            WriteTwiceAsBig(channelID + 1 + "", 4, -2, labelColor);
                        }
                        else {
                            Write(channelID + 1 + "", 2, 1, labelColor);

                        }
                        if (channel.IsMuted)
                            return;
                        float scopezoom = 80f / (100f / App.Settings.Visualizer.OscilloscopeZoom);
                        Color waveColor = App.Settings.Visualizer.OscilloscopeColorfulWaves ? GetColorOfWaveFromTable(channel.WaveIndex, channel.WaveMorphPosition) : Color.White;

                        float position;
                        float sample = 0;

                        float lastSample = sample;
                        bool first = true;

                        if (channel.currentInstrument is SampleInstrument instrument) {
                            Sample audioSample = instrument.sample;
                            for (float i = -width / 2; i < width / 2; i += 0.0625f) {
                                sample = -audioSample.GetMonoSample((i / width * channel.CurrentFrequency / scopezoom) + (int)channel.SampleTime, channel.SampleStartOffset / 100f) * (height / 2f) * channel.CurrentAmplitudeAsWave / 1.5f + (height / 2f);
                                if (first) {
                                    lastSample = sample;
                                    first = false;
                                }
                                int px = (int)Math.Round(i + width / 2);
                                if (px <= width - 1) {
                                    DrawOscCol(px, 0, lastSample, sample, Color.White, App.Settings.Visualizer.OscilloscopeThickness + 1);
                                }
                                lastSample = sample;
                            }
                        }
                        else {
                            for (float i = -width / 2; i < width / 2; i += 0.0625f) {
                                position = i / width * channel.CurrentFrequency / scopezoom;
                                sample = (-channel.EvaluateWave(position) * channel.CurrentAmplitude * 0.5f + 0.5f) * height;
                                if (first) {
                                    lastSample = sample;
                                    first = false;
                                }
                                int px = (int)Math.Round(i + width / 2);
                                if (px <= width - 1) {
                                    DrawOscCol(px, 0, lastSample, sample, waveColor, App.Settings.Visualizer.OscilloscopeThickness + 1);
                                }
                                lastSample = sample;
                            }
                        }
                    }
                }
                void DrawOscCol(int x, int y, float min, float max, Color c, int size) {
                    if (min < max) {
                        DrawRect(x, y + (int)min, size, (int)(max - min) + size, c);
                    }
                    else {
                        DrawRect(x, y + (int)max, size, (int)(min - max) + size, c);
                    }
                }
            }
        }

        public class MiniTrackerLayout : Clickable {
            const int LINE_HEIGHT = 7;
            const int HEADER_HEIGHT = 18;
            int NumVisibleRows => (height - HEADER_HEIGHT) / LINE_HEIGHT;

            public MiniTrackerLayout(Element parent) {
                SetParent(parent);
            }

            public void Draw() {
                DrawRect(0, 0, width, height, App.Settings.Colors.Theme["Row background"]);
                int channelWidth = 65 * App.CurrentModule.ChannelCount >= width ? 35 : 65;

                int trackerWidth = App.CurrentModule.ChannelCount * channelWidth;
                int px = (width - trackerWidth) / 2;

                DrawBubbleRect(-1, 0, px, HEADER_HEIGHT, Color.White);

                DrawRect(px - 1, 0, 1, height, App.Settings.Colors.Theme["Channel separator"]);
                for (int i = 0; i < App.CurrentModule.ChannelCount; ++i) {
                    DrawBubbleRect(px + i * channelWidth, 0, channelWidth - 1, HEADER_HEIGHT, Color.White);

                    string str;
                    if (channelWidth == 35) {
                        str = "Ch " + (i + 1).ToString("D2");
                    }
                    else {
                        str = "Channel " + (i + 1).ToString("D2");
                    }

                    Color textColor = ChannelManager.channels[i].IsMuted ? new Color(230, 69, 57) : UIColors.label;

                    Write(str, px + i * channelWidth + channelWidth / 2 - Helpers.GetWidthOfText(str) / 2 - 1, 3, textColor);
                    int volumeStartX = px + i * channelWidth + 2;
                    int maxVolumeWidth = channelWidth - 1 - 4;
                    DrawRect(volumeStartX, 12, maxVolumeWidth, 3, UIColors.panel);

                    // draw volume amp
                    int amp = (int)(maxVolumeWidth / 2 * App.PatternEditor.ChannelHeaders[i].Amplitude);
                    Color volumeColor = ChannelManager.channels[i].IsMuted ? UIColors.labelLight : new Color(63, 215, 52);
                    DrawRect(volumeStartX + maxVolumeWidth / 2 - amp, 12, amp * 2, 3, volumeColor);


                    // draw row separator
                    DrawRect(px + (i + 1) * channelWidth - 1, 0, 1, height, App.Settings.Colors.Theme["Channel separator"]);
                }
                DrawBubbleRect(px + trackerWidth, 0, width - (px + trackerWidth) + 1, HEADER_HEIGHT, Color.White);


                for (int i = 0; i < NumVisibleRows; ++i) {
                    int rowY = HEADER_HEIGHT + i * LINE_HEIGHT;
                    int thisRow = Playback.position.Row + i - NumVisibleRows / 2;
                    if (thisRow == Playback.position.Row) {
                        DrawRect(px - 1, rowY, trackerWidth + 1, LINE_HEIGHT, Helpers.Alpha(App.Settings.Colors.Theme["Cursor"], 90));
                    }

                    if (thisRow >= 0 && thisRow < Playback.Frame.GetLength())
                        DrawRow(px, rowY, Playback.position.Frame, thisRow, thisRow == Playback.position.Row, channelWidth);
                }
            }

            void DrawRow(int x, int y, int frame, int row, bool currRow, int channelWidth) {
                // draw pattern events
                for (int channel = 0; channel < App.CurrentModule.ChannelCount; ++channel) {
                    if (channelWidth == 35) {
                        DrawPatternEventCompact(frame, row, channel, x + channel * channelWidth + 1, y, App.CurrentSong.NumEffectColumns[channel], currRow ? 255 : 120);
                    }
                    else {
                        DrawPatternEventExpanded(frame, row, channel, x + channel * channelWidth + 1, y, App.CurrentSong.NumEffectColumns[channel], currRow ? 255 : 120);
                    }
                }
            }

            void DrawPatternEventCompact(int frame, int row, int channel, int x, int y, int effectColumns, int alpha) {

                int noteValue = App.CurrentSong[frame][row, channel, CellType.Note];
                int instrumentValue = App.CurrentSong[frame][row, channel, CellType.Instrument];
                int volumeValue = App.CurrentSong[frame][row, channel, CellType.Volume];

                Color emptyColor = App.Settings.Colors.Theme["Row text"].MultiplyWith(App.Settings.Colors.Theme["Empty dashes tint"]);
                Color noteColor = Helpers.Alpha(App.Settings.Colors.Theme["Row text"], alpha);
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
                            Write(Helpers.FlushString((char)thisEffectType + ""), x + 2, y, Helpers.Alpha(App.Settings.Colors.Theme["Effect"], alpha));
                            if (Helpers.IsEffectHex((char)thisEffectType))
                                WriteMonospaced(thisEffectParameter.ToString("X2"), x + 7, y, Helpers.Alpha(App.Settings.Colors.Theme["Effect parameter"], alpha), 4);
                            else
                                WriteMonospaced(thisEffectParameter.ToString("D2"), x + 7, y, Helpers.Alpha(App.Settings.Colors.Theme["Effect parameter"], alpha), 4);
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
                        WriteMonospaced(volumeValue.ToString("D2"), x + 21, y, Helpers.Alpha(App.Settings.Colors.Theme["Volume"], alpha), 4);
                    }
                }
                else {
                    Color instrumentColor;
                    if (instrumentValue < App.CurrentModule.Instruments.Count) {
                        if (App.CurrentModule.Instruments[instrumentValue] is WaveInstrument)
                            instrumentColor = App.Settings.Colors.Theme["Instrument (wave)"];
                        else
                            instrumentColor = App.Settings.Colors.Theme["Instrument (sample)"];
                    }
                    else {
                        instrumentColor = Color.Red;
                    }
                    WriteMonospaced(instrumentValue.ToString("D2"), x + 21, y, Helpers.Alpha(instrumentColor, alpha), 4);
                }
            }

            void DrawPatternEventExpanded(int frame, int row, int channel, int x, int y, int effectColumns, int alpha) {

                int noteValue = App.CurrentSong[frame][row, channel, CellType.Note];
                int instrumentValue = App.CurrentSong[frame][row, channel, CellType.Instrument];
                int volumeValue = App.CurrentSong[frame][row, channel, CellType.Volume];

                Color emptyColor = App.Settings.Colors.Theme["Row text"].MultiplyWith(App.Settings.Colors.Theme["Empty dashes tint"]);
                Color noteColor = Helpers.Alpha(App.Settings.Colors.Theme["Row text"], alpha);
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
                            instrumentColor = App.Settings.Colors.Theme["Instrument (wave)"];
                        else
                            instrumentColor = App.Settings.Colors.Theme["Instrument (sample)"];
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
                    WriteMonospaced(volumeValue.ToString("D2"), x + 34, y, Helpers.Alpha(App.Settings.Colors.Theme["Volume"], alpha), 4);
                }

                for (int i = 0; i < effectColumns; ++i) {
                    int thisEffectType = App.CurrentSong[frame][row, channel, CellType.Effect1 + i * 2];
                    int thisEffectParameter = App.CurrentSong[frame][row, channel, CellType.Effect1Parameter + i * 2];


                    if (thisEffectType != WTPattern.EVENT_EMPTY) {
                        Write(Helpers.FlushString((char)thisEffectType + ""), x + 47, y, Helpers.Alpha(App.Settings.Colors.Theme["Effect"], alpha));
                        if (Helpers.IsEffectHex((char)thisEffectType))
                            WriteMonospaced(thisEffectParameter.ToString("X2"), x + 52, y, Helpers.Alpha(App.Settings.Colors.Theme["Effect parameter"], alpha), 4);
                        else
                            WriteMonospaced(thisEffectParameter.ToString("D2"), x + 52, y, Helpers.Alpha(App.Settings.Colors.Theme["Effect parameter"], alpha), 4);
                        break;
                    }
                    else if (i == effectColumns - 1) {
                        WriteMonospaced("···", x + 48, y, emptyColor, 4);
                    }
                }
            }

           

            void DrawBubbleRect(int x, int y, int w, int h, Color c) {
                DrawRect(x + 1, y, w - 2, h, c);
                DrawRect(x, y + 1, w, h - 1, c);
            }

        }
    }
}