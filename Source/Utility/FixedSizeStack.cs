using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker {

    /// <summary>
    /// A stack that supports a maximum capacity, and automatically forgets the oldest elements added to it should it go over capacity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizeStack<T> {
        private List<T> items = new List<T>();
        int capacity = -1;

        /// <summary>
        /// The maximum capacity of this stack.<br></br>
        /// Setting this will automatically forget any extra items.<br></br>
        /// Set to -1 for no size limit.
        /// </summary>
        public int Capacity {
            get { return capacity; }
            set { capacity = value; RemoveExtraItems(); }
        }

        /// <summary>
        /// Gets the number of elements contained in the stack
        /// </summary>
        public int Count { get { return items.Count; } }

        /// <summary>
        /// Creates a new FixedSizeStack with a maximum size of <c>capacity</c>
        /// </summary>
        /// <param name="capacity"></param>
        public FixedSizeStack(int capacity) {
            this.capacity = capacity;
            items = new List<T>();
        }

        /// <summary>
        /// Creates a new fixed size stack with no size limit, functionally the same as a stack
        /// </summary>
        public FixedSizeStack() {
            capacity = -1;
            items = new List<T>();
        }

        /// <summary>
        /// Pushes an item on top of the stack
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item) {
            items.Add(item);
        }

        /// <summary>
        /// Clears the stack
        /// </summary>
        public void Clear() {
            items.Clear();
        }

        /// <summary>
        /// Pops the topmost item on the stack
        /// </summary>
        /// <returns>The item that was popped</returns>
        public T Pop() {
            if (items.Count > 0) {
                T temp = items[0];
                items.RemoveAt(0);
                return temp;
            }
            else
                return default;
        }

        /// <summary>
        /// Looks at the topmost item on the stack, without removing it.
        /// </summary>
        /// <returns>The item at the top of the stack</returns>
        public T Peek() {
            if (items.Count > 0) {
                return items[0];
            }
            else
                return default;
        }

        private void RemoveExtraItems() {
            if (capacity < 0) return;
            while (items.Count > Capacity) {
                items.RemoveAt(items.Count - 1);
            }
        }
    }
}
