using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class SongSettings : Panel {
        Forms.EditSongSettings dialog;
        bool dialogOpen;
        Textbox title, author, copyright, speed;
        NumberBox rows;
        SpriteButton editButton;
        float ampLeft, ampRight;
        int ampL, ampR;
        public void Initialize(Texture2D editButtonsource) {
            title = new Textbox("Title", 4, 12, 155, 110, this);

            //title.canEdit = false;
            author = new Textbox("Author", 4, 26, 155, 110, this);
            //author.canEdit = false;
            copyright = new Textbox("Copyright", 4, 40, 155, 110, this);
            // copyright.canEdit = false;

            speed = new Textbox("Speed (ticks/row)", 167, 12, 132, 40, this);
            // speed.canEdit = false;
            rows = new NumberBox("Frame Length", 167, 26, 132, 40, this);
            rows.SetValueLimits(1, 256);
            //rows.canEdit = false;

            editButton = new SpriteButton(296, 0, 10, 9, editButtonsource, 0, this);
            editButton.SetTooltip("Edit Song Settings", "Open the song settings editing window");
            InitializePanel("Song", 2, 18, 306, 84);
        }

        public void Update() {

            title.Text = App.CurrentModule.Title;
            title.Update();
            if (title.ValueWasChangedInternally) {
                App.CurrentModule.Title = title.Text;
            }

            author.Text = App.CurrentModule.Author;
            author.Update();
            if (author.ValueWasChangedInternally) {
                App.CurrentModule.Author = author.Text;
            }

            copyright.Text = App.CurrentModule.Year;
            copyright.Update();
            if (copyright.ValueWasChangedInternally) {
                App.CurrentModule.Year = copyright.Text;
            }

            speed.Text = App.CurrentSong.GetTicksAsString();
            speed.Update();
            if (speed.ValueWasChangedInternally) {
                App.CurrentSong.LoadTicksFromString(speed.Text);
            }


            rows.Value = App.CurrentSong.RowsPerFrame;
            rows.Update();
            if (rows.ValueWasChangedInternally) {
                App.CurrentSong.RowsPerFrame = rows.Value;
                App.PatternEditor.cursorPosition.Normalize(App.CurrentSong);

            }
            if (editButton.Clicked) {
                if (!dialogOpen) {
                    dialogOpen = true;
                    StartDialog();
                }
            }
            else {
                dialogOpen = false;
            }


            float meterDecay = 0;
            switch (Preferences.profile.meterDecaySpeed) {
                case 0:
                    meterDecay = 0.5f;
                    break;
                case 1:
                    meterDecay = 1.25f;
                    break;
                case 2:
                    meterDecay = 5;
                    break;
            }
            ampLeft *= 1 - meterDecay / 10f;
            ampRight *= 1 - meterDecay / 10f;
        }
        public void Draw() {
            DrawPanel();
            editButton.Draw();
            title.Draw();
            author.Draw();
            copyright.Draw();
            speed.Draw();
            rows.Draw();
            if (Audio.AudioEngine.currentBuffer != null) {
                if (Audio.AudioEngine.currentBuffer.Length > 0) {
                    if (Preferences.profile.oscilloscopeMode == 0)
                        DrawMonoOscilloscope(166, 44, 135, 35, new Color(56, 64, 102));
                    if (Preferences.profile.oscilloscopeMode == 1)
                        DrawStereoOscilloscope(166, 44, 135, 35, new Color(56, 64, 102));
                    if (Preferences.profile.oscilloscopeMode == 2)
                        DrawOverlappedOscilloscope(166, 44, 135, 35, new Color(56, 64, 102));
                    DrawVolumeMeters(16, 70, 143, 4);
                }
            }
        }


        public void DrawVolumeMeters(int px, int py, int width, int height) {
            Color grey = new Color(163, 167, 194);
            #region draw letters
            DrawRect(px - 7, py, 1, 4, grey);
            DrawRect(px - 6, py + 3, 2, 1, grey);

            DrawRect(px - 7, py + height + 1, 1, 4, grey);
            DrawRect(px - 6, py + height + 1, 1, 1, grey);
            DrawRect(px - 6, py + height + 3, 1, 1, grey);
            DrawRect(px - 5, py + height + 2, 1, 1, grey);
            DrawRect(px - 5, py + height + 4, 1, 1, grey);
            #endregion

            for (int i = 0; i < Audio.AudioEngine.currentBuffer.GetLength(1); i++) {
                float l = Math.Abs(Audio.AudioEngine.currentBuffer[0, i]);
                float r = Math.Abs(Audio.AudioEngine.currentBuffer[1, i]);
                if (l > ampLeft)
                    ampLeft = l;
                if (r > ampRight)
                    ampRight = r;
            }
            double dbL = 20 * Math.Log10(ampLeft);
            double dbR = 20 * Math.Log10(ampRight);
            ampL = (int)Helpers.MapClamped((float)dbL, -60, 0, 0, width);
            ampR = (int)Helpers.MapClamped((float)dbR, -60, 0, 0, width);

            Color shadow = new Color(126, 133, 168);
            Color bar = new Color(0, 219, 39);

            // draw volume bar frames
            DrawRect(px, py, width, height, grey);
            DrawRect(px, py, width, 1, shadow);
            DrawRect(px, py + height + 1, width, height, grey);
            DrawRect(px, py + height + 1, width, 1, shadow);

            // draw volume bars
            if (Preferences.profile.meterColorMode == 0) // flat
            {
                Color barCol = ampLeft >= 1 && Preferences.profile.meterFlashWhenClipping ? Color.Red : bar;
                DrawRect(px, py + 1, ampL, height - 1, barCol);
                barCol = ampRight >= 1 && Preferences.profile.meterFlashWhenClipping ? Color.Red : bar;
                DrawRect(px, py + height + 2, ampR, height - 1, barCol);
            }
            else // gradient
            {
                for (int x = 0; x < ampL; x++) {
                    Color barCol = ampLeft >= 1 && Preferences.profile.meterFlashWhenClipping ? Color.Red : Helpers.HSLtoRGB((int)Helpers.MapClamped(x, width * 0.6667f, width, 130, 10), 1, 0.42f);
                    DrawRect(px + x, py + 1, 1, height - 1, barCol);
                }
                for (int x = 0; x < ampR; x++) {
                    Color barCol = ampRight >= 1 && Preferences.profile.meterFlashWhenClipping ? Color.Red : Helpers.HSLtoRGB((int)Helpers.MapClamped(x, width * 0.6667f, width, 130, 10), 1, 0.42f);
                    DrawRect(px + x, py + height + 2, 1, height - 1, barCol);
                }
            }

            // draw channel squares
            for (int i = 0; i < Tracker.Song.CHANNEL_COUNT; i++) {
                DrawRect(px + i * 6, py - 9, 5, 5, Helpers.LerpColor(grey, bar, Math.Clamp(Audio.ChannelManager.channels[i].CurrentAmplitude, 0, 1)));
            }
        }

        public void DrawOverlappedOscilloscope(int px, int py, int width, int height, Color back) {
            DrawRect(px, py, width, height, back);
            float[,] samples = Audio.AudioEngine.currentBuffer;
            int i = 0;
            int drawX = 0;
            int zoomX = Audio.AudioEngine.SamplesPerBuffer / width;
            while (drawX < width - 2) {
                int[] ys = new int[zoomX];
                int minValR = 99;
                int maxValR = -99;
                int minValL = 99;
                int maxValL = -99;
                for (int j = 0; j < zoomX; j++) {
                    if (i >= samples.GetLength(1))
                        break;
                    int sampleL = (int)Math.Round(Math.Clamp((-samples[1, i] * 20), height / -2, height / 2));
                    int sampleR = (int)Math.Round(Math.Clamp((-samples[0, i] * 20), height / -2, height / 2));
                    if (sampleL < minValL)
                        minValL = sampleL;
                    if (sampleL > maxValL)
                        maxValL = sampleL;
                    if (sampleR < minValR)
                        minValR = sampleR;
                    if (sampleR > maxValR)
                        maxValR = sampleR;
                    i++;

                }

                if (minValL == minValR && maxValR == maxValL)
                    DrawRect(1 + px + drawX, minValL + py + (height / 2), 1, maxValL - minValL + 1, Color.White);
                else {
                    float actualMax = Math.Max(Math.Max(Math.Abs(maxValR), Math.Abs(minValR)), Math.Max(Math.Abs(maxValL), Math.Abs(minValL)));
                    float distMin = Math.Abs(minValL - minValR) / actualMax / 1.15f;
                    float distMax = Math.Abs(maxValL - maxValR) / actualMax / 1.15f;
                    float dist = Math.Clamp((distMin + distMax) / 2f, 0, 1);
                    DrawRect(1 + px + drawX, minValL + py + (height / 2), 1, maxValL - minValL + 1, Helpers.LerpColor(Color.Gray, Color.White, 1 - dist));
                    DrawRect(1 + px + drawX, minValR + py + (height / 2), 1, maxValR - minValR + 1, Helpers.LerpColor(Color.Gray, Color.White, 1 - dist));
                }
                drawX++;
            }
        }

        public void DrawMonoOscilloscope(int px, int py, int width, int height, Color back) {
            DrawRect(px, py, width, height, back);
            float[,] samples = Audio.AudioEngine.currentBuffer;
            int i = 0;
            int drawX = 0;
            int zoomX = Audio.AudioEngine.SamplesPerBuffer / width;
            while (drawX < width - 2) {

                int minValL = 99;
                int maxValL = -99;
                for (int j = 0; j < zoomX; j++) {
                    if (i >= samples.GetLength(1))
                        break;
                    float sampleL = Math.Clamp(-samples[1, i] * 20, height / -2, height / 2);
                    float sampleR = Math.Clamp(-samples[0, i] * 20, height / -2, height / 2);
                    int sample = (int)Math.Round((sampleL + sampleR) / 2f);
                    if (sample < minValL)
                        minValL = sample;
                    if (sample > maxValL)
                        maxValL = sample;
                    i++;

                }
                i--;
                DrawRect(1 + px + drawX, minValL + py + (height / 2), 1, maxValL - minValL + 1, Color.White);
                drawX++;
            }
        }

        public void DrawStereoOscilloscope(int px, int py, int width, int height, Color back) {
            Random rand = new Random();
            DrawRect(px, py, width, height, back);
            float[,] samples = Audio.AudioEngine.currentBuffer;
            int i = 0;
            int drawX = 0;
            int zoomX = Audio.AudioEngine.SamplesPerBuffer / width * 2;
            while (drawX < width / 2 - 1) {
                int[] ys = new int[zoomX];
                int minVal = 99;
                int maxVal = -99;
                for (int j = 0; j < zoomX; j++) {
                    if (i >= samples.GetLength(1))
                        break;
                    int sampleR = (int)Math.Round(Math.Clamp((-samples[0, i] * 20), height / -2, height / 2));

                    int sample = sampleR;
                    if (sample < minVal)
                        minVal = sample;
                    if (sample > maxVal)
                        maxVal = sample;
                    i++;

                }
                i--;

                DrawRect(1 + px + drawX, minVal + py + (height / 2), 1, maxVal - minVal + 1, Color.White);
                drawX++;
            }
            drawX = 0;
            i = 0;
            while (drawX < 66) {
                int[] ys = new int[zoomX];
                int minVal = 99;
                int maxVal = -99;
                for (int j = 0; j < zoomX; j++) {
                    if (i >= samples.GetLength(1))
                        break;
                    int sampleL = (int)Math.Round(Math.Clamp((-samples[1, i] * 20), height / -2, height / 2));

                    int sample = sampleL;
                    if (sample < minVal)
                        minVal = sample;
                    if (sample > maxVal)
                        maxVal = sample;
                    i++;

                }
                i--;

                DrawRect(68 + px + drawX, minVal + py + 17, 1, maxVal - minVal + 1, Color.White);
                drawX++;
            }
        }

        public void StartDialog() {
            Input.DialogStarted();
            dialog = new Forms.EditSongSettings();
            dialog.title.Text = title.Text;
            dialog.author.Text = author.Text;
            dialog.copyright.Text = copyright.Text;
            dialog.songSpeed.Text = speed.Text;
            dialog.frameLength.Value = Song.currentSong.rowsPerFrame;
            dialog.tickRate.Value = Song.currentSong.tickRate;
            dialog.quantizeChannelAmplitude.Checked = Song.currentSong.quantizeChannelAmplitude;
            if (dialog.ShowDialog() == DialogResult.OK) {
                Song.currentSong.name = Helpers.FlushString(dialog.title.Text);
                Song.currentSong.author = Helpers.FlushString(dialog.author.Text);
                Song.currentSong.year = Helpers.FlushString(dialog.copyright.Text);
                Song.currentSong.LoadTicksFromString(dialog.songSpeed.Text);
                Song.currentSong.rowsPerFrame = (int)dialog.frameLength.Value;
                Song.currentSong.tickRate = dialog.tickRate.Value;
                Song.currentSong.quantizeChannelAmplitude = dialog.quantizeChannelAmplitude.Checked;
            }
        }
    }
}
