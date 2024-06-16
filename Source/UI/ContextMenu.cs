using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public static class ContextMenu {
        static Menu contextMenu;

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
            Rendering.Graphics.Write(contextMenu + "", 2, 22, Color.Red);

            if (contextMenu != null) {
                contextMenu.Draw();
            }
        }
    }
}
