using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class Clickable : Element {
        public int width;
        public int height;
        public bool enabled = true;
        public bool isPartOfInternalDialog = false;
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
                if (!inFocus)
                    return false;
                bool h = MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0;
                if (h) {
                    Tooltip.TooltipTextLong = TooltipTextLong;
                    Tooltip.TooltipText = TooltipText;
                }
                return h;
            }
        }
        public bool IsPressed { get { return IsHovered && Input.GetClick(KeyModifier._Any) && globalPointIsInBounds(Input.lastClickLocation); } }

        public bool Clicked {
            get {
                return enabled && IsHovered && Input.GetClickUp(KeyModifier._Any) && globalPointIsInBounds(Input.lastClickLocation) && globalPointIsInBounds(Input.lastClickReleaseLocation);
            }
        }
        public bool ClickedDown {
            get {
                return enabled && IsHovered && Input.GetClickDown(KeyModifier._Any) && globalPointIsInBounds(Input.lastClickLocation);
            }
        }

        public bool DoubleClicked {
            get {
                return enabled && IsHovered && Input.GetDoubleClick(KeyModifier._Any) && globalPointIsInBounds(Input.lastClickLocation) && globalPointIsInBounds(Input.lastClickReleaseLocation);
            }
        }

        public bool ClickedM(KeyModifier modifier) {
            if (!inFocus)
                return false;
            return enabled && IsHovered && Input.GetClickUp(modifier) && globalPointIsInBounds(Input.lastClickLocation) && globalPointIsInBounds(Input.lastClickReleaseLocation);
        }

        public bool SingleClickedM(KeyModifier modifier) {
            if (!inFocus)
                return false;
            return enabled && IsHovered && Input.GetSingleClickUp(modifier) && globalPointIsInBounds(Input.lastClickLocation) && globalPointIsInBounds(Input.lastClickReleaseLocation);
        }


        public bool DoubleClickedM(KeyModifier modifier) {
            return enabled && IsHovered && Input.GetDoubleClick(modifier);
        }

        public bool globalPointIsInBounds(Point p) {
            return p.X >= this.globalX && p.Y >= this.globalY && p.X < this.globalX + this.width && p.Y < this.globalY + this.height;
        }
    }
}
