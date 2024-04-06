using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;
using WaveTracker.Audio;

namespace WaveTracker.UI {
    public class SampleEditor : Element {
        public Sample Sample { get; set; }

        MouseRegion waveformRegion;
        int mouseSampleIndex;
        NumberBox baseKey;
        NumberBox fineTune;
        Button importSample;
        Dropdown resamplingMode;
        public SampleEditor() {
            waveformRegion = new MouseRegion(0, 10, 562, 175, this);
            baseKey = new NumberBox("Base Key", 0, 220, this);
            baseKey.SetTooltip("", "The note where the sample is played at its original pitch");

            fineTune = new NumberBox("Fine tune", 0, 237, this);
            fineTune.SetValueLimits(-199, 199);
            fineTune.DisplayMode = NumberBox.NumberDisplayMode.PlusMinus;
            fineTune.SetTooltip("", "Slightly adjust the pitch of the sample, in cents");
        }

        public void Update() {
            if (waveformRegion.IsHovered) {
                mouseSampleIndex = (int)(waveformRegion.MouseXClamped * Sample.Length);
            }
            else {
                mouseSampleIndex = -1;
            }
        }

        public void Draw() {
            if (Sample == null) {
                return;
            }

            Write(Sample.Length + " samples", waveformRegion.x, waveformRegion.y + 9, UIColors.label);
            WriteRightAlign((Sample.Length / (float)AudioEngine.SAMPLE_RATE).ToString("F5") + " seconds", waveformRegion.x + waveformRegion.width, waveformRegion.y + 9, UIColors.label);

            if (Sample.IsStereo) {
                DrawWaveform(waveformRegion.x, waveformRegion.y, Sample.sampleDataAccessL, waveformRegion.width, waveformRegion.height / 2);
                DrawWaveform(waveformRegion.x, waveformRegion.y + waveformRegion.height / 2 + 1, Sample.sampleDataAccessR, waveformRegion.width, waveformRegion.height / 2);
            }
            else {
                DrawRect(waveformRegion.x, waveformRegion.y, waveformRegion.width, waveformRegion.height, UIColors.black);
                DrawWaveform(waveformRegion.x, waveformRegion.y, Sample.sampleDataAccessL, waveformRegion.width, waveformRegion.height);
            }
        }

        void DrawWaveform(int x, int y, short[] data, int width, int height) {
            int startY = y + height / 2;
            int lastSampleNum;
            int sampleNum = 0;
            Color sampleColor = new Color(207, 117, 43);
            DrawRect(x, y, width, height, UIColors.black);
            if (data.Length > 0) {
                for (int i = 0; i < width; i++) {
                    float percentage = (float)i / width;
                    lastSampleNum = sampleNum;
                    sampleNum = (int)(percentage * data.Length - 1);
                    sampleNum = Math.Clamp(sampleNum, 0, data.Length - 1);
                    float min = 1;
                    float max = -1;
                    for (int j = lastSampleNum; j <= sampleNum; j++) {
                        float val = data[j] / (float)(short.MaxValue);
                        if (val < min) {
                            min = val;
                        }
                        if (val > max) {
                            max = val;
                        }
                    }
                    min *= height / 2;
                    max *= height / 2;
                    if (i > 0)
                        DrawRect(x + i - 1, startY - (int)max, 1, (int)(max - min) + 1, sampleColor);

                }

                if (Sample.loopType != Sample.LoopType.OneShot) {
                    int loopPosition = (int)((float)Sample.sampleLoopIndex / data.Length * width);
                    DrawRect(x + loopPosition, y, 1, height, Color.Yellow);
                    DrawRect(x + loopPosition, y, height - loopPosition, height, Helpers.Alpha(Color.Yellow, 50));
                }
                if (Sample.currentPlaybackPosition < data.Length && Audio.ChannelManager.previewChannel.IsPlaying) {
                    DrawRect(x + (int)((float)Sample.currentPlaybackPosition / data.Length * width), y, 1, height, Color.Aqua);
                }
                DrawRect(x + (int)((float)mouseSampleIndex / data.Length * width), y, 1, height, Helpers.Alpha(Color.White, 128));

            }
        }
    }
}
