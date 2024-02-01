using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WaveTracker.Tracker {

    /// <summary>
    /// Holds a position in a song
    /// </summary>
    public struct CursorPos {

        public const int COLUMN_NOTE = 0;
        public const int COLUMN_INSTRUMENT1 = 1;
        public const int COLUMN_INSTRUMENT2 = 2;
        public const int COLUMN_VOLUME1 = 3;
        public const int COLUMN_VOLUME2 = 4;
        public const int COLUMN_EFFECT = 5;
        public const int COLUMN_EFFECT_PARAMETER1 = 6;
        public const int COLUMN_EFFECT_PARAMETER2 = 7;

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
        public int Column { get; set; }
        /* 
         * Column Cheatsheet
         *  0 - note
         *  1 - instrument1
         *  2 - instrument2
         *  3 - volume1
         *  4 - volume2
         *  5 - effect1
         *  6 - effect1 param1
         *  7 - effect1 param2
         *  8 - effect2
         *  9 - effect2 param1
         * 10 - effect2 param2
         * 11 - effect3
         * 12 - effect3 param1
         * 13 - effect3 param2
         * 14 - effect4
         * 15 - effect4 param1
         * 16 - effect4 param2
         */

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
        /// Returns the column number but treats all effects as if they were 1 column
        /// <br></br>0: Note, 1: inst1, 2: inst2, 3: vol1, 4: vol2, 5: fx, 6: fxparam1, 7: fxparam2
        /// </summary>
        /// <returns></returns>
        public int GetColumnAsSingleEffectChannel() {
            if (Column < 5)
                return Column;
            else
                return 5 + (Column - 5) % 3;
        }

        /// <summary>
        /// Returns the column type of this position. Corresponds 1:1 to columns in a PatternEvent
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public CursorColumnType GetColumnType() {
            return Column switch {
                0 => CursorColumnType.Note,
                1 or 2 => CursorColumnType.Instrument,
                3 or 4 => CursorColumnType.Volume,
                5 => CursorColumnType.Effect1,
                6 or 7 => CursorColumnType.Effect1Param,
                8 => CursorColumnType.Effect2,
                9 or 10 => CursorColumnType.Effect2Param,
                11 => CursorColumnType.Effect3,
                12 or 13 => CursorColumnType.Effect3Param,
                14 => CursorColumnType.Effect4,
                15 or 16 => CursorColumnType.Effect4Param,
                _ => throw new IndexOutOfRangeException()
            };
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

        public CursorPos(int frame, int row, int channel, int column) {
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
            if (Column > 2 + song.NumEffectColumns[Channel] * 2) return false;
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
            Channel = Math.Clamp(Channel, 0, song.ChannelCount);
            Column = Math.Clamp(Column, 0, 4 + song.NumEffectColumns[Channel] * 3);
        }

        /// <summary>
        /// Moves the position one column to the left in a song
        /// </summary>
        public void MoveLeft(WTSong song) {
            int column = Column - 1;
            if (column < 0) {
                MoveToChannel(Channel - 1, song);
                column = song.GetNumColumnsOfChannel(Channel) - 1;
            }
            Column = column;
        }

        /// <summary>
        /// Moves the position one column to the right in a song
        /// </summary>
        public void MoveRight(WTSong song) {
            int column = Column + 1;
            if (column > song.GetNumColumnsOfChannel(Channel) - 1) {
                MoveToChannel(Channel + 1, song);
                column = 0;
            }
            Column = column;
        }

        /// <summary>
        /// Moves the position to the first column of a channel
        /// </summary>
        /// <param name="channel"></param>
        public void MoveToChannel(int channel, WTSong song) {
            channel %= song.ChannelCount;
            if (channel < 0) {
                channel += song.ChannelCount;
            }
            Channel = channel;
            Column = 0;
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
        /// Returns true if this position in song is empty
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public bool IsPositionEmpty(WTSong song) {
            if (GetColumnAsSingleEffectChannel() == COLUMN_EFFECT_PARAMETER1 || GetColumnAsSingleEffectChannel() == COLUMN_EFFECT_PARAMETER2) {
                return song[Frame][Row][Channel][(int)GetColumnType() - 1] == PatternEvent.EMPTY;
            }
            else {
                return song[this] == PatternEvent.EMPTY;
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
            return "[f:" + Frame + "|r:" + Row + "|t:" + Channel + "|c:" + Column + "]";
        }

        public static bool operator ==(CursorPos a, CursorPos b) {
            return a.Frame == b.Frame && a.Row == b.Row && a.Channel == b.Channel && a.Column == b.Column;
        }
        public static bool operator !=(CursorPos a, CursorPos b) {
            return a.Frame != b.Frame || a.Row != b.Row || a.Channel != b.Channel || a.Column != b.Column;
        }
    }

    public enum CursorColumnType {
        Note,
        Instrument,
        Volume,
        Effect1,
        Effect1Param,
        Effect2,
        Effect2Param,
        Effect3,
        Effect3Param,
        Effect4,
        Effect4Param
    }
}
