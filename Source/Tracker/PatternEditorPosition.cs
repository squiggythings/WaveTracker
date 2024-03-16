using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Tracker {
    /// <summary>
    /// Stores a snapshot of cursor positions and selection information
    /// </summary>
    public struct PatternEditorPosition {
        public CursorPos CursorPosition { get; private set; }
        public CursorPos SelectionStart { get; private set; }
        public CursorPos SelectionEnd { get; private set; }
        public bool SelectionIsActive { get; private set; }

        public PatternEditorPosition(CursorPos cursorPosition, PatternSelection patternSelection) {
            CursorPosition = cursorPosition;
            SelectionStart = patternSelection.min;
            SelectionEnd = patternSelection.max;
            SelectionIsActive = patternSelection.IsActive;
        }
    }
}
