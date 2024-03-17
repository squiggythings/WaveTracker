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
    public class WTPattern {

        byte[][] cells;

        public string CellsAsString { get; private set; }

        public bool IsDirty { get; private set; }

        public WTSong ParentSong { get; set; }

        /// <summary>
        /// The width of this pattern in cells
        /// </summary>
        public int Width { get { return cells[0].Length; } }

        /// <summary>
        /// Returns true if this pattern is empty
        /// </summary>
        public bool IsEmpty {
            get {
                for (int row = 0; row < cells.Length; ++row) {
                    for (int column = 0; column < cells[row].Length; ++column) {
                        if (cells[row][column] != EVENT_EMPTY) return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// If this pattern had any changes since the last pack, will update CellsAsString to reflect the current pattern state.
        /// </summary>
        public void PackAnyChanges() {
            if (IsDirty)
                CellsAsString = GetCellData();
            IsDirty = false;
        }


        public string GetCellData() {

            byte[] data = new byte[cells.Length * cells[0].Length];
            int index = 0;
            for (int column = 0; column < cells[0].Length; ++column) {
                for (int row = 0; row < cells.Length; ++row) {
                    data[index] = cells[row][column];
                    index++;
                }
            }
            return Encoding.Unicode.GetString(data);
        }

        public void ReadFromCellData(string cellData) {
            if (cellData == "") {
                for (int row = 0; row < 256; ++row) {
                    cells[row] = new byte[ParentSong.ParentModule.ChannelCount * 11];
                    for (int column = 0; column < cells[0].Length; ++column) {
                        cells[row][column] = WTPattern.EVENT_EMPTY;
                    }
                }
            }
            else {
                byte[] data = Encoding.Unicode.GetBytes(cellData);
                int i = 0;
                for (int column = 0; column < cells[0].Length; ++column) {
                    for (int row = 0; row < 256; ++row) {
                        if (cells[row] == null)
                            cells[row] = new byte[ParentSong.ParentModule.ChannelCount * 11];
                        cells[row][column] = data[i];
                        ++i;
                    }
                }
            }
        }

        /// <summary>
        /// Resizes this pattern to hold <c>channelCount</c> channels
        /// </summary>
        /// <param name="channelCount"></param>
        public void Resize() {
            for (int row = 0; row < cells.Length; ++row) {
                byte[] resizedRow = new byte[ParentSong.ParentModule.ChannelCount * 11];
                for (int column = 0; column < resizedRow.Length; ++column) {
                    if (column < cells[row].Length)
                        resizedRow[column] = cells[row][column];
                    else
                        resizedRow[column] = WTPattern.EVENT_EMPTY;
                }

                cells[row] = resizedRow;
            }
            IsDirty = true;
        }

        /// <summary>
        /// Initializes a new pattern with all empty events
        /// </summary>
        /// <param name="parentSong"></param>
        public WTPattern(WTSong parentSong) {
            ParentSong = parentSong;
            cells = new byte[256][];
            for (int row = 0; row < cells.Length; row++) {
                cells[row] = new byte[parentSong.ParentModule.ChannelCount * 11];
                for (int column = 0; column < cells[row].Length; ++column) {
                    cells[row][column] = EVENT_EMPTY;
                }
            }
            CellsAsString = GetCellData();
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
                        if (value == EVENT_NOTE_CUT || value == EVENT_NOTE_RELEASE || value == EVENT_EMPTY) {
                            cells[row][column] = (byte)value;
                            if (value == EVENT_NOTE_CUT || value == EVENT_NOTE_RELEASE)
                                cells[row][column + 1] = EVENT_EMPTY;
                        }
                        else {
                            cells[row][column] = (byte)Math.Clamp(value, 12, 131);
                        }
                        break;

                    case CellType.Instrument:
                        if (value == EVENT_EMPTY || cells[row][column - 1] == EVENT_NOTE_CUT || cells[row][column - 1] == EVENT_NOTE_RELEASE) {
                            cells[row][column] = EVENT_EMPTY;
                        }
                        else {
                            cells[row][column] = (byte)Math.Clamp(value, 0, 99);
                        }
                        break;

                    case CellType.Volume:
                        if (value == EVENT_EMPTY) {
                            cells[row][column] = EVENT_EMPTY;
                        }
                        else {
                            cells[row][column] = (byte)Math.Clamp(value, 0, 99);
                        }
                        break;
                    case CellType.Effect1:
                    case CellType.Effect2:
                    case CellType.Effect3:
                    case CellType.Effect4:
                        if (cells[row][column] == EVENT_EMPTY) {
                            // if the effect was previously empty, intialize the parameter with the default value
                            if (value == '8' || value == 'P')
                                cells[row][column + 1] = 50;
                            else
                                cells[row][column + 1] = 0;
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
                        if (Helpers.IsEffectHex((char)cells[row][column - 1])) {
                            cells[row][column] = (byte)Math.Clamp(value, 0, 255);
                        }
                        else {
                            cells[row][column] = (byte)Math.Clamp(value, 0, 99);
                        }
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
            if (cellType == CellType.Note)
                return cells[row][channel * 11 + (int)CellType.Note] == WTPattern.EVENT_EMPTY ||
                    cells[row][channel * 11 + (int)CellType.Note] == WTPattern.EVENT_NOTE_RELEASE ||
                    cells[row][channel * 11 + (int)CellType.Note] == WTPattern.EVENT_NOTE_CUT;
            else
                return CellIsEmpty(row, channel, cellType);
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
            if (cellType == CellType.Note)
                return cells[row][column] == WTPattern.EVENT_EMPTY ||
                    cells[row][column] == WTPattern.EVENT_NOTE_RELEASE ||
                    cells[row][column] == WTPattern.EVENT_NOTE_CUT;
            else
                return CellIsEmpty(row, column);
        }
        /// <summary>
        /// Returns true if the cell at the given location is empty
        /// </summary>
        /// <param name="row"></param>
        /// <param name="channel"></param>
        /// <param name="cellType"></param>
        /// <returns></returns>
        public bool CellIsEmpty(int row, int channel, CellType cellType) {
            if (cellType == CellType.Effect1Parameter)
                return cells[row][channel * 11 + (int)CellType.Effect1] == WTPattern.EVENT_EMPTY;
            if (cellType == CellType.Effect2Parameter)
                return cells[row][channel * 11 + (int)CellType.Effect2] == WTPattern.EVENT_EMPTY;
            if (cellType == CellType.Effect3Parameter)
                return cells[row][channel * 11 + (int)CellType.Effect3] == WTPattern.EVENT_EMPTY;
            if (cellType == CellType.Effect4Parameter)
                return cells[row][channel * 11 + (int)CellType.Effect4] == WTPattern.EVENT_EMPTY;
            return cells[row][channel * 11 + (int)cellType] == WTPattern.EVENT_EMPTY;
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
            if (cellType == CellType.Effect1Parameter)
                return cells[row][column - 1] == WTPattern.EVENT_EMPTY;
            if (cellType == CellType.Effect2Parameter)
                return cells[row][column - 1] == WTPattern.EVENT_EMPTY;
            if (cellType == CellType.Effect3Parameter)
                return cells[row][column - 1] == WTPattern.EVENT_EMPTY;
            if (cellType == CellType.Effect4Parameter)
                return cells[row][column - 1] == WTPattern.EVENT_EMPTY;
            return cells[row][column] == WTPattern.EVENT_EMPTY;
        }



        /// <summary>
        /// Gets the length of this pattern, taking into account effects that may cut it short. (Cxx, Dxx, and Bxx)
        /// </summary>
        public int GetModifiedLength() {
            for (int row = 0; row < ParentSong.RowsPerFrame; ++row) {
                for (int channel = 0; channel < ParentSong.ParentModule.ChannelCount; ++channel) {
                    for (int effectColumn = 0; effectColumn < ParentSong.NumEffectColumns[channel]; ++effectColumn) {
                        if ("CDB".Contains((char)this[row, channel, CellType.Effect1 + effectColumn * 2]))
                            return row + 1;
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

    ///// <summary>
    ///// A row of channel events
    ///// </summary>
    //[ProtoContract(SkipConstructor = true)]
    //public class PatternRow {
    //    [ProtoMember(1)]
    //    private PatternEvent[] channelEvents;
    //    private byte[] cells;

    //    /// <summary>
    //    /// Returns true if this row is empty
    //    /// </summary>
    //    public bool IsEmpty {
    //        get {
    //            foreach (PatternEvent e in channelEvents) {
    //                if (!e.IsEmpty)
    //                    return false;
    //            }
    //            return true;
    //        }
    //    }

    //    /// <summary>
    //    /// The number of pattern events in this row
    //    /// </summary>
    //    public int Length => channelEvents.Length;

    //    /// <summary>
    //    /// Initializes this row with the number of channels, numChannels
    //    /// </summary>
    //    /// <param name="numChannels"></param>
    //    public PatternRow(int numChannels) {
    //        channelEvents = new PatternEvent[numChannels];
    //        for (int i = 0; i < channelEvents.Length; i++) {
    //            channelEvents[i] = new PatternEvent();
    //        }
    //        cells = new byte[numChannels * 11];
    //        for (int i = 0; i < cells.Length; i++) {
    //            cells[i] = PatternEvent.EMPTY;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the PatternEvent stored at the channel.
    //    /// </summary>
    //    /// <param name="channel"></param>
    //    /// <returns></returns>
    //    /// <exception cref="IndexOutOfRangeException"></exception>
    //    public PatternEvent this[int channel] {
    //        get {
    //            return channelEvents[channel];
    //        }
    //        set {
    //            channelEvents[channel] = value;
    //        }
    //    }

    //    public string PackToString() {
    //        string rowAsString = "";
    //        foreach (PatternEvent e in channelEvents) {
    //            rowAsString += e.PackToString();
    //        }
    //        return rowAsString;
    //    }

    //}

    ///// <summary>
    ///// A single row in a channel track. Can contain commands for note, instrument, volume, and effects
    ///// </summary>
    //[ProtoContract(SkipConstructor = true)]
    //public class PatternEvent {
    //    /// <summary>
    //    /// The byte value reserved to denote an empty space in the pattern
    //    /// </summary>
    //    public const byte EMPTY = 255;
    //    /// <summary>
    //    /// The byte value reserved to denote a note cut in a pattern
    //    /// </summary>
    //    public const byte NOTE_CUT = 254;
    //    /// <summary>
    //    /// The byte value reserved to denote a note release in a pattern
    //    /// </summary>
    //    public const byte NOTE_RELEASE = 253;

    //    [ProtoMember(1)]
    //    private byte note;
    //    [ProtoMember(2)]
    //    private byte instrument;
    //    [ProtoMember(3)]
    //    private byte volume;
    //    [ProtoMember(4)]
    //    private Effect effect1;
    //    [ProtoMember(5)]
    //    private Effect effect2;
    //    [ProtoMember(6)]
    //    private Effect effect3;
    //    [ProtoMember(7)]
    //    private Effect effect4;

    //    /// <summary>
    //    /// Initializes the event with the default empty values
    //    /// </summary>
    //    public PatternEvent() {
    //        note = EMPTY;
    //        instrument = EMPTY;
    //        volume = EMPTY;
    //        effect1.Type = (char)EMPTY;
    //        effect1.Parameter = EMPTY;
    //        effect2.Type = (char)EMPTY;
    //        effect2.Parameter = EMPTY;
    //        effect3.Type = (char)EMPTY;
    //        effect3.Parameter = EMPTY;
    //        effect4.Type = (char)EMPTY;
    //        effect4.Parameter = EMPTY;
    //    }

    //    /// <summary>
    //    /// The note value as MIDI. Also contains cut and releases too.<br></br>
    //    /// min C-0 = 12, max: B-9 = 131
    //    /// </summary>
    //    public int Note {
    //        get { return note; }
    //        set {
    //            if (value == NOTE_CUT || value == NOTE_RELEASE || value == EMPTY) {
    //                note = (byte)value;
    //                if (value == NOTE_CUT || value == NOTE_RELEASE)
    //                    instrument = EMPTY;
    //            }
    //            else {
    //                note = (byte)Math.Clamp(value, 12, 131);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Index of the instrument to use from 00-99
    //    /// </summary>
    //    public int Instrument {
    //        get { return instrument; }
    //        set {
    //            if (note == NOTE_CUT || note == NOTE_RELEASE)
    //                instrument = EMPTY;
    //            else if (value == EMPTY) {
    //                instrument = (byte)value;
    //            }
    //            else {
    //                instrument = (byte)Math.Clamp(value, 0, 99);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Volume value from 00-99
    //    /// </summary>
    //    public int Volume {
    //        get { return volume; }
    //        set {
    //            if (value == EMPTY) {
    //                volume = (byte)value;
    //            }
    //            else {
    //                volume = (byte)Math.Clamp(value, 0, 99);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// The effect in the first effects column
    //    /// </summary>
    //    public Effect Effect1 { get { return effect1; } set { effect1 = value; } }

    //    /// <summary>
    //    /// The effect in the second effects column
    //    /// </summary>
    //    public Effect Effect2 { get { return effect2; } set { effect2 = value; } }

    //    /// <summary>
    //    /// The effect in the third effects column
    //    /// </summary>
    //    public Effect Effect3 { get { return effect3; } set { effect3 = value; } }

    //    /// <summary>
    //    /// The effect in the fourth effects column
    //    /// </summary>
    //    public Effect Effect4 { get { return effect4; } set { effect4 = value; } }


    //    /// <summary>
    //    /// Returns the value of the property at column. <br></br>
    //    /// (Double digit values, [vol, inst, effect parameters] are treated as 1 value)
    //    /// </summary>
    //    /// <param name="column"></param>
    //    /// <returns></returns>
    //    /// <exception cref="IndexOutOfRangeException"></exception>
    //    public int this[int column] {
    //        get {
    //            return column switch {
    //                0 => Note,
    //                1 => Instrument,
    //                2 => Volume,
    //                3 => (int)Effect1.Type,
    //                4 => Effect1.Parameter,
    //                5 => (int)Effect2.Type,
    //                6 => Effect2.Parameter,
    //                7 => (int)Effect3.Type,
    //                8 => Effect3.Parameter,
    //                9 => (int)Effect4.Type,
    //                10 => Effect4.Parameter,
    //                _ => throw new IndexOutOfRangeException(),
    //            };
    //        }
    //        set {
    //            switch (column) {
    //                case 0: Note = value; break;
    //                case 1: Instrument = value; break;
    //                case 2: Volume = value; break;
    //                case 3: effect1.Type = (char)value; break;
    //                case 4: effect1.Parameter = value; break;
    //                case 5: effect2.Type = (char)value; break;
    //                case 6: effect2.Parameter = value; break;
    //                case 7: effect3.Type = (char)value; break;
    //                case 8: effect3.Parameter = value; break;
    //                case 9: effect4.Type = (char)value; break;
    //                case 10: effect4.Parameter = value; break;
    //                default: throw new IndexOutOfRangeException();
    //            }
    //        }
    //    }
    //    /// <summary>
    //    /// Returns the value of the property at column. <br></br>
    //    /// (Double digit values, [vol, inst, effect parameters] are treated as 1 value)
    //    /// </summary>
    //    /// <param name="column"></param>
    //    /// <returns></returns>
    //    /// <exception cref="IndexOutOfRangeException"></exception>
    //    public int this[CursorColumnType columnType] {
    //        get {
    //            return (int)columnType switch {
    //                0 => Note,
    //                1 => Instrument,
    //                2 => Volume,
    //                3 => (int)Effect1.Type,
    //                4 => Effect1.Parameter,
    //                5 => (int)Effect2.Type,
    //                6 => Effect2.Parameter,
    //                7 => (int)Effect3.Type,
    //                8 => Effect3.Parameter,
    //                9 => (int)Effect4.Type,
    //                10 => Effect4.Parameter,
    //                _ => throw new IndexOutOfRangeException(),
    //            };
    //        }
    //        set {
    //            switch ((int)columnType) {
    //                case 0: Note = value; break;
    //                case 1: Instrument = value; break;
    //                case 2: Volume = value; break;
    //                case 3: effect1.Type = (char)value; break;
    //                case 4: effect1.Parameter = value; break;
    //                case 5: effect2.Type = (char)value; break;
    //                case 6: effect2.Parameter = value; break;
    //                case 7: effect3.Type = (char)value; break;
    //                case 8: effect3.Parameter = value; break;
    //                case 9: effect4.Type = (char)value; break;
    //                case 10: effect4.Parameter = value; break;
    //                default: throw new IndexOutOfRangeException();
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Returns true if this event is empty
    //    /// </summary>
    //    public bool IsEmpty {
    //        get {
    //            if (Note == EMPTY)
    //                if (Instrument == EMPTY)
    //                    if (Volume == EMPTY)
    //                        if (Effect1.Type == EMPTY)
    //                            if (Effect2.Type == EMPTY)
    //                                if (Effect3.Type == EMPTY)
    //                                    if (Effect4.Type == EMPTY)
    //                                        return true;
    //            return false;
    //        }
    //    }
    //    public void SetEmpty() {
    //        note = EMPTY;
    //        instrument = EMPTY;
    //        volume = EMPTY;
    //        effect1.Type = (char)EMPTY;
    //        effect1.Parameter = EMPTY;
    //        effect2.Type = (char)EMPTY;
    //        effect2.Parameter = EMPTY;
    //        effect3.Type = (char)EMPTY;
    //        effect3.Parameter = EMPTY;
    //        effect4.Type = (char)EMPTY;
    //        effect4.Parameter = EMPTY;
    //    }

    //    public void CopyFrom(PatternEvent other) {
    //        note = other.note;
    //        instrument = other.instrument;
    //        volume = other.volume;
    //        Effect1 = other.Effect1;
    //        Effect2 = other.Effect2;
    //        Effect3 = other.Effect3;
    //        Effect4 = other.Effect4;
    //    }

    //    /// <summary>
    //    /// Returns the specific effect, effectNum, under this event
    //    /// </summary>
    //    /// <param name="effectNum"></param>
    //    /// <returns></returns>
    //    public Effect GetEffect(int effectNum) {
    //        return effectNum switch {
    //            0 => Effect1,
    //            1 => Effect2,
    //            2 => Effect3,
    //            3 => Effect4,
    //            _ => throw new IndexOutOfRangeException(),
    //        };
    //    }


    //    public string PackToString() {
    //        // 11 chars long
    //        return (char)note + (char)instrument + (char)volume + Effect1.PackToString() + Effect2.PackToString() + Effect3.PackToString() + Effect4.PackToString();
    //    }
    //}

    ///// <summary>
    ///// Stores an effect, with a type and parameter value.
    ///// </summary>
    //[ProtoContract(SkipConstructor = true)]
    //public struct Effect {

    //    [ProtoMember(1)]
    //    char type;
    //    [ProtoMember(2)]
    //    byte parameter;

    //    /// <summary>
    //    /// The name of the effect as a character.<br></br>
    //    /// Setting this will automatically intialize the parameter, if needed
    //    /// </summary>
    //    public char Type {
    //        get { return type; }
    //        set {
    //            // if this effect was previously empty, initialize the effect with default values
    //            if (type == PatternEvent.EMPTY) {
    //                Parameter = value switch {
    //                    // pan and detune are intialized to center 
    //                    '8' or 'P' => 50,
    //                    _ => 0,
    //                }; ;
    //            }

    //            type = value;

    //            // if we are setting this effect to empty, make the parameter empty too
    //            if (value == PatternEvent.EMPTY) {
    //                parameter = PatternEvent.EMPTY;
    //            }
    //            else {
    //                // clamps parameter to make sure it's within bounds if switching from hex to decimal
    //                Parameter += 0;
    //            }
    //        }
    //    }

    //    public Effect() {
    //        type = (char)PatternEvent.EMPTY;
    //        parameter = PatternEvent.EMPTY;
    //    }

    //    public Effect(char type, byte parameter) {
    //        this.type = type;
    //        this.parameter = parameter;
    //    }

    //    /// <summary>
    //    /// The parameter for this effect, information on how the effect should be applied
    //    /// </summary>
    //    public int Parameter {
    //        get { return parameter; }
    //        set {
    //            if (ParameterIsHex) {
    //                parameter = (byte)value;
    //            }
    //            else {
    //                parameter = (byte)Math.Clamp(value, 0, 99);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Returns true if this parameter is dual, _xy instead of _xx. <br></br>
    //    /// This means the parameter should operate in hexadecimal for greater range per digit.
    //    /// </summary>
    //    public bool ParameterIsHex {
    //        get {
    //            return IsTypeHex(Type);
    //        }
    //    }

    //    bool IsTypeHex(char type) {

    //        return type switch {
    //            '0' or '4' or '7' or 'Q' or 'R' => true,
    //            _ => false,
    //        };

    //    }

    //    /// <summary>
    //    /// Returns the parameter value as a 2 character string
    //    /// </summary>
    //    /// <returns></returns>
    //    public string GetParameterAsString() {
    //        if (type == PatternEvent.EMPTY) {
    //            return "··";
    //        }
    //        else if (ParameterIsHex) {
    //            return parameter.ToString("X2");
    //        }
    //        else {
    //            return parameter.ToString("D2");
    //        }
    //    }

    //    public string PackToString() {
    //        return "" + type + (char)Parameter;
    //    }

    //    /* Effects Cheatsheet
    //     * 
    //     * 0xy - arpeggio
    //     * 1xx - rise
    //     * 2xx - fall
    //     * 3xx - portamento
    //     * 4xy - vibrato
    //     * 7xy - tremolo
    //     * 8xx - pan
    //     * 9xx - stereo phase offset
    //     * 
    //     * Axx - volume fade down
    //     * Wxx - volume fade up
    //     * 
    //     * Gxx - delay row
    //     * 
    //     * Qxy - note bend up
    //     * Rxy - note bend down
    //     * Pxx - pitch
    //     * 
    //     * Bxx - loop to frame #
    //     * Cxx - stop song
    //     * Dxx - skip # frames
    //     * Fxx - speed change
    //     * 
    //     * Sxx - cut
    //     * Lxx - release
    //     * 
    //     * Vxx - set wave
    //     * Xxx - blend wave
    //     * Zxx - stretch wave
    //     * Mxx - fm
    //     */
    //}
}
