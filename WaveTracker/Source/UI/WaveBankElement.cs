using Microsoft.Xna.Framework;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveBankElement : Clickable {
        private Wave Wave {
            get {
                return App.CurrentModule.WaveBank[id];
            }
        }

        private int id;
        private double phase;

        public WaveBankElement(Element parent, int i) {
            width = 22;
            height = 22;
            id = i;
            SetParent(parent);
        }

        public void Update() {

        }

        public void Draw() {
            if (WaveBank.lastSelectedWave == id) {
                DrawRoundedRect(0, 0, 22, 22, new Color(68, 75, 120));
            }
            if (IsHovered || WaveBank.currentWaveID == id) {
                if (WaveEditor.enabled) {
                    phase += 2 * App.GameTime.ElapsedGameTime.TotalMilliseconds / 16.66667f;
                }
                else {
                    phase += 4 * App.GameTime.ElapsedGameTime.TotalMilliseconds / 16.66667f;
                }
                DrawRoundedRect(0, 0, 22, 22, new Color(104, 111, 153));
            }
            else {
                phase = 1;
            }
            if (phase > 64) {
                phase -= 64;
            }

            DrawRect(3, 4, 16, 8, new Color(20, 24, 46));

            if (!Wave.IsEmpty()) {
                int wx = 3;
                int wy;
                for (int i = 0; i < Wave.samples.Length; i += 4) {
                    int sum = Wave.GetSample((i + (int)(phase / 4) * 4) + 1);

                    wy = (31 - sum) / 4;

                    DrawRect(wx, wy + 4, 1, 4 - wy, new Color(60, 112, 97));
                    DrawRect(wx, wy + 4, 1, 1, new Color(200, 212, 93));
                    wx++;
                }
                WriteMonospaced(id.ToString("D2"), 6, 14, new Color(200, 212, 93), 4);
            }
            else {
                WriteMonospaced(id.ToString("D2"), 6, 14, new Color(20, 24, 46), 4);
            }
        }

    }
}
