using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class NumberBox : Clickable {
        private bool dialogOpen;

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
                if (DoubleClicked && MouseX < width - 10) {
                    if (!dialogOpen) {
                        dialogOpen = true;
                        StartDialog();
                    }
                }
                else {
                    dialogOpen = false;
                }
                if (IsInHierarchy(Input.lastClickFocus)) {
                    if (LastClickPos.X >= 0 && LastClickPos.Y >= 0) {
                        if (LastClickPos.X <= width - 10 && LastClickPos.Y <= height) {
                            if (Input.GetClickDown(KeyModifier.None)) {
                                valueSaved = Value;
                            }

                            if (Input.GetClick(KeyModifier.None) && !ValueWasChangedInternally) {
                                Value = valueSaved - (MouseY - LastClickPos.Y) / 2;
                                App.MouseCursorArrow = 2;
                            }
                        }
                    }
                }
                if (IsHovered && canScroll) {
                    Value += Input.MouseScrollWheel(KeyModifier.None);
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

                ValueWasChangedInternally = Value != valueBeforeUpdate;
            }
        }

        public void Draw() {
            Color dark = UIColors.label;
            Color text = UIColors.black;
            Color labelCol = UIColors.labelDark;
            if (IsHovered && enabled) {
                labelCol = UIColors.black;
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
            DrawRect(width, boxStartY + 6, -10, 1, ButtonColors.Round.backgroundColor);
            switch (DisplayMode) {
                case NumberDisplayMode.Number:
                    Write(Value + "", boxStart + 4, height / 2 - 3, text);
                    break;
                case NumberDisplayMode.Note:
                    Write(Value + " (" + Helpers.MIDINoteToText(Value) + ")", boxStart + 4, height / 2 - 3, text);
                    break;
                case NumberDisplayMode.NoteOnly:
                    Write(Helpers.MIDINoteToText(Value), boxStart + 4, height / 2 - 3, text);
                    break;
                case NumberDisplayMode.PlusMinus:
                    Write((Value <= 0 ? Value : "+" + Value) + "", boxStart + 4, height / 2 - 3, text);
                    break;
                case NumberDisplayMode.Percent:
                    Write(Value + "%", boxStart + 4, height / 2 - 3, text);
                    break;
                case NumberDisplayMode.Milliseconds:
                    Write(Value + "ms", boxStart + 4, height / 2 - 3, text);
                    break;
            }

            bUp.Draw();
            bDown.Draw();
        }

        public void StartDialog() {
            Input.DialogStarted();

            Console.WriteLine("starting number box dialog");

            Dialogs.enterTextDialog.Open(
                label,
                Value + "",
                dialogCallback
            );
        }

        private void dialogCallback(string input) {
            Console.WriteLine("ending number box dialog with " + input);
            if (input != null) {
                if (int.TryParse(input, out int a)) {
                    Console.WriteLine("prev Value = " + Value);
                    Value = a;
                    ValueWasChangedInternally = true;
                    Console.WriteLine("Value = " + Value);
                }
            }
        }
    }
}
