using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace WaveTracker.UI
{
    public class Button : Clickable
    {
        string label;
        public bool centerLabel = true;
        ButtonColors colors;
        readonly int labelWidth;
        ButtonType type;

        public Button(string label, int x, int y, Element parent)
        {
            enabled = true;
            this.x = x;
            this.y = y;
            this.label = label;
            colors = ButtonColors.Round;
            type = ButtonType.Rounded;
            width = Helpers.getWidthOfText(label) + 8;
            //if (width < 30)
            //    width = 30;
            labelWidth = Helpers.getWidthOfText(label);
            height = 13;
            SetParent(parent);
        }

        Color getBackgroundColor()
        {
            if (IsPressed)
                return colors.backgroundColorPressed;
            if (IsHovered)
                return colors.backgroundColorHover;
            return colors.backgroundColor;
        }

        Color getTextColor()
        {
            if (IsPressed)
                return colors.textColorPressed;
            return colors.textColor;
        }

        Color getBorderColor()
        {
            if (IsPressed)
                return colors.borderColorPressed;
            return colors.borderColor;
        }

        public void Draw()
        {
            if (enabled)
            {
                if (type == ButtonType.Square)
                {
                    DrawRect(0, 0, width, height, getBorderColor());
                    DrawRect(1, 1, width - 2, height - 2, getBackgroundColor());
                }
                if (type == ButtonType.Rounded)
                {
                    DrawRoundedRect(0, 0, width, height, getBackgroundColor());
                }
                int textOffset = type == ButtonType.Rounded && IsPressed ? 1 : 0;

                if (centerLabel)
                    Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, getTextColor());
                else
                    Write(label, 4, (height + 1) / 2 - 4 + textOffset, getTextColor());
            }
            else
            {
                if (type == ButtonType.Square)
                {
                    DrawRect(0, 0, width, height, colors.backgroundColorDisabled);
                }
                if (type == ButtonType.Rounded)
                {
                    DrawRoundedRect(0, 0, width, height, colors.backgroundColorDisabled);
                }
                if (centerLabel)
                    Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4, colors.textColorDisabled);
                else
                    Write(label, 4, (height + 1) / 2 - 4, colors.textColorDisabled);
            }

        }

    }
    public enum ButtonType { Square, Rounded }
}
