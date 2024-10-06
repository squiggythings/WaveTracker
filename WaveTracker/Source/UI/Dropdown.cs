using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class Dropdown : Clickable {
        public static Dropdown currentlyOpen;
        /// <summary>
        /// Returns true if a dropdown is open
        /// </summary>
        public static bool IsADropdownOpen { get { return currentlyOpen != null; } }

        /// <summary>
        /// Returns true if any dropdown or dropdown button menu is open
        /// </summary>
        public static bool IsAnyDropdownOpen { get { return IsADropdownOpen || DropdownButton.IsADropdownButtonOpen; } }

        private Element previousFocus;
        private bool showMenu;
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

        private int hoveredValue;
        private string[] options;
        private int cooldown;

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
                options[i] = items[i];
                if (Helpers.GetWidthOfText(options[i]) > maxlength) {
                    maxlength = Helpers.GetWidthOfText(options[i]);
                }
            }
            if (width == 0) {
                width = maxlength + 18;
            }
        }

        public void Update() {
            int previousValue = Value;
            if (!InFocus) {
                cooldown = 2;
            }
            else {
                if (IsHovered && Input.MouseScrollWheel(KeyModifier.None) != 0) {
                    Value -= Input.MouseScrollWheel(KeyModifier.None);
                    Value = ScrollWrap ? (Value + options.Length) % options.Length : Math.Clamp(Value, 0, options.Length - 1);
                    hoveredValue = Value;
                }
                if (currentlyOpen == this) {
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
                    if (cooldown > 0) {
                        cooldown--;
                    }
                }
            }
            ValueWasChangedInternally = previousValue != Value;
        }

        private void OpenMenu() {
            previousFocus = Input.focus;
            Input.focus = this;
            hoveredValue = Value;
            currentlyOpen = this;
            showMenu = true;
        }

        private void CloseMenu() {
            showMenu = false;
            currentlyOpen = null;
            Input.focus = previousFocus;
        }

        private Color GetBackgroundColor() {
            if (!enabled) {
                return ButtonColors.backgroundColorDisabled;
            }
            else {
                if (IsPressed) {
                    return ButtonColors.backgroundColorPressed;
                }
                else {
                    return IsHovered || showMenu ? ButtonColors.backgroundColorHover : ButtonColors.backgroundColor;
                }
            }
        }

        private Color GetTextColor() {
            if (!enabled) {
                return ButtonColors.textColorDisabled;
            }
            else {
                return IsPressed ? ButtonColors.textColorPressed : ButtonColors.textColor;
            }
        }
        public void Draw() {
            int textOffset = IsPressed && enabled ? 1 : 0;
            DrawRoundedRect(0, 0, width, height, GetBackgroundColor());
            if (Value < options.Length) {
                Write(options[Value], 4, (height + 1) / 2 - 4 + textOffset, GetTextColor());
            }
            DrawRect(width - 9, 5 + textOffset, 5, 1, GetTextColor());
            DrawRect(width - 8, 6 + textOffset, 3, 1, GetTextColor());
            DrawRect(width - 7, 7 + textOffset, 1, 1, GetTextColor());
        }

        public static void DrawCurrentMenu() {
            currentlyOpen?.DrawMenu();
        }

        private void DrawMenu() {
            DrawRect(0, 13, width, 11 * options.Length + 2, UIColors.labelDark);
            for (int i = 0; i < options.Length; i++) {
                int y = i * 11 + 14;
                if (i == hoveredValue) {
                    DrawRect(1, y, width - 2, 11, UIColors.selection.Lerp(Color.White, 0.7f));
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
