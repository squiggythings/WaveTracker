﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace WaveTracker.UI {
    public class MenuStrip : Clickable {
        public List<MenuStripButton> StripButtons { get; private set; }

        //MenuStripButton currentButton;
        private Menu currentMenu;
        private int nextX;

        public MenuStrip(int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            height = 9;
            this.width = width;
            StripButtons = [];
            SetParent(parent);
        }

        public void AddButton(string name, Menu menu) {
            StripButtons.Add(new MenuStripButton(name, nextX, 0, menu, this));
            nextX += StripButtons[StripButtons.Count - 1].width;
        }
        public void AddButton(string name, Func<Menu> createMenuFunc) {
            StripButtons.Add(new MenuStripButton(name, nextX, 0, createMenuFunc, this));
            nextX += StripButtons[StripButtons.Count - 1].width;
        }

        public void Update() {
            width = App.WindowWidth;
            currentMenu?.Update();
            foreach (MenuStripButton button in StripButtons) {
                if (button.ClickedDown) {
                    button.CreateMenu();
                    currentMenu = button.menu;
                    currentMenu.Open();
                }

                if (currentMenu != null && currentMenu.enabled && button.enabled) {
                    if (button.IsMouseOverRegion && button.menu != currentMenu) {
                        currentMenu.Close();
                        button.CreateMenu();
                        currentMenu = button.menu;
                        currentMenu.Open();
                    }
                }
            }

        }

        public void Draw() {
            DrawRect(0, 0, width, height, Color.White);
            DrawRect(0, height, width, 1, UIColors.labelLight);
            foreach (MenuStripButton button in StripButtons) {
                button.Draw();
            }
            currentMenu?.Draw();
        }
    }

    public class MenuStripButton : Clickable {
        private const int MARGIN = 5;
        public Menu menu;
        private Func<Menu> createMenuFunc;
        public string Name { get; private set; }
        public MenuStripButton(string name, int x, int y, Menu menu, Element parent) {
            width = Helpers.GetWidthOfText(name) + MARGIN * 2;
            height = 9;
            this.menu = menu;
            this.x = x;
            this.y = y;
            menu.x = 0;
            menu.y = height;
            Name = name;
            menu.SetParent(this);
            SetParent(parent);
        }
        public MenuStripButton(string name, int x, int y, Func<Menu> createMenu, Element parent) {
            width = Helpers.GetWidthOfText(name) + MARGIN * 2;
            height = 9;
            createMenuFunc = createMenu;
            this.x = x;
            this.y = y;
            Name = name;
            SetParent(parent);
        }

        public void CreateMenu() {
            if (createMenuFunc != null) {
                menu = createMenuFunc.Invoke();
                menu.x = 0;
                menu.y = height;
                menu.SetParent(this);
            }
        }

        public void Draw() {
            if (menu != null && menu.enabled) {
                DrawRect(0, 0, width, height, UIColors.selection);
                Write(Name, MARGIN, 1, Color.White);

            }
            else {
                if (IsHovered) {
                    DrawRect(0, 0, width, height, UIColors.selectionLight);
                }
                else {
                    DrawRect(0, 0, width, height, Color.White);
                }
                Write(Name, MARGIN, 1, UIColors.black);

            }
        }
    }
}
