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
        public static bool enabled;
        public Instrument currentMacro;
        public int startcooldown;
        int id;
        public Button sample_importSample, sample_normalize, sample_reverse, sample_fadeIn, sample_fadeOut, sample_amplifyUp, sample_amplifyDown, sample_invert;
        public NumberBox sample_loopPoint;
        public NumberBox sample_baseKey, sample_detune;
        public Toggle sample_loopOneshot, sample_loopForward, sample_loopPingpong;
        public EnvelopeEditor envelopeEditor;
        public Checkbox visualize_toggle;
        public SampleBrowser browser;
        public Dropdown sample_resampleDropdown;
        public Dropdown wave_modTypeDropdown;
        Instrument CurrentInstrument => App.CurrentModule.Instruments[id];
        TabGroup tabGroup;

        public InstrumentEditor() : base("Instrument Editor", 568, 340) {
            ExitButton.SetTooltip("Close", "Close instrument editor");
            sample_importSample = new Button("Import Sample    ", 20, 224, this);
            sample_importSample.SetTooltip("", "Import an audio file into the instrument");

            wave_modTypeDropdown = new Dropdown(478, 260, this);
            wave_modTypeDropdown.SetMenuItems(new string[] { "Wave Blend", "Wave Stretch", "FM" });
            wave_modTypeDropdown.Value = 0;
            wave_modTypeDropdown.SetTooltip("", "Wave Blend: crossfade the current wave with the next one in the bank - Wave stretch: bends the wave shape - FM: modulate the frequency of this wave by the next one in the bank");

            envelopeEditor = new EnvelopeEditor(17, 37, this);
            #region sample editor buttons
            sample_loopOneshot = new Toggle("One shot", 187, 224, this);
            sample_loopOneshot.SetTooltip("", "Set the sample so that it plays only once, without looping");

            sample_loopForward = new Toggle("Forward", 233, 224, this);
            sample_loopForward.SetTooltip("", "Set the sample so that it continuously loops forward");

            sample_loopPingpong = new Toggle("Ping-pong", 275, 224, this);
            sample_loopPingpong.SetTooltip("", "Set the sample so that it loops continuously forward and backward");

            sample_resampleDropdown = new Dropdown(224, 273, this);
            sample_resampleDropdown.SetMenuItems(new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });

            sample_normalize = new Button("Normalize", 337, 224, this);
            sample_normalize.width = 50;
            sample_normalize.SetTooltip("", "Maximize the amplitude of the sample");

            sample_amplifyUp = new Button("Amplify+", 439, 224, this);
            sample_amplifyUp.width = 50;
            sample_amplifyUp.SetTooltip("", "Increase the volume of the sample");

            sample_amplifyDown = new Button("Amplify-", 439, 238, this);
            sample_amplifyDown.width = 50;
            sample_amplifyDown.SetTooltip("", "Decrease the volume of the sample");

            sample_fadeIn = new Button("Fade In", 388, 224, this);
            sample_fadeIn.width = 50;
            sample_fadeIn.SetTooltip("", "Fade the sample from silence to full volume");

            sample_fadeOut = new Button("Fade Out", 388, 238, this);
            sample_fadeOut.width = 50;
            sample_fadeOut.SetTooltip("", "Fade the sample from full volume to silence");

            sample_reverse = new Button("Reverse", 337, 238, this);
            sample_reverse.width = 50;
            sample_reverse.SetTooltip("", "Reverse the sample");

            sample_invert = new Button("Invert", 490, 224, this);
            sample_invert.width = 50;
            sample_invert.SetTooltip("", "Reverse the polarity of the sample");

            sample_baseKey = new NumberBox("Base Key", 20, 256, 100, 56, this);
            sample_baseKey.SetTooltip("", "The note where the sample is played at its original pitch");
            sample_baseKey.SetValueLimits(12, 131);
            sample_baseKey.displayMode = NumberBox.DisplayMode.Note;

            sample_detune = new NumberBox("Fine tune", 20, 273, 100, 56, this);
            sample_detune.SetTooltip("", "The note where the sample is played at its original pitch");
            sample_detune.SetValueLimits(-199, 199);
            sample_detune.displayMode = NumberBox.DisplayMode.PlusMinus;

            sample_loopPoint = new NumberBox("Loop position (samples)", 140, 239, 183, 80, this);
            sample_loopPoint.SetTooltip("", "Set the position in audio samples where the sound loops back to");


            visualize_toggle = new Checkbox(547, 286, this);
            visualize_toggle.SetTooltip("", "Make the sample visible in visualizer mode. It is a good idea to turn this off for samples such as drums and percussion");
            #endregion


        }

        public void Update() {
            if (windowIsOpen) {
                if (startcooldown > 0) {

                    //SetTabToggles();
                    startcooldown--;
                    if (CurrentInstrument.instrumentType == InstrumentType.Sample) {
                        if (tabGroup.SelectedTabIndex == 0) {
                            sample_resampleDropdown.Value = (int)CurrentInstrument.sample.resampleMode;
                            sample_baseKey.Value = CurrentInstrument.sample.BaseKey;
                            sample_detune.Value = CurrentInstrument.sample.Detune;
                            sample_loopOneshot.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.OneShot;
                            sample_loopForward.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.Forward;
                            sample_loopPingpong.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.PingPong;
                            visualize_toggle.Value = CurrentInstrument.sample.useInVisualization;
                        }
                    }
                    else {
                        wave_modTypeDropdown.Value = CurrentInstrument.waveModType;
                        envelopeEditor.SetEnvelope(CurrentInstrument.volumeEnvelope, 0);
                    }
                }
                else {
                    if (ExitButton.Clicked || Input.GetKeyDown(Keys.Escape, KeyModifier.None)) {
                        Close();
                    }
                    if (CurrentInstrument.instrumentType == InstrumentType.Sample) {
                        if (tabGroup.SelectedTabIndex == 0) {
                            #region sample editor
                            if (CurrentInstrument.instrumentType == InstrumentType.Sample) {
                                if (sample_importSample.Clicked) {
                                    browser.Open(this);
                                }

                                if (sample_normalize.Clicked) {
                                    CurrentInstrument.sample.Normalize();
                                    App.CurrentModule.SetDirty();
                                }

                                if (sample_reverse.Clicked) {
                                    CurrentInstrument.sample.Reverse();
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_invert.Clicked) {
                                    CurrentInstrument.sample.Invert();
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_fadeIn.Clicked) {
                                    CurrentInstrument.sample.FadeIn();
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_fadeOut.Clicked) {
                                    CurrentInstrument.sample.FadeOut();
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_amplifyUp.Clicked) {
                                    CurrentInstrument.sample.Amplify(1.1f);
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_amplifyDown.Clicked) {
                                    CurrentInstrument.sample.Amplify(0.9f);
                                    App.CurrentModule.SetDirty();
                                }
                                if (CurrentInstrument.sample.sampleLoopType != SampleLoopType.OneShot) {
                                    sample_loopPoint.SetValueLimits(0, CurrentInstrument.sample.sampleDataAccessL.Length < 1 ? 0 : CurrentInstrument.sample.sampleDataAccessL.Length - 2);
                                    sample_loopPoint.Value = CurrentInstrument.sample.sampleLoopIndex;
                                    sample_loopPoint.Update();
                                    CurrentInstrument.sample.sampleLoopIndex = sample_loopPoint.Value;
                                }
                                #region resampling modes
                                sample_resampleDropdown.Value = (int)CurrentInstrument.sample.resampleMode;
                                sample_resampleDropdown.Update();
                                if (sample_resampleDropdown.ValueWasChangedInternally) {
                                    App.CurrentModule.SetDirty();
                                    CurrentInstrument.sample.resampleMode = (Audio.ResamplingMode)sample_resampleDropdown.Value;
                                }
                                #endregion

                                #region loop modes
                                sample_loopOneshot.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.OneShot;
                                sample_loopForward.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.Forward;
                                sample_loopPingpong.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.PingPong;
                                if (sample_loopOneshot.Clicked) {
                                    CurrentInstrument.sample.sampleLoopType = SampleLoopType.OneShot;
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_loopForward.Clicked) {
                                    CurrentInstrument.sample.sampleLoopType = SampleLoopType.Forward;
                                    App.CurrentModule.SetDirty();
                                }
                                if (sample_loopPingpong.Clicked) {
                                    CurrentInstrument.sample.sampleLoopType = SampleLoopType.PingPong;
                                    App.CurrentModule.SetDirty();
                                }
                                sample_loopOneshot.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.OneShot;
                                sample_loopForward.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.Forward;
                                sample_loopPingpong.Value = CurrentInstrument.sample.sampleLoopType == SampleLoopType.PingPong;

                                sample_baseKey.Value = CurrentInstrument.sample.BaseKey;
                                sample_baseKey.Update();
                                if (sample_baseKey.ValueWasChangedInternally) {
                                    CurrentInstrument.sample.SetBaseKey(sample_baseKey.Value);
                                    App.CurrentModule.SetDirty();
                                }
                                sample_detune.Value = CurrentInstrument.sample.Detune;
                                sample_detune.Update();
                                if (sample_detune.ValueWasChangedInternally) {
                                    CurrentInstrument.sample.SetDetune(sample_detune.Value);
                                    App.CurrentModule.SetDirty();
                                }
                                #endregion

                                visualize_toggle.Value = CurrentInstrument.sample.useInVisualization;
                                visualize_toggle.Update();
                                if (visualize_toggle.Value != CurrentInstrument.sample.useInVisualization) {
                                    App.CurrentModule.SetDirty();
                                    CurrentInstrument.sample.useInVisualization = visualize_toggle.Value;
                                }
                            }
                            #endregion
                        }
                        else {
                            ShowEnvelope(tabGroup.SelectedTabIndex - 1);
                        }

                    }
                    else {
                        if (tabGroup.SelectedTabIndex == 4 && tabGroup.tabs[4].toggle.Value) {
                            wave_modTypeDropdown.enabled = true;
                            wave_modTypeDropdown.Value = CurrentInstrument.waveModType;
                            wave_modTypeDropdown.Update();
                            if (wave_modTypeDropdown.Value != CurrentInstrument.waveModType) {
                                App.CurrentModule.SetDirty();
                                ChannelManager.previewChannel.ResetModulations();
                                CurrentInstrument.waveModType = wave_modTypeDropdown.Value;
                            }

                        }
                        else {
                            wave_modTypeDropdown.enabled = false;
                        }
                        ShowEnvelope(tabGroup.SelectedTabIndex);
                    }
                }
                SetTabTogglesFromInstrument();
                tabGroup.Update();
                ReadFromTabTogglesIntoInstrument();
                browser.Update();
            }
        }

        public void ShowEnvelope(int id) {
            if (id == 0)
                envelopeEditor.EditEnvelope(CurrentInstrument.volumeEnvelope, id, ChannelManager.previewChannel.volumeEnv);
            if (id == 1)
                envelopeEditor.EditEnvelope(CurrentInstrument.arpEnvelope, id, ChannelManager.previewChannel.arpEnv);
            if (id == 2)
                envelopeEditor.EditEnvelope(CurrentInstrument.pitchEnvelope, id, ChannelManager.previewChannel.pitchEnv);
            if (id == 3)
                envelopeEditor.EditEnvelope(CurrentInstrument.waveEnvelope, id, ChannelManager.previewChannel.waveEnv);
            if (id == 4)
                envelopeEditor.EditEnvelope(CurrentInstrument.waveModEnvelope, id, ChannelManager.previewChannel.waveModEnv);
        }

        public void Open(Instrument instrumentToEdit, int instrumentIndex) {
            if (Input.focus == null) {
                enabled = true;
                Open();
                startcooldown = 4;
                id = instrumentIndex;
                tabGroup = new TabGroup(8, 15, this);
                if (instrumentToEdit.instrumentType == InstrumentType.Wave) {
                    tabGroup.AddTab("Volume", true);
                    tabGroup.AddTab("Arpeggio", true);
                    tabGroup.AddTab("Pitch", true);
                    tabGroup.AddTab("Wave", true);
                    tabGroup.AddTab("Wave Mod", true);
                }
                if (instrumentToEdit.instrumentType == InstrumentType.Sample) {
                    tabGroup.AddTab("Sample", false);
                    tabGroup.AddTab("Volume", true);
                    tabGroup.AddTab("Arpeggio", true);
                    tabGroup.AddTab("Pitch", true);
                }
                ShowEnvelope(id);
                SetTabTogglesFromInstrument();
            }
        }

        public void SetTabTogglesFromInstrument() {
            if (CurrentInstrument.instrumentType == InstrumentType.Wave) {
                tabGroup.tabs[0].toggle.Value = CurrentInstrument.volumeEnvelope.isActive;
                tabGroup.tabs[1].toggle.Value = CurrentInstrument.arpEnvelope.isActive;
                tabGroup.tabs[2].toggle.Value = CurrentInstrument.pitchEnvelope.isActive;
                tabGroup.tabs[3].toggle.Value = CurrentInstrument.waveEnvelope.isActive;
                tabGroup.tabs[4].toggle.Value = CurrentInstrument.waveModEnvelope.isActive;
            }
            if (CurrentInstrument.instrumentType == InstrumentType.Sample) {
                tabGroup.tabs[1].toggle.Value = CurrentInstrument.volumeEnvelope.isActive;
                tabGroup.tabs[2].toggle.Value = CurrentInstrument.arpEnvelope.isActive;
                tabGroup.tabs[3].toggle.Value = CurrentInstrument.pitchEnvelope.isActive;
            }
        }

        public void ReadFromTabTogglesIntoInstrument() {
            if (CurrentInstrument.instrumentType == InstrumentType.Wave) {
                if (tabGroup.tabs[0].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.volumeEnvelope.isActive = tabGroup.tabs[0].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
                if (tabGroup.tabs[1].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.arpEnvelope.isActive = tabGroup.tabs[1].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
                if (tabGroup.tabs[2].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.pitchEnvelope.isActive = tabGroup.tabs[2].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
                if (tabGroup.tabs[3].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.waveEnvelope.isActive = tabGroup.tabs[3].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
                if (tabGroup.tabs[4].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.waveModEnvelope.isActive = tabGroup.tabs[4].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
            }
            if (CurrentInstrument.instrumentType == InstrumentType.Sample) {
                if (tabGroup.tabs[1].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.volumeEnvelope.isActive = tabGroup.tabs[1].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
                if (tabGroup.tabs[2].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.arpEnvelope.isActive = tabGroup.tabs[2].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
                if (tabGroup.tabs[3].toggle.ValueWasChangedInternally) {
                    CurrentInstrument.pitchEnvelope.isActive = tabGroup.tabs[3].toggle.Value;
                    App.CurrentModule.SetDirty();
                }
            }
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
            enabled = false;
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
                name = "Edit Instrument " + id.ToString("D2");
                base.Draw();
                // black box across screen behind window
                //DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));

                // draw window
                DrawRoundedRect(8, 28, 552, 270, Color.White);
                DrawPiano(43, 306);

                if (CurrentInstrument.instrumentType == InstrumentType.Sample && tabGroup.SelectedTabIndex == 0) {
                    Write("L", 14, 86, UIColors.labelLight);
                    DrawRect(20, 46, 528, 87, UIColors.black);
                    Write("R", 14, 174, UIColors.labelLight);
                    DrawRect(20, 134, 528, 87, UIColors.black);

                    Write("Loop Mode", 140, 227, UIColors.labelDark);
                    Write("Resampling Filter", 150, 276, UIColors.labelDark);
                    DrawRect(330, 224, 1, 66, UIColors.label);
                    Write("Show in visualizer", 469, 287, UIColors.labelDark);
                    //DrawSprite(tex, 0, 0, new Rectangle(10, 0, 568, 340));
                }
                else {
                    // draw envelope editor background
                    DrawRect(16, 36, 535, 222, UIColors.black);
                    DrawRect(17, 57, 44, 200, new Color(31, 36, 63));
                    // draw loop ribbon
                    DrawRect(17, 37, 44, 9, new Color(172, 202, 162));
                    DrawRect(61, 37, 489, 9, new Color(14, 72, 55));
                    Write("Loop", 39, 38, UIColors.black);
                    // draw release ribbon
                    DrawRect(17, 47, 44, 9, new Color(234, 192, 165));
                    DrawRect(61, 47, 489, 9, new Color(125, 56, 51));
                    Write("Release", 27, 48, UIColors.black);

                    // DrawSprite(tex, 0, 0, new Rectangle(10, 341, 568, 340));
                }

                tabGroup.Draw();
                DrawRect(9, 28, 280, 1, Color.White);
                // draw sample base key
                if (CurrentInstrument.instrumentType == InstrumentType.Sample && tabGroup.SelectedTabIndex == 0)
                    if (Helpers.IsNoteBlackKey(sample_baseKey.Value))
                        DrawSprite(sample_baseKey.Value * 4 - 4, 307, new Rectangle(60, 80, 4, 24));
                    else
                        DrawSprite(sample_baseKey.Value * 4 - 4, 307, new Rectangle(56, 80, 4, 24));
                // draw currently played key
                if (App.pianoInput > -1) {
                    int note = App.pianoInput + ChannelManager.previewChannel.arpEnv.Evaluate();
                    if (note >= 12 && note < 132) {
                        if (Helpers.IsNoteBlackKey(note))
                            DrawSprite(note * 4 - 4, 307, new Rectangle(52, 80, 4, 24));
                        else
                            DrawSprite(note * 4 - 4, 307, new Rectangle(48, 80, 4, 24));
                    }
                }


                if (CurrentInstrument.instrumentType == InstrumentType.Sample) {
                    if (tabGroup.SelectedTabIndex == 0) {
                        // sample length information
                        Write(CurrentInstrument.sample.sampleDataAccessL.Length + " samples", 20, 37, UIColors.label);
                        WriteRightAlign((CurrentInstrument.sample.sampleDataAccessL.Length / (float)AudioEngine.sampleRate).ToString("F5") + " seconds", 547, 37, UIColors.label);

                        // draw import button
                        sample_importSample.Draw();
                        DrawSprite(sample_importSample.x + 68, sample_importSample.y + (sample_importSample.IsPressed ? 3 : 2), new Rectangle(72, 81, 12, 9));

                        // waveforms
                        if (CurrentInstrument.sample.sampleDataAccessR.Length > 0) {
                            DrawWaveform(20, 46, CurrentInstrument.sample.sampleDataAccessL);
                            DrawWaveform(20, 134, CurrentInstrument.sample.sampleDataAccessR);
                        }
                        else {
                            DrawRect(20, 133, 528, 1, UIColors.black);
                            DrawRect(11, 46, 8, 175, Color.White);
                            DrawWaveform(20, 46, CurrentInstrument.sample.sampleDataAccessL, 175);
                        }
                        sample_loopPingpong.Draw();
                        sample_loopForward.Draw();
                        sample_loopOneshot.Draw();

                        if (CurrentInstrument.sample.sampleLoopType != SampleLoopType.OneShot) {
                            sample_loopPoint.Value = CurrentInstrument.sample.sampleLoopIndex;
                            sample_loopPoint.Draw();
                        }

                        sample_resampleDropdown.Draw();

                        sample_normalize.Draw();
                        sample_reverse.Draw();
                        sample_fadeIn.Draw();
                        sample_fadeOut.Draw();
                        sample_invert.Draw();
                        sample_amplifyUp.Draw();
                        sample_amplifyDown.Draw();
                        sample_detune.Draw();
                        sample_baseKey.Draw();
                        visualize_toggle.Draw();
                    }
                    else {
                        envelopeEditor.Draw();
                    }
                }
                else {
                    envelopeEditor.Draw();
                    if (tabGroup.SelectedTabIndex == 4) {
                        WriteRightAlign("Mode", wave_modTypeDropdown.x - 4, wave_modTypeDropdown.y + 3, UIColors.label);
                        wave_modTypeDropdown.Draw();
                    }
                }
                if (!tabGroup.GetSelectedTab.toggle.Value && tabGroup.GetSelectedTab.hasToggle) {
                    DrawRect(16, 36, 535, 253, new Color(255, 255, 255, 100));
                }
                browser.Draw();
            }
        }

        public void DrawWaveform(int x, int y, float[] data, int height = 87) {
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
                        if (data[j] < min)
                            min = data[j];
                        if (data[j] > max)
                            max = data[j];
                    }
                    min *= boxHeight / 2;
                    max *= boxHeight / 2;
                    if (i > 0)
                        DrawRect(x + i - 1, startY - (int)(max), 1, (int)(max - min) + 1, new Color(207, 117, 43));

                }
                if (CurrentInstrument.sample.sampleLoopType != SampleLoopType.OneShot)
                    DrawRect(x + (int)((float)CurrentInstrument.sample.sampleLoopIndex / data.Length * boxLength), y, 1, boxHeight, Color.Yellow);
                if (CurrentInstrument.sample.currentPlaybackPosition < data.Length && Audio.ChannelManager.previewChannel.isPlaying)
                    DrawRect(x + (int)((float)CurrentInstrument.sample.currentPlaybackPosition / data.Length * boxLength), y, 1, boxHeight, Color.Aqua);
            }
        }

        public void LoadSampleFromFile(string path) {
            Instrument macro = CurrentInstrument;
            bool successfulReadWAV = (Helpers.readWav(path, out macro.sample.sampleDataAccessL, out macro.sample.sampleDataAccessR));
            macro.sample.SetBaseKey(Preferences.profile.defaultBaseKey);
            macro.sample.SetDetune(0);
            macro.sample.sampleLoopIndex = 0;
            macro.sample.sampleLoopType = macro.sample.sampleDataAccessL.Length < 1000 ? SampleLoopType.Forward : SampleLoopType.OneShot;
            macro.sample.resampleMode = (Audio.ResamplingMode)Preferences.profile.defaultResampleSample;
            if (successfulReadWAV) {
                if (Preferences.profile.automaticallyTrimSamples)
                    macro.sample.TrimSilence();
                if (Preferences.profile.automaticallyNormalizeSamples)
                    macro.sample.Normalize();

                macro.sample.resampleMode = (Audio.ResamplingMode)Preferences.profile.defaultResampleSample;
                App.CurrentModule.SetDirty();
            }
            else {
                macro.sample.sampleDataAccessL = Array.Empty<float>();
                macro.sample.sampleDataAccessR = Array.Empty<float>();
                App.CurrentModule.SetDirty();
            }
        }
    }
}
