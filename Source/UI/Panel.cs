using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Rendering;

namespace WaveTracker.UI {
    public abstract class Panel : Element {
        protected string name;
        protected int width;
        protected int height;

        /// <summary>
        /// Creates a new panel element
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public Panel(string name, int x, int y, int w, int h) {
            this.name = name;
            width = w;
            height = h;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Returns true if the mouse is within the panel bounds
        /// </summary>
        public bool IsMouseOverPanel { get { return MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0; } }

        public void DrawHorizontalLabel(string labelText, int x, int y, int width, int labelInset = 8) {
            int textWidth = Helpers.GetWidthOfText(labelText);
            DrawRect(x, y, labelInset - 3, 1, UIColors.labelLight);
            Write(labelText, x + labelInset, y - 3, UIColors.labelLight);
            DrawRect(x + labelInset + textWidth + 3, y, width - textWidth - 3 - labelInset, 1, UIColors.labelLight);
        }

        public void Draw() {
            DrawRoundedRect(0, 0, width, height, UIColors.panel);
            DrawRect(1, 0, width - 2, 1, Color.White);
            DrawRect(0, 1, width, 8, Color.White);
            Write(name, 4, 1, UIColors.panelTitle);
        }
    }
}
