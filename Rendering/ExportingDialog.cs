using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;


namespace WaveTracker.Rendering
{
    public class ExportingDialog : Panel
    {
        bool enabled;
        public string Path { get; set; }
        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }

        public Button Cancel;
        public ExportingDialog()
        {
            InitializePanelCentered("Exporting .wav", 300, 88);
            Cancel = new Button("Cancel", width / 2 - 25, 72, this);
            Cancel.width = 51;
            Cancel.centerLabel = true;

        }

        public void Update()
        {
            if (enabled)
            {
                if (Tracker.Playback.isPlaying)
                {
                    Cancel.SetLabel("Cancel");
                    if (Cancel.Clicked)
                    {
                        Audio.AudioEngine.cancelRender = true;
                        Close();
                    }
                }
                else
                {
                    Cancel.SetLabel("Done");
                    if (Cancel.Clicked)
                    {
                        Close();
                    }
                }
            }
        }

        public void Open()
        {
            Cancel.SetLabel("Cancel");
            enabled = true;
            Input.focus = this;
        }

        public void Close()
        {
            enabled = false;
            Input.focus = null;
        }
        public void Draw()
        {
            if (enabled)
            {
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));
                DrawPanel();
                int barwidth = width - 20;
                int maxRow = Audio.AudioEngine.totalRows;
                int procRow = Math.Clamp(Audio.AudioEngine.processedRows, 0, maxRow);

                float fraction = (float)procRow / maxRow;
                WriteCenter(Helpers.TrimTextToWidth(width - 20, "Saving to " + Path), width / 2, 16, UIColors.label); ;
                DrawRect(10, 29, barwidth, 1, UIColors.labelLight);
                WriteCenter("Row " + (procRow + "/" + maxRow) + ": (" + (int)(fraction * 100) + "% done)", width / 2, 35, UIColors.label);
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
