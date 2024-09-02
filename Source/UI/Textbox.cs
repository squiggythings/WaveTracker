using Microsoft.Xna.Framework;
// using System.Windows.Forms;

namespace WaveTracker.UI {
    public class Textbox : Clickable {
        private Forms.EnterText dialog;
        public bool canEdit = true;
        private string label;
        private string textPrefix = "";
        private int textboxWidth;
        public bool ValueWasChanged { get; private set; }
        public bool ValueWasChangedInternally { get; set; }
        public string Text { get; set; }

        private string lastText;
        public int MaxLength { get; set; }
        public Textbox(string label, int x, int y, int width, int textBoxWidth, Element parent) {
            this.width = width;
            textboxWidth = textBoxWidth;
            this.x = x;
            this.y = y;
            this.label = label;
            height = 13;
            MaxLength = 32;
            SetParent(parent);
        }

        public Textbox(string label, int x, int y, int width, Element parent) {
            this.width = width;
            textboxWidth = label == "" ? width : width - Helpers.GetWidthOfText(label) - 4;
            this.x = x;
            this.y = y;
            this.label = label;
            height = 13;
            MaxLength = 32;
            SetParent(parent);
        }

        public void SetPrefix(string prefix) {
            textPrefix = prefix;
        }

        public void Update() {
            if (enabled) {
                ValueWasChangedInternally = false;
                if (Clicked && canEdit) {
                    if (Input.dialogOpenCooldown == 0) {
                        StartDialog();
                    }
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
            Color dark = UIColors.labelDark;
            Color text = UIColors.black;
            if (IsHovered && canEdit && enabled) {
                dark = text;
            }
            Write(label + "", 0, height / 2 - 3, dark);
            DrawRect(width - textboxWidth, 0, textboxWidth, height, dark);
            DrawRect(width - textboxWidth + 1, 1, textboxWidth - 2, height - 2, Color.White);
            if (canEdit) {
                DrawRect(width - textboxWidth + 1, 1, textboxWidth - 2, 1, new Color(193, 196, 213));
            }

            string t = textPrefix + Text + "";
            if (t.Length > 0) {
                Write(Helpers.TrimTextToWidth(textboxWidth, t), width - textboxWidth + 4, height / 2 - 3, text);
            }
            else {
                Write(Helpers.TrimTextToWidth(textboxWidth, t), width - textboxWidth + 4, height / 2 - 3, text);
            }
        }

        public void StartDialog() {
            Input.DialogStarted();
            // dialog = new Forms.EnterText();
            // dialog.textBox.Text = Text;
            // dialog.label.Text = label;
            // dialog.textBox.MaxLength = MaxLength;
            // if (dialog.ShowDialog() == DialogResult.OK) {
            //     Text = Helpers.FlushString(dialog.textBox.Text);
            //     ValueWasChangedInternally = true;
            // }
        }
    }
}
