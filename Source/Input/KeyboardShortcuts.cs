using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;

namespace WaveTracker {
    [Serializable]
    public static class KeyboardShortcuts {
        public static Dictionary<string, KeyboardBinding> bindings;


        public static void Init() {
            bindings.Add("NoteCut", new KeyboardBinding(Keys.OemTilde, KeyModifier.None));
            bindings.Add("NoteRelease", new KeyboardBinding(Keys.OemPlus, KeyModifier.None));
            bindings.Add("IncreaseOctave", new KeyboardBinding(Keys.OemCloseBrackets, KeyModifier.None, Keys.Multiply, KeyModifier.None));
            bindings.Add("DecreaseOctave", new KeyboardBinding(Keys.OemOpenBrackets, KeyModifier.None, Keys.Divide, KeyModifier.None));
            bindings.Add("Play/Stop", new KeyboardBinding(Keys.Enter, KeyModifier.None));

        }
    }
}
