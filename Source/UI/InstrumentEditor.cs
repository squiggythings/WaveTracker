using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Xml.Schema;

namespace WaveTracker.UI {
    public class InstrumentEditor : Window {
        public bool IsOpen { get { return windowIsOpen; } }

        int currentInstrumentID;
        Instrument CurrentInstrument => App.CurrentModule.Instruments[currentInstrumentID];

        //public Button sample_importSample, sample_normalize, sample_reverse, sample_fadeIn, sample_fadeOut, sample_amplifyUp, sample_amplifyDown, sample_invert;
        //public NumberBox sample_loopPoint;
        //public NumberBox sample_baseKey, sample_detune;
        //public Toggle sample_loopOneshot, sample_loopForward, sample_loopPingpong;
        public EnvelopeEditor envelopeEditor;
        public SampleEditor sampleEditor;
        public EnvelopeListBox envelopeList;
        //public Checkbox visualize_toggle;

        //public Dropdown sample_resampleDropdown;
        // public Dropdown wave_modTypeDropdown;
        TabGroup tabGroup;

        public InstrumentEditor() : base("Instrument Editor", 600, 340) {
            ExitButton.SetTooltip("Close", "Close instrument editor");
            sampleEditor = new SampleEditor(16, 36, this);
            envelopeEditor = new EnvelopeEditor(118, 36, 464, this);
            envelopeList = new EnvelopeListBox(16, 48, 132, this);

        }

        public void Update() {
            if (windowIsOpen) {
                if (ExitButton.Clicked)
                    Close();
                DoDragging();
                tabGroup.Update();
                if (CurrentInstrument is SampleInstrument) {
                    if (tabGroup.SelectedTabIndex == 0) {
                        sampleEditor.Update();
                    }
                    else {
                        envelopeList.Update();
                        if (envelopeList.SelectedIndex >= 0) {
                            envelopeEditor.SetEnvelope(CurrentInstrument.envelopes[envelopeList.SelectedIndex], ChannelManager.previewChannel.envelopePlayers[CurrentInstrument.envelopes[envelopeList.SelectedIndex].Type]);
                            envelopeEditor.Update();
                        }
                        else {
                            envelopeEditor.SetEnvelope(null, null);
                        }
                    }
                }
                else {
                    envelopeList.Update();
                    if (envelopeList.SelectedIndex >= 0) {
                        envelopeEditor.SetEnvelope(CurrentInstrument.envelopes[envelopeList.SelectedIndex], ChannelManager.previewChannel.envelopePlayers[CurrentInstrument.envelopes[envelopeList.SelectedIndex].Type]);
                        envelopeEditor.Update();
                    }
                    else {
                        envelopeEditor.SetEnvelope(null, null);
                    }
                }
            }
        }


        public void Open(Instrument instrumentToEdit, int instrumentIndex) {
            Open();
            currentInstrumentID = instrumentIndex;
            tabGroup = new TabGroup(8, 15, this);
            if (instrumentToEdit is SampleInstrument) {
                tabGroup.AddTab("Sample", false);
                tabGroup.AddTab("Envelopes", false);
                sampleEditor.Sample = ((SampleInstrument)CurrentInstrument).sample;
            }
            else {
                tabGroup.AddTab("Envelopes", false);
            }
            envelopeList.Intialize(CurrentInstrument.envelopes);
        }

        public int GetPianoMouseInput() {
            if (!windowIsOpen || !InFocus)
                return -1;

            if (LastClickPos.X < 44 || LastClickPos.X > 523 || LastClickPos.Y > 330 || LastClickPos.Y < 307)
                return -1;
            if (MouseX < 44 || MouseX > 523 || MouseY > 330 || MouseY < 307)
                return -1;
            if (!Input.GetClick(KeyModifier.None))
                return -1;
            else {
                return (MouseX + 4) / 4;
            }
        }

        public new void Close() {
            envelopeEditor.ResetScrollbar();
            base.Close();
        }

        void DrawPiano(int x, int y) {
            DrawRect(x, y, 482, 26, UIColors.black);
            for (int i = 0; i < 10; ++i) {
                // draw 1 octave of the piano sprite
                DrawSprite(x + i * 48 + 1, y + 1, new Rectangle(0, 80, 48, 24));
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                name = "Edit Instrument " + currentInstrumentID.ToString("D2");
                base.Draw();
                // black box across screen behind window
                //DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));

                // draw window
                DrawRoundedRect(8, 28, 552, 270, Color.White);
                DrawPiano(43, 306);

                tabGroup.Draw();
                DrawRect(9, 28, 280, 1, Color.White);



                if (CurrentInstrument is SampleInstrument) {
                    if (tabGroup.SelectedTabIndex == 0) {
                        sampleEditor.Draw();
                        if (Helpers.IsNoteBlackKey(((SampleInstrument)CurrentInstrument).sample.BaseKey))
                            DrawSprite((((SampleInstrument)CurrentInstrument).sample.BaseKey) * 4 - 4, 307, new Rectangle(60, 80, 4, 24));
                        else
                            DrawSprite((((SampleInstrument)CurrentInstrument).sample.BaseKey) * 4 - 4, 307, new Rectangle(56, 80, 4, 24));
                    }
                    else {
                        envelopeEditor.Draw();
                        envelopeList.Draw();
                    }
                }
                else {
                    envelopeEditor.Draw();
                    envelopeList.Draw();
                }

                // draw currently played key
                if (App.pianoInput > -1) {
                    int note = App.pianoInput + ChannelManager.previewChannel.envelopePlayers[Envelope.EnvelopeType.Arpeggio].Value;
                    if (note >= 12 && note < 132) {
                        if (Helpers.IsNoteBlackKey(note))
                            DrawSprite(note * 4 - 4, 307, new Rectangle(52, 80, 4, 24));
                        else
                            DrawSprite(note * 4 - 4, 307, new Rectangle(48, 80, 4, 24));
                    }
                }
                //if (!tabGroup.GetSelectedTab.toggle.Value && tabGroup.GetSelectedTab.hasToggle) {
                //    DrawRect(16, 36, 535, 253, new Color(255, 255, 255, 100));
                //}
                //browser.Draw();
            }
        }

        public void DrawWaveform(int x, int y, short[] data, int height = 87) {
            int boxLength = 528;
            int boxHeight = height;
            int startY = y + boxHeight / 2;
            int lastSampleNum;
            int sampleNum = 0;
            if (data.Length > 0) {
                for (int i = 0; i < boxLength; i++) {
                    float percentage = (float)i / boxLength;
                    lastSampleNum = sampleNum;
                    sampleNum = (int)(percentage * data.Length - 1);
                    sampleNum = Math.Clamp(sampleNum, 0, data.Length - 1);
                    float min = 1;
                    float max = -1;
                    for (int j = lastSampleNum; j <= sampleNum; j++) {
                        float val = data[j] / (float)(short.MaxValue);
                        if (val < min)
                            min = val;
                        if (val > max)
                            max = val;
                    }
                    min *= boxHeight / 2;
                    max *= boxHeight / 2;
                    if (i > 0)
                        DrawRect(x + i - 1, startY - (int)(max), 1, (int)(max - min) + 1, new Color(207, 117, 43));

                }
                SampleInstrument inst = CurrentInstrument as SampleInstrument;
                if (inst.sample.loopType != Sample.LoopType.OneShot)
                    DrawRect(x + (int)((float)inst.sample.sampleLoopIndex / data.Length * boxLength), y, 1, boxHeight, Color.Yellow);
                if (inst.sample.currentPlaybackPosition < data.Length && Audio.ChannelManager.previewChannel.IsPlaying)
                    DrawRect(x + (int)((float)inst.sample.currentPlaybackPosition / data.Length * boxLength), y, 1, boxHeight, Color.Aqua);
            }
        }


    }
}
