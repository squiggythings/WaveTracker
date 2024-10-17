using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class Scrollbar : Clickable {
        public int totalSize;
        public int viewportSize;
        public Rectangle bar;
        private bool lastClickWasOnScrollbar;
        public int ScrollValue { get; set; }
        public int MaxScrollValue { get { return totalSize - viewportSize; } }
        public bool IsVisible { get { return viewportSize < totalSize; } }

        public int CoarseStepAmount { get; set; }

        private int barClickOffset;

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
                            bar.Y = Math.Clamp(MouseY + barClickOffset, 1, height - bar.Height - 1);
                            ScrollValue = (int)Math.Round(BarValFromPos() * MaxScrollValue);
                        }
                        else if (IsHovered) {
                            ScrollValue -= Input.MouseScrollWheel(KeyModifier._Any) * CoarseStepAmount;
                        }
                        UpdateScrollValue();
                    }

                }
            }
        }

        public void Draw() {
            if (viewportSize < totalSize) {
                Color background = UIColors.panel;
                Color barSpace = UIColors.labelLight;
                Color barDefault = ButtonColors.backgroundColor;
                Color barHover = UIColors.labelDark;
                Color barPressed = UIColors.black;
                //DrawRect(0, 0, width, height, new Color(255, 0, 0, 40));

                DrawRect(bar.X, 0, bar.Width, height, background);
                DrawRoundedRect(bar.X + 1, 1, bar.Width - 2, height - 2, barSpace);
                if (BarIsPressed) {
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barPressed);
                }
                else if (BarIsHovered) {
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barHover);
                }
                else {
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barDefault);
                }
            }
        }

        /// <summary>
        /// Clamps the scroll value if the scroll value is out of range
        /// </summary>
        public void UpdateScrollValue() {
            if (viewportSize < totalSize) {
                ScrollValue = Math.Clamp(ScrollValue, 0, MaxScrollValue);
                int position = (int)Math.Round(BarPosFromVal() * (height - 2f - bar.Height) + 1);
                bar.Y = Math.Clamp(position, 1, height - bar.Height - 1);

            }
        }

        /// <summary>
        /// 0.0-1.0 of how scrolled the bar is from the y position
        /// </summary>
        /// <returns></returns>
        private float BarValFromPos() {
            return (bar.Y - 1f) / (height - bar.Height - 2f);
        }

        /// <summary>
        /// 0.0-1.0 of how scrolled the bar is from the scrollValue
        /// </summary>
        /// <returns></returns>
        private float BarPosFromVal() {
            return ScrollValue / (float)MaxScrollValue;

        }

        private bool BarIsHovered {
            get {
                return InFocus && bar.Contains(MouseX, MouseY);
            }
        }

        private bool BarIsPressed {
            get {
                return InFocus && Input.GetClick(KeyModifier._Any) && lastClickWasOnScrollbar;
            }
        }
    }
}
