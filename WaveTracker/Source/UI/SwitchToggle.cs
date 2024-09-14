using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class SwitchToggle : Clickable {
        public bool HasContrastOutline { get; set; }
        public bool Value { get; set; }
        public bool ValueWasChangedInternally { get; private set; }
        public SwitchToggle(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            width = 12;
            height = 9;
            SetParent(parent);
        }

        private Rectangle GetSpriteBounds(int index) {
            return new Rectangle(HasContrastOutline ? 448 : 432, 144 + index * 9, 12, 9);
        }

        public void Update() {
            if (Clicked) {
                ValueWasChangedInternally = true;
                Value = !Value;
            }
            else {
                ValueWasChangedInternally = false;
            }
        }

        public void Draw() {
            if (enabled) {
                if (Value) {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetSpriteBounds(5));
                    }
                    else if (IsHovered) {
                        DrawSprite(0, 0, GetSpriteBounds(4));
                    }
                    else {
                        DrawSprite(0, 0, GetSpriteBounds(3));
                    }
                }
                else {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetSpriteBounds(2));
                    }
                    else if (IsHovered) {
                        DrawSprite(0, 0, GetSpriteBounds(1));
                    }
                    else {
                        DrawSprite(0, 0, GetSpriteBounds(0));
                    }
                }
            }
            else {
                if (Value) {
                    DrawSprite(0, 0, GetSpriteBounds(6));
                }
                else {
                    DrawSprite(0, 0, GetSpriteBounds(7));
                }
            }
        }
    }
}
