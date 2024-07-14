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
        public const int MAX_CHANNEL_COUNT = 24;
        public const int DEFAULT_CHANNEL_COUNT = 24;
        public const int MAX_SONG_COUNT = 32;

        public bool IsDirty { get; private set; }

        /// <summary>
        /// The version this module was created in
        /// </summary>
        [ProtoMember(1)]
        public string Version { get; private set; }
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
        public List<OldInstrument> OldInstruments { get; set; }

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
        [ProtoMember(11)]
        public List<Instrument> Instruments { get; set; }

        public WTModule() {
            ChannelCount = App.Settings.Files.DefaultNumberOfChannels;
            Author = App.Settings.Files.DefaultAuthorName;
            Songs = new List<WTSong>();
            Songs.Add(new WTSong(this));
            Instruments = new List<Instrument> {
                new WaveInstrument ()
            };
            WaveBank = new Wave[100];
            WaveBank[0] = Wave.Sine;
            WaveBank[1] = Wave.Triangle;
            WaveBank[2] = Wave.Saw;
            WaveBank[3] = Wave.Pulse50;
            WaveBank[4] = Wave.Pulse25;
            WaveBank[5] = Wave.Pulse12pt5;
            Version = App.VERSION;
            TickRate = 60;
            for (int i = 6; i < 100; i++) {
                WaveBank[i] = new Wave();
            }
        }

        [ProtoBeforeSerialization]
        internal void BeforeSerialization() {
            //foreach(WTSong song in Songs) {
            //    song.BeforeSerialized();
            //}
            return;
        }
        [ProtoAfterDeserialization]
        internal void AfterDeserialization() {
            if (Instruments == null) {
                Instruments = new List<Instrument>();
                foreach (OldInstrument oldInstrument in OldInstruments) {
                    Instruments.Add(oldInstrument.ToNewInstrument());
                }
            }
            foreach (WTSong song in Songs) {
                song.AfterDeserialized(this);
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
            App.PatternEditor.OnResizeChannels();

        }

        public static WTModule FromOldSongFormat(Song song) {
            WTModule module = new WTModule() {
                Comment = song.comment,
                Title = song.name,
                Author = song.author,
                Year = song.year,
                OldInstruments = song.instruments,
                WaveBank = song.waves,
                TickRate = song.tickRate,
                ChannelCount = 24
            };
            module.Instruments = new List<Instrument>();
            foreach (OldInstrument oldInstrument in module.OldInstruments) {
                module.Instruments.Add(oldInstrument.ToNewInstrument());
            }
            WTSong wtsong = new WTSong(module) {
                RowHighlightPrimary = song.rowHighlight1,
                RowHighlightSecondary = song.rowHighlight2,
                TicksPerRow = song.ticksPerRow,
                RowsPerFrame = song.rowsPerFrame
            };

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellValue"></param>
        /// <param name="isNote"></param>
        /// <returns></returns>
        static int ConvertCell(short cellValue, bool isNote) {
            if (cellValue == Frame.NOTE_EMPTY_VALUE) return WTPattern.EVENT_EMPTY;
            if (cellValue == Frame.NOTE_CUT_VALUE) return WTPattern.EVENT_NOTE_CUT;
            if (cellValue == Frame.NOTE_RELEASE_VALUE) return WTPattern.EVENT_NOTE_RELEASE;
            if (isNote)
                return cellValue + 12;
            else
                return cellValue;

        }

        /// <summary>
        /// Returns an array of strings containing the name of each song in this module
        /// </summary>
        /// <returns></returns>
        public string[] GetSongNames() {
            string[] ret = new string[Songs.Count];
            for (int i = 0; i < Songs.Count; i++) {
                ret[i] = "#" + (i + 1) + " " + Songs[i].Name;
            }
            return ret;
        }

        /// <summary>
        /// Swaps instances of <c>inst1</c> and <c>inst2</c> with each other across the whole song.
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="inst2"></param>
        public void SwapInstrumentsInSongs(int inst1, int inst2) {
            foreach (WTSong song in Songs) {
                foreach (WTPattern pattern in song.Patterns) {
                    for (int row = 0; row < 256; row++) {
                        for (int channel = 0; channel < ChannelCount; channel++) {
                            // ex: swapping 00 with 01
                            // all instruments that are 01 are set to 128
                            // all instruments that are 00 are set to 01
                            if (pattern[row, channel, CellType.Instrument] == inst2) {
                                pattern.SetCellRaw(row, channel, CellType.Instrument, 128);
                            }
                            if (pattern[row, channel, CellType.Instrument] == inst1) {
                                pattern.SetCellRaw(row, channel, CellType.Instrument, (byte)inst2);
                            }
                        }
                    }
                    for (int row = 0; row < 256; row++) {
                        for (int channel = 0; channel < ChannelCount; channel++) {
                            // ex: swapping 00 with 01
                            // all instruments that are 01 are set to 128
                            // all instruments that are 00 are set to 01
                            if (pattern[row, channel, CellType.Instrument] == 128) {
                                pattern.SetCellRaw(row, channel, CellType.Instrument, (byte)inst1);
                            }

                        }
                    }
                }
            }
        }

        public void RemoveUnusedInstruments() {
            UI.Dialogs.messageDialog.Open("This function has not been implemented yet!", UI.MessageDialog.Icon.Warning, "OK");
        }

        public void RemoveUnusedWaves() {
            UI.Dialogs.messageDialog.Open("This function has not been implemented yet!", UI.MessageDialog.Icon.Warning, "OK");
        }

        /// <summary>
        /// Call whenever a change is made to the module that needs to be saved
        /// </summary>
        public void SetDirty() {
            IsDirty = true;
        }

        /// <summary>
        /// Called after the module is saved to disk
        /// </summary>
        public void OnSaveModule() {
            IsDirty = false;
        }

        /// <summary>
        /// If an instrument is deleted, this will shift the values of all instances of instruments in the module, so that they match the new order.
        /// Any instance where the deleted instrument was used will be deleted as well.
        /// </summary>
        /// <param name="indexOfDeletedInstrument"></param>
        public void AdjustForDeletedInstrument(int indexOfDeletedInstrument) {
            foreach (WTSong song in Songs) {
                foreach (WTPattern pattern in song.Patterns) {
                    for (int row = 0; row < 256; row++) {
                        for (int channel = 0; channel < ChannelCount; channel++) {
                            if (!pattern.CellIsEmpty(row, channel, CellType.Instrument)) {
                                if (pattern[row, channel, CellType.Instrument] > indexOfDeletedInstrument) {
                                    pattern[row, channel, CellType.Instrument]--;
                                }
                                else if (pattern[row, channel, CellType.Instrument] == indexOfDeletedInstrument) {
                                    pattern[row, channel, CellType.Instrument] = WTPattern.EVENT_EMPTY;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
