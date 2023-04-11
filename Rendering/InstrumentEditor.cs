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
        int startcooldown;
        int id;
        public SpriteButton closeButton;
        public Button sample_importSample, sample_normalize, sample_reverse;
        public NumberBox sample_loopPoint;
        public NumberBox sample_baseKey, sample_detune;
        public Toggle sample_resampLinear, sample_resampNone, sample_resampMix, sample_loopOneshot, sample_loopForward, sample_loopPingpong;
        public EnvelopeEditor envelopeEditor;
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

            sample_resampNone = new Toggle("None", 222, 273, this);
            sample_resampNone.SetTooltip("", "Set the resampling mode to no interpolation. Has a harsher, gritty sound.");
            sample_resampNone.isPartOfInternalDialog = true;
            sample_resampNone.width = 33;

            sample_resampLinear = new Toggle("Linear", 256, 273, this);
            sample_resampLinear.SetTooltip("", "Set the resampling mode to linear interpolation. Has a mellow, softer sound.");
            sample_resampLinear.isPartOfInternalDialog = true;
            sample_resampLinear.width = 33;

            sample_resampMix = new Toggle("Mix", 290, 273, this);
            sample_resampMix.SetTooltip("", "Set the resampling mode to an average between none and linear interpolation.");
            sample_resampMix.isPartOfInternalDialog = true;
            sample_resampMix.width = 33;

            sample_normalize = new Button("Normalize", 337, 224, this);
            sample_normalize.SetTooltip("", "Maximize the amplitude of the sample");
            sample_normalize.isPartOfInternalDialog = true;

            sample_reverse = new Button("Reverse", 337, 238, this);
            sample_reverse.SetTooltip("", "Reverse the sample");
            sample_reverse.isPartOfInternalDialog = true;

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
            sample_detune.bDown.isPartOfInternalDialog = true;
            sample_detune.bUp.isPartOfInternalDialog = true;
            sample_detune.displayMode = NumberBox.DisplayMode.PlusMinus;

            sample_loopPoint = new NumberBox("Loop position", 183, 239, 140, 80, this);
            sample_loopPoint.isPartOfInternalDialog = true;
            sample_loopPoint.SetTooltip("", "Set the position in audio samples where the sound loops back to");
            sample_loopPoint.bDown.isPartOfInternalDialog = true;
            sample_loopPoint.bUp.isPartOfInternalDialog = true;

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
                            sample_baseKey.Value = instrument.sample.BaseKey;
                            sample_detune.Value = instrument.sample.Detune;
                            sample_loopOneshot.Value = instrument.sample.sampleLoopType == SampleLoopType.OneShot;
                            sample_loopForward.Value = instrument.sample.sampleLoopType == SampleLoopType.Forward;
                            sample_loopPingpong.Value = instrument.sample.sampleLoopType == SampleLoopType.PingPong;
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
                                    LoadSampleFromFile(instrument);
                                }

                                if (sample_normalize.Clicked)
                                    instrument.sample.Normalize();

                                if (sample_reverse.Clicked)
                                    instrument.sample.Reverse();
                                if (instrument.sample.sampleLoopType != SampleLoopType.OneShot)
                                {
                                    sample_loopPoint.SetValueLimits(0, instrument.sample.sampleDataLeft.Count < 1 ? 0 : instrument.sample.sampleDataLeft.Count - 2);
                                    sample_loopPoint.Value = instrument.sample.sampleLoopIndex;
                                    sample_loopPoint.Update();
                                    instrument.sample.sampleLoopIndex = sample_loopPoint.Value;
                                }
                                #region resampling modes
                                sample_resampNone.Value = instrument.sample.resampleMode == Audio.ResamplingModes.NoInterpolation;
                                sample_resampLinear.Value = instrument.sample.resampleMode == Audio.ResamplingModes.LinearInterpolation;
                                sample_resampMix.Value = instrument.sample.resampleMode == Audio.ResamplingModes.Average;
                                if (sample_resampNone.Clicked)
                                    instrument.sample.resampleMode = Audio.ResamplingModes.NoInterpolation;
                                if (sample_resampLinear.Clicked)
                                    instrument.sample.resampleMode = Audio.ResamplingModes.LinearInterpolation;
                                if (sample_resampMix.Clicked)
                                    instrument.sample.resampleMode = Audio.ResamplingModes.Average;
                                sample_resampNone.Value = instrument.sample.resampleMode == Audio.ResamplingModes.NoInterpolation;
                                sample_resampLinear.Value = instrument.sample.resampleMode == Audio.ResamplingModes.LinearInterpolation;
                                sample_resampMix.Value = instrument.sample.resampleMode == Audio.ResamplingModes.Average;
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
            }
        }

        public void ShowEnvelope(int id)
        {
            if (id == 0)
                envelopeEditor.EditEnvelope(instrument.volumeEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel], ChannelManager.instance.channels[Game1.previewChannel].volumeEnv, startcooldown == 0);
            if (id == 1)
                envelopeEditor.EditEnvelope(instrument.arpEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel], ChannelManager.instance.channels[Game1.previewChannel].arpEnv, startcooldown == 0);
            if (id == 2)
                envelopeEditor.EditEnvelope(instrument.pitchEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel], ChannelManager.instance.channels[Game1.previewChannel].pitchEnv, startcooldown == 0);
            if (id == 3)
                envelopeEditor.EditEnvelope(instrument.waveEnvelope, id, ChannelManager.instance.channels[Game1.previewChannel], ChannelManager.instance.channels[Game1.previewChannel].waveEnv, startcooldown == 0);
        }

        public void EditMacro(Macro m, int num)
        {
            enabled = true;
            Input.internalDialogIsOpen = true;
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
            if (!enabled)
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
        }

        public void Draw()
        {
            if (enabled)
            {
                // black box across screen behind window
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 60));

                // draw window
                if (instrument.macroType == MacroType.Sample && tabGroup.selected == 0)
                    DrawSprite(tex, 0, 0, new Rectangle(10, 0, 568, 340));
                else
                    DrawSprite(tex, 0, 0, new Rectangle(10, 341, 568, 340));


                Write("Edit Instrument " + id.ToString("D2"), 4, 1, new Color(64, 72, 115));

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
                        Write(instrument.sample.sampleDataLeft.Count + " samples", 20, 37, ButtonColors.Round.backgroundColor);
                        WriteRightAlign((instrument.sample.sampleDataLeft.Count / (float)AudioEngine.sampleRate).ToString("F5") + " seconds", 547, 37, ButtonColors.Round.backgroundColor);

                        // draw import button
                        sample_importSample.Draw();
                        DrawSprite(tex, sample_importSample.x + 68, sample_importSample.y + (sample_importSample.IsPressed ? 2 : 2), new Rectangle(595, 0, 11, 9));

                        // waveforms
                        if (instrument.sample.sampleDataRight.Count > 0)
                        {
                            DrawWaveform(20, 46, instrument.sample.sampleDataLeft);
                            DrawWaveform(20, 134, instrument.sample.sampleDataRight);
                        }
                        else
                        {
                            DrawRect(20, 133, 528, 1, new Color(20, 24, 46));
                            DrawRect(11, 46, 8, 175, Color.White);
                            DrawWaveform(20, 46, instrument.sample.sampleDataLeft, 175);
                        }
                        sample_loopPingpong.Draw();
                        sample_loopForward.Draw();
                        sample_loopOneshot.Draw();

                        if (instrument.sample.sampleLoopType != SampleLoopType.OneShot)
                        {
                            sample_loopPoint.Value = instrument.sample.sampleLoopIndex;
                            sample_loopPoint.Draw();
                        }

                        sample_resampNone.Draw();
                        sample_resampLinear.Draw();
                        sample_resampMix.Draw();

                        sample_normalize.Draw();
                        sample_reverse.Draw();
                        sample_detune.Draw();
                        sample_baseKey.Draw();
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
            }
        }

        public void DrawWaveform(int x, int y, List<float> data, int height = 87)
        {
            int boxLength = 528;
            int boxHeight = height;
            int samplesPerPixel = data.Count / boxLength;
            int startY = y + boxHeight / 2;
            int lastSampleNum;
            int sampleNum = 0;
            if (data.Count > 0)
            {
                for (int i = 0; i < boxLength; i++)
                {
                    float percentage = (float)i / boxLength;
                    lastSampleNum = sampleNum;
                    sampleNum = (int)(percentage * data.Count - 1);
                    sampleNum = Math.Clamp(sampleNum, 0, data.Count - 1);
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
                    DrawRect(x + (int)((float)instrument.sample.sampleLoopIndex / data.Count * boxLength), y, 1, boxHeight, Color.Yellow);
                if (instrument.sample.currentPlaybackPosition < data.Count && Audio.ChannelManager.instance.channels[Game1.previewChannel].isPlaying)
                    DrawRect(x + (int)((float)instrument.sample.currentPlaybackPosition / data.Count * boxLength), y, 1, boxHeight, Color.Aqua);
            }
        }

        public static void LoadSampleFromFile(Macro macro)
        {
            if (Input.dialogOpenCooldown == 0)
            {
                if (macro.macroType == MacroType.Sample)
                {
                    bool successfulReadWAV = false;
                    bool didReadWAV;
                    string fileName = "";
                    didReadWAV = false;
                    List<float> sampleDataLeft, sampleDataRight;
                    sampleDataLeft = new List<float>();
                    sampleDataRight = new List<float>();
                    Thread t = new Thread((ThreadStart)(() =>
                     {

                         Input.DialogStarted();
                         System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                         openFileDialog.Filter = "Audio Files (*.wav, *.mp3, *.flac)|*.wav;*.mp3;*.flac";
                         openFileDialog.Multiselect = false;
                         openFileDialog.Title = "Import Sample...";
                         sampleDataLeft = new List<float>();
                         sampleDataRight = new List<float>();
                         if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                         {
                             didReadWAV = true;
                             fileName = openFileDialog.SafeFileName;
                             successfulReadWAV = (Helpers.readWav(openFileDialog.FileName, out macro.sample.sampleDataLeft, out macro.sample.sampleDataRight));
                         }

                     }));

                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();
                    if (didReadWAV)
                    {
                        macro.sample.SetBaseKey(48);
                        macro.sample.SetDetune(0);
                        macro.sample.sampleLoopIndex = 0;
                        macro.sample.sampleLoopType = macro.sample.sampleDataLeft.Count < 1000 ? SampleLoopType.Forward : SampleLoopType.OneShot;
                        macro.sample.resampleMode = ResamplingModes.LinearInterpolation;
                        if (successfulReadWAV)
                        {
                            if (Preferences.automaticallyTrimSamples)
                                macro.sample.TrimSilence();
                            if (Preferences.automaticallyNormalizeSamples)
                                macro.sample.Normalize();

                            macro.sample.resampleMode = ResamplingModes.LinearInterpolation;
                        }
                        else
                        {
                            macro.sample.sampleDataLeft = new List<float>();
                            macro.sample.sampleDataRight = new List<float>();
                        }
                    }
                }
            }
        }
    }
}
