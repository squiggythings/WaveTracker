using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class PreviewPiano : Clickable {
        public int CurrentClickedNote {
            get {
                return IsPressed && enabled ? MouseX / 4 + 12 : -1;
            }
        }
        public int BaseKeyIndex { get; set; }
        public bool ShowBaseKey { get; set; }
        public PreviewPiano(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            width = 480;
            height = 24;
            SetParent(parent);
        }

        public void Draw() {
            Draw(-1);
        }
        public void Draw(int pressedNote) {
            DrawRect(-1, -1, width + 2, height + 2, UIColors.black);
            // draw 10 octaves of the piano sprite
            for (int i = 0; i < 10; ++i) {
                DrawSprite(i * 48, 0, new Rectangle(0, 80, 48, 24));
            }
            // draw the base key
            if (ShowBaseKey) {
                if (Helpers.IsNoteBlackKey(BaseKeyIndex)) {
                    DrawSprite((BaseKeyIndex - 12) * 4, 0, new Rectangle(60, 80, 4, 24));
                }
                else {
                    DrawSprite((BaseKeyIndex - 12) * 4, 0, new Rectangle(56, 80, 4, 24));
                }
            }
            // draw the pressed note
            if (pressedNote >= 12 && pressedNote < 132)
                if (Helpers.IsNoteBlackKey(pressedNote)) {
                    DrawSprite((pressedNote - 12) * 4, 0, new Rectangle(52, 80, 4, 24));
                }
                else {
                    DrawSprite((pressedNote - 12) * 4, 0, new Rectangle(48, 80, 4, 24));
                }
            }
        }
    }
}
