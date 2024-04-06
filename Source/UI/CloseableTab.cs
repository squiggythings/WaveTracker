using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class CloseableTab : Clickable {
        public string label;
        public MouseRegion exitButton;
        public CloseableTab(string label, int x, int y, Element parent) {
            width = Helpers.GetWidthOfText(label) + 20;
            height = 13;
            this.x = x;
            this.y = y;
            this.label = label;
            exitButton = new MouseRegion(width - 10, 3, 7, 7, this);
            SetParent(parent);
        }

        void Update() {

        }

        public void Draw(bool isSelected) {
            Color bgCol;
            if (isSelected) {
                bgCol = Color.White;
            }
            else {
                if (IsHovered) {
                    bgCol = new Color(191, 194, 212);
                }
                else {
                    bgCol = new Color(176, 180, 202);
                }
            }
            DrawRect(0, 1, width, height, bgCol);
            DrawRect(1, 0, width - 2, height, bgCol);
            Write(label, 5, 3, UIColors.labelDark);
            if (IsHovered || isSelected) {
                if (exitButton.IsPressed) {
                    DrawRect(exitButton.x, exitButton.y, exitButton.width, exitButton.height, Helpers.Alpha(UIColors.labelDark, 128));
                    DrawSprite(exitButton.x + 1, exitButton.y + 1, exitButton.width - 2, exitButton.height - 2, new Rectangle(478, 48, 5, 5));
                }
                else {
                    DrawRect(exitButton.x, exitButton.y, exitButton.width, exitButton.height, Helpers.Alpha(UIColors.labelDark, 50));
                    if (exitButton.IsHovered)
                        DrawSprite(exitButton.x + 1, exitButton.y + 1, exitButton.width - 2, exitButton.height - 2, new Rectangle(472, 48, 5, 5));
                }
            }
        }
    }
}