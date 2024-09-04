using System;
using System.Text;

namespace WaveTracker.Tracker {
    public class WTPattern {
        private byte[][] cells;

        public string CellsAsString { get; private set; }

        public bool IsDirty { get; set; }

        public WTSong ParentSong { get; set; }

        /// <summary>
        /// The width of this pattern in cells
        /// </summary>
        public int Width { get { return cells[0].Length; } }

        /// <summary>
        /// The height of this pattern in rows
        /// </summary>
        public int Height { get { return cells.Length; } }

        /// <summary>
        /// Returns true if this pattern is empty
        /// </summary>
        public bool IsEmpty {
            get {
                for (int row = 0; row < Height; ++row) {
                    for (int column = 0; column < Width; ++column) {
                        if (cells[row][column] != EVENT_EMPTY) {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Initializes a new pattern with all empty events
        /// </summary>
        /// <param name="parentSong"></param>
        public WTPattern(WTSong parentSong) {
            ParentSong = parentSong;
            cells = new byte[256][];
            for (int row = 0; row < Height; row++) {
                cells[row] = new byte[parentSong.ParentModule.ChannelCount * 11];
                for (int column = 0; column < cells[row].Length; ++column) {
                    cells[row][column] = EVENT_EMPTY;
                }
            }
            CellsAsString = GetCellDataAsString();
        }

        /// <summary>
        /// Returns a copy of this pattern
        /// </summary>
        /// <returns></returns>
        public WTPattern Clone() {
            WTPattern clone = new WTPattern(ParentSong);
            clone.ParentSong = ParentSong;
            for (int r = 0; r < Height; r++) {
                for (int c = 0; c < cells[r].Length; c++) {
                    clone.cells[r][c] = cells[r][c];
                }
            }
            clone.CellsAsString = CellsAsString;
            clone.IsDirty = IsDirty;
            return clone;
        }

        /// <summary>
        /// Gets the position playback will go to immediately after finishing this pattern
        /// </summary>
        /// <returns>-1 if stopping, otherwise returns the index of the frame that should play after this one</returns>
        public void GetNextPlaybackPosition(int currentFrame, out int nextFrame, out int nextRow) {
            int lastRow = GetModifiedLength() - 1;
            for (int channel = 0; channel < ParentSong.ParentModule.ChannelCount; ++channel) {
                for (int effectColumn = 0; effectColumn < ParentSong.NumEffectColumns[channel]; ++effectColumn) {
                    if ((char)this[lastRow, channel, CellType.Effect1 + effectColumn * 2] == 'C') {
                        nextFrame = -1;
                        nextRow = -1;
                        return;
                    }
                    if ((char)this[lastRow, channel, CellType.Effect1 + effectColumn * 2] == 'B') {
                        nextFrame = this[lastRow, channel, CellType.Effect1Parameter + effectColumn * 2];
                        nextRow = 0;
                        return;
                    }
                    if ((char)this[lastRow, channel, CellType.Effect1 + effectColumn * 2] == 'D') {
                        nextFrame = (currentFrame + 1) % ParentSong.FrameSequence.Count;
                        nextRow = this[lastRow, channel, CellType.Effect1Parameter + effectColumn * 2];
                        return;
                    }
                }
            }
            nextFrame = currentFrame + 1;
            if (nextFrame >= ParentSong.FrameSequence.Count) {
                nextFrame = 0;
            }

            nextRow = 0;
            return;
        }

        /// <summary>
        /// If this pattern had any changes since the last pack, will update CellsAsString to reflect the current pattern state.
        /// </summary>
        public void PackAnyChanges() {
            if (IsDirty) {
                CellsAsString = GetCellDataAsString();
            }

            IsDirty = false;
        }

        /// <summary>
        /// If this pattern had any changes since the last pack, will update CellsAsString to reflect the current pattern state.
        /// </summary>
        public void ForcePackAnyChanges() {
            CellsAsString = GetCellDataAsString();
        }

        /// <summary>
        /// Gets this pattern's data as an encoded string
        /// </summary>
        /// <returns></returns>
        public string GetCellDataAsString() {
            byte[] data = new byte[Height * Width];
            int index = 0;
            for (int column = 0; column < Width; ++column) {
                for (int row = 0; row < Height; ++row) {
                    data[index] = cells[row][column];
                    index++;
                }
            }
            return Encoding.Unicode.GetString(data);
        }

        /// <summary>
        /// Reads data from an encoded string
        /// </summary>
        /// <param name="cellData"></param>
        public void ReadCellDataFromString(string cellData) {
            if (cellData == "") {
                for (int row = 0; row < 256; ++row) {
                    cells[row] = new byte[ParentSong.ParentModule.ChannelCount * 11];
                    for (int column = 0; column < Width; ++column) {
                        cells[row][column] = WTPattern.EVENT_EMPTY;
                    }
                }
            }
            else {
                byte[] data = Encoding.Unicode.GetBytes(cellData);
                int i = 0;
                cells = new byte[256][];
                for (int column = 0; column < ParentSong.ParentModule.ChannelCount * 11; ++column) {
                    for (int row = 0; row < 256; ++row) {
                        if (cells[row] == null) {
                            cells[row] = new byte[ParentSong.ParentModule.ChannelCount * 11];
                        }

                        cells[row][column] = data[i];
                        ++i;
                    }
                }
            }
        }

        /// <summary>
        /// Resizes this pattern to hold the parent module's <c>ChannelCount</c> number of channels
        /// </summary>
        /// <param name="channelCount"></param>
        public void Resize() {
            for (int row = 0; row < Height; ++row) {
                byte[] resizedRow = new byte[ParentSong.ParentModule.ChannelCount * 11];
                for (int column = 0; column < resizedRow.Length; ++column) {
                    resizedRow[column] = column < cells[row].Length ? cells[row][column] : WTPattern.EVENT_EMPTY;
                }

                cells[row] = resizedRow;
            }
            IsDirty = true;
        }

        /// <summary>
        /// Sets cell at <c>[row, column]</c> to <c>value</c> without updating any dependent cells
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void SetCellRaw(int row, int column, byte value) {
            cells[row][column] = value;
        }
        /// <summary>
        /// Sets cell at <c>[row, channel, cellType]</c> to <c>value</c> without updating any dependent cells
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <param name="value"></param>
        public void SetCellRaw(int row, int channel, CellType cellType, byte value) {
            cells[row][channel * 11 + (int)cellType] = value;
        }

        /// <summary>
        /// Accesses a cell in this pattern, setting will automatically clamp/update dependent cells if needed
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[int row, int column] {
            get {
                return cells[row][column];
            }
            set {
                IsDirty = true;
                CellType cellType = (CellType)(column % 11);
                switch (cellType) {
                    case CellType.Note:
                        if (value is EVENT_NOTE_CUT or EVENT_NOTE_RELEASE or EVENT_EMPTY) {
                            cells[row][column] = (byte)value;
                            if (value is EVENT_NOTE_CUT or EVENT_NOTE_RELEASE) {
                                cells[row][column + 1] = EVENT_EMPTY;
                            }
                        }
                        else {
                            cells[row][column] = (byte)Math.Clamp(value, 12, 131);
                        }
                        break;

                    case CellType.Instrument:
                        cells[row][column] = value == EVENT_EMPTY || cells[row][column - 1] == EVENT_NOTE_CUT || cells[row][column - 1] == EVENT_NOTE_RELEASE
                            ? EVENT_EMPTY
                            : (byte)Math.Clamp(value, 0, 99);
                        break;

                    case CellType.Volume:
                        cells[row][column] = value == EVENT_EMPTY ? EVENT_EMPTY : (byte)Math.Clamp(value, 0, 99);
                        break;
                    case CellType.Effect1:
                    case CellType.Effect2:
                    case CellType.Effect3:
                    case CellType.Effect4:
                        if (cells[row][column] == EVENT_EMPTY) {
                            // if the effect was previously empty, intialize the parameter with the default value
                            cells[row][column + 1] = value is '8' or 'P' ? (byte)50 : (byte)0;
                        }
                        if (value == EVENT_EMPTY) {
                            // if the effect is being set to empty, set the parameter to empty too
                            cells[row][column + 1] = EVENT_EMPTY;
                        }
                        else if (!Helpers.IsEffectHex((char)value)) {
                            // if the effect is not hex, clamp the parameter in case it was higher than 99
                            cells[row][column + 1] = Math.Clamp(cells[row][column + 1], (byte)0, (byte)99);
                        }
                        cells[row][column] = (byte)value;
                        break;
                    case CellType.Effect1Parameter:
                    case CellType.Effect2Parameter:
                    case CellType.Effect3Parameter:
                    case CellType.Effect4Parameter:
                        cells[row][column] = Helpers.IsEffectHex((char)cells[row][column - 1]) ? (byte)Math.Clamp(value, 0, 255) : (byte)Math.Clamp(value, 0, 99);
                        break;
                }
            }
        }

        /// <summary>
        /// Accesses a cell by channel, setting will automatically clamp/update dependent cells if needed
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <returns></returns>
        public int this[int row, int channel, CellType cellType] {
            get {
                return cells[row][channel * 11 + (int)cellType];
            }
            set {
                int col = channel * 11 + (int)cellType;
                this[row, col] = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Returns true if the cell at the given location is empty or a note cut or note release
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <returns></returns>
        public bool CellIsEmptyOrNoteCutRelease(int row, int channel, CellType cellType) {
            return cellType == CellType.Note
                ? cells[row][channel * 11 + (int)CellType.Note] is WTPattern.EVENT_EMPTY or
                    WTPattern.EVENT_NOTE_RELEASE or
                    WTPattern.EVENT_NOTE_CUT
                : CellIsEmpty(row, channel, cellType);
        }
        /// <summary>
        /// Returns true if the cell at the given location is empty or a note cut or note release
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <returns></returns>
        public bool CellIsEmptyOrNoteCutRelease(int row, int column) {
            CellType cellType = (CellType)(column % 11);
            return cellType == CellType.Note
                ? cells[row][column] is WTPattern.EVENT_EMPTY or
                    WTPattern.EVENT_NOTE_RELEASE or
                    WTPattern.EVENT_NOTE_CUT
                : CellIsEmpty(row, column);
        }
        /// <summary>
        /// Returns true if the cell at the given location is empty
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <returns></returns>
        public bool CellIsEmpty(int row, int channel, CellType cellType) {
            return cellType == CellType.Effect1Parameter
                ? cells[row][channel * 11 + (int)CellType.Effect1] == WTPattern.EVENT_EMPTY
                : cellType == CellType.Effect2Parameter
                ? cells[row][channel * 11 + (int)CellType.Effect2] == WTPattern.EVENT_EMPTY
                : cellType == CellType.Effect3Parameter
                ? cells[row][channel * 11 + (int)CellType.Effect3] == WTPattern.EVENT_EMPTY
                : cellType == CellType.Effect4Parameter
                ? cells[row][channel * 11 + (int)CellType.Effect4] == WTPattern.EVENT_EMPTY
                : cells[row][channel * 11 + (int)cellType] == WTPattern.EVENT_EMPTY;
        }

        /// <summary>
        /// Returns true if the cell at the given location is empty
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <returns></returns>
        public bool CellIsEmpty(int row, int column) {
            CellType cellType = (CellType)(column % 11);
            return cellType == CellType.Effect1Parameter
                ? cells[row][column - 1] == WTPattern.EVENT_EMPTY
                : cellType == CellType.Effect2Parameter
                ? cells[row][column - 1] == WTPattern.EVENT_EMPTY
                : cellType == CellType.Effect3Parameter
                ? cells[row][column - 1] == WTPattern.EVENT_EMPTY
                : cellType == CellType.Effect4Parameter
                ? cells[row][column - 1] == WTPattern.EVENT_EMPTY
                : cells[row][column] == WTPattern.EVENT_EMPTY;
        }

        /// <summary>
        /// Gets the length of this pattern, taking into account effects that may cut it short. (Cxx, Dxx, and Bxx)
        /// </summary>
        public int GetModifiedLength() {
            for (int row = 0; row < ParentSong.RowsPerFrame; ++row) {
                for (int channel = 0; channel < ParentSong.ParentModule.ChannelCount; ++channel) {
                    for (int effectColumn = 0; effectColumn < ParentSong.NumEffectColumns[channel]; ++effectColumn) {
                        if ("CDB".Contains((char)this[row, channel, CellType.Effect1 + effectColumn * 2])) {
                            return row + 1;
                        }
                    }
                }
            }
            return ParentSong.RowsPerFrame;
        }

        /// <summary>
        /// Gets the cell type of a column in the pattern
        /// </summary>
        /// <param name="cellColumn"></param>
        /// <returns></returns>
        public static CellType GetCellTypeFromCellColumn(int cellColumn) {
            return (CellType)(cellColumn % 11);
        }

        /// <summary>
        /// The byte value reserved to denote an empty space in the pattern
        /// </summary>
        public const byte EVENT_EMPTY = 255;
        /// <summary>
        /// The byte value reserved to denote a note cut in a pattern
        /// </summary>
        public const byte EVENT_NOTE_CUT = 254;
        /// <summary>
        /// The byte value reserved to denote a note release in a pattern
        /// </summary>
        public const byte EVENT_NOTE_RELEASE = 253;
    }

    public enum CellType {
        Note,
        Instrument,
        Volume,
        Effect1,
        Effect1Parameter,
        Effect2,
        Effect2Parameter,
        Effect3,
        Effect3Parameter,
        Effect4,
        Effect4Parameter
    }
}
