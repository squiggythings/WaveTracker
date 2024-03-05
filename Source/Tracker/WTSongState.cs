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
    public class WTSongState {

        List<string> packedString;
        /// <summary>
        /// The cursor position before this action was committed
        /// </summary>
        public CursorPos previousPosition { get; private set; }
        /// <summary>
        /// The cursor position after this action was committed
        /// </summary>
        public CursorPos currentPosition { get; private set; }

        public WTSongState(WTSong song, CursorPos previous, CursorPos next) {
            //songData = new MemoryStream();
            //Serializer.Serialize(songData, song);
            packedString = song.PackPatternsToString();
            previousPosition = previous;
            currentPosition = next;
            Debug.WriteLine("str: [" + packedString[0] + "]");
        }

        //public WTSong GetSong() {
        //    //return Serializer.Deserialize<WTSong>(songData);
        //}

    }
}
