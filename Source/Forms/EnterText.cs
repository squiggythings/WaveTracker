using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text;
using WaveTracker.UI;

namespace WaveTracker.Forms {
    public class EnterText : Dialog {
        public string result;

        protected Action<string> OnDialogExit;

        public string Message { get; protected set; }

        protected Button cancelButton;
        protected Button renameButton;

        private int textHeight;
        private int textWidth;

        String charBuffer = "";

        private bool pressedEnter = false;

        public EnterText(string name) : base(name, 240, 80, hasExitButton: false) { }

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="message">The message to display to the user</param>
        /// <param name="icon">The icon to display alongside the message</param>
        /// <param name="buttonNames">A list of buttons to add to close the message</param>
        public void Open(string message, string defaultText) {
            Open(message, defaultText, null);
        }

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="message">The message to display to the user</param>
        /// <param name="icon">The icon to display alongside the message</param>
        /// <param name="buttonNames">A list of buttons to add to close the message</param>
        /// <param name="onExitCallback">Callback where the name of the pressed button is passed in as a parameter</param>
        public void Open(string message, string defaultText, Action<string> onExitCallback) {
            Message = message;
            charBuffer = defaultText;

            ClearBottomButtons();
            cancelButton = AddNewBottomButton("Cancel", this);
            renameButton = AddNewBottomButton("Rename", this);

            OnDialogExit = onExitCallback;

            textWidth = width - 16;
            textHeight = Helpers.GetHeightOfMultilineText(Message, textWidth);

            App.ClientWindow.TextInput += OnInput;
            Open();
        }

        public override void Update() {
            if (WindowIsOpen) {
                DoDragging();

                if (cancelButton.Clicked) {
                    charBuffer = "";
                    Close(null);
                }

                if (renameButton.Clicked || pressedEnter) {
                    pressedEnter = false;
                    string result = charBuffer;
                    charBuffer = "";
                    Close(result);
                }
            }
        }

        public void Close(string result) {
            Input.CancelClick();
            base.Close();
            App.ClientWindow.TextInput -= OnInput;
            OnDialogExit?.Invoke(result);
        }

        private void OnInput(object sender, TextInputEventArgs e) {
            var k = e.Key;
            var c = e.Character;

            switch (k) {
                case Keys.Back: {
                    if (charBuffer.Length > 0)
                        charBuffer = charBuffer.Remove(charBuffer.Length - 1, 1);
                    break;
                }

                case Keys.Enter: {
                    pressedEnter = true;
                    break;
                }

                default: {
                    charBuffer = Helpers.FlushString(charBuffer + c);
                    break;
                }
            }

            Console.WriteLine(charBuffer);
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();

                // 220
                DrawRect(0, 9, width, height - 28, Color.White);

                int textY = 10 + (height - 25 - textHeight) / 2;

                WriteMultiline(Message, 8, textY, textWidth, UIColors.labelDark);

                if (charBuffer != null)
                    WriteMultiline(charBuffer, 8, textY + textHeight, textWidth, UIColors.labelDark);
            }
        }
    }
}
