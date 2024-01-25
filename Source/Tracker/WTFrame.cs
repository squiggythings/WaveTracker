using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.Tracker {
    /// <summary>
    ///  Object in a song's frame sequence which determines what patterns to play.
    /// </summary>
    public struct WTFrame {

        /// <summary>
        /// The index of the pattern this frame should reference
        /// </summary>
        public int PatternIndex { get; set; }

        /// <summary>
        /// The song that owns this frame
        /// </summary>
        public WTSong Parent { get; private set; }

        /// <summary>
        /// Returns the pattern this frame references
        /// </summary>
        /// <returns></returns>
        public WTPattern GetPattern() {
            return Parent.Patterns[PatternIndex];
        }

        /// <summary>
        /// Gets the length of this frame's pattern, taking into account Cxx, Bxx and Dxx commands
        /// </summary>
        /// <returns></returns>
        public int GetLength() {
            return Parent.Patterns[PatternIndex].GetModifiedLength();
        }

        /// <summary>
        /// Creates a new frame that holds a pattern index and a reference to song that holds it.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        public WTFrame(int index, WTSong parent) {
            PatternIndex = index;
            Parent = parent;
        }
    }
}
