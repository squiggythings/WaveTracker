using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker {
    /// <summary>
    /// Wrapper class for accessing the system's clipboard<br/>
    /// TODO: Make this support multiple platform clipboards
    /// </summary>
    public static class SystemClipboard {
        public static void SetText(string text) {
            System.Windows.Forms.Clipboard.SetText(text);
        }
        public static string GetText() {
            if (HasText()) {
                return System.Windows.Forms.Clipboard.GetText();
            }
            else {
                return "";
            }
        }

        public static bool HasText() {
            return System.Windows.Forms.Clipboard.ContainsText();
        }
    }
}
