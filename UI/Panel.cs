using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.UI
{
    public abstract class Panel : Element
    {
        public string label;
        public int width;
        public int height;
        Color bgColor = new Color(222, 223, 231);
        Color frameColor = new Color(255, 255, 255);
        Color labelColor = new Color(64, 72, 115);

        public bool isInFocus { get { return MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0; } }

        public void InitializePanel(string name, int x, int y, int w, int h)
        {
            label = name;
            width = w;
            height = h;
            this.x = x;
            this.y = y;
        }
        public void DrawPanel()
        {
            DrawRoundedRect(0, 0, width, height, bgColor);
            DrawRect(1, 0, width - 2, 1, frameColor);
            DrawRect(0, 1, width, 8, frameColor);
            Write(label, 4, 1, labelColor);
        }
    }
}
