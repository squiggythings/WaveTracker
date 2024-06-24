using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker {

    public class AcceptableKeys {
        public List<Keys> keys = new List<Keys>();

        void Init() {
            //Keys.Add(Keys.)
        }
    }
    [Serializable]
    public class KeyboardBinding {
        List<KeyboardShortcut> defaultShortcuts;
        List<KeyboardShortcut> overrideShortcuts;
        public KeyboardBinding() {
            defaultShortcuts = new List<KeyboardShortcut>();
            overrideShortcuts = new List<KeyboardShortcut>();

        }
        public KeyboardBinding(Keys key, KeyModifier modifier) {
            defaultShortcuts = new List<KeyboardShortcut>();
            overrideShortcuts = new List<KeyboardShortcut>();
            defaultShortcuts.Add(new KeyboardShortcut(key, modifier));

        }
        public KeyboardBinding(Keys key, KeyModifier modifier, Keys key2, KeyModifier modifier2) {
            defaultShortcuts = new List<KeyboardShortcut>();
            overrideShortcuts = new List<KeyboardShortcut>();
            defaultShortcuts.Add(new KeyboardShortcut(key, modifier));
            defaultShortcuts.Add(new KeyboardShortcut(key2, modifier2));
        }

 

     

        public bool IsPressed() {
            foreach (KeyboardShortcut shortcut in defaultShortcuts) {
                if (shortcut.IsPressed())
                    return true;
            }
            foreach (KeyboardShortcut shortcut in overrideShortcuts) {
                if (shortcut.IsPressed())
                    return true;
            }
            return false;
        }
        public bool IsPressedDown() {
            foreach (KeyboardShortcut shortcut in defaultShortcuts) {
                if (shortcut.IsPressedDown())
                    return true;
            }
            foreach (KeyboardShortcut shortcut in overrideShortcuts) {
                if (shortcut.IsPressedDown())
                    return true;
            }
            return false;
        }
        public bool IsPressedRepeat() {
            foreach (KeyboardShortcut shortcut in defaultShortcuts) {
                if (shortcut.IsPressedRepeat())
                    return true;
            }
            foreach (KeyboardShortcut shortcut in overrideShortcuts) {
                if (shortcut.IsPressedRepeat())
                    return true;
            }
            return false;
        }
        public bool WasReleasedThisFrame() {
            foreach (KeyboardShortcut shortcut in defaultShortcuts) {
                if (shortcut.WasReleasedThisFrame())
                    return true;
            }
            foreach (KeyboardShortcut shortcut in overrideShortcuts) {
                if (shortcut.WasReleasedThisFrame())
                    return true;
            }
            return false;
        }
    }

    public struct KeyboardShortcut {
        Keys key;
        KeyModifier modifier;
        public KeyboardShortcut(Keys key, KeyModifier modifier) {
            this.key = key;
            this.modifier = modifier;
        }

        public bool IsPressed() {
            return Input.GetKey(key, modifier);
        }
        public bool IsPressedDown() {
            return Input.GetKeyDown(key, modifier);
        }
        public bool IsPressedRepeat() {
            return Input.GetKeyRepeat(key, modifier);
        }
        public bool WasReleasedThisFrame() {
            return Input.GetKeyUp(key, modifier);
        }

        public override string ToString() {
            string modifier = "";
            switch (this.modifier) {
                case KeyModifier.None:
                    modifier = "";
                    break;
                case KeyModifier.Shift:
                    modifier = "Shift+";
                    break;
                case KeyModifier.Alt:
                    modifier = "Alt+";
                    break;
                case KeyModifier.Ctrl:
                    modifier = "Ctrl+";
                    break;
                case KeyModifier.ShiftAlt:
                    modifier = "Shift+Alt+";
                    break;
                case KeyModifier.CtrlShift:
                    modifier = "Ctrl+Shift+";
                    break;
                case KeyModifier.CtrlAlt:
                    modifier = "Ctrl+Alt+";
                    break;
                case KeyModifier.CtrlShiftAlt:
                    modifier = "Ctrl+Shift+Alt+";
                    break;
            };
            return modifier + key.ToString();
        }
    }


}
