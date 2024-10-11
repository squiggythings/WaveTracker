using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class InstrumentEditor : Window {
        public bool IsOpen { get { return WindowIsOpen; } }

        private int currentInstrumentID;

        private Instrument CurrentInstrument {
            get {
                return App.CurrentModule.Instruments[currentInstrumentID];
            }
        }

        public EnvelopeEditor envelopeEditor;
        public SampleEditor sampleEditor;
        public EnvelopeListBox envelopeList;
        private PreviewPiano piano;
        private TabGroup tabGroup;

        public InstrumentEditor() : base("Instrument Editor", 600, 340) {
            ExitButton.SetTooltip("Close", "Close instrument editor");
            sampleEditor = new SampleEditor(16, 36, this);
            envelopeEditor = new EnvelopeEditor(118, 36, 464, this);
            envelopeList = new EnvelopeListBox(16, 48, 132, this);
            piano = new PreviewPiano(60, 306, this);
        }

        public void Update() {
            if (WindowIsOpen) {
                if (InFocus && (ExitButton.Clicked || Input.GetKeyDown(Keys.Escape, KeyModifier.None))) {
                    Close();
                }

                DoDragging();
                tabGroup.Update();
                if (CurrentInstrument is SampleInstrument) {
                    if (tabGroup.SelectedTabIndex == 0) {
                        sampleEditor.Update();
                    }
                    else {
                        envelopeList.Update();
                        if (envelopeList.SelectedIndex >= 0) {
                            envelopeEditor.SetEnvelope(CurrentInstrument.envelopes[envelopeList.SelectedIndex], ChannelManager.PreviewChannel.envelopePlayers[CurrentInstrument.envelopes[envelopeList.SelectedIndex].Type]);
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
                        envelopeEditor.SetEnvelope(CurrentInstrument.envelopes[envelopeList.SelectedIndex], ChannelManager.PreviewChannel.envelopePlayers[CurrentInstrument.envelopes[envelopeList.SelectedIndex].Type]);
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
                sampleEditor.Reset();
            }
            else {
                tabGroup.AddTab("Envelopes", false);
            }
            envelopeList.Intialize(CurrentInstrument.envelopes);
        }

        public int GetPianoMouseInput() {
            return !WindowIsOpen || !InFocus ? -1 : piano.CurrentClickedNote;
        }

        public new void Close() {
            envelopeEditor.ResetScrollbar();
            base.Close();
        }

        public new void Draw() {
            if (WindowIsOpen) {
                name = "Edit Instrument " + currentInstrumentID.ToString("D2");

                // draw window
                base.Draw();

                Write(" (" + App.CurrentModule.Instruments[currentInstrumentID].name + ")", 6 + Helpers.GetWidthOfText(name), 1, UIColors.labelLight);

                DrawRoundedRect(8, 28, width - 16, 270, Color.White);
                tabGroup.Draw();
                DrawRect(9, 28, 280, 1, Color.White);
                if (PianoInput.CurrentNote > -1) {
                    piano.Draw(PianoInput.CurrentNote + ChannelManager.PreviewChannel.envelopePlayers[Envelope.EnvelopeType.Arpeggio].Value);
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
