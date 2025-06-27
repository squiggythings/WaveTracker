﻿using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class ExportingDialog : Window {
        public string Path { get; set; }
        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }

        public Button Cancel;
        public ExportingDialog() : base("Exporting .wav", 300, 88, hasExitButton: false) {
            Cancel = new Button("Cancel", width / 2 - 25, 72, this);
            Cancel.width = 51;
            Cancel.LabelIsCentered = true;

        }

        public void Update() {
            if (WindowIsOpen) {
                if (Tracker.Playback.IsPlaying) {
                    Cancel.SetLabel("Cancel");
                    if (Cancel.Clicked) {
                        Audio.AudioEngine.CancelRenderFlag = true;
                        Close();
                    }
                }
                else {
                    Cancel.SetLabel("Done");
                    if (Cancel.Clicked) {
                        Close();
                    }
                }
            }
        }

        public new void Open() {
            base.Open();
            Cancel.SetLabel("Cancel");
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                int barwidth = width - 20;
                int maxRow = Audio.AudioEngine.RenderTotalRows;
                int procRow = Math.Clamp(Audio.AudioEngine.RenderProcessedRows, 0, maxRow);

                float fraction = (float)procRow / maxRow;
                WriteCenter(Helpers.TrimTextToWidth(width - 20, "Saving to " + Path), width / 2, 16, UIColors.label);
                DrawRect(10, 29, barwidth, 1, UIColors.labelLight);
                WriteCenter("Row " + procRow + "/" + maxRow + ": (" + (int)(fraction * 100) + "% done)", width / 2, 35, UIColors.label);
                Color shadow = new Color(126, 133, 168);
                Color grey = new Color(163, 167, 194);
                Color bar = new Color(0, 219, 39);
                DrawRect(10, 49, barwidth, 8, grey);
                DrawRect(10, 48, barwidth, 1, shadow);
                DrawRect(10, 49, (int)(barwidth * fraction), 8, bar);
                DrawRect(10, 64, barwidth, 1, UIColors.labelLight);
                Cancel.Draw();
            }
        }

    }
}
