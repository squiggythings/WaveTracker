using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.Diagnostics;

namespace WaveTracker.Tracker {
    /// <summary>
    /// 
    /// </summary>
    public record PatternEditorState {

        string[] patternStrings;
        /// <summary>
        /// The position before this action was committed
        /// </summary>
        public PatternEditorPosition PrePosition { get; private set; }
        /// <summary>
        /// The position after this action was committed
        /// </summary>
        public PatternEditorPosition PostPosition { get; private set; }

        /// <summary>
        /// The song's frameSequence
        /// </summary>
        string frameSequence;

        public PatternEditorState(WTSong song, PatternEditorPosition previous, PatternEditorPosition next) {
            patternStrings = song.PackPatternsToStrings();
            frameSequence = song.GetFrameSequenceAsString();
            PrePosition = previous;
            PostPosition = next;
        }

        /// <summary>
        /// Writes this state's data into the <c>WTSong</c> passed into this function
        /// </summary>
        /// <param name="song"></param>
        public void RestoreIntoSong(WTSong song) {
            song.UnpackFrameSequence(frameSequence);
            song.UnpackPatternsFromStrings(patternStrings);
        }
    }
}