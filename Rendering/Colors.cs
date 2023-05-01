using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.Rendering
{
    public class Colors
    {
        public static readonly Color rowText = Color.White;
        public static readonly Color rowTextHighlighted = new(202, 245, 254);
        public static readonly Color rowTextSubHighlighted = new(187, 215, 254);
        public static readonly Color rowTextEmpty = new(134, 138, 219, 35);
        public static readonly Color instrumentColumnText = new(90, 234, 61);
        public static readonly Color instrumentSampleColumnText = new(255, 153, 50);
        public static readonly Color volumeColumnText = new(80, 233, 230);
        public static readonly Color effectColumnText = new(255, 82, 119);
        public static readonly Color effectColumnParameterText = new(255, 208, 208);

        public static readonly Color rowHighlightColor = new(33, 40, 64);
        public static readonly Color rowSubHighlightColor = new(26, 31, 54);
        public static readonly Color rowDefaultColor = new(20, 24, 46);
        public static readonly Color rowPlaybackColor = new(42, 29, 81);
        public static readonly Color rowPlaybackText = new(60, 37, 105);
        public static readonly Color currentRowEditColor = new(109, 29, 78);
        public static readonly Color currentRowEditEmptyText = new(162, 39, 107);
        public static readonly Color currentRowDefaultColor = new(27, 55, 130);
        public static readonly Color currentRowDefaultEmptyText = new(42, 83, 156);
        public static readonly Color cursorColor = new(126, 133, 168);
        public static readonly Color rowSeparatorColor = new(49, 56, 89);
        public static readonly Color selection = new(128, 128, 255, 150);

    }
}
