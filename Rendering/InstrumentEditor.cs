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


namespace WaveTracker.Rendering
{
    public class InstrumentEditor : Element
    {
        public static bool enabled;
        public Macro currentMacro;
        public int startcooldown;
        int id;
        public SpriteButton closeButton;
        public Button sample_importSample, sample_normalize, sample_reverse, sample_fadeIn, sample_fadeOut, sample_amplifyUp, sample_amplifyDown, sample_invert;
        public NumberBox sample_loopPoint;
        public NumberBox sample_baseKey, sample_detune;
        public Toggle sample_loopOneshot, sample_loopForward, sample_loopPingpong;
        public EnvelopeEditor envelopeEditor;
        public SpriteToggle visualize_toggle;
        public SampleBrowser browser;
        public Dropdown sample_resampleDropdown;
        Macro instrument => Game1.currentSong.instruments[id];
        TabGroup tabGroup;
        public static Texture2D tex;

        public InstrumentEditor(Texture2D tex)
        {
            InstrumentEditor.tex = tex;
            x = 193;
            y = 100;
            closeButton = new SpriteButton(558, 0, 10, 9, UI.NumberBox.buttons, 4, this);
            closeButton.isPartOfInternalDialog = true;

            closeButton.SetTooltip("Close", "Close instrument editor");
            sample_importSample = new Button("Import Sample    ", 20, 224, this);
            sample_importSample.SetTooltip("", "Import an audio file into the instrument");
            sample_importSample.isPartOfInternalDialog = true;
            envelopeEditor = new EnvelopeEditor(17, 37, tex, this);
            #region sample editor buttons
            sample_loopOneshot = new Toggle("One shot", 187, 224, this);
            sample_loopOneshot.SetTooltip("", "Set the sample so that it plays only once, without looping");
            sample_loopOneshot.isPartOfInternalDialog = true;

            sample_loopForward = new Toggle("Forward", 233, 224, this);
            sample_loopForward.SetTooltip("", "Set the sample so that it continuously loops forward");
            sample_loopForward.isPartOfInternalDialog = true;

            sample_loopPingpong = new Toggle("Ping-pong", 275, 224, this);
            sample_loopPingpong.SetTooltip("", "Set the sample so that it loops continuously forward and backward");
            sample_loopPingpong.isPartOfInternalDialog = true;

            sample_resampleDropdown = new Dropdown(224, 273, this);
            sample_resampleDropdown.SetMenuItems(new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });

            sample_normalize = new Button("Normalize", 337, 224, this);
            sample_normalize.width = 50;
            sample_normalize.SetTooltip("", "Maximize the amplitude of the sample");
            sample_normalize.isPartOfInternalDialog = true;

            sample_amplifyUp = new Button("Amplify+", 439, 224, this);
            sample_amplifyUp.width = 50;
            sample_amplifyUp.SetTooltip("", "Increase the volume of the sample");
            sample_amplifyUp.isPartOfInternalDialog = true;

            sample_amplifyDown = new Button("Amplify-", 439, 238, this);
            sample_amplifyDown.width = 50;
            sample_amplifyDown.SetTooltip("", "Decrease the volume of the sample");
            sample_amplifyDown.isPartOfInternalDialog = true;

            sample_fadeIn = new Button("Fade In", 388, 224, this);
            sample_fadeIn.width = 50;
            sample_fadeIn.SetTooltip("", "Fade the sample from silence to full volume");
            sample_fadeIn.isPartOfInternalDialog = true;

            sample_fadeOut = new Button("Fade Out", 388, 238, this);
            sample_fadeOut.width = 50;
            sample_fadeOut.SetTooltip("", "Fade the sample from full volume to silence");
            sample_fadeOut.isPartOfInternalDialog = true;

            sample_reverse = new Button("Reverse", 337, 238, this);
            sample_reverse.width = 50;
            sample_reverse.SetTooltip("", "Reverse the sample");
            sample_reverse.isPartOfInternalDialog = true;

            sample_invert = new Button("Invert", 490, 224, this);
            sample_invert.width = 50;
            sample_invert.SetTooltip("", "Reverse the polarity of the sample");
            sample_invert.isPartOfInternalDialog = true;

            sample_baseKey = new NumberBox("Base Key", 20, 256, 100, 56, this);
            sample_baseKey.isPartOfInternalDialog = true;
            sample_baseKey.SetTooltip("", "The note where the sample is played at its original pitch");
            sample_baseKey.SetValueLimits(0, 119);
            sample_baseKey.bDown.isPartOfInternalDialog = true;
            sample_baseKey.bUp.isPartOfInternalDialog = true;
            sample_baseKey.displayMode = NumberBox.DisplayMode.Note;

            sample_detune = new NumberBox("Fine tune", 20, 273, 100, 56, this);
            sample_detune.isPartOfInternalDialog = true;
            sample_detune.SetTooltip("", "The note where the sample is played at its original pitch");
            sample_detune.SetValueLimits(-199, 199);
            sample_detune.displayMode = NumberBox.DisplayMode.PlusMinus;

            sample_loopPoint = new NumberBox("Loop position", 183, 239, 140, 80, this);
            sample_loopPoint.isPartOfInternalDialog = true;
            sample_loopPoint.SetTooltip("", "Set the position in audio samples where the sound loops back to");


            visualize_toggle = new SpriteToggle(547, 286, 9, 9, InstrumentEditor.tex, 0, this);
            visualize_toggle.SetTooltip("", "Make the sample visible in visualizer mode. It is a good idea to turn this off for samples such as drums and percussion");
            #endregion


        }

        public void Update()
        {
            if (enabled)
            {
                if (startcooldown > 0)
                {

                    //SetTabToggles();
                    startcooldown--;
                    if (instrument.macroType == MacroType.Sample)
                    {
                        if (tabGroup.selected == 0)
                        {
                            sample_resampleDropdown.Value = (int)instrument.sample.resampleMode;
                            sample_baseKey.Value = instrument.sample.BaseKey;
                            sample_detune.Value = instrument.sample.Detune;
                            sample_loopOneshot.Value = instrument.sample.sampleLoopType == SampleLoopType.OneShot;
                            sample_loopForward.Value = instrument.sample.sampleLoopType == SampleLoopType.Forward;
                            sample_loopPingpong.Value = instrument.sample.sampleLoopType == SampleLoopType.PingPong;
                            visualize_toggle.Value = instrument.sample.useInVisualization;
                        }
                    }
                    else
                    {
                        envelopeEditor.SetEnvelope(instrument.volumeEnvelope, 0);
                    }
                }
                else
                {
                    if (closeButton.Clicked || Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape, KeyModifier.None))
                    {
                        Close();
                    }
                    if (instrument.macroType == MacroType.Sample)
                    {
                        if (tabGroup.selected == 0)
                        {
                            #region sample editor
                            if (instrument.macroType == MacroType.Sample)
                            {
                                if (sample_importSample.Clicked)
                                {
                                    browser.Open(this);
                                }

                                if (sample_normalize.Clicked)
                                    instrument.sample.Normalize();

                                if (sample_reverse.Clicked)
                                    instrument.sample.Reverse();
                                if (sample_invert.Clicked)
                                    instrument.sample.Invert();
                                if (sample_fadeIn.Clicked)
                                    instrument.sample.FadeIn();
                                if (sample_fadeOut.Clicked)
                                    instrument.sample.FadeOut();
                                if (sample_amplifyUp.Clicked)
                                    instrument.sample.Amplify(1.1f);
                                if (sample_amplifyDown.Clicked)
                                    instrument.sample.Amplify(0.9f);
                                if (instrument.sample.sampleLoopType != SampleLoopType.OneShot)
                                {
                                    sample_loopPoint.SetValueLimits(0, instrument.sample.sampleDataAccessL.Length < 1 ? 0 : instrument.sample.sampleDataAccessL.Length - 2);
                                    sample_loopPoint.Value = instrument.sample.sampleLoopIndex;
                                    sample_loopPoint.Update();
                                    instrument.sample.sampleLoopIndex = sample_loopPoint.Value;
                                }
                                #region resampling modes
                                sample_resampleDropdown.Value = (int)instrument.sample.resampleMode;
                                sample_resampleDropdown.Update();
                                instrument.sample.resampleMode = (Audio.ResamplingModes)sample_resampleDropdown.Value;
                                #endregion

                                #region loop modes
                                sample_loopOneshot.Value = instrument.sample.sampleLoopType == SampleLoopType.OneShot;
                                sample_loopForward.Value = instrument.sample.sampleLoopType == SampleLoopType.Forward;
                                sample_loopPingpong.Value = instrument.sample.sampleLoopType == SampleLoopType.PingPong;
                                if (sample_loopOneshot.Clicked)
                                    instrument.sample.sampleLoopType = SampleLoopType.OneShot;
                                if (sample_loopForward.Clicked)
                                    instrument.sample.sampleLoopType = SampleLoopType.Forward;
                                if (sample_loopPingpong.Clicked)
                                    instrument.sample.sampleLoopType = SampleLoopType.PingPong;
                                sample_loopOneshot.Value = instrument.sample.sampleLoopType == SampleLoopType.OneShot;
                                sample_loopForward.Value = instrument.sample.sampleLoopType == SampleLoopType.Forward;
                                sample_loopPingpong.Value = instrument.sample.sampleLoopType == SampleLoopType.PingPong;

                                sample_baseKey.Value = instrument.sample.BaseKey;
                                sample_baseKey.Update();
                                instrument.sample.SetBaseKey(sample_baseKey.Value);
                                sample_detune.Value = instrument.sample.Detune;
                                sample_detune.Update();
                                instrument.sample.SetDetune(sample_detune.Value);
                                #endregion

                                visualize_toggle.Value = instrument.sample.useInVisualization;
                                visualize_toggle.Update();
                                instrument.sample.useInVisualization = visualize_toggle.Value;


                            }
                            #endregion
                        }
                        else
                        {
                            ShowEnvelope(tabGroup.selected - 1);
                        }

                    }
                    else
                    {
                        ShowEnvelope(tabGroup.selected);
                    }
                }
                SetTabTogglesFromInstrument();
                tabGroup.Update();
                ReadFromTabTogglesIntoInstrument();
                browser.Update();
            }
        }

        public void ShowEnvelope(int id)
        {
            if (id == 0)
                envelopeEditor.EditEnvelope(instrument.volumeEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel].volumeEnv, startcooldown == 0);
            if (id == 1)
                envelopeEditor.EditEnvelope(instrument.arpEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel].arpEnv, startcooldown == 0);
            if (id == 2)
                envelopeEditor.EditEnvelope(instrument.pitchEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel].pitchEnv, startcooldown == 0);
            if (id == 3)
                envelopeEditor.EditEnvelope(instrument.waveEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel].waveEnv, startcooldown == 0);
        }

        public void EditMacro(Macro m, int num)
        {
            if (Input.focus == null)
            {
                Input.focus = this;
                //Input.internalDialogIsOpen = true;
                startcooldown = 4;
                enabled = true;
                id = num;
                tabGroup = new TabGroup(8, 15, this);
                if (m.macroType == MacroType.Wave)
                {
                    tabGroup.AddTab("Volume", true);
                    tabGroup.AddTab("Arpeggio", true);
                    tabGroup.AddTab("Pitch", true);
                    tabGroup.AddTab("Wave", true);
                }
                if (m.macroType == MacroType.Sample)
                {
                    tabGroup.AddTab("Sample", false);
                    tabGroup.AddTab("Volume", true);
                    tabGroup.AddTab("Arpeggio", true);
                    tabGroup.AddTab("Pitch", true);
                }
                ShowEnvelope(id);
                SetTabTogglesFromInstrument();
            }
        }

        public void SetTabTogglesFromInstrument()
        {
            if (instrument.macroType == MacroType.Wave)
            {
                tabGroup.tabs[0].toggle.Value = instrument.volumeEnvelope.isActive;
                tabGroup.tabs[1].toggle.Value = instrument.arpEnvelope.isActive;
                tabGroup.tabs[2].toggle.Value = instrument.pitchEnvelope.isActive;
                tabGroup.tabs[3].toggle.Value = instrument.waveEnvelope.isActive;
            }
            if (instrument.macroType == MacroType.Sample)
            {
                tabGroup.tabs[1].toggle.Value = instrument.volumeEnvelope.isActive;
                tabGroup.tabs[2].toggle.Value = instrument.arpEnvelope.isActive;
                tabGroup.tabs[3].toggle.Value = instrument.pitchEnvelope.isActive;
            }
        }

        public void ReadFromTabTogglesIntoInstrument()
        {
            if (instrument.macroType == MacroType.Wave)
            {
                instrument.volumeEnvelope.isActive = tabGroup.tabs[0].toggle.Value;
                instrument.arpEnvelope.isActive = tabGroup.tabs[1].toggle.Value;
                instrument.pitchEnvelope.isActive = tabGroup.tabs[2].toggle.Value;
                instrument.waveEnvelope.isActive = tabGroup.tabs[3].toggle.Value;
            }
            if (instrument.macroType == MacroType.Sample)
            {
                instrument.volumeEnvelope.isActive = tabGroup.tabs[1].toggle.Value;
                instrument.arpEnvelope.isActive = tabGroup.tabs[2].toggle.Value;
                instrument.pitchEnvelope.isActive = tabGroup.tabs[3].toggle.Value;
            }
        }

        public int pianoInput()
        {
            if (!enabled || !inFocus)
                return -1;

            if (LastClickPos.X < 44 || LastClickPos.X > 523 || LastClickPos.Y > 330 || LastClickPos.Y < 307)
                return -1;
            if (MouseX < 44 || MouseX > 523 || MouseY > 330 || MouseY < 307)
                return -1;
            if (!Input.GetClick(KeyModifier.None))
                return -1;
            else
            {
                return (MouseX - 44) / 4;
            }
        }

        public void Close()
        {
            envelopeEditor.ResetScrollbar();
            enabled = false;
            Input.internalDialogIsOpen = false;
            Input.focus = null;
        }

        public void Draw()
        {
            if (enabled)
            {
                // black box across screen behind window
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));

                // draw window
                if (instrument.macroType == MacroType.Sample && tabGroup.selected == 0)
                    DrawSprite(tex, 0, 0, new Rectangle(10, 0, 568, 340));
                else
                    DrawSprite(tex, 0, 0, new Rectangle(10, 341, 568, 340));


                Write("Edit Instrument " + id.ToString("D2"), 4, 1, UIColors.panelTitle);

                closeButton.Draw();

                tabGroup.Draw();

                // draw sample base key
                if (instrument.macroType == MacroType.Sample && tabGroup.selected == 0)
                    if (Helpers.isNoteBlackKey(sample_baseKey.Value))
                        DrawSprite(tex, sample_baseKey.Value * 4 + 44, 307, new Rectangle(590, 0, 4, 24));
                    else
                        DrawSprite(tex, sample_baseKey.Value * 4 + 44, 307, new Rectangle(586, 0, 4, 24));
                // draw currently played key
                if (Game1.pianoInput > -1)
                {
                    int note = Game1.pianoInput + ChannelManager.instance.channels[Game1.previewChannel].arpEnv.Evaluate();
                    if (note >= 0 && note < 120)
                    {
                        if (Helpers.isNoteBlackKey(note))
                            DrawSprite(tex, note * 4 + 44, 307, new Rectangle(582, 0, 4, 24));
                        else
                            DrawSprite(tex, note * 4 + 44, 307, new Rectangle(578, 0, 4, 24));
                    }
                }


                if (instrument.macroType == MacroType.Sample)
                {
                    if (tabGroup.selected == 0)
                    {
                        // sample length information
                        Write(instrument.sample.sampleDataAccessL.Length + " samples", 20, 37, UIColors.label);
                        WriteRightAlign((instrument.sample.sampleDataAccessL.Length / (float)AudioEngine.sampleRate).ToString("F5") + " seconds", 547, 37, UIColors.label);

                        // draw import button
                        sample_importSample.Draw();
                        DrawSprite(tex, sample_importSample.x + 68, sample_importSample.y + (sample_importSample.IsPressed ? 2 : 2), new Rectangle(595, 0, 11, 9));

                        // waveforms
                        if (instrument.sample.sampleDataAccessR.Length > 0)
                        {
                            DrawWaveform(20, 46, instrument.sample.sampleDataAccessL);
                            DrawWaveform(20, 134, instrument.sample.sampleDataAccessR);
                        }
                        else
                        {
                            DrawRect(20, 133, 528, 1, UIColors.black);
                            DrawRect(11, 46, 8, 175, Color.White);
                            DrawWaveform(20, 46, instrument.sample.sampleDataAccessL, 175);
                        }
                        sample_loopPingpong.Draw();
                        sample_loopForward.Draw();
                        sample_loopOneshot.Draw();

                        if (instrument.sample.sampleLoopType != SampleLoopType.OneShot)
                        {
                            sample_loopPoint.Value = instrument.sample.sampleLoopIndex;
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
                    else
                    {
                        envelopeEditor.Draw();
                    }
                }
                else
                {
                    envelopeEditor.Draw();
                }
                browser.Draw();
            }
        }

        public void DrawWaveform(int x, int y, float[] data, int height = 87)
        {
            int boxLength = 528;
            int boxHeight = height;
            int startY = y + boxHeight / 2;
            int lastSampleNum;
            int sampleNum = 0;
            if (data.Length > 0)
            {
                for (int i = 0; i < boxLength; i++)
                {
                    float percentage = (float)i / boxLength;
                    lastSampleNum = sampleNum;
                    sampleNum = (int)(percentage * data.Length - 1);
                    sampleNum = Math.Clamp(sampleNum, 0, data.Length - 1);
                    float min = 1;
                    float max = -1;
                    for (int j = lastSampleNum; j <= sampleNum; j++)
                    {
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
                if (instrument.sample.sampleLoopType != SampleLoopType.OneShot)
                    DrawRect(x + (int)((float)instrument.sample.sampleLoopIndex / data.Length * boxLength), y, 1, boxHeight, Color.Yellow);
                if (instrument.sample.currentPlaybackPosition < data.Length && Audio.ChannelManager.instance.channels[Game1.previewChannel].isPlaying)
                    DrawRect(x + (int)((float)instrument.sample.currentPlaybackPosition / data.Length * boxLength), y, 1, boxHeight, Color.Aqua);
            }
        }

        public void LoadSampleFromFile(string path)
        {
            Macro macro = instrument;
            bool successfulReadWAV = (Helpers.readWav(path, out macro.sample.sampleDataAccessL, out macro.sample.sampleDataAccessR));
            macro.sample.SetBaseKey(48);
            macro.sample.SetDetune(0);
            macro.sample.sampleLoopIndex = 0;
            macro.sample.sampleLoopType = macro.sample.sampleDataAccessL.Length < 1000 ? SampleLoopType.Forward : SampleLoopType.OneShot;
            macro.sample.resampleMode = ResamplingModes.Linear;
            if (successfulReadWAV)
            {
                if (Preferences.automaticallyTrimSamples)
                    macro.sample.TrimSilence();
                if (Preferences.automaticallyNormalizeSamples)
                    macro.sample.Normalize();

                macro.sample.resampleMode = ResamplingModes.Linear;
            }
            else
            {
                macro.sample.sampleDataAccessL = Array.Empty<float>();
                macro.sample.sampleDataAccessR = Array.Empty<float>();
            }
        }
    }
}
