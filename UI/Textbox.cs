using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace WaveTracker.UI
{
    public class Textbox : Clickable
    {
        private Forms.EnterText dialog;
        public bool canEdit = true;
        string label;
        int textboxWidth;
        public bool ValueWasChanged { get; private set; }
        public string Text { get; set; }
        string lastText;
        public int maxLength = 32;
        public Textbox(string label, int x, int y, int width, int textBoxWidth, Element parent)
        {
            this.width = width;
            this.textboxWidth = textBoxWidth;
            this.x = x;
            this.y = y;
            this.label = label;
            this.height = 13;
            SetParent(parent);
        }

        public void Update()
        {
            if (enabled)
            {
                if (Clicked && canEdit)
                {
                    if (Input.dialogOpenCooldown == 0)
                    {
                        StartDialog();
                    }
                }

                if (Text != lastText)
                {
                    ValueWasChanged = true;
                    lastText = Text;
                }
                else
                {
                    ValueWasChanged = false;
                }
            }
        }

        public void Draw()
        {
            Color dark = new Color(104, 111, 153);
            Color text = new Color(20, 24, 46);
            if (IsHovered && canEdit)
            {
                dark = text;
            }
            Write(label + "", 0, height / 2 - 3, dark);
            DrawRect(width - textboxWidth, 0, textboxWidth, height, dark);
            DrawRect(width - textboxWidth + 1, 1, textboxWidth - 2, height - 2, Color.White);
            if (canEdit)
                DrawRect(width - textboxWidth + 1, 1, textboxWidth - 2, 1, new Color(193, 196, 213));
            string t = Text + "";
            
            Write(Helpers.TrimTextToWidth(textboxWidth, t), width - textboxWidth + 4, height / 2 - 3, text);

        }

        public void StartDialog()
        {
            Input.DialogStarted();
            dialog = new Forms.EnterText();
            dialog.textBox.Text = Text;
            dialog.label.Text = label;
            dialog.textBox.MaxLength = maxLength;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Text = Helpers.FlushString(dialog.textBox.Text);
            }
        }
    }
}
