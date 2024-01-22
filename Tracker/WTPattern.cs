using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.Tracker {
    public class WTPattern {
        PatternRow[] rows;
        WTSong parent;

        /// <summary>
        /// Returns true if this pattern is empty
        /// </summary>
        public bool IsEmpty {
            get {
                foreach (PatternRow row in rows) {
                    if (!row.IsEmpty) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Initializes a new pattern with all blank events
        /// </summary>
        /// <param name="parent"></param>
        public WTPattern(WTSong parent) {
            this.parent = parent;
            rows = new PatternRow[256];
            for (int i = 0; i < rows.Length; ++i) {
                rows[i] = new PatternRow(parent.ChannelCount);
            }
        }

        /// <summary>
        /// Gets a row of events in this pattern
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public PatternRow this[int row] {
            get {
                return rows[row];
            }
        }

        public void SetValue(int row, int channel, int column, byte value) {

        }

        /// <summary>
        /// Gets the length of this frame, taking into account effects that cut this pattern short. (Cxx, Dxx, and Bxx)
        /// </summary>
        public int GetModifiedLength() {
            for (int r = 0; r < parent.RowsPerFrame; r++) {
                for (int c = 0; c < rows[r].Length; ++c) {
                    for (int i = 0; i < parent.NumEffectColumns[c]; ++i) {

                        // start at column index 3, the first effect column
                        // skip by 2 to the next effect column (jump over the effect parameters)
                        char effectType = (char)rows[r][c][3 + i * 2];
                        if (effectType == 'C' || effectType == 'D' || effectType == 'B')
                            return r + 1; // we've found an ending effect, return the length of this span
                    }
                }
            }
            // if no ending effect return the complete length
            return parent.RowsPerFrame;
        }
    }

    /// <summary>
    /// A row of channel events
    /// </summary>
    public class PatternRow {
        private PatternEvent[] channelEvents;

        /// <summary>
        /// Returns true if this row is empty
        /// </summary>
        public bool IsEmpty {
            get {
                foreach (PatternEvent e in channelEvents) {
                    if (!e.IsEmpty)
                        return false;
                }
                return true;
            }
        }

        public int Length => channelEvents.Length;

        /// <summary>
        /// Initializes this row with the number of channels, numChannels
        /// </summary>
        /// <param name="numChannels"></param>
        public PatternRow(int numChannels) {
            channelEvents = new PatternEvent[numChannels];
            for (int i = 0; i < channelEvents.Length; i++) {
                channelEvents[i] = new PatternEvent();
            }
        }

        /// <summary>
        /// Gets the PatternEvent stored at the channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public PatternEvent this[int channel] {
            get {
                return channelEvents[channel];
            }
            set {
                channelEvents[channel] = value;
            }
        }

    }

    /// <summary>
    /// A single row in a channel track. Can contain commands for note, instrument, volume, and effects
    /// </summary>
    public class PatternEvent {
        /// <summary>
        /// The byte value reserved to mean an empty space in the pattern
        /// </summary>
        public const byte EMPTY = 255;
        /// <summary>
        /// The byte value reserved to mean a note cut in a pattern
        /// </summary>
        public const byte NOTE_CUT = 254;
        /// <summary>
        /// The byte value reserved to mean a note release in a pattern
        /// </summary>
        public const byte NOTE_RELEASE = 253;

        private byte note;
        private byte instrument;
        private byte volume;
        private Effect effect1;
        private Effect effect2;
        private Effect effect3;
        private Effect effect4;

        /// <summary>
        /// Initializes the event with the default empty values
        /// </summary>
        public PatternEvent() {
            note = EMPTY;
            instrument = EMPTY;
            volume = EMPTY;
            effect1.Type = (char)EMPTY;
            effect1.Parameter = EMPTY;
            effect2.Type = (char)EMPTY;
            effect2.Parameter = EMPTY;
            effect3.Type = (char)EMPTY;
            effect3.Parameter = EMPTY;
            effect4.Type = (char)EMPTY;
            effect4.Parameter = EMPTY;
        }

        /// <summary>
        /// The note value as MIDI. Also contains cut and releases too.<br></br>
        /// min C-0 = 12, max: B-9 = 131
        /// </summary>
        public int Note {
            get { return note; }
            set {
                if (value == NOTE_CUT || value == NOTE_RELEASE || value == EMPTY) {
                    note = (byte)value;
                    if (value == NOTE_CUT || value == NOTE_RELEASE)
                        instrument = EMPTY;
                }
                else {
                    note = (byte)Math.Clamp(value, 12, 131);
                }
            }
        }

        /// <summary>
        /// Index of the instrument to use from 00-99
        /// </summary>
        public int Instrument {
            get { return instrument; }
            set {
                if (note == NOTE_CUT || note == NOTE_RELEASE)
                    instrument = EMPTY;
                else if (value == EMPTY) {
                    instrument = (byte)value;
                }
                else {
                    instrument = (byte)Math.Clamp(value, 0, 99);
                }
            }
        }

        /// <summary>
        /// Volume value from 00-99
        /// </summary>
        public int Volume {
            get { return volume; }
            set {
                if (value == EMPTY) {
                    volume = (byte)value;
                }
                else {
                    volume = (byte)Math.Clamp(value, 0, 99);
                }
            }
        }

        /// <summary>
        /// The effect in the first effects column
        /// </summary>
        public Effect Effect1 { get { return effect1; } set { effect1 = value; } }

        /// <summary>
        /// The effect in the second effects column
        /// </summary>
        public Effect Effect2 { get { return effect2; } set { effect2 = value; } }

        /// <summary>
        /// The effect in the third effects column
        /// </summary>
        public Effect Effect3 { get { return effect3; } set { effect3 = value; } }

        /// <summary>
        /// The effect in the fourth effects column
        /// </summary>
        public Effect Effect4 { get { return effect4; } set { effect4 = value; } }

        /// <summary>
        /// Converts a cursor column position to an event column position
        /// </summary>
        /// <param name="cursorColumn"></param>
        /// <returns></returns>
        public static int CursorColumnToEventColumn(int cursorColumn) {
            return cursorColumn switch {
                0 => 0,         // note
                1 or 2 => 1,    // instrument
                3 or 4 => 2,    // volume
                5 => 3,         // fx1 
                6 or 7 => 4,    // fx1 param
                8 => 5,         // fx2 
                9 or 10 => 6,   // fx2 param
                11 => 7,        // fx3
                12 or 13 => 8,  // fx3 param
                14 => 9,        // fx4
                15 or 16 => 10, // fx4 param
                _ => throw new IndexOutOfRangeException()
            };
        }

        /// <summary>
        /// Returns the value of the property at column. <br></br>
        /// (Double digit values, [vol, inst, effect parameters] are treated as 1 value)
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[int column] {
            get {
                return column switch {
                    0 => Note,
                    1 => Instrument,
                    2 => Volume,
                    3 => (int)Effect1.Type,
                    4 => Effect1.Parameter,
                    5 => (int)Effect2.Type,
                    6 => Effect2.Parameter,
                    7 => (int)Effect3.Type,
                    8 => Effect3.Parameter,
                    9 => (int)Effect4.Type,
                    10 => Effect4.Parameter,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
            set {
                switch (column) {
                    case 0: Note = value; break;
                    case 1: Instrument = value; break;
                    case 2: Volume = value; break;
                    case 3: effect1.Type = (char)value; break;
                    case 4: effect1.Parameter = value; break;
                    case 5: effect2.Type = (char)value; break;
                    case 6: effect2.Parameter = value; break;
                    case 7: effect3.Type = (char)value; break;
                    case 8: effect3.Parameter = value; break;
                    case 9: effect4.Type = (char)value; break;
                    case 10: effect4.Parameter = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns true if this event is empty
        /// </summary>
        public bool IsEmpty {
            get {
                if (Note == EMPTY)
                    if (Instrument == EMPTY)
                        if (Volume == EMPTY)
                            if (Effect1.Type == EMPTY)
                                if (Effect2.Type == EMPTY)
                                    if (Effect3.Type == EMPTY)
                                        if (Effect4.Type == EMPTY)
                                            return true;
                return false;
            }
        }
        public void SetEmpty() {
            note = EMPTY;
            instrument = EMPTY;
            volume = EMPTY;
            effect1.Type = (char)EMPTY;
            effect1.Parameter = EMPTY;
            effect2.Type = (char)EMPTY;
            effect2.Parameter = EMPTY;
            effect3.Type = (char)EMPTY;
            effect3.Parameter = EMPTY;
            effect4.Type = (char)EMPTY;
            effect4.Parameter = EMPTY;
        }

        public void CopyFrom(PatternEvent other) {
            note = other.note;
            instrument = other.instrument;
            volume = other.volume;
            Effect1 = other.Effect1;
            Effect2 = other.Effect2;
            Effect3 = other.Effect3;
            Effect4 = other.Effect4;
        }

        /// <summary>
        /// Returns the specific effect, effectNum, under this event
        /// </summary>
        /// <param name="effectNum"></param>
        /// <returns></returns>
        public Effect GetEffect(int effectNum) {
            return effectNum switch {
                0 => Effect1,
                1 => Effect2,
                2 => Effect3,
                3 => Effect4,
                _ => throw new IndexOutOfRangeException(),
            };
        }
    }

    /// <summary>
    /// Stores an effect, with a type and parameter value.
    /// </summary>
    public struct Effect {

        char type;
        byte parameter;

        /// <summary>
        /// The name of the effect as a character.<br></br>
        /// Setting this will automatically intialize the parameter, if needed
        /// </summary>
        public char Type {
            get { return type; }
            set {
                // if this effect was previously empty, initialize the effect with default values
                if (type == PatternEvent.EMPTY) {
                    Parameter = value switch {
                        // pan and detune are intialized to center 
                        '8' or 'P' => 50,
                        _ => 0,
                    }; ;
                }

                type = value;

                // if we are setting this effect to empty, make the parameter empty too
                if (value == PatternEvent.EMPTY) {
                    parameter = PatternEvent.EMPTY;
                }
                else {
                    // clamps parameter to make sure it's within bounds if switching from hex to decimal
                    Parameter += 0;
                }
            }
        }

        public Effect() {
            type = (char)PatternEvent.EMPTY;
            parameter = PatternEvent.EMPTY;
        }

        public Effect(char type, byte parameter) {
            this.type = type;
            this.parameter = parameter;
        }

        /// <summary>
        /// The parameter for this effect, information on how the effect should be applied
        /// </summary>
        public int Parameter {
            get { return parameter; }
            set {
                if (ParameterIsHex) {
                    parameter = (byte)value;
                }
                else {
                    parameter = (byte)Math.Clamp(value, 0, 99);
                }
            }
        }

        /// <summary>
        /// Returns true if this parameter is dual, _xy instead of _xx. <br></br>
        /// This means the parameter should operate in hexadecimal for greater range per digit.
        /// </summary>
        public bool ParameterIsHex {
            get {
                return IsTypeHex(Type);
            }
        }

        bool IsTypeHex(char type) {

            return type switch {
                '0' or '4' or '7' or 'Q' or 'R' => true,
                _ => false,
            };

        }

        /// <summary>
        /// Returns the parameter value as a 2 character string
        /// </summary>
        /// <returns></returns>
        public string GetParameterAsString() {
            if (type == PatternEvent.EMPTY) {
                return "··";
            }
            else if (ParameterIsHex) {
                return parameter.ToString("X2");
            }
            else {
                return parameter.ToString("D2");
            }
        }

        /* Effects Cheatsheet
         * 
         * 0xy - arpeggio
         * 1xx - rise
         * 2xx - fall
         * 3xx - portamento
         * 4xy - vibrato
         * 7xy - tremolo
         * 8xx - pan
         * 9xx - stereo phase offset
         * 
         * Axx - volume fade down
         * Wxx - volume fade up
         * 
         * Gxx - delay row
         * 
         * Qxy - note bend up
         * Rxy - note bend down
         * Pxx - pitch
         * 
         * Bxx - loop to frame #
         * Cxx - stop song
         * Dxx - skip # frames
         * Fxx - speed change
         * 
         * Sxx - cut
         * Lxx - release
         * 
         * Vxx - set wave
         * Xxx - blend wave
         * Zxx - stretch wave
         * Mxx - fm
         */
    }
}
