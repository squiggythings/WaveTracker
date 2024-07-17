using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WaveTracker.Audio;

namespace WaveTracker.UI {
    public class Visualizer : Clickable {
        VisualizerPiano piano;
        public Visualizer() {
            x = 0;
            y = 0;

        }

        public void Update() {
            width = App.WindowWidth;
            height = App.WindowHeight;
        }

        public void Draw() {
            //DrawRect(0, 0, width, height, new Color(255, 0, 0, 128));

        }


        public class VisualizerPiano : Clickable {

            List<NoteData[]> channelPitches;

            public VisualizerPiano(int x, int y, int width, int height, Element parent) {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                SetParent(parent);

                channelPitches = new NoteData[channelPitches.Length, height];
            }

            public void Draw() {
                DrawSprite(0, 0,)
            }
        }

        struct NoteData {
            float pitch;
            float volume;
            Color color;
        }
    }
}
