using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

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
        public bool ValueWasChanged { get; private set; }
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
            SetParent(parent);
            bUp = new SpriteButton(width - 10, 0, 10, 6, buttons, 1, this);
            bDown = new SpriteButton(width - 10, 7, 10, 6, buttons, 2, this);
        }

        public void SetValueLimits(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public void Update()
        {
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
            bUp.enabled = Value < max;
            bDown.enabled = Value > min;
            if (IsHovered)
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
        }

        public void Draw()
        {

            Color dark = new Color(104, 111, 153);
            Color text = new Color(20, 24, 46);
            Color labelCol = new Color(64, 73, 115);
            if (IsHovered)
            {
                labelCol = Color.Black;
                dark = new Color(64, 73, 115);
            }
            int bWidth = boxWidth - 10;
            int boxStart = width - boxWidth;
            Write(label + "", 0, height / 2 - 3, labelCol);
            DrawRect(boxStart, 0, bWidth, height, dark);
            DrawRect(boxStart + 1, 1, bWidth - 2, height - 2, Color.White);
            DrawRect(boxStart + 1, 1, bWidth - 2, 1, new Color(193, 196, 213));
            DrawRect(width, 6, -10, 1, ButtonColors.Round.backgroundColor);
            Write(Value + "", boxStart + 4, height / 2 - 3, text);
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
