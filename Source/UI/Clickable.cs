using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class Clickable : Element {
        public int width;
        public int height;
        public bool enabled = true;
        public string TooltipText {
            get; private set;
        }
        public string TooltipTextLong { get; private set; }

        public void SetTooltip(string ttshort, string ttlong) {
            TooltipText = ttshort;
            TooltipTextLong = ttlong;
        }
        public void SetTooltip(string ttshort) {
            TooltipText = ttshort;
            TooltipTextLong = ttshort;
        }

        public bool IsHovered {
            get {
                if (!InFocus)
                    return false;
                bool h = MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0;
                if (h) {
                    Tooltip.TooltipTextLong = TooltipTextLong;
                    Tooltip.TooltipText = TooltipText;
                }
                return h;
            }
        }
        public bool IsPressed { get { return IsHovered && Input.GetClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation); } }

        public bool Clicked {
            get {
                return enabled && IsHovered && Input.GetClickUp(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation);
            }
        }
        public bool ClickedDown {
            get {
                return enabled && IsHovered && Input.GetClickDown(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation);
            }
        }

        public bool DoubleClicked {
            get {
                return enabled && IsHovered && Input.GetDoubleClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation);
            }
        }

        public bool ClickedM(KeyModifier modifier) {
            if (!InFocus)
                return false;
            return enabled && IsHovered && Input.GetClickUp(modifier) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation);
        }

        public bool SingleClickedM(KeyModifier modifier) {
            if (!InFocus)
                return false;
            return enabled && IsHovered && Input.GetSingleClickUp(modifier) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation);
        }


        public bool IsPressedM(KeyModifier modifier) {
            return IsHovered && Input.GetClick(modifier) && GlobalPointIsInBounds(Input.lastClickLocation);
        }


        public bool DoubleClickedM(KeyModifier modifier) {
            return enabled && IsHovered && Input.GetDoubleClick(modifier);
        }

        public bool GlobalPointIsInBounds(Point p) {
            return p.X >= this.GlobalX && p.Y >= this.GlobalY && p.X < this.GlobalX + this.width && p.Y < this.GlobalY + this.height;
        }
    }
}
