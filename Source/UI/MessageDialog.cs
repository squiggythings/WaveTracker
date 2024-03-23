using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class MessageDialog : Dialog {
        protected Action<string> OnDialogExit;
        public enum Icon { Information, Error, Warning, Question, None }
        Icon icon;
        public string Message { get; protected set; }
        protected Button[] buttons;
        int textHeight;
        int textWidth;
        public MessageDialog() : base("WaveTracker", 240, 80, hasExitButton: false) {

        }

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="message">The message to display to the user</param>
        /// <param name="icon">The icon to display alongside the message</param>
        /// <param name="buttonNames">A list of buttons to add to close the message</param>
        /// <param name="onExitCallback">Callback where the name of the pressed button is passed in as a parameter</param>
        public void Open(string message, Icon icon, string[] buttonNames, Action<string> onExitCallback) {
            Message = message;
            this.icon = icon;
            ClearBottomButtons();
            buttons = new Button[buttonNames.Length];
            for (int i = buttonNames.Length - 1; i >= 0; --i) {
                buttons[i] = AddNewBottomButton(buttonNames[i], this);
            }
            OnDialogExit = onExitCallback;
            if (icon == Icon.Information)
                System.Media.SystemSounds.Asterisk.Play();
            if (icon == Icon.Error)
                System.Media.SystemSounds.Hand.Play();
            if (icon == Icon.Warning)
                System.Media.SystemSounds.Exclamation.Play();
            if (icon == Icon.Question)
                System.Media.SystemSounds.Asterisk.Play();
            textWidth = width - (icon == Icon.None ? 16 : 64);
            textHeight = Helpers.GetHeightOfMultilineText(Message, textWidth);
            Open();
        }

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="message">The message to display to the user</param>
        /// <param name="icon">The icon to display alongside the message</param>
        /// <param name="buttonNames">A list of buttons to add to close the message</param>
        public void Open(string message, Icon icon, string[] buttonNames) {
            Message = message;
            this.icon = icon;
            ClearBottomButtons();
            buttons = new Button[buttonNames.Length];
            for (int i = buttonNames.Length - 1; i >= 0; --i) {
                buttons[i] = AddNewBottomButton(buttonNames[i], this);
            }
            OnDialogExit = null;
            if (icon == Icon.Information)
                System.Media.SystemSounds.Asterisk.Play();
            if (icon == Icon.Error)
                System.Media.SystemSounds.Hand.Play();
            if (icon == Icon.Warning)
                System.Media.SystemSounds.Exclamation.Play();
            if (icon == Icon.Question)
                System.Media.SystemSounds.Asterisk.Play();
            textWidth = width - (icon == Icon.None ? 16 : 64);
            textHeight = Helpers.GetHeightOfMultilineText(Message, textWidth);
            Open();
        }

        public void Update() {
            if (windowIsOpen) {
                DoDragging();
                foreach (Button b in buttons) {
                    if (b.Clicked) {
                        Close(b.Label);
                    }
                }
            }
        }

        public void Close(string result) {
            Input.CancelClick();
            base.Close();
            if (OnDialogExit != null) {
                OnDialogExit.Invoke(result);
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                // 220
                DrawRect(0, 9, width, height - 28, Color.White);


                int textY = 10 + (height - 25 - textHeight) / 2;
                if (icon == Icon.Information)
                    DrawSprite(8, 19, new Rectangle(256, 80, 32, 32));
                if (icon == Icon.Error)
                    DrawSprite(8, 19, new Rectangle(288, 80, 32, 32));
                if (icon == Icon.Warning)
                    DrawSprite(8, 19, new Rectangle(320, 80, 32, 32));
                if (icon == Icon.Question)
                    DrawSprite(8, 19, new Rectangle(352, 80, 32, 32));
                WriteMultiline(Message, icon == Icon.None ? 8 : 48, textY, textWidth, UIColors.labelDark);
            }
        }

    }
}
