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
        static Color[] waveColors;
        public Visualizer() {
            x = 0;
            y = 0;
            piano = new Piano(20, 50, 120 * 2, 120 * 2, this);
            oscilloscopeGrid = new OscilloscopeGrid(this);
        }

        public void GetWaveColors() {
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
        Color GenerateColorFromWave(Wave w) {
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
            int scale = App.Settings.Visualizer.DrawInHighResolution ? 2 : 1;
            x = 0;
            y = (App.MENUSTRIP_HEIGHT + 15) * scale;
            width = App.Settings.Visualizer.DrawInHighResolution ? App.ClientWindow.ClientBounds.Width : App.WindowWidth;
            height = (App.Settings.Visualizer.DrawInHighResolution ? App.ClientWindow.ClientBounds.Height : App.WindowHeight) - y - 9 * scale;
            piano.x = 20 * scale;
            piano.y = 10 * scale;
            piano.width = (int)((width - 20) / 1.5f);
            piano.height = (int)(height * 0.75f);
            oscilloscopeGrid.x = piano.BoundsRight + 5 * scale;
            oscilloscopeGrid.y = piano.y;
            oscilloscopeGrid.width = (width - piano.BoundsRight - 25);
            oscilloscopeGrid.height = piano.height;
            oscilloscopeGrid.Update();
        }

        public void Draw() {
            //DrawRect(0, 0, width, height, new Color(255, 0, 0, 128));
            DrawRect(0, 0, width, height, Color.Black);
            piano.Draw();
            oscilloscopeGrid.Draw();
        }

        public void RecordChannelStates() {
            piano.RecordChannelStates();
        }



        public class Piano : Clickable {

            ChannelState[][] channelStates;
            int PianoNoteWidth => width / 120;
            int PianoHeight => 24 * (PianoNoteWidth * 120) / 600;
            int drawIndex;
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

                    if ((chan.currentInstrument is WaveInstrument || (chan.currentInstrument is SampleInstrument inst && inst.sample.useInVisualization)) && chan.CurrentAmplitude > 0.01f && chan.CurrentPitch >= 12 && chan.CurrentPitch < 132 && ChannelManager.IsChannelOn(c)) {
                        channelStates[writeIndex][c].Set(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentInstrument is WaveInstrument ? GetColorOfWaveFromTable(chan.WaveIndex, chan.WaveMorphPosition) : Color.White, chan.IsPlaying);
                    }
                    else {
                        channelStates[writeIndex][c].Clear();
                    }
                }
                chan = ChannelManager.previewChannel;
                channelStates[writeIndex][24].Set(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentInstrument is WaveInstrument ? GetColorOfWaveFromTable(chan.WaveIndex, chan.WaveMorphPosition) : Color.White, chan.IsPlaying);
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
                        if (state.isPlaying) {
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
                        if (state != null && state.isPlaying) {
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
                public bool isPlaying;

                /// <summary>
                /// Sets the state's values
                /// </summary>
                /// <param name="pitch"></param>
                /// <param name="volume"></param>
                /// <param name="color"></param>
                /// <param name="isPlaying"></param>
                public void Set(float pitch, float volume, Color color, bool isPlaying) {
                    this.pitch = pitch;
                    this.volume = volume;
                    this.color = color;
                    this.isPlaying = isPlaying;
                }

                /// <summary>
                /// Clears the state
                /// </summary>
                public void Clear() {
                    this.pitch = 0;
                    this.volume = 0;
                    this.isPlaying = false;
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
                        DrawRect(-1, -1, width + 2, height + 2, Color.White);
                    }
                    DrawRect(0, 0, width, height, UIColors.black);
                    if (App.Settings.Visualizer.OscilloscopeCrosshairs > 0) {
                        DrawRect(0, height / 2, width, 1, new Color(44, 53, 77));
                        if (App.Settings.Visualizer.OscilloscopeCrosshairs > 1)
                            DrawRect(width / 2, 0, 1, height, new Color(44, 53, 77));
                    }
                    if (App.Settings.Visualizer.DrawInHighResolution) {
                        WriteTwiceAsBig(channelID + 1 + "", 6, 4, new Color(126, 133, 168));
                    }
                    else {
                        Write(channelID + 1 + "", 3, 2, new Color(126, 133, 168));
                    }
                    if (channel.IsMuted)
                        return;
                    float scopezoom = 80f / (100f / App.Settings.Visualizer.OscilloscopeZoom);
                    Color waveColor = App.Settings.Visualizer.OscilloscopeColorfulWaves ? GetColorOfWaveFromTable(channel.WaveIndex, channel.WaveMorphPosition) : Color.White;

                    float position = -width / 2 / width * channel.CurrentFrequency / scopezoom;
                    float sample = 0;

                    float lastSample = sample;
                    bool first = true;

                    if (channel.currentInstrument is SampleInstrument instrument) {
                        Sample audioSample = instrument.sample;
                        for (float i = -width / 2; i < width / 2; i += 0.0625f) {
                            position = i / width * channel.CurrentFrequency / scopezoom;
                            sample = -audioSample.GetMonoSample((i / width * channel.CurrentFrequency / scopezoom) + (int)channel.SampleTime, channel.SampleStartOffset / 100f) * (height / 2f) * channel.CurrentAmplitudeAsWave / 1.5f + (height / 2f);
                            if (first) {
                                lastSample = sample;
                                first = false;
                            }
                            DrawOscCol((int)Math.Round(i + width / 2), 0, lastSample, sample, Color.White, App.Settings.Visualizer.OscilloscopeThickness + 1);
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
                            DrawOscCol((int)Math.Round(i + width / 2), 0, lastSample, sample, waveColor, App.Settings.Visualizer.OscilloscopeThickness + 1);
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

        public class OscilloscopeGrid : Clickable {
            public Oscilloscope[] oscilloscopes;
            public OscilloscopeGrid(Element parent) {
                SetParent(parent);
                oscilloscopes = new Oscilloscope[ChannelManager.channels.Count];
                for (int i = 0; i < ChannelManager.channels.Count; ++i) {
                    oscilloscopes[i] = new Oscilloscope(i, this);
                }
            }
            public void Update() {
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
                        oscilloscopes[oscNum].width = width / numOscsX - 1;
                        oscilloscopes[oscNum].y = (height / numOscsY) * y;
                        oscilloscopes[oscNum].height = height / numOscsY - 1;
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
        }
    }
}
