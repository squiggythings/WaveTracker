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
    public class ListBox<T> : Clickable {

        List<T> items;
        Scrollbar scrollbar;
        int numRows;
        int selectedIndex;


        public bool ShowItemNumbers { get; set; }
        public int SelectedIndex { get { return selectedIndex; } set { selectedIndex = Math.Clamp(value, 0, items.Count - 1); } }
        public T SelectedItem {
            get {
                if (items == null)
                    return default(T);
                else if (items.Count == 0)
                    return default(T);
                else
                    return items[SelectedIndex];
            }
        }

        public ListBox(int x, int y, int width, int numRows, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = numRows * 11;
            this.numRows = numRows;
            scrollbar = new Scrollbar(0, 0, width, height, this);
            SetParent(parent);
        }

        public void SetList(List<T> list) {
            items = list;
        }


        public void Update() {
            if (ClickedDown) {
                if (MouseX >= 0 && MouseX < width - 7) {
                    int rowNum = 0;
                    for (int i = scrollbar.ScrollValue; i < numRows + scrollbar.ScrollValue; i++) {
                        if (MouseY > rowNum && MouseY <= rowNum + 11) {
                            SelectedIndex = i;
                        }
                        rowNum += 11;
                    }
                }
            }
            if (GlobalPointIsInBounds(Input.lastClickLocation)) {
                if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None)) {
                    SelectedIndex--;
                    if (SelectedIndex < 0)
                        SelectedIndex = 0;
                    MoveBounds();
                }
                if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None)) {
                    SelectedIndex++;
                    MoveBounds();
                }
            }
            scrollbar.SetSize(items.Count, numRows);
            scrollbar.UpdateScrollValue();
            scrollbar.Update();
        }

        public void MoveBounds() {
            if (SelectedIndex > scrollbar.ScrollValue + numRows - 1) {
                scrollbar.ScrollValue = SelectedIndex - numRows + 1;
            }
            if (SelectedIndex < scrollbar.ScrollValue) {
                scrollbar.ScrollValue = SelectedIndex;
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
                if (i == SelectedIndex)
                    rowColor = selected;
                else if (i % 2 == 0)
                    rowColor = even;
                else
                    rowColor = odd;
                DrawRect(0, rowNum * 11, width, 11, rowColor);
                if (items.Count > i && i >= 0) {
                    string text;
                    if (ShowItemNumbers) {
                        text = "#" + (i + 1) + ". " + items[i].ToString();
                    }
                    else {
                        text = items[i].ToString();
                    }
                    Write(Helpers.TrimTextToWidth(width - 7, Helpers.FlushString(text)), 3, rowNum * 11 + 2, Color.White);

                }
                ++rowNum;
            }
            scrollbar.Draw();
        }
    }
}
