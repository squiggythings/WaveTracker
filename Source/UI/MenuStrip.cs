using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class MenuStrip : Clickable {
        public List<MenuStripButton> StripButtons { get; private set; }
        //MenuStripButton currentButton;
        Menu currentMenu;
        int nextX;
        bool showAlt;

        public MenuStrip(int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            this.height = 9;
            this.width = width;
            StripButtons = new List<MenuStripButton>();
            SetParent(parent);
            AddButton("File", new Menu(new MenuItemBase[] {
                new MenuOption("New",null),
                new MenuOption("Open...",null),
                new MenuOption("Save",null),
                new MenuOption("Save As...",null),
                null,
                new MenuOption("Export as WAV...",null),
                null,
                new MenuOption("Configuration...",null),
                null,
                new SubMenu("Recent files",new MenuItemBase[] {
                    new MenuOption("Clear",null),
                    null,
                    new MenuOption("1. C:\\Users\\Elias\\Music\\wavetracker\\moon2.0.wtm",null),
                    new MenuOption("2. C:\\Users\\Elias\\Music\\wavetracker\\wtdemo2.wtm",null),
                    new MenuOption("3. C:\\Users\\Elias\\Music\\wavetracker\\freezedraft.wtm",null),
                    new MenuOption("4. C:\\Users\\Elias\\Music\\wavetracker\\fmcomplextro.wtm",null),
                    new MenuOption("5. C:\\Users\\Elias\\Music\\wavetracker\\modtesting.wtm",null),
                    new MenuOption("6. C:\\Users\\Elias\\Music\\wavetracker\\largetest2.0.wtm",null),
                    new MenuOption("7. C:\\Users\\Elias\\Music\\wavetracker\\katamarisolo7.wtm",null),
                    new MenuOption("8. C:\\Users\\Elias\\Music\\wavetracker\\itsmyblaster2.0.wtm",null),
                }),
                null,
                new MenuOption("Exit", App.ExitApplication),
            }));
            AddButton("Edit", new Menu(new MenuItemBase[] {
                new MenuOption("Undo",null),
                new MenuOption("Redo",null),
                null,
                new MenuOption("Cut",null),
                new MenuOption("Copy",null),
                new MenuOption("Paste",null),
                new MenuOption("Paste and mix",null),
            }));
            AddButton("Song", new Menu(new MenuItemBase[] {
                new MenuOption("Insert frame",null),
                new MenuOption("Remove frame",null),
                new MenuOption("Duplicate frame",null),
                new MenuOption("Duplicate frame",null),
                null,
                new MenuOption("Move frame up",null),
                new MenuOption("Move frame down",null),
            }));
        }

        public void AddButton(string name, Menu menu) {
            StripButtons.Add(new MenuStripButton(name, nextX, 0, menu, this));
            nextX += StripButtons[StripButtons.Count - 1].width;
        }

        public void Update() {
            foreach (MenuStripButton button in StripButtons) {
                if (button.Clicked) {
                    currentMenu = button.menu;
                    currentMenu.Open();
                }
                if (currentMenu != null && currentMenu.enabled && button.enabled) {
                    if (button.IsMouseOverRegion && button.menu != currentMenu) {
                        currentMenu.Close();
                        currentMenu = button.menu;
                        currentMenu.Open();
                    }
                }
            }
            currentMenu?.Update();
        }

        public void Draw() {
            DrawRect(0, 0, width, height, Color.White);
            DrawRect(0, height, width, 1, UIColors.labelLight);
            foreach (MenuStripButton button in StripButtons) {
                button.Draw(showAlt);
            }
            currentMenu?.Draw();
        }
    }

    public class MenuStripButton : Clickable {
        const int MARGIN = 5;
        public Menu menu;

        public string Name { get; private set; }
        public MenuStripButton(string name, int x, int y, Menu menu, Element parent) {
            width = Helpers.GetWidthOfText(name) + MARGIN * 2;
            height = 9;
            this.menu = menu;
            this.x = x;
            this.y = y;
            menu.x = x;
            menu.y = height;
            Name = name;
            SetParent(parent);
        }

        public void Update() {
            //if (Clicked) {
            //    menu.Open();
            //}
        }
        public void Draw(bool highlightletter) {
            if (menu.enabled) {
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
