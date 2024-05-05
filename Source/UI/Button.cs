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
        ButtonType type;

        public Button(string label, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.Label = label;
            colors = ButtonColors.Round;
            LabelIsCentered = true;
            type = ButtonType.Rounded;
            width = Helpers.GetWidthOfText(label) + 8;
            //if (width < 30)
            //    width = 30;
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
            type = ButtonType.Rounded;
            this.width = width;
            //if (width < 30)
            //    width = 30;
            labelWidth = Helpers.GetWidthOfText(label);
            height = 13;
            SetParent(parent);
        }

        public void SetLabel(string label) {
            this.Label = label;
            labelWidth = Helpers.GetWidthOfText(label);
        }
        Color getBackgroundColor() {
            if (IsPressed)
                return colors.backgroundColorPressed;
            if (IsHovered)
                return colors.backgroundColorHover;
            return colors.backgroundColor;
        }

        Color getTextColor() {
            if (IsPressed)
                return colors.textColorPressed;
            return colors.textColor;
        }

        Color getBorderColor() {
            if (IsPressed)
                return colors.borderColorPressed;
            return colors.borderColor;
        }

        public void Draw() {
            if (enabled) {
                if (type == ButtonType.Square) {
                    DrawRect(0, 0, width, height, getBorderColor());
                    DrawRect(1, 1, width - 2, height - 2, getBackgroundColor());
                }
                if (type == ButtonType.Rounded) {
                    DrawRoundedRect(0, 0, width, height, getBackgroundColor());
                }
                int textOffset = type == ButtonType.Rounded && IsPressed ? 1 : 0;

                if (LabelIsCentered)
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, getTextColor());
                else
                    Write(Label, 4, (height + 1) / 2 - 4 + textOffset, getTextColor());
            }
            else {
                if (type == ButtonType.Square) {
                    DrawRect(0, 0, width, height, colors.backgroundColorDisabled);
                }
                if (type == ButtonType.Rounded) {
                    DrawRoundedRect(0, 0, width, height, colors.backgroundColorDisabled);
                }
                if (LabelIsCentered)
                    Write(Label, (width - labelWidth) / 2, (height + 1) / 2 - 4, colors.textColorDisabled);
                else
                    Write(Label, 4, (height + 1) / 2 - 4, colors.textColorDisabled);
            }

        }

    }
    public enum ButtonType { Square, Rounded }
}
