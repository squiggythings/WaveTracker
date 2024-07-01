using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker {
    /// <summary>
    /// Structure containing a key and a key modifier
    /// </summary>
    [Serializable]
    public struct KeyboardShortcut {
        public Keys Key { get; private set; }
        public KeyModifier Modifier { get; private set; }
        public KeyboardShortcut(Keys key, KeyModifier modifier) {
            Key = key;
            Modifier = modifier;
        }
        public KeyboardShortcut(Keys key) {
            Key = key;
            Modifier = KeyModifier.None;
        }
        public KeyboardShortcut() {
            Key = Keys.None;
            Modifier = KeyModifier.None;
        }

        public bool IsPressed() {
            return Input.GetKey(Key, Modifier);
        }
        public bool IsPressedDown() {
            return Input.GetKeyDown(Key, Modifier);
        }
        public bool IsPressedRepeat() {
            return Input.GetKeyRepeat(Key, Modifier);
        }
        public bool WasReleasedThisFrame() {
            return Input.GetKeyUp(Key, Modifier);
        }

        public override string ToString() {
            if (Key == Keys.None) {
                return "(none)";
            }            
            return Helpers.ModifierToString(Modifier) + Helpers.KeyToString(Key);
        }

        public static bool operator ==(KeyboardShortcut one, KeyboardShortcut two) {
            return one.Key == two.Key && one.Modifier == two.Modifier;
        }
        public static bool operator !=(KeyboardShortcut one, KeyboardShortcut two) {
            return one.Key != two.Key || one.Modifier != two.Modifier;
        }

        public override bool Equals(object obj) {
            if (obj is KeyboardShortcut other) {
                if (other.Key == Key && other.Modifier == Modifier) {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode() {
            return (Key, Modifier).GetHashCode();
        }

        public static KeyboardShortcut None { get { return new KeyboardShortcut(Keys.None, KeyModifier.None); } }
    }


}
