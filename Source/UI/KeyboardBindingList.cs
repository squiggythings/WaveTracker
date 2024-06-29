using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.UI {
    public class KeyboardBindingList : Clickable {
        List<ListEntry> entries;
        Scrollbar scrollbar;
        int numRows;
        int selectedIndex;
        const int ROW_HEIGHT = 13;
        SpriteToggleTwoSided editButton;
        SpriteButton resetToDefaultButton;

        public bool ShowItemNumbers { get; set; }
        public int HoveredIndex { get { return selectedIndex; } set { selectedIndex = Math.Clamp(value, 0, entries.Count - 1); } }

        public KeyboardBindingList(int x, int y, int width, int numVisibleRows, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = numVisibleRows * ROW_HEIGHT;
            this.numRows = numVisibleRows;
            scrollbar = new Scrollbar(0, 0, width, height, this);
            editButton = new SpriteToggleTwoSided(0, 0, 10, 9, 474, 144, this);
            resetToDefaultButton = new SpriteButton(0, 0, 10, 9, 464, 144, this);
            resetToDefaultButton.SetTooltip("Revert to default", "Revert the keybind to the default setting");
            SetParent(parent);
        }

        /// <summary>
        /// Sets the list for this box to reference
        /// </summary>
        /// <param name="list"></param>
        public void SetDictionary(Dictionary<string, KeyboardShortcut> bindings) {
            entries = new List<ListEntry>();
            List<string> categories = new List<string>();
            for (int i = 0; i < bindings.Count; i++) {
                string category = bindings.ElementAt(i).Key.Split('\\')[0];
                string name = bindings.ElementAt(i).Key.Split('\\')[1];
                if (!categories.Contains(category)) {
                    categories.Add(category);
                    entries.Add(new ListEntry(i, category, "", new KeyboardShortcut(), true));
                }
                entries.Add(new ListEntry(i, category, name, bindings.ElementAt(i).Value, false));
            }
        }


        public void Update() {
            scrollbar.SetSize(entries.Count, numRows);
            scrollbar.UpdateScrollValue();
            scrollbar.Update();
            if (!editButton.Value) {
                editButton.SetTooltip("Rebind action", "Rebind the action to a new keystroke");
                HoveredIndex = -1;
                editButton.enabled = false;
                resetToDefaultButton.enabled = false;
                if (MouseX >= 0 && MouseX < width - 7) {
                    int rowPos = 0;
                    for (int i = scrollbar.ScrollValue; i < numRows + scrollbar.ScrollValue; i++) {
                        if (MouseY > rowPos && MouseY <= rowPos + ROW_HEIGHT) {
                            HoveredIndex = i;
                            if (!entries[i].isLabel) {
                                editButton.enabled = true;
                                resetToDefaultButton.enabled = entries[i].shortcut != App.CurrentSettings.keyboard.defaultShortcuts.ElementAt(entries[i].dictionaryIndex).Value;
                                editButton.x = (width - 7) / 2 - editButton.width - 0;
                                editButton.y = rowPos + 2;
                                resetToDefaultButton.x = (width - 7) / 2 - editButton.width + Helpers.GetWidthOfText(entries[i].shortcut.ToString()) + 15;
                                resetToDefaultButton.y = rowPos + 2;
                                editButton.Update();
                                if (resetToDefaultButton.Clicked) {
                                    entries[i].shortcut = App.CurrentSettings.keyboard.defaultShortcuts.ElementAt(entries[i].dictionaryIndex).Value;
                                }
                            }
                        }
                        rowPos += ROW_HEIGHT;
                    }
                }
            }
            else {
                resetToDefaultButton.enabled = false;
                editButton.enabled = true;
                editButton.SetTooltip("Clear keybind", "Remove the keybind");
                editButton.Update();
                if (editButton.Clicked) {
                    entries[HoveredIndex].shortcut = KeyboardShortcut.None;
                }
                else if (Input.CurrentPressedShortcut.IsPressedDown()) {
                    editButton.Value = false;
                    entries[HoveredIndex].shortcut = Input.CurrentPressedShortcut;
                }
            }
        }


        public void Draw() {
            Color odd = new Color(43, 49, 81);
            Color even = new Color(59, 68, 107);
            Color selected = UIColors.selection;
            int rowNum = 0;
            DrawRect(0, 0, width, height, selected);
            for (int i = scrollbar.ScrollValue; i < numRows + scrollbar.ScrollValue; i++) {
                Color rowColor;
                if (selectedIndex == i)
                    rowColor = even;
                else
                    rowColor = odd;

                List<string> conflictingActions = new List<string>();
                if (entries[i].shortcut != KeyboardShortcut.None && !entries[i].isLabel) {
                    for (int j = 0; j < entries.Count; j++) {
                        if (j != i) {
                            if (entries[i].shortcut == entries[j].shortcut) {
                                conflictingActions.Add(entries[j].actionName);
                                rowColor = new Color(120, 29, 79);
                            }
                        }
                    }
                }

                DrawRect(0, rowNum * ROW_HEIGHT, width, ROW_HEIGHT, rowColor);
                if (entries.Count > i && i >= 0) {
                    if (rowNum == 0 || entries[i].isLabel) {
                        string categoryName = entries[i].categoryName;
                        int nameWidth = Helpers.GetWidthOfText(categoryName);
                        int labelInset = 12;
                        DrawRect(0, rowNum * ROW_HEIGHT, width, ROW_HEIGHT, odd);
                        Write(categoryName, labelInset, rowNum * ROW_HEIGHT + 3, UIColors.labelLight);
                        DrawRect(3, rowNum * ROW_HEIGHT + 6, labelInset - 3 - 3, 1, UIColors.label);
                        DrawRect(labelInset + nameWidth + 3, rowNum * ROW_HEIGHT + 6, width - 7 - 3 - nameWidth - 3 - labelInset, 1, UIColors.label);
                    }
                    else {
                        Write(entries[i].actionName, 3, rowNum * ROW_HEIGHT + 3, Color.White);
                        if (editButton.Value && HoveredIndex == i) {
                            if (Input.CurrentPressedShortcut == KeyboardShortcut.None) {
                                Write("waiting for input...", width / 2, rowNum * ROW_HEIGHT + 3, UIColors.label);
                            }
                            else if (Input.CurrentPressedShortcut.Key == Keys.None) {
                                Write(Helpers.ModifierToString(Input.CurrentModifier), width / 2, rowNum * ROW_HEIGHT + 3, UIColors.labelLight);
                            }
                            else {
                                Write(Input.CurrentPressedShortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, Color.White);
                            }
                        }
                        else {
                            if (entries[i].shortcut == KeyboardShortcut.None) {
                                Write("(none)", width / 2, rowNum * ROW_HEIGHT + 3, UIColors.label);
                            }
                            else {
                                if (entries[i].shortcut == App.CurrentSettings.keyboard.defaultShortcuts.ElementAt(entries[i].dictionaryIndex).Value) {
                                    Write(entries[i].shortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, Color.White);
                                }
                                else {
                                    Write(entries[i].shortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, new Color(248, 208, 102));
                                }
                            }
                        }
                        if (conflictingActions.Count > 0) {
                            DrawSprite(width - 7 - 12, rowNum * ROW_HEIGHT + 2, new Rectangle(484, 144, 9, 9));
                            if (MouseY > rowNum * ROW_HEIGHT && MouseY <= (rowNum + 1) * ROW_HEIGHT) {
                                if (MouseX < width - 7 - 12 && MouseX > width - 7 - 3) {
                                    int ypos = rowNum * ROW_HEIGHT;
                                    foreach (string action in conflictingActions) {
                                        Write(action, width + 4, ypos, Color.Red);
                                        ypos += 11;
                                    }
                                }
                            }
                        }
                    }
                }
                ++rowNum;
            }
            editButton.Draw();
            resetToDefaultButton.Draw();
            scrollbar.Draw();
        }

        class ListEntry {
            public int dictionaryIndex;
            public string categoryName;
            public string actionName;
            public KeyboardShortcut shortcut;
            public bool isLabel;

            public ListEntry(int i, string categoryName, string actionName, KeyboardShortcut shortcut, bool isLabel) {
                dictionaryIndex = i;
                this.categoryName = categoryName;
                this.actionName = actionName;
                this.shortcut = shortcut;
                this.isLabel = isLabel;
            }
        }
    }
}
