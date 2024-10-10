using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class ScrollbarHorizontal : Clickable {
        public int totalSize;
        public int viewportSize;
        public Rectangle bar;
        private bool lastClickWasOnScrollbar;
        public int ScrollValue { get; set; }
        public int MaxScrollValue { get { return totalSize - viewportSize; } }
        public bool IsVisible { get { return viewportSize < totalSize; } }

        public int CoarseStepAmount { get; set; }

        private int barClickOffset;

        public ScrollbarHorizontal(int x, int y, int width, int height, Element parent) {
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
            bar.Height = 6;
            bar.Y = height - bar.Height;
            bar.Width = Math.Max((int)(width * (viewportSize / (float)totalSize)), 4);
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
                            if (MouseY >= bar.Y && MouseY <= bar.Y + bar.Height) {
                                if (lastClickWasOnScrollbar) {
                                    barClickOffset = bar.X - MouseX;
                                }
                                else {
                                    // step bar towards mouse
                                    if (MouseX > bar.X) {
                                        ScrollValue += CoarseStepAmount;
                                    }
                                    else {
                                        ScrollValue -= CoarseStepAmount;
                                    }
                                }
                            }
                        }
                        if (BarIsPressed) {
                            bar.X = Math.Clamp(MouseX + barClickOffset, 1, width - bar.Width - 1);
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

        public void Draw() {
            if (enabled) {
                if (IsVisible) {
                    Color background = UIColors.panel;
                    Color barSpace = UIColors.labelLight;
                    Color barDefault = ButtonColors.backgroundColor;
                    Color barHover = UIColors.labelDark;
                    Color barPressed = UIColors.black;

                    DrawRect(0, bar.Y, width, bar.Height, background);
                    DrawRoundedRect(1, bar.Y + 1, width - 2, bar.Height - 2, barSpace);
                    if (BarIsPressed && !Input.internalDialogIsOpen) {
                        DrawRoundedRect(bar.X, bar.Y + 1, bar.Width, bar.Height - 2, barPressed);
                    }
                    else if (BarisHovered && !Input.internalDialogIsOpen) {
                        DrawRoundedRect(bar.X, bar.Y + 1, bar.Width, bar.Height - 2, barHover);
                    }
                    else {
                        DrawRoundedRect(bar.X, bar.Y + 1, bar.Width, bar.Height - 2, barDefault);
                    }
                }
            }
        }

        /// <summary>
        /// Clamps the scroll value if the scroll value is out of range
        /// </summary>
        private void UpdateScrollValue() {
            if (IsVisible) {
                ScrollValue = Math.Clamp(ScrollValue, 0, MaxScrollValue);
                int position = (int)Math.Round(BarPosFromVal() * (width - 2f - bar.Width) + 1);
                bar.X = Math.Clamp(position, 1, width - bar.Width - 1);
            }
            else {
                ScrollValue = 0;
            }
        }

        /// <summary>
        /// 0.0-1.0 of how scrolled the bar is from the x position
        /// </summary>
        /// <returns></returns>
        private float BarValFromPos() {
            return (bar.X - 1f) / (width - bar.Width - 2f);
        }

        /// <summary>
        /// 0.0-1.0 of how scrolled the bar is from the scrollValue
        /// </summary>
        /// <returns></returns>
        private float BarPosFromVal() {
            return ScrollValue / (float)MaxScrollValue;

        }

        private bool BarisHovered {
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
