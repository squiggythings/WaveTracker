using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;

namespace WaveTracker.UI {
    public class Dropdown : Clickable {

        static Dropdown currentlyOpen;

        Element previousFocus;
        bool showMenu;
        public int Value { get; set; }
        public string ValueName {
            get { return options[Value]; }
            set {
                int i = 0;
                foreach (string option in options) {
                    if (option == value) {
                        Value = i;
                        break;
                    }
                    ++i;
                }
            }
        }

        public bool ValueWasChangedInternally { get; set; }

        /// <summary>
        /// If set to true, scrolling over this drop down wrap around the options list
        /// </summary>
        public bool ScrollWrap { get; set; }
        int hoveredValue;
        string[] options;
        int cooldown;

        public Dropdown(int x, int y, Element parent, bool scrollWrap = true) {
            enabled = true;
            this.x = x;
            this.y = y;
            height = 13;
            ScrollWrap = scrollWrap;
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
            if (width == 0)
                width = maxlength + 18;
        }

        public void Update() {
            if (!InFocus)
                cooldown = 2;
            int previousValue = Value;
            if (IsHovered && Input.MouseScrollWheel(KeyModifier.None) != 0) {
                Value -= Input.MouseScrollWheel(KeyModifier.None);
                if (ScrollWrap)
                    Value = (Value + options.Length) % options.Length;
                else
                    Value = Math.Clamp(Value, 0, options.Length - 1);
                hoveredValue = Value;
            }
            if (showMenu) {

                for (int i = 0; i < options.Length; i++) {
                    int y = i * 11 + 14;
                    if (MouseX > 0 && MouseX < width) {
                        if (MouseY >= y && MouseY < y + 11) {
                            hoveredValue = i;
                            if (Input.GetClickUp(KeyModifier.None)) {
                                Value = i;
                                CloseMenu();
                                Input.CancelClick();
                                ValueWasChangedInternally = previousValue != Value;
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
            ValueWasChangedInternally = previousValue != Value;
        }

        void OpenMenu() {
            previousFocus = Input.focus;
            Input.focus = this;
            hoveredValue = Value;
            currentlyOpen = this;
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
            if (Value < options.Length) {
                Write(options[Value], 4, (height + 1) / 2 - 4 + textOffset, getTextColor());
            }
            DrawRect(width - 9, 5 + textOffset, 5, 1, getTextColor());
            DrawRect(width - 8, 6 + textOffset, 3, 1, getTextColor());
            DrawRect(width - 7, 7 + textOffset, 1, 1, getTextColor());
            //if (showMenu) {
            //    DrawMenu();
            //}
        }

        public static void DrawCurrentMenu() {
            if (currentlyOpen != null) {
                currentlyOpen.DrawMenu();
            }
        }

        void DrawMenu() {
            DrawRect(0, 13, width, 11 * options.Length + 2, UIColors.labelDark);
            for (int i = 0; i < options.Length; i++) {
                int y = i * 11 + 14;
                if (i == hoveredValue) {
                    DrawRect(1, y, width - 2, 11, Helpers.LerpColor(UIColors.selection, Color.White, 0.7f));
                    Write(Helpers.TrimTextToWidth(width - 8, options[i]), 4, y + 2, UIColors.black);
                }
                else {
                    DrawRect(1, y, width - 2, 11, Color.White);
                    Write(Helpers.TrimTextToWidth(width - 8, options[i]), 4, y + 2, UIColors.labelDark);
                }
            }
        }
    }
}
