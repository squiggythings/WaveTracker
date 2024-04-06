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
        public WaveEditor editor;
        public static int currentWave;
        public static int lastSelectedWave;
        public WaveBank() : base("Wave Bank", 510, 18, 448, 130) {
            waveBankElements = new WaveBankElement[100];
            lastSelectedWave = 0;
            int index = 0;
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 20; x++) {
                    waveBankElements[index] = new WaveBankElement(this, index);
                    waveBankElements[index].x = x * 22 + 4;
                    waveBankElements[index].y = y * 22 + 13;
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
                currentWave = -1;
            foreach (WaveBankElement e in waveBankElements) {
                e.Update();
                if (e.Clicked) {
                    currentWave = i;
                    lastSelectedWave = i;
                    editor.Open(i);
                    if (ChannelManager.previewChannel.currentInstrument is WaveInstrument) {
                        if (!ChannelManager.previewChannel.envelopePlayers[3].envelopeToPlay.IsActive)
                            ChannelManager.previewChannel.SetWave(i);
                    }
                }

                ++i;
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
