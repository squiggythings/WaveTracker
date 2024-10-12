using Microsoft.Xna.Framework;
using System;
using static WaveTracker.UI.NumberBox;

namespace WaveTracker.UI {
    public class NumberBoxDecimal : Clickable {
        public SpriteButton bUp;
        public SpriteButton bDown;
        private int boxWidth;
        private string label;
        private float min = float.MinValue;
        private float max = float.MaxValue;
        private bool canScroll = true;
        public enum NumberDisplayMode { Number, PlusMinus, Percent, Milliseconds }
        public NumberDisplayMode DisplayMode = NumberDisplayMode.Number;
        public bool ValueWasChanged { get; private set; }
        public bool ValueWasChangedInternally { get; private set; }

        private int lastMouseY;
        private float lastValue;
        private float _value;

        private int DecimalPlaces { get; set; }
        private float Step { get; set; }
        public float Value { get { return _value; } set { _value = Math.Clamp(value, min, max); } }

        private InputField InputField { get; set; }
        public new bool InFocus => base.InFocus || InputField.InFocus;

        public NumberBoxDecimal(string label, int x, int y, int width, int boxWidth, Element parent, float step = 0.01f, int decimalPlaces = 2) {
            this.label = label;
            this.x = x;
            this.y = y;
            this.width = width;
            this.boxWidth = boxWidth;
            Step = step;
            DecimalPlaces = decimalPlaces;
            height = 13;
            canScroll = true;
            SetParent(parent);

            InputField = new InputField(width - boxWidth, 0, boxWidth - 10, this);
            InputField.AllowedCharacters = "-0123456789.";

            bUp = new SpriteButton(width - 10, 0, 10, 6, 416, 144, this);
            bDown = new SpriteButton(width - 10, 7, 10, 6, 416, 176, this);
        }

        public NumberBoxDecimal(string label, int x, int y, Element parent, float step = 0.01f, int decimalPlaces = 2) {
            this.label = label;
            this.x = x;
            this.y = y;
            width = Helpers.GetWidthOfText(label) + 46;
            boxWidth = 38;
            Step = step;
            DecimalPlaces = decimalPlaces;
            height = 13;
            canScroll = true;
            SetParent(parent);

            InputField = new InputField(width - boxWidth, 0, boxWidth - 10, this);
            InputField.AllowedCharacters = "-0123456789.";

            bUp = new SpriteButton(width - 10, 0, 10, 6, 416, 144, this);
            bDown = new SpriteButton(width - 10, 7, 10, 6, 416, 176, this);
        }

        public void EnableScrolling() { canScroll = true; }
        public void DisableScrolling() { canScroll = false; }

        public void SetValueLimits(int min, int max) {
            this.min = min;
            this.max = max;
        }

        public void Update() {
            if (enabled && InFocus) {
                float valueBeforeUpdate = _value;

                //Input field text editing
                if (InputField.DoubleClicked) {
                    Input.CancelClick();
                    InputField.Open(Value + "", selectAll: true);
                }
                InputField.Update();
                if (InputField.ValueWasChangedInternally) {
                    if (float.TryParse(InputField.EditedText, out float num)) {
                        Value = num;
                    }
                }

                //Dragging
                if (LastClickPos.X >= 0 && LastClickPos.Y >= 0) {
                    if (LastClickPos.X <= width - 10 && LastClickPos.Y <= height) {
                        if (Input.GetClick(KeyModifier.None)) {
                            Value -= (MouseY - lastMouseY) * Step;
                            App.MouseCursorArrow = 2;
                        }

                        if (Input.GetClick(KeyModifier.Shift)) {
                            Value -= (MouseY - lastMouseY) * Step * 4;
                            App.MouseCursorArrow = 2;
                        }
                    }
                }

                //Scrolling
                if (IsHovered && canScroll) {
                    Value += Input.MouseScrollWheel(KeyModifier.None);
                }

                bUp.enabled = Value < max;
                bDown.enabled = Value > min;
                if (bUp.Clicked) {
                    Value += Step;
                }
                if (bDown.Clicked) {
                    Value -= Step;
                }

                if (_value != lastValue) {
                    ValueWasChanged = true;
                    lastValue = _value;
                }
                else {
                    ValueWasChanged = false;
                }

                ValueWasChangedInternally = _value != valueBeforeUpdate;
                lastMouseY = MouseY;
            }
        }

        public void Draw() {
            Color dark = UIColors.label;
            Color labelCol = UIColors.labelDark;
            if (IsHovered) {
                labelCol = Color.Black;
                dark = UIColors.label;
            }
            int bWidth = boxWidth - 10;
            int boxStart = width - boxWidth;
            int boxHeight = 13;
            int boxStartY = (height - boxHeight) / 2;
            Write(label + "", 0, height / 2 - 3, labelCol);
            DrawRect(boxStart, boxStartY, bWidth, boxHeight, dark);
            DrawRect(boxStart + 1, boxStartY + 1, bWidth - 2, boxHeight - 2, Color.White);
            DrawRect(boxStart + 1, boxStartY + 1, bWidth - 2, 1, new Color(193, 196, 213));
            DrawRect(width, boxStartY + 6, -10, 1, ButtonColors.backgroundColor);

            string text = "";
            switch (DisplayMode) {
                case NumberDisplayMode.Number:
                    text = MathF.Round(Value, DecimalPlaces) + "";
                    break;
                case NumberDisplayMode.PlusMinus:
                    text = (Value <= 0 ? Value : "+" + Value) + "";
                    break;
                case NumberDisplayMode.Percent:
                    text = Value + "%";
                    break;
                case NumberDisplayMode.Milliseconds:
                    text = Value + "ms";
                    break;
            }
            InputField.Draw(text);

            bUp.Draw();
            bDown.Draw();
        }
    }
}
