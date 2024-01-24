using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;
using WaveTracker.Tracker;
using WaveTracker;
using WaveTracker.Audio;

namespace WaveTracker.UI {

    /// <summary>
    /// The pattern editor interface, including the channel headers
    /// </summary>
    public class PatternEditor : Clickable {

        const int ROW_HEIGHT = 7;
        const int ROW_COLUMN_WIDTH = 22;

        public bool EditMode { get; set; }
        public int CurrentOctave { get; set; }
        public bool InstrumentMask { get; set; }

        public int InputStep { get; set; }

        /// <summary>
        /// The position of the main cursor in the song
        /// </summary>
        CursorPos cursorPosition;
        /// <summary>
        /// The position of the main cursor in the song
        /// </summary>
        public CursorPos GetCursorPos { get { return cursorPosition; } }

        /// <summary>
        /// Whether or not there is a selection active in the pattern editor
        /// </summary>
        bool selectionActive;

        /// <summary>
        /// The start position of a selection the user makes
        /// </summary>
        CursorPos selectionStart;

        /// <summary>
        /// The end position of a selection the user makes
        /// </summary>
        CursorPos selectionEnd;

        /// <summary>
        /// The top-left position of the selection rectangle
        /// </summary>
        CursorPos selectionMin;
        /// <summary>
        /// The bottom-right position of the selection rectangle
        /// </summary>
        CursorPos selectionMax;

        /// <summary>
        /// A list of all positions in the selection
        /// </summary>
        List<CursorPos> selection;

        /// <summary>
        /// The index of the leftmost channel to render
        /// </summary>
        public int FirstVisibleChannel { get; private set; }

        /// <summary>
        /// The index of the rightmost channel to render
        /// </summary>
        public int LastVisibleChannel { get; private set; }

        /// <summary>
        /// The position of the right-bound of the last channel<br></br>
        /// Where the pattern editor ends.
        /// </summary>
        public int LastChannelEndPos { get; private set; }

        /// <summary>
        /// Contents of the clipboard to copy/paste
        /// </summary>
        byte[][] clipboard;

        WChannelHeader[] channelHeaders;

        WTSong CurrentSong => WTSong.currentSong;
        WTFrame CurrentFrame => CurrentSong.FrameSequence[cursorPosition.Frame];
        WTPattern CurrentPattern => CurrentFrame.GetPattern();

        PatternEvent CurrentPatternEvent => CurrentSong[cursorPosition.Frame][cursorPosition.Row][cursorPosition.Channel];

        public PatternEditor(int x, int y) {
            this.x = x;
            this.y = y;
            channelHeaders = new WChannelHeader[WTModule.NUM_CHANNELS];
            for (int i = 0; i < channelHeaders.Length; ++i) {
                channelHeaders[i] = new WChannelHeader(0, -32, 63, i, this);
            }

            InputStep = 1;
            selection = new List<CursorPos>();
        }

        public void Update() {

            // responsive height
            int bottomMargin = 3;
            height = Game1.WindowHeight - y - bottomMargin;

            // responsive width
            int rightMargin = 156;
            width = Game1.WindowWidth - x - rightMargin - 1;

            FirstVisibleChannel -= Input.MouseScrollWheel(KeyModifier.Alt);
            FirstVisibleChannel = Math.Clamp(FirstVisibleChannel, 0, CurrentSong.ChannelCount - 1);


            CalculateChannelPositioning();

            if (MouseY < 0 && MouseY >= -32) {
                if (MouseX < ROW_COLUMN_WIDTH || MouseX > LastChannelEndPos) {
                    if (MouseX < width) {
                        if (Input.GetClick(KeyModifier.None)) {
                            ChannelManager.UnmuteAllChannels();
                        }
                    }
                }
            }

            #region change octave
            if (Input.GetKeyRepeat(Keys.OemOpenBrackets, KeyModifier.None) || Input.GetKeyRepeat(Keys.Divide, KeyModifier.None))
                CurrentOctave--;
            if (Input.GetKeyRepeat(Keys.OemCloseBrackets, KeyModifier.None) || Input.GetKeyRepeat(Keys.Multiply, KeyModifier.None))
                CurrentOctave++;
            CurrentOctave = Math.Clamp(CurrentOctave, 0, 9);
            #endregion
            #region solo/mute channels function keys
            if (Input.GetKeyRepeat(Keys.F9, KeyModifier.Alt))
                ChannelManager.ToggleChannel(cursorPosition.Channel);

            if (Input.GetKeyRepeat(Keys.F10, KeyModifier.Alt)) {
                if (ChannelManager.IsChannelSoloed(cursorPosition.Channel))
                    ChannelManager.UnmuteAllChannels();
                else
                    ChannelManager.SoloChannel(cursorPosition.Channel);
            }
            #endregion

            //////////////////////////////
            //        NAVIGATION        //
            //////////////////////////////

            #region home/end navigation
            // On home key press
            if (Input.GetKeyRepeat(Keys.Home, KeyModifier.None)) {
                GoToTopOfFrame();
            }
            // On end key press
            if (Input.GetKeyRepeat(Keys.End, KeyModifier.None)) {
                GoToBottomOfFrame();
            }
            #endregion
            #region moving cursor with arrow keys
            // On arrow keys
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.None)) {
                CancelSelection();
                MoveCursorLeft();
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.None)) {
                CancelSelection();
                MoveCursorRight();
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Ctrl)) {
                CancelSelection();
                PreviousFrame();
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Ctrl)) {
                CancelSelection();
                NextFrame();
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Alt)) {
                CancelSelection();
                MoveToChannel(cursorPosition.Channel - 1);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Alt)) {
                CancelSelection();
                MoveToChannel(cursorPosition.Channel + 1);
            }
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None)) {
                CancelSelection();
                MoveToRow(cursorPosition.Row + 1);
            }
            if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None)) {
                CancelSelection();
                MoveToRow(cursorPosition.Row - 1);
            }
            #endregion
            #region navigate with mouse
            if (ClickedDown) {
                CancelSelection();
            }
            if (SingleClickedM(KeyModifier.None) && MouseX < channelHeaders[Song.CHANNEL_COUNT - 1].x + width && !Input.MouseJustEndedDragging) {
                cursorPosition = GetCursorPositionFromPoint(MouseX, MouseY);
            }
            #endregion
            #region selection with arrow keys
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Shift)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                MoveCursorLeft();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Shift)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                MoveCursorRight();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.ShiftAlt)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                MoveToChannel(cursorPosition.Channel - 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.ShiftAlt)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                MoveToChannel(cursorPosition.Channel + 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.Shift)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                MoveToRow(cursorPosition.Row + 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Up, KeyModifier.Shift)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                MoveToRow(cursorPosition.Row - 1);
                SetSelectionEnd(cursorPosition);
            }
            #endregion
            #region selection with mouse
            if (Input.GetClick(KeyModifier.None) && Input.MouseIsDragging && GlobalPointIsInBounds(Input.lastClickLocation)) {
                if (!selectionActive) {
                    SetSelectionStart(GetCursorPositionFromPoint(LastClickPos.X, LastClickPos.Y));
                    selectionActive = true;
                }
                if (GetMouseLineNumber() == 0) {
                    MoveToRow(cursorPosition.Row - 1);
                }
                if (GetMouseLineNumber() > NumVisibleLines - 2) {
                    MoveToRow(cursorPosition.Row + 1);
                }
                SetSelectionEnd(GetCursorPositionFromPoint(MouseX, MouseY));
            }
            if (IsPressedM(KeyModifier.Shift)) {
                if (!selectionActive) {
                    SetSelectionStart(cursorPosition);
                    selectionActive = true;
                }
                SetSelectionEnd(GetCursorPositionFromPoint(MouseX, MouseY));
            }
            #endregion
            #region selection with CTRL-A and ESC
            if (Input.GetKeyRepeat(Keys.A, KeyModifier.Ctrl)) {
                if (selectionMax.Channel == cursorPosition.Channel && selectionMax.Row == CurrentPattern.GetModifiedLength() - 1 && selectionMax.Frame == cursorPosition.Frame && selectionMax.Column == GetNumFullColumns(cursorPosition.Channel) - 1 &&
                    selectionMin.Channel == cursorPosition.Channel && selectionMin.Row == 0 && selectionMin.Frame == cursorPosition.Frame && selectionMin.Column == 0) {
                    SetSelectionStart(cursorPosition);
                    selectionStart.Channel = 0;
                    selectionStart.Row = 0;
                    selectionStart.Column = 0;
                    SetSelectionEnd(cursorPosition);
                    selectionEnd.Channel = CurrentSong.ChannelCount - 1;
                    selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                    selectionEnd.Column = GetNumFullColumns(selectionEnd.Channel) - 1;
                }
                else {
                    selectionActive = true;
                    SetSelectionStart(cursorPosition);
                    selectionStart.Row = 0;
                    selectionStart.Column = 0;
                    SetSelectionEnd(cursorPosition);
                    selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                    selectionEnd.Column = GetNumFullColumns(selectionEnd.Channel) - 1;
                }
            }
            if (Input.GetKeyRepeat(Keys.Escape, KeyModifier.None)) {
                CancelSelection();
            }
            #endregion


            if (!selectionActive) {
                selectionStart = selectionEnd = cursorPosition;
            }
            CreateSelectionBounds();

            if (Input.GetKeyDown(Keys.Space, KeyModifier.None)) {
                EditMode = !EditMode;
            }
            if (!EditMode)
                return;

            ///////////////////////////////
            //           INPUT           //
            ///////////////////////////////
            #region field input
            if (cursorPosition.Column == 0) {
                // input note column
                foreach (Keys k in KeyInputs_Piano.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int note = KeyInputs_Piano[k];
                        if (note != PatternEvent.NOTE_CUT && note != PatternEvent.NOTE_RELEASE)
                            note += (CurrentOctave + 1) * 12;
                        CurrentPatternEvent.Note = note;
                        if (!InstrumentMask) {
                            CurrentPatternEvent.Instrument = InstrumentBank.CurrentInstrumentIndex;
                        }
                        MoveToRow(cursorPosition.Row + InputStep);
                    }
                }
            }
            else if (cursorPosition.Column == CursorPos.COLUMN_INSTRUMENT1 || cursorPosition.Column == CursorPos.COLUMN_VOLUME1) {
                // input 10's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = CurrentSong[cursorPosition];
                        if (val == PatternEvent.EMPTY)
                            val = 0;
                        CurrentSong[cursorPosition] = KeyInputs_Decimal[k] * 10 + val % 10;
                        MoveToRow(cursorPosition.Row + InputStep);
                        break;
                    }
                }
            }
            else if (cursorPosition.Column == CursorPos.COLUMN_INSTRUMENT2 || cursorPosition.Column == CursorPos.COLUMN_VOLUME2) {
                // input 1's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = CurrentSong[cursorPosition];
                        if (val == PatternEvent.EMPTY)
                            val = 0;
                        CurrentSong[cursorPosition] = (val / 10 * 10) + KeyInputs_Decimal[k];
                        MoveToRow(cursorPosition.Row + InputStep);
                        break;
                    }
                }
            }
            else if (cursorPosition.GetColumnType() == CursorPos.COLUMN_EFFECT) {
                // input effects
                foreach (Keys k in KeyInputs_Effect.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        CurrentSong[cursorPosition] = KeyInputs_Effect[k];
                        MoveToRow(cursorPosition.Row + InputStep);
                        break;
                    }
                }
            }
            else if (cursorPosition.GetColumnType() == CursorPos.COLUMN_EFFECT_PARAMETER1) {
                // input 10's place effect parameters (or 16's if it is hex)
                if (CurrentPatternEvent.GetEffect((cursorPosition.Column - 5) / 3).ParameterIsHex) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = CurrentSong[cursorPosition];
                            if (val == PatternEvent.EMPTY)
                                val = 0;
                            CurrentSong[cursorPosition] = KeyInputs_Hex[k] * 16 + val % 16;
                            MoveToRow(cursorPosition.Row + InputStep);
                            break;
                        }
                    }
                }
                else {
                    // decimal
                    foreach (Keys k in KeyInputs_Decimal.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = CurrentSong[cursorPosition];
                            if (val == PatternEvent.EMPTY)
                                val = 0;
                            CurrentSong[cursorPosition] = KeyInputs_Decimal[k] * 10 + val % 10;
                            MoveToRow(cursorPosition.Row + InputStep);
                            break;
                        }
                    }
                }
            }
            else if (cursorPosition.GetColumnType() == CursorPos.COLUMN_EFFECT_PARAMETER2) {
                // input 1's place effect parameters
                if (CurrentPatternEvent.GetEffect((cursorPosition.Column - 5) / 3).ParameterIsHex) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = CurrentSong[cursorPosition];
                            if (val == PatternEvent.EMPTY)
                                val = 0;
                            CurrentSong[cursorPosition] = (val / 16 * 16) + KeyInputs_Hex[k];
                            MoveToRow(cursorPosition.Row + InputStep);
                            break;
                        }
                    }
                }
                else {
                    // decimal
                    foreach (Keys k in KeyInputs_Decimal.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = CurrentSong[cursorPosition];
                            if (val == PatternEvent.EMPTY)
                                val = 0;
                            CurrentSong[cursorPosition] = (val / 10 * 10) + KeyInputs_Decimal[k];
                            MoveToRow(cursorPosition.Row + InputStep);
                            break;
                        }
                    }
                }
            }
            #endregion
            #region scroll field modifiers
            if (IsHovered) {
                // scrolling up and down the pattern
                if (Input.MouseScrollWheel(KeyModifier.None) > 0 || Input.GetKeyRepeat(Keys.PageUp, KeyModifier.None)) {
                    MoveToRow(cursorPosition.Row - Preferences.profile.pageJumpAmount);
                }
                if (Input.MouseScrollWheel(KeyModifier.None) < 0 || Input.GetKeyRepeat(Keys.PageDown, KeyModifier.None)) {
                    MoveToRow(cursorPosition.Row + Preferences.profile.pageJumpAmount);
                }

                // scrolling field modifiers
                if (CurrentSong[cursorPosition] != PatternEvent.EMPTY) {
                    if (cursorPosition.GetColumnType() == CursorPos.COLUMN_NOTE) {
                        // if cursor is on the note column, use SCROLL+CTRL to transpose
                        CurrentSong[cursorPosition] += Input.MouseScrollWheel(KeyModifier.Ctrl);
                    }
                    else if (cursorPosition.GetColumnType() >= CursorPos.COLUMN_INSTRUMENT1 && cursorPosition.GetColumnType() <= CursorPos.COLUMN_INSTRUMENT2) {
                        // if cursor is instrument or volume, use SCROLL+SHIFT to adjust
                        CurrentSong[cursorPosition] += Input.MouseScrollWheel(KeyModifier.Shift);
                    }
                    else if (cursorPosition.GetColumnType() != CursorPos.COLUMN_EFFECT) {
                        // if cursor is an effect parameter
                        if (CurrentPatternEvent.GetEffect((cursorPosition.Column - 5) / 3).ParameterIsHex) {
                            // if effect is hex, adjust each digit independently
                            if (cursorPosition.GetColumnType() == CursorPos.COLUMN_EFFECT_PARAMETER1)
                                CurrentSong[cursorPosition] += Input.MouseScrollWheel(KeyModifier.Shift) * 16;
                            else
                                CurrentSong[cursorPosition] += Input.MouseScrollWheel(KeyModifier.Shift);
                        }
                        else {
                            // if effect is decimal, use SCROLL+SHIFT to adjust
                            CurrentSong[cursorPosition] += Input.MouseScrollWheel(KeyModifier.Shift);
                        }
                    }
                }
            }
            #endregion


            #region backspace + delete
            if (KeyPress(Keys.Back, KeyModifier.None)) {
                if (cursorPosition.Column == CursorPos.COLUMN_NOTE) {
                    if (selectionActive) {
                        int frameWrap = 0;
                        CursorPos p = selectionMin;
                        while (!p.IsRightOf(selectionMax)) {
                            p.Frame = selectionMin.Frame;
                            p.Row = selectionMin.Row;
                            while (!p.IsBelow(selectionMin)) {
                                PullRowsUp(p);
                                p.MoveToRow(p.Row + 1, CurrentSong, ref frameWrap);
                            }
                            p.MoveRight(CurrentSong);
                        }
                    }
                    else {
                        MoveToRow(cursorPosition.Row - 1);
                        PullRowsUp(cursorPosition.Frame, cursorPosition.Row, cursorPosition.Channel);
                    }
                    AddToUndoHistory();
                }
            }
            if (KeyPress(Keys.Delete, KeyModifier.None)) {
                if (cursorPosition.Column == CursorPos.COLUMN_NOTE) {
                    CurrentPatternEvent.Note = PatternEvent.EMPTY;
                    CurrentPatternEvent.Instrument = PatternEvent.EMPTY;
                    MoveToRow(cursorPosition.Row + 1);
                    AddToUndoHistory();
                }
            }
            #endregion
        }

        public void Draw() {

            DrawRect(0, 0, width, height, Colors.theme.background);

            DrawHeaderRect(0, -32, width);
            DrawRect(ROW_COLUMN_WIDTH - 2, -32, channelHeaders[LastVisibleChannel].x + channelHeaders[LastVisibleChannel].width - ROW_COLUMN_WIDTH + 4, 1, Colors.theme.rowSeparator);

            CursorPos renderPos = cursorPosition;
            int frameWrap = 0;
            renderPos.MoveToRow(renderPos.Row - NumVisibleLines / 2, WTSong.currentSong, ref frameWrap);
            for (int i = 0; i < NumVisibleLines; i++) {
                DrawRowBG(i, renderPos.Frame, renderPos.Row, frameWrap);

                renderPos.MoveToRow(renderPos.Row + 1, WTSong.currentSong, ref frameWrap);
            }

            // draw cursor behind text if the cursor cel is not empty
            if (!cursorPosition.IsPositionEmpty(CurrentSong))
                DrawCursor(cursorPosition);


            renderPos = cursorPosition;
            frameWrap = 0;
            renderPos.MoveToRow(renderPos.Row - NumVisibleLines / 2, WTSong.currentSong, ref frameWrap);
            for (int i = 0; i < NumVisibleLines; i++) {
                DrawRow(i, renderPos.Frame, renderPos.Row, frameWrap);
                if (frameWrap != 0)
                    DrawRect(0, i * 7, width, 7, Helpers.Alpha(Colors.theme.background, 180));
                renderPos.MoveToRow(renderPos.Row + 1, WTSong.currentSong, ref frameWrap);
            }

            // draw cursor infront of text if the cursor cel is empty
            if (cursorPosition.IsPositionEmpty(CurrentSong))
                DrawCursor(cursorPosition);

            DrawRect(MouseX, MouseY, 1, 1, Color.Green);

            DrawRect(ROW_COLUMN_WIDTH - 1, -32, 1, height + 32, Colors.theme.rowSeparator);
            for (int i = FirstVisibleChannel; i <= LastVisibleChannel; ++i) {
                channelHeaders[i].Draw();
                //DrawRect(channelHeaders[i].x + channelHeaders[i].width - 1, -32, 3, 1, Colors.theme.rowSeparator);
                //DrawRect(channelHeaders[i].x - 2, -32, 3, 1, Colors.theme.rowSeparator);
                //DrawRect(channelHeaders[i].x + channelHeaders[i].width + 1, -32, 1, 1, Colors.theme.rowSeparator);
                DrawRect(channelHeaders[i].x + channelHeaders[i].width, -32, 1, height + 32, Colors.theme.rowSeparator);
            }
            // DrawRect(channelHeaders[LastVisibleChannel].x + channelHeaders[LastVisibleChannel].x - 2, -32, 3, 1, Colors.theme.rowSeparator);

            DrawRect(width, -32, 1, height + 32, Colors.theme.rowSeparator);
            DrawRect(width + 1, -32, 1, 1, Colors.theme.rowSeparator);
        }
        #region Draw Methods
        void DrawRow(int line, int frame, int row, int frameWrap) {
            // get the row color
            Color rowTextColor = Colors.theme.patternText;
            if (row % Song.currentSong.rowHighlight1 == 0)
                rowTextColor = Colors.theme.patternTextHighlighted;
            else if (row % Song.currentSong.rowHighlight2 == 0)
                rowTextColor = Colors.theme.patternTextSubHighlight;

            // draw row numbers
            if (Preferences.profile.showRowNumbersInHex)
                WriteMonospaced(row.ToString("X2"), 6, line * 7, rowTextColor, 4);
            else
                WriteMonospaced(row.ToString("D3"), 4, line * 7, rowTextColor, 4);

            // draw pattern events
            for (int channel = FirstVisibleChannel; channel <= LastVisibleChannel; ++channel) {
                DrawPatternEvent(frame, row, channel, frameWrap, GetStartPositionOfChannel(channel), line * 7, CurrentSong.NumEffectColumns[channel], rowTextColor);
            }
        }

        void DrawPatternEvent(int frame, int row, int channel, int frameWrap, int x, int y, int effectColumns, Color rowTextColor) {

            PatternEvent thisEvent = CurrentSong[frame][row][channel];

            bool isCursorOnThisRow = frameWrap == 0 && cursorPosition.Row == row;
            bool isCursorOnThisEvent = isCursorOnThisRow && cursorPosition.Channel == channel;
            Color emptyColor;
            if (isCursorOnThisRow)
                emptyColor = (EditMode ? Colors.theme.rowEditText : Colors.theme.rowCurrentText);
            else
                emptyColor = Helpers.Alpha(rowTextColor, Colors.theme.patternEmptyTextAlpha);

            // draw note

            if (thisEvent.Note == PatternEvent.NOTE_CUT) {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("OFF", x + 2, y, rowTextColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 2, rowTextColor);
                }
            }
            else if (thisEvent.Note == PatternEvent.NOTE_RELEASE) {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("REL", x + 2, y, rowTextColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 1, rowTextColor);
                    DrawRect(x + 3, y + 4, 13, 1, rowTextColor);
                }
            }
            else if (thisEvent.Note == PatternEvent.EMPTY) {
                WriteMonospaced("···", x + 3, y, emptyColor, 4);
            }
            else {
                string noteName = Helpers.MIDINoteToText(thisEvent.Note);
                if (noteName.Contains('#')) {
                    Write(noteName, x + 2, y, rowTextColor);
                }
                else {
                    WriteMonospaced(noteName[0] + "-", x + 2, y, rowTextColor, 5);
                    Write(noteName[2] + "", x + 13, y, rowTextColor);
                }
            }

            // draw instrument column
            if (thisEvent.Instrument == PatternEvent.EMPTY) {
                WriteMonospaced("··", x + 22, y, emptyColor, 4);
            }
            else {
                Color instrumentColor;
                if (thisEvent.Instrument < WTModule.currentModule.Instruments.Count) {
                    if (WTModule.currentModule.Instruments[thisEvent.Instrument].macroType == MacroType.Wave)
                        instrumentColor = Colors.theme.instrumentColumnWave;
                    else
                        instrumentColor = Colors.theme.instrumentColumnSample;
                }
                else {
                    instrumentColor = Color.Red;
                }
                WriteMonospaced(thisEvent.Instrument.ToString("D2"), x + 21, y, instrumentColor, 4);
            }

            // draw volumn column
            if (thisEvent.Volume == PatternEvent.EMPTY) {
                WriteMonospaced("··", x + 35, y, emptyColor, 4);
            }
            else {
                Color volumeColor;
                bool isCursorOverThisVolumeText = isCursorOnThisEvent && (cursorPosition.Column == 3 || cursorPosition.Column == 4);
                if (Preferences.profile.fadeVolumeColumn && !isCursorOverThisVolumeText) {
                    volumeColor = Helpers.Alpha(Colors.theme.volumeColumn, (int)(thisEvent.Volume / 100f * 180 + (255 - 180)));
                }
                else {
                    volumeColor = Colors.theme.volumeColumn;
                }

                WriteMonospaced(thisEvent.Volume.ToString("D2"), x + 34, y, volumeColor, 4);
            }

            for (int i = 0; i < effectColumns; ++i) {
                if (thisEvent.GetEffect(i).Type == PatternEvent.EMPTY) {
                    WriteMonospaced("···", x + 48 + 18 * i, y, emptyColor, 4);
                }
                else {
                    Write(thisEvent.GetEffect(i).Type + "", x + 47 + 18 * i, y, Colors.theme.effectColumn);
                    WriteMonospaced(thisEvent.GetEffect(i).GetParameterAsString(), x + 52 + 18 * i, y, Colors.theme.effectColumnParameter, 4);
                }
            }
        }

        void DrawHeaderRect(int x, int y, int width) {
            DrawRect(x, y, width, 31, Color.White);
            DrawRect(x, y + 20, width, 11, new Color(223, 224, 232));
        }
        void DrawCursor(CursorPos position) {
            Rectangle rect = GetRectFromCursorPos(position);
            int width = position.Column == 0 ? 17 : 6;
            int offset = position.Column switch {
                7 or 10 or 13 or 16 => -1,
                2 or 4 or 6 or 9 or 12 or 15 => 0,
                _ => 1
            };
            DrawRect(rect.X + offset, rect.Y, width, 7, Colors.theme.cursor);
            //Write("Chan: " + (position.Channel + 1), rect.X, rect.Y + 10, Color.White);
            //Write("Col: " + position.Column, rect.X, rect.Y + 20, Color.White);
            //Write("Oct: " + CurrentOctave, rect.X, rect.Y + 30, Color.White);
        }

        void DrawRowBG(int line, int frame, int row, int frameWrap) {
            CursorPos thisPos = new CursorPos();
            thisPos.Row = row;
            thisPos.Frame = frame;
            Color rowBGcolor = new Color();
            bool needToDrawRowBG = true;
            int linePositionY = line * 7;

            if (frame == cursorPosition.Frame && row == cursorPosition.Row && frameWrap == 0) {
                rowBGcolor = EditMode ? Colors.theme.rowEditColor : Colors.theme.rowCurrentColor;
            }
            else if (Playback.isPlaying && Playback.playbackFrame == frame && Playback.playbackRow == row) {
                rowBGcolor = Colors.theme.rowPlaybackColor;
            }
            else if (row % WTSong.currentSong.RowHighlightPrimary == 0) {
                rowBGcolor = Colors.theme.backgroundHighlighted;
            }
            else if (row % WTSong.currentSong.RowHighlightSecondary == 0) {
                rowBGcolor = Colors.theme.backgroundSubHighlight;
            }
            else {
                // this row is not highlighted, no need to draw a background
                needToDrawRowBG = false;
            }

            if (needToDrawRowBG) {
                DrawRect(ROW_COLUMN_WIDTH, linePositionY, width - ROW_COLUMN_WIDTH, 7, rowBGcolor);
            }

            // if this row is within the selection bounds
            if (selectionActive && !thisPos.IsAbove(selectionMin) && !thisPos.IsBelow(selectionMax)) {
                int start = channelHeaders[selectionMin.Channel].x + columnStartPosOffsets[selectionMin.Column];
                int end = channelHeaders[selectionMax.Channel].x + columnStartPosOffsets[selectionMax.Column] + columnSpaceWidths[selectionMax.Column];
                bool endsOnRowSeparator = selectionMax.GetColumnType() == CursorPos.COLUMN_EFFECT_PARAMETER2;
                if (endsOnRowSeparator)
                    end--;
                if (start < ROW_COLUMN_WIDTH - 1)
                    start = ROW_COLUMN_WIDTH - 1;
                if (end < ROW_COLUMN_WIDTH - 1)
                    end = ROW_COLUMN_WIDTH - 1;
                DrawRect(start, linePositionY, end - start + 1, 7, Colors.theme.selection);

                // draw selection outline
                DrawRect(start, linePositionY, 1, 7, Colors.theme.selection);
                DrawRect(end, linePositionY, 1, 7, Colors.theme.selection);
                if (selectionMin.Row == row && selectionMin.Frame == frame)
                    DrawRect(start, linePositionY, end - start, 1, Colors.theme.selection);
                if (selectionMax.Row == row && selectionMax.Frame == frame)
                    DrawRect(start, linePositionY + 6, end - start, 1, Colors.theme.selection);
            }
        }
        #endregion


        public void OnSwitchSong() {
            MoveToChannel(0);
            MoveToFrame(0);
            MoveToRow(0);
            FirstVisibleChannel = 0;
        }

        /// <summary>
        /// Moves the cursor to a row
        /// </summary>
        /// <param name="row"></param>
        void MoveToRow(int row) {

            bool wrapFrames = true;
            bool wrapCursor = true;
            if (wrapFrames) {
                while (row < 0) {
                    MoveToFrame(cursorPosition.Frame - 1);
                    row += CurrentFrame.GetLength();
                }
                while (row >= CurrentFrame.GetLength()) {
                    row -= CurrentFrame.GetLength();
                    MoveToFrame(cursorPosition.Frame + 1);
                }
            }
            else if (wrapCursor) {
                row %= CurrentFrame.GetLength();
                if (row < 0) {
                    row += CurrentFrame.GetLength();
                }
            }
            else {
                row = Math.Clamp(row, 0, CurrentFrame.GetLength() - 1);
            }
            cursorPosition.Row = row;
        }

        /// <summary>
        /// Moves the cursor to a frame
        /// </summary>
        /// <param name="frame"></param>
        void MoveToFrame(int frame) {
            int numFrames = WTSong.currentSong.FrameSequence.Count;

            frame %= numFrames;
            if (frame < 0) {
                frame += numFrames;
            }
            cursorPosition.Frame = frame;

            // make sure the cursor is in bounds of this frame
            cursorPosition.Row = Math.Clamp(cursorPosition.Row, 0, CurrentFrame.GetLength() - 1);
        }

        /// <summary>
        /// Moves the cursor one column to the left
        /// </summary>
        void MoveCursorLeft() {
            int column = cursorPosition.Column - 1;
            if (column < 0) {
                MoveToChannel(cursorPosition.Channel - 1);
                column = WTSong.currentSong.GetNumColumnsOfChannel(cursorPosition.Channel) - 1;
            }
            cursorPosition.Column = column;
        }

        /// <summary>
        /// Moves the cursor one column to the right
        /// </summary>
        void MoveCursorRight() {
            int column = cursorPosition.Column + 1;
            if (column > WTSong.currentSong.GetNumColumnsOfChannel(cursorPosition.Channel) - 1) {
                MoveToChannel(cursorPosition.Channel + 1);
                column = 0;
            }
            cursorPosition.Column = column;
        }

        /// <summary>
        /// Moves the cursor to the first column of a channel
        /// </summary>
        /// <param name="channel"></param>
        void MoveToChannel(int channel) {
            channel %= WTSong.currentSong.ChannelCount;
            if (channel < 0) {
                channel += WTSong.currentSong.ChannelCount;
            }
            cursorPosition.Channel = channel;
            cursorPosition.Column = 0;
        }


        /// <summary>
        /// Pulls all rows of the current channel below startRow up one, leaving a blank row at the end.
        /// </summary>
        /// <param name="startRow"></param>
        void PullRowsUp(int frame, int startRow, int channel) {
            for (int i = startRow; i < 255; i++) {
                CurrentSong[frame][i][channel].CopyFrom(CurrentSong[frame][i + 1][channel]);
            }
            CurrentSong[frame][255][channel].SetEmpty();
        }

        /// <summary>
        /// Pulls all rows of the current channel below startRow up one, leaving a blank row at the end.
        /// </summary>
        /// <param name="startRow"></param>
        void PullRowsUp(CursorPos pos) {
            for (int i = pos.Row; i < 255; i++) {
                CurrentSong[pos.Frame][i][pos.Channel][pos.Column] = CurrentSong[pos.Frame][i + 1][pos.Channel][pos.Column];
            }
            CurrentSong[pos.Frame][255][pos.Channel][pos.Column] = PatternEvent.EMPTY;
        }

        void AddToUndoHistory() {

        }

        /// <summary>
        /// Moves to the next frame in the song
        /// </summary>
        public void NextFrame() {
            MoveToFrame(cursorPosition.Frame + 1);
            CancelSelection();
        }

        /// <summary>
        /// Moves to the previous frame in the song
        /// </summary>
        public void PreviousFrame() {
            MoveToFrame(cursorPosition.Frame - 1);
            CancelSelection();
        }

        /// <summary>
        /// Moves the cursor to the first row of the current frame
        /// </summary>
        void GoToTopOfFrame() {
            cursorPosition.Row = 0;
        }

        /// <summary>
        /// Moves the cursor to the last row of the current frame
        /// </summary>
        void GoToBottomOfFrame() {
            cursorPosition.Row = CurrentFrame.GetLength() - 1;
        }


        /// <summary>
        /// Gets a key press, taking into account the key repeat setting in preferences
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyModifier"></param>
        /// <returns></returns>
        bool KeyPress(Keys key, KeyModifier keyModifier) {
            if (Preferences.profile.keyRepeat)
                return Input.GetKeyRepeat(key, keyModifier);
            else
                return Input.GetKeyDown(key, keyModifier);
        }

        /// <summary>
        /// Sets the selection start position
        /// </summary>
        /// <param name="pos"></param>
        void SetSelectionStart(CursorPos pos) {
            selectionStart = pos;
        }

        /// <summary>
        /// Sets the selection end position
        /// </summary>
        /// <param name="pos"></param>
        void SetSelectionEnd(CursorPos pos) {
            selectionEnd = pos;
        }


        /// <summary>
        /// Sets selectionMin and selectionMax to the upper-right and bottom-left of the selection bounds from selectionStart and selectionEnd
        /// </summary>
        void CreateSelectionBounds() {
            selectionMin = selectionStart;
            selectionMax = selectionEnd;
            // perform any swaps necessary
            if (selectionStart.IsBelow(selectionEnd)) {
                selectionMin.Row = selectionEnd.Row;
                selectionMin.Frame = selectionEnd.Frame;
                selectionMax.Row = selectionStart.Row;
                selectionMax.Frame = selectionStart.Frame;
            }
            if (selectionStart.IsRightOf(selectionEnd)) {
                selectionMax.Channel = selectionStart.Channel;
                selectionMax.Column = selectionStart.Column;
                selectionMin.Channel = selectionEnd.Channel;
                selectionMin.Column = selectionEnd.Column;
            }

            // if the user selected on the row column, set the selection to span all channels
            if (selectionMax.Channel == -1 && selectionMin.Channel == -1) {
                selectionMin.Channel = 0;
                selectionMin.Column = 0;
                selectionMax.Channel = CurrentSong.ChannelCount - 1;
                selectionMax.Column = GetNumFullColumns(selectionMax.Channel) - 1;
            }

            // adjust the cursor positions to fill all of mulit-digit fields like instrument, volume and effects
            if (selectionMax.Column == 1)
                selectionMax.Column = 2;
            else if (selectionMax.Column == 3)
                selectionMax.Column = 4;
            else if (selectionMax.Column == 5 || selectionMax.Column == 6)
                selectionMax.Column = 7;
            else if (selectionMax.Column == 8 || selectionMax.Column == 9)
                selectionMax.Column = 10;
            else if (selectionMax.Column == 11 || selectionMax.Column == 12)
                selectionMax.Column = 13;
            else if (selectionMax.Column == 14 || selectionMax.Column == 15)
                selectionMax.Column = 16;
            if (selectionMin.Column == 2)
                selectionMin.Column = 1;
            else if (selectionMin.Column == 4)
                selectionMin.Column = 3;
            else if (selectionMin.Column == 6 || selectionMin.Column == 7)
                selectionMin.Column = 5;
            else if (selectionMin.Column == 9 || selectionMin.Column == 10)
                selectionMin.Column = 8;
            else if (selectionMin.Column == 12 || selectionMin.Column == 13)
                selectionMin.Column = 11;
            else if (selectionMin.Column == 15 || selectionMin.Column == 16)
                selectionMin.Column = 14;

            if (selectionMin.Channel < 0)
                selectionMin.Channel = 0;
            if (selectionMax.Channel < 0)
                selectionMax.Channel = 0;
            if (selectionMax.Column >= GetNumFullColumns(selectionMax.Channel))
                selectionMax.Column = GetNumFullColumns(selectionMax.Channel) - 1;
        }

        /// <summary>
        /// Populates the selection array with all the CursorPositions inside the selection bounds
        /// </summary>
        void GetSelectionPositions() {
            int frameWrap = 0;
            CursorPos p = selectionMin;
            selection.Clear();
            while (!p.IsRightOf(selectionMax)) {
                p.Row = selectionMin.Row;
                p.Frame = selectionMin.Frame;
                while (!p.IsBelow(selectionMax)) {
                    selection.Add(p);
                    p.MoveToRow(p.Row + 1, CurrentSong, ref frameWrap);
                }
                p.MoveRight(CurrentSong);
            }
        }

        /// <summary>
        /// Removes the selection
        /// </summary>
        void CancelSelection() {
            selectionActive = false;
            selectionMin = cursorPosition;
            selectionMax = cursorPosition;
            CreateSelectionBounds();
        }

        /// <summary>
        /// Calculates where channels are positioned based on the horziontal scroll and their expansions
        /// </summary>
        void CalculateChannelPositioning() {
            // get the total width of channels that are not visible on the left side
            int prevWidth = 0;
            for (int i = 0; i < FirstVisibleChannel; ++i) {
                prevWidth += GetWidthOfChannel(i);
            }

            // figure out which channel headers are visible, and enable/position/update them
            int px = ROW_COLUMN_WIDTH - prevWidth;
            for (int channel = 0; channel < channelHeaders.Length; ++channel) {

                if (px >= ROW_COLUMN_WIDTH && px < width) {
                    channelHeaders[channel].enabled = true;
                    LastVisibleChannel = channel;
                }
                else {
                    channelHeaders[channel].enabled = false;
                }
                channelHeaders[channel].x = px;
                channelHeaders[channel].width = GetWidthOfChannel(channel) - 1;
                channelHeaders[channel].NumEffectColumns = CurrentSong.NumEffectColumns[channel];
                channelHeaders[channel].Update();

                // if the user changed the number of effect columns
                if (channelHeaders[channel].NumEffectColumns != CurrentSong.NumEffectColumns[channel]) {
                    CurrentSong.NumEffectColumns[channel] = channelHeaders[channel].NumEffectColumns;
                    channelHeaders[channel].width = GetWidthOfChannel(channel) - 1;
                    cursorPosition.Normalize(CurrentSong);
                }
                px += GetWidthOfChannel(channel);
            }
            LastChannelEndPos = channelHeaders[channelHeaders.Length - 1].x + channelHeaders[channelHeaders.Length - 1].width;
        }


        /// <summary>
        /// Returns the line index the mouse is under (Render row)
        /// </summary>
        /// <returns></returns>
        int GetMouseLineNumber() {
            if (MouseY < 0)
                return -1;
            return MouseY / ROW_HEIGHT;
        }

        /// <summary>
        /// Returns the number of lines to be rendered in the pattern editor
        /// </summary>
        /// <returns></returns>
        int NumVisibleLines {
            get {
                return height / ROW_HEIGHT;
            }
        }


        /// <summary>
        /// Returns the channel under an x position
        /// </summary>
        /// <param name="x"></param>
        /// <returns>Channel number, -1 means the row number's column</returns>
        int GetChannelAtPoint(int x) {

            // row column width
            int width = ROW_COLUMN_WIDTH;

            // return -1 if the mouse is over the row numbers column
            if (x <= width)
                return -1;


            // start at the first visible channel
            int c = FirstVisibleChannel;
            width += GetWidthOfChannel(c);

            while (width <= x) {
                c++;
                if (c >= CurrentSong.ChannelCount)
                    return c - 1;
                width += GetWidthOfChannel(c);
            }
            return c;
        }

        /// <summary>
        /// Returns the column under an x position
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        int GetColumnAtPoint(int x) {

            int channel = GetChannelAtPoint(x);
            if (channel < 0)
                return 0;
            if (channel > LastVisibleChannel)
                return GetNumColumns(channel) - 1;

            x -= GetStartPositionOfChannel(channel);

            int col = 0;
            x -= columnSpaceWidths[col];
            while (x > 0) {
                col++;
                if (col > 16)
                    return col - 1;
                x -= columnSpaceWidths[col];
            }

            return col;

        }

        /// <summary>
        /// Returns the number of columns a channel track has
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        int GetNumColumns(int channel) {
            return 3 + CurrentSong.NumEffectColumns[channel];
        }

        /// <summary>
        /// Returns the number of cursor columns a channel track has
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        int GetNumFullColumns(int channel) {
            return 5 + CurrentSong.NumEffectColumns[channel] * 3;
        }

        /// <summary>
        /// Gets the cursor position under a point
        /// </summary>
        /// <returns></returns>
        CursorPos GetCursorPositionFromPoint(int x, int y) {
            CursorPos p = new CursorPos {
                Frame = cursorPosition.Frame,
                Row = cursorPosition.Row,
                Channel = GetChannelAtPoint(x),
                Column = GetColumnAtPoint(x)
            };

            int lineNum = y / ROW_HEIGHT;
            int frameWrap = 0;

            // move to the first line, then forward lineNum lines.
            p.MoveToRow(p.Row - NumVisibleLines / 2 + lineNum, CurrentSong, ref frameWrap);
            return p;
        }

        /// <summary>
        /// Returns the screen bounds of a cursor position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Rectangle GetRectFromCursorPos(CursorPos position) {

            position.Normalize(CurrentSong);

            int frameWrap = 0;
            CursorPos p = cursorPosition;

            // p.MoveToRow(p.Row, WTSong.currentSong, ref frameWrap);

            int x = channelHeaders[position.Channel].x + columnStartPosOffsets[position.Column];

            int lineNumber = NumVisibleLines / 2;
            while (p.IsBelow(position)) {
                p.MoveToRow(p.Row - 1, CurrentSong, ref frameWrap);
                lineNumber--;
            }
            while (p.IsAbove(position)) {
                p.MoveToRow(p.Row + 1, CurrentSong, ref frameWrap);
                lineNumber++;
            }
            return new Rectangle(x, lineNumber * 7, columnSpaceWidths[position.Column], 7);
        }


        /// <summary>
        /// Returns the starting x position of a channel visible in the frame view
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        int GetStartPositionOfChannel(int channel) {
            return channelHeaders[channel].x;
        }


        /// <summary>
        /// Returns the width of a channel track in pixels
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        int GetWidthOfChannel(int channel) {
            // width of note, inst, vol together is 45
            // width of effect + effect param is 18
            // width of the channel separator is 1
            return 45 + 18 * WTSong.currentSong.NumEffectColumns[channel] + 1;
        }

        /// <summary>
        /// The offset start position of columns from the left of a channel track
        /// </summary>
        readonly int[] columnStartPosOffsets = new int[] { 0, 19, 25, 32, 38, 45, 51, 57, 63, 69, 75, 81, 87, 93, 99, 105, 111 };

        /// <summary>
        /// The width of spaces a column takes up
        /// </summary>
        readonly int[] columnSpaceWidths = new int[] { 19, 6, 7, 6, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };

        #region Key Input Dictionaries
        readonly Dictionary<Keys, int> KeyInputs_Piano = new Dictionary<Keys, int>() {
            {Keys.D1, PatternEvent.NOTE_CUT },
            {Keys.OemPlus, PatternEvent.NOTE_RELEASE },

            { Keys.Z, 0 },
            { Keys.S, 1 },
            { Keys.X, 2 },
            { Keys.D, 3 },
            { Keys.C, 4 },
            { Keys.V, 5 },
            { Keys.G, 6 },
            { Keys.B, 7 },
            { Keys.H, 8 },
            { Keys.N, 9 },
            { Keys.J, 10 },
            { Keys.M, 11 },
            { Keys.OemComma, 12 },
            { Keys.L, 13 },
            { Keys.OemPeriod, 14 },
            { Keys.OemSemicolon, 15 },
            { Keys.OemQuestion, 16 },

            { Keys.Q, 12 },
            { Keys.D2, 13 },
            { Keys.W, 14 },
            { Keys.D3, 15 },
            { Keys.E, 16 },
            { Keys.R, 17 },
            { Keys.D5, 18 },
            { Keys.T, 19 },
            { Keys.D6, 20 },
            { Keys.Y, 21 },
            { Keys.D7, 22 },
            { Keys.U, 23 },
            { Keys.I, 24 },
            { Keys.D9, 25 },
            { Keys.O, 26 },
            { Keys.D0, 27 },
            { Keys.P, 28 },
        };

        readonly Dictionary<Keys, char> KeyInputs_Effect = new Dictionary<Keys, char>() {
            {Keys.D0, '0'},
            {Keys.D1, '1'},
            {Keys.D2, '2'},
            {Keys.D3, '3'},
            {Keys.D4, '4'},
            {Keys.D7, '7'},
            {Keys.D8, '8'},
            {Keys.D9, '9'},
            {Keys.Q, 'Q'},
            {Keys.R, 'R'},
            {Keys.P, 'P'},
            {Keys.F, 'F'},
            {Keys.V, 'V'},
            {Keys.C, 'C'},
            {Keys.D, 'D'},
            {Keys.B, 'B'},
            {Keys.G, 'G'},
            {Keys.S, 'S'},
            {Keys.L, 'L'},
            {Keys.A, 'A'},
            {Keys.W, 'W'},
            {Keys.I, 'I'},
            {Keys.J, 'J'},
            {Keys.M, 'M'},

        };

        readonly Dictionary<Keys, int> KeyInputs_Hex = new Dictionary<Keys, int>() {
            {Keys.D0, 0},
            {Keys.D1, 1},
            {Keys.D2, 2},
            {Keys.D3, 3},
            {Keys.D4, 4},
            {Keys.D5, 5},
            {Keys.D6, 6},
            {Keys.D7, 7},
            {Keys.D8, 8},
            {Keys.D9, 9},
            {Keys.A, 10},
            {Keys.B, 11},
            {Keys.C, 12},
            {Keys.D, 13},
            {Keys.E, 14},
            {Keys.F, 15},
        };

        readonly Dictionary<Keys, int> KeyInputs_Decimal = new Dictionary<Keys, int>() {
            {Keys.D0, 0},
            {Keys.D1, 1},
            {Keys.D2, 2},
            {Keys.D3, 3},
            {Keys.D4, 4},
            {Keys.D5, 5},
            {Keys.D6, 6},
            {Keys.D7, 7},
            {Keys.D8, 8},
            {Keys.D9, 9},
        };
        #endregion
    }
}
