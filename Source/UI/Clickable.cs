using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public abstract class Clickable : Element {
        public int width;
        public int height;

        public int BoundsBottom {
            get {
                return y + height;
            }
        }

        public int BoundsRight {
            get {
                return x + width;
            }
        }

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
                if (!InFocus) {
                    return false;
                }

                if (MouseX < width && MouseY < height && MouseX >= 0 && MouseY >= 0) {
                    if (TooltipTextLong != "") {
                        Tooltip.TooltipTextLong = TooltipTextLong;
                    }

                    if (TooltipText != "") {
                        Tooltip.TooltipText = TooltipText;
                    }

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
        public bool IsPressed { get { return IsHovered && Input.GetClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastClickLocation) && IsMeOrAParent(Input.lastClickFocus); } }

        public bool Clicked {
            get {
                return enabled && IsHovered && Input.GetClickUp(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastClickLocation) && GlobalPointIsInBounds(Input.LastClickReleaseLocation) && IsMeOrAParent(Input.lastClickFocus);
            }
        }

        public bool RightClicked {
            get {
                return enabled && IsHovered && Input.GetRightClickUp(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastRightClickLocation) && GlobalPointIsInBounds(Input.LastRightClickReleaseLocation);
            }
        }

        public bool RightClickedDown {
            get {
                return enabled && IsHovered && Input.GetRightClickDown(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastRightClickLocation);
            }
        }

        public bool ClickedDown {
            get {
                return enabled && IsHovered && Input.GetClickDown(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastClickLocation) && IsMeOrAParent(Input.lastClickFocus);
            }
        }

        public bool DoubleClicked {
            get {
                return enabled && IsHovered && Input.GetDoubleClick(KeyModifier._Any) && GlobalPointIsInBounds(Input.LastClickLocation) && GlobalPointIsInBounds(Input.LastClickReleaseLocation) && IsMeOrAParent(Input.lastClickFocus);
            }
        }

        public bool ClickedM(KeyModifier modifier) {
            return InFocus
&& enabled && IsHovered && Input.GetClickUp(modifier) && GlobalPointIsInBounds(Input.LastClickLocation) && GlobalPointIsInBounds(Input.LastClickReleaseLocation) && IsMeOrAParent(Input.lastClickFocus);
        }

        public bool SingleClickedM(KeyModifier modifier) {
            return InFocus
&& enabled && IsHovered && Input.GetSingleClickUp(modifier) && GlobalPointIsInBounds(Input.LastClickLocation) && GlobalPointIsInBounds(Input.LastClickReleaseLocation) && IsMeOrAParent(Input.lastClickFocus);
            ;
        }

        public bool IsPressedM(KeyModifier modifier) {
            return IsHovered && Input.GetClick(modifier) && GlobalPointIsInBounds(Input.LastClickLocation) && IsMeOrAParent(Input.lastClickFocus);
        }

        public bool DoubleClickedM(KeyModifier modifier) {
            return enabled && IsHovered && Input.GetDoubleClick(modifier) && IsMeOrAParent(Input.lastClickFocus);
        }

        public bool GlobalPointIsInBounds(Point p) {
            return p.X >= GlobalX && p.Y >= GlobalY && p.X < GlobalX + width && p.Y < GlobalY + height;
        }
    }
}
