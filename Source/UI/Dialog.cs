using System.Collections.Generic;

namespace WaveTracker.UI {
    /// <summary>
    /// A window with buttons along the bottom
    /// </summary>
    public abstract class Dialog : Window {
        private List<Button> bottomButtons;

        /// <summary>
        /// Initializes a dialog
        /// </summary>
        /// <param name="name"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public Dialog(string name, int width, int height, bool hasExitButton = true) : base(name, width, height, hasExitButton) {
            this.name = name;
            this.width = width;
            this.height = height;
            bottomButtons = [];
        }

        protected void ClearBottomButtons() {
            bottomButtons.Clear();
        }

        protected Button AddNewBottomButton(string name, Element parent) {

            Button ret = new Button(name, width - 54 * (bottomButtons.Count + 1), height - 16, parent);
            ret.width = 51;
            if (Helpers.GetWidthOfText(name) > ret.width - 6) {
                ret.width = Helpers.GetWidthOfText(name) + 6;
            }
            bottomButtons.Add(ret);
            int x = width;
            foreach (Button button in bottomButtons) {
                x -= button.width + 3;
            }
            ret.x = x;
            return ret;
        }

        public abstract void Update();

        /// <summary>
        /// Draws the window and the buttons at the bottom
        /// </summary>
        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                foreach (Button button in bottomButtons) {
                    button.Draw();
                }
            }
        }
    }
}

