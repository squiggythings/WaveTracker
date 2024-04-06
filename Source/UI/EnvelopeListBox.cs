using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class EnvelopeListBox : Clickable {

        public List<Envelope> list;
        public EnvelopeListItem[] items;
        public EnvelopeListBox(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            SetParent(parent);
            items = new EnvelopeListItem[8];
            for (int i = 0; i < items.Length; ++i) {
                items[i] = new EnvelopeListItem(1, 1 + i * 14, this);
            }
        }

        public void Update() {
            for (int i = 0; i < items.Length; ++i) {
                if (i < list.Count) {
                    items[i].Envelope = list[i];
                }
                else {
                    items[i].Envelope = null;
                }
                items[i].Update();
                if (items[i].WasClickedOnToSelect) {

                }
            }
        }

        public void Draw() {

        }
    }
}
