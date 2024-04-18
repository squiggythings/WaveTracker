using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    /// <summary>
    /// Base class for menu items
    /// </summary>
    public abstract class MenuItemBase : Clickable {
        public Menu parentMenu;

        public const int MAX_WIDTH = 400;
        public const int PADDING_LEFT = 2;
        public const int MARGIN_LEFT = 12;

        public const int PADDING_RIGHT = 6;
        public const int MARGIN_RIGHT = 0;
        public const int OPTION_HEIGHT = 10;


        public abstract void Update();
        public abstract void Draw();

        public abstract void SetWidth(int width);

        /// <summary>
        /// Gets the child menu item at <c>index</c>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public MenuItemBase this[int index] {
            get {
                if (this is SubMenu sub) {
                    return sub.menu[index];
                }
                else {
                    throw new IndexOutOfRangeException("This menu item does not have any children!");
                }
            }
            set {
                if (this is SubMenu sub) {
                    sub[index] = value;
                }
                else {
                    throw new IndexOutOfRangeException("This menu item does not have any children!");
                }
            }
        }
    }

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
                if (IsHovered) {
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
            menu.Update();
        }

        void DrawArrow(int x, int y, Color col) {
            DrawRect(x, y, 1, 5, col);
            DrawRect(x + 1, y + 1, 1, 3, col);
            DrawRect(x + 2, y + 2, 1, 1, col);
        }
        public override void Draw() {
            if (enabled) {
                if (IsHovered) {
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

    public class Menu : Clickable {
        public static bool IsAMenuOpen { get { return CurrentOpenMenu != null; } }
        public static Menu CurrentOpenMenu { get; private set; }
        const int MIN_WIDTH = 120;
        Element opened;
        SubMenu parentSubMenu;
        List<MenuItemBase> Items { get; set; }
        List<int> breakIndexes;

        bool IsRootMenu => parent == null;

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
