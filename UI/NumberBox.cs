using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using WaveTracker.Rendering;

namespace WaveTracker.UI
{
    public class NumberBox : Clickable
    {
        private Forms.EnterText dialog;
        bool dialogOpen;

        public SpriteButton bUp;
        public SpriteButton bDown;
        int boxWidth;
        string label;
        public static Texture2D buttons;
        int min = int.MinValue;
        int max = int.MaxValue;
        int valueSaved;
        bool canScroll = true;
        public enum DisplayMode { Number, Note, PlusMinus }
        public DisplayMode displayMode = DisplayMode.Number;
        public bool ValueWasChanged { get; private set; }
        public bool ValueWasChangedInternally { get; private set; }
        int lastValue;
        int _value;
        public int Value { get { return _value; } set { _value = Math.Clamp(value, min, max); } }

        public NumberBox(string label, int x, int y, int width, int boxWidth, Element parent)
        {
            this.label = label;
            this.x = x;
            this.y = y;
            this.width = width;
            this.boxWidth = boxWidth;
            height = 13;
            canScroll = true;
            SetParent(parent);
            bUp = new SpriteButton(width - 10, 0, 10, 6, buttons, 1, this);
            bDown = new SpriteButton(width - 10, 7, 10, 6, buttons, 2, this);
        }

        public void EnableScrolling() { canScroll = true; }
        public void DisableScrolling() { canScroll = false; }

        public void SetValueLimits(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public void Update()
        {
            if (enabled && inFocus)
            {
                int valueBeforeUpdate = Value;
                if (DoubleClicked && MouseX < width - 10)
                {
                    if (!dialogOpen)
                    {
                        dialogOpen = true;
                        StartDialog();
                    }
                }
                else
                {
                    dialogOpen = false;
                }
                if (LastClickPos.X >= 0 && LastClickPos.Y >= 0)
                {
                    if (LastClickPos.X <= width - 10 && LastClickPos.Y <= height)
                    {
                        if (Input.GetClickDown(KeyModifier.None))
                            valueSaved = Value;
                        if (Input.GetClick(KeyModifier.None))
                        {
                            Value = valueSaved - (MouseY - LastClickPos.Y) / 2;
                            Game1.mouseCursorArrow = 2;
                        }
                    }
                }
                bUp.enabled = Value < max;
                bDown.enabled = Value > min;
                if (IsHovered && canScroll)
                    Value += Input.MouseScrollWheel(KeyModifier.None);

                if (bUp.Clicked)
                    Value++;
                if (bDown.Clicked)
                    Value--;

                if (Value != lastValue)
                {
                    ValueWasChanged = true;
                    lastValue = Value;
                }
                else
                {
                    ValueWasChanged = false;
                }

                ValueWasChangedInternally = Value != valueBeforeUpdate;
            }
        }

        public void Draw()
        {
            Color dark = UIColors.label;
            Color text = UIColors.black;
            Color labelCol = UIColors.labelDark;
            if (IsHovered)
            {
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
            DrawRect(width, boxStartY + 6, -10, 1, ButtonColors.Round.backgroundColor);
            if (displayMode == DisplayMode.Number)
                Write(Value + "", boxStart + 4, height / 2 - 3, text);
            if (displayMode == DisplayMode.Note)
                Write(Value + " (" + Helpers.GetNoteName(Value) + ")", boxStart + 4, height / 2 - 3, text);
            if (displayMode == DisplayMode.PlusMinus)
                Write((Value <= 0 ? Value : "+" + Value) + "", boxStart + 4, height / 2 - 3, text);
            bUp.Draw();
            bDown.Draw();
        }

        public void StartDialog()
        {
            Input.DialogStarted();
            dialog = new Forms.EnterText();
            dialog.textBox.Text = Value + "";
            dialog.label.Text = label;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int a;
                if (int.TryParse(dialog.textBox.Text, out a))
                {
                    Value = a;
                }
            }
        }
    }
}
