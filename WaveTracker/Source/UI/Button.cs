using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class Button : Clickable {
        public string Label { get; private set; }
        public bool LabelIsCentered { get; set; }

        private int labelWidth;

        public Button(string label, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            Label = label;
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
            if (IsPressed) {
                return ButtonColors.backgroundColorPressed;
            }
            else {
                if (IsHovered) {
                    return ButtonColors.backgroundColorHover;
                }
                else {
                    return ButtonColors.backgroundColor;
                }
            }
        }

        private Color GetTextColor() {
            if (IsPressed) {
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

                if (LabelIsCentered) {
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                }
                else {
                    Write(Label, 4, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                }
            }
            else {
                DrawRoundedRect(0, 0, width, height, ButtonColors.backgroundColorDisabled);
                if (LabelIsCentered) {
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4, ButtonColors.textColorDisabled);
                }
                else {
                    Write(Label, 4, (height + 1) / 2 - 4, ButtonColors.textColorDisabled);
                }
            }

        }

    }
}
