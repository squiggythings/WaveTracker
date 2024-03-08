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
using System.Threading;

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

        public bool FollowMode { get; set; }

        /// <summary>
        /// The position of the main cursor in the song
        /// </summary>
        CursorPos cursorPosition;

        CursorPos lastCursorPosition;
        /// <summary>
        /// The position of the main cursor in the song
        /// </summary>
        public CursorPos CursorPosition { get { return cursorPosition; } }

        /// <summary>
        /// Whether or not there is a selection active in the pattern editor
        /// </summary>
        public bool SelectionIsActive { get; private set; }

        /// <summary>
        /// The start position of a selection the user makes
        /// </summary>
        CursorPos selectionStart;

        /// <summary>
        /// The end position of a selection the user makes
        /// </summary>
        CursorPos selectionEnd;



        /// <summary>
        /// Used for optimization purposes
        /// </summary>
        CursorPos lastSelectionStart, lastSelectionEnd;

        /// <summary>
        /// A list of all positions in the selection
        /// </summary>
        List<CursorPos> selection;

        PatternSelection currentSelection;

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

        /// <summary>
        /// The list of states that could be reverted back to
        /// </summary>
        FixedSizeStack<WTSongState> undoHistory;

        /// <summary>
        /// The list of states that could be reverted forward to
        /// </summary>
        FixedSizeStack<WTSongState> redoHistory;


        WChannelHeader[] channelHeaders;

        WTFrame CurrentFrame => App.CurrentSong.FrameSequence[cursorPosition.Frame];
        WTPattern CurrentPattern => CurrentFrame.GetPattern();


        public PatternEditor(int x, int y) {
            Playback.patternEditor = this;
            this.x = x;
            this.y = y;
            channelHeaders = new WChannelHeader[WTModule.DEFAULT_CHANNEL_COUNT];
            for (int i = 0; i < channelHeaders.Length; ++i) {
                channelHeaders[i] = new WChannelHeader(0, -32, 63, i, this);
            }

            InputStep = 1;
            CurrentOctave = 4;
            undoHistory = new FixedSizeStack<WTSongState>();
            redoHistory = new FixedSizeStack<WTSongState>();
            selection = new List<CursorPos>();
            currentSelection = new PatternSelection();
        }

        public void Update() {

            // responsive height
            int bottomMargin = 3;
            height = App.WindowHeight - y - bottomMargin;

            // responsive width
            int rightMargin = 156;
            width = App.WindowWidth - x - rightMargin - 1;

            FirstVisibleChannel -= Input.MouseScrollWheel(KeyModifier.Alt);
            FirstVisibleChannel = Math.Clamp(FirstVisibleChannel, 0, App.CurrentModule.ChannelCount - 1);


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

            if (Input.focus != null)
                return;
            lastCursorPosition = cursorPosition;

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
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                MoveCursorLeft();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                MoveCursorRight();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.ShiftAlt)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                MoveToChannel(cursorPosition.Channel - 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.ShiftAlt)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                MoveToChannel(cursorPosition.Channel + 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                MoveToRow(cursorPosition.Row + 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Up, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                MoveToRow(cursorPosition.Row - 1);
                SetSelectionEnd(cursorPosition);
            }
            #endregion
            #region selection with mouse
            if (Input.GetClick(KeyModifier.None) && Input.MouseIsDragging && GlobalPointIsInBounds(Input.lastClickLocation)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(GetCursorPositionFromPoint(LastClickPos.X, LastClickPos.Y));
                    SelectionIsActive = true;
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
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    SelectionIsActive = true;
                }
                SetSelectionEnd(GetCursorPositionFromPoint(MouseX, MouseY));
            }
            #endregion
            #region selection with CTRL-A and ESC
            if (Input.GetKeyRepeat(Keys.A, KeyModifier.Ctrl)) {
                if (currentSelection.maxPosition.Channel == cursorPosition.Channel && currentSelection.maxPosition.Row == CurrentPattern.GetModifiedLength() - 1 && currentSelection.maxPosition.Frame == cursorPosition.Frame && currentSelection.maxPosition.Column == CurrentSong.GetNumCursorColumns(cursorPosition.Channel) - 1 &&
                    currentSelection.minPosition.Channel == cursorPosition.Channel && currentSelection.minPosition.Row == 0 && currentSelection.minPosition.Frame == cursorPosition.Frame && currentSelection.minPosition.Column == 0) {
                    SetSelectionStart(cursorPosition);
                    selectionStart.Channel = 0;
                    selectionStart.Row = 0;
                    selectionStart.Column = 0;
                    SetSelectionEnd(cursorPosition);
                    selectionEnd.Channel = CurrentSong.ChannelCount - 1;
                    selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                    selectionEnd.Column = CurrentSong.GetNumCursorColumns(selectionEnd.Channel) - 1;
                }
                else {
                    SelectionIsActive = true;
                    SetSelectionStart(cursorPosition);
                    selectionStart.Row = 0;
                    selectionStart.Column = 0;
                    SetSelectionEnd(cursorPosition);
                    selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                    selectionEnd.Column = CurrentSong.GetNumCursorColumns(selectionEnd.Channel) - 1;
                }
            }
            if (Input.GetKeyRepeat(Keys.Escape, KeyModifier.None)) {
                CancelSelection();
            }
            #endregion


            if (!SelectionIsActive) {
                selectionStart = selectionEnd = cursorPosition;
            }
            //currentSelection.Set(CurrentSong, selectionStart, selectionEnd);
            //CreateSelectionBounds();

            if (Input.GetKeyDown(Keys.Space, KeyModifier.None)) {
                EditMode = !EditMode;
            }
            if (!EditMode)
                return;

            /////////////////////////////////
            //           EDITING           //
            /////////////////////////////////
            #region field input
            int currentCellColumn = cursorPosition.GetCellColumn();
            if (cursorPosition.GetCellIndex() == 0) {
                // input note column
                foreach (Keys k in KeyInputs_Piano.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int note = KeyInputs_Piano[k];
                        if (note != PatternEvent.NOTE_CUT && note != PatternEvent.NOTE_RELEASE)
                            note += (CurrentOctave + 1) * 12;
                        CurrentPattern[cursorPosition.Row, cursorPosition.Row, CellType.Note] = (byte)note;
                        if (!InstrumentMask) {
                            CurrentPattern[cursorPosition.Row, cursorPosition.Row, CellType.Instrument] = (byte)InstrumentBank.CurrentInstrumentIndex;
                        }
                        MoveToRow(cursorPosition.Row + InputStep);
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Instrument1 || cursorPosition.Column == CursorColumnType.Volume1) {
                // input 10's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = App.CurrentSong[cursorPosition];
                        if (val == PatternEvent.EMPTY)
                            val = 0;
                        App.CurrentSong[cursorPosition] = (byte)(KeyInputs_Decimal[k] * 10 + val % 10);
                        MoveToRow(cursorPosition.Row + InputStep);
                        break;
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Instrument2 || cursorPosition.Column == CursorColumnType.Volume2) {
                // input 1's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = App.CurrentSong[cursorPosition];
                        if (val == PatternEvent.EMPTY)
                            val = 0;
                        App.CurrentSong[cursorPosition] = (byte)((val / 10 * 10) + KeyInputs_Decimal[k]);
                        MoveToRow(cursorPosition.Row + InputStep);
                        break;
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Effect1 ||
                     cursorPosition.Column == CursorColumnType.Effect2 ||
                     cursorPosition.Column == CursorColumnType.Effect3 ||
                     cursorPosition.Column == CursorColumnType.Effect4) {
                // input effects
                foreach (Keys k in KeyInputs_Effect.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        App.CurrentSong[cursorPosition] = (byte)KeyInputs_Effect[k];
                        MoveToRow(cursorPosition.Row + InputStep);
                        break;
                    }
                }
            }
            else if (cursorPosition.GetColumnAsSingleEffectChannel() == CursorPos.COLUMN_EFFECT_PARAMETER1) {
                // input 10's place effect parameters (or 16's if it is hex)
                if (CurrentPatternEvent.GetEffect((cursorPosition.Column - 5) / 3).ParameterIsHex) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = CurrentSong[cursorPosition];
                            if (val == PatternEvent.EMPTY)
                                val = 0;
                            CurrentSong[cursorPosition] = (byte)(KeyInputs_Hex[k] * 16 + val % 16);
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
                            CurrentSong[cursorPosition] = (byte)(KeyInputs_Decimal[k] * 10 + val % 10);
                            MoveToRow(cursorPosition.Row + InputStep);
                            break;
                        }
                    }
                }
            }
            else if (cursorPosition.GetColumnAsSingleEffectChannel() == CursorPos.COLUMN_EFFECT_PARAMETER2) {
                // input 1's place effect parameters
                if (CurrentPatternEvent.GetEffect((cursorPosition.Column - 5) / 3).ParameterIsHex) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = CurrentSong[cursorPosition];
                            if (val == PatternEvent.EMPTY)
                                val = 0;
                            CurrentSong[cursorPosition] = (byte)((val / 16 * 16) + KeyInputs_Hex[k]);
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
                            CurrentSong[cursorPosition] = (byte)((val / 10 * 10) + KeyInputs_Decimal[k]);
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
                    if (cursorPosition.GetColumnAsSingleEffectChannel() == CursorPos.COLUMN_NOTE) {
                        // if cursor is on the note column, use SCROLL+CTRL to transpose
                        CurrentSong[cursorPosition] += (byte)Input.MouseScrollWheel(KeyModifier.Ctrl);
                    }
                    else if (cursorPosition.GetColumnAsSingleEffectChannel() >= CursorPos.COLUMN_INSTRUMENT1 && cursorPosition.GetColumnAsSingleEffectChannel() <= CursorPos.COLUMN_INSTRUMENT2) {
                        // if cursor is instrument or volume, use SCROLL+SHIFT to adjust
                        CurrentSong[cursorPosition] += (byte)Input.MouseScrollWheel(KeyModifier.Shift);
                    }
                    else if (cursorPosition.GetColumnAsSingleEffectChannel() != CursorPos.COLUMN_EFFECT) {
                        // if cursor is an effect parameter
                        if (CurrentPatternEvent.GetEffect((cursorPosition.Column - 5) / 3).ParameterIsHex) {
                            // if effect is hex, adjust each digit independently
                            if (cursorPosition.GetColumnAsSingleEffectChannel() == CursorPos.COLUMN_EFFECT_PARAMETER1)
                                CurrentSong[cursorPosition] += (byte)(Input.MouseScrollWheel(KeyModifier.Shift) * 16);
                            else
                                CurrentSong[cursorPosition] += (byte)Input.MouseScrollWheel(KeyModifier.Shift);
                        }
                        else {
                            // if effect is decimal, use SCROLL+SHIFT to adjust
                            CurrentSong[cursorPosition] += (byte)Input.MouseScrollWheel(KeyModifier.Shift);
                        }
                    }
                }
            }
            #endregion
            #region backspace + insert + delete
            if (KeyPress(Keys.Back, KeyModifier.None)) {
                if (SelectionIsActive) {
                    for (int x = currentSelection.SelectionPositions.GetLength(0) - 1; x >= 0; x--) {
                        for (int y = currentSelection.SelectionPositions.GetLength(1) - 1; y >= 0; y--) {
                            PullRowsUp(currentSelection.SelectionPositions[x, y]);
                        }
                    }
                    CancelSelection();
                }
                else if (cursorPosition.Row > 0) {
                    MoveToRow(cursorPosition.Row - 1);
                    CursorPos p = cursorPosition;
                    for (int i = 0; i < CurrentSong.GetNumCursorColumns(cursorPosition.Channel); ++i) {
                        p.GoToBeginningOfColumnType((CursorColumnType)i);
                        PullRowsUp(p);
                    }
                }
                AddToUndoHistory();
            }
            if (KeyPress(Keys.Insert, KeyModifier.None)) {
                if (SelectionIsActive) {
                    CursorPos p = currentSelection.minPosition;
                    while (p.IsLeftOf(currentSelection.maxPosition)) {
                        PushRowsDown(p);
                        p.MoveRight(CurrentSong);
                    }

                    for (int x = 0; x < currentSelection.SelectionPositions.GetLength(0); x++) {
                        PushRowsDown(currentSelection.SelectionPositions[x, 0]);
                    }
                    //PushRowsDown(p);
                }
                else {
                    CursorPos p = cursorPosition;
                    for (int i = 0; i < CurrentSong.GetNumCursorColumns(cursorPosition.Channel); ++i) {
                        p.GoToBeginningOfColumnType((CursorColumnType)i);
                        PushRowsDown(p);
                    }
                }
                AddToUndoHistory();
            }
            if (KeyPress(Keys.Delete, KeyModifier.None)) {
                CursorPos p = currentSelection.minPosition;
                do {
                    p.Channel = currentSelection.minPosition.Channel;
                    p.Column = currentSelection.minPosition.Column;
                    do {
                        CurrentSong[p] = PatternEvent.EMPTY;
                        p.MoveRight(CurrentSong);
                    } while (p.IsLeftOf(currentSelection.maxPosition));
                    p.MoveToRow(p.Row + 1, CurrentSong);
                } while (p.IsAbove(currentSelection.maxPosition));
                if (!SelectionIsActive) {
                    // if no selection and we delete the note, also delete the instrument and volume
                    if (cursorPosition.GetColumnType() == CursorColumnType.Note) {
                        CurrentPatternEvent.Instrument = PatternEvent.EMPTY;
                        CurrentPatternEvent.Volume = PatternEvent.EMPTY;
                    }
                    MoveToRow(cursorPosition.Row + 1);
                }
                AddToUndoHistory();
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
                DrawPatternEvent(frame, row, channel, frameWrap, GetStartPositionOfChannel(channel), line * 7, App.CurrentSong.NumEffectColumns[channel], rowTextColor);
            }
        }

        void DrawPatternEvent(int frame, int row, int channel, int frameWrap, int x, int y, int effectColumns, Color rowTextColor) {

            //PatternEvent thisEvent = App.CurrentSong[frame][row][channel];
            int noteValue = CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument];
            int instrumentValue = CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument];
            int volumeValue = CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument];

            bool isCursorOnThisRow = frameWrap == 0 && cursorPosition.Row == row;
            bool isCursorOnThisEvent = isCursorOnThisRow && cursorPosition.Channel == channel;
            Color emptyColor;
            if (isCursorOnThisRow)
                emptyColor = (EditMode ? Colors.theme.rowEditText : Colors.theme.rowCurrentText);
            else
                emptyColor = Helpers.Alpha(rowTextColor, Colors.theme.patternEmptyTextAlpha);

            // draw note

            if (noteValue == PatternEvent.NOTE_CUT) {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("OFF", x + 2, y, rowTextColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 2, rowTextColor);
                }
            }
            else if (noteValue == PatternEvent.NOTE_RELEASE) {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("REL", x + 2, y, rowTextColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 1, rowTextColor);
                    DrawRect(x + 3, y + 4, 13, 1, rowTextColor);
                }
            }
            else if (noteValue == PatternEvent.EMPTY) {
                WriteMonospaced("···", x + 3, y, emptyColor, 4);
            }
            else {
                string noteName = Helpers.MIDINoteToText(noteValue);
                if (noteName.Contains('#')) {
                    Write(noteName, x + 2, y, rowTextColor);
                }
                else {
                    WriteMonospaced(noteName[0] + "-", x + 2, y, rowTextColor, 5);
                    Write(noteName[2] + "", x + 13, y, rowTextColor);
                }
            }

            // draw instrument column
            if (CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument] == PatternEvent.EMPTY) {
                WriteMonospaced("··", x + 22, y, emptyColor, 4);
            }
            else {
                Color instrumentColor;
                if (instrumentValue < App.CurrentModule.Instruments.Count) {
                    if (App.CurrentModule.Instruments[instrumentValue].macroType == MacroType.Wave)
                        instrumentColor = Colors.theme.instrumentColumnWave;
                    else
                        instrumentColor = Colors.theme.instrumentColumnSample;
                }
                else {
                    instrumentColor = Color.Red;
                }
                WriteMonospaced(instrumentValue.ToString("D2"), x + 21, y, instrumentColor, 4);
            }

            // draw volumn column
            if (volumeValue == PatternEvent.EMPTY) {
                WriteMonospaced("··", x + 35, y, emptyColor, 4);
            }
            else {
                Color volumeColor;
                bool isCursorOverThisVolumeText = isCursorOnThisEvent && (cursorPosition.Column == CursorColumnType.Volume1 || cursorPosition.Column == CursorColumnType.Volume2);
                if (Preferences.profile.fadeVolumeColumn && !isCursorOverThisVolumeText) {
                    volumeColor = Helpers.Alpha(Colors.theme.volumeColumn, (int)(volumeValue / 100f * 180 + (255 - 180)));
                }
                else {
                    volumeColor = Colors.theme.volumeColumn;
                }

                WriteMonospaced(volumeValue.ToString("D2"), x + 34, y, volumeColor, 4);
            }

            for (int i = 0; i < effectColumns; ++i) {
                byte thisEffectType = CurrentPattern[cursorPosition.Row, cursorPosition.Channel, (CellType)((int)CellType.Effect1 + i * 2)];
                byte thisEffectParameter = CurrentPattern[cursorPosition.Row, cursorPosition.Channel, (CellType)((int)CellType.Effect1Parameter + i * 2)];

                if (thisEffectType == PatternEvent.EMPTY) {
                    WriteMonospaced("···", x + 48 + 18 * i, y, emptyColor, 4);
                }
                else {
                    Write((char)thisEffectType + "", x + 47 + 18 * i, y, Colors.theme.effectColumn);
                    if (IsEffectHex((char)thisEffectType))
                        WriteMonospaced(thisEffectParameter.ToString("X2"), x + 52 + 18 * i, y, Colors.theme.effectColumnParameter, 4);
                    else
                        WriteMonospaced(thisEffectParameter.ToString("D2"), x + 52 + 18 * i, y, Colors.theme.effectColumnParameter, 4);

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
            int offset = (int)position.Column switch {
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
            else if (Playback.isPlaying && Playback.position.Frame == frame && Playback.position.Row == row) {
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
            if (SelectionIsActive && !thisPos.IsAbove(currentSelection.minPosition) && !thisPos.IsBelow(currentSelection.maxPosition)) {
                int start = channelHeaders[currentSelection.minPosition.Channel].x + columnStartPosOffsets[currentSelection.minPosition.Column];
                int end = channelHeaders[currentSelection.maxPosition.Channel].x + columnStartPosOffsets[currentSelection.maxPosition.Column] + columnSpaceWidths[currentSelection.maxPosition.Column];
                bool endsOnRowSeparator = currentSelection.maxPosition.GetColumnAsSingleEffectChannel() == CursorPos.COLUMN_EFFECT_PARAMETER2;
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
                if (currentSelection.minPosition.Row == row && currentSelection.minPosition.Frame == frame)
                    DrawRect(start, linePositionY, end - start, 1, Colors.theme.selection);
                if (currentSelection.maxPosition.Row == row && currentSelection.maxPosition.Frame == frame)
                    DrawRect(start, linePositionY + 6, end - start, 1, Colors.theme.selection);
            }
        }
        #endregion


        #region move cursor methods

        /// <summary>
        /// Resets the cursor position and view to the beginning of the song
        /// </summary>
        public void OnSwitchSong() {
            cursorPosition.Initialize();
            FirstVisibleChannel = 0;
        }

        /// <summary>
        /// Moves the cursor to a row
        /// </summary>
        /// <param name="row"></param>
        public void MoveToRow(int row) {

            cursorPosition.MoveToRow(row, CurrentSong);
        }

        /// <summary>
        /// Moves the cursor to a frame
        /// </summary>
        /// <param name="frame"></param>
        public void MoveToFrame(int frame) {
            cursorPosition.MoveToFrame(frame, CurrentSong);
        }

        /// <summary>
        /// Moves the cursor one column to the left
        /// </summary>
        void MoveCursorLeft() {
            cursorPosition.MoveLeft(CurrentSong);
        }

        /// <summary>
        /// Moves the cursor one column to the right
        /// </summary>
        void MoveCursorRight() {
            cursorPosition.MoveRight(CurrentSong);
        }

        /// <summary>
        /// Moves the cursor to the first column of a channel
        /// </summary>
        /// <param name="channel"></param>
        void MoveToChannel(int channel) {
            cursorPosition.MoveToChannel(channel, CurrentSong);
        }
        #endregion

        #region selectionMethods

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
        /// Sets currentSelection.minPosition and currentSelection.maxPosition to the upper-right and bottom-left of the selection bounds from selectionStart and selectionEnd
        /// </summary>
        void CreateSelectionBounds() {
            return;
            if (lastSelectionStart == selectionStart && lastSelectionEnd == selectionEnd)
                return;
            currentSelection.minPosition = selectionStart;
            currentSelection.maxPosition = selectionEnd;
            lastSelectionStart = selectionStart;
            lastSelectionEnd = selectionEnd;
            // perform any swaps necessary
            if (selectionStart.IsBelow(selectionEnd)) {
                currentSelection.minPosition.Row = selectionEnd.Row;
                currentSelection.minPosition.Frame = selectionEnd.Frame;
                currentSelection.maxPosition.Row = selectionStart.Row;
                currentSelection.maxPosition.Frame = selectionStart.Frame;
            }
            if (selectionStart.IsRightOf(selectionEnd)) {
                currentSelection.maxPosition.Channel = selectionStart.Channel;
                currentSelection.maxPosition.Column = selectionStart.Column;
                currentSelection.minPosition.Channel = selectionEnd.Channel;
                currentSelection.minPosition.Column = selectionEnd.Column;
            }

            // if the user selected on the row column, set the selection to span all channels
            if (currentSelection.maxPosition.Channel == -1 && currentSelection.minPosition.Channel == -1) {
                currentSelection.minPosition.Channel = 0;
                currentSelection.minPosition.Column = 0;
                currentSelection.maxPosition.Channel = CurrentSong.ChannelCount - 1;
                currentSelection.maxPosition.Column = CurrentSong.GetNumCursorColumns(currentSelection.maxPosition.Channel) - 1;
            }

            // adjust the cursor positions to fill all of mulit-digit fields like instrument, volume and effects
            if (currentSelection.maxPosition.Column == 1)
                currentSelection.maxPosition.Column = 2;
            else if (currentSelection.maxPosition.Column == 3)
                currentSelection.maxPosition.Column = 4;
            else if (currentSelection.maxPosition.Column == 5 || currentSelection.maxPosition.Column == 6)
                currentSelection.maxPosition.Column = 7;
            else if (currentSelection.maxPosition.Column == 8 || currentSelection.maxPosition.Column == 9)
                currentSelection.maxPosition.Column = 10;
            else if (currentSelection.maxPosition.Column == 11 || currentSelection.maxPosition.Column == 12)
                currentSelection.maxPosition.Column = 13;
            else if (currentSelection.maxPosition.Column == 14 || currentSelection.maxPosition.Column == 15)
                currentSelection.maxPosition.Column = 16;
            if (currentSelection.minPosition.Column == 2)
                currentSelection.minPosition.Column = 1;
            else if (currentSelection.minPosition.Column == 4)
                currentSelection.minPosition.Column = 3;
            else if (currentSelection.minPosition.Column == 6 || currentSelection.minPosition.Column == 7)
                currentSelection.minPosition.Column = 5;
            else if (currentSelection.minPosition.Column == 9 || currentSelection.minPosition.Column == 10)
                currentSelection.minPosition.Column = 8;
            else if (currentSelection.minPosition.Column == 12 || currentSelection.minPosition.Column == 13)
                currentSelection.minPosition.Column = 11;
            else if (currentSelection.minPosition.Column == 15 || currentSelection.minPosition.Column == 16)
                currentSelection.minPosition.Column = 14;

            if (currentSelection.minPosition.Channel < 0)
                currentSelection.minPosition.Channel = 0;
            if (currentSelection.maxPosition.Channel < 0)
                currentSelection.maxPosition.Channel = 0;
            if (currentSelection.maxPosition.Column >= CurrentSong.GetNumCursorColumns(currentSelection.maxPosition.Channel))
                currentSelection.maxPosition.Column = CurrentSong.GetNumCursorColumns(currentSelection.maxPosition.Channel) - 1;

            //GetSelectionPositions();
        }

        /// <summary>
        /// Populates the selection array with all the CursorPositions inside the selection bounds
        /// </summary>
        void GetSelectionPositions() {
            CursorPos p = currentSelection.maxPosition;
            selection.Clear();
            //while (!p.IsRightOf(currentSelection.maxPosition)) {
            //    p.Row = currentSelection.minPosition.Row;
            //    p.Frame = currentSelection.minPosition.Frame;
            //    while (p.IsAbove(currentSelection.maxPosition)) {
            //        selection.Add(p);
            //        p.MoveToRow(p.Row + 1, CurrentSong);
            //    }
            //    selection.Add(p);
            //    p.MoveRight(CurrentSong);
            //}
            while (p.IsBelow(currentSelection.minPosition)) {
                p.Channel = currentSelection.maxPosition.Channel;
                p.Column = currentSelection.maxPosition.Column;
                while (p.IsRightOf(currentSelection.minPosition)) {
                    selection.Add(p);
                    p.MoveLeft(CurrentSong);
                }
                selection.Add(p);
                p.MoveToRow(p.Row - 1, CurrentSong);
            }
            p.Channel = currentSelection.maxPosition.Channel;
            p.Column = currentSelection.maxPosition.Column;
            while (p.IsRightOf(currentSelection.minPosition)) {
                selection.Add(p);
                p.MoveLeft(CurrentSong);
            }
            selection.Add(p);
        }

        /// <summary>
        /// Removes the selection
        /// </summary>
        void CancelSelection() {

            currentSelection = new PatternSelection(CurrentSong, cursorPosition, cursorPosition);
            SelectionIsActive = false;
            //currentSelection.minPosition = cursorPosition;
            //currentSelection.maxPosition = cursorPosition;
            //CreateSelectionBounds();
        }

        #endregion


        /// <summary>
        /// Pulls all rows below pos up one, leaving a blank row at the end.
        /// </summary>
        /// <param name="pos"></param>
        void PullRowsUp(CursorPos pos) {
            for (int i = pos.Row; i < 255; i++) {
                CurrentSong[pos.Frame][i, pos.Channel, pos.GetCellIndex()] = CurrentSong[pos.Frame][i + 1, pos.Channel, pos.GetCellIndex()];
            }
            CurrentSong[pos.Frame][255, pos.Channel, pos.GetCellIndex()] = WTPattern.EVENT_EMPTY;
        }

        /// <summary>
        /// Pushes all rows starting from pos down one. Creating a blank cell at pos
        /// </summary>
        /// <param name="pos"></param>
        void PushRowsDown(CursorPos pos) {
            for (int i = 255; i > pos.Row; i--) {
                CurrentSong[pos.Frame][i, pos.Channel, pos.GetCellIndex()] = CurrentSong[pos.Frame][i - 1, pos.Channel, pos.GetCellIndex()];
            }
            CurrentSong[pos.Frame][pos.Row, pos.Channel, pos.GetCellIndex()] = WTPattern.EVENT_EMPTY;
        }

        /// <summary>
        /// Adds the current song state to the undo stack, and clears redo history.
        /// </summary>
        void AddToUndoHistory() {
            undoHistory.Push(new WTSongState(CurrentSong, lastCursorPosition, cursorPosition));
            redoHistory.Clear();
        }

        /// <summary>
        /// Reverts the song to the state at the top of the undo stack, moving the current state to the redo stack
        /// </summary>
        public void Undo() {
            WTSongState newState = undoHistory.Pop();
            redoHistory.Push(new WTSongState(CurrentSong, lastCursorPosition, cursorPosition));
            cursorPosition = newState.previousPosition;
            //WTSong.currentSong = newState.GetSong();
        }

        /// <summary>
        /// Returns true if there are any actions that can be undone
        /// </summary>
        /// <returns></returns>
        public bool CanUndo() {
            return undoHistory.Count > 0;
        }

        /// <summary>
        /// Reverts the song to the state at the top of the undo stack 
        /// </summary>
        public void Redo() {
            WTSongState newState = redoHistory.Pop();
            undoHistory.Push(new WTSongState(CurrentSong, lastCursorPosition, cursorPosition));
            //WTSong.currentSong = newState.GetSong();
            cursorPosition = newState.currentPosition;
        }

        /// <summary>
        /// Returns true if there are any actions that can be redone.
        /// </summary>
        /// <returns></returns>
        public bool CanRedo() {
            return redoHistory.Count > 0;
        }

        /// <summary>
        /// Removes all data in the selection and copies it to the clipboard
        /// </summary>
        public void Cut() {
            CopyToClipboard();

        }

        /// <summary>
        /// Copies the current selection to the clipboard
        /// </summary>
        public void CopyToClipboard() {

        }

        /// <summary>
        /// Pastes the contents of the clipboard into the song at the current cursor position
        /// </summary>
        public void PasteFromClipboard() {

        }

        /// <summary>
        /// Pastes the contents of the clipboard into the song at the current cursor position, without overwriting existing contents.
        /// </summary>
        public void PasteAndMix() {

        }

        /// <summary>
        /// Interpolates values linearly from the start of the selection to the end.
        /// </summary>
        public void InterpolateSelection() {

        }

        /// <summary>
        /// Reverses the selection vertically, contents at the end will become the starting contents.
        /// </summary>
        public void ReverseSelection() {

        }

        #region frame methods

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
        /// Inserts a new frame at the currrent editor position, and moves the editor to that frame.<br></br>
        /// The new frame will have the next empty pattern
        /// </summary>
        public void InsertNewFrame() {
            CurrentSong.InsertNewFrame(cursorPosition.Frame);
            MoveToFrame(cursorPosition.Frame + 1);
        }

        /// <summary>
        /// Duplicates the frame at the current position
        /// </summary>
        public void DuplicateFrame() {
            CurrentSong.DuplicateFrame(cursorPosition.Frame);
            MoveToFrame(cursorPosition.Frame + 1);
        }

        public void RemoveFrame() {
            CurrentSong.RemoveFrame(cursorPosition.Frame);
            MoveToFrame(cursorPosition.Frame - 1);
        }

        public void MoveFrameRight() {
            CurrentSong.SwapFrames(cursorPosition.Frame, cursorPosition.Frame + 1);
            MoveToFrame(cursorPosition.Frame + 1);
        }
        public void MoveFrameLeft() {
            CurrentSong.SwapFrames(cursorPosition.Frame, cursorPosition.Frame - 1);
            MoveToFrame(cursorPosition.Frame - 1);
        }
        #endregion

        /// <summary>
        /// Snaps the cursor position to the playback row.
        /// </summary>
        public void SnapToPlaybackPosition() {
            cursorPosition.Frame = Playback.position.Frame;
            cursorPosition.Row = Playback.position.Row;
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
                return CurrentSong.GetNumCursorColumns(channel) - 1;

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

            // move to the first line, then forward lineNum lines.
            p.MoveToRow(p.Row - NumVisibleLines / 2 + lineNum, CurrentSong);
            return p;
        }

        /// <summary>
        /// Returns the screen bounds of a cursor position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Rectangle GetRectFromCursorPos(CursorPos position) {

            //position.Normalize(CurrentSong);

            CursorPos p = cursorPosition;

            int x = channelHeaders[position.Channel].x + GetColumnStartPositionOffset(position.Column);

            int lineNumber = NumVisibleLines / 2;
            while (p.IsBelow(position)) {
                p.MoveToRow(p.Row - 1, App.CurrentSong);
                lineNumber--;
            }
            while (p.IsAbove(position)) {
                p.MoveToRow(p.Row + 1, App.CurrentSong);
                lineNumber++;
            }
            return new Rectangle(x, lineNumber * 7, GetWidthOfCursorColumn(position.Column), 7);
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
            return 45 + 18 * App.CurrentSong.NumEffectColumns[channel] + 1;
        }

        /// <summary>
        /// Returns true if the given effect is a hexadecimal effect
        /// </summary>
        /// <param name="effectType"></param>
        /// <returns></returns>
        static bool IsEffectHex(char effectType) {
            return effectType switch {
                '0' or '4' or '7' or 'Q' or 'R' => true,
                _ => false,
            };
        }

        /// <summary>
        /// The offset start position of columns from the left of a channel track
        /// </summary>
        static int GetColumnStartPositionOffset(CursorColumnType cursorColumn) {
            return cursorColumn switch {
                CursorColumnType.Note => 0,
                CursorColumnType.Instrument1 => 19,
                CursorColumnType.Instrument2 => 25,
                CursorColumnType.Volume1 => 32,
                CursorColumnType.Volume2 => 38,
                CursorColumnType.Effect1 => 45,
                CursorColumnType.Effect1Param1 => 51,
                CursorColumnType.Effect1Param2 => 57,
                CursorColumnType.Effect2 => 63,
                CursorColumnType.Effect2Param1 => 69,
                CursorColumnType.Effect2Param2 => 75,
                CursorColumnType.Effect3 => 81,
                CursorColumnType.Effect3Param1 => 87,
                CursorColumnType.Effect3Param2 => 93,
                CursorColumnType.Effect4 => 99,
                CursorColumnType.Effect4Param1 => 105,
                CursorColumnType.Effect4Param2 => 111,
                _ => 0
            };
        }
        /// <summary>
        /// The width of the space a cursor column takes up
        /// </summary>
        static int GetWidthOfCursorColumn(CursorColumnType cursorColumn) {
            return cursorColumn switch {
                CursorColumnType.Note => 19,
                CursorColumnType.Instrument1 => 6,
                CursorColumnType.Instrument2 => 7,
                CursorColumnType.Volume1 => 6,
                CursorColumnType.Volume2 => 7,
                CursorColumnType.Effect1 => 6,
                CursorColumnType.Effect1Param1 => 6,
                CursorColumnType.Effect1Param2 => 6,
                CursorColumnType.Effect2 => 6,
                CursorColumnType.Effect2Param1 => 6,
                CursorColumnType.Effect2Param2 => 6,
                CursorColumnType.Effect3 => 6,
                CursorColumnType.Effect3Param1 => 6,
                CursorColumnType.Effect3Param2 => 6,
                CursorColumnType.Effect4 => 6,
                CursorColumnType.Effect4Param1 => 6,
                CursorColumnType.Effect4Param2 => 6,
                _ => 0
            };
        }

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
