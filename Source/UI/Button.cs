using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace WaveTracker.UI {
    public class Button : Clickable {
        public string Label { get; private set; }
        public bool LabelIsCentered { get; set; }
        ButtonColors colors;
        int labelWidth;

        public Button(string label, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.Label = label;
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
            this.Label = label;
            colors = ButtonColors.Round;
            LabelIsCentered = true;
            this.width = width;
            labelWidth = Helpers.GetWidthOfText(label);
            height = 13;
            SetParent(parent);
        }

        public void SetLabel(string label) {
            this.Label = label;
            labelWidth = Helpers.GetWidthOfText(label);
        }
        Color GetBackgroundColor() {
            if (IsPressed)
                return colors.backgroundColorPressed;
            if (IsHovered)
                return colors.backgroundColorHover;
            return colors.backgroundColor;
        }

        Color GetTextColor() {
            if (IsPressed)
                return colors.textColorPressed;
            return colors.textColor;
        }

        Color GetBorderColor() {
            if (IsPressed)
                return colors.borderColorPressed;
            return colors.borderColor;
        }

        public void Draw() {
            if (enabled) {
                DrawRoundedRect(0, 0, width, height, GetBackgroundColor());

                int textOffset = IsPressed ? 1 : 0;

                if (LabelIsCentered)
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, GetTextColor());
                else
                    Write(Label, 4, (height + 1) / 2 - 4 + textOffset, GetTextColor());
            }
            else {
                DrawRoundedRect(0, 0, width, height, colors.backgroundColorDisabled);
                if (LabelIsCentered)
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4, colors.textColorDisabled);
                else
                    Write(Label, 4, (height + 1) / 2 - 4, colors.textColorDisabled);
            }

        }

    }
}
