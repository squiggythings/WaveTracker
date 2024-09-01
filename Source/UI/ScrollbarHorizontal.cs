using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class ScrollbarHorizontal : Clickable {
        public int totalSize;
        public int viewportSize;
        public Rectangle bar;
        private bool lastClickWasOnScrollbar;
        public int ScrollValue { get; set; }
        public int CoarseStepAmount { get; set; }

        private int barClickOffset;

        public bool IsVisible { get { return viewportSize < totalSize; } }
        public ScrollbarHorizontal(int x, int y, int width, int height, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            CoarseStepAmount = 1;
            if (parent != null) {
                SetParent(parent);
            }
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
                            bar.X = MouseX + barClickOffset;
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
                    Color barDefault = ButtonColors.Round.backgroundColor;
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
        public void UpdateScrollValue() {
            if (IsVisible) {
                ScrollValue = Math.Clamp(ScrollValue, 0, totalSize - viewportSize);
                bar.X = (int)Math.Round(BarPosFromVal() * (width - 2) + 1);
            }
            else {
                ScrollValue = 0;
            }
        }

        private float BarValFromPos() {
            return (bar.X - 1) / (float)(width - 2 - bar.Width);
        }

        private float BarPosFromVal() {
            return ScrollValue / (float)totalSize;
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
