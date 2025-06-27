﻿using System;

namespace WaveTracker.Tracker {
    /// <summary>
    ///  Object in a song's frame sequence which determines what patterns to play.
    /// </summary>
    public class WTFrame {

        /// <summary>
        /// The index of the pattern this frame should reference
        /// </summary>
        private int patternIndex;

        /// <summary>
        /// The index of the pattern this frame should reference, automatically clamped between 0-99
        /// </summary>
        public int PatternIndex {
            get { return patternIndex; }
            set {
                if (patternIndex != value) {
                    patternIndex = Math.Clamp(value, 0, 99);
                    App.CurrentModule.SetDirty();
                }
            }
        }

        /// <summary>
        /// The song that owns this frame
        /// </summary>
        public WTSong ParentSong { get; private set; }

        /// <summary>
        /// Returns the pattern this frame references
        /// </summary>
        /// <returns></returns>
        public WTPattern GetPattern() {
            return ParentSong.Patterns[PatternIndex];
        }

        /// <summary>
        /// Gets the length of this frame's pattern, taking into account Cxx, Bxx and Dxx commands
        /// </summary>
        /// <returns></returns>
        public int GetLength() {
            return ParentSong.Patterns[PatternIndex].GetModifiedLength();
        }

        /// <summary>
        /// Creates a new frame that holds a pattern index and a reference to song that holds it.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        public WTFrame(int index, WTSong parent) {
            PatternIndex = index;
            ParentSong = parent;
        }
    }
}
