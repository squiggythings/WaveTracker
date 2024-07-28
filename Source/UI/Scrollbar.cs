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
    public class Scrollbar : Clickable {
        public int totalSize;
        public int viewportSize;
        public Rectangle bar;
        bool lastClickWasOnScrollbar;
        public int ScrollValue { get; set; }
        public int MaxScrollValue { get { return totalSize - viewportSize; } }
        public bool IsVisible { get { return viewportSize < totalSize; } }

        public int CoarseStepAmount { get; set; }
        int barClickOffset;
        public Scrollbar(int x, int y, int width, int height, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            CoarseStepAmount = 1;
            SetParent(parent);
        }

        public void SetSize(int totalSize, int viewportSize) {
            this.viewportSize = viewportSize;
            this.totalSize = totalSize;
            bar.Width = 6;
            bar.X = width - bar.Width;
            bar.Height = Math.Max((int)(height * (viewportSize / (float)totalSize)), 4);
            UpdateScrollValue();
        }



        public void Update() {
            if (InFocus) {
                if (enabled) {
                    if (IsVisible) {
                        if (Input.GetClickDown(KeyModifier._Any)) {
                            lastClickWasOnScrollbar = bar.Contains(LastClickPos);
                        }
                        if (ClickedDown) {
                            lastClickWasOnScrollbar = bar.Contains(LastClickPos);
                            if (MouseX >= bar.X && MouseX <= bar.X + bar.Width) {
                                if (lastClickWasOnScrollbar) {
                                    barClickOffset = bar.Y - MouseY;
                                }
                                else {
                                    // step bar towards mouse
                                    if (MouseY > bar.Y) {
                                        ScrollValue += CoarseStepAmount;
                                    }
                                    else {
                                        ScrollValue -= CoarseStepAmount;
                                    }
                                }
                            }
                        }
                        if (BarIsPressed) {
                            bar.Y = MouseY + barClickOffset;

                            ScrollValue = (int)Math.Round(BarValFromPos() * (totalSize - viewportSize));
                        }
                        else if (IsHovered) {
                            ScrollValue -= Input.MouseScrollWheel(KeyModifier._Any) * CoarseStepAmount;
                        }
                        UpdateScrollValue();
                    }

                }
            }
        }

        /// <summary>
        /// Clamps the scroll value if the scroll value is out of range
        /// </summary>
        public void UpdateScrollValue() {
            if (viewportSize < totalSize) {
                ScrollValue = Math.Clamp(ScrollValue, 0, totalSize - viewportSize);
                bar.Y = (int)Math.Round(BarPosFromVal() * (height - 2) + 1);
            }
        }

        public void Draw() {
            if (viewportSize < totalSize) {
                Color background = UIColors.panel;
                Color barSpace = UIColors.labelLight;
                Color barDefault = ButtonColors.Round.backgroundColor;
                Color barHover = UIColors.labelDark;
                Color barPressed = UIColors.black;
                //DrawRect(0, 0, width, height, new Color(255, 0, 0, 40));

                DrawRect(bar.X, 0, bar.Width, height, background);
                DrawRoundedRect(bar.X + 1, 1, bar.Width - 2, height - 2, barSpace);
                if (BarIsPressed)
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barPressed);
                else if (BarIsHovered)
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barHover);
                else
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barDefault);
            }
        }

        float BarValFromPos() {
            return (bar.Y - 1) / (float)(height - 2 - bar.Height);
        }

        float BarPosFromVal() {
            return ScrollValue / (float)(totalSize);

        }



        bool BarIsHovered => InFocus && bar.Contains(MouseX, MouseY);
        bool BarIsPressed => InFocus && Input.GetClick(KeyModifier._Any) && lastClickWasOnScrollbar;

    }
}
