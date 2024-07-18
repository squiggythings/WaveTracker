using System;
using System.Collections.Generic;
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
        static Color[] waveColors;
        public Visualizer() {
            x = 0;
            y = 0;
            piano = new Piano(20, 50, 120 * 6, 120 * 3, this);
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
                    waveColors[i] = GetColorFromWave(w);
                ++i;
            }
        }
        static Color GetColorOfWaveFromTable(int index, float morph) {
            return waveColors[index].Lerp(waveColors[(index + 1) % 100], morph);
        }
        Color GetColorFromWave(Wave w) {
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
            width = App.WindowWidth;
            height = App.WindowHeight;
        }

        public void Draw() {
            //DrawRect(0, 0, width, height, new Color(255, 0, 0, 128));
            piano.Draw();
        }

        public void RecordChannelStates() {
            piano.RecordChannelStates(piano.states);
        }



        public class Piano : Clickable {

            public List<ChannelState[]> states;
            int maxNoteWidth => width / 120;

            public Piano(int x, int y, int width, int height, Element parent) {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                SetParent(parent);

                states = new List<ChannelState[]>();
            }

            public void RecordChannelStates(List<ChannelState[]> states) {
                ChannelState[] rowOfStates = new ChannelState[ChannelManager.channels.Count];
                for (int c = 0; c < ChannelManager.channels.Count; c++) {
                    Channel chan = ChannelManager.channels[c];

                    if ((chan.currentInstrument is WaveInstrument || (chan.currentInstrument is SampleInstrument inst && inst.sample.useInVisualization)) && chan.CurrentAmplitude > 0.01f && chan.CurrentPitch >= 12 && chan.CurrentPitch < 132 && ChannelManager.IsChannelOn(c)) {
                        ChannelState state = new ChannelState(chan.CurrentPitch, chan.CurrentAmplitude, chan.currentInstrument is WaveInstrument ? GetColorOfWaveFromTable(chan.WaveIndex, chan.WaveMorphPosition) : Color.White, chan.IsPlaying);
                        rowOfStates[c] = state;
                    }
                }
                Array.Sort(rowOfStates);
                if (states.Count > 0)
                    states.Insert(0, rowOfStates);
                else
                    states.Add(rowOfStates);
                //while (states.Count > height - 24)
                //    states.RemoveAt(states.Count - 1);
            }

            public void Draw() {
                //RecordChannelStates(statesPrevious);
                for (int i = 0; i < 10; ++i) {
                    DrawSprite(i * maxNoteWidth * 12, 0, maxNoteWidth * 12, 24, new Rectangle(0, 104, 60, 24), Color.White);
                }
                for (int i = 0; i < height - 24; ++i) {
                    if (i < states.Count) {
                        for (int j = 0; j < App.CurrentModule.ChannelCount; ++j) {
                        }
                    }
                }
                //DrawRect((int)((ChannelManager.channels[0].CurrentPitch - 12) * notewidth), 24, notewidth, 24, Color.Beige);
                while (states.Count > height - 24) {
                    states.RemoveAt(states.Count - 1);
                }

                for (int i = 0; i < states.Count; i++) {
                    if (states.Count <= i)
                        return;
                    foreach (ChannelState state in states[i]) {
                        if (state != null && state.isOn) {
                            int noteWidth;
                            if (App.Settings.Visualizer.ChangeNoteWidthByVolume)
                                noteWidth = (int)state.volume.Map(0, 1, 1, maxNoteWidth + 2);
                            else
                                noteWidth = maxNoteWidth;
                            DrawRect((int)((state.pitch - 12) * maxNoteWidth), i + 24, (int)((maxNoteWidth + 2) * state.volume + 0.5f), 1, state.GetColor(App.Settings.Visualizer.ChangeNoteOpacityByVolume));
                        }
                    }
                }
            }
        }

        public record ChannelState : IComparable {
            public float pitch;
            public float volume;
            Color color;
            public bool isOn;

            public ChannelState(float pitch, float volume, Color color, bool isPlaying) {
                this.pitch = pitch;
                this.volume = volume;
                this.color = color;
                this.isOn = isPlaying;
            }

            public Color GetColor(bool withVolume) {
                if (withVolume) {
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
}
