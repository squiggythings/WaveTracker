using Microsoft.Xna.Framework;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveBank : Panel {
        const int MAX_WIDTH = 448;
        private WaveBankElement[] waveBankElements;
        public static int currentWaveID;
        public static int lastSelectedWave;
        private int maxVisibleWaves;
        private ScrollbarHorizontal scrollbar;
        public WaveBank(int x, int y) : base("Wave Bank", x, y, MAX_WIDTH, 130) {
            waveBankElements = new WaveBankElement[100];
            lastSelectedWave = 0;
            int index = 0;
            for (int iy = 0; iy < 5; iy++) {
                for (int ix = 0; ix < 20; ix++) {
                    waveBankElements[index] = new WaveBankElement(this, index);
                    waveBankElements[index].x = ix * 22 + 4;
                    waveBankElements[index].y = iy * 22 + 13;
                    waveBankElements[index].SetTooltip("", "Wave " + index.ToString("D2"));
                    index++;
                }
            }
            scrollbar = new ScrollbarHorizontal(2, 11, width - 4, height - 11, this);
        }

        public void Update() {
            if (Input.focus == null) {
                currentWaveID = -1;
            }

            width = App.WindowWidth - x - 2;
            if (width > MAX_WIDTH) {
                width = MAX_WIDTH;
            }
            scrollbar.width = width - 4;
            scrollbar.SetSize(20, (width - 8) / 22);
            scrollbar.Update();
            int i = 0;
            foreach (WaveBankElement waveButton in waveBankElements) {
                waveButton.x = (i % 20 - scrollbar.ScrollValue) * 22 + 4;

                if (waveButton.x > 0 && waveButton.BoundsRight < width - 4) {
                    waveButton.Update();
                }
                if (waveButton.Clicked) {
                    currentWaveID = i;
                    lastSelectedWave = i;
                    App.WaveEditor.Open(i);
                    if (!ChannelManager.PreviewChannel.envelopePlayers[Envelope.EnvelopeType.Wave].HasActiveEnvelopeData) {
                        ChannelManager.PreviewChannel.SetWave(i);
                    }
                }
                i++;
            }
            if (InFocus) {
                if (App.Shortcuts["General\\Edit wave"].IsPressedDown) {
                    currentWaveID = lastSelectedWave;
                    App.WaveEditor.Open(currentWaveID);
                }
            }
        }

        public new void Draw() {
            base.Draw();
            DrawRect(2, 11, width - 4, 114, new Color(43, 49, 81));
            foreach (WaveBankElement waveButton in waveBankElements) {
                if (waveButton.x > 0 && waveButton.x < width - 24) {
                    waveButton.Draw();
                }
            }
            scrollbar.Draw();
        }
    }
}
