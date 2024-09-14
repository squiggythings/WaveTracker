using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class Toggle : Clickable {
        private string label;
        public bool centerLabel = true;
        private ButtonColors colors;
        private int labelWidth;
        public bool Value { get; set; }

        public Toggle(string label, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.label = label;
            colors = ButtonColors.Round;
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

        private Color getBackgroundColor() {
            if (Value) {
                return IsPressed ? colors.backgroundColorPressed : IsHovered ? colors.toggleBackgroundColor : colors.backgroundColorPressed;
            }
            else {
                if (IsPressed) {
                    return colors.backgroundColor;
                }

                if (IsHovered) {
                    return colors.backgroundColorHover;
                }
            }
            return colors.backgroundColor;
        }

        private Color getTextColor() {
            return IsPressed || Value ? colors.textColorPressed : colors.textColor;
        }

        public void Draw() {
            if (enabled) {
                DrawRoundedRect(0, 0, width, height, getBackgroundColor());
                int textOffset = IsPressed ? 1 : 0;

                if (centerLabel) {
                    Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, getTextColor());
                }
                else {
                    Write(label, 4, (height + 1) / 2 - 4 + textOffset, getTextColor());
                }
            }
            else {
                DrawRoundedRect(0, 0, width, height, colors.backgroundColorDisabled);
                if (centerLabel) {
                    Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4, colors.textColorDisabled);
                }
                else {
                    Write(label, 4, (height + 1) / 2 - 4, colors.textColorDisabled);
                }
            }

        }

    }
}
