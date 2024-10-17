using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace WaveTracker.UI {
    public class ColorButtonList : Clickable {
        private Dictionary<string, ColorButton> entries;
        private Scrollbar scrollbar;
        private int numRows;
        private const int ROW_HEIGHT = 17;

        public ColorButtonList(int x, int y, int width, int numVisibleRows, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            height = numVisibleRows * ROW_HEIGHT;
            numRows = numVisibleRows;
            scrollbar = new Scrollbar(0, 0, width, height, this);

            SetParent(parent);
        }

        /// <summary>
        /// Sets the list for this box to reference
        /// </summary>
        public void SetDictionary(Dictionary<string, Color> colors) {
            entries = [];
            foreach (KeyValuePair<string, Color> pair in colors) {
                ColorButton button = new ColorButton(pair.Value, 0, 0, this);
                button.DrawBorder = false;
                entries.Add(pair.Key, button);
            }
        }

        public void ResetView() {
            scrollbar.ScrollValue = 0;
        }

        /// <summary>
        /// Saves the list in this box to the given dictionary
        /// </summary>
        public void SaveDictionaryInto(Dictionary<string, Color> colors) {
            foreach (KeyValuePair<string, ColorButton> pair in entries) {
                colors[pair.Key] = pair.Value.Color;
            }
        }

        public void Update() {
            if (InFocus) {
                scrollbar.SetSize(entries.Count, numRows);
                scrollbar.UpdateScrollValue();
                scrollbar.Update();
                int rowNum = numRows - 1;
                for (int i = numRows + scrollbar.ScrollValue - 1; i >= scrollbar.ScrollValue; i--) {
                    entries.ElementAt(i).Value.x = 2;
                    entries.ElementAt(i).Value.y = 2 + rowNum * ROW_HEIGHT;
                    entries.ElementAt(i).Value.Update();
                    rowNum--;
                }
            }
        }

        public void Draw() {
            scrollbar.Draw();

            Color bgColor = new Color(43, 49, 81);
            Color bgColor2 = new Color(59, 68, 107);

            int rowNum = numRows - 1;
            for (int i = numRows + scrollbar.ScrollValue - 1; i >= scrollbar.ScrollValue; i--) {
                DrawRect(0, rowNum * ROW_HEIGHT, width - 6, ROW_HEIGHT, i % 2 == 0 ? bgColor2 : bgColor);
                entries.ElementAt(i).Value.Draw();
                Write(entries.ElementAt(i).Key, 68, rowNum * ROW_HEIGHT + 5, Color.White);
                rowNum--;
            }
        }
    }
}
