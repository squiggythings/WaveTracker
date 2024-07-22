using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;

namespace WaveTracker.UI {
    public class WaveBank : Panel {
        WaveBankElement[] waveBankElements;
        public static int currentWaveID;
        public static int lastSelectedWave;
        public WaveBank(int x, int y) : base("Wave Bank", x, y, 448, 130) {
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
        }
        public Wave GetWave(int num) {
            return App.CurrentModule.WaveBank[num];
        }
        public void Initialize() {

        }

        public void Update() {
            int i = 0;
            if (Input.focus == null)
                currentWaveID = -1;
            foreach (WaveBankElement e in waveBankElements) {
                e.Update();
                if (e.Clicked) {
                    currentWaveID = i;
                    lastSelectedWave = i;
                    App.WaveEditor.Open(i);
                    if (!ChannelManager.previewChannel.envelopePlayers[Envelope.EnvelopeType.Wave].HasActiveEnvelopeData)
                        ChannelManager.previewChannel.SetWave(i);
                }
                ++i;
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
            DrawRect(2, 11, 444, 114, new Color(43, 49, 81));
            foreach (WaveBankElement e in waveBankElements) {
                e.Draw();
            }
        }
    }
}
