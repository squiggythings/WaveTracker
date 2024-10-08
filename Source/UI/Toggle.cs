using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class Toggle : Clickable {
        private string label;
        public bool centerLabel = true;
        private int labelWidth;
        public bool Value { get; set; }

        public Toggle(string label, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.label = label;
            width = Helpers.GetWidthOfText(label) + 8;
            //if (width < 30)
            //    width = 30;
            labelWidth = Helpers.GetWidthOfText(label);
            height = 13;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                Value = !Value;
            }
        }

        private Color GetBackgroundColor() {
            if (Value) {
                if (IsPressed) {
                    return ButtonColors.backgroundColorPressed;
                }
                else {
                    if (IsHovered) {
                        return ButtonColors.toggleBackgroundColor;
                    }
                    else {
                        return ButtonColors.backgroundColorPressed;
                    }
                }
            }
            else {
                if (IsPressed) {
                    return ButtonColors.backgroundColor;
                }

                if (IsHovered) {
                    return ButtonColors.backgroundColorHover;
                }
            }
            return ButtonColors.backgroundColor;
        }

        private Color GetTextColor() {
            if (IsPressed || Value) {
                return ButtonColors.textColorPressed;
            }
            else {
                return ButtonColors.textColor;
            }
        }

        public void Draw() {
            if (enabled) {
                DrawRoundedRect(0, 0, width, height, GetBackgroundColor());
                int textOffset = IsPressed ? 1 : 0;

                if (centerLabel) {
                    Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                }
                else {
                    Write(label, 4, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                }
            }
            else {
                DrawRoundedRect(0, 0, width, height, ButtonColors.backgroundColorDisabled);
                if (centerLabel) {
                    Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4, ButtonColors.textColorDisabled);
                }
                else {
                    Write(label, 4, (height + 1) / 2 - 4, ButtonColors.textColorDisabled);
                }
            }

        }

    }
}
