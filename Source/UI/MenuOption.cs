using Microsoft.Xna.Framework;
using System;


namespace WaveTracker.UI {
    /// <summary>
    /// A menu item that performs an action when clicked
    /// </summary>
    public class MenuOption : MenuItemBase {

        public string Name { get; set; }
        public Action OnClick { get; set; }


        public MenuOption(string name, Action onClick) {
            Name = name;
            OnClick = onClick;
            height = OPTION_HEIGHT;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
        }

        public MenuOption(string name, Action onClick, string tooltip) {
            Name = name;
            OnClick = onClick;
            height = OPTION_HEIGHT;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
            SetTooltip("", tooltip);
        }
        public MenuOption(string name, Action onClick, bool enabledOrNot, string tooltip) {
            Name = name;
            OnClick = onClick;
            height = OPTION_HEIGHT;
            enabled = enabledOrNot;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
            SetTooltip("", tooltip);
        }
        public MenuOption(string name, Action onClick, bool enabledOrNot) {
            Name = name;
            OnClick = onClick;
            height = OPTION_HEIGHT;
            enabled = enabledOrNot;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
        }


        public override void SetWidth(int width) {
            this.width = width;
        }

        public override void Update() {
            if (enabled) {
                if (Clicked) {
                    parentMenu.CloseParent();
                    OnClick?.Invoke();
                }
            }
        }
        public override void Draw() {
            if (enabled) {
                if (IsHoveredExclusive) {
                    DrawRect(0, 0, width, height, UIColors.selectionLight);
                }
                if (OnClick != null)
                    Write(Name, MARGIN_LEFT + PADDING_LEFT, 1, UIColors.labelDark);
                else
                    Write(Name, MARGIN_LEFT + PADDING_LEFT, 1, Color.Red);
            }
            else {
                if (OnClick != null)
                    Write(Name, MARGIN_LEFT + PADDING_LEFT, 1, Helpers.Alpha(UIColors.labelLight, 128));
                else
                    Write(Name, MARGIN_LEFT + PADDING_LEFT, 1, Helpers.Alpha(Color.Red, 128));
            }
        }
    }
}
