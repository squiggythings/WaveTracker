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
using WaveTracker.Tracker;
using WaveTracker.Rendering;

namespace WaveTracker {
    public class Helpers {
        /// <summary>
        /// Returns a MIDI note number as a string in the format: [note][-][octave]
        /// </summary>
        /// <returns></returns>
        public static string MIDINoteToText(int note) {
            if (note == WTPattern.EVENT_NOTE_CUT) { return "OFF"; }
            if (note == WTPattern.EVENT_NOTE_RELEASE) { return "REL"; }
            if (note == WTPattern.EVENT_EMPTY) { return "···"; }
            int noteWithinOctave = note % 12;
            int octave = note / 12 - 1;
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

        /// <summary>
        /// OUTDATED - do not use
        /// </summary>
        public static char GetEffectCharacter(int num) {
            return num switch {
                0 => '0',
                1 => '1',
                2 => '2',
                3 => '3',
                4 => '4',
                7 => '7',
                8 => '8',
                9 => '9',
                10 => 'Q',
                11 => 'R',
                12 => 'A',
                13 => 'W',
                14 => 'P',
                15 => 'F',
                16 => 'V',
                17 => 'G',
                18 => 'S',
                19 => 'I',
                20 => 'C',
                21 => 'B',
                22 => 'D',
                23 => 'M',
                24 => 'J',
                25 => 'J',
                _ => '-',
            };
        }

        /// <summary>
        /// OUTDATED - do not use
        /// </summary>
        public static bool isEffectFrameTerminator(int num) {
            return num >= 20 && num <= 22;
        }

        public static int GetWidthOfText(string text) {
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

        public static int GetHeightOfMultilineText(string text, int width, int lineSpacing = 10) {
            string str = "";
            string[] words = text.Split(' ');
            int w = 0;
            foreach (string word in words) {
                w += GetWidthOfText(word + " ");
                if (w > width) {
                    str += "\n" + word + " ";
                    w = GetWidthOfText(word + " ");
                }
                else {
                    str += word + " ";
                }
            }
            string[] lines = str.Split('\n');
            return lineSpacing * lines.Length;
        }

        /// <summary>
        /// Truncates a string if it would go beyond a certain width in pixels, adding ellipses at the end.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string TrimTextToWidth(int width, string t) {

            if (GetWidthOfText(t) > width - 6) {
                while (GetWidthOfText(t + "...") > width - 6) {
                    t = t.Remove(t.Length - 1, 1);
                    if (t[t.Length - 1] == ' ') {
                        t = t.Remove(t.Length - 1, 1);
                    }
                }
                t += "...";
            }
            return t;
        }

        /// <summary>
        /// Converts a Keys enum to a nicer string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string KeyToString(Keys key) {
            return key switch {
                Keys.Add => "Numpad +",
                Keys.Back => "Backspace",
                Keys.D0 => "Alphanumeric 0",
                Keys.D1 => "Alphanumeric 1",
                Keys.D2 => "Alphanumeric 2",
                Keys.D3 => "Alphanumeric 3",
                Keys.D4 => "Alphanumeric 4",
                Keys.D5 => "Alphanumeric 5",
                Keys.D6 => "Alphanumeric 6",
                Keys.D7 => "Alphanumeric 7",
                Keys.D8 => "Alphanumeric 8",
                Keys.D9 => "Alphanumeric 9",
                Keys.Delete => "Del",
                Keys.Divide => "Numpad /",
                Keys.Multiply => "Numpad *",
                Keys.NumPad0 => "Numpad 0",
                Keys.NumPad1 => "Numpad 1",
                Keys.NumPad2 => "Numpad 2",
                Keys.NumPad3 => "Numpad 3",
                Keys.NumPad4 => "Numpad 4",
                Keys.NumPad5 => "Numpad 5",
                Keys.NumPad6 => "Numpad 6",
                Keys.NumPad7 => "Numpad 7",
                Keys.NumPad8 => "Numpad 8",
                Keys.NumPad9 => "Numpad 9",
                Keys.Subtract => "Numpad -",
                Keys.Oem8 => "Oem8",
                Keys.OemAuto => "Auto",
                Keys.OemBackslash => "\\",
                Keys.OemClear => "Clear",
                Keys.OemCloseBrackets => "]",
                Keys.OemComma => ",",
                Keys.OemSemicolon => ";",
                Keys.OemCopy => "Copy",
                Keys.OemEnlW => "EnlW",
                Keys.OemMinus => "-",
                Keys.OemOpenBrackets => "[",
                Keys.OemPeriod => ".",
                Keys.OemPipe => "|",
                Keys.OemPlus => "=",
                Keys.OemQuestion => "?",
                Keys.OemQuotes => "'",
                Keys.OemTilde => "`",
                _ => key.ToString(),
            };
        }

        public static string ModifierToString(KeyModifier modifier) {
            return modifier switch {
                KeyModifier.Shift => "Shift+",
                KeyModifier.Alt => "Alt+",
                KeyModifier.Ctrl => "Ctrl+",
                KeyModifier.ShiftAlt => "Shift+Alt+",
                KeyModifier.CtrlShift => "Ctrl+Shift+",
                KeyModifier.CtrlAlt => "Ctrl+Alt+",
                KeyModifier.CtrlShiftAlt => "Ctrl+Shift+Alt+",
                _ => "",
            };
            ;
        }

        /// <summary>
        /// Ensures that a string will not contain any characters that the font does not support
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string FlushString(string original) {
            if (original == null)
                return "";
            string alphabet = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()_+-={}[]\\|'\":;?/>.<,~`©àèìòùÀÈÌÒÙáéíóúýÁÉÍÓÚÝâêîôûÂÊÎÔÛãñõÃÑÕäëïöüÿÄËÏÖÜŸæçÇ";
            string ret = "";
            foreach (char c in original) {
                if (alphabet.Contains(c))
                    ret += c;
            }
            return ret;
        }
        /// <summary>
        /// Returns true if the given effect is a hexadecimal effect
        /// </summary>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public static bool IsEffectHex(char effectType) {
            return effectType switch {
                '0' or '4' or '7' or 'Q' or 'R' => true,
                _ => false,
            };
        }
        /// <summary>
        /// Returns true if the given effect is a hexadecimal effect
        /// </summary>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public static bool IsEffectHex(byte effectType) {
            return (char)effectType switch {
                '0' or '4' or '7' or 'Q' or 'R' => true,
                _ => false,
            };
        }

        public static float NoteToFrequency(float midiNoteNum) {
            return (float)Math.Pow(2, (midiNoteNum - 69) / 12.0) * 440;
        }

        public static float Mod(float a, float b) {
            return (a - b * MathF.Floor(a / b));
        }
        public static double Mod(double a, double b) {
            return (a - b * Math.Floor(a / b));
        }

        /// <summary>
        /// Quick and dirty approximation of e^x
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float Exp(float x) {
            return (6 + x * (6 + x * (3 + x))) * 0.16666666f;
        }


        public static double FastPower(double a, double b) {
            int tmp = (int)(BitConverter.DoubleToInt64Bits(a) >> 32);
            int tmp2 = (int)(b * (tmp - 1072632447) + 1072632447);
            return BitConverter.Int64BitsToDouble(((long)tmp2) << 32);
        }

        public static float FastPower(float a, float b) {
            int tmp = (int)(BitConverter.DoubleToInt64Bits(a) >> 32);
            int tmp2 = (int)(b * (tmp - 1072632447) + 1072632447);
            return (float)BitConverter.Int64BitsToDouble(((long)tmp2) << 32);
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
        /// <summary>
        /// Map a float from one range to another
        /// </summary>
        /// <param name="value"></param>
        /// <param name="from1"></param>
        /// <param name="to1"></param>
        /// <param name="from2"></param>
        /// <param name="to2"></param>
        /// <returns></returns>
        public static float Map(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        /// <summary>
        /// Map a float from one range to another, without exceeding the bounds
        /// </summary>
        /// <param name="value"></param>
        /// <param name="from1"></param>
        /// <param name="to1"></param>
        /// <param name="from2"></param>
        /// <param name="to2"></param>
        /// <returns></returns>
        public static float MapClamped(float value, float from1, float to1, float from2, float to2) {
            if (from2 < to2)
                return Math.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
            else
                return Math.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, to2, from2);
        }


        /// <summary>
        /// Returns the midi note number of the current piano key held down, -1 if none are pressed
        /// </summary>
        /// <param name="currentOctave"></param>
        /// <returns></returns>
        public static int GetPianoInput(int currentOctave) {
            currentOctave++;
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
            currentOctave++;
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
            currentOctave++;
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

        /// <summary>
        /// Returns true if the midi note number is a black key
        /// </summary>
        /// <param name="noteNum"></param>
        /// <returns></returns>
        public static bool IsNoteBlackKey(int midiNoteNum) {
            return (midiNoteNum % 12) switch {
                0 or 2 or 4 or 5 or 7 or 9 or 11 => false,
                _ => true,
            };
        }

        //public static bool readWav(string filepath, out float[] L, out float[] R) {
        //    List<float> LChannel = new List<float>();
        //    List<float> RChannel = new List<float>();
        //    try {
        //        AudioFileReader Nreader = new AudioFileReader(filepath);
        //        ISampleProvider isp;
        //        bool mono = Nreader.WaveFormat.Channels == 1;

        //        var outFormat = new WaveFormat(44100, Nreader.WaveFormat.Channels);
        //        IWaveProvider waveProvider = Nreader.ToWaveProvider();
        //        if (Preferences.profile.automaticallyResampleSamples)
        //            using (var resampler = new MediaFoundationResampler(Nreader, outFormat)) {
        //                isp = resampler.ToSampleProvider();
        //            }
        //        else
        //            isp = Nreader.ToSampleProvider();
        //        long sampleLength = (long)(Nreader.Length * (44100.0 / Nreader.WaveFormat.SampleRate));
        //        float[] buffer = new float[sampleLength / 4];
        //        isp.Read(buffer, 0, buffer.Length);
        //        for (int sampleIndex = 0, bufferIndex = 0; bufferIndex < buffer.Length; sampleIndex++) {
        //            if (sampleIndex > 16777216)
        //                break;
        //            LChannel.Add(buffer[bufferIndex++]);
        //            if (!mono)
        //                RChannel.Add(buffer[bufferIndex++]);
        //        }
        //        if (mono)
        //            RChannel.Clear();
        //        L = LChannel.ToArray();
        //        R = RChannel.ToArray();
        //        return true;
        //    } catch {
        //        LChannel.Add(0f);
        //        RChannel.Add(0f);
        //        L = LChannel.ToArray();
        //        R = RChannel.ToArray();
        //        return false;
        //    }
        //}

        /// <summary>
        /// Read an audio file into 2 arrays of shorts
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="L"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public static bool ReadAudioFile(string filepath, out short[] L, out short[] R, out int fileSampleRate) {
            List<short> LChannel = new List<short>();
            List<short> RChannel = new List<short>();
            try {
                AudioFileReader Nreader = new AudioFileReader(filepath);
                int bytesPerSample = Nreader.WaveFormat.BitsPerSample / 8;
                bool isMono = Nreader.WaveFormat.Channels == 1;
                fileSampleRate = Nreader.WaveFormat.SampleRate;
                int sampleRate = fileSampleRate;//App.Settings.SamplesWaves.AutomaticallyResampleSamples ? Audio.AudioEngine.SampleRate : Nreader.WaveFormat.SampleRate;

                ISampleProvider isp;
                WaveFormat desiredFormat = new WaveFormat(sampleRate, 16, Nreader.WaveFormat.Channels);
                IWaveProvider waveProvider = Nreader.ToWaveProvider();
                using (var resampler = new MediaFoundationResampler(Nreader, desiredFormat)) {
                    isp = resampler.ToSampleProvider();
                }
                float[] buffer = new float[Nreader.Length / bytesPerSample];
                isp.Read(buffer, 0, buffer.Length);
                for (int s = 0, v = 0; v < buffer.Length; s++) {
                    if (s > 16777216)
                        break;
                    LChannel.Add((short)(buffer[v++] * short.MaxValue));
                    if (!isMono)
                        RChannel.Add((short)(buffer[v++] * short.MaxValue));
                }
                if (isMono)
                    RChannel.Clear();
                L = LChannel.ToArray();
                R = RChannel.ToArray();
                return true;
            } catch {
                L = new short[] { };
                R = new short[] { };
                fileSampleRate = 44100;
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
            byte r;
            byte g;
            byte b;

            if (s == 0) {
                r = g = b = (byte)(l * 255);
            }
            else {
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

        /// <summary>
        /// Compresses a string with run length encoding
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RLECompress(string input) {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                char count = char.MinValue;
                while (i < input.Length - 1 && input[i] == input[i + 1]) {
                    i++;
                    count++;
                    if (count == char.MaxValue)
                        break;
                }
                output.Append(count + "" + input[i]);
            }
            return output.ToString();
        }

        /// <summary>
        /// Decompresses a string encoded with RLECompress
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RLEDecompress(string input) {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < input.Length - 1; i += 2) {
                char count = input[i];
                char data = input[i + 1];
                for (int j = 0; j <= count; j++) {
                    output.Append(data);
                }
            }
            return output.ToString();
        }
    }



    public static class ExtensionMethods {

        /// <summary>
        /// Returns true if this string only contains numbers
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string str) {
            foreach (char c in str) {
                if (!"0123456789".Contains(c))
                    return false;
            }
            return true;
        }
        public static bool ApproximatelyEqualTo(this float value, float other, float tolerance = 0.001f) {
            return Math.Abs(value - other) < tolerance;
        }

        public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Color MultiplyWith(this Color col, Color other) {
            byte r = (byte)(col.R * (other.R / 255f));
            byte g = (byte)(col.G * (other.G / 255f));
            byte b = (byte)(col.B * (other.B / 255f));
            byte a = (byte)(col.A * (other.A / 255f));
            return new Color(r, g, b, a);
        }

        public static Color AddTo(this Color value, Color other) {
            return new Color(value.R + other.R, value.G + other.G, value.B + other.B, value.A + other.A);
        }

        /// <summary>
        /// Returns the negative of this color
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color ToNegative(this Color value) {
            return new Color(255 - value.R, 255 - value.G, 255 - value.B, 255);
        }

        /// <summary>
        /// Converts this color to an HSLColor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
                }
                else {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max) {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max) {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max) {
                    H = 4f + (_R - _G) / _Delta;
                }
            }
            H *= 60f;
            if (H < 0) H += 360;
            H %= 360;
            return new HSLColor(H, S, L, value.A / 255f);
        }

        /// <summary>
        /// Returns the hex code of this color, ignoring alpha
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetHexCode(this Color value) {
            byte[] bytes = { value.R, value.G, value.B };
            return Convert.ToHexString(bytes).ToLower();
        }
        /// <summary>
        /// Returns the hex code of this color, including alpha if it has transparency
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetHexCodeWithAlpha(this Color value) {
            if (value.A == 255)
                return GetHexCode(value);
            else
                return GetHexCodeWithAlphaAlways(value);
        }
        /// <summary>
        /// Returns the hex code of this color, with alpha channel even if it doesnt have transparency
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetHexCodeWithAlphaAlways(this Color value) {
            byte[] bytes = { value.R, value.G, value.B, value.A };
            return Convert.ToHexString(bytes).ToLower();
        }

        /// <summary>
        /// Lerps this color with another color
        /// </summary>
        /// <param name="col"></param>
        /// <param name="other"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Color Lerp(this Color col, Color other, float t) {
            byte r = (byte)MathHelper.Lerp(col.R, other.R, t);
            byte g = (byte)MathHelper.Lerp(col.G, other.G, t);
            byte b = (byte)MathHelper.Lerp(col.B, other.B, t);
            byte a = (byte)MathHelper.Lerp(col.A, other.A, t);
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Sets this color from a hex string <c>hexCode</c><br></br>
        /// <c>hexCode</c> can optionally contain alpha information.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="hexCode"></param>
        /// <returns></returns>
        public static void SetFromHex(this ref Color value, string hexCode) {
            value = Helpers.HexCodeToColor(hexCode);
        }

        public static CellType ToCellType(this CursorColumnType cursorColumn) {
            return cursorColumn switch {
                CursorColumnType.Note => CellType.Note,
                CursorColumnType.Instrument1 or CursorColumnType.Instrument2 => CellType.Instrument,
                CursorColumnType.Volume1 or CursorColumnType.Volume2 => CellType.Volume,
                CursorColumnType.Effect1 => CellType.Effect1,
                CursorColumnType.Effect1Param1 or CursorColumnType.Effect1Param2 => CellType.Effect1Parameter,
                CursorColumnType.Effect2 => CellType.Effect2,
                CursorColumnType.Effect2Param1 or CursorColumnType.Effect2Param2 => CellType.Effect2Parameter,
                CursorColumnType.Effect3 => CellType.Effect3,
                CursorColumnType.Effect3Param1 or CursorColumnType.Effect3Param2 => CellType.Effect3Parameter,
                CursorColumnType.Effect4 => CellType.Effect4,
                CursorColumnType.Effect4Param1 or CursorColumnType.Effect4Param2 => CellType.Effect4Parameter,
                _ => throw new IndexOutOfRangeException()
            };
        }

        public static CursorColumnType ToNearestCursorColumn(this CellType cellType) {
            return cellType switch {
                CellType.Note => CursorColumnType.Note,
                CellType.Instrument => CursorColumnType.Instrument1,
                CellType.Volume => CursorColumnType.Volume1,
                CellType.Effect1 => CursorColumnType.Effect1,
                CellType.Effect1Parameter => CursorColumnType.Effect1Param1,
                CellType.Effect2 => CursorColumnType.Effect2,
                CellType.Effect2Parameter => CursorColumnType.Effect2Param1,
                CellType.Effect3 => CursorColumnType.Effect3,
                CellType.Effect3Parameter => CursorColumnType.Effect3Param1,
                CellType.Effect4 => CursorColumnType.Effect4,
                CellType.Effect4Parameter => CursorColumnType.Effect4Param1,
                _ => throw new IndexOutOfRangeException()
            };
        }
    }
}
