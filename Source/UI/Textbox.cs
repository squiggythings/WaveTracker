using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace WaveTracker.UI {
    public class Textbox : Clickable {
        private Forms.EnterText dialog;
        public bool canEdit = true;
        private string label;
        private string textPrefix = "";
        private int textBoxWidth;
        public bool ValueWasChanged { get; private set; }
        public bool ValueWasChangedInternally { get; set; }
        public string Text { get; set; }

        private string lastText;
        public int MaxLength { get; set; }
        public InputField InputField { get; private set; }

        public new bool InFocus => base.InFocus || InputField.InFocus;

        public Textbox(string label, int x, int y, int width, int textBoxWidth, Element parent) {
            this.width = width;
            this.textBoxWidth = textBoxWidth;
            this.x = x;
            this.y = y;
            this.label = label;
            height = 13;
            MaxLength = 32;
            InputField = new InputField(width - textBoxWidth, 0, textBoxWidth, this);
            SetParent(parent);
        }

        public Textbox(string label, int x, int y, int width, Element parent) {
            this.width = width;
            textBoxWidth = label == "" ? width : width - Helpers.GetWidthOfText(label) - 4;
            this.x = x;
            this.y = y;
            this.label = label;
            height = 13;
            MaxLength = 32;
            InputField = new InputField(width - textBoxWidth, 0, textBoxWidth, this);
            SetParent(parent);
        }

        public void SetPrefix(string prefix) {
            textPrefix = prefix;
        }

        public void Update() {
            if (enabled) {
                ValueWasChangedInternally = false;
                if (InputField.Clicked && canEdit) {
                    InputField.Open(Text);
                }
                InputField.Update();
                if (InputField.ValueWasChangedInternally) {
                    Text = InputField.EditedText;
                    ValueWasChangedInternally = true;
                }
                if (Text != lastText) {
                    ValueWasChanged = true;
                    lastText = Text;
                }
                else {
                    ValueWasChanged = false;
                }
            }
        }

        public void Draw() {
            Color borderColor = UIColors.labelDark;
           


            Write(label + "", 0, height / 2 - 3, borderColor);
            //DrawRect(width - textBoxWidth, 0, textBoxWidth, height, borderColor);
            //DrawRect(width - textBoxWidth + 1, 1, textBoxWidth - 2, height - 2, Color.White);
            if (canEdit) {
            //    DrawRect(width - textBoxWidth + 1, 1, textBoxWidth - 2, 1, new Color(193, 196, 213));
            }

            string t = textPrefix + Text + "";
            InputField.Draw(t);
        }

        public void StartDialog() {
            Input.DialogStarted();
            dialog = new Forms.EnterText();
            dialog.textBox.Text = Text;
            dialog.label.Text = label;
            dialog.textBox.MaxLength = MaxLength;
            if (dialog.ShowDialog() == DialogResult.OK) {
                Text = dialog.textBox.Text;
                ValueWasChangedInternally = true;
            }
        }
    }
}
