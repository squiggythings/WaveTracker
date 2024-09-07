using ProtoBuf;
using System;
using System.Collections.Generic;

namespace WaveTracker.Tracker {
    [ProtoContract(SkipConstructor = true)]
    public class WTSong {

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
        public WTPattern[] Patterns { get; private set; }

        /// <summary>
        /// An array of strings containing the data for all patterns in the module
        /// </summary>
        [ProtoMember(4)]
        private string[] patternsAsStrings;

        /// <summary>
        /// The order of frames in this song
        /// </summary>
        public List<WTFrame> FrameSequence { get; set; }

        /// <summary>
        /// A byte array containing the frame-pattern sequence
        /// </summary>
        [ProtoMember(5)]
        private byte[] frameSequence;

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

        /// <summary>
        /// The module that this song belongs to
        /// </summary>
        public WTModule ParentModule { get; set; }

        /// <summary>
        /// Initializes a new song with empty patterns and default settings
        /// </summary>
        public WTSong(WTModule parentModule) {
            Name = "New Song";
            ParentModule = parentModule;
            Patterns = new WTPattern[100];
            for (int i = 0; i < Patterns.Length; ++i) {
                Patterns[i] = new WTPattern(this);
            }
            NumEffectColumns = new int[ParentModule.ChannelCount];
            for (int i = 0; i < NumEffectColumns.Length; ++i) {
                NumEffectColumns[i] = 1;
            }
            FrameSequence = [
                new WTFrame(0, this)
            ];
            TicksPerRow = new int[] { 4 };
            LoadTicksFromString(App.Settings.Files.DefaultTicksPerRow);
            RowsPerFrame = App.Settings.Files.DefaultRowsPerFrame;
            RowHighlightPrimary = App.Settings.Files.DefaultRowPrimaryHighlight;
            RowHighlightSecondary = App.Settings.Files.DefaultRowSecondaryHighlight;
        }

        /// <summary>
        /// Returns a copy of this song
        /// </summary>
        /// <returns></returns>
        public WTSong Clone() {
            WTSong clone = new WTSong(ParentModule);
            clone.Name = Name + " Copy";
            clone.UnpackFrameSequence(GetFrameSequenceAsByteArray());
            int i = 0;
            foreach (WTPattern pattern in Patterns) {
                clone.Patterns[i] = pattern.Clone();
                i++;
            }
            Array.Copy(TicksPerRow, clone.TicksPerRow, TicksPerRow.Length);
            Array.Copy(NumEffectColumns, clone.NumEffectColumns, NumEffectColumns.Length);
            clone.RowsPerFrame = RowsPerFrame;
            clone.RowHighlightPrimary = RowHighlightPrimary;
            clone.RowHighlightSecondary = RowHighlightSecondary;
            return clone;
        }

        /// <summary>
        /// Resizes this song's patterns to fit the parent module's channel count
        /// </summary>
        public void ResizeChannelCount() {
            int[] oldArray = NumEffectColumns;
            NumEffectColumns = new int[ParentModule.ChannelCount];

            for (int i = 0; i < NumEffectColumns.Length; ++i) {
                NumEffectColumns[i] = i < oldArray.Length ? oldArray[i] : 1;
            }
            foreach (WTPattern pattern in Patterns) {
                pattern.Resize();
            }
        }

        [ProtoBeforeSerialization]
        internal void BeforeSerialized() {
            SetPatternsDirty();
            patternsAsStrings = PackPatternsToStrings();
            frameSequence = GetFrameSequenceAsByteArray();
            return;

        }
        public void AfterDeserialized(WTModule parent) {
            ParentModule = parent;
            UnpackFrameSequence(frameSequence);
            UnpackPatternsFromStrings(patternsAsStrings);
        }

        /// <summary>
        /// Appends a new frame at the end of the sequence using the next free pattern
        /// </summary>
        public void AddNewFrame() {
            if (FrameSequence.Count < 100) {
                FrameSequence.Add(new WTFrame(GetNextFreePattern(), this));
                App.PatternEditor.AddToUndoHistory();
            }
            App.CurrentModule.SetDirty();
        }

        /// <summary>
        /// Inserts a new frame in the sequence using the next free pattern
        /// </summary>
        public void InsertNewFrame(int index) {
            if (FrameSequence.Count < 100) {
                FrameSequence.Insert(index, new WTFrame(GetNextFreePattern(), this));
                App.PatternEditor.AddToUndoHistory();
            }
            App.CurrentModule.SetDirty();
        }

        /// <summary>
        /// Inserts a frame that references the same pattern as the previous one
        /// </summary>
        /// <param name="index"></param>
        public void DuplicateFrame(int index) {
            if (FrameSequence.Count < 100) {
                FrameSequence.Insert(index, new WTFrame(FrameSequence[index].PatternIndex, this));
                App.PatternEditor.AddToUndoHistory();
            }
            App.CurrentModule.SetDirty();
        }

        /// <summary>
        /// Removes a frame at the given index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveFrame(int index) {
            if (FrameSequence.Count > 1) {
                FrameSequence.RemoveAt(index);
                App.PatternEditor.AddToUndoHistory();
            }
            App.CurrentModule.SetDirty();
        }

        /// <summary>
        /// Swaps two frames in the list
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public void SwapFrames(int index1, int index2) {
            if (index1 >= 0 && index2 >= 0 && index1 < FrameSequence.Count && index2 < FrameSequence.Count) {
                (FrameSequence[index1], FrameSequence[index2]) = (FrameSequence[index2], FrameSequence[index1]);
                App.PatternEditor.AddToUndoHistory();
            }
            App.CurrentModule.SetDirty();
        }

        /// <summary>
        /// Copies a frames pattern into an empty pattern and updates the frame's pattern index to the newly copied one.
        /// </summary>
        /// <param name="index"></param>
        public void MakeFrameUnique(int index) {
            int newPatternIndex = GetNextFreePattern();
            WTPattern newPattern = Patterns[newPatternIndex];
            WTPattern currentPattern = FrameSequence[index].GetPattern();
            for (int row = 0; row < currentPattern.Height; ++row) {
                for (int column = 0; column < currentPattern.Width; ++column) {
                    newPattern[row, column] = currentPattern[row, column];
                }
            }
            FrameSequence[index].PatternIndex = newPatternIndex;
            App.PatternEditor.AddToUndoHistory();
            App.CurrentModule.SetDirty();
        }

        /// <summary>
        /// Gets the index of the next free pattern in the pattern bank
        /// </summary>
        /// <returns></returns>
        private int GetNextFreePattern() {
            for (int i = 0; i < 100; ++i) {
                if (Patterns[i].IsEmpty) {
                    bool containedInSongAlready = false;
                    foreach (WTFrame frame in FrameSequence) {
                        if (frame.PatternIndex == i) {
                            containedInSongAlready = true;
                        }
                    }
                    if (!containedInSongAlready) {
                        return i;
                    }
                }
            }
            return 99;
        }

        public bool HasFreePattern() {
            for (int i = 0; i < 100; ++i) {
                if (Patterns[i].IsEmpty) {
                    bool containedInSongAlready = false;
                    foreach (WTFrame frame in FrameSequence) {
                        if (frame.PatternIndex == i) {
                            containedInSongAlready = true;
                        }
                    }
                    if (!containedInSongAlready) {
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetPatternsDirty() {
            foreach (WTPattern pattern in Patterns) {
                pattern.IsDirty = true;
            }
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
            List<int> ticks = [];
            foreach (string word in text.Split(' ')) {
                if (word.IsNumeric()) {
                    if (int.TryParse(word, out int val)) {
                        ticks.Add(val);
                    }
                }
            }
            if (ticks.Count == 0) {
                ticks.Add(1);
            }

            TicksPerRow = new int[ticks.Count];
            int n = 0;
            foreach (int i in ticks) {
                TicksPerRow[n] = i;
                n++;
            }
        }

        /// <summary>
        /// Returns the number of file columns a channel track has.
        /// <br></br>File columns: [Note, Inst, Vol, FX1, FX1param, FX2, FX2 param, ...]
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public int GetNumColumns(int channel) {
            return 3 + NumEffectColumns[channel] * 2;
        }

        /// <summary>
        /// Returns the number of cursor columns a channel track has.
        /// <br></br>Cursor columns: [Note, Inst1, Inst2, Vol1, Vol2, FX1, FX1param1 FX1param2, ...]
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public int GetNumCursorColumns(int channel) {
            return 5 + NumEffectColumns[channel] * 3;
        }

        /// <summary>
        /// Returns the last cursor column type in a channel (will always be an effectparam2)
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public CursorColumnType GetLastCursorColumnOfChannel(int channel) {
            return (CursorColumnType)(4 + NumEffectColumns[channel] * 3);
        }

        /// <summary>
        /// Accesses the byte at a cursor position.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[CursorPos cursorPosition] {
            get {
                return Patterns[FrameSequence[cursorPosition.Frame].PatternIndex][cursorPosition.Row, cursorPosition.Channel, cursorPosition.Column.ToCellType()];
            }
            set {
                Patterns[FrameSequence[cursorPosition.Frame].PatternIndex][cursorPosition.Row, cursorPosition.Channel, cursorPosition.Column.ToCellType()] = value;
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

        /// <summary>
        /// Returns an array of 100 strings containing each pattern's data
        /// </summary>
        /// <returns></returns>
        public string[] PackPatternsToStrings() {
            string[] patternData = new string[100];
            int i = 0;
            foreach (WTPattern pattern in Patterns) {
                if (pattern.IsEmpty) {
                    patternData[i] = "";
                }
                else {
                    pattern.PackAnyChanges();
                    patternData[i] = Helpers.RLECompress(pattern.CellsAsString);
                }
                ++i;
            }
            return patternData;
        }
        /// <summary>
        /// Returns an array of 100 strings containing each pattern's data
        /// </summary>
        /// <returns></returns>
        public string[] ForcePackPatternsToStrings() {
            string[] patternData = new string[100];
            int i = 0;
            foreach (WTPattern pattern in Patterns) {
                if (pattern.IsEmpty) {
                    patternData[i] = "";
                }
                else {
                    pattern.ForcePackAnyChanges();
                    patternData[i] = Helpers.RLECompress(pattern.CellsAsString);
                }
                ++i;
            }
            return patternData;
        }

        /// <summary>
        /// Returns the title of this song
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Name;
        }

        public int GetNumberOfRows(int loops) {
            int frame = 0;
            int row = 0;
            int rows = 0;
            while (loops > 0) {
                while (row < FrameSequence[frame].GetLength()) {
                    rows++;
                    row++;
                }
                this[frame].GetNextPlaybackPosition(frame, out int nextFrame, out int nextRow);
                if (nextFrame <= frame) {
                    loops--;
                }
                frame = nextFrame;
                row = nextRow;
                if (frame < 0) { // CXX command
                    break;
                }
            }
            return rows;
        }

        public void UnpackPatternsFromStrings(string[] packedStrings) {
            Patterns = new WTPattern[100];
            for (int i = 0; i < Patterns.Length; ++i) {
                Patterns[i] = new WTPattern(this);
                Patterns[i].ReadCellDataFromString(Helpers.RLEDecompress(packedStrings[i]));
            }
        }

        /// <summary>
        /// Returns a string containing the frame sequence data, each char is that frame's pattern index.
        /// </summary>
        /// <returns></returns>
        public byte[] GetFrameSequenceAsByteArray() {
            byte[] frameSequence = new byte[FrameSequence.Count];
            int i = 0;
            foreach (WTFrame frame in FrameSequence) {
                frameSequence[i] = (byte)frame.PatternIndex;
                ++i;
            }
            return frameSequence;
        }

        /// <summary>
        /// Sets this song's frame sequence from an input array where each byte is a pattern index
        /// </summary>
        /// <param name="frameSequence"></param>
        public void UnpackFrameSequence(byte[] frameSequence) {
            FrameSequence = [];
            foreach (byte index in frameSequence) {
                FrameSequence.Add(new WTFrame(index, this));
            }
        }
    }
}
