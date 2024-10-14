using System;

namespace WaveTracker.UI {
    /// <summary>
    /// An invisible region that can detect click events
    /// </summary>
    public class MouseRegion : Clickable {
        public MouseRegion(int x, int y, int width, int height, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            SetParent(parent);
        }

        public new int MouseX {
            get {
                return base.MouseX;
            }
        }

        public float MouseXPercentage {
            get {
                return MouseX / (float)(width - 1);
            }
        }

        public new int MouseY {
            get {
                return base.MouseY;
            }
        }
        public float MouseYPercentage {
            get {
                return MouseY / (float)(height - 1);
            }
        }

        public float MouseXPercentageClamped {
            get {
                return Math.Clamp(MouseX / (float)(width - 1), 0f, 1f);
            }
        }

        public float MouseYPercentageClamped {
            get {
                return Math.Clamp(MouseY / (float)(height - 1), 0f, 1f);
            }
        }

        /// <summary>
        /// Returns true if the mouse was clicked in the region and is still held down, even if the mouse is no longer in the region
        /// </summary>
        public bool DidClickInRegion {
            get {
                return Input.GetClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastClickLocation) && IsMeOrAParent(Input.lastClickFocus);
            }
        }

        public bool ClickedDownM(KeyModifier keyModifier) {
            return Input.GetClickDown(keyModifier) && GlobalPointIsInBounds(Input.LastClickLocation) && IsMeOrAParent(Input.lastClickFocus);
        }

        public bool DidClickInRegionM(KeyModifier keyModifier) {
            return Input.GetClick(keyModifier) && GlobalPointIsInBounds(Input.LastClickLocation) && IsMeOrAParent(Input.lastClickFocus);
        }
    }
}
