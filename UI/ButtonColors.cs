using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.UI {
    public struct ButtonColors {
        public Color backgroundColor;
        public Color backgroundColorHover;
        public Color backgroundColorPressed;
        public Color textColor;
        public Color textColorPressed;
        public Color borderColor;
        public Color borderColorPressed;
        public Color textColorDisabled;
        public Color backgroundColorDisabled;
        public Color toggleBackgroundColor;
        public static ButtonColors Default {
            get {
                ButtonColors result = new ButtonColors();

                result.backgroundColor = new Color(190, 192, 211);
                result.backgroundColorHover = new Color(163, 167, 194);
                result.backgroundColorPressed = new Color(141, 156, 192);

                result.borderColor = new Color(104, 111, 153);
                result.borderColorPressed = new Color(8, 124, 232);

                result.textColor = new Color(20, 24, 46);
                result.textColorPressed = new Color(20, 24, 46);
                result.textColorDisabled = new Color(163, 167, 194);
                result.backgroundColorDisabled = new Color(190, 192, 211);
                result.toggleBackgroundColor = result.backgroundColorPressed;
                return result;
            }
        }

        public static ButtonColors Round {
            get {
                ButtonColors result = new ButtonColors();

                result.backgroundColor = new Color(104, 111, 153);
                result.backgroundColorHover = new Color(132, 138, 172);
                result.backgroundColorPressed = new Color(8, 124, 232);

                result.borderColor = new Color(104, 111, 153);
                result.borderColorPressed = new Color(8, 124, 232);

                result.textColor = new Color(255, 255, 255);
                result.textColorPressed = new Color(255, 255, 255);

                result.textColorDisabled = new Color(163, 167, 194);
                result.backgroundColorDisabled = new Color(192, 195, 212);
                result.toggleBackgroundColor = new Color(32, 144, 246);
                return result;
            }
        }

        public static ButtonColors RoundLight {
            get {
                ButtonColors result = new ButtonColors();

                result.backgroundColorHover = new Color(170, 174, 199);
                result.backgroundColor = new Color(182, 185, 207);
                result.backgroundColorPressed = new Color(137, 142, 177);

                result.borderColor = new Color(104, 111, 153);
                result.borderColorPressed = new Color(8, 124, 232);

                result.textColor = new Color(20, 24, 46);
                result.textColorPressed = new Color(20, 24, 46);

                result.textColorDisabled = new Color(174, 176, 199);
                result.backgroundColorDisabled = new Color(207, 208, 221);
                result.toggleBackgroundColor = new Color(22, 134, 236);
                return result;
            }
        }
    }
}
