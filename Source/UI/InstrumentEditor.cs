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
        public bool IsOpen { get { return WindowIsOpen; } }

        int currentInstrumentID;
        Instrument CurrentInstrument => App.CurrentModule.Instruments[currentInstrumentID];

        //public Button sample_importSample, sample_normalize, sample_reverse, sample_fadeIn, sample_fadeOut, sample_amplifyUp, sample_amplifyDown, sample_invert;
        //public NumberBox sample_loopPoint;
        //public NumberBox sample_baseKey, sample_detune;
        //public Toggle sample_loopOneshot, sample_loopForward, sample_loopPingpong;
        public EnvelopeEditor envelopeEditor;
        public SampleEditor sampleEditor;
        public EnvelopeListBox envelopeList;
        PreviewPiano piano;
        //public Checkbox visualize_toggle;

        //public Dropdown sample_resampleDropdown;
        // public Dropdown wave_modTypeDropdown;
        TabGroup tabGroup;

        public InstrumentEditor() : base("Instrument Editor", 600, 340) {
            ExitButton.SetTooltip("Close", "Close instrument editor");
            sampleEditor = new SampleEditor(16, 36, this);
            envelopeEditor = new EnvelopeEditor(118, 36, 464, this);
            envelopeList = new EnvelopeListBox(16, 48, 132, this);
            piano = new PreviewPiano(60, 306, this);
        }

        public void Update() {
            if (WindowIsOpen) {
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
            if (instrumentToEdit is SampleInstrument instrument) {
                tabGroup.AddTab("Sample", false);
                tabGroup.AddTab("Envelopes", false);
                sampleEditor.Sample = instrument.sample;
            }
            else {
                tabGroup.AddTab("Envelopes", false);
            }
            envelopeList.Intialize(CurrentInstrument.envelopes);
        }

        public int GetPianoMouseInput() {
            if (!WindowIsOpen || !InFocus)
                return -1;
            return piano.CurrentClickedNote;
        }

        public new void Close() {
            envelopeEditor.ResetScrollbar();
            base.Close();
        }

        public new void Draw() {
            if (WindowIsOpen) {
                name = "Edit Instrument " + currentInstrumentID.ToString("D2");
                base.Draw();
                // black box across screen behind window
                //DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));

                // draw window
                DrawRoundedRect(8, 28, width - 16, 270, Color.White);
                
                
                tabGroup.Draw();
                DrawRect(9, 28, 280, 1, Color.White);

                if (PianoInput.CurrentNote > -1) {
                    piano.Draw(PianoInput.CurrentNote + ChannelManager.previewChannel.envelopePlayers[Envelope.EnvelopeType.Arpeggio].Value);
                }
                else {
                    piano.Draw();
                }

                if (CurrentInstrument is SampleInstrument instrument && tabGroup.SelectedTabIndex == 0) {
                    sampleEditor.Draw();
                    piano.ShowBaseKey = true;
                    piano.BaseKeyIndex = instrument.sample.BaseKey;
                }
                else {
                    piano.ShowBaseKey = false;
                    envelopeEditor.Draw();
                    envelopeList.Draw();
                }

            }
        }
    }
}
