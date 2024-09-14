using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace WaveTracker.UI {
    public class InputField : Clickable {

        /// <summary>
        /// The text as it is being edited.
        /// </summary>
        public string EditedText { get; set; }

        /// <summary>
        /// The characters that are allowed to be input. Leave blank to allow any characters
        /// </summary>
        public string AllowedCharacters { get; set; }

        /// <summary>
        /// The maximum amount of characters allowed to be input
        /// </summary>
        public int MaximumLength { get; set; }

        private string lastText;

        /// <summary>
        /// Is this InputField currently being edited
        /// </summary>
        public bool IsBeingEdited { get; private set; }

        /// <summary>
        /// Should ValueWasChangedInternally be flagged every time the edited text changes or only when the input field is closed
        /// </summary>
        public bool UpdateLive { get; set; }

        private int selectionStart;
        private int selectionEnd;
        private int selectionMin;
        private int selectionMax;

        private bool SelectionIsActive { get; set; }

        private int caretPosition;
        private int lastCaretPosition;
        private int mouseCursorCaret;

        public int ScrollPosition { get; set; }

        public bool ValueWasChangedInternally { get; private set; }
        private Element previousFocus;

        /// <summary>
        /// The inputfield that is being currently edited, if any.
        /// </summary>
        private static InputField currentlyEditing;

        public static bool IsAnInputFieldBeingEdited { get { return currentlyEditing != null; } }

        private float caretFlashTimer;
        private static RasterizerState scissorOn;

        public InputField(int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            height = 13;
            scissorOn = new RasterizerState();
            scissorOn.CullMode = CullMode.None;
            scissorOn.ScissorTestEnable = true;
            MaximumLength = 128;
            AllowedCharacters = "";
            SetParent(parent);

        }

        public void Update() {
            ValueWasChangedInternally = false;
            if (IsBeingEdited) {
                caretFlashTimer += (float)App.GameTime.ElapsedGameTime.TotalSeconds;
                mouseCursorCaret = GetMouseCaretPosition();
                int maxScrollPosition = Helpers.GetWidthOfText(EditedText) - width + 8;
                if (Input.GetKeyDown(Keys.Enter, KeyModifier.None) || Input.GetClickDown(KeyModifier.None) && !IsHovered) {
                    Input.CancelKey(Keys.Enter);
                    Close();
                }

                if (Input.GetClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastClickLocation)) {
                    if (ClickedDown) {
                        selectionStart = mouseCursorCaret;
                    }

                    caretPosition = mouseCursorCaret;
                    if (Input.IsShiftPressed()) {
                        if (!SelectionIsActive) {
                            SelectionIsActive = true;
                            selectionStart = lastCaretPosition;
                        }
                        selectionEnd = caretPosition;
                    }
                    else {
                        if (mouseCursorCaret != selectionStart) {
                            selectionEnd = mouseCursorCaret;
                            SelectionIsActive = true;
                        }
                        else {
                            SelectionIsActive = false;
                        }
                    }
                    caretFlashTimer = 0;
                }

                if (Input.GetKeyRepeat(Keys.Left, KeyModifier._Any)) {
                    if (caretPosition > 0) {
                        caretPosition--;
                        caretFlashTimer = 0;

                        if (Input.IsCtrlPressed()) {
                            caretFlashTimer = 0;
                            GotoPreviousWord();

                        }
                    }
                    if (Input.IsShiftPressed()) {
                        if (!SelectionIsActive) {
                            SelectionIsActive = true;
                            selectionStart = lastCaretPosition;
                        }
                        selectionEnd = caretPosition;
                    }
                    else {
                        if (SelectionIsActive) {
                            caretPosition = selectionMin;
                            SelectionIsActive = false;
                        }
                    }
                }
                else if (Input.GetKeyRepeat(Keys.Right, KeyModifier._Any)) {
                    if (caretPosition < EditedText.Length) {

                        caretPosition++;
                        caretFlashTimer = 0;

                        if (Input.IsCtrlPressed()) {
                            caretFlashTimer = 0;
                            GotoNextWord();
                        }
                    }
                    if (Input.IsShiftPressed()) {
                        if (!SelectionIsActive) {
                            SelectionIsActive = true;
                            selectionStart = lastCaretPosition;
                        }
                        selectionEnd = caretPosition;
                    }
                    else {
                        if (SelectionIsActive) {
                            caretPosition = selectionMax;
                            SelectionIsActive = false;
                        }
                    }
                }

                if (lastText != EditedText) {
                    lastText = EditedText;
                    caretFlashTimer = 0;
                    if (UpdateLive) {
                        ValueWasChangedInternally = true;
                    }
                }

                if (lastCaretPosition != caretPosition) {
                    lastCaretPosition = caretPosition;
                    int caretXPosition = 4 + Helpers.GetWidthOfText(EditedText.Substring(0, caretPosition)) - ScrollPosition;
                    while (caretXPosition > width - 6) {
                        ScrollPosition += 20;
                        caretXPosition = 4 + Helpers.GetWidthOfText(EditedText.Substring(0, caretPosition)) - ScrollPosition;
                    }
                    while (caretXPosition < 3) {
                        ScrollPosition -= 20;
                        caretXPosition = 4 + Helpers.GetWidthOfText(EditedText.Substring(0, caretPosition)) - ScrollPosition;
                    }

                }

                ScrollPosition = Math.Clamp(ScrollPosition, 0, Math.Max(0, maxScrollPosition));

                if (SelectionIsActive) {
                    if (selectionStart < selectionEnd) {
                        selectionMin = selectionStart;
                        selectionMax = selectionEnd;
                    }
                    else {
                        selectionMin = selectionEnd;
                        selectionMax = selectionStart;
                    }
                }
            }
        }

        public void Open(string originalText) {
            if (currentlyEditing == null) {
                originalText ??= "";
                EditedText = originalText;
                lastText = EditedText;
                previousFocus = Input.focus;
                currentlyEditing = this;
                Input.focus = this;
                IsBeingEdited = true;
                App.ClientWindow.TextInput += OnInput;
                caretPosition = GetMouseCaretPosition();
                caretFlashTimer = 0;
            }
        }

        public void Close() {
            IsBeingEdited = false;
            currentlyEditing = null;
            App.ClientWindow.TextInput -= OnInput;
            Input.focus = previousFocus;
            previousFocus = null;
            ValueWasChangedInternally = true;
            IsBeingEdited = false;
        }
        private void OnInput(object sender, TextInputEventArgs e) {
            switch (e.Key) {
                case Keys.Back:
                    if (SelectionIsActive) {
                        DeleteSelection();
                    }
                    else if (caretPosition > 0) {
                        int startPosition = caretPosition;
                        caretPosition--;
                        if (Input.IsCtrlPressed()) {
                            GotoPreviousWord();
                        }
                        EditedText = EditedText.Remove(caretPosition, startPosition - caretPosition);
                    }
                    break;
                default:
                    if (e.Character >= 32) {
                        if (SelectionIsActive) {
                            DeleteSelection();
                        }
                        Insert(e.Character + "");
                    }
                    else {
                        switch ((int)e.Character) {
                            case 1: // CTRL-A
                                SelectionIsActive = true;
                                selectionStart = 0;
                                selectionEnd = EditedText.Length;
                                selectionMin = 0;
                                selectionMax = EditedText.Length;
                                break;
                            case 3: // CTRL-C
                                if (SelectionIsActive) {
                                    SystemClipboard.SetText(EditedText.Substring(selectionMin, selectionMax - selectionMin));
                                }
                                break;
                            case 22: // CTRL-V
                                if (SelectionIsActive) {
                                    DeleteSelection();
                                }
                                Insert(SystemClipboard.GetText());
                                break;
                            case 24: // CTRL-X
                                if (SelectionIsActive) {
                                    SystemClipboard.SetText(EditedText.Substring(selectionMin, selectionMax - selectionMin));
                                    DeleteSelection();
                                }
                                break;
                        }
                        Debug.WriteLine("unrecognized character " + (int)e.Character);
                    }
                    break;
            }
        }
        private void GotoPreviousWord() {
            while (caretPosition > 0 && IsWhitespace(EditedText[caretPosition])) {
                caretPosition--;
            }
            while (caretPosition > 0 && !IsWhitespace(EditedText[caretPosition - 1])) {
                caretPosition--;
            }
        }
        private void GotoNextWord() {
            while (caretPosition < EditedText.Length && !IsWhitespace(EditedText[caretPosition])) {
                caretPosition++;
            }
            while (caretPosition < EditedText.Length && IsWhitespace(EditedText[caretPosition])) {
                caretPosition++;
            }
        }

        private void Insert(string text) {
            if (AllowedCharacters != "") {
                text = Helpers.FlushString(text, AllowedCharacters);
            }
            EditedText = EditedText.Insert(caretPosition, text);
            caretPosition += text.Length;
            if (EditedText.Length > MaximumLength) {
                EditedText = EditedText.Substring(0, MaximumLength);
                caretPosition = EditedText.Length;
            }
        }

        private void DeleteSelection() {
            EditedText = EditedText.Remove(selectionMin, selectionMax - selectionMin);
            caretPosition = selectionMin;
            SelectionIsActive = false;
        }

        private static bool IsWhitespace(char c) {
            return " !?-|\\[]".Contains(c);
        }

        private int GetMouseCaretPosition() {
            if (MouseY < 0) {
                return 0;
            }
            if (MouseY > height) {
                return EditedText.Length;
            }
            for (int i = 1; i <= EditedText.Length; i++) {
                int dist1 = Helpers.GetWidthOfText(EditedText.Substring(0, i)) + 4 - ScrollPosition;
                int dist2 = Helpers.GetWidthOfText(EditedText.Substring(0, i - 1)) + 4 - ScrollPosition;
                if (MouseX >= dist2) {
                    if (MouseX < dist1) {
                        if (Math.Abs(MouseX - dist2) < Math.Abs(MouseX - dist1)) {
                            return i - 1;
                        }
                        else {
                            return i;
                        }
                    }
                }
                else {
                    return 0;
                }
            }
            return EditedText.Length;
        }

        public void Draw(string defaultText) {

            Color borderColor = UIColors.labelDark;
            Color textColor = UIColors.black;

            if (IsHovered) {
                borderColor = UIColors.black;
            }
            if (IsBeingEdited) {
                borderColor = UIColors.selection;
            }

            // draw border
            DrawRect(0, 0, width, height, borderColor);

            // draw background
            DrawRect(1, 1, width - 2, height - 2, Color.White);

            // draw shadow
            DrawRect(1, 1, width - 2, 1, new Color(193, 196, 213));

            if (IsBeingEdited) {

                StartRectangleMask(2, 0, width - 4, height);

                int textX = -ScrollPosition;
                for (int i = 0; i < EditedText.Length; i++) {
                    int widthOfCharacter = Helpers.GetWidthOfText(EditedText[i] + "");
                    if (SelectionIsActive && i >= selectionMin && i < selectionMax) {
                        if (i == selectionMax - 1) {
                            DrawRect(4 + textX, 3, widthOfCharacter, height - 6, UIColors.selection);
                        }
                        else {
                            DrawRect(4 + textX, 3, widthOfCharacter + 1, height - 6, UIColors.selection);
                        }
                        Write(EditedText[i] + "", 4 + textX, 3, Color.White);
                    }
                    else {
                        Write(EditedText[i] + "", 4 + textX, 3, textColor);
                    }
                    textX += widthOfCharacter + 1;
                }
                if (caretFlashTimer % 1 < 0.5f) {
                    // draw caret cursor
                    Write("|", 4 + Helpers.GetWidthOfText(EditedText.Substring(0, caretPosition)) - ScrollPosition, 3, textColor);
                }

                EndRectangleMask();
            }
            else {
                if (defaultText.Length > 0) {
                    Write(Helpers.TrimTextToWidth(width, defaultText), 4, 3, textColor);
                }
                else {
                    Write(Helpers.TrimTextToWidth(width, defaultText), 4, 3, textColor);
                }
            }
        }
    }
}
