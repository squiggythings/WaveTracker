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
        public CursorPos cursorPosition;

        CursorPos lastCursorPosition;
        PatternSelection lastSelection;

        /// <summary>
        /// Whether or not there is a selection active in the pattern editor
        /// </summary>
        public bool SelectionIsActive { get { return selection.IsActive; } }

        /// <summary>
        /// The start position of a selection the user makes
        /// </summary>
        CursorPos selectionStart;

        /// <summary>
        /// The end position of a selection the user makes
        /// </summary>
        CursorPos selectionEnd;

        /// <summary>
        /// The current selection
        /// </summary>
        PatternSelection selection;

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
        /// Returns true if there are any actions that can be undone
        /// </summary>
        /// <returns></returns>
        public bool CanUndo { get { return historyIndex > 0; } }

        /// <summary>
        /// Returns true if there are any actions that can be redone.
        /// </summary>
        /// <returns></returns>
        public bool CanRedo { get { return historyIndex < history.Count - 1; } }

        /// <summary>
        /// Returns true if there is information in the clipboard
        /// </summary>
        public bool HasClipboard { get { return clipboard != null; } }

        /// <summary>
        /// Contents of the clipboard to copy/paste
        /// </summary>
        byte[,] clipboard;
        CellType clipboardStartCellType;


        List<PatternEditorState> history;
        int historyIndex;

        /// <summary>
        /// The array of channelHeaders in rendering
        /// </summary>
        WChannelHeader[] channelHeaders;

        public HumanizeDialog humanizeDialog;

        WTFrame CurrentFrame => App.CurrentSong.FrameSequence[cursorPosition.Frame];
        WTPattern CurrentPattern => CurrentFrame.GetPattern();

        int playbackFrame;
        int playbackRow;
        CursorPos renderCursorPos;


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
            selection = new PatternSelection();
            lastSelection = new PatternSelection();
            history = new List<PatternEditorState>();
        }

        public void Update() {

            if (history.Count < 1) {
                ClearHistory();
            }
            // responsive height
            int bottomMargin = 3;
            height = App.WindowHeight - y - bottomMargin;

            // responsive width
            int rightMargin = 156;
            width = App.WindowWidth - x - rightMargin - 1;

            FirstVisibleChannel -= Input.MouseScrollWheel(KeyModifier.Alt);
            FirstVisibleChannel = Math.Clamp(FirstVisibleChannel, 0, App.CurrentModule.ChannelCount - 1);


            CalculateChannelPositioning();

            #region change octave
            if (Input.GetKeyRepeat(Keys.OemOpenBrackets, KeyModifier.None) || Input.GetKeyRepeat(Keys.Divide, KeyModifier.None))
                CurrentOctave--;
            if (Input.GetKeyRepeat(Keys.OemCloseBrackets, KeyModifier.None) || Input.GetKeyRepeat(Keys.Multiply, KeyModifier.None))
                CurrentOctave++;
            CurrentOctave = Math.Clamp(CurrentOctave, 0, 9);
            #endregion

            if (Input.focus != null || Input.focusTimer < 1)
                return;

            if (MouseY < 0 && MouseY >= -32) {
                if (MouseX < ROW_COLUMN_WIDTH || MouseX > LastChannelEndPos) {
                    if (MouseX < width) {
                        if (Input.GetClick(KeyModifier.None)) {
                            ChannelManager.UnmuteAllChannels();
                        }
                    }
                }
            }



            lastCursorPosition = cursorPosition;
            lastSelection.Set(App.CurrentSong, selection.min, selection.max);
            lastSelection.IsActive = selection.IsActive;

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
                if (MouseX > ROW_COLUMN_WIDTH)
                    cursorPosition = GetCursorPositionFromPoint(MouseX, MouseY);
            }
            // scrolling up and down the pattern
            if ((IsHovered && Input.MouseScrollWheel(KeyModifier.None) > 0) || Input.GetKeyRepeat(Keys.PageUp, KeyModifier.None)) {
                MoveToRow(cursorPosition.Row - Preferences.profile.pageJumpAmount);
            }
            if ((IsHovered && Input.MouseScrollWheel(KeyModifier.None) < 0) || Input.GetKeyRepeat(Keys.PageDown, KeyModifier.None)) {
                MoveToRow(cursorPosition.Row + Preferences.profile.pageJumpAmount);
            }


            #endregion
            #region selection with arrow keys
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveCursorLeft();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveCursorRight();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.ShiftAlt)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveToChannel(cursorPosition.Channel - 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.ShiftAlt)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveToChannel(cursorPosition.Channel + 1);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                cursorPosition.MoveToRow(cursorPosition.Row + 1, App.CurrentSong);
                if (cursorPosition.Frame != selectionStart.Frame) {
                    CancelSelection();
                }
                else {
                    SetSelectionEnd(cursorPosition);
                }
            }
            if (Input.GetKeyRepeat(Keys.Up, KeyModifier.Shift)) {

                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }

                cursorPosition.MoveToRow(cursorPosition.Row - 1, App.CurrentSong);
                if (cursorPosition.Frame != selectionStart.Frame) {
                    CancelSelection();
                }
                else {
                    SetSelectionEnd(cursorPosition);
                }
            }
            #endregion
            #region selection with mouse
            if (Input.GetClick(KeyModifier.None) && Input.MouseIsDragging && GlobalPointIsInBounds(Input.lastClickLocation)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(GetCursorPositionFromPoint(LastClickPos.X, LastClickPos.Y));
                    selection.IsActive = true;
                }
                if (GetMouseLineNumber() == 0) {
                    MoveToRow(cursorPosition.Row - 1);
                }
                if (GetMouseLineNumber() > NumVisibleLines - 2) {
                    MoveToRow(cursorPosition.Row + 1);
                }
                SetSelectionEnd(GetCursorPositionFromPointClampedToFrame(MouseX, MouseY, selectionStart.Frame, MouseY > LastClickPos.Y));
            }
            if (IsPressedM(KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                SetSelectionEnd(GetCursorPositionFromPointClampedToFrame(MouseX, MouseY, cursorPosition.Frame, GetMouseLineNumber() > NumVisibleLines / 2));
            }
            #endregion
            #region selection with CTRL-A and ESC
            if (Input.GetKeyRepeat(Keys.A, KeyModifier.Ctrl)) {
                if (selection.max.Channel == cursorPosition.Channel && selection.max.Row == CurrentPattern.GetModifiedLength() - 1 && selection.max.Frame == cursorPosition.Frame && selection.max.Column == App.CurrentSong.GetLastCursorColumnOfChannel(cursorPosition.Channel) &&
                    selection.min.Channel == cursorPosition.Channel && selection.min.Row == 0 && selection.min.Frame == cursorPosition.Frame && selection.min.Column == CursorColumnType.Note) {
                    SetSelectionStart(cursorPosition);
                    selectionStart.Channel = 0;
                    selectionStart.Row = 0;
                    selectionStart.Column = 0;
                    SetSelectionEnd(cursorPosition);
                    selectionEnd.Channel = App.CurrentModule.ChannelCount - 1;
                    selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                    selectionEnd.Column = App.CurrentSong.GetLastCursorColumnOfChannel(selectionEnd.Channel);
                }
                else {
                    selection.IsActive = true;
                    SetSelectionStart(cursorPosition);
                    selectionStart.Row = 0;
                    selectionStart.Column = 0;
                    SetSelectionEnd(cursorPosition);
                    selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                    selectionEnd.Column = App.CurrentSong.GetLastCursorColumnOfChannel(selectionEnd.Channel);
                }
            }
            if (Input.GetKeyRepeat(Keys.Escape, KeyModifier.None)) {
                CancelSelection();
            }
            #endregion


            if (!SelectionIsActive) {
                selectionStart = selectionEnd = cursorPosition;
            }
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
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
            if (cursorPosition.Column == CursorColumnType.Note) {
                // input note column
                foreach (Keys k in KeyInputs_Piano.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int note = KeyInputs_Piano[k];
                        if (note != WTPattern.EVENT_NOTE_CUT && note != WTPattern.EVENT_NOTE_RELEASE)
                            note += (CurrentOctave + 1) * 12;
                        CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Note] = (byte)note;
                        if (!InstrumentMask) {
                            CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument] = (byte)App.InstrumentBank.CurrentInstrumentIndex;
                        }
                        MoveToRow(cursorPosition.Row + InputStep);
                        AddToUndoHistory();
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Instrument1 || cursorPosition.Column == CursorColumnType.Volume1) {
                // input 10's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = App.CurrentSong[cursorPosition];
                        if (val == WTPattern.EVENT_EMPTY)
                            val = 0;
                        App.CurrentSong[cursorPosition] = (byte)(KeyInputs_Decimal[k] * 10 + val % 10);
                        MoveToRow(cursorPosition.Row + InputStep);
                        AddToUndoHistory();
                        break;
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Instrument2 || cursorPosition.Column == CursorColumnType.Volume2) {
                // input 1's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = App.CurrentSong[cursorPosition];
                        if (val == WTPattern.EVENT_EMPTY)
                            val = 0;
                        App.CurrentSong[cursorPosition] = (byte)((val / 10 * 10) + KeyInputs_Decimal[k]);
                        MoveToRow(cursorPosition.Row + InputStep);
                        AddToUndoHistory();
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
                        CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn] = (byte)KeyInputs_Effect[k];
                        MoveToRow(cursorPosition.Row + InputStep);
                        AddToUndoHistory();
                        break;
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Effect1Param1 ||
                     cursorPosition.Column == CursorColumnType.Effect2Param1 ||
                     cursorPosition.Column == CursorColumnType.Effect3Param1 ||
                     cursorPosition.Column == CursorColumnType.Effect4Param1) {
                // input 10's place effect parameters (or 16's if it is hex)
                if (Helpers.IsEffectHex((char)CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn - 1])) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = App.CurrentSong[cursorPosition];
                            App.CurrentSong[cursorPosition] = (byte)(KeyInputs_Hex[k] * 16 + val % 16);
                            MoveToRow(cursorPosition.Row + InputStep);
                            AddToUndoHistory();
                            break;
                        }
                    }
                }
                else {
                    // decimal
                    foreach (Keys k in KeyInputs_Decimal.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = App.CurrentSong[cursorPosition];
                            App.CurrentSong[cursorPosition] = (byte)(KeyInputs_Decimal[k] * 10 + val % 10);
                            MoveToRow(cursorPosition.Row + InputStep);
                            AddToUndoHistory();
                            break;
                        }
                    }
                }
            }
            else if (cursorPosition.Column == CursorColumnType.Effect1Param2 ||
                     cursorPosition.Column == CursorColumnType.Effect2Param2 ||
                     cursorPosition.Column == CursorColumnType.Effect3Param2 ||
                     cursorPosition.Column == CursorColumnType.Effect4Param2) {
                // input 1's place effect parameters
                if (Helpers.IsEffectHex((char)CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn - 1])) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = App.CurrentSong[cursorPosition];
                            App.CurrentSong[cursorPosition] = (byte)((val / 16 * 16) + KeyInputs_Hex[k]);
                            MoveToRow(cursorPosition.Row + InputStep);
                            AddToUndoHistory();
                            break;
                        }
                    }
                }
                else {
                    // decimal
                    foreach (Keys k in KeyInputs_Decimal.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = App.CurrentSong[cursorPosition];
                            App.CurrentSong[cursorPosition] = (byte)((val / 10 * 10) + KeyInputs_Decimal[k]);
                            MoveToRow(cursorPosition.Row + InputStep);
                            AddToUndoHistory();
                            break;
                        }
                    }
                }
            }
            #endregion
            #region scroll field modifiers
            if (IsHovered) {
                if (!SelectionIsActive) {
                    // scrolling field modifiers
                    if (!CurrentPattern.CellIsEmptyOrNoteCutRelease(cursorPosition.Row, cursorPosition.CellColumn)) {
                        if (cursorPosition.Column == CursorColumnType.Note) {
                            // if cursor is on the note column, use SCROLL+CTRL to transpose
                            if (Input.MouseScrollWheel(KeyModifier.Ctrl) > 0) {
                                App.CurrentSong[cursorPosition] += 1;
                                AddToUndoHistory();
                            }
                            else if (Input.MouseScrollWheel(KeyModifier.Ctrl) < 0) {
                                App.CurrentSong[cursorPosition] -= 1;
                                AddToUndoHistory();
                            }
                        }
                        else if (cursorPosition.Column == CursorColumnType.Instrument1 ||
                                 cursorPosition.Column == CursorColumnType.Instrument2 ||
                                 cursorPosition.Column == CursorColumnType.Volume1 ||
                                 cursorPosition.Column == CursorColumnType.Volume2) {
                            // if cursor is instrument or volume, use SCROLL+SHIFT to adjust
                            if (Input.MouseScrollWheel(KeyModifier.Shift) > 0) {
                                App.CurrentSong[cursorPosition] += 1;
                                AddToUndoHistory();
                            }
                            else if (Input.MouseScrollWheel(KeyModifier.Shift) < 0) {
                                App.CurrentSong[cursorPosition] -= 1;
                                AddToUndoHistory();
                            }
                        }
                        else if (cursorPosition.Column == CursorColumnType.Effect1Param1 ||
                                 cursorPosition.Column == CursorColumnType.Effect2Param1 ||
                                 cursorPosition.Column == CursorColumnType.Effect3Param1 ||
                                 cursorPosition.Column == CursorColumnType.Effect4Param1) {
                            // if cursor is an effect parameter
                            if (Helpers.IsEffectHex((char)CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn - 1])) {
                                // if effect is hex, adjust each digit independently
                                if (Input.MouseScrollWheel(KeyModifier.Shift) > 0) {
                                    App.CurrentSong[cursorPosition] += 16;
                                    AddToUndoHistory();
                                }
                                else if (Input.MouseScrollWheel(KeyModifier.Shift) < 0) {
                                    App.CurrentSong[cursorPosition] -= 16;
                                    AddToUndoHistory();
                                }

                            }
                            else {
                                // if effect is decimal, use SCROLL+SHIFT to adjust regularly
                                if (Input.MouseScrollWheel(KeyModifier.Shift) > 0) {
                                    App.CurrentSong[cursorPosition] += 1;
                                    AddToUndoHistory();
                                }
                                else if (Input.MouseScrollWheel(KeyModifier.Shift) < 0) {
                                    App.CurrentSong[cursorPosition] -= 1;
                                    AddToUndoHistory();
                                }
                            }
                        }
                        else if (cursorPosition.Column == CursorColumnType.Effect1Param2 ||
                                 cursorPosition.Column == CursorColumnType.Effect2Param2 ||
                                 cursorPosition.Column == CursorColumnType.Effect3Param2 ||
                                 cursorPosition.Column == CursorColumnType.Effect4Param2) {
                            if (Input.MouseScrollWheel(KeyModifier.Shift) > 0) {
                                App.CurrentSong[cursorPosition] += 1;
                                AddToUndoHistory();
                            }
                            else if (Input.MouseScrollWheel(KeyModifier.Shift) < 0) {
                                App.CurrentSong[cursorPosition] -= 1;
                                AddToUndoHistory();
                            }
                        }
                    }
                }
                else {
                    bool performedTask = false;
                    if (Input.MouseScrollWheel(KeyModifier.Ctrl) != 0) {
                        for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                            for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                                CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                                if (cellType == CellType.Note &&
                                    CurrentPattern[r, c] != WTPattern.EVENT_NOTE_RELEASE &&
                                    CurrentPattern[r, c] != WTPattern.EVENT_NOTE_CUT &&
                                    CurrentPattern[r, c] != WTPattern.EVENT_EMPTY) {
                                    CurrentPattern[r, c] += Input.MouseScrollWheel(KeyModifier.Ctrl);
                                    performedTask = true;
                                }
                            }
                        }
                    }
                    if (Input.MouseScrollWheel(KeyModifier.Shift) != 0) {
                        for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                            for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                                if (CurrentPattern[r, c] != WTPattern.EVENT_EMPTY) {
                                    CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                                    if (cellType != CellType.Note &&
                                        cellType != CellType.Effect1 &&
                                        cellType != CellType.Effect2 &&
                                        cellType != CellType.Effect3 &&
                                        cellType != CellType.Effect4) {
                                        CurrentPattern[r, c] += Input.MouseScrollWheel(KeyModifier.Shift);
                                        performedTask = true;
                                    }
                                }
                            }
                        }
                    }
                    if (performedTask) {
                        AddToUndoHistory();
                    }
                }
            }
            #endregion
            #region backspace + insert + delete

            if (KeyPress(Keys.Back, KeyModifier.None)) {
                Backspace();
            }
            if (KeyPress(Keys.Insert, KeyModifier.None)) {
                Insert();
            }
            if (KeyPress(Keys.Delete, KeyModifier.None)) {
                Delete();
            }

            #endregion
            #region value increment and transposing
            if (KeyPress(Keys.F1, KeyModifier.Ctrl)) {
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        if (WTPattern.GetCellTypeFromCellColumn(x) == CellType.Note) {
                            if (CurrentPattern[r, c] != WTPattern.EVENT_EMPTY &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_RELEASE &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_CUT)
                                CurrentPattern[r, c] += 1;
                        }
                    }
                }
            }
            if (KeyPress(Keys.F2, KeyModifier.Ctrl)) {
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        if (WTPattern.GetCellTypeFromCellColumn(x) == CellType.Note) {
                            if (CurrentPattern[r, c] != WTPattern.EVENT_EMPTY &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_RELEASE &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_CUT)
                                CurrentPattern[r, c] -= 1;
                        }
                    }
                }
            }
            if (KeyPress(Keys.F1, KeyModifier.Ctrl)) {
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        if (WTPattern.GetCellTypeFromCellColumn(x) == CellType.Note) {
                            if (CurrentPattern[r, c] != WTPattern.EVENT_EMPTY &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_RELEASE &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_CUT)
                                CurrentPattern[r, c] += 1;
                        }
                    }
                }
            }
            if (KeyPress(Keys.F1, KeyModifier.Shift)) {
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        if (WTPattern.GetCellTypeFromCellColumn(x) != CellType.Note) {
                            if (CurrentPattern[r, c] != WTPattern.EVENT_EMPTY &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_RELEASE &&
                                CurrentPattern[r, c] != WTPattern.EVENT_NOTE_CUT)
                                CurrentPattern[r, c] -= 1;
                        }
                    }
                }
            }
            #endregion
            #region interpolate and reverse
            if (SelectionIsActive) {
                if (KeyPress(Keys.G, KeyModifier.Ctrl)) {
                    InterpolateSelection();
                }
                if (KeyPress(Keys.R, KeyModifier.Ctrl)) {
                    ReverseSelection();
                }
            }
            #endregion
            #region copy/paste
            if (SelectionIsActive) {
                if (KeyPress(Keys.C, KeyModifier.Ctrl)) {
                    CopyToClipboard();
                }
            }
            if (clipboard != null) {
                if (KeyPress(Keys.V, KeyModifier.Ctrl)) {
                    PasteFromClipboard();
                }
                if (KeyPress(Keys.M, KeyModifier.Ctrl)) {
                    PasteAndMix();
                }
            }
            #endregion
            #region undo/redo
            if (KeyPress(Keys.Z, KeyModifier.Ctrl))
                Undo();
            if (KeyPress(Keys.Y, KeyModifier.Ctrl))
                Redo();
            #endregion
            #region alt+s replace instrument
            if (SelectionIsActive) {
                if (Input.GetKeyDown(Keys.S, KeyModifier.Alt)) {
                    for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                        for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                            if (WTPattern.GetCellTypeFromCellColumn(column) == CellType.Instrument && !CurrentPattern.CellIsEmpty(row, column)) {
                                CurrentPattern[row, column] = (byte)App.InstrumentBank.CurrentInstrumentIndex;
                            }
                        }
                    }
                }
            }
            #endregion
            #region alt+h humanize
            if (SelectionIsActive) {
                if (Input.GetKeyDown(Keys.H, KeyModifier.Alt)) {
                    humanizeDialog.Open(this);
                }
            }
            #endregion
        }

        #region Draw Methods
        public void Draw() {
            playbackFrame = Playback.position.Frame;
            playbackRow = Playback.position.Row;
            renderCursorPos = cursorPosition;
            DrawRect(0, 0, width, height, Colors.theme.background);

            DrawHeaderRect(0, -32, width);
            DrawRect(ROW_COLUMN_WIDTH - 2, -32, channelHeaders[LastVisibleChannel].x + channelHeaders[LastVisibleChannel].width - ROW_COLUMN_WIDTH + 4, 1, Colors.theme.rowSeparator);

            int frameWrap = 0;
            int frame = renderCursorPos.Frame;
            int row = renderCursorPos.Row;
            int length = App.CurrentSong[frame].GetModifiedLength();
            for (int i = NumVisibleLines / 2; i < NumVisibleLines; i++) {
                DrawRowBG(i, frame, row, frameWrap);
                row++;
                if (row >= length) {
                    frame++;
                    frame %= App.CurrentSong.FrameSequence.Count;
                    length = App.CurrentSong[frame].GetModifiedLength();
                    row = 0;
                    frameWrap++;
                }
            }
            frame = renderCursorPos.Frame;
            row = renderCursorPos.Row;
            //length = App.CurrentSong[frame].GetModifiedLength();
            frameWrap = 0;
            for (int i = NumVisibleLines / 2; i >= 0; i--) {
                DrawRowBG(i, frame, row, frameWrap);
                row--;
                if (row < 0) {
                    frame += App.CurrentSong.FrameSequence.Count - 1;
                    frame %= App.CurrentSong.FrameSequence.Count;
                    length = App.CurrentSong[frame].GetModifiedLength();
                    row = length - 1;
                    frameWrap--;
                }
            }

            // draw cursor behind text if the cursor cel is not empty
            if (!CurrentPattern.CellIsEmpty(renderCursorPos.Row, renderCursorPos.Channel, renderCursorPos.Column.ToCellType()))
                DrawCursor(ref renderCursorPos);

            frameWrap = 0;
            frame = renderCursorPos.Frame;
            row = renderCursorPos.Row;
            length = App.CurrentSong[frame].GetModifiedLength();
            for (int i = NumVisibleLines / 2; i < NumVisibleLines; i++) {
                DrawRow(i, frame, row, frameWrap);
                if (frameWrap != 0)
                    DrawRect(0, i * 7, width, 7, Helpers.Alpha(Colors.theme.background, 180));
                row++;
                if (row >= length) {
                    frame++;
                    frame %= App.CurrentSong.FrameSequence.Count;
                    length = App.CurrentSong[frame].GetModifiedLength();
                    row = 0;
                    frameWrap++;
                }
            }
            frame = renderCursorPos.Frame;
            row = renderCursorPos.Row;
            frameWrap = 0;
            for (int i = NumVisibleLines / 2; i >= 0; i--) {
                DrawRow(i, frame, row, frameWrap);
                if (frameWrap != 0)
                    DrawRect(0, i * 7, width, 7, Helpers.Alpha(Colors.theme.background, 180));
                row--;
                if (row < 0) {
                    frame += App.CurrentSong.FrameSequence.Count - 1;
                    frame %= App.CurrentSong.FrameSequence.Count;
                    length = App.CurrentSong[frame].GetModifiedLength();
                    row = length - 1;
                    frameWrap--;
                }
            }

            // draw cursor infront of text if the cursor cel is empty
            if (CurrentPattern.CellIsEmpty(renderCursorPos.Row, renderCursorPos.Channel, renderCursorPos.Column.ToCellType()))
                DrawCursor(ref renderCursorPos);

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
            //Write(selection.min.ToString(), 0, 0, Color.Red);
            //Write(selection.max.ToString(), 0, 20, Color.Red);
            DrawRect(width, -32, 1, height + 32, Colors.theme.rowSeparator);
            DrawRect(width + 1, -32, 1, 1, Colors.theme.rowSeparator);

        }
        void DrawRow(int line, int frame, int row, int frameWrap) {
            // get the row color
            Color rowTextColor = Colors.theme.patternText;
            if (row % App.CurrentSong.RowHighlightPrimary == 0)
                rowTextColor = Colors.theme.patternTextHighlighted;
            else if (row % App.CurrentSong.RowHighlightSecondary == 0)
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
            int noteValue = App.CurrentSong[frame][row, channel, CellType.Note];
            int instrumentValue = App.CurrentSong[frame][row, channel, CellType.Instrument];
            int volumeValue = App.CurrentSong[frame][row, channel, CellType.Volume];

            bool isCursorOnThisRow = frameWrap == 0 && renderCursorPos.Row == row;
            bool isCursorOnThisEvent = isCursorOnThisRow && renderCursorPos.Channel == channel;
            Color emptyColor;
            if (isCursorOnThisRow)
                emptyColor = (EditMode ? Colors.theme.rowEditText : Colors.theme.rowCurrentText);
            else
                emptyColor = Helpers.Alpha(rowTextColor, Colors.theme.patternEmptyTextAlpha);

            // draw note

            if (noteValue == WTPattern.EVENT_NOTE_CUT) {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("OFF", x + 2, y, rowTextColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 2, rowTextColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_NOTE_RELEASE) {
                if (Preferences.profile.showNoteCutAndReleaseAsText)
                    Write("REL", x + 2, y, rowTextColor);
                else {
                    DrawRect(x + 3, y + 2, 13, 1, rowTextColor);
                    DrawRect(x + 3, y + 4, 13, 1, rowTextColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_EMPTY) {
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
            if (instrumentValue == WTPattern.EVENT_EMPTY) {
                WriteMonospaced("··", x + 22, y, emptyColor, 4);
            }
            else {
                Color instrumentColor;
                if (instrumentValue < App.CurrentModule.Instruments.Count) {
                    if (App.CurrentModule.Instruments[instrumentValue].instrumentType == InstrumentType.Wave)
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
            if (volumeValue == WTPattern.EVENT_EMPTY) {
                WriteMonospaced("··", x + 35, y, emptyColor, 4);
            }
            else {
                Color volumeColor;
                bool isCursorOverThisVolumeText = isCursorOnThisEvent && (renderCursorPos.Column == CursorColumnType.Volume1 || renderCursorPos.Column == CursorColumnType.Volume2);
                if (Preferences.profile.fadeVolumeColumn && !isCursorOverThisVolumeText) {
                    volumeColor = Helpers.Alpha(Colors.theme.volumeColumn, (int)(volumeValue / 100f * 180 + (255 - 180)));
                }
                else {
                    volumeColor = Colors.theme.volumeColumn;
                }

                WriteMonospaced(volumeValue.ToString("D2"), x + 34, y, volumeColor, 4);
            }

            for (int i = 0; i < effectColumns; ++i) {
                int thisEffectType = App.CurrentSong[frame][row, channel, CellType.Effect1 + i * 2];
                int thisEffectParameter = App.CurrentSong[frame][row, channel, CellType.Effect1Parameter + i * 2];

                if (thisEffectType == WTPattern.EVENT_EMPTY) {
                    WriteMonospaced("···", x + 48 + 18 * i, y, emptyColor, 4);
                }
                else {
                    Write(Helpers.FlushString((char)thisEffectType + ""), x + 47 + 18 * i, y, Colors.theme.effectColumn);
                    if (Helpers.IsEffectHex((char)thisEffectType))
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
        void DrawCursor(ref CursorPos position) {
            Rectangle rect = GetRectFromCursorPos(ref position);
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

            if (frame == renderCursorPos.Frame && row == renderCursorPos.Row && frameWrap == 0) {
                rowBGcolor = EditMode ? Colors.theme.rowEditColor : Colors.theme.rowCurrentColor;
            }
            else if (Playback.isPlaying && playbackFrame == frame && playbackRow == row) {
                rowBGcolor = Colors.theme.rowPlaybackColor;
            }
            else if (row % App.CurrentSong.RowHighlightPrimary == 0) {
                rowBGcolor = Colors.theme.backgroundHighlighted;
            }
            else if (row % App.CurrentSong.RowHighlightSecondary == 0) {
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
            bool above = frame == selection.min.Frame ? row < selection.min.Row : frame < selection.min.Frame;
            bool below = frame == selection.min.Frame ? row > selection.max.Row : frame > selection.max.Frame;
            if (SelectionIsActive && !above && !below) {
                int start = channelHeaders[selection.min.Channel].x + GetColumnStartPositionOffset(selection.min.Column);
                int end = channelHeaders[selection.max.Channel].x + GetColumnStartPositionOffset(selection.max.Column) + GetWidthOfCursorColumn(selection.max.Column);
                bool endsOnRowSeparator = selection.max.Column == App.CurrentSong.GetLastCursorColumnOfChannel(selection.max.Channel);
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
                if (selection.min.Row == row && selection.min.Frame == frame)
                    DrawRect(start, linePositionY, end - start, 1, Colors.theme.selection);
                if (selection.max.Row == row && selection.max.Frame == frame)
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
            cursorPosition.MoveToRow(row, App.CurrentSong);
        }

        /// <summary>
        /// Moves the cursor to a frame
        /// </summary>
        /// <param name="frame"></param>
        public void MoveToFrame(int frame) {
            cursorPosition.MoveToFrame(frame, App.CurrentSong);
        }

        /// <summary>
        /// Moves the cursor one column to the left
        /// </summary>
        void MoveCursorLeft() {
            cursorPosition.MoveLeft(App.CurrentSong);
        }

        /// <summary>
        /// Moves the cursor one column to the right
        /// </summary>
        void MoveCursorRight() {
            cursorPosition.MoveRight(App.CurrentSong);
        }

        /// <summary>
        /// Moves the cursor to the first column of a channel
        /// </summary>
        /// <param name="channel"></param>
        void MoveToChannel(int channel) {
            cursorPosition.MoveToChannel(channel, App.CurrentSong);
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
        /// Removes the selection
        /// </summary>
        void CancelSelection() {
            selection = new PatternSelection(App.CurrentSong, cursorPosition, cursorPosition);
            selection.IsActive = false;
            //currentSelection.minPosition = cursorPosition;
            //currentSelection.maxPosition = cursorPosition;
            //CreateSelectionBounds();
        }

        #endregion


        /// <summary>
        /// Pulls all cells below the given position up one, leaving a blank cell at the end.
        /// </summary>
        /// <param name="pos"></param>
        void PullCellsUp(int row, int cellColumn) {
            for (int i = row; i < 255; i++) {
                CurrentPattern[i, cellColumn] = CurrentPattern[i + 1, cellColumn];
            }
            CurrentPattern[255, cellColumn] = WTPattern.EVENT_EMPTY;
        }

        /// <summary>
        /// Pushes all cell starting from pos down one. Creating a blank cell at pos
        /// </summary>
        /// <param name="pos"></param>
        void PushCellsDown(int row, int cellColumn) {
            for (int i = 255; i > row; i--) {
                CurrentPattern[i, cellColumn] = CurrentPattern[i - 1, cellColumn];
            }
            CurrentPattern[row, cellColumn] = WTPattern.EVENT_EMPTY;
        }

        public void ClearHistory() {
            history.Clear();
            history.Add(new PatternEditorState(App.CurrentSong, GetPreviousEditorPosition(), GetCurrentEditorPosition()));
            historyIndex = 0;
        }

        /// <summary>
        /// Adds the current pattern editor state to the undo history
        /// </summary>
        public void AddToUndoHistory() {
            // initialize 
            if (history.Count == 0) {
                history.Add(new PatternEditorState(App.CurrentSong, GetPreviousEditorPosition(), GetCurrentEditorPosition()));
                historyIndex = 0;
                return;
            }
            if (!EditMode)
                return;

            while (history.Count - 1 > historyIndex) {
                history.RemoveAt(history.Count - 1);
            }
            history.Add(new PatternEditorState(App.CurrentSong, GetPreviousEditorPosition(), GetCurrentEditorPosition()));
            historyIndex++;
            if (history.Count > 64) {
                history.RemoveAt(0);
                historyIndex--;
            }
        }

        /// <summary>
        /// Reverts the editor back to the last previous 
        /// </summary>
        public void Undo() {
            cursorPosition = history[historyIndex].PrePosition.CursorPosition;
            SetSelectionStart(history[historyIndex].PrePosition.SelectionStart);
            SetSelectionEnd(history[historyIndex].PrePosition.SelectionEnd);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            selection.IsActive = history[historyIndex].PrePosition.SelectionIsActive;
            historyIndex--;
            if (historyIndex < 0)
                historyIndex = 0;
            history[historyIndex].RestoreIntoSong(App.CurrentSong);
        }
        public void Redo() {
            historyIndex++;
            if (historyIndex >= history.Count)
                historyIndex = history.Count - 1;
            history[historyIndex].RestoreIntoSong(App.CurrentSong);
            cursorPosition = history[historyIndex].PostPosition.CursorPosition;
            SetSelectionStart(history[historyIndex].PostPosition.SelectionStart);
            SetSelectionEnd(history[historyIndex].PostPosition.SelectionEnd);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            selection.IsActive = history[historyIndex].PostPosition.SelectionIsActive;

        }

        public PatternEditorPosition GetCurrentEditorPosition() {
            return new PatternEditorPosition(cursorPosition, selection);
        }
        public PatternEditorPosition GetPreviousEditorPosition() {
            return new PatternEditorPosition(lastCursorPosition, lastSelection);
        }

        ///// <summary>
        ///// Adds the current song state to the undo stack, and clears redo history.
        ///// </summary>
        //void AddToUndoHistory() {
        //    undoHistory.Push(new WTSongState(App.CurrentSong, lastCursorPosition, cursorPosition));
        //    redoHistory.Clear();
        //}

        ///// <summary>
        ///// Reverts the song to the state at the top of the undo stack, moving the current state to the redo stack
        ///// </summary>
        //public void Undo() {
        //    WTSongState newState = undoHistory.Pop();
        //    redoHistory.Push(new WTSongState(App.CurrentSong, lastCursorPosition, cursorPosition));
        //    cursorPosition = newState.previousPosition;
        //    newState.RestoreIntoSong(App.CurrentSong);
        //}

        ///// <summary>
        ///// Reverts the song to the state at the top of the undo stack 
        ///// </summary>
        //public void Redo() {
        //    WTSongState newState = redoHistory.Pop();
        //    undoHistory.Push(new WTSongState(App.CurrentSong, lastCursorPosition, cursorPosition));
        //    newState.RestoreIntoSong(App.CurrentSong);
        //    cursorPosition = newState.currentPosition;
        //}

        /// <summary>
        /// Removes all data in the selection and copies it to the clipboard
        /// </summary>
        public void Cut() {
            CopyToClipboard();
            Delete();
        }

        /// <summary>
        /// Removes all data in the selection
        /// </summary>
        public void Delete() {
            if (SelectionIsActive) {
                for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                    for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                        CurrentPattern[row, column] = WTPattern.EVENT_EMPTY;
                    }
                }
            }
            else {
                for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                    CurrentPattern[cursorPosition.Row, column] = WTPattern.EVENT_EMPTY;
                }
                MoveToRow(cursorPosition.Row + 1);
            }
            AddToUndoHistory();
        }

        /// <summary>
        /// Inserts empty cells at the cursor position or beginning of selection
        /// </summary>
        public void Insert() {
            if (SelectionIsActive) {
                for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                    PushCellsDown(selection.min.Row, c);
                }
            }
            else {
                for (int c = 0; c < App.CurrentSong.GetNumCursorColumns(cursorPosition.Channel); ++c) {
                    PushCellsDown(cursorPosition.Row, c + cursorPosition.Channel * 11);
                }
            }
            AddToUndoHistory();
        }

        /// <summary>
        /// Deletes all cells in the selection and pulls up the ones that come after
        /// </summary>
        public void Backspace() {
            if (SelectionIsActive) {
                for (int r = selection.max.Row; r >= selection.min.Row; --r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        PullCellsUp(r, c);
                    }
                }
                CancelSelection();
            }
            else if (cursorPosition.Row > 0) {
                MoveToRow(cursorPosition.Row - 1);
                for (int c = 0; c < App.CurrentSong.GetNumCursorColumns(cursorPosition.Channel); ++c) {
                    PullCellsUp(cursorPosition.Row, c + cursorPosition.Channel * 11);
                }
            }
            AddToUndoHistory();
        }

        /// <summary>
        /// Copies the current selection to the clipboard
        /// </summary>
        public void CopyToClipboard() {
            clipboard = new byte[selection.Height, selection.Width];
            clipboardStartCellType = selection.min.Column.ToCellType();
            for (int row = 0; row < selection.Height; row++) {
                for (int column = 0; column < selection.Width; column++) {
                    clipboard[row, column] = (byte)CurrentPattern[selection.min.Row + row, selection.min.CellColumn + column];
                }
            }
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the song at the current cursor position
        /// </summary>
        public void PasteFromClipboard() {
            CursorPos p = cursorPosition;
            p.Column = clipboardStartCellType.ToNearestCursorColumn();
            int columnStart = p.CellColumn;
            int patternHeight = 256;
            int patternWidth = CurrentPattern.Width;
            for (int row = 0; row < clipboard.GetLength(0); row++) {
                for (int column = 0; column < clipboard.GetLength(1); column++) {
                    if (columnStart + column >= patternWidth)
                        break;
                    CurrentPattern[p.Row + row, columnStart + column] = clipboard[row, column];
                }
                if (cursorPosition.Row + row >= patternHeight)
                    break;
            }
            selection.IsActive = true;
            SetSelectionStart(p);
            p.MoveToRow(Math.Clamp(p.Row + clipboard.GetLength(0) - 1, 0, CurrentPattern.GetModifiedLength() - 1), App.CurrentSong);
            p.Channel += clipboard.GetLength(1) / 11;
            p.Column += clipboard.GetLength(1) % 11;
            SetSelectionEnd(p);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            AddToUndoHistory();
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the song at the current cursor position, without overwriting existing contents.
        /// </summary>
        public void PasteAndMix() {
            CursorPos p = cursorPosition;
            p.Column = clipboardStartCellType.ToNearestCursorColumn();
            int columnStart = p.CellColumn;
            int patternHeight = 256;
            int patternWidth = CurrentPattern.Width;
            for (int row = 0; row < selection.Height; row++) {
                for (int column = 0; column < selection.Width; column++) {
                    if (columnStart + column >= patternWidth)
                        break;
                    if (CurrentPattern.CellIsEmpty(cursorPosition.Row + row, columnStart + column))
                        CurrentPattern[cursorPosition.Row + row, columnStart + column] = clipboard[row, column];
                }
                if (cursorPosition.Row + row >= patternHeight)
                    break;
            }
            selection.IsActive = true;
            SetSelectionStart(p);
            p.MoveToRow(Math.Clamp(p.Row + clipboard.GetLength(0) - 1, 0, CurrentPattern.GetModifiedLength() - 1), App.CurrentSong);
            p.Channel += clipboard.GetLength(1) / 11;
            p.Column += clipboard.GetLength(1) % 11;
            SetSelectionEnd(p);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            AddToUndoHistory();
        }

        /// <summary>
        /// Interpolates values linearly from the start of the selection to the end.
        /// </summary>
        public void InterpolateSelection() {
            CursorPos p = selection.min;
            int col = selection.min.CellColumn;
            if (p.Column == CursorColumnType.Effect1 || p.Column == CursorColumnType.Effect2 || p.Column == CursorColumnType.Effect3 || p.Column == CursorColumnType.Effect4) {
                col++;
            }
            WTPattern pattern = App.CurrentSong[selection.min.Frame];
            int val1 = pattern[selection.min.Row, col];
            int val2 = pattern[selection.max.Row, col];
            if (pattern.CellIsEmptyOrNoteCutRelease(selection.min.Row, col))
                return;
            if (pattern.CellIsEmptyOrNoteCutRelease(selection.max.Row, col))
                return;
            int startRow = selection.min.Row;
            int endRow = selection.max.Row;
            for (int r = startRow; r < endRow; r++) {
                if (p.Column == CursorColumnType.Effect1 || p.Column == CursorColumnType.Effect2 || p.Column == CursorColumnType.Effect3 || p.Column == CursorColumnType.Effect4) {
                    pattern[r, col - 1] = pattern[startRow, col - 1];
                }
                pattern[r, col] = (byte)Math.Round(MathHelper.Lerp(val1, val2, (r - startRow) / (float)selection.Height));
            }
            AddToUndoHistory();
        }

        /// <summary>
        /// Reverses the selection vertically, contents at the end will become the starting contents.
        /// </summary>
        public void ReverseSelection() {
            int col = selection.min.CellColumn;
            int startRow = selection.min.Row;
            int endRow = selection.max.Row;
            for (int r = 0; r <= selection.Height / 2; ++r) {
                int val1 = CurrentPattern[startRow + r, col];
                int val2 = CurrentPattern[endRow - r, col];
                CurrentPattern[startRow + r, col] = val2;
                CurrentPattern[endRow - r, col] = val1;
            }
            AddToUndoHistory();
        }

        public void RandomizeSelectedVolumes(int randomOffset) {
            if (SelectionIsActive) {
                Random r = new Random();
                for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                    for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                        if (WTPattern.GetCellTypeFromCellColumn(column) == CellType.Volume && !CurrentPattern.CellIsEmpty(row, column)) {
                            CurrentPattern[row, column] = (byte)Math.Clamp(CurrentPattern[row, column] + r.Next(-randomOffset, randomOffset), 0, 99);
                        }
                    }
                }
            }
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
            MoveToFrame(cursorPosition.Frame + 1);
            App.CurrentSong.InsertNewFrame(cursorPosition.Frame);
        }

        /// <summary>
        /// Duplicates the frame at the current position
        /// </summary>
        public void DuplicateFrame() {
            App.CurrentSong.DuplicateFrame(cursorPosition.Frame);
            MoveToFrame(cursorPosition.Frame + 1);
        }

        public void RemoveFrame() {
            App.CurrentSong.RemoveFrame(cursorPosition.Frame);
            MoveToFrame(cursorPosition.Frame - 1);
        }

        public void MoveFrameRight() {
            App.CurrentSong.SwapFrames(cursorPosition.Frame, cursorPosition.Frame + 1);
            MoveToFrame(cursorPosition.Frame + 1);
        }
        public void MoveFrameLeft() {
            App.CurrentSong.SwapFrames(cursorPosition.Frame, cursorPosition.Frame - 1);
            MoveToFrame(cursorPosition.Frame - 1);
        }
        public void IncreaseFramePatternIndex() {
            CurrentFrame.PatternIndex++;
            AddToUndoHistory();
        }
        public void DecreaseFramePatternIndex() {
            CurrentFrame.PatternIndex--;
            AddToUndoHistory();
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
            cursorPosition.Row = CurrentPattern.GetModifiedLength() - 1;
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
                channelHeaders[channel].NumEffectColumns = App.CurrentSong.NumEffectColumns[channel];
                channelHeaders[channel].Update();

                // if the user changed the number of effect columns
                if (channelHeaders[channel].NumEffectColumns != App.CurrentSong.NumEffectColumns[channel]) {
                    App.CurrentSong.NumEffectColumns[channel] = channelHeaders[channel].NumEffectColumns;
                    channelHeaders[channel].width = GetWidthOfChannel(channel) - 1;
                    cursorPosition.Normalize(App.CurrentSong);
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
                if (c >= App.CurrentModule.ChannelCount)
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
        CursorColumnType GetColumnAtPoint(int x) {

            int channel = GetChannelAtPoint(x);
            if (channel < 0)
                return 0;
            if (channel > LastVisibleChannel)
                return App.CurrentSong.GetLastCursorColumnOfChannel(channel);

            x -= GetStartPositionOfChannel(channel);

            CursorColumnType col = 0;
            x -= GetWidthOfCursorColumn(col);
            while (x > 0) {
                col++;
                if (col > CursorColumnType.Effect4Param2)
                    return col - 1;
                x -= GetWidthOfCursorColumn(col);
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
            p.MoveToRow(p.Row - NumVisibleLines / 2 + lineNum, App.CurrentSong);
            return p;
        }
        /// <summary>
        /// Gets the cursor position under a but clamps it to a frame
        /// </summary>
        /// <returns></returns>
        CursorPos GetCursorPositionFromPointClampedToFrame(int x, int y, int frame, bool isAboveClampedFrame) {
            CursorPos p = new CursorPos {
                Frame = cursorPosition.Frame,
                Row = cursorPosition.Row,
                Channel = GetChannelAtPoint(x),
                Column = GetColumnAtPoint(x)
            };

            int lineNum = y / ROW_HEIGHT;

            // move to the first line, then forward lineNum lines.
            p.MoveToRow(p.Row - NumVisibleLines / 2 + lineNum, App.CurrentSong);
            if (p.Frame != frame) {
                p.Frame = frame;
                if (isAboveClampedFrame) {
                    p.Row = App.CurrentSong[frame].GetModifiedLength() - 1;
                }
                else {
                    p.Row = 0;
                }
            }
            return p;
        }

        /// <summary>
        /// Returns the screen bounds of a cursor position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Rectangle GetRectFromCursorPos(ref CursorPos position) {

            //position.Normalize(CurrentSong);
            int x = channelHeaders[position.Channel].x + GetColumnStartPositionOffset(position.Column);

            int lineNumber = NumVisibleLines / 2;
            //while (p.IsBelow(position)) {
            //    p.MoveToRow(p.Row - 1, App.CurrentSong);
            //    lineNumber--;
            //}
            //while (p.IsAbove(position)) {
            //    p.MoveToRow(p.Row + 1, App.CurrentSong);
            //    lineNumber++;
            //}
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
            { Keys.D1, WTPattern.EVENT_NOTE_CUT },
            { Keys.OemPlus, WTPattern.EVENT_NOTE_RELEASE },

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
