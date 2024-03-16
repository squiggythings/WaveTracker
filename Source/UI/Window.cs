using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.UI {
    /// <summary>
    /// A panel with an exit button that can be opened and closed, locking focus to itself when it's open
    /// </summary>
    public abstract class Window : Panel {
        /// <summary>
        /// The red X button in the corner of the window
        /// </summary>
        protected SpriteButton ExitButton { get; private set; }
        /// <summary>
        /// The element that opened this window
        /// </summary>
        protected Element opened;
        /// <summary>
        /// Whether the window is visible or not
        /// </summary>
        protected bool windowIsEnabled;

        bool isDragging;
        Point dragOffset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hasExitButton"></param>
        public Window(string name, int width, int height, bool hasExitButton = true) : base(name, (App.WindowWidth - width) / 2, (App.WindowHeight - height) / 2, width, height) {
            if (hasExitButton)
                ExitButton = new SpriteButton(width - 10, 0, 10, 9, UI.NumberBox.buttons, 4, this);
        }


        public void InitializeInCenterOfScreen() {
            x = (App.WindowWidth - width) / 2;
            y = (App.WindowHeight - height) / 2;
        }

        protected void DoDragging() {
            if (windowIsEnabled) {
                if (Input.GetClickDown(KeyModifier._Any)) {
                    if (LastClickPos.X >= 0 && LastClickPos.X <= width && LastClickPos.Y >= 0 && LastClickPos.Y <= 9) {
                        isDragging = true;
                        dragOffset = new Point(Input.MousePositionX - x, Input.MousePositionY - y);
                    }
                }
                if (Input.GetClick(KeyModifier._Any)) {
                    if (isDragging) {
                        x = Input.MousePositionX - dragOffset.X;
                        y = Input.MousePositionY - dragOffset.Y;
                    }
                }
                if (Input.GetClickUp(KeyModifier._Any)) {
                    if (isDragging) {
                        isDragging = false;
                    }
                }
            }
        }

        protected new void Draw() {
            if (windowIsEnabled) {
                x = Math.Clamp(x, 0, App.WindowWidth - width);
                y = Math.Clamp(y, 0, App.WindowHeight - height);

                // draw a transparent black rectangle behind the window
                DrawRect(-x, -y, App.WindowWidth, App.WindowHeight, Helpers.Alpha(Color.Black, 90));
                
                // draw the panel
                base.Draw();
                // draw the exit button if there is one
                if (ExitButton != null)
                    ExitButton.Draw();
            }
        }
        protected void Open() {
            opened = Input.focus;
            Input.focus = this;
            windowIsEnabled = true;
            InitializeInCenterOfScreen();
        }
        protected void Open(Element opened) {
            Input.focus = this;
            windowIsEnabled = true;
            InitializeInCenterOfScreen();
            this.opened = opened;
        }
        protected void Close() {
            Input.focus = opened;
            windowIsEnabled = false;
        }
    }
}
