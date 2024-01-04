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

namespace WaveTracker.Rendering {
    public class WaveBankElement : Clickable {
        Wave wave => Song.currentSong.waves[id];
        int id;
        double phase;

        public WaveBankElement(Element parent, int i) {
            width = 22;
            height = 22;
            id = i;
            SetParent(parent);
        }

        public void Update() {
            if (IsHovered || WaveBank.currentWave == id) {
                phase += 4;
            } else {
                phase = 1;
            }

        }


        public void Draw() {
            if (WaveBank.lastSelectedWave == id) {
                DrawRoundedRect(0, 0, 22, 22, new Color(68, 75, 120));
            }
            if (IsHovered || (WaveEditor.enabled && WaveBank.currentWave == id)) {
                if (WaveEditor.enabled) {
                    phase -= 2;
                }
                DrawRoundedRect(0, 0, 22, 22, new Color(104, 111, 153));
            }

            bool isEmpty = true;
            foreach (int sample in wave.samples) {
                if (sample != 16)
                    isEmpty = false;
            }
            DrawRect(3, 4, 16, 8, new Color(20, 24, 46));

            if (!isEmpty) {
                int wx = 3;
                int wy;
                for (int i = 0; i < wave.samples.Length; i += 4) {
                    int sum = wave.getSample((int)(i + phase + 0));

                    wy = ((31 - sum) / 4);

                    DrawRect(wx, wy + 4, 1, (4 - wy), new Color(60, 112, 97));
                    DrawRect(wx, wy + 4, 1, 1, new Color(200, 212, 93));
                    wx++;
                }
                WriteMonospaced(id.ToString("D2"), 6, 14, new Color(200, 212, 93), 4);
            } else {
                WriteMonospaced(id.ToString("D2"), 6, 14, new Color(20, 24, 46), 4);
            }
        }

    }
}
