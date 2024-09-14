using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class Button : Clickable {
        public string Label { get; private set; }
        public bool LabelIsCentered { get; set; }

        private ButtonColors colors;
        private int labelWidth;

        public Button(string label, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            Label = label;
            colors = ButtonColors.Round;
            LabelIsCentered = true;
            width = Helpers.GetWidthOfText(label) + 8;
            labelWidth = Helpers.GetWidthOfText(label);
            height = 13;
            SetParent(parent);
        }
        public Button(string label, int x, int y, int width, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            Label = label;
            colors = ButtonColors.Round;
            LabelIsCentered = true;
            this.width = width;
            labelWidth = Helpers.GetWidthOfText(label);
            height = 13;
            SetParent(parent);
        }

        public void SetLabel(string label) {
            Label = label;
            labelWidth = Helpers.GetWidthOfText(label);
        }

        private Color GetBackgroundColor() {
            return IsPressed ? colors.backgroundColorPressed : IsHovered ? colors.backgroundColorHover : colors.backgroundColor;
        }

        private Color GetTextColor() {
            return IsPressed ? colors.textColorPressed : colors.textColor;
        }

        private Color GetBorderColor() {
            return IsPressed ? colors.borderColorPressed : colors.borderColor;
        }

        public void Draw() {
            if (enabled) {
                DrawRoundedRect(0, 0, width, height, GetBackgroundColor());

                int textOffset = IsPressed ? 1 : 0;

                if (LabelIsCentered) {
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                }
                else {
                    Write(Label, 4, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                }
            }
            else {
                DrawRoundedRect(0, 0, width, height, colors.backgroundColorDisabled);
                if (LabelIsCentered) {
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4, colors.textColorDisabled);
                }
                else {
                    Write(Label, 4, (height + 1) / 2 - 4, colors.textColorDisabled);
                }
            }

        }

    }
}
