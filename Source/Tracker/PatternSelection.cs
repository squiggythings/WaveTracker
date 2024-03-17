using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.Tracker {
    public class PatternSelection {
        public WTSong Song { get; private set; }
        /// <summary>
        /// The top-left position of the selection rectangle
        /// </summary>
        public CursorPos min;
        /// <summary>
        /// The bottom-right position of the selection rectangle
        /// </summary>
        public CursorPos max;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool IsActive { get; set; }

        public void Set(WTSong song, CursorPos startPos, CursorPos endPos) {
            Song = song;
            min = startPos;
            max = endPos;


            // perform any swaps necessary
            if (startPos.IsBelow(endPos)) {
                min.Row = endPos.Row;
                min.Frame = endPos.Frame;
                max.Row = startPos.Row;
                max.Frame = startPos.Frame;
            }
            if (startPos.IsRightOf(endPos)) {
                max.Channel = startPos.Channel;
                max.Column = startPos.Column;
                min.Channel = endPos.Channel;
                min.Column = endPos.Column;
            }

            // if the user selected on the row column, set the selection to span all channels
            if (max.Channel == -1 && min.Channel == -1) {
                min.Channel = 0;
                min.Column = 0;
                max.Channel = App.CurrentModule.ChannelCount - 1;
                max.Column = song.GetLastCursorColumnOfChannel(max.Channel);
            }

            // adjust the cursor positions to fill all of mulit-digit fields like instrument, volume and effects
            if (max.Column == CursorColumnType.Instrument1)
                max.Column = CursorColumnType.Instrument2;
            else if (max.Column == CursorColumnType.Volume1)
                max.Column = CursorColumnType.Volume2;
            else if (max.Column == CursorColumnType.Effect1 || max.Column == CursorColumnType.Effect1Param1)
                max.Column = CursorColumnType.Effect1Param2;
            else if (max.Column == CursorColumnType.Effect2 || max.Column == CursorColumnType.Effect2Param1)
                max.Column = CursorColumnType.Effect2Param2;
            else if (max.Column == CursorColumnType.Effect3 || max.Column == CursorColumnType.Effect3Param1)
                max.Column = CursorColumnType.Effect3Param2;
            else if (max.Column == CursorColumnType.Effect4 || max.Column == CursorColumnType.Effect4Param1)
                max.Column = CursorColumnType.Effect4Param2;

            if (min.Column == CursorColumnType.Instrument2)
                min.Column = CursorColumnType.Instrument1;
            else if (min.Column == CursorColumnType.Volume2)
                min.Column = CursorColumnType.Volume1;
            else if (min.Column == CursorColumnType.Effect1Param1 || min.Column == CursorColumnType.Effect1Param2)
                min.Column = CursorColumnType.Effect1;
            else if (min.Column == CursorColumnType.Effect2Param1 || min.Column == CursorColumnType.Effect2Param2)
                min.Column = CursorColumnType.Effect2;
            else if (min.Column == CursorColumnType.Effect3Param1 || min.Column == CursorColumnType.Effect3Param2)
                min.Column = CursorColumnType.Effect3;
            else if (min.Column == CursorColumnType.Effect4Param1 || min.Column == CursorColumnType.Effect4Param2)
                min.Column = CursorColumnType.Effect4;

            if (min.Channel < 0)
                min.Channel = 0;
            if (max.Channel < 0)
                max.Channel = 0;
            if (max.Column > song.GetLastCursorColumnOfChannel(max.Channel))
                max.Column = song.GetLastCursorColumnOfChannel(max.Channel);

            Width = max.CellColumn - min.CellColumn + 1;
            Height = max.Row - min.Row + 1;
        }


        public PatternSelection(WTSong song, CursorPos startPos, CursorPos endPos) {
            Set(song, startPos, endPos);
        }

        public PatternSelection() {
            IsActive = false;
        }
    }
}
