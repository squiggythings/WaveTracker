using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Rendering;

namespace WaveTracker.UI
{
    public abstract class Panel : Element {
        public string label;
        public int width;
        public int height;

        public bool isInFocus { get { return MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0; } }

        public void InitializePanel(string name, int x, int y, int w, int h) {
            label = name;
            width = w;
            height = h;
            this.x = x;
            this.y = y;
        }

        public void InitializePanelCentered(string name, int w, int h) {
            label = name;
            width = w;
            height = h;
            this.x = (960 - w) / 2;
            this.y = (500 - h) / 2;
        }
        public void DrawPanel() {
            DrawRoundedRect(0, 0, width, height, UIColors.panel);
            DrawRect(1, 0, width - 2, 1, Color.White);
            DrawRect(0, 1, width, 8, Color.White);
            Write(label, 4, 1, UIColors.panelTitle);
        }
    }
}
