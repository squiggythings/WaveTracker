using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace WaveTracker.UI {
    public class Menu : Clickable {
        public static bool IsAMenuOpen { get { return CurrentOpenMenu != null; } }
        public static Menu CurrentOpenMenu { get; private set; }
        const int MIN_WIDTH = 120;
        Element opened;
        SubMenu parentSubMenu;
        List<MenuItemBase> Items { get; set; }
        List<int> breakIndexes;

        bool IsRootMenu => parentSubMenu == null;

        public Menu() {
            enabled = false;
            parentSubMenu = null;
            Items = new List<MenuItemBase>();
            breakIndexes = new List<int>();
            width = MIN_WIDTH;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public Menu(MenuItemBase[] items) {
            enabled = false;
            parentSubMenu = null;
            Items = new List<MenuItemBase>();
            breakIndexes = new List<int>();
            width = MIN_WIDTH;
            AddItems(items);
        }

        public Menu(SubMenu parentSubMenu) {
            enabled = false;
            this.parentSubMenu = parentSubMenu;
            Items = new List<MenuItemBase>();
            breakIndexes = new List<int>();
            width = 0;
        }

        /// <summary>
        /// Adds an item and adjusts the width of all other items if necessary
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(MenuItemBase item) {
            item.SetParent(this);
            item.parentMenu = this;
            Items.Add(item);
            if (item.width + 6 > width)
                width = item.width + 6;
            UpdateItemPositions();
        }

        public void AddItems(MenuItemBase[] items) {
            foreach (MenuItemBase item in items) {
                if (item == null)
                    AddBreak();
                else
                    AddItem(item);
            }
        }

        void UpdateItemPositions() {
            int itemY = 3;
            int i = 0;
            foreach (MenuItemBase menuItem in Items) {
                menuItem.x = 3;
                menuItem.y = itemY;
                menuItem.SetWidth(width - 6);
                itemY += menuItem.height;
                if (breakIndexes.Contains(i)) {
                    itemY += 5;
                }
                ++i;
            }
            height = itemY + 3;
        }

        /// <summary>
        /// Adds a break to the list of options in the menu
        /// </summary>
        public void AddBreak() {
            if (Items.Count > 0) {
                breakIndexes.Add(Items.Count - 1);
            }
            UpdateItemPositions();
        }

        public bool IsHoveredOrAChildMenuHovered() {
            if (enabled) {
                if (IsHovered) return true;
                foreach (MenuItemBase item in Items) {
                    if (item is SubMenu sub) {
                        if (sub.menu.enabled && sub.menu.IsHoveredOrAChildMenuHovered())
                            return true;
                    }
                }
            }
            return false;
        }

        public void Open() {
            if (enabled == false) {
                enabled = true;
                if (IsRootMenu) {
                    CurrentOpenMenu = this;
                    opened = Input.focus;
                    Input.focus = this;
                }
            }
        }

        public bool IsOutOfBoundsX() {
            return GlobalX + width > App.WindowWidth - 12;
        }
        public bool IsOutOfBoundsY() {
            return GlobalY + height > App.WindowHeight - 12;
        }

        /// <summary>
        /// Position a root menu at X and Y, and flips if needed to stay in bounds
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPositionClamped(int x, int y) {
            if (IsRootMenu) {
                this.x = x;
                this.y = y;
                if (IsOutOfBoundsX()) {
                    this.x -= width;
                }
                if (IsOutOfBoundsY())
                    this.y -= height;
            }
        }

        /// <summary>
        /// Closes this menu and its children
        /// </summary>
        public void Close() {
            if (enabled == true) {
                enabled = false;
                if (IsRootMenu) {
                    Input.focus = opened;
                }
                foreach (MenuItemBase item in Items) {
                    if (item is SubMenu sub) {
                        sub.menu.Close();
                    }
                }
                if (CurrentOpenMenu == this) {
                    CurrentOpenMenu = null;
                }
            }
        }
        /// <summary>
        /// Closes this menu and its parents
        /// </summary>
        public void CloseParent() {
            if (!IsRootMenu) {
                if (parentSubMenu != null) {
                    parentSubMenu.parentMenu.CloseParent();
                }
                else {
                    Close();
                }
            }
            else {
                Close();
            }
        }

        public void Update() {
            if (enabled && InFocus) {
                if (IsRootMenu) {
                    if (Input.GetClickDown(KeyModifier._Any) || Input.GetRightClickDown(KeyModifier._Any)) {
                        if (!IsHoveredOrAChildMenuHovered()) {
                            Close();
                        }
                    }
                }
                foreach (MenuItemBase item in Items) {
                    item.Update();
                }
            }
        }

        public void Draw() {
            if (enabled) {
                DrawRect(-1, -1, width + 2, height + 2, Helpers.Alpha(UIColors.black, 30));
                DrawRect(0, 0, width, height, UIColors.labelLight);
                DrawRect(1, 1, width - 2, height - 2, Color.White);
                for (int i = Items.Count - 1; i >= 0; --i) {
                    Items[i].Draw();
                    if (breakIndexes.Contains(i)) {
                        DrawRect(3 + MenuItemBase.MARGIN_LEFT, Items[i].y + Items[i].height + 2, width - MenuItemBase.MARGIN_RIGHT - MenuItemBase.MARGIN_LEFT - 6, 1, Helpers.Alpha(UIColors.labelLight, 128));
                    }
                }
            }
        }

        public MenuItemBase this[int index] {
            get {
                return Items[index];
            }
            set {
                Items[index] = value;
                UpdateItemPositions();
            }
        }

    }
}
