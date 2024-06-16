using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public abstract class Clickable : Element {
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
                if (MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0) {
                    if (TooltipTextLong != "")
                        Tooltip.TooltipTextLong = TooltipTextLong;
                    if (TooltipText != "")
                        Tooltip.TooltipText = TooltipText;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if this element is hovered, regardless of if it is in focus or not
        /// </summary>
        public bool IsMouseOverRegion {
            get {
                return MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0;
            }
        }
        public bool IsPressed { get { return IsHovered && Input.GetClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && IsInHierarchy(Input.lastClickFocus); } }

        public bool Clicked {
            get {
                return enabled && IsHovered && Input.GetClickUp(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation) && IsInHierarchy(Input.lastClickFocus);
            }
        }

        public bool RightClicked {
            get {
                return enabled && IsHovered && Input.GetRightClickUp(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastRightClickLocation) && GlobalPointIsInBounds(Input.lastRightClickReleaseLocation);
            }
        }

        public bool RightClickedDown {
            get {
                return enabled && IsHovered && Input.GetRightClickDown(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastRightClickLocation);
            }
        }

        public bool ClickedDown {
            get {
                return enabled && IsHovered && Input.GetClickDown(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && IsInHierarchy(Input.lastClickFocus);
            }
        }

        public bool DoubleClicked {
            get {
                return enabled && IsHovered && Input.GetDoubleClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation) && IsInHierarchy(Input.lastClickFocus);
            }
        }

        public bool ClickedM(KeyModifier modifier) {
            if (!InFocus)
                return false;
            return enabled && IsHovered && Input.GetClickUp(modifier) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation) && IsInHierarchy(Input.lastClickFocus);
        }

        public bool SingleClickedM(KeyModifier modifier) {
            if (!InFocus)
                return false;
            return enabled && IsHovered && Input.GetSingleClickUp(modifier) && GlobalPointIsInBounds(Input.lastClickLocation) && GlobalPointIsInBounds(Input.lastClickReleaseLocation) && IsInHierarchy(Input.lastClickFocus); ;
        }


        public bool IsPressedM(KeyModifier modifier) {
            return IsHovered && Input.GetClick(modifier) && GlobalPointIsInBounds(Input.lastClickLocation) && IsInHierarchy(Input.lastClickFocus);
        }


        public bool DoubleClickedM(KeyModifier modifier) {
            return enabled && IsHovered && Input.GetDoubleClick(modifier) && IsInHierarchy(Input.lastClickFocus);
        }

        public bool GlobalPointIsInBounds(Point p) {
            return p.X >= this.GlobalX && p.Y >= this.GlobalY && p.X < this.GlobalX + this.width && p.Y < this.GlobalY + this.height;
        }
    }
}
