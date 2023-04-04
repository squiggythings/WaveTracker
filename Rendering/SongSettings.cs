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


namespace WaveTracker.Rendering
{
    public class SongSettings : UI.Panel
    {
        Forms.EditSongSettings dialog;
        bool dialogOpen;
        Textbox title, author, copyright, speed, rows;
        SpriteButton editButton;
        float[,] linearRMS = new float[2, 10];
        int RMScounter;
        int ampL, ampR;
        public void Initialize(Texture2D editButtonsource)
        {
            title = new Textbox("Title", 4, 12, 155, 110, this);
            title.canEdit = false;
            author = new Textbox("Author", 4, 26, 155, 110, this);
            author.canEdit = false;
            copyright = new Textbox("Copyright", 4, 40, 155, 110, this);
            copyright.canEdit = false;

            speed = new Textbox("Speed (ticks/row)", 167, 12, 132, 40, this);
            speed.canEdit = false;
            rows = new Textbox("Frame Length", 167, 26, 132, 40, this);
            rows.canEdit = false;

            editButton = new SpriteButton(296, 0, 10, 9, editButtonsource, 0, this);
            editButton.SetTooltip("Edit Song Settings", "Open the song settings editing window");
            InitializePanel("Song", 2, 18, 306, 84);
        }

        public void Update()
        {

            title.Text = Game1.currentSong.name;
            author.Text = Game1.currentSong.author;
            copyright.Text = Game1.currentSong.year;
            speed.Text = Game1.currentSong.GetTicksAsString();
            rows.Text = Game1.currentSong.rowsPerFrame + "";
            if (editButton.Clicked)
            {
                if (!dialogOpen)
                {
                    dialogOpen = true;
                    StartDialog();
                }
            }
            else
            {
                dialogOpen = false;
            }
        }
        public void Draw()
        {
            DrawPanel();
            editButton.Draw();
            title.Draw();
            author.Draw();
            copyright.Draw();
            speed.Draw();
            rows.Draw();
            if (Preferences.oscilloscopeMode == 1)
                DrawMonoOscilloscope(166, 44, 135, 35, new Color(56, 64, 102));
            if (Preferences.oscilloscopeMode == 2)
                DrawStereoOscilloscope(166, 44, 135, 35, new Color(56, 64, 102));
            if (Preferences.oscilloscopeMode == 3)
                DrawOverlappedOscilloscope(166, 44, 135, 35, new Color(56, 64, 102));
            DrawVolumeMeters(16, 70, 143, 4);
        }

        public void DrawVolumeMeters(int px, int py, int width, int height)
        {
            Color grey = new Color(163, 167, 194);
            #region drawletters
            DrawRect(px - 7, py, 1, 4, grey);
            DrawRect(px - 6, py + 3, 2, 1, grey);

            DrawRect(px - 7, py + height + 1, 1, 4, grey);
            DrawRect(px - 6, py + height + 1, 1, 1, grey);
            DrawRect(px - 6, py + height + 3, 1, 1, grey);
            DrawRect(px - 5, py + height + 2, 1, 1, grey);
            DrawRect(px - 5, py + height + 4, 1, 1, grey);
            #endregion

            float[,] samples = Audio.AudioEngine.instance.currentBuffer;
            float avgL = 0;
            float avgR = 0;

            for (int i = 0; i < Audio.AudioEngine.instance.SamplesPerBuffer; i++)
            {
                avgL += Math.Abs(samples[0, i]) * 2;
                avgR += Math.Abs(samples[1, i]) * 2;
            }
            avgL /= Audio.AudioEngine.instance.SamplesPerBuffer;
            avgR /= Audio.AudioEngine.instance.SamplesPerBuffer;
            RMScounter++;
            if (RMScounter >= linearRMS.GetLength(1))
                RMScounter = 0;
            linearRMS[0, RMScounter] = avgL;
            linearRMS[1, RMScounter] = avgR;
            avgL = 0;
            avgR = 0;
            for (int i = 0; i < linearRMS.GetLength(1); i++)
            {
                avgL += linearRMS[0, i];
                avgR += linearRMS[1, i];
            }

            avgL /= linearRMS.GetLength(1);
            avgR /= linearRMS.GetLength(1);
            float linearRMSL = (float)Math.Sqrt(avgL);
            float linearRMSR = (float)Math.Sqrt(avgR);

            int ampLeft = (int)(Math.Clamp(linearRMSL, 0, 1) * width);
            int ampRight = (int)(Math.Clamp(linearRMSR, 0, 1) * width);
            ampL = ampLeft;
            ampR = ampRight;


            Color shadow = new Color(126, 133, 168);
            Color bar = new Color(0, 219, 39);

            DrawRect(px, py, width, height, grey);
            DrawRect(px, py, width, 1, shadow);
            DrawRect(px, py + 1, ampL, height - 1, linearRMSL > 1 ? Color.Red : bar);

            DrawRect(px, py + height + 1, width, height, grey);
            DrawRect(px, py + height + 1, width, 1, shadow);
            DrawRect(px, py + height + 2, ampR, height - 1, linearRMSR > 1 ? Color.Red : bar);

            for (int i = 0; i < Tracker.Song.CHANNEL_COUNT; i++)
            {
                DrawRect(px + i * 6, py - 9, 5, 5, Helpers.LerpColor(grey, bar, Audio.ChannelManager.instance.channels[i].CurrentAmplitude));
            }
        }

        public void DrawOverlappedOscilloscope(int px, int py, int width, int height, Color back)
        {
            DrawRect(px, py, width, height, back);
            float[,] samples = Audio.AudioEngine.instance.currentBuffer;
            int i = 0;
            int drawX = 0;
            int zoomX = Audio.AudioEngine.instance.SamplesPerBuffer / width;
            while (drawX < width - 2)
            {
                int[] ys = new int[zoomX];
                int minValR = 99;
                int maxValR = -99;
                int minValL = 99;
                int maxValL = -99;
                for (int j = 0; j < zoomX; j++)
                {
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
                else
                {
                    float actualMax = Math.Max(Math.Max(Math.Abs(maxValR), Math.Abs(minValR)), Math.Max(Math.Abs(maxValL), Math.Abs(minValL)));
                    float distMin = Math.Abs(minValL - minValR) / actualMax / 1.15f;
                    float distMax = Math.Abs(maxValL - maxValR) / actualMax / 1.15f;
                    float dist = Math.Clamp((distMin + distMax) / 2f, 0, 1);
                    DrawRect(1 + px + drawX, minValL + py + (height / 2), 1, maxValL - minValL + 1, Helpers.LerpColor(Color.Gray, Color.White, 1 - dist));
                    DrawRect(1 + px + drawX, minValR + py + (height / 2), 1, maxValR - minValR + 1, Helpers.LerpColor(Color.Gray, Color.White, 1 - dist));
                    if (drawX == 0)
                        Write(distMin + " " + distMax + " " + dist, 200, 480, Color.Red);

                }
                drawX++;
            }
        }

        public void DrawMonoOscilloscope(int px, int py, int width, int height, Color back)
        {
            DrawRect(px, py, width, height, back);
            float[,] samples = Audio.AudioEngine.instance.currentBuffer;
            int i = 0;
            int drawX = 0;
            int zoomX = Audio.AudioEngine.instance.SamplesPerBuffer / width;
            while (drawX < width - 2)
            {

                int minValL = 99;
                int maxValL = -99;
                for (int j = 0; j < zoomX; j++)
                {
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

        public void DrawStereoOscilloscope(int px, int py, int width, int height, Color back)
        {
            DrawRect(px, py, width, height, back);
            float[,] samples = Audio.AudioEngine.instance.currentBuffer;
            int i = 0;
            int drawX = 0;
            int zoomX = Audio.AudioEngine.instance.SamplesPerBuffer / width * 2;
            while (drawX < width / 2 - 1)
            {
                int[] ys = new int[zoomX];
                int minVal = 99;
                int maxVal = -99;
                for (int j = 0; j < zoomX; j++)
                {
                    if (i >= samples.GetLength(1))
                        break;
                    int sampleL = (int)Math.Round(Math.Clamp((-samples[1, i] * 20), height / -2, height / 2));
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
            while (drawX < 66)
            {
                int[] ys = new int[zoomX];
                int minVal = 99;
                int maxVal = -99;
                for (int j = 0; j < zoomX; j++)
                {
                    if (i >= samples.GetLength(1))
                        break;
                    int sampleL = (int)Math.Round(Math.Clamp((-samples[1, i] * 20), height / -2, height / 2));
                    int sampleR = (int)Math.Round(Math.Clamp((-samples[0, i] * 20), height / -2, height / 2));
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

        public void StartDialog()
        {
            Input.DialogStarted();
            dialog = new Forms.EditSongSettings();
            dialog.title.Text = title.Text;
            dialog.author.Text = author.Text;
            dialog.copyright.Text = copyright.Text;
            dialog.songSpeed.Text = speed.Text;
            dialog.frameLength.Value = Game1.currentSong.rowsPerFrame;
            dialog.tickRate.Value = Game1.currentSong.tickRate;
            dialog.quantizeChannelAmplitude.Checked = Game1.currentSong.quantizeChannelAmplitude;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Game1.currentSong.name = Helpers.FlushString(dialog.title.Text);
                Game1.currentSong.author = Helpers.FlushString(dialog.author.Text);
                Game1.currentSong.year = Helpers.FlushString(dialog.copyright.Text);
                Game1.currentSong.LoadTicksFromString(dialog.songSpeed.Text);
                Game1.currentSong.rowsPerFrame = (int)dialog.frameLength.Value;
                Game1.currentSong.tickRate = dialog.tickRate.Value;
                Game1.currentSong.quantizeChannelAmplitude = dialog.quantizeChannelAmplitude.Checked;
            }
        }
    }
}
