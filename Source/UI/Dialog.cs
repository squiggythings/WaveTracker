using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    /// <summary>
    /// A window that can have buttons at the bottom of the window
    /// </summary>
    public abstract class Dialog : Window {
        List<Button> bottomButtons;


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
            bottomButtons = new List<Button>();
        }

        protected Button AddNewBottomButton(string name, Element parent) {

            Button ret = new Button(name, width - 54 * (bottomButtons.Count + 1), height - 16, parent);
            ret.width = 51;
            bottomButtons.Add(ret);
            return ret;
        }

        /// <summary>
        /// Draws the window and the buttons at the bottom
        /// </summary>
        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                foreach (Button button in bottomButtons) {
                    button.Draw();
                }
            }
        }
    }
}

