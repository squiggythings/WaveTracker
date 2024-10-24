using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class SpriteToggle : Clickable {
        private int sourceX;
        private int sourceY;

        public bool Value { get; set; }
        public bool ValueWasChangedInternally { get; private set; }

        public SpriteToggle(int x, int y, int width, int height, int sourceX, int sourceY, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.sourceX = sourceX;
            this.sourceY = sourceY;
            SetParent(parent);
        }

        public void Update() {
            if (enabled) {
                if (Clicked) {
                    Value = !Value;
                    ValueWasChangedInternally = true;
                }
                else {
                    ValueWasChangedInternally = false;
                }
            }

        }

        private Rectangle GetBounds(int num) {
            return new Rectangle(sourceX, sourceY + num * height, width, height);
        }
        public void Draw() {

            if (enabled) {
                if (Value) {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetBounds(2));
                    }
                    else {
                        DrawSprite(0, 0, GetBounds(3));
                    }
                    return;
                }
                if (IsHovered) {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetBounds(2));
                    }
                    else {
                        DrawSprite(0, 0, GetBounds(1));
                    }
                }
                else {
                    DrawSprite(0, 0, GetBounds(0));
                }
            }
            else {
                DrawSprite(0, 0, GetBounds(4));
            }

        }
    }
}
