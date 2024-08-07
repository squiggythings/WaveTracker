using Microsoft.Xna.Framework.Input;

namespace WaveTracker.UI {
    public static class ContextMenu {
        private static Menu contextMenu;

        public static void Open(Menu menu) {
            if (contextMenu != menu) {
                contextMenu?.Close();
                contextMenu = menu;
            }
            contextMenu.SetPositionClamped(Input.MousePositionX, Input.MousePositionY);
            contextMenu.Open();
        }
        public static void Update() {
            if (contextMenu != null) {
                if (Input.GetKeyDown(Keys.Escape, KeyModifier.None)) {
                    contextMenu.Close();
                    return;
                }
                contextMenu.Update();
                if (contextMenu != null) {
                    if (contextMenu.enabled == false) {
                        contextMenu = null;
                    }
                }
            }
        }

        public static void Draw() {
            contextMenu?.Draw();
        }
    }
}
