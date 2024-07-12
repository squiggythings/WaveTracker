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
    public class ColorList : Clickable {
        Dictionary<string, Color> entries;
        ColorButton[] colorButtons;
        Scrollbar scrollbar;
        int numRows;
        int selectedIndex;
        const int ROW_HEIGHT = 13;

        public bool ShowItemNumbers { get; set; }
        public int HoveredIndex { get { return selectedIndex; } set { selectedIndex = Math.Clamp(value, 0, entries.Count - 1); } }

        public ColorList(int x, int y, int width, int numVisibleRows, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = numVisibleRows * ROW_HEIGHT;
            this.numRows = numVisibleRows;
            scrollbar = new Scrollbar(0, 0, width, height, this);
            SetParent(parent);
        }

        /// <summary>
        /// Sets the list for this box to reference
        /// </summary>
        public void SetDictionary(Dictionary<string, Color> colors) {
            entries = new Dictionary<string, Color>();
            foreach (KeyValuePair<string, Color> pair in colors) {
                entries.Add(pair.Key, pair.Value);
            }
        }



        /// <summary>
        /// Saves the list in this box to the given dictionary
        /// </summary>
        public void SaveDictionaryInto(Dictionary<string, Color> colors) {
            foreach (KeyValuePair<string, Color> pair in entries) {
                colors[pair.Key] = pair.Value;
            }
        }


        public void Update() {
            scrollbar.SetSize(entries.Count, numRows);
            scrollbar.UpdateScrollValue();
            scrollbar.Update();
            HoveredIndex = -1;
            if (MouseX >= 0 && MouseX < width - 7) {
                int rowPos = 0;
                for (int i = scrollbar.ScrollValue; i < numRows + scrollbar.ScrollValue; i++) {
                    if (MouseY > rowPos && MouseY <= rowPos + ROW_HEIGHT) {
                        HoveredIndex = i;
                    }
                    rowPos += ROW_HEIGHT;
                }
            }
        }


        public void Draw() {
            //scrollbar.Draw();

            //Color bgColor = new Color(43, 49, 81);
            //Color selectedColor = new Color(59, 68, 107);
            //Color errorColor = new Color(120, 29, 79);
            //int rowNum = numRows - 1;
            //for (int i = numRows + scrollbar.ScrollValue - 1; i >= scrollbar.ScrollValue; i--) {
            //    List<string> conflicts = new List<string>();
            //    conflicts.Add("Conflicts with:");
            //    int maxConflictLength = Helpers.GetWidthOfText("Conflicts with:");
            //    if (entries[i].shortcut != KeyboardShortcut.None && !entries[i].isLabel) {
            //        for (int j = 0; j < entries.Count; j++) {
            //            if (j != i) {
            //                if (entries[i].shortcut == entries[j].shortcut) {
            //                    conflicts.Add(entries[j].actionName);
            //                    int textwidth = Helpers.GetWidthOfText(entries[j].actionName);
            //                    if (textwidth > maxConflictLength)
            //                        maxConflictLength = textwidth;
            //                }
            //            }
            //        }
            //    }
            //    if (entries.Count > i && i >= 0) {
            //        Color rowColor;
            //        if (selectedIndex == i) {
            //            rowColor = selectedColor;
            //        }
            //        else {
            //            rowColor = bgColor;
            //        }

            //        if (rowNum == 0 || entries[i].isLabel) {
            //            string categoryName = entries[i].categoryName;
            //            int nameWidth = Helpers.GetWidthOfText(categoryName);
            //            int labelInset = 12;
            //            DrawRect(0, rowNum * ROW_HEIGHT, width - 6, ROW_HEIGHT, bgColor);
            //            Write(categoryName, labelInset, rowNum * ROW_HEIGHT + 3, UIColors.labelLight);
            //            DrawRect(3, rowNum * ROW_HEIGHT + 6, labelInset - 3 - 3, 1, UIColors.label);
            //            DrawRect(labelInset + nameWidth + 3, rowNum * ROW_HEIGHT + 6, width - 7 - 3 - nameWidth - 3 - labelInset, 1, UIColors.label);
            //        }
            //        else {
            //            if (conflicts.Count > 1) {
            //                rowColor = errorColor;
            //            }
            //            DrawRect(0, rowNum * ROW_HEIGHT, width - 6, ROW_HEIGHT, rowColor);

            //            Write(entries[i].actionName, 3, rowNum * ROW_HEIGHT + 3, Color.White);
            //            if (editButton.Value && HoveredIndex == i) {
            //                if (Input.CurrentPressedShortcut == KeyboardShortcut.None) {
            //                    Write("waiting for input...", width / 2, rowNum * ROW_HEIGHT + 3, UIColors.label);
            //                }
            //                else if (Input.CurrentPressedShortcut.Key == Keys.None) {
            //                    Write(Helpers.ModifierToString(Input.CurrentModifier), width / 2, rowNum * ROW_HEIGHT + 3, UIColors.labelLight);
            //                }
            //                else {
            //                    Write(Input.CurrentPressedShortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, Color.White);
            //                }
            //            }
            //            else {
            //                if (entries[i].shortcut == KeyboardShortcut.None) {
            //                    if (entries[i].shortcut == App.Settings.Keyboard.defaultShortcuts.ElementAt(entries[i].dictionaryIndex).Value) {
            //                        Write("(none)", width / 2, rowNum * ROW_HEIGHT + 3, UIColors.label);
            //                    }
            //                    else {
            //                        Write(entries[i].shortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, new Color(248, 208, 102, 100));
            //                    }
            //                }
            //                else {
            //                    if (entries[i].shortcut == App.Settings.Keyboard.defaultShortcuts.ElementAt(entries[i].dictionaryIndex).Value) {
            //                        Write(entries[i].shortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, Color.White);
            //                    }
            //                    else {
            //                        Write(entries[i].shortcut.ToString(), width / 2, rowNum * ROW_HEIGHT + 3, new Color(248, 208, 102));
            //                    }
            //                }
            //            }

            //            if (conflicts.Count > 1) {
            //                DrawSprite(width - 7 - 11, rowNum * ROW_HEIGHT + 2, new Rectangle(484, 144, 9, 9));
            //                if (HoveredIndex == i) {
            //                    if (true) {
            //                        DrawRect(width - 11, rowNum * ROW_HEIGHT + 2, 5, 9, new Color(230, 69, 57));
            //                        DrawRoundedRect(width - 8, rowNum * ROW_HEIGHT + 2, maxConflictLength + 10, conflicts.Count * 11 + 5, new Color(237, 34, 34));
            //                        int ypos = rowNum * ROW_HEIGHT + 5;

            //                        foreach (string action in conflicts) {
            //                            Write(action, width - 4, ypos, Color.White);
            //                            ypos += 11;
            //                        }
            //                    }

            //                }
            //            }
            //        }

            //        --rowNum;
            //    }
            //}
            //editButton.Draw();
            //resetToDefaultButton.Draw();

        }

        class ListEntry {
            public int dictionaryIndex;
            public string categoryName;
            public string actionName;
            public Color color;
            public bool isLabel;

            public ListEntry(int i, string categoryName, string actionName, Color color, bool isLabel) {
                dictionaryIndex = i;
                this.categoryName = categoryName;
                this.actionName = actionName;
                this.color = color;
                this.isLabel = isLabel;
            }
        }
    }
}
