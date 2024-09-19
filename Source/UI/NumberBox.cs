using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class NumberBox : Clickable {

        public SpriteButton bUp;
        public SpriteButton bDown;
        private int boxWidth;
        private string label;
        private int min = int.MinValue;
        private int max = int.MaxValue;
        private int valueSaved;
        private bool canScroll = true;
        public enum NumberDisplayMode { Number, Note, NoteOnly, PlusMinus, Percent, Milliseconds }
        public NumberDisplayMode DisplayMode { get; set; }
        public bool ValueWasChanged { get; private set; }
        public bool ValueWasChangedInternally { get; private set; }

        private int lastValue;
        private int _value;
        public int Value { get { return _value; } set { _value = Math.Clamp(value, min, max); } }

        private InputField InputField { get; set; }
        public new bool InFocus => base.InFocus || InputField.InFocus;

        public NumberBox(string label, int x, int y, int width, int boxWidth, Element parent) {
            this.label = label;
            DisplayMode = NumberDisplayMode.Number;
            this.x = x;
            this.y = y;
            this.width = width;
            this.boxWidth = boxWidth;
            height = 13;
            canScroll = true;
            SetParent(parent);
            InputField = new InputField(width - boxWidth, 0, boxWidth - 10, this);
            InputField.AllowedCharacters = "-0123456789";
            bUp = new SpriteButton(width - 10, 0, 10, 6, 416, 144, this);
            bDown = new SpriteButton(width - 10, 7, 10, 6, 416, 176, this);
        }

        public NumberBox(string label, int x, int y, Element parent) {
            this.label = label;
            this.x = x;
            this.y = y;
            width = Helpers.GetWidthOfText(label) + 46;
            DisplayMode = NumberDisplayMode.Number;
            boxWidth = 38;
            height = 13;
            canScroll = true;
            SetParent(parent);
            InputField = new InputField(width - boxWidth, 0, boxWidth - 10, this);
            InputField.AllowedCharacters = "-0123456789";
            bUp = new SpriteButton(width - 10, 0, 10, 6, 416, 144, this);
            bDown = new SpriteButton(width - 10, 7, 10, 6, 416, 176, this);
        }

        public void EnableScrolling() { canScroll = true; }
        public void DisableScrolling() { canScroll = false; }

        public void SetValueLimits(int min, int max) {
            this.min = min;
            this.max = max;
            ArgumentOutOfRangeException.ThrowIfLessThan(max, min);
            if (Value < min) {
                Value = min;
            }

            if (Value > max) {
                Value = max;
            }
        }

        public void Update() {
            bUp.enabled = enabled && Value < max;
            bDown.enabled = enabled && Value > min;
            if (enabled && InFocus) {
                int valueBeforeUpdate = Value;
                if (InputField.DoubleClicked) {
                    Input.CancelClick();
                    InputField.Open(Value + "", selectAll: true);
                }
                InputField.Update();
                if (InputField.ValueWasChangedInternally) {
                    if (int.TryParse(InputField.EditedText, out int num)) {
                        Value = num;
                    }
                }
                if (IsHovered && canScroll) {
                    Value += Input.MouseScrollWheel(KeyModifier.None);
                }

                if (IsMeOrAParent(Input.lastClickFocus)) {
                    if (LastClickPos.X >= 0 && LastClickPos.Y >= 0) {
                        if (LastClickPos.X <= width - 10 && LastClickPos.Y <= height) {
                            if (Input.GetClickDown(KeyModifier.None)) {
                                valueSaved = Value;
                            }

                            if (Input.GetClick(KeyModifier.None)) {
                                Value = valueSaved - (MouseY - LastClickPos.Y) / 2;
                                App.MouseCursorArrow = 2;
                            }
                        }
                    }
                }

                if (bUp.Clicked) {
                    Value++;
                }

                if (bDown.Clicked) {
                    Value--;
                }
                if (Value != lastValue) {
                    ValueWasChanged = true;
                    lastValue = Value;
                }
                else {
                    ValueWasChanged = false;
                }
                if (valueBeforeUpdate != Value) {
                    ValueWasChangedInternally = true;
                }
            }
        }

        public void Draw() {
            Color labelColor = UIColors.labelDark;
            if (IsHovered && enabled) {
                labelColor = UIColors.black;
            }

            int boxHeight = 13;
            int boxStartY = (height - boxHeight) / 2;
            Write(label + "", 0, height / 2 - 3, labelColor);

            // draw little strip in between the up down buttons to fill the gap
            DrawRect(width, boxStartY + 6, -10, 1, ButtonColors.backgroundColor);

            string text = "";
            switch (DisplayMode) {
                case NumberDisplayMode.Number:
                    text = Value + "";
                    break;
                case NumberDisplayMode.Note:
                    text = Value + " (" + Helpers.MIDINoteToText(Value) + ")";
                    break;
                case NumberDisplayMode.NoteOnly:
                    text = Helpers.MIDINoteToText(Value);
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
