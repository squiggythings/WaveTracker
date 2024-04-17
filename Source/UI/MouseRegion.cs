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

        public new int MouseX => base.MouseX;
        public new int MouseY => base.MouseY;

        public float MouseXClamped => Math.Clamp(MouseX / (float)(width - 1), 0f, 1f);
        public float MouseYClamped => Math.Clamp(MouseY / (float)(height - 1), 0f, 1f);

        /// <summary>
        /// Returns true if the mouse was clicked in the region and is still held down, even if the mouse is no longer in the region
        /// </summary>
        public bool DidClickInRegion => Input.GetClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && IsInHierarchy(Input.lastClickFocus);

        public bool DidClickInRegionM(KeyModifier keyModifier) => Input.GetClick(keyModifier) && GlobalPointIsInBounds(Input.lastClickLocation) && IsInHierarchy(Input.lastClickFocus);
    }
}
