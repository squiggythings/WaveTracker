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
        public CursorPos minPosition;
        /// <summary>
        /// The bottom-right position of the selection rectangle
        /// </summary>
        public CursorPos maxPosition;

        /// <summary>
        /// A list of all positions in the selection
        /// </summary>
        public CursorPos[,] SelectionPositions { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        CursorPos prevStartPos, prevEndPos;


        public void Set(WTSong song, CursorPos startPos, CursorPos endPos) {

            if (prevStartPos == startPos && prevEndPos == endPos)
                return;

            prevStartPos = startPos;
            prevEndPos = endPos;

            Song = song;

            minPosition = startPos;
            maxPosition = endPos;

            // perform any swaps necessary
            if (startPos.IsBelow(endPos)) {
                minPosition.Row = endPos.Row;
                minPosition.Frame = endPos.Frame;
                maxPosition.Row = startPos.Row;
                maxPosition.Frame = startPos.Frame;
            }
            if (startPos.IsRightOf(endPos)) {
                maxPosition.Channel = startPos.Channel;
                maxPosition.Column = startPos.Column;
                minPosition.Channel = endPos.Channel;
                minPosition.Column = endPos.Column;
            }

            // if the user selected on the row column, set the selection to span all channels
            if (maxPosition.Channel == -1 && minPosition.Channel == -1) {
                minPosition.Channel = 0;
                minPosition.Column = 0;
                maxPosition.Channel = song.ChannelCount - 1;
                maxPosition.Column = song.GetNumCursorColumns(maxPosition.Channel) - 1;
            }

            // adjust the cursor positions to fill all of mulit-digit fields like instrument, volume and effects
            if (maxPosition.Column == 1)
                maxPosition.Column = 2;
            else if (maxPosition.Column == 3)
                maxPosition.Column = 4;
            else if (maxPosition.Column == 5 || maxPosition.Column == 6)
                maxPosition.Column = 7;
            else if (maxPosition.Column == 8 || maxPosition.Column == 9)
                maxPosition.Column = 10;
            else if (maxPosition.Column == 11 || maxPosition.Column == 12)
                maxPosition.Column = 13;
            else if (maxPosition.Column == 14 || maxPosition.Column == 15)
                maxPosition.Column = 16;
            if (minPosition.Column == 2)
                minPosition.Column = 1;
            else if (minPosition.Column == 4)
                minPosition.Column = 3;
            else if (minPosition.Column == 6 || minPosition.Column == 7)
                minPosition.Column = 5;
            else if (minPosition.Column == 9 || minPosition.Column == 10)
                minPosition.Column = 8;
            else if (minPosition.Column == 12 || minPosition.Column == 13)
                minPosition.Column = 11;
            else if (minPosition.Column == 15 || minPosition.Column == 16)
                minPosition.Column = 14;

            if (minPosition.Channel < 0)
                minPosition.Channel = 0;
            if (maxPosition.Channel < 0)
                maxPosition.Channel = 0;
            if (maxPosition.Column >= song.GetNumCursorColumns(maxPosition.Channel))
                maxPosition.Column = song.GetNumCursorColumns(maxPosition.Channel) - 1;
            return;
            // find width and height of selection bounds
            CursorPos p = minPosition;
            Width = 1;
            Height = 1;
            while (p.IsLeftOf(maxPosition)) {
                p.MoveRight(song);
                // skip over the second digit of instruments, volumes, and effect parameters, since we already accounted for them with the first digit
                if (p.Column == 2 || p.Column == 4 || p.Column == 7 || p.Column == 10 || p.Column == 13)
                    p.MoveRight(song);
                Width++;
            }
            while (p.IsAbove(maxPosition)) {
                p.MoveToRow(p.Row + 1, song);
                Height++;
            }

            // populate selection positions
            SelectionPositions = new CursorPos[Width, Height];
            p = minPosition;

            for (int x = 0; x < Width; x++) {
                p.Frame = minPosition.Frame;
                p.Row = minPosition.Row;
                for (int y = 0; y < Height; y++) {
                    SelectionPositions[x, y] = p;
                    p.MoveToRow(p.Row + 1, song);
                }
                p.MoveRight(song);
                // skip over the second digit of instruments, volumes, and effect parameters, since we already accounted for them with the first digit
                if (p.Column == 2 || p.Column == 4 || p.Column == 7 || p.Column == 10 || p.Column == 13)
                    p.MoveRight(song);
            }
        }


        public PatternSelection(WTSong song, CursorPos startPos, CursorPos endPos) {
            Set(song, startPos, endPos);
        }

        public PatternSelection() {

        }
    }
}
