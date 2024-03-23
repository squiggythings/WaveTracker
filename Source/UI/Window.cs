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
        protected bool windowIsOpen;

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
                ExitButton = new SpriteButton(width - 10, 0, 10, 9, Rendering.Graphics.img, 472, 0, this);
        }


        /// <summary>
        /// Positions the window in the center of the screen
        /// </summary>
        public void PositionInCenterOfScreen() {
            x = (App.WindowWidth - width) / 2;
            y = (App.WindowHeight - height) / 2;
        }

        protected void DoDragging() {
            if (windowIsOpen && InFocus) {
                if (Input.GetClickDown(KeyModifier._Any)) {
                    if (ExitButton != null) {
                        if (LastClickPos.X >= 0 && LastClickPos.X < width - ExitButton.width && LastClickPos.Y >= 0 && LastClickPos.Y <= 9) {
                            isDragging = true;
                            dragOffset = new Point(Input.MousePositionX - x, Input.MousePositionY - y);
                        }
                    }
                    else {
                        if (LastClickPos.X >= 0 && LastClickPos.X < width && LastClickPos.Y >= 0 && LastClickPos.Y <= 9) {
                            isDragging = true;
                            dragOffset = new Point(Input.MousePositionX - x, Input.MousePositionY - y);
                        }
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
                ClampPosition();
            }
        }

        /// <summary>
        /// Clamps the position of the window to be within screen bounds
        /// </summary>
        void ClampPosition() {
            if (x > App.WindowWidth - width - 2) {
                x = App.WindowWidth - width - 2;
            }
            if (y > App.WindowHeight - height - 10) {
                y = App.WindowHeight - height - 10;
            }
            if (x < 2)
                x = 2;
            if (y < 2)
                y = 2;
        }

        protected new void Draw() {
            if (windowIsOpen) {
                ClampPosition();

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
            windowIsOpen = true;
            PositionInCenterOfScreen();
        }
        protected void Open(Element opened) {
            Input.focus = this;
            windowIsOpen = true;
            PositionInCenterOfScreen();
            this.opened = opened;
        }
        protected void Close() {
            Input.focus = opened;
            windowIsOpen = false;
        }
    }
}
