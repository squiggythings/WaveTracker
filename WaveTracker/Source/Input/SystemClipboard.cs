using TextCopy;

namespace WaveTracker {
    /// <summary>
    /// Wrapper class for accessing multiple platform's clipboards
    /// </summary>
    public static class SystemClipboard {
        public static void SetText(string text) {
            ClipboardService.SetText(text);
        }
        public static string GetText() {
            return ClipboardService.GetText() ?? "";
        }
        public static bool HasText() {
            return ClipboardService.GetText() != null;
        }
    }
}
