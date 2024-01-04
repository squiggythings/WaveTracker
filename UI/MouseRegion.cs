using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
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

        public bool DidClickInRegion => Input.GetClick(KeyModifier._Any) && globalPointIsInBounds(Input.lastClickLocation);
    }
}
