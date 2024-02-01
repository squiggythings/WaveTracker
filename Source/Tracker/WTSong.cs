using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace WaveTracker.Tracker {
    [ProtoContract(SkipConstructor = true)]
    public class WTSong {

        public static WTSong currentSong;

        /// <summary>
        /// The name of this song
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// The song's speed or groove setting
        /// </summary>
        [ProtoMember(2)]
        public int[] TicksPerRow { get; set; }

        /// <summary>
        /// The length of each frame in this song
        /// </summary>
        [ProtoMember(3)]
        public int RowsPerFrame { get; set; }

        /// <summary>
        /// The bank of patterns available to use in sequencing
        /// </summary>
        [ProtoMember(4)]
        public WTPattern[] Patterns { get; private set; }

        /// <summary>
        /// The order of frames in this song
        /// </summary>
        [ProtoMember(5)]
        public List<WTFrame> FrameSequence { get; set; }

        /// <summary>
        /// The number of effect columns each channel has
        /// </summary>
        [ProtoMember(6)]
        public int[] NumEffectColumns { get; set; }

        /// <summary>
        /// Measure highlighting
        /// </summary>
        [ProtoMember(7)]
        public int RowHighlightPrimary { get; set; }

        /// <summary>
        /// Beat highlighting
        /// </summary>
        [ProtoMember(8)]
        public int RowHighlightSecondary { get; set; }

        [ProtoMember(9)]
        public int ChannelCount { get { return WTModule.NUM_CHANNELS; } }

        /// <summary>
        /// Initializes a new song with empty patterns and default settings
        /// </summary>
        public WTSong() {
            Patterns = new WTPattern[100];
            for (int i = 0; i < Patterns.Length; ++i) {
                Patterns[i] = new WTPattern(this);
            }
            NumEffectColumns = new int[WTModule.NUM_CHANNELS];
            for (int i = 0; i < NumEffectColumns.Length; ++i) {
                NumEffectColumns[i] = 1;
            }
            FrameSequence = new List<WTFrame>();
            FrameSequence.Add(new WTFrame(0, this));
            TicksPerRow = new int[] { 4 };
            RowsPerFrame = 16;
            RowHighlightPrimary = 16;
            RowHighlightSecondary = 4;
        }

        /// <summary>
        /// Appends a new frame at the end of the sequence using the next free pattern
        /// </summary>
        public void AddNewFrame() {
            if (FrameSequence.Count < 100)
                FrameSequence.Add(new WTFrame(GetNextFreePattern(), this));
        }

        /// <summary>
        /// Inserts a new frame at the end of the sequence using the next free pattern
        /// </summary>
        public void InsertNewFrame(int index) {
            if (FrameSequence.Count < 100)
                FrameSequence.Insert(index, new WTFrame(GetNextFreePattern(), this));
        }

        /// <summary>
        /// Inserts a frame that references the same pattern as the previous one
        /// </summary>
        /// <param name="index"></param>
        public void DuplicateFrame(int index) {
            if (FrameSequence.Count < 100)
                FrameSequence.Insert(index, FrameSequence[index]);
        }

        /// <summary>
        /// Removes a frame at the given index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveFrame(int index) {
            if (FrameSequence.Count > 2) {
                FrameSequence.RemoveAt(index);
            }
        }

        /// <summary>
        /// Swaps two frames in the list
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public void SwapFrames(int index1, int index2) {
            if (index1 >= 0 && index2 >= 0 && index1 < FrameSequence.Count && index2 < FrameSequence.Count) {
                (FrameSequence[index1], FrameSequence[index2]) = (FrameSequence[index2], FrameSequence[index1]);
            }
        }

        /// <summary>
        /// Gets the index of the next free pattern in the pattern bank
        /// </summary>
        /// <returns></returns>
        int GetNextFreePattern() {
            for (int i = 0; i < 100; ++i) {
                if (Patterns[i].IsEmpty)
                    return i;
            }
            return 99;
        }

        /// <summary>
        /// Returns the number of columns on a channel, accounting for effect expansions
        /// </summary>
        public int GetNumColumnsOfChannel(int channel) {
            // note 1 column
            // vol 2 columns
            // inst 2 columns
            // effect 3 columns
            return 5 + NumEffectColumns[channel] * 3;
        }

        /// <summary>
        /// Returns the ticks per row as a string separated by spaces
        /// </summary>
        /// <returns></returns>
        public string GetTicksAsString() {
            string ret = "";
            for (int i = 0; i < TicksPerRow.Length - 1; i++) {
                ret += TicksPerRow[i] + " ";
            }
            ret += TicksPerRow[TicksPerRow.Length - 1];
            return ret;
        }
        /// <summary>
        /// Sets the ticks per row from an input string separated by spaces
        /// </summary>
        /// <param name="text"></param>
        public void LoadTicksFromString(string text) {
            List<int> ticks = new List<int>();
            foreach (string word in text.Split(' ')) {
                if (IsStringANumber(word))
                    ticks.Add(int.Parse(word));
            }
            if (ticks.Count == 0)
                ticks.Add(1);
            TicksPerRow = new int[ticks.Count];
            int n = 0;
            foreach (int i in ticks) {
                TicksPerRow[n] = i;
                n++;
            }
        }

        /// <summary>
        /// Returns true if 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        bool IsStringANumber(string str) {
            foreach (char c in str) {
                if (!"0123456789".Contains(c))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Accesses the byte at the cursor position.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[CursorPos cursorPosition] {
            get {
                return Patterns[FrameSequence[cursorPosition.Frame].PatternIndex][cursorPosition.Row][cursorPosition.Channel][cursorPosition.GetColumnType()];
            }
            set {
                Patterns[FrameSequence[cursorPosition.Frame].PatternIndex][cursorPosition.Row][cursorPosition.Channel][cursorPosition.GetColumnType()] = value;
            }
        }

        /// <summary>
        /// Accesses the pattern at this song's frame, frame.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public WTPattern this[int frame] {
            get {
                return Patterns[FrameSequence[frame].PatternIndex];
            }
            set {
                Patterns[FrameSequence[frame].PatternIndex] = value;
            }
        }
    }
}
