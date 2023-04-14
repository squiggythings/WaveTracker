using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using System.Windows.Forms;

namespace WaveTracker.Rendering
{
    public class WaveEditor : Element
    {
        public Texture2D tex;
        public static bool enabled;
        public SpriteButton presetSine, presetTria, presetSaw, presetRect50, presetRect25, presetRect12, presetRand, presetClear;
        public Toggle filterNone, filterLinear, filterMix;
        public int startcooldown;
        public SpriteButton closeButton;
        public UI.Button bCopy, bPaste, bPhaseL, bPhaseR, bMoveUp, bMoveDown, bInvert, bMutate, bSmooth;
        public UI.Textbox waveText;
        public int id;
        int holdPosY, holdPosX;
        static string clipboardWave = "";
        static Audio.ResamplingModes clipboardSampleMode = Audio.ResamplingModes.Mix;
        int phase;
        public WaveEditor(Texture2D tex)
        {
            this.tex = tex;
            x = 220;
            y = 130;

            bCopy = new UI.Button("Copy", 424, 23, this);
            bCopy.width = 54;
            bCopy.isPartOfInternalDialog = true;
            bCopy.SetTooltip("", "Copy wave settings");

            bPaste = new UI.Button("Paste", 424, 37, this);
            bPaste.width = 54;
            bPaste.isPartOfInternalDialog = true;
            bPaste.SetTooltip("", "Paste wave settings");

            bPhaseR = new UI.Button("Phase »", 424, 57, this);
            bPhaseR.width = 54;
            bPhaseR.isPartOfInternalDialog = true;
            bPhaseR.SetTooltip("", "Shift phase once to the right");

            bPhaseL = new UI.Button("Phase «", 424, 71, this);
            bPhaseL.width = 54;
            bPhaseL.isPartOfInternalDialog = true;
            bPhaseL.SetTooltip("", "Shift phase once to the left");

            bMoveUp = new UI.Button("Shift Up", 424, 85, this);
            bMoveUp.width = 54;
            bMoveUp.isPartOfInternalDialog = true;
            bMoveUp.SetTooltip("", "Raise the wave 1 step up");

            bMoveDown = new UI.Button("Shift Down", 424, 99, this);
            bMoveDown.width = 54;
            bMoveDown.isPartOfInternalDialog = true;
            bMoveDown.SetTooltip("", "Lower the wave 1 step down");

            bInvert = new UI.Button("Invert", 424, 119, this);
            bInvert.width = 54;
            bInvert.isPartOfInternalDialog = true;
            bInvert.SetTooltip("", "Invert the wave vertically");

            bSmooth = new UI.Button("Smooth", 424, 133, this);
            bSmooth.width = 54;
            bSmooth.isPartOfInternalDialog = true;
            bSmooth.SetTooltip("", "Smooth out rough corners in the wave");

            bMutate = new UI.Button("Mutate", 424, 147, this);
            bMutate.width = 54;
            bMutate.isPartOfInternalDialog = true;
            bMutate.SetTooltip("", "Slightly randomize the wave");

            presetSine = new SpriteButton(17, 215, 18, 12, tex, 0, this);
            presetSine.isPartOfInternalDialog = true;
            presetSine.SetTooltip("Sine", "Sine wave preset");

            presetTria = new SpriteButton(36, 215, 18, 12, tex, 1, this);
            presetTria.isPartOfInternalDialog = true;
            presetTria.SetTooltip("Triangle", "Triangle wave preset");
            presetSaw = new SpriteButton(55, 215, 18, 12, tex, 2, this);
            presetSaw.isPartOfInternalDialog = true;
            presetSaw.SetTooltip("Sawtooth", "Sawtooth wave preset");

            presetRect50 = new SpriteButton(74, 215, 18, 12, tex, 3, this);
            presetRect50.isPartOfInternalDialog = true;
            presetRect50.SetTooltip("Pulse 50%", "Pulse wave preset with 50% duty cycle");

            presetRect25 = new SpriteButton(93, 215, 18, 12, tex, 4, this);
            presetRect25.isPartOfInternalDialog = true;
            presetRect25.SetTooltip("Pulse 25%", "Pulse wave preset with 25% duty cycle");

            presetRect12 = new SpriteButton(112, 215, 18, 12, tex, 5, this);
            presetRect12.isPartOfInternalDialog = true;
            presetRect12.SetTooltip("Pulse 12.5%", "Pulse wave preset with 12.5% duty cycle");
            presetRand = new SpriteButton(131, 215, 18, 12, tex, 6, this);
            presetRand.SetTooltip("Random", "Create random noise");
            presetRand.isPartOfInternalDialog = true;
            presetClear = new SpriteButton(150, 215, 18, 12, tex, 7, this);
            presetClear.isPartOfInternalDialog = true;
            presetClear.SetTooltip("Clear", "Clear wave");

            closeButton = new SpriteButton(490, 0, 10, 9, UI.NumberBox.buttons, 4, this);
            closeButton.isPartOfInternalDialog = true;
            closeButton.SetTooltip("Close", "Close wave editor");

            filterNone = new Toggle("None", 382, 215, this);
            filterNone.SetTooltip("", "Set the resampling mode to no interpolation. Has a harsher, gritty sound.");
            filterNone.isPartOfInternalDialog = true;
            filterNone.width = 33;

            filterLinear = new Toggle("Linear", 416, 215, this);
            filterLinear.SetTooltip("", "Set the resampling mode to linear interpolation. Has a mellow, softer sound.");
            filterLinear.isPartOfInternalDialog = true;
            filterLinear.width = 33;

            filterMix = new Toggle("Mix", 450, 215, this);
            filterMix.SetTooltip("", "Set the resampling mode to an average between none and linear interpolation.");
            filterMix.isPartOfInternalDialog = true;
            filterMix.width = 33;

            waveText = new UI.Textbox("", 17, 188, 384, 384, this);
            waveText.isPartOfInternalDialog = true;
            waveText.canEdit = true;
            waveText.maxLength = 192;
        }
        public void EditWave(Wave wave, int num)
        {
            Input.internalDialogIsOpen = true;
            startcooldown = 10;
            id = num;
            enabled = true;
        }

        public void Close()
        {
            enabled = false;
            Input.internalDialogIsOpen = false;
        }

        public int pianoInput()
        {
            if (!enabled)
                return -1;
            if (MouseX < 10 || MouseX > 488 || MouseY > 258 || MouseY < 235)
                return -1;
            if (!Input.GetClick(KeyModifier.None))
                return -1;
            else
            {
                return (MouseX - 9) / 4;
            }
        }

        bool mouseInBounds()
        {
            return canvasMouseX > 0 && canvasMouseY > 0 && canvasMouseX < 384 && canvasMouseY < 160;
        }
        int canvasMouseX => MouseX - 17;
        int canvasMouseY => MouseY - 23;
        int canvasPosX => canvasMouseX / 6;
        int canvasPosY => 31 - (canvasMouseY / 5);

        public void Update()
        {
            if (enabled)
            {
                phase++;
                filterNone.Value = Game1.currentSong.waves[WaveBank.currentWave].resamplingMode == Audio.ResamplingModes.None;
                filterLinear.Value = Game1.currentSong.waves[WaveBank.currentWave].resamplingMode == Audio.ResamplingModes.Linear;
                filterMix.Value = Game1.currentSong.waves[WaveBank.currentWave].resamplingMode == Audio.ResamplingModes.Mix;
                if (startcooldown > 0)
                {
                    waveText.Text = Game1.currentSong.waves[WaveBank.currentWave].ToNumberString();
                    startcooldown--;
                }
                else
                {
                    if (closeButton.Clicked || Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape, KeyModifier.None))
                    {
                        Close();
                    }
                    waveText.Update();
                    if (waveText.ValueWasChanged)
                    {
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromNumber(waveText.Text);
                    }
                    else
                    {
                        waveText.Text = Game1.currentSong.waves[WaveBank.currentWave].ToNumberString();
                    }
                    if (filterNone.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].resamplingMode = Audio.ResamplingModes.None;
                    if (filterLinear.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].resamplingMode = Audio.ResamplingModes.Linear;
                    if (filterMix.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].resamplingMode = Audio.ResamplingModes.Mix;

                    if (presetSine.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("HJKMNOQRSTUUVVVVVVVUUTSRQONMKJHGECB9875432110000000112345789BCEF");
                    if (presetTria.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("GHIJKLMNOPQRSTUVVUTSRQPONMLKJIHGFEDCBA98765432100123456789ABCDEF");
                    if (presetSaw.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("GGHHIIJJKKLLMMNNOOPPQQRRSSTTUUVV00112233445566778899AABBCCDDEEFF");
                    if (presetRect50.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV00000000000000000000000000000000");
                    if (presetRect25.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("VVVVVVVVVVVVVVVV000000000000000000000000000000000000000000000000");
                    if (presetRect12.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("VVVVVVVV00000000000000000000000000000000000000000000000000000000");

                    if (presetRand.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].Randomize();
                    if (presetClear.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG");

                    if (bCopy.Clicked)
                    {
                        clipboardWave = Game1.currentSong.waves[WaveBank.currentWave].ToString();
                        clipboardSampleMode = Game1.currentSong.waves[WaveBank.currentWave].resamplingMode;
                    }
                    if (bPaste.Clicked)
                    {
                        if (clipboardWave.Length == 64)
                        {
                            Game1.currentSong.waves[WaveBank.currentWave].SetWaveformFromString(clipboardWave);
                            Game1.currentSong.waves[WaveBank.currentWave].resamplingMode = clipboardSampleMode;
                        }
                    }

                    if (bPhaseL.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].ShiftPhase(1);

                    if (bPhaseR.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].ShiftPhase(-1);
                    if (bMoveUp.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].Move(1);
                    if (bMoveDown.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].Move(-1);
                    if (bInvert.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].Invert();
                    if (bSmooth.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].Smooth(2);
                    if (bMutate.Clicked)
                        Game1.currentSong.waves[WaveBank.currentWave].Mutate();

                    if (mouseInBounds())
                    {
                        if (Input.GetClickDown(KeyModifier._Any))
                        {
                            holdPosX = canvasPosX;
                            holdPosY = canvasPosY;
                        }


                        if (Input.GetClick(KeyModifier.None))
                        {
                            Game1.currentSong.waves[WaveBank.currentWave].samples[canvasPosX] = (byte)canvasPosY;
                        }
                        if (Input.GetClickUp(KeyModifier.Shift))
                        {
                            int diff = Math.Abs(holdPosX - canvasPosX);
                            if (diff > 0)
                            {
                                if (holdPosX < canvasPosX)
                                {
                                    for (int i = holdPosX; i <= canvasPosX; ++i)
                                    {
                                        Game1.currentSong.waves[WaveBank.currentWave].samples[i] = (byte)Math.Round(Lerp(holdPosY, canvasPosY, (float)(i - holdPosX) / diff));
                                    }
                                }
                                else
                                {
                                    for (int i = canvasPosX; i <= holdPosX; ++i)
                                    {
                                        Game1.currentSong.waves[WaveBank.currentWave].samples[i] = (byte)Math.Round(Lerp(canvasPosY, holdPosY, (float)(i - canvasPosX) / diff));
                                    }
                                }
                            }
                            else
                            {
                                Game1.currentSong.waves[WaveBank.currentWave].samples[canvasPosX] = (byte)canvasPosY;
                            }
                        }
                    }
                }
            }
        }
        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        public void Draw()
        {
            if (enabled)
            {
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 60));
                DrawSprite(tex, 0, 0, new Rectangle(0, 60, 500, 270));
                Write("Edit Wave " + id.ToString("D2"), 4, 1, new Color(64, 72, 115));
                closeButton.Draw();
                presetSine.Draw();
                presetTria.Draw();
                presetRect50.Draw();
                presetSaw.Draw();
                presetRect25.Draw();
                presetRect12.Draw();
                presetClear.Draw();
                presetRand.Draw();
                filterNone.Draw();
                filterLinear.Draw();
                filterMix.Draw();
                waveText.Draw();

                bCopy.Draw();
                bPaste.Draw();
                bPhaseL.Draw();
                bPhaseR.Draw();
                bMoveUp.Draw();
                bMoveDown.Draw();
                bInvert.Draw();
                bSmooth.Draw();
                bMutate.Draw();

                Color waveColor = new Color(200, 212, 93);
                Color waveBG = new Color(59, 125, 79, 150);
                for (int i = 0; i < 64; ++i)
                {
                    int samp = Game1.currentSong.waves[WaveBank.currentWave].getSample(i);
                    int samp2 = Game1.currentSong.waves[WaveBank.currentWave].getSample(i + phase);

                    DrawRect(17 + i * 6, 102, 6, -5 * (samp - 16), waveBG);
                    DrawRect(17 + i * 6, 183 - samp * 5, 6, -5, waveColor);
                    DrawRect(419 + i, 183, 1, 16 - samp2, new Color(190, 192, 211));
                    DrawRect(419 + i, 199 - samp2, 1, 1, new Color(118, 124, 163));
                }
                if (mouseInBounds())
                {
                    DrawRect(17 + (canvasMouseX / 6 * 6), 183 - ((31 - canvasMouseY / 5) * 5), 6, -5, Helpers.Alpha(Color.White, 80));
                    if (Input.GetClick(KeyModifier.Shift))
                    {
                        int diff = Math.Abs(holdPosX - canvasPosX);
                        if (diff > 0)
                        {
                            if (holdPosX < canvasPosX)
                            {
                                for (int i = holdPosX; i <= canvasPosX; ++i)
                                {
                                    int y = (int)Math.Round(Lerp(holdPosY, canvasPosY, (float)(i - holdPosX) / diff));
                                    DrawRect(17 + i * 6, 183 - y * 5, 6, -5, Helpers.Alpha(Color.White, 80));
                                }
                            }
                            else
                            {
                                for (int i = canvasPosX; i <= holdPosX; ++i)
                                {
                                    int y = (int)Math.Round(Lerp(canvasPosY, holdPosY, (float)(i - canvasPosX) / diff));
                                    DrawRect(17 + i * 6, 183 - y * 5, 6, -5, Helpers.Alpha(Color.White, 80));
                                }
                            }
                        }
                    }
                }

                if (Game1.pianoInput > -1)
                {
                    if (Helpers.isNoteBlackKey(Game1.pianoInput))
                        DrawSprite(tex, Game1.pianoInput * 4 + 10, 235, new Rectangle(504, 61, 4, 24));
                    else
                        DrawSprite(tex, Game1.pianoInput * 4 + 10, 235, new Rectangle(500, 61, 4, 24));
                }
            }
        }
    }
}
