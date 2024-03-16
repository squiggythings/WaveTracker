using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;

namespace WaveTracker.Rendering {
    public class FrameRenderer : Element {
        int cursorRow;
        int cursorColumn;
        ChannelHeader[] headers;
        public static int centerRow => numOfVisibleRows / 2;
        public static int numOfVisibleRows = 24;

        public void Initialize() {
            headers = new ChannelHeader[Song.CHANNEL_COUNT + 1];
            for (int i = 0; i < headers.Length; i++) {
                if (i == 0)
                    headers[i] = new ChannelHeader(i - 1, null);
                else
                    headers[i] = new ChannelHeader(i - 1, ChannelManager.channels[i - 1]);
                headers[i].x = 22 + (i - 1) * 64;
                headers[i].y = 1;
                headers[i].SetParent(this);
            }
        }
        public void Update(GameTime gameTime) {
            numOfVisibleRows = (App.WindowHeight - 184) / 7 - 1;
            if (numOfVisibleRows < 5) numOfVisibleRows = 5;
            foreach (ChannelHeader header in headers) {
                if (header.id != -1) {
                    header.x = 22 + (header.id - 0 - FrameEditor.channelScroll) * 64;
                    header.enabled = header.id - FrameEditor.channelScroll >= 0 && header.id - FrameEditor.channelScroll < 12;
                }
                header.Update(gameTime);
            }
        }

        public void UpdateChannelHeaders() {
            foreach (ChannelHeader header in headers) {
                header.UpdateAmplitude();
            }
        }

        public float GetChannelHeaderAmp(int id) {
            return headers[id + 1].amplitude / 50f;
        }

        public void DrawFrame(Frame frame, int cursorRow, int cursorColumn) {
            DrawRect(0, 0, 790, 500, Colors.theme.background);
            int rowsNUM = frame.GetLastRow() + 1;

            this.cursorRow = cursorRow;
            this.cursorColumn = cursorColumn;
            for (int i = 0; i < numOfVisibleRows; i++) {
                int rowNum = i - centerRow + cursorRow;
                if (rowNum >= 0 && rowNum < rowsNUM)
                    DrawRow(rowNum, frame.pattern[rowNum], i * 7 + 33);
            }

            if (FrameEditor.selectionActive)
                DrawSelection();

            for (int i = 0; i < numOfVisibleRows; i++) {
                int rowNum = i - centerRow + cursorRow;
                if (rowNum >= 0 && rowNum < rowsNUM && rowNum != cursorRow)
                    DrawRowText(rowNum, frame.pattern[rowNum], i * 7 + 33);
            }
            DrawRowText(cursorRow, frame.pattern[cursorRow], centerRow * 7 + 33);

            // draw channel dividers and headers
            foreach (ChannelHeader header in headers) {
                if (header.id != -1) {
                    header.x = 22 + (header.id - 0 - FrameEditor.channelScroll) * 64;
                }
            }

            DrawRect(0, 0, 790, 5, Colors.theme.rowSeparator);
            for (int i = 0; i < 12; ++i) {
                DrawRect(i * 64 + 21, 0, 1, 389 + 90, Colors.theme.rowSeparator);

                headers[i + FrameEditor.channelScroll + 1].Draw();
            }
            DrawRect(0, 32, 790, 1, Colors.theme.rowSeparator);
            headers[0].Draw();
        }

        void DrawRow(int rowNum, short[] rowContent, int y) {
            #region getRowColor
            Color rowTextColor = Colors.theme.patternText;
            Color rowColor = Colors.theme.background;

            if (rowNum % Song.currentSong.rowHighlight1 == 0) {
                rowTextColor = Colors.theme.patternTextHighlighted;
                rowColor = Colors.theme.backgroundHighlighted;
            }
            else if (rowNum % Song.currentSong.rowHighlight2 == 0) {
                rowTextColor = Colors.theme.patternTextSubHighlight;
                rowColor = Colors.theme.backgroundSubHighlight;
            }
            if (Playback.isPlaying && Playback.position.Frame == FrameEditor.currentFrame && Playback.position.Row == rowNum && !Audio.AudioEngine.rendering) {
                rowColor = Colors.theme.rowPlaybackColor;
            }
            if (rowNum == cursorRow)
                if (FrameEditor.canEdit)
                    rowColor = Colors.theme.rowEditColor;
                else
                    rowColor = Colors.theme.rowCurrentColor;
            #endregion

            if (Preferences.profile.showRowNumbersInHex)
                WriteMonospaced(rowNum.ToString("X2"), 6, y, rowTextColor, 4);
            else
                WriteMonospaced(rowNum.ToString("D3"), 4, y, rowTextColor, 4);

            DrawRect(22, y, 767, 7, rowColor);

        }

        void DrawRowText(int rowNum, short[] rowContent, int y) {
            #region getRowColor
            Color rowTextColor = Colors.theme.patternText;

            if (rowNum % Song.currentSong.rowHighlight1 == 0) {
                rowTextColor = Colors.theme.patternTextHighlighted;
            }
            else if (rowNum % Song.currentSong.rowHighlight2 == 0) {
                rowTextColor = Colors.theme.patternTextSubHighlight;
            }

            #endregion
            bool isCurrRow = rowNum == cursorRow;
            if (isCurrRow && rowContent[FrameEditor.currentColumn] != -1) {
                DrawCursor(22, y);
            }
            int x = 24;
            for (int i = 0; i < 60; i += 5) {
                WriteNote(rowContent[i + FrameEditor.channelScroll * 5], x, y, rowTextColor, isCurrRow);
                x += 19;
                WriteInstrument(rowContent[i + FrameEditor.channelScroll * 5 + 1], x, y, isCurrRow, rowTextColor);
                x += 13;
                WriteVolume(rowContent[i + FrameEditor.channelScroll * 5 + 2], x, y, isCurrRow, FrameEditor.currentColumn == i + FrameEditor.channelScroll * 5 + 2, rowTextColor);
                x += 13;
                WriteEffect(rowContent[i + FrameEditor.channelScroll * 5 + 3], x, y, isCurrRow, rowTextColor);
                x += 5;
                WriteEffectParameter(rowContent[i + FrameEditor.channelScroll * 5 + 4], x, y, isCurrRow, rowContent[i + FrameEditor.channelScroll * 5 + 3], rowTextColor);
                x += 14;
            }
            if (isCurrRow && rowContent[FrameEditor.currentColumn] == -1) {
                DrawCursor(22, y);
            }
        }

        void DrawCursor(int px, int py) {

            px += cursorColumn / 8 * 64 + 1;
            px -= FrameEditor.channelScroll * 64;
            int rowSegment = cursorColumn % 8;
            int width = 8;
            if (rowSegment == 0) // note column
                width = 19;
            if (rowSegment > 0) //
                px += 19;
            if (rowSegment > 1)
                px += 5;
            if (rowSegment > 2)
                px += 8;
            if (rowSegment > 3)
                px += 5;
            if (rowSegment > 4)
                px += 8;
            if (rowSegment > 5)
                px += 5;
            if (rowSegment > 6)
                px += 5;
            DrawRect(px, py, width - 2, 7, Colors.theme.cursor);
            //DrawRoundedRect(px - 1, py - 1, width, 9, Colors.theme.cursorColor);
        }

        void DrawSelection() {

            int startCol = FrameEditor.selectionMin.X;
            int startRow = FrameEditor.selectionMin.Y;


            int endCol = FrameEditor.selectionMax.X;
            int endRow = FrameEditor.selectionMax.Y;

            int startX = FrameEditor.getStartPosOfFileColumn(startCol);
            int startY = FrameEditor.getStartPosOfRow(startRow);
            int endY = FrameEditor.getEndPosOfRow(endRow);
            if (endY > 184 + numOfVisibleRows * 7)
                endY = 184 + numOfVisibleRows * 7;
            int width = FrameEditor.getEndPosOfFileColumn(endCol) - startX;
            int height = endY - startY;

            if (startX < 0) {
                width += startX;
                if (width < 0)
                    width = 0;
                startX = 0;
            }

            if (startY < y + 33) {
                height += startY - y - 33;
                startY = y + 33;
            }
            if (height < 0)
                height = 0;

            DrawRect(startX + 22, startY - y, width, height, Colors.theme.selection);

        }

        void WriteNote(int value, int x, int y, Color rowText, bool currRow) {
            if (value == -2) // off
            {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("OFF", x, y, rowText);
                else {
                    DrawRect(x + 1, y + 2, 13, 2, rowText);
                }
            }
            else if (value == -3) // release 
            {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("REL", x, y, rowText);
                else {
                    DrawRect(x + 1, y + 2, 13, 1, rowText);
                    DrawRect(x + 1, y + 4, 13, 1, rowText);
                }
            }
            else if (value < 0) // empty
            {
                if (FrameEditor.canEdit)
                    WriteMonospaced("···", x + 1, y, currRow ? Colors.theme.rowEditText : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha), 4);
                else
                    WriteMonospaced("···", x + 1, y, currRow ? Colors.theme.rowCurrentText : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha), 4);
            }
            else {
                string val = Helpers.MIDINoteToText(value);
                if (val.Contains('#')) {
                    Write(val, x, y, rowText);
                }
                else {
                    WriteMonospaced(val[0] + "-", x, y, rowText, 5);
                    Write(val[2] + "", x + 11, y, rowText);
                }

            }
        }

        void WriteInstrument(int value, int x, int y, bool currRow, Color rowText) {
            if (value < 0) {
                if (FrameEditor.canEdit)
                    WriteMonospaced("··", x + 1, y, currRow ? Colors.theme.rowEditText : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha), 4);
                else
                    WriteMonospaced("··", x + 1, y, currRow ? Colors.theme.rowCurrentText : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha), 4);
            }
            else {
                if (value >= Song.currentSong.instruments.Count)
                    WriteMonospaced(value.ToString("D2"), x, y, Color.Red, 4);
                else if (Song.currentSong.instruments[value].instrumentType == InstrumentType.Sample)
                    WriteMonospaced(value.ToString("D2"), x, y, Colors.theme.instrumentColumnSample, 4);
                else
                    WriteMonospaced(value.ToString("D2"), x, y, Colors.theme.instrumentColumnWave, 4);

            }
        }

        void WriteVolume(int value, int x, int y, bool currRow, bool currColumn, Color rowText) {
            if (value < 0) {
                WriteMonospaced("··", x + 1, y, currRow ? (FrameEditor.canEdit ? Colors.theme.rowEditText : Colors.theme.rowCurrentText) : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha), 4);
            }
            else {
                if (currRow && currColumn || !Preferences.profile.fadeVolumeColumn)
                    WriteMonospaced(value.ToString("D2"), x, y, Colors.theme.volumeColumn, 4);
                else
                    WriteMonospaced(value.ToString("D2"), x, y, Helpers.Alpha(Colors.theme.volumeColumn, (int)(value / 100f * 180 + (255 - 180))), 4);
            }
        }

        void WriteEffect(int value, int x, int y, bool currRow, Color rowText) {
            if (value < 0) {
                Write("·", x + 1, y, currRow ? (FrameEditor.canEdit ? Colors.theme.rowEditText : Colors.theme.rowCurrentText) : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha));
            }
            else {
                Write("" + Helpers.GetEffectCharacter(value), x, y, Colors.theme.effectColumn);
            }
        }

        void WriteEffectParameter(int value, int x, int y, bool currRow, int effectNum, Color rowText) {

            if (value < 0) {
                WriteMonospaced("··", x + 1, y, currRow ? (FrameEditor.canEdit ? Colors.theme.rowEditText : Colors.theme.rowCurrentText) : Helpers.Alpha(rowText, Colors.theme.patternEmptyTextAlpha), 4);
            }
            else {
                if (Helpers.IsEffectHex((char)effectNum))
                    WriteMonospaced(value.ToString("X2"), x, y, Colors.theme.effectColumnParameter, 4);
                else
                    WriteMonospaced(value.ToString("D2"), x, y, Colors.theme.effectColumnParameter, 4);
            }
        }
    }

    // || -1 -1 -1 -1 -1 || -1 -1 -1 -1 -1 || -1 -1 -1 -1 -1 ||
}
