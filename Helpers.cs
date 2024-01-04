using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WaveTracker {
    public class Helpers {
        /// <summary>
        /// Returns the name of a note value.
        /// <br></br>
        /// 48 returns "C-4", 51 returns "D#4"<br></br>-1 returns "...", -2 returns "OFF"
        /// </summary>
        /// <param name="noteNum"></param>
        /// <returns></returns>
        public static string GetNoteName(int noteNum) {
            if (noteNum == Tracker.Frame.NOTE_CUT_VALUE) { return "OFF"; }
            if (noteNum == Tracker.Frame.NOTE_RELEASE_VALUE) { return "REL"; }
            if (noteNum == Tracker.Frame.NOTE_EMPTY_VALUE) { return "..."; }
            int noteWithinOctave = noteNum % 12;
            int octave = noteNum / 12;
            string noteName = "";
            switch (noteWithinOctave) {
                case 0:
                    noteName = "C-";
                    break;
                case 1:
                    noteName = "C#";
                    break;
                case 2:
                    noteName = "D-";
                    break;
                case 3:
                    noteName = "D#";
                    break;
                case 4:
                    noteName = "E-";
                    break;
                case 5:
                    noteName = "F-";
                    break;
                case 6:
                    noteName = "F#";
                    break;
                case 7:
                    noteName = "G-";
                    break;
                case 8:
                    noteName = "G#";
                    break;
                case 9:
                    noteName = "A-";
                    break;
                case 10:
                    noteName = "A#";
                    break;
                case 11:
                    noteName = "B-";
                    break;
            }
            return noteName + octave;
        }



        public static string GetEffectCharacter(int num) {
            return num switch {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                7 => "7",
                8 => "8",
                9 => "9",
                10 => "Q",
                11 => "R",
                12 => "A",
                13 => "W",
                14 => "P",
                15 => "F",
                16 => "V",
                17 => "G",
                18 => "S",
                19 => "I",
                20 => "C",
                21 => "B",
                22 => "D",
                23 => "M",
                24 => "J",
                25 => "L",
                _ => "-",
            };
        }

        public static bool isEffectFrameTerminator(int num) {
            return num >= 20 && num <= 22;
        }

        public static int getWidthOfText(string text) {
            int ret = 0;
            string alphabet = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()_+-={}[]\\|'\":;?/>.<,~`©àèìòùÀÈÌÒÙáéíóúýÁÉÍÓÚÝâêîôûÂÊÎÔÛãñõÃÑÕäëïöüÿÄËÏÖÜŸæçÇ";
            string chars5 = "WX&%#@~YM«»mw";
            string chars3 = "kI 1-crtT<>{}+\"^\\/?=";
            string chars2 = "(),[]*j";
            string chars1 = "li.'!";

            foreach (char c in text) {
                if (alphabet.Contains(c))
                    if (chars5.Contains(c))
                        ret += 6;
                    else if (chars3.Contains(c))
                        ret += 4;
                    else if (chars2.Contains(c))
                        ret += 3;
                    else if (chars1.Contains(c))
                        ret += 2;
                    else
                        ret += 5;
                else
                    ret += 0;
            }
            return ret - 1;
        }

        public static string TrimTextToWidth(int width, string t) {

            if (getWidthOfText(t) > width - 6) {
                while (getWidthOfText(t + "...") > width - 6) {
                    t = t.Remove(t.Length - 1, 1);
                    if (t[t.Length - 1] == ' ') {
                        t = t.Remove(t.Length - 1, 1);
                    }
                }
                t += "...";
            }
            return t;
        }
        public static string FlushString(string original) {
            string alphabet = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()_+-={}[]\\|'\":;?/>.<,~`©àèìòùÀÈÌÒÙáéíóúýÁÉÍÓÚÝâêîôûÂÊÎÔÛãñõÃÑÕäëïöüÿÄËÏÖÜŸæçÇ";
            string ret = "";
            foreach (char c in original) {
                if (alphabet.Contains(c))
                    ret += c;
            }
            return ret;
        }
        public static bool isEffectHexadecimal(int effectNum) {
            return new List<int> { 0, 4, 10, 11, 7 }.Contains(effectNum);
        }

        public static float NoteToFrequency(float noteNum) {
            return (float)Math.Pow(2, (noteNum - 57) / 12.0) * 440;
        }

        public static float Mod(float a, float b) {
            return (a - b * MathF.Floor(a / b));
        }
        public static double Mod(double a, double b) {
            return (a - b * Math.Floor(a / b));
        }

        public static double PowerA(double a, double b) {
            int tmp = (int)(BitConverter.DoubleToInt64Bits(a) >> 32);
            int tmp2 = (int)(b * (tmp - 1072632447) + 1072632447);
            return BitConverter.Int64BitsToDouble(((long)tmp2) << 32);
        }
        /// <summary>
        /// Sets alpha of color. from 0-255
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color Alpha(Color c, int a) {
            return new Color(c.R, c.G, c.B, a);
        }

        public static Color LerpColor(Color a, Color b, float amt) {
            Color c = Color.White;
            c.R = (byte)(a.R + (b.R - a.R) * amt);
            c.G = (byte)(a.G + (b.G - a.G) * amt);
            c.B = (byte)(a.B + (b.B - a.B) * amt);
            return c;
        }
        public static float Map(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public static float MapClamped(float value, float from1, float to1, float from2, float to2) {
            if (from2 < to2)
                return Math.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
            else
                return Math.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, to2, from2);
        }
        //public static float MapClamped(float s, float a1, float a2, float b1, float b2)
        //{
        //    if (b1 > b2)
        //        return Math.Clamp(b1 + (s - a1) * (b2 - b1) / (a2 - a1), b2, b1);
        //    else
        //        return Math.Clamp(b1 + (s - a1) * (b2 - b1) / (a2 - a1), b1, b2);
        //}

        public static int GetPianoInput(int currentOctave) {

            if (Input.GetKey(Keys.Z, KeyModifier.None)) return currentOctave * 12 + 0;
            if (Input.GetKey(Keys.S, KeyModifier.None)) return currentOctave * 12 + 1;
            if (Input.GetKey(Keys.X, KeyModifier.None)) return currentOctave * 12 + 2;
            if (Input.GetKey(Keys.D, KeyModifier.None)) return currentOctave * 12 + 3;
            if (Input.GetKey(Keys.C, KeyModifier.None)) return currentOctave * 12 + 4;
            if (Input.GetKey(Keys.V, KeyModifier.None)) return currentOctave * 12 + 5;
            if (Input.GetKey(Keys.G, KeyModifier.None)) return currentOctave * 12 + 6;
            if (Input.GetKey(Keys.B, KeyModifier.None)) return currentOctave * 12 + 7;
            if (Input.GetKey(Keys.H, KeyModifier.None)) return currentOctave * 12 + 8;
            if (Input.GetKey(Keys.N, KeyModifier.None)) return currentOctave * 12 + 9;
            if (Input.GetKey(Keys.J, KeyModifier.None)) return currentOctave * 12 + 10;
            if (Input.GetKey(Keys.M, KeyModifier.None)) return currentOctave * 12 + 11;
            if (Input.GetKey(Keys.OemComma, KeyModifier.None)) return currentOctave * 12 + 12;
            if (Input.GetKey(Keys.L, KeyModifier.None)) return currentOctave * 12 + 13;
            if (Input.GetKey(Keys.OemPeriod, KeyModifier.None)) return currentOctave * 12 + 14;
            if (Input.GetKey(Keys.OemSemicolon, KeyModifier.None)) return currentOctave * 12 + 15;
            if (Input.GetKey(Keys.OemQuestion, KeyModifier.None)) return currentOctave * 12 + 16;
            if (Input.GetKey(Keys.Q, KeyModifier.None)) return currentOctave * 12 + 12;
            if (Input.GetKey(Keys.D2, KeyModifier.None)) return currentOctave * 12 + 13;
            if (Input.GetKey(Keys.W, KeyModifier.None)) return currentOctave * 12 + 14;
            if (Input.GetKey(Keys.D3, KeyModifier.None)) return currentOctave * 12 + 15;
            if (Input.GetKey(Keys.E, KeyModifier.None)) return currentOctave * 12 + 16;
            if (Input.GetKey(Keys.R, KeyModifier.None)) return currentOctave * 12 + 17;
            if (Input.GetKey(Keys.D5, KeyModifier.None)) return currentOctave * 12 + 18;
            if (Input.GetKey(Keys.T, KeyModifier.None)) return currentOctave * 12 + 19;
            if (Input.GetKey(Keys.D6, KeyModifier.None)) return currentOctave * 12 + 20;
            if (Input.GetKey(Keys.Y, KeyModifier.None)) return currentOctave * 12 + 21;
            if (Input.GetKey(Keys.D7, KeyModifier.None)) return currentOctave * 12 + 22;
            if (Input.GetKey(Keys.U, KeyModifier.None)) return currentOctave * 12 + 23;
            if (Input.GetKey(Keys.I, KeyModifier.None)) return currentOctave * 12 + 24;
            if (Input.GetKey(Keys.D9, KeyModifier.None)) return currentOctave * 12 + 25;
            if (Input.GetKey(Keys.O, KeyModifier.None)) return currentOctave * 12 + 26;
            if (Input.GetKey(Keys.D0, KeyModifier.None)) return currentOctave * 12 + 27;
            if (Input.GetKey(Keys.P, KeyModifier.None)) return currentOctave * 12 + 28;
            return -1;
        }
        public static int GetPianoInputDown(int currentOctave) {

            if (Input.GetKeyDown(Keys.Z, KeyModifier.None)) return currentOctave * 12 + 0;
            if (Input.GetKeyDown(Keys.S, KeyModifier.None)) return currentOctave * 12 + 1;
            if (Input.GetKeyDown(Keys.X, KeyModifier.None)) return currentOctave * 12 + 2;
            if (Input.GetKeyDown(Keys.D, KeyModifier.None)) return currentOctave * 12 + 3;
            if (Input.GetKeyDown(Keys.C, KeyModifier.None)) return currentOctave * 12 + 4;
            if (Input.GetKeyDown(Keys.V, KeyModifier.None)) return currentOctave * 12 + 5;
            if (Input.GetKeyDown(Keys.G, KeyModifier.None)) return currentOctave * 12 + 6;
            if (Input.GetKeyDown(Keys.B, KeyModifier.None)) return currentOctave * 12 + 7;
            if (Input.GetKeyDown(Keys.H, KeyModifier.None)) return currentOctave * 12 + 8;
            if (Input.GetKeyDown(Keys.N, KeyModifier.None)) return currentOctave * 12 + 9;
            if (Input.GetKeyDown(Keys.J, KeyModifier.None)) return currentOctave * 12 + 10;
            if (Input.GetKeyDown(Keys.M, KeyModifier.None)) return currentOctave * 12 + 11;
            if (Input.GetKeyDown(Keys.OemComma, KeyModifier.None)) return currentOctave * 12 + 12;
            if (Input.GetKeyDown(Keys.L, KeyModifier.None)) return currentOctave * 12 + 13;
            if (Input.GetKeyDown(Keys.OemPeriod, KeyModifier.None)) return currentOctave * 12 + 14;
            if (Input.GetKeyDown(Keys.OemSemicolon, KeyModifier.None)) return currentOctave * 12 + 15;
            if (Input.GetKeyDown(Keys.OemQuestion, KeyModifier.None)) return currentOctave * 12 + 16;
            if (Input.GetKeyDown(Keys.Q, KeyModifier.None)) return currentOctave * 12 + 12;
            if (Input.GetKeyDown(Keys.D2, KeyModifier.None)) return currentOctave * 12 + 13;
            if (Input.GetKeyDown(Keys.W, KeyModifier.None)) return currentOctave * 12 + 14;
            if (Input.GetKeyDown(Keys.D3, KeyModifier.None)) return currentOctave * 12 + 15;
            if (Input.GetKeyDown(Keys.E, KeyModifier.None)) return currentOctave * 12 + 16;
            if (Input.GetKeyDown(Keys.R, KeyModifier.None)) return currentOctave * 12 + 17;
            if (Input.GetKeyDown(Keys.D5, KeyModifier.None)) return currentOctave * 12 + 18;
            if (Input.GetKeyDown(Keys.T, KeyModifier.None)) return currentOctave * 12 + 19;
            if (Input.GetKeyDown(Keys.D6, KeyModifier.None)) return currentOctave * 12 + 20;
            if (Input.GetKeyDown(Keys.Y, KeyModifier.None)) return currentOctave * 12 + 21;
            if (Input.GetKeyDown(Keys.D7, KeyModifier.None)) return currentOctave * 12 + 22;
            if (Input.GetKeyDown(Keys.U, KeyModifier.None)) return currentOctave * 12 + 23;
            if (Input.GetKeyDown(Keys.I, KeyModifier.None)) return currentOctave * 12 + 24;
            if (Input.GetKeyDown(Keys.D9, KeyModifier.None)) return currentOctave * 12 + 25;
            if (Input.GetKeyDown(Keys.O, KeyModifier.None)) return currentOctave * 12 + 26;
            if (Input.GetKeyDown(Keys.D0, KeyModifier.None)) return currentOctave * 12 + 27;
            if (Input.GetKeyDown(Keys.P, KeyModifier.None)) return currentOctave * 12 + 28;
            return -1;
        }
        public static int GetPianoInputUp(int currentOctave) {

            if (Input.GetKeyUp(Keys.Z, KeyModifier.None)) return currentOctave * 12 + 0;
            if (Input.GetKeyUp(Keys.S, KeyModifier.None)) return currentOctave * 12 + 1;
            if (Input.GetKeyUp(Keys.X, KeyModifier.None)) return currentOctave * 12 + 2;
            if (Input.GetKeyUp(Keys.D, KeyModifier.None)) return currentOctave * 12 + 3;
            if (Input.GetKeyUp(Keys.C, KeyModifier.None)) return currentOctave * 12 + 4;
            if (Input.GetKeyUp(Keys.V, KeyModifier.None)) return currentOctave * 12 + 5;
            if (Input.GetKeyUp(Keys.G, KeyModifier.None)) return currentOctave * 12 + 6;
            if (Input.GetKeyUp(Keys.B, KeyModifier.None)) return currentOctave * 12 + 7;
            if (Input.GetKeyUp(Keys.H, KeyModifier.None)) return currentOctave * 12 + 8;
            if (Input.GetKeyUp(Keys.N, KeyModifier.None)) return currentOctave * 12 + 9;
            if (Input.GetKeyUp(Keys.J, KeyModifier.None)) return currentOctave * 12 + 10;
            if (Input.GetKeyUp(Keys.M, KeyModifier.None)) return currentOctave * 12 + 11;
            if (Input.GetKeyUp(Keys.OemComma, KeyModifier.None)) return currentOctave * 12 + 12;
            if (Input.GetKeyUp(Keys.L, KeyModifier.None)) return currentOctave * 12 + 13;
            if (Input.GetKeyUp(Keys.OemPeriod, KeyModifier.None)) return currentOctave * 12 + 14;
            if (Input.GetKeyUp(Keys.OemSemicolon, KeyModifier.None)) return currentOctave * 12 + 15;
            if (Input.GetKeyUp(Keys.OemQuestion, KeyModifier.None)) return currentOctave * 12 + 16;
            if (Input.GetKeyUp(Keys.Q, KeyModifier.None)) return currentOctave * 12 + 12;
            if (Input.GetKeyUp(Keys.D2, KeyModifier.None)) return currentOctave * 12 + 13;
            if (Input.GetKeyUp(Keys.W, KeyModifier.None)) return currentOctave * 12 + 14;
            if (Input.GetKeyUp(Keys.D3, KeyModifier.None)) return currentOctave * 12 + 15;
            if (Input.GetKeyUp(Keys.E, KeyModifier.None)) return currentOctave * 12 + 16;
            if (Input.GetKeyUp(Keys.R, KeyModifier.None)) return currentOctave * 12 + 17;
            if (Input.GetKeyUp(Keys.D5, KeyModifier.None)) return currentOctave * 12 + 18;
            if (Input.GetKeyUp(Keys.T, KeyModifier.None)) return currentOctave * 12 + 19;
            if (Input.GetKeyUp(Keys.D6, KeyModifier.None)) return currentOctave * 12 + 20;
            if (Input.GetKeyUp(Keys.Y, KeyModifier.None)) return currentOctave * 12 + 21;
            if (Input.GetKeyUp(Keys.D7, KeyModifier.None)) return currentOctave * 12 + 22;
            if (Input.GetKeyUp(Keys.U, KeyModifier.None)) return currentOctave * 12 + 23;
            if (Input.GetKeyUp(Keys.I, KeyModifier.None)) return currentOctave * 12 + 24;
            if (Input.GetKeyUp(Keys.D9, KeyModifier.None)) return currentOctave * 12 + 25;
            if (Input.GetKeyUp(Keys.O, KeyModifier.None)) return currentOctave * 12 + 26;
            if (Input.GetKeyUp(Keys.D0, KeyModifier.None)) return currentOctave * 12 + 27;
            if (Input.GetKeyUp(Keys.P, KeyModifier.None)) return currentOctave * 12 + 28;
            return -1;
        }

        public static bool isNoteBlackKey(int noteNum) {
            switch (noteNum % 12) {
                case 0:
                case 2:
                case 4:
                case 5:
                case 7:
                case 9:
                case 11:
                    return false;
            }
            return true;
        }

        public static bool readWav(string filepath, out float[] L, out float[] R) {
            List<float> LChannel = new List<float>();
            List<float> RChannel = new List<float>();
            try {
                AudioFileReader Nreader = new AudioFileReader(filepath);
                ISampleProvider isp;
                bool mono = Nreader.WaveFormat.Channels == 1;

                var outFormat = new WaveFormat(44100, Nreader.WaveFormat.Channels);
                IWaveProvider waveProvider = Nreader.ToWaveProvider();
                if (Preferences.profile.automaticallyResampleSamples)
                    using (var resampler = new MediaFoundationResampler(Nreader, outFormat)) {
                        isp = resampler.ToSampleProvider();
                    }
                else
                    isp = Nreader.ToSampleProvider();
                long sampleLength = (long)(Nreader.Length * (44100.0 / Nreader.WaveFormat.SampleRate));
                float[] buffer = new float[sampleLength / 4];
                isp.Read(buffer, 0, buffer.Length);
                for (int s = 0, v = 0; v < buffer.Length; s++) {
                    if (s > 44100 * 120)
                        break;
                    LChannel.Add(buffer[v++]);
                    if (!mono)
                        RChannel.Add(buffer[v++]);
                }
                if (mono)
                    RChannel.Clear();
                L = LChannel.ToArray();
                R = RChannel.ToArray();
                return true;
            } catch {
                LChannel.Add(0f);
                RChannel.Add(0f);
                L = LChannel.ToArray();
                R = RChannel.ToArray();
                return false;
            }
        }


        /// <summary>
        /// Converts from HSL to RGB. Hue from 0-360, Saturation and Lightness from 0.0-1.0
        /// </summary>
        /// <param name="h">Hue from 0-360</param>
        /// <param name="s">Saturation from 0.0-1.0</param>
        /// <param name="l">Lightness from 0.0-1.0</param>
        /// <returns>RGB version of the HSL color</returns>
        public static Color HSLtoRGB(int h, float s, float l) {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (s == 0) {
                r = g = b = (byte)(l * 255);
            } else {
                float v1, v2;
                float hue = (float)h / 360;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return new Color(r, g, b);
        }

        private static float HueToRGB(float v1, float v2, float vH) {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
        public static Color HexCodeToColor(string hexCode) {
            if (hexCode.StartsWith("#")) {
                hexCode = hexCode.Substring(1);
            }
            byte[] bytes = Convert.FromHexString(hexCode.ToUpper());
            if (bytes.Length > 3)
                return new Color(bytes[0], bytes[1], bytes[2], bytes[3]);
            else
                return new Color(bytes[0], bytes[1], bytes[2], (byte)255);
        }
    }

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
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (S == 0) {
                r = g = b = (byte)(L * 255);
            } else {
                float v1, v2;
                float hue = (float)H / 360;

                v2 = (L < 0.5) ? (L * (1 + S)) : ((L + S) - (L * S));
                v1 = 2 * L - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return new Color(r, g, b, (byte)(A * 255));
        }
        private static float HueToRGB(float v1, float v2, float vH) {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
    }

    public static class ExtensionMethods {
        public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static Color AddTo(this Color value, Color other) {
            return new Color(value.R + other.R, value.G + other.G, value.B + other.B, value.A + other.A);
        }

        public static Color ToNegative(this Color value) {
            return new Color(255 - value.R, 255 - value.G, 255 - value.B, 255);
        }

        public static HSLColor ToHSL(this Color value) {
            float _R = (value.R / 255f);
            float _G = (value.G / 255f);
            float _B = (value.B / 255f);

            float _Min = Math.Min(Math.Min(_R, _G), _B);
            float _Max = Math.Max(Math.Max(_R, _G), _B);
            float _Delta = _Max - _Min;

            float H = 0;
            float S = 0;
            float L = (float)((_Max + _Min) / 2.0f);

            if (_Delta != 0) {
                if (L < 0.5f) {
                    S = (float)(_Delta / (_Max + _Min));
                } else {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max) {
                    H = (_G - _B) / _Delta;
                } else if (_G == _Max) {
                    H = 2f + (_B - _R) / _Delta;
                } else if (_B == _Max) {
                    H = 4f + (_R - _G) / _Delta;
                }
            }
            H = H * 60f;
            if (H < 0) H += 360;
            H %= 360;
            return new HSLColor(H, S, L, value.A / 255f);
        }

        public static string GetHexCode(this Color value) {
            byte[] bytes = { value.R, value.G, value.B };
            return Convert.ToHexString(bytes).ToLower();
        }
        public static string GetHexCodeWithAlpha(this Color value) {
            if (value.A == 255)
                return GetHexCode(value);
            else
                return GetHexCodeWithAlphaAlways(value);
        }
        public static string GetHexCodeWithAlphaAlways(this Color value) {
            byte[] bytes = { value.R, value.G, value.B, value.A };
            return Convert.ToHexString(bytes).ToLower();
        }

        public static Color Lerp(this Color col, Color other, float t) {
            byte r = (byte)MathHelper.Lerp(col.R, other.R, t);
            byte g = (byte)MathHelper.Lerp(col.G, other.G, t);
            byte b = (byte)MathHelper.Lerp(col.B, other.B, t);
            byte a = (byte)MathHelper.Lerp(col.A, other.A, t);
            return new Color(r, g, b, a);
        }
    }
}
