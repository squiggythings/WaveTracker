using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class CheckboxLabeled : Clickable {
        public bool Value { get; set; }

        private string label;
        public bool ShowCheckboxOnRight { get; set; }

        public CheckboxLabeled(string label, int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.label = label;
            height = 11;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                Value = !Value;
            }
        }

        private Rectangle GetBounds(int num) {
            return new Rectangle(400, 144 + num * 9, 9, 9);
        }

        public void Draw() {
            int checkX = ShowCheckboxOnRight ? width - 9 : 0;
            if (enabled) {
                if (Value) {
                    if (IsHovered) {
                        if (IsPressed) {
                            DrawSprite(checkX, (height - 9) / 2, GetBounds(5));
                        }
                        else {
                            DrawSprite(checkX, (height - 9) / 2, GetBounds(4));
                        }
                    }
                    else {
                        DrawSprite(checkX, (height - 9) / 2, GetBounds(3));
                    }
                }
                else {
                    if (IsHovered) {
                        if (IsPressed) {
                            DrawSprite(checkX, (height - 9) / 2, GetBounds(2));
                        }
                        else {
                            DrawSprite(checkX, (height - 9) / 2, GetBounds(1));
                        }
                    }
                    else {
                        DrawSprite(checkX, (height - 9) / 2, GetBounds(0));
                    }
                }

            }
            else {
                if (Value) {
                    DrawSprite(checkX, (height - 9) / 2, GetBounds(7));
                }
                else {
                    DrawSprite(checkX, (height - 9) / 2, GetBounds(6));
                }
            }
            Color labelCol = UIColors.labelDark;
            if (IsHovered) {
                labelCol = Color.Black;
            }

            Write(label + "", ShowCheckboxOnRight ? 0 : 13, (height - 7) / 2, labelCol);
        }

    }
}
