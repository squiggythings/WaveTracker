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


namespace WaveTracker.Tracker {
    public class WTSongState {

        MemoryStream songData;
        /// <summary>
        /// The cursor position before this action was committed
        /// </summary>
        public CursorPos previousPosition { get; private set; }
        /// <summary>
        /// The cursor position after this action was committed
        /// </summary>
        public CursorPos nextPosition { get; private set; }

        public WTSongState(WTSong song, CursorPos previous, CursorPos next) {
            Serializer.Serialize(songData, song);
            previousPosition = previous;
            nextPosition = next;
        }

        WTSong GetSong() {
            return Serializer.Deserialize<WTSong>(songData);
        }

    }
}
