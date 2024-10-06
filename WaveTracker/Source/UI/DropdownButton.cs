using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class DropdownButton : Clickable {
        private static DropdownButton currentlyOpen;
        /// <summary>
        /// Returns true if a dropdown button is open
        /// </summary>
        public static bool IsADropdownButtonOpen { get { return currentlyOpen != null; } }

        private Element previousFocus;
        private bool showMenu;
        public int SelectedIndex { get; set; }
        public bool SelectedAnItem { get; set; }

        public bool LabelIsCentered { get; set; }

        private int menuWidth;
        private int menuX;
        private int menuY;
        private string label;
        private int labelWidth;
        private int hoveredValue;
        private string[] options;
        private int cooldown;

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
                options[i] = items[i];
                if (Helpers.GetWidthOfText(options[i]) > maxlength) {
                    maxlength = Helpers.GetWidthOfText(options[i]);
                }
            }
            menuWidth = maxlength + 18;
        }

        public void Update() {
            if (!InFocus) {
                cooldown = 2;
            }

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
                if (cooldown > 0) {
                    cooldown--;
                }
            }
        }

        private void OpenMenu() {
            previousFocus = Input.focus;
            Input.focus = this;
            hoveredValue = -1;
            currentlyOpen = this;
            menuX = width;
            menuY = 0;
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

            if (LabelIsCentered) {
                Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4 + textOffset, ButtonColors.textColor);
            }
            else {
                Write(label, 4, (height + 1) / 2 - 4 + textOffset, ButtonColors.textColor);
            }

            DrawRect(width - 9, 5 + textOffset, 5, 1, GetTextColor());
            DrawRect(width - 8, 6 + textOffset, 3, 1, GetTextColor());
            DrawRect(width - 7, 7 + textOffset, 1, 1, GetTextColor());
        }

        public static void DrawCurrentMenu() {
            currentlyOpen?.DrawMenu();
        }

        private void DrawMenu() {
            DrawRect(menuX, menuY, menuWidth, 11 * options.Length + 2, UIColors.labelDark);
            for (int i = 0; i < options.Length; i++) {
                int y = i * 11 + menuY + 1;
                if (i == hoveredValue) {
                    DrawRect(menuX + 1, y, menuWidth - 2, 11, UIColors.selection.Lerp(Color.White, 0.7f));
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
