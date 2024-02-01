using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker {
    public class FixedSizeStack<T> {
        private List<T> items = new List<T>();
        int capacity = -1;
        public int Capacity {
            get { return capacity; }
            set { capacity = value; RemoveExtraItems(); }
        }

        public FixedSizeStack(int capacity) {
            this.capacity = capacity;
            items = new List<T>();
        }
        public FixedSizeStack() {
            capacity = -1;
            items = new List<T>();
        }

        public void Push(T item) {
            items.Add(item);
        }
        public T Pop() {
            if (items.Count > 0) {
                T temp = items[0];
                items.RemoveAt(0);
                return temp;
            }
            else
                return default;
        }
        void RemoveExtraItems() {
            if (capacity < 0) return;
            while (items.Count > Capacity) {
                items.RemoveAt(items.Count - 1);
            }
        }
    }
}
