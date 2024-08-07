using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class SwitchToggleCustom : Clickable {
        /// <summary>
        /// The color of this toggle when it is turned on.
        /// </summary>
        public Color ToggledColor { get; set; }
        public bool Value { get; set; }
        public bool ValueWasChangedInternally { get; set; }

        private string label;

        public SwitchToggleCustom(int x, int y, string label) {
            width = 16;
            height = 7;
            this.label = label;
            ToggledColor = UIColors.selection;
        }
        public SwitchToggleCustom(int x, int y) {
            width = 16;
            height = 7;
            label = null;
            ToggledColor = UIColors.selection;
        }

        public void Update() {
            if (enabled) {
                ValueWasChangedInternally = false;
                if (Clicked) {
                    Value = !Value;
                    ValueWasChangedInternally = true;
                }
            }
        }

        public void Draw() {
            if (enabled) {
                DrawRoundedRect(0, 0, width, height, Value ? ToggledColor : UIColors.label);
                if (IsPressed) {
                    DrawRoundedRect(Value ? 7 : 2, 1, 7, 5, Color.White);
                }
                else {
                    DrawRoundedRect(Value ? 8 : 1, 1, 7, 5, Color.White);
                }
                Write(label, width + 8, 0, UIColors.label);
            }
            else {
                DrawRoundedRect(0, 0, width, height, UIColors.labelLight);
                DrawRoundedRect(Value ? 8 : 1, 1, 7, 5, Color.White);
                Write(label, width + 8, 0, UIColors.labelLight);
            }
        }
    }
}
