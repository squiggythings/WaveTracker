using Microsoft.Xna.Framework;
using System;


namespace WaveTracker.UI {
    /// <summary>
    /// A menu item that holds another menu inside it
    /// </summary>
    public class SubMenu : MenuItemBase {
        const int TIME_TO_OPEN_MENU = 500;
        public string Name { get; set; }
        public Menu menu;
        /// <summary>
        /// milliseconds since hovered
        /// </summary>
        int hoverTime;

        public SubMenu(string name) {
            menu = new Menu(this);
            menu.SetParent(this);
            Name = name;
            height = OPTION_HEIGHT;
            menu.enabled = false;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
        }
        public SubMenu(string name, MenuItemBase[] items) {
            menu = new Menu(this);
            menu.SetParent(this);
            Name = name;
            height = OPTION_HEIGHT;
            menu.enabled = false;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
            AddItems(items);
        }
        public SubMenu(string name, Menu menu) {
            this.menu = menu;
            menu.SetParent(this);
            Name = name;
            height = OPTION_HEIGHT;
            menu.enabled = false;
            width = Math.Min(Helpers.GetWidthOfText(name) + MARGIN_LEFT + MARGIN_RIGHT + PADDING_LEFT + PADDING_RIGHT, MAX_WIDTH);
        }
        public override void SetWidth(int width) {
            this.width = width;
        }

        public void AddItem(MenuItemBase item) {
            menu.AddItem(item);
        }

        public void AddItems(MenuItemBase[] items) {
            menu.AddItems(items);
        }

        internal void OnMenuClose() {
            hoverTime = 0;
        }

        void PositionMenu() {
            menu.x = width;
            menu.y = -1;
            if (menu.IsOutOfBoundsY()) {
                menu.y = -menu.height - 1 + height;
            }
            if (menu.IsOutOfBoundsX()) {
                menu.x = -menu.width;
            }
        }

        public override void Update() {
            menu.Update();
            if (IsHovered || (menu.IsHoveredOrAChildMenuHovered())) {
                if (hoverTime < 0)
                    hoverTime = 0;
                if (hoverTime < TIME_TO_OPEN_MENU) {
                    hoverTime += Input.deltaTime;
                }
                else {
                    menu.Open();
                    PositionMenu();
                    hoverTime = TIME_TO_OPEN_MENU;

                }
            }
            else {
                if (hoverTime > 0)
                    hoverTime = 0;
                if (hoverTime > -TIME_TO_OPEN_MENU) {
                    hoverTime -= Input.deltaTime;
                }
                else {
                    menu.Close();
                    hoverTime = -TIME_TO_OPEN_MENU;
                }
            }
        }

        void DrawArrow(int x, int y, Color col) {
            DrawRect(x, y, 1, 5, col);
            DrawRect(x + 1, y + 1, 1, 3, col);
            DrawRect(x + 2, y + 2, 1, 1, col);
        }
        public override void Draw() {
            
            if (enabled) {
                if (IsHoveredExclusive) {
                    DrawRect(0, 0, width, height, UIColors.selectionLight);
                }
                Write(Name, MARGIN_LEFT + PADDING_LEFT, 1, UIColors.labelDark);
                DrawArrow(width - 7, 2, UIColors.labelDark);
            }
            else {
                Write(Name, MARGIN_LEFT + PADDING_LEFT, 1, Helpers.Alpha(UIColors.labelLight, 128));
                DrawArrow(width - 7, 2, Helpers.Alpha(UIColors.labelLight, 128));
            }
            menu.Draw();
        }
    }
}
