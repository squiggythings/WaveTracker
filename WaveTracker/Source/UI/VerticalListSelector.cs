using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class VerticalListSelector : Clickable {
        public int SelectedItemIndex { get; set; }
        public bool ValueWasChangedInternally { get; private set; }
        public string SelectedItem { get { return listItems[SelectedItemIndex]; } }

        private int hoveredItemIndex;
        private string[] listItems;
        public VerticalListSelector(int x, int y, int width, int height, string[] listItems, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.listItems = listItems;
            SetParent(parent);
            SelectedItemIndex = 0;
        }

        public void Update() {
            hoveredItemIndex = -1;
            ValueWasChangedInternally = false;
            for (int i = 0; i < listItems.Length; i++) {
                if (MouseX > 0 && MouseX < width) {
                    if (MouseY >= i * 11 && MouseY < (i + 1) * 11) {
                        hoveredItemIndex = i;
                        if (GlobalPointIsInBounds(Input.LastClickLocation) && IsPressed) {
                            if (SelectedItemIndex != i) {
                                ValueWasChangedInternally = true;
                            }

                            SelectedItemIndex = i;
                        }
                    }
                }
            }
        }

        public void Draw() {
            DrawRect(0, 0, width, height, Color.White);
            for (int i = 0; i < listItems.Length; i++) {
                if (i == SelectedItemIndex) {
                    DrawRect(0, i * 11, width, 11, UIColors.selection);
                    Write(listItems[i], 4, i * 11 + 2, Color.White);
                }
                else {
                    if (i == hoveredItemIndex) {
                        DrawRect(0, i * 11, width, 11, new Color(219, 237, 255));
                    }
                    Write(listItems[i], 4, i * 11 + 2, UIColors.labelDark);
                }

            }
            DrawRect(0, 0, 1, 1, UIColors.panel);
            DrawRect(width - 1, 0, 1, 1, UIColors.panel);
            DrawRect(0, height - 1, 1, 1, UIColors.panel);
            DrawRect(width - 1, height - 1, 1, 1, UIColors.panel);
        }
    }
}
