using Microsoft.Xna.Framework;
using WaveTracker.Rendering;
using WaveTracker.Tracker;

namespace WaveTracker {
    public static class Tooltip {
        private static int hoverTime;
        private static int elapsedMS;
        private static int px, py;
        private static string lasttooltip;
        private static bool show;
        public static string TooltipText { get; set; }
        public static string TooltipTextLong { get; set; }

        public static char LastEffect { get; set; }

        public static void Update() {
            TooltipText = "";
            TooltipTextLong = "";

            elapsedMS = App.GameTime.ElapsedGameTime.Milliseconds;

        }

        /// <summary>
        /// Write bottom info bar
        /// </summary>
        private static void WriteInfo() {
            int y = App.WindowHeight - 8;
            // draw bar
            Graphics.DrawRect(0, y, App.WindowWidth, 9, new Color(230, 230, 230, 255));
            int x = App.WindowWidth - 40;
            Color textColor = UIColors.label;
            Graphics.WriteMonospaced(Playback.PlaybackTime.Minute.ToString("D2") + ":", x, y, textColor, 4);
            x += 12;
            Graphics.WriteMonospaced(Playback.PlaybackTime.Second.ToString("D2") + ":", x, y, textColor, 4);
            x += 12;
            Graphics.WriteMonospaced((Playback.PlaybackTime.Millisecond / 10).ToString("D2"), x, y, textColor, 4);
            x = App.WindowWidth - 100;

            float ticksPerBeat = (float)Playback.CurrentTicksPerRow * App.CurrentSong.RowHighlightSecondary;
            float ticksPerMinute = (float)App.CurrentModule.TickRate * 60;
            float BPM = ticksPerMinute / ticksPerBeat;

            Graphics.Write(BPM.ToString("0.00") + " BPM", x, y, textColor);

            x -= 46;
            Graphics.Write("Speed " + Playback.CurrentTicksPerRow, x, y, textColor);
            x -= 46;
            Graphics.Write(App.CurrentModule.TickRate + " Hz", x, y, textColor);
            if (TooltipTextLong != "" && TooltipTextLong != null) {
                Graphics.Write(TooltipTextLong, 2, y, new Color(58, 63, 94));
                LastEffect = '\0';
            }
            else if (LastEffect != '\0') {
                (string, string, string, string) effectDescription = Helpers.GetEffectDescription(LastEffect);
                if (effectDescription.Item1.Length > 0) {
                    Graphics.Write(effectDescription.Item1 + " - " + effectDescription.Item2 + " " + effectDescription.Item3, 2, y, new Color(58, 63, 94));
                }
            }

        }

        public static void Draw() {
            WriteInfo();
            if (TooltipText != lasttooltip) {
                hoverTime = 0;
                lasttooltip = TooltipText;
            }


            if (TooltipText != "" && TooltipText != null) {
                hoverTime += elapsedMS;
                if (hoverTime > 500) {
                    if (show == false) {
                        px = Input.MousePositionX;
                        py = Input.MousePositionY + 10;
                    }
                    show = true;
                    int width = Helpers.GetWidthOfText(TooltipText) + 6;
                    if (px + width + 1 > App.WindowWidth) {
                        int diff = px + width + 1 - App.WindowWidth;
                        px -= diff;
                    }
                    Graphics.DrawRect(px - 1, py - 1, width, 10, new Color(151, 156, 186));

                    Graphics.DrawRect(px, py, width - 2, 8, new Color(255, 255, 255));
                    Graphics.Write(TooltipText, px + 2, py, new Color(151, 156, 186));
                }
                else {
                    show = false;
                }
            }
            else {
                show = false;
                hoverTime = 0;
            }
        }
    }
}
