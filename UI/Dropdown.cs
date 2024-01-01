using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;

namespace WaveTracker.UI
{
    public class Dropdown : Clickable
    {
        Element previousFocus;
        bool showMenu;
        public int Value { get; set; }
        int hoveredValue;
        string[] options;
        int cooldown;

        public Dropdown(int x, int y, Element parent)
        {
            enabled = true;
            this.x = x;
            this.y = y;
            height = 13;
            SetParent(parent);

        }

        public void SetMenuItems(string[] items)
        {
            int maxlength = 0;
            options = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                options[i] = Helpers.FlushString(items[i]);
                if (Helpers.getWidthOfText(options[i]) > maxlength)
                    maxlength = Helpers.getWidthOfText(options[i]);
            }
            if (width == 0)
                width = maxlength + 18;
        }

        public void Update()
        {
            if (!inFocus)
                cooldown = 2;
            if (IsHovered && Input.MouseScrollWheel(KeyModifier.None) != 0)
            {
                Value -= Input.MouseScrollWheel(KeyModifier.None);
                Value = (Value + options.Length) % options.Length;
                hoveredValue = Value;
            }
            if (showMenu)
            {

                for (int i = 0; i < options.Length; i++)
                {
                    int y = i * 11 + 15;
                    if (MouseX > 0 && MouseX < width)
                    {
                        if (MouseY >= y && MouseY <= y + 11)
                        {
                            hoveredValue = i;
                            if (Input.GetClickUp(KeyModifier.None))
                            {
                                Value = i;
                                CloseMenu();
                                Input.CancelClick();
                                return;
                            }
                        }
                    }
                }
                if (Input.GetClickUp(KeyModifier.None))
                {
                    CloseMenu();
                }
            }
            else
            {
                if (Clicked && cooldown <= 0)
                {
                    OpenMenu();
                }
                if (cooldown > 0)
                    cooldown--;
            }
        }

        public void OpenMenu()
        {
            previousFocus = Input.focus;
            Input.focus = this;
            hoveredValue = Value;
            showMenu = true;
        }

        public void CloseMenu()
        {
            showMenu = false;
            Input.focus = previousFocus;
        }

        Color getBackgroundColor()
        {
            if (IsPressed && enabled)
                return ButtonColors.Round.backgroundColorPressed;
            if ((IsHovered || showMenu) && enabled)
                return ButtonColors.Round.backgroundColorHover;
            return ButtonColors.Round.backgroundColor;
        }

        Color getTextColor()
        {
            if (IsPressed && enabled)
                return ButtonColors.Round.textColorPressed;
            return ButtonColors.Round.textColor;
        }
        public void Draw()
        {
            int textOffset = IsPressed && enabled ? 1 : 0;
            DrawRoundedRect(0, 0, width, height, getBackgroundColor());
            Write(options[Value], 4, (height + 1) / 2 - 4 + textOffset, getTextColor());
            DrawRect(width - 9, 5 + textOffset, 5, 1, Color.White);
            DrawRect(width - 8, 6 + textOffset, 3, 1, Color.White);
            DrawRect(width - 7, 7 + textOffset, 1, 1, Color.White);
            if (showMenu)
            {
                DrawMenu();
            }
        }

        public void DrawMenu()
        {
            DrawRect(0, 13, width, 11 * options.Length + 2, UIColors.labelDark);
            for (int i = 0; i < options.Length; i++)
            {
                int y = i * 11 + 14;
                if (i == hoveredValue)
                {
                    DrawRect(1, y, width - 2, 11, Helpers.LerpColor(UIColors.selection, Color.White, 0.7f));
                    Write(options[i], 4, y + 2, UIColors.black);
                }
                else
                {
                    DrawRect(1, y, width - 2, 11, Color.White);
                    Write(options[i], 4, y + 2, UIColors.labelDark);
                }
            }
        }
    }
}
