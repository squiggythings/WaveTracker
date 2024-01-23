using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using WaveTracker.Rendering;

namespace WaveTracker
{
    public static class Tooltip {
        static int hoverTime;
        static int elapsedMS;
        static int px, py;
        static string lasttooltip;
        static bool show;
        public static string TooltipText { get; set; }
        public static string TooltipTextLong { get; set; }
        public static void Update(GameTime gameTime) {
            TooltipText = "";
            TooltipTextLong = "";
            elapsedMS = gameTime.ElapsedGameTime.Milliseconds;


        }
        public static void Draw() {
            int y = Game1.WindowHeight - 8;
            Graphics.DrawRect(0, y, 960, 9, new Color(230, 230, 230, 255));
            if (TooltipText != lasttooltip) {
                hoverTime = 0;
                lasttooltip = TooltipText;
            }
            if (TooltipTextLong != "" && TooltipTextLong != null) Graphics.Write(TooltipTextLong, 2, y, new Color(58, 63, 94));
            if (TooltipText != "" && TooltipText != null) {
                hoverTime += elapsedMS;
                if (hoverTime > 500) {
                    if (show == false) {
                        px = Input.MousePositionX;
                        py = Input.MousePositionY + 10;
                    }
                    show = true;
                    int width = Helpers.getWidthOfText(TooltipText) + 6;
                    if (px + width + 1 > 960) {
                        int diff = (px + width + 1) - 960;
                        px -= diff;
                    }
                    Graphics.DrawRect(px - 1, py - 1, width, 10, new Color(151, 156, 186));

                    Graphics.DrawRect(px, py, width - 2, 8, new Color(255, 255, 255));
                    Graphics.Write(TooltipText, px + 2, py, new Color(151, 156, 186));
                } else {
                    show = false;
                }
            } else {
                show = false;
                hoverTime = 0;
            }
        }
    }
}
