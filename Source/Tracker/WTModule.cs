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
    public class WTModule {
        public const int DEFAULT_CHANNEL_COUNT = 24;
        public const int MAX_SONG_COUNT = 100;

        [ProtoMember(1)]
        string version;
        /// <summary>
        /// The title of this module
        /// </summary>
        [ProtoMember(2)]
        public string Title { get; set; }
        /// <summary>
        /// The author of this module
        /// </summary>
        [ProtoMember(3)]
        public string Author { get; set; }
        /// <summary>
        /// The copyright or year this module was made
        /// </summary>
        [ProtoMember(4)]
        public string Year { get; set; }
        /// <summary>
        /// Comment information about this module
        /// </summary>
        [ProtoMember(5)]
        public string Comment { get; set; }

        /// <summary>
        /// The number of ticks per second
        /// </summary>
        [ProtoMember(6)]
        public int TickRate { get; set; }

        /// <summary>
        /// The bank of 100 waves in this module
        /// </summary>
        [ProtoMember(7)]
        public Wave[] WaveBank { get; private set; }

        /// <summary>
        /// The list of instruments in this module
        /// </summary>
        [ProtoMember(8)]
        public List<Instrument> Instruments { get; set; }

        /// <summary>
        /// The number of channels in this module
        /// </summary>
        [ProtoMember(9)]
        public int ChannelCount { get; private set; }

        /// <summary>
        /// The list of songs in this module
        /// </summary>
        [ProtoMember(10)]
        public List<WTSong> Songs { get; set; }

        public WTModule() {
            ChannelCount = DEFAULT_CHANNEL_COUNT;
            Songs = new List<WTSong>();
            Songs.Add(new WTSong(this));
            Instruments = new List<Instrument>();
            Instruments.Add(new Instrument(InstrumentType.Wave));
            WaveBank = new Wave[100];
            WaveBank[0] = Wave.Sine;
            WaveBank[1] = Wave.Triangle;
            WaveBank[2] = Wave.Saw;
            WaveBank[3] = Wave.Pulse50;
            WaveBank[4] = Wave.Pulse25;
            WaveBank[5] = Wave.Pulse12pt5;
            for (int i = 6; i < 100; i++) {
                WaveBank[i] = new Wave();
            }
        }

        [ProtoBeforeSerialization]
        internal void BeforeSerialization() {
            version = App.VERSION;
            return;
        }
        [ProtoAfterDeserialization]
        internal void AfterDeserialization() {
            foreach (WTSong song in Songs) {
                song.ParentModule = this;
            }
        }

        /// <summary>
        /// Changes the number of channels in this module
        /// </summary>
        /// <param name="newChannelCount"></param>
        public void ResizeChannelCount(int newChannelCount) {
            ChannelCount = Math.Clamp(newChannelCount, 1, 24);
            foreach (WTSong song in Songs) {
                song.ResizeChannelCount();
            }
            App.PatternEditor.CalculateChannelPositioning();
            App.PatternEditor.OnResizeChannels();
        }

        public static WTModule FromOldSongFormat(Song song) {
            WTModule module = new WTModule();
            module.Comment = song.comment;
            module.Title = song.name;
            module.Author = song.author;
            module.Year = song.year;
            module.Instruments = song.instruments;
            module.WaveBank = song.waves;
            module.TickRate = song.tickRate;
            module.ChannelCount = 24;
            WTSong wtsong = new WTSong(module);
            wtsong.RowHighlightPrimary = song.rowHighlight1;
            wtsong.RowHighlightSecondary = song.rowHighlight2;
            wtsong.TicksPerRow = song.ticksPerRow;
            wtsong.RowsPerFrame = song.rowsPerFrame;

            int patternIndex = 0;
            foreach (Frame frame in song.frames) {
                wtsong.AddNewFrame();
                WTPattern pat = wtsong.Patterns[patternIndex];
                for (int row = 0; row < 256; ++row) {
                    for (int channel = 0; channel < module.ChannelCount; ++channel) {
                        pat[row, channel, CellType.Note] = ConvertCell(frame.pattern[row][channel * 5 + 0], true);
                        pat[row, channel, CellType.Instrument] = ConvertCell(frame.pattern[row][channel * 5 + 1], false);
                        pat[row, channel, CellType.Volume] = ConvertCell(frame.pattern[row][channel * 5 + 2], false);
                        int effect = ConvertCell(frame.pattern[row][channel * 5 + 3], false);
                        if (effect != WTPattern.EVENT_EMPTY) {
                            effect = Helpers.GetEffectCharacter(effect);
                        }
                        pat[row, channel, CellType.Effect1] = effect;
                        pat[row, channel, CellType.Effect1Parameter] = ConvertCell(frame.pattern[row][channel * 5 + 4], false);
                    }
                }
                patternIndex++;
            }
            module.Songs[0] = wtsong;
            return module;
        }

        static int ConvertCell(short cellValue, bool isNote) {
            if (cellValue == Frame.NOTE_EMPTY_VALUE) return WTPattern.EVENT_EMPTY;
            if (cellValue == Frame.NOTE_CUT_VALUE) return WTPattern.EVENT_NOTE_CUT;
            if (cellValue == Frame.NOTE_RELEASE_VALUE) return WTPattern.EVENT_NOTE_RELEASE;
            if (isNote)
                return cellValue + 12;
            else
                return cellValue;

        }
    }
}
