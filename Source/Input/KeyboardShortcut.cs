using Microsoft.Xna.Framework.Input;
using System;
using System.Text.Json.Serialization;

namespace WaveTracker {
    /// <summary>
    /// Structure containing a key and a key modifier
    /// </summary>
    [Serializable]
    public struct KeyboardShortcut {
        [JsonRequired]
        public Keys Key { get; set; }
        [JsonRequired]
        public KeyModifier Modifier { get; set; }
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
        [JsonIgnore]
        public bool IsPressed {
            get { return Input.GetKey(Key, Modifier); }
        }
        [JsonIgnore]
        public bool IsPressedDown {
            get { return Input.GetKeyDown(Key, Modifier); }
        }
        [JsonIgnore]
        public bool IsPressedRepeat {
            get { return Input.GetKeyRepeat(Key, Modifier); }
        }
        [JsonIgnore]
        public bool WasReleasedThisFrame {
            get { return Input.GetKeyUp(Key, Modifier); }
        }

        public override string ToString() {
            return Key == Keys.None ? "(none)" : Helpers.ModifierToString(Modifier) + Helpers.KeyToString(Key);
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
