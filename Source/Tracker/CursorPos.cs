using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WaveTracker.Tracker {

    /// <summary>
    /// Holds a position in a song
    /// </summary>
    public struct CursorPos {

        /// <summary>
        /// Broad y position, the frame this position is on
        /// </summary>
        public int Frame { get; set; }
        /// <summary>
        /// Fine y position, the row of this frame's pattern this position is on
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Broad x position, the channel this position is on
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// Fine x position, the column this position is on
        /// </summary>
        public CursorColumnType Column { get; set; }


        public bool IsAbove(CursorPos other) {
            if (Frame == other.Frame) return Row < other.Row;
            return Frame < other.Frame;
        }

        public bool IsBelow(CursorPos other) {
            if (Frame == other.Frame) return Row > other.Row;
            return Frame > other.Frame;
        }

        public bool IsLeftOf(CursorPos other) {
            if (Channel == other.Channel) return Column < other.Column;
            return Channel < other.Channel;
        }

        public bool IsRightOf(CursorPos other) {
            if (Channel == other.Channel) return Column > other.Column;
            return Channel > other.Channel;
        }

        /// <summary>
        /// Initializes this position at the beginning of a song
        /// </summary>
        /// <param name="song"></param>
        public void Initialize() {
            Frame = 0;
            Row = 0;
            Channel = 0;
            Column = 0;
        }

        public CursorPos(int frame, int row, int channel, CursorColumnType column) {
            Frame = frame;
            Row = row;
            Channel = channel;
            Column = column;
        }

        /// <summary>
        /// Returns true if this position is valid in the song we're currently editing
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public bool IsValid(WTSong song) {
            if (Frame < 0) return false;
            if (Frame >= song.FrameSequence.Count) return false;
            if (Row < 0) return false;
            if (Row >= song.FrameSequence[Frame].GetPattern().GetModifiedLength()) return false;
            if (Channel < 0) return false;
            if (Channel >= song.NumEffectColumns.Length) return false;
            if (Column < 0) return false;
            if ((int)Column > 2 + song.NumEffectColumns[Channel] * 2) return false;
            return true;
        }

        /// <summary>
        /// If this position is out of bounds for a song, make it in bounds
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public void Normalize(WTSong song) {
            Frame = Math.Clamp(Frame, 0, song.FrameSequence.Count - 1);
            Row = Math.Clamp(Row, 0, song.FrameSequence[Frame].GetPattern().GetModifiedLength() - 1);
            Channel = Math.Clamp(Channel, 0, song.ParentModule.ChannelCount);
            Column = (CursorColumnType)Math.Clamp((int)Column, 0, song.GetNumCursorColumns(Channel) - 1);
        }

        public void ClampRow(int maxRow) {
            if (Row > maxRow)
                Row = maxRow;
        }

        /// <summary>
        /// Moves the position one cursor column to the left in a song
        /// </summary>
        public void MoveLeft(WTSong song) {
            int column = (int)Column - 1;
            if (column < 0) {
                MoveToChannel(Channel - 1, song);
                column = song.GetNumCursorColumns(Channel) - 1;
            }
            Column = (CursorColumnType)column;
        }

        /// <summary>
        /// Moves the position one cursor column to the right in a song
        /// </summary>
        public void MoveRight(WTSong song) {
            int column = (int)Column + 1;
            if (column > song.GetNumCursorColumns(Channel) - 1) {
                MoveToChannel(Channel + 1, song);
                column = 0;
            }
            Column = (CursorColumnType)column;
        }

        /// <summary>
        /// Moves the position to the first column of a channel
        /// </summary>
        /// <param name="channel"></param>
        public void MoveToChannel(int channel, WTSong song) {
            channel %= song.ParentModule.ChannelCount;
            if (channel < 0) {
                channel += song.ParentModule.ChannelCount;
            }
            Channel = channel;
            Column = CursorColumnType.Note;
        }

        /// <summary>
        /// Moves the cursor to a row
        /// </summary>
        /// <param name="row"></param>
        public void MoveToRow(int row, WTSong song, ref int frameWrapCount) {
            while (row < 0) {
                MoveToFrame(Frame - 1, song);
                row += song.FrameSequence[Frame].GetLength();
                frameWrapCount--;
            }
            while (row >= song.FrameSequence[Frame].GetLength()) {
                row -= song.FrameSequence[Frame].GetLength();
                MoveToFrame(Frame + 1, song);
                frameWrapCount++;
            }
            Row = row;
        }

        /// <summary>
        /// Moves the cursor to a row
        /// </summary>
        /// <param name="row"></param>
        public void MoveToRow(int row, WTSong song) {
            while (row < 0) {
                MoveToFrame(Frame - 1, song);
                row += song.FrameSequence[Frame].GetLength();
            }
            while (row >= song.FrameSequence[Frame].GetLength()) {
                row -= song.FrameSequence[Frame].GetLength();
                MoveToFrame(Frame + 1, song);
            }
            Row = row;
        }

        /// <summary>
        /// Moves the cursor to a row in a frame, but can never that a frame
        /// </summary>
        /// <param name="row"></param>
        public void MoveToRowClampedToFrame(int row, int frame, WTSong song) {
            Frame = frame;
            Row = Math.Clamp(row, 0, song[Frame].GetModifiedLength() - 1);
        }



        /// <summary>
        /// Returns true if this position in song is empty
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public bool IsPositionEmpty(WTSong song) {
            if (Column == CursorColumnType.Effect1Param1 || Column == CursorColumnType.Effect1Param2) {
                return song[Frame][Row, Channel, CellType.Effect1] == WTPattern.EVENT_EMPTY;
            }
            else if (Column == CursorColumnType.Effect2Param1 || Column == CursorColumnType.Effect2Param2) {
                return song[Frame][Row, Channel, CellType.Effect2] == WTPattern.EVENT_EMPTY;
            }
            else if (Column == CursorColumnType.Effect3Param1 || Column == CursorColumnType.Effect3Param2) {
                return song[Frame][Row, Channel, CellType.Effect3] == WTPattern.EVENT_EMPTY;
            }
            else if (Column == CursorColumnType.Effect4Param1 || Column == CursorColumnType.Effect4Param2) {
                return song[Frame][Row, Channel, CellType.Effect4] == WTPattern.EVENT_EMPTY;
            }
            else {
                return song[Frame][Row, Channel, Column.ToCellType()] == WTPattern.EVENT_EMPTY;
            }
        }

        /// <summary>
        /// Moves the cursor to a frame
        /// </summary>
        /// <param name="frame"></param>
        public void MoveToFrame(int frame, WTSong song) {
            int numFrames = song.FrameSequence.Count;

            frame %= numFrames;
            if (frame < 0) {
                frame += numFrames;
            }
            Frame = frame;

            // make sure the cursor is in bounds of this frame
            Row = Math.Clamp(Row, 0, song.FrameSequence[Frame].GetLength() - 1);
        }

        public override string ToString() {
            return "[f:" + Frame + "|r:" + Row + "|t:" + Channel + "|c:" + Column.ToString() + "]";
        }

        public override bool Equals(object obj) => obj is CursorPos other && Equals(other);

        public bool Equals(CursorPos p) => Frame == p.Frame && Row == p.Row && Channel == p.Channel && Column == p.Column;

        public override int GetHashCode() => (Frame, Row, Channel, Column).GetHashCode();

        public static bool operator ==(CursorPos lhs, CursorPos rhs) => lhs.Equals(rhs);

        public static bool operator !=(CursorPos lhs, CursorPos rhs) => !(lhs == rhs);

        /// <summary>
        /// The column of this position in the patterns cell array
        /// </summary>
        /// <returns></returns>
        public int CellColumn {
            get {
                return Channel * 11 + (int)Column.ToCellType();
            }
        }
    }



    public enum CursorColumnType {
        Note,
        Instrument1,
        Instrument2,
        Volume1,
        Volume2,
        Effect1,
        Effect1Param1,
        Effect1Param2,
        Effect2,
        Effect2Param1,
        Effect2Param2,
        Effect3,
        Effect3Param1,
        Effect3Param2,
        Effect4,
        Effect4Param1,
        Effect4Param2
    }
}
