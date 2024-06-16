using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class DropdownButton : Clickable {
        static DropdownButton currentlyOpen;

        Element previousFocus;
        bool showMenu;
        public int SelectedIndex { get; set; }
        public bool SelectedAnItem { get; set; }

        public bool LabelIsCentered { get; set; }

        int menuWidth;
        int menuX;
        int menuY;


        string label;
        int labelWidth;
        int hoveredValue;
        string[] options;
        int cooldown;

        public DropdownButton(string label, int x, int y, Element parent) {
            enabled = true;
            this.label = label;
            labelWidth = Helpers.GetWidthOfText(label);
            this.x = x;
            this.y = y;
            height = 13;
            width = labelWidth + 18;
            SetParent(parent);
        }
        public DropdownButton(string label, int x, int y, int width, Element parent) {
            enabled = true;
            this.label = label;
            labelWidth = Helpers.GetWidthOfText(label);
            this.x = x;
            this.y = y;
            height = 13;
            this.width = width;
            SetParent(parent);
        }

        public void SetMenuItems(string[] items) {
            int maxlength = 0;
            options = new string[items.Length];
            for (int i = 0; i < items.Length; i++) {
                options[i] = Helpers.FlushString(items[i]);
                if (Helpers.GetWidthOfText(options[i]) > maxlength)
                    maxlength = Helpers.GetWidthOfText(options[i]);
            }
            menuWidth = maxlength + 18;
        }

        public void Update() {
            if (!InFocus)
                cooldown = 2;
            SelectedAnItem = false;
            if (showMenu) {
                for (int i = 0; i < options.Length; i++) {
                    int y = i * 11 + menuY;
                    if (MouseX >= menuX && MouseX < menuX + menuWidth) {
                        if (MouseY >= y && MouseY <= y + 11) {
                            hoveredValue = i;
                            if (Input.GetClickUp(KeyModifier.None)) {
                                SelectedIndex = i;
                                SelectedAnItem = true;
                                CloseMenu();
                                Input.CancelClick();
                                return;
                            }
                        }
                    }
                }
                if (Input.GetClickUp(KeyModifier.None)) {
                    CloseMenu();
                }
            }
            else {
                if (Clicked && cooldown <= 0) {
                    OpenMenu();
                }
                if (cooldown > 0)
                    cooldown--;
            }
        }

        void OpenMenu() {
            previousFocus = Input.focus;
            Input.focus = this;
            hoveredValue = -1;
            currentlyOpen = this;
            menuX = width;
            menuY = 0;
            showMenu = true;
        }

        void CloseMenu() {
            showMenu = false;
            currentlyOpen = null;
            Input.focus = previousFocus;
        }

        Color getBackgroundColor() {
            if (!enabled)
                return ButtonColors.Round.backgroundColorDisabled;
            if (IsPressed)
                return ButtonColors.Round.backgroundColorPressed;
            if ((IsHovered || showMenu))
                return ButtonColors.Round.backgroundColorHover;

            return ButtonColors.Round.backgroundColor;
        }

        Color getTextColor() {
            if (!enabled)
                return ButtonColors.Round.textColorDisabled;
            if (IsPressed)
                return ButtonColors.Round.textColorPressed;
            return ButtonColors.Round.textColor;
        }
        public void Draw() {

            int textOffset = IsPressed && enabled ? 1 : 0;

            DrawRoundedRect(0, 0, width, height, getBackgroundColor());


            if (LabelIsCentered)
                Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, ButtonColors.Round.textColor);
            else
                Write(label, 4, (height + 1) / 2 - 4 + textOffset, ButtonColors.Round.textColor);
            DrawRect(width - 9, 5 + textOffset, 5, 1, getTextColor());
            DrawRect(width - 8, 6 + textOffset, 3, 1, getTextColor());
            DrawRect(width - 7, 7 + textOffset, 1, 1, getTextColor());
        }

        public static void DrawCurrentMenu() {
            if (currentlyOpen != null) {
                currentlyOpen.DrawMenu();
            }
        }

        void DrawMenu() {
            DrawRect(menuX, menuY, menuWidth, 11 * options.Length + 2, UIColors.labelDark);
            for (int i = 0; i < options.Length; i++) {
                int y = i * 11 + menuY + 1;
                if (i == hoveredValue) {
                    DrawRect(menuX + 1, y, menuWidth - 2, 11, Helpers.LerpColor(UIColors.selection, Color.White, 0.7f));
                    Write(Helpers.TrimTextToWidth(menuWidth - 8, options[i]), menuX + 4, y + 2, UIColors.black);
                }
                else {
                    DrawRect(menuX + 1, y, menuWidth - 2, 11, Color.White);
                    Write(Helpers.TrimTextToWidth(menuWidth - 8, options[i]), menuX + 4, y + 2, UIColors.labelDark);
                }
            }
        }
    }
}
