using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class CloseableTabGroup : Element {
        List<CloseableTab> tabs;
        public int CurrentTabIndex { get; set; }
        public CloseableTabGroup() {
            CurrentTabIndex = -1;
        }

        public void Update() {

        }

        void CalculateTabPositions() {
            int width = 0;
            for(int i = 0; i < tabs.Count; i++) {
                tabs[i].x = width;
                width += tabs[i].width + 1;
            }
        }

        public void Draw() {
            for (int i = 0; i < tabs.Count; i++) {
                tabs[i].Draw(CurrentTabIndex == i);
            }
        }
    }
}
