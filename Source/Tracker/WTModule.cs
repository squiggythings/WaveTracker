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
    [ProtoContract]
    public class WTModule {
        public const int DEFAULT_CHANNEL_COUNT = 24;

        public List<WTSong> Songs { get; set; }
        /// <summary>
        /// The title of this module
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The author of this module
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// The copyright or year this module was made
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// Comment information about this module
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The number of ticks per second
        /// </summary>
        public int TickRate { get; set; }

        /// <summary>
        /// The bank of 100 waves in this module
        /// </summary>
        public Wave[] WaveBank { get; private set; }

        /// <summary>
        /// The list of instruments in this module
        /// </summary>
        public List<Macro> Instruments { get; set; }

        /// <summary>
        /// The number of channels in this module
        /// </summary>
        public int ChannelCount { get; set; }

        public WTModule() {
            Songs = new List<WTSong>();
            Songs.Add(new WTSong());
            Instruments = new List<Macro>();
            Instruments.Add(new Macro(MacroType.Wave));
            WaveBank = new Wave[100];
            WaveBank[0] = Wave.Sine;
            WaveBank[1] = Wave.Triangle;
            WaveBank[2] = Wave.Saw;
            WaveBank[3] = Wave.Pulse50;
            WaveBank[4] = Wave.Pulse25;
            WaveBank[5] = Wave.Pulse12pt5;
            ChannelCount = DEFAULT_CHANNEL_COUNT;
        }

        [ProtoBeforeSerialization]
        internal void BeforeSerialization() {
            return;
           
        }
        [ProtoAfterDeserialization]
        internal void AfterDeserialization() {
            foreach (WTSong song in Songs) {
                song.ParentModule = this;
            }
        }
    }
}
