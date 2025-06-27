﻿using Microsoft.Xna.Framework;

namespace WaveTracker.Rendering {
    public struct HSLColor {
        /// <summary>
        /// Hue from 0.0-360.0
        /// </summary>
        public float H;
        /// <summary>
        /// Saturation from 0.0-1.0
        /// </summary>
        public float S;
        /// <summary>
        /// Lightness from 0.0-1.0
        /// </summary>
        public float L;
        /// <summary>
        /// Alpha from 0.0-1.0
        /// </summary>
        public float A;

        public HSLColor(float h, float s, float l, float a) {
            H = h;
            S = s;
            L = l;
            A = a;
        }

        public HSLColor(float h, float s, float l) {
            H = h;
            S = s;
            L = l;
            A = 1.0f;
        }

        public Color ToRGB() {
            byte r;
            byte g;
            byte b;
            if (S == 0) {
                r = g = b = (byte)(L * 255);
            }
            else {
                float v1, v2;
                float hue = H / 360;

                v2 = (L < 0.5) ? (L * (1 + S)) : (L + S - L * S);
                v1 = 2 * L - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + 1.0f / 3));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - 1.0f / 3));
            }

            return new Color(r, g, b, (byte)(A * 255));
        }
        private static float HueToRGB(float v1, float v2, float vH) {
            if (vH < 0) {
                vH += 1;
            }

            if (vH > 1) {
                vH -= 1;
            }

            return 6 * vH < 1 ? v1 + (v2 - v1) * 6 * vH : 2 * vH < 1 ? v2 : 3 * vH < 2 ? v1 + (v2 - v1) * (2.0f / 3 - vH) * 6 : v1;
        }
    }
}
