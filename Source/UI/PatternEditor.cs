using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {

    /// <summary>
    /// The pattern editor interface, including the channel headers
    /// </summary>
    public class PatternEditor : Clickable {
        private const int ROW_HEIGHT = 7;
        private const int ROW_COLUMN_WIDTH = 22;

        public bool EditMode { get; set; }
        public int CurrentOctave { get; set; }
        public bool InstrumentMask { get; set; }

        public int InputStep { get; set; }

        public bool FollowMode { get; set; }

        /// <summary>
        /// The position of the main cursor in the song
        /// </summary>
        public CursorPos cursorPosition;
        private CursorPos lastCursorPosition;
        private PatternSelection lastSelection;

        /// <summary>
        /// Whether or not there is a selection active in the pattern editor
        /// </summary>
        public bool SelectionIsActive { get { return selection.IsActive; } }

        /// <summary>
        /// The start position of a selection the user makes
        /// </summary>
        private CursorPos selectionStart;

        /// <summary>
        /// The end position of a selection the user makes
        /// </summary>
        private CursorPos selectionEnd;

        /// <summary>
        /// The current selection
        /// </summary>
        private PatternSelection selection;

        /// <summary>
        /// The index of the leftmost channel to render
        /// </summary>
        public int FirstVisibleChannel { get; private set; }

        /// <summary>
        /// The index of the rightmost channel to render
        /// </summary>
        public int LastVisibleChannel { get; private set; }

        /// <summary>
        /// The maximum channel scroll amount
        /// </summary>
        public int MaxChannelScroll { get; private set; }

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
        private byte[,] clipboard;
        private CellType clipboardStartCellType;

        /// <summary>
        /// A copy of the volume values before scaling
        /// </summary>
        private float[,] scaleClipboard;
        private List<PatternEditorState> history;
        private int historyIndex;

        /// <summary>
        /// The array of channelHeaders in rendering
        /// </summary>
        public ChannelHeader[] ChannelHeaders { get; }

        private WTFrame CurrentFrame {
            get {
                return App.CurrentSong.FrameSequence[cursorPosition.Frame];
            }
        }

        private WTPattern CurrentPattern {
            get {
                return CurrentFrame.GetPattern();
            }
        }
        private WTPattern SelectionPattern {
            get {
                return App.CurrentSong[selection.min.Frame];
            }
        }

        private ScrollbarHorizontal channelScrollbar;
        private int playbackFrame;
        private int playbackRow;
        private CursorPos renderCursorPos;

        public PatternEditor(int x, int y) {
            this.x = x;
            this.y = y;
            ChannelHeaders = new ChannelHeader[WTModule.MAX_CHANNEL_COUNT];
            for (int i = 0; i < ChannelHeaders.Length; ++i) {
                ChannelHeaders[i] = new ChannelHeader(0, -32, 63, i, this);
            }

            InputStep = 1;
            CurrentOctave = 4;
            FollowMode = true;
            selection = new PatternSelection();
            lastSelection = new PatternSelection();
            history = [];
            channelScrollbar = new ScrollbarHorizontal(ROW_COLUMN_WIDTH, 0, width, 6, this);
        }

        public Menu CreateEditMenu() {
            return new Menu([
                        new MenuOption("Undo", Undo, CanUndo && !App.VisualizerMode),
                        new MenuOption("Redo", Redo, CanRedo && !App.VisualizerMode),
                        null,
                        new MenuOption("Cut", Cut, SelectionIsActive && !App.VisualizerMode),
                        new MenuOption("Copy", CopyToClipboard, SelectionIsActive && !App.VisualizerMode),
                        new MenuOption("Paste", PasteFromClipboard, HasClipboard && !App.VisualizerMode),
                        new MenuOption("Delete", Delete, SelectionIsActive && !App.VisualizerMode),
                        new MenuOption("Select All", SelectAll, !App.VisualizerMode),
                        null,
                        new SubMenu("Pattern", [
                            new MenuOption("Interpolate", InterpolateSelection, SelectionIsActive && !App.VisualizerMode),
                            new MenuOption("Reverse", ReverseSelection, SelectionIsActive && !App.VisualizerMode),
                            new MenuOption("Replace Instrument", ReplaceInstrument, SelectionIsActive && !App.VisualizerMode),
                            new MenuOption("Humanize Volumes", Humanize, SelectionIsActive && !App.VisualizerMode),
                            null,
                            new MenuOption("Expand", ExpandSelection, SelectionIsActive && !App.VisualizerMode),
                            new MenuOption("Shrink", ShrinkSelection, SelectionIsActive && !App.VisualizerMode),
                            new MenuOption("Stretch...", OpenStretchDialog, SelectionIsActive && !App.VisualizerMode),
                            null,
                            new SubMenu("Transpose",[
                                new MenuOption("Increase note", IncreaseNote, !App.VisualizerMode),
                                new MenuOption("Decrease note", DecreaseNote, !App.VisualizerMode),
                                new MenuOption("Increase octave", IncreaseOctave, !App.VisualizerMode),
                                new MenuOption("Decrease octave", DecreaseOctave, !App.VisualizerMode),
                            ])

                        ]),
                        null,
                        new MenuOption("Preferences...", Dialogs.configurationDialog.Open),
                    ]);
        }
        public void Update() {

            if (history.Count < 1) {
                ClearHistory();
            }
            // responsive height
            int bottomMargin = 8;
            if (channelScrollbar.IsVisible) {
                height = App.WindowHeight - y - bottomMargin - channelScrollbar.height;
            }
            else {
                height = App.WindowHeight - y - bottomMargin;
            }

            // responsive width
            int rightMargin = 156;
            width = App.WindowWidth - x - rightMargin - 1;
            #region change octave
            if (App.Shortcuts["General\\Decrease octave"].IsPressedRepeat) {
                CurrentOctave--;
                PianoInput.ClearAllNotes();
            }
            if (App.Shortcuts["General\\Increase octave"].IsPressedRepeat) {
                CurrentOctave++;
                PianoInput.ClearAllNotes();
            }
            for (int i = 0; i < 9; ++i) {
                if (App.Shortcuts["General\\Set octave " + i].IsPressedRepeat) {
                    CurrentOctave = i;
                    PianoInput.ClearAllNotes();
                }
            }
            CurrentOctave = Math.Clamp(CurrentOctave, 0, 9);
            #endregion

            FirstVisibleChannel -= Input.MouseScrollWheel(KeyModifier.Alt);
            FirstVisibleChannel = Math.Clamp(FirstVisibleChannel, 0, App.CurrentModule.ChannelCount - 1);

            CalculateChannelPositioning(false);
            channelScrollbar.width = width - ROW_COLUMN_WIDTH + 1;
            channelScrollbar.y = height;
            channelScrollbar.SetSize(App.CurrentModule.ChannelCount, App.CurrentModule.ChannelCount - MaxChannelScroll);
            channelScrollbar.ScrollValue = FirstVisibleChannel;
            channelScrollbar.Update();
            FirstVisibleChannel = channelScrollbar.ScrollValue;
            if (FirstVisibleChannel > MaxChannelScroll) {
                FirstVisibleChannel = MaxChannelScroll;
            }
            CalculateChannelPositioning(true);

            if (Input.focus != null || Input.focusTimer < 1 || App.VisualizerMode) {
                return;
            }

            if (RightClicked) {
                ContextMenu.Open(CreateEditMenu());
            }
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
            if (App.Shortcuts["General\\Toggle channel"].IsPressedDown) {
                ChannelManager.ToggleChannel(cursorPosition.Channel);
            }

            if (App.Shortcuts["General\\Solo channel"].IsPressedDown) {
                if (ChannelManager.IsChannelSoloed(cursorPosition.Channel)) {
                    ChannelManager.UnmuteAllChannels();
                }
                else {
                    ChannelManager.SoloChannel(cursorPosition.Channel);
                }
            }
            #endregion

            //////////////////////////////
            //        NAVIGATION        //
            //////////////////////////////

            #region home/end navigation
            // On home key press
            if (Input.GetKeyRepeat(App.Shortcuts["Pattern\\Jump to top of frame"].Key, KeyModifier.None)) {
                GoToTopOfFrame();
            }
            // On end key press
            if (Input.GetKeyRepeat(App.Shortcuts["Pattern\\Jump to bottom of frame"].Key, KeyModifier.None)) {
                GoToBottomOfFrame();
            }
            #endregion
            #region moving cursor with arrow keys
            // On arrow keys
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.None)) {
                CancelSelection();
                MoveCursorLeft();
                MakeCursorVisibleInBounds();
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.None)) {
                CancelSelection();
                MoveCursorRight();
                MakeCursorVisibleInBounds();
            }
            if (App.Shortcuts["Frame\\Previous frame"].IsPressedRepeat) {
                CancelSelection();
                PreviousFrame();
            }
            if (App.Shortcuts["Frame\\Next frame"].IsPressedRepeat) {
                CancelSelection();
                NextFrame();
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Alt)) {
                CancelSelection();
                MoveToChannel(cursorPosition.Channel - 1);
                MakeCursorVisibleInBounds();
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Alt)) {
                CancelSelection();
                MoveToChannel(cursorPosition.Channel + 1);
                MakeCursorVisibleInBounds();
            }
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None)) {
                CancelSelection();
                MoveToRow(cursorPosition.Row + (App.Settings.PatternEditor.IgnoreStepWhenMoving ? 1 : InputStep));
            }
            if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None)) {
                CancelSelection();
                MoveToRow(cursorPosition.Row - (App.Settings.PatternEditor.IgnoreStepWhenMoving ? 1 : InputStep));
            }
            #endregion
            #region navigate with mouse
            if (ClickedDown) {
                CancelSelection();
            }
            if (ClickedM(KeyModifier._Any) && Input.CurrentModifier != KeyModifier.Shift && MouseX <= LastChannelEndPos && !Input.MouseJustEndedDragging) {
                if (MouseX > ROW_COLUMN_WIDTH && MouseX < width) {
                    cursorPosition = GetCursorPositionFromPoint(MouseX, MouseY);
                    MakeCursorVisibleInBounds();
                }
            }
            // scrolling up and down the pattern
            if (IsHovered && Input.MouseScrollWheel(KeyModifier.None) > 0 || Input.GetKeyRepeat(Keys.PageUp, KeyModifier.None)) {
                MoveToRow(cursorPosition.Row - App.Settings.PatternEditor.PageJumpAmount);
            }
            if (IsHovered && Input.MouseScrollWheel(KeyModifier.None) < 0 || Input.GetKeyRepeat(Keys.PageDown, KeyModifier.None)) {
                MoveToRow(cursorPosition.Row + App.Settings.PatternEditor.PageJumpAmount);
            }

            #endregion
            #region selection with arrow keys
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveCursorLeft();
                MakeCursorVisibleInBounds();
                SetSelectionEnd(cursorPosition);

            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveCursorRight();
                MakeCursorVisibleInBounds();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.ShiftAlt)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveToChannel(cursorPosition.Channel - 1);
                MakeCursorVisibleInBounds();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.ShiftAlt)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveToChannel(cursorPosition.Channel + 1);
                MakeCursorVisibleInBounds();
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                cursorPosition.MoveToRow(cursorPosition.Row + (App.Settings.PatternEditor.IgnoreStepWhenMoving ? 1 : InputStep), App.CurrentSong);
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

                cursorPosition.MoveToRow(cursorPosition.Row - (App.Settings.PatternEditor.IgnoreStepWhenMoving ? 1 : InputStep), App.CurrentSong);
                if (cursorPosition.Frame != selectionStart.Frame) {
                    CancelSelection();
                }
                else {
                    SetSelectionEnd(cursorPosition);
                }
            }
            if (Input.GetKeyRepeat(Keys.PageUp, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveToRow(cursorPosition.Row - App.Settings.PatternEditor.PageJumpAmount);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(Keys.PageDown, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                MoveToRow(cursorPosition.Row + App.Settings.PatternEditor.PageJumpAmount);
                SetSelectionEnd(cursorPosition);
            }
            if (Input.GetKeyRepeat(App.Shortcuts["Pattern\\Jump to top of frame"].Key, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                GoToTopOfFrame();
                SetSelectionEnd(cursorPosition);
            }
            // On end key press
            if (Input.GetKeyRepeat(App.Shortcuts["Pattern\\Jump to bottom of frame"].Key, KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                GoToBottomOfFrame();
                SetSelectionEnd(cursorPosition);
            }
            #endregion
            #region selection with mouse
            if (Input.GetClick(KeyModifier.None) && Input.MouseIsDragging && GlobalPointIsInBounds(Input.LastClickLocation) && MouseX < App.WindowWidth - 12 && MouseY < App.WindowHeight - 12) {
                if (!SelectionIsActive) {
                    SetSelectionStart(GetCursorPositionFromPoint(LastClickPos.X, LastClickPos.Y));
                    selection.IsActive = true;
                }
                if (GetMouseLineNumber() == 0) {
                    MoveToRow(cursorPosition.Row - 1);
                }
                if (GetMouseLineNumber() >= NumVisibleLines - 2) {
                    MoveToRow(cursorPosition.Row + 1);
                }
                SetSelectionEnd(GetCursorPositionFromPointClampedToFrame(MouseX, MouseY, selectionStart.Frame, MouseY > LastClickPos.Y));
            }
            if (IsPressedM(KeyModifier.Shift)) {
                if (!SelectionIsActive) {
                    SetSelectionStart(cursorPosition);
                    selection.IsActive = true;
                }
                if (GetMouseLineNumber() == 0) {
                    MoveToRow(cursorPosition.Row - 1);
                }
                if (GetMouseLineNumber() >= NumVisibleLines - 2) {
                    MoveToRow(cursorPosition.Row + 1);
                }
                SetSelectionEnd(GetCursorPositionFromPointClampedToFrame(MouseX, MouseY, cursorPosition.Frame, GetMouseLineNumber() > NumVisibleLines / 2));
            }
            if (DoubleClicked && GetMouseLineNumber() == NumVisibleLines / 2) {
                selection.IsActive = true;
                SetSelectionStart(cursorPosition);
                selectionStart.Row = 0;
                selectionStart.Column = 0;
                SetSelectionEnd(cursorPosition);
                selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                selectionEnd.Column = App.CurrentSong.GetLastCursorColumnOfChannel(selectionEnd.Channel);
            }
            #endregion
            #region selection with CTRL-A and ESC
            if (App.Shortcuts["Edit\\Select all"].IsPressedRepeat) {
                SelectAll();
            }
            if (App.Shortcuts["Edit\\Deselect"].IsPressedRepeat) {
                CancelSelection();
            }
            #endregion

            if (!SelectionIsActive) {
                selectionStart = selectionEnd = cursorPosition;
            }
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);

            #region copying selection
            if (SelectionIsActive) {
                if (App.Shortcuts["Edit\\Copy"].IsPressedDown) {
                    CopyToClipboard();
                }
            }
            #endregion

            if (App.Shortcuts["General\\Toggle edit mode"].IsPressedRepeat) {
                ToggleEditMode();
            }
            if (!EditMode) {
                return;
            }
            /////////////////////////////////
            //           EDITING           //
            /////////////////////////////////

            #region field input
            if (cursorPosition.Column == CursorColumnType.Note) {
                // input note column
                foreach (string pianoKeyShortcut in KeyInputs_Piano.Keys) {
                    if (KeyPress(App.Shortcuts[pianoKeyShortcut])) {
                        int note = KeyInputs_Piano[pianoKeyShortcut];
                        if (note is WTPattern.EVENT_NOTE_CUT or WTPattern.EVENT_NOTE_RELEASE) {
                            CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Note] = (byte)note;
                            if (!InstrumentMask) {
                                CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument] = (byte)App.InstrumentBank.CurrentInstrumentIndex;
                            }
                            MoveToRow(cursorPosition.Row + InputStep);
                            AddToUndoHistory();
                        }

                    }
                }
            }
            else if (cursorPosition.Column is CursorColumnType.Instrument1 or CursorColumnType.Volume1) {
                // input 10's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = App.CurrentSong[cursorPosition];
                        if (val == WTPattern.EVENT_EMPTY) {
                            val = 0;
                        }

                        App.CurrentSong[cursorPosition] = (byte)(KeyInputs_Decimal[k] * 10 + val % 10);
                        if (cursorPosition.Column == CursorColumnType.Instrument1) {
                            if (App.CurrentSong[cursorPosition] < App.CurrentModule.Instruments.Count) {
                                App.InstrumentBank.CurrentInstrumentIndex = App.CurrentSong[cursorPosition];
                            }
                        }
                        switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                            case SettingsProfile.MoveToNextRowBehavior.Always:
                                MoveToRow(cursorPosition.Row + InputStep);
                                break;
                            case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                            case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                MoveCursorRight();
                                break;
                            case SettingsProfile.MoveToNextRowBehavior.Never:
                                App.CurrentSong[cursorPosition] = (byte)(val % 10 * 10 + KeyInputs_Decimal[k]);
                                break;
                        }
                        AddToUndoHistory();
                        break;
                    }
                }
            }
            else if (cursorPosition.Column is CursorColumnType.Instrument2 or CursorColumnType.Volume2) {
                // input 1's place decimals (inst + vol)
                foreach (Keys k in KeyInputs_Decimal.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        int val = App.CurrentSong[cursorPosition];
                        if (val == WTPattern.EVENT_EMPTY) {
                            val = 0;
                        }

                        App.CurrentSong[cursorPosition] = (byte)(val / 10 * 10 + KeyInputs_Decimal[k]);
                        if (cursorPosition.Column == CursorColumnType.Instrument2) {
                            if (App.CurrentSong[cursorPosition] < App.CurrentModule.Instruments.Count) {
                                App.InstrumentBank.CurrentInstrumentIndex = App.CurrentSong[cursorPosition];
                            }
                        }
                        switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                            case SettingsProfile.MoveToNextRowBehavior.Always:
                            case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                            case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                MoveToRow(cursorPosition.Row + InputStep);
                                break;
                            case SettingsProfile.MoveToNextRowBehavior.Never:
                                App.CurrentSong[cursorPosition] = (byte)(val % 10 * 10 + KeyInputs_Decimal[k]);
                                break;
                        }
                        AddToUndoHistory();
                        break;
                    }
                }
            }
            else if (cursorPosition.Column is CursorColumnType.Effect1 or
                     CursorColumnType.Effect2 or
                     CursorColumnType.Effect3 or
                     CursorColumnType.Effect4) {
                // input effects
                foreach (Keys k in KeyInputs_Effect.Keys) {
                    if (KeyPress(k, KeyModifier.None)) {
                        CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn] = (byte)KeyInputs_Effect[k];
                        switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                            case SettingsProfile.MoveToNextRowBehavior.Always:
                            case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                                MoveToRow(cursorPosition.Row + InputStep);
                                break;
                            case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                MoveCursorRight();
                                break;
                        }
                        AddToUndoHistory();
                        break;
                    }
                }
            }
            else if (cursorPosition.Column is CursorColumnType.Effect1Param1 or
                     CursorColumnType.Effect2Param1 or
                     CursorColumnType.Effect3Param1 or
                     CursorColumnType.Effect4Param1) {
                // input 10's place effect parameters (or 16's if it is hex)
                if (Helpers.IsEffectHex((char)CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn - 1])) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = App.CurrentSong[cursorPosition];
                            App.CurrentSong[cursorPosition] = (byte)(KeyInputs_Hex[k] * 16 + val % 16);
                            switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                                case SettingsProfile.MoveToNextRowBehavior.Always:
                                    MoveToRow(cursorPosition.Row + InputStep);
                                    break;
                                case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                                case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                    MoveCursorRight();
                                    break;
                                case SettingsProfile.MoveToNextRowBehavior.Never:
                                    App.CurrentSong[cursorPosition] = (byte)(val % 16 * 16 + KeyInputs_Hex[k]);
                                    break;
                            }
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
                            switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                                case SettingsProfile.MoveToNextRowBehavior.Always:
                                    MoveToRow(cursorPosition.Row + InputStep);
                                    break;
                                case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                                case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                    MoveCursorRight();
                                    break;
                                case SettingsProfile.MoveToNextRowBehavior.Never:
                                    App.CurrentSong[cursorPosition] = (byte)(val % 10 * 10 + KeyInputs_Decimal[k]);
                                    break;
                            }
                            AddToUndoHistory();
                            break;
                        }
                    }
                }
            }
            else if (cursorPosition.Column is CursorColumnType.Effect1Param2 or
                     CursorColumnType.Effect2Param2 or
                     CursorColumnType.Effect3Param2 or
                     CursorColumnType.Effect4Param2) {
                // input 1's place effect parameters
                if (Helpers.IsEffectHex((char)CurrentPattern[cursorPosition.Row, cursorPosition.CellColumn - 1])) {
                    // hex
                    foreach (Keys k in KeyInputs_Hex.Keys) {
                        if (KeyPress(k, KeyModifier.None)) {
                            int val = App.CurrentSong[cursorPosition];
                            App.CurrentSong[cursorPosition] = (byte)(val / 16 * 16 + KeyInputs_Hex[k]);
                            switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                                case SettingsProfile.MoveToNextRowBehavior.Always:
                                case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                                case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                    MoveToRow(cursorPosition.Row + InputStep);
                                    break;
                                case SettingsProfile.MoveToNextRowBehavior.Never:
                                    App.CurrentSong[cursorPosition] = (byte)(val % 16 * 16 + KeyInputs_Hex[k]);
                                    break;

                            }
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
                            App.CurrentSong[cursorPosition] = (byte)(val / 10 * 10 + KeyInputs_Decimal[k]);
                            switch (App.Settings.PatternEditor.StepAfterNumericInput) {
                                case SettingsProfile.MoveToNextRowBehavior.Always:
                                case SettingsProfile.MoveToNextRowBehavior.AfterCell:
                                case SettingsProfile.MoveToNextRowBehavior.AfterCellIncludingEffect:
                                    MoveToRow(cursorPosition.Row + InputStep);
                                    break;
                                case SettingsProfile.MoveToNextRowBehavior.Never:
                                    App.CurrentSong[cursorPosition] = (byte)(val % 10 * 10 + KeyInputs_Decimal[k]);
                                    break;

                            }
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
                        else if (cursorPosition.Column is CursorColumnType.Instrument1 or
                                 CursorColumnType.Instrument2 or
                                 CursorColumnType.Volume1 or
                                 CursorColumnType.Volume2) {
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
                        else if (cursorPosition.Column is CursorColumnType.Effect1Param1 or
                                 CursorColumnType.Effect2Param1 or
                                 CursorColumnType.Effect3Param1 or
                                 CursorColumnType.Effect4Param1) {
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
                        else if (cursorPosition.Column is CursorColumnType.Effect1Param2 or
                                 CursorColumnType.Effect2Param2 or
                                 CursorColumnType.Effect3Param2 or
                                 CursorColumnType.Effect4Param2) {
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
                                    SelectionPattern[r, c] != WTPattern.EVENT_NOTE_RELEASE &&
                                    SelectionPattern[r, c] != WTPattern.EVENT_NOTE_CUT &&
                                    SelectionPattern[r, c] != WTPattern.EVENT_EMPTY) {
                                    SelectionPattern[r, c] += Input.MouseScrollWheel(KeyModifier.Ctrl);
                                    performedTask = true;
                                }
                            }
                        }
                    }
                    if (Input.MouseScrollWheel(KeyModifier.Shift) != 0) {
                        for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                            for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                                if (SelectionPattern[r, c] != WTPattern.EVENT_EMPTY) {
                                    CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                                    if (cellType != CellType.Note &&
                                        cellType != CellType.Effect1 &&
                                        cellType != CellType.Effect2 &&
                                        cellType != CellType.Effect3 &&
                                        cellType != CellType.Effect4) {
                                        SelectionPattern[r, c] += Input.MouseScrollWheel(KeyModifier.Shift);
                                        performedTask = true;
                                    }
                                }
                            }
                        }
                    }
                    if (Input.MouseScrollWheel(KeyModifier.ShiftAlt) != 0) {
                        if (scaleClipboard == null) {
                            RecordOriginalSelectionContents();
                        }

                        ScaleSelection(Input.MouseScrollWheel(KeyModifier.ShiftAlt));
                        performedTask = true;

                    }

                    if (performedTask) {
                        AddToUndoHistory();
                    }
                }
            }
            #endregion
            #region backspace + insert + delete

            if (KeyPress(App.Shortcuts["Edit\\Backspace"])) {
                Backspace();
            }
            if (KeyPress(App.Shortcuts["Edit\\Insert"])) {
                Insert();
            }
            if (KeyPress(App.Shortcuts["Edit\\Delete"])) {
                Delete();
            }

            #endregion
            #region value increment and transposing
            if (App.Shortcuts["Pattern\\Transpose: note down"].IsPressedRepeat) {
                DecreaseNote();
            }
            if (App.Shortcuts["Pattern\\Transpose: note up"].IsPressedRepeat) {
                IncreaseNote();
            }
            if (App.Shortcuts["Pattern\\Transpose: octave down"].IsPressedRepeat) {
                DecreaseOctave();
            }
            if (App.Shortcuts["Pattern\\Transpose: octave up"].IsPressedRepeat) {
                IncreaseOctave();
            }
            if (App.Shortcuts["Pattern\\Decrease values"].IsPressedRepeat) {
                bool didSomething = false;
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                        if (cellType != CellType.Note &&
                            cellType != CellType.Effect1 &&
                            cellType != CellType.Effect2 &&
                            cellType != CellType.Effect3 &&
                            cellType != CellType.Effect4) {
                            if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                                SelectionPattern[r, c] -= 1;
                                didSomething = true;
                            }
                        }
                    }
                }
                if (didSomething) {
                    AddToUndoHistory();
                }
            }
            if (App.Shortcuts["Pattern\\Increase values"].IsPressedRepeat) {
                bool didSomething = false;
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                        if (cellType != CellType.Note &&
                            cellType != CellType.Effect1 &&
                            cellType != CellType.Effect2 &&
                            cellType != CellType.Effect3 &&
                            cellType != CellType.Effect4) {
                            if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                                SelectionPattern[r, c] += 1;
                                didSomething = true;
                            }
                        }
                    }
                }
                if (didSomething) {
                    AddToUndoHistory();
                }
            }
            if (App.Shortcuts["Pattern\\Coarse decrease values"].IsPressedRepeat) {
                bool didSomething = false;
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                        if (cellType != CellType.Note &&
                            cellType != CellType.Effect1 &&
                            cellType != CellType.Effect2 &&
                            cellType != CellType.Effect3 &&
                            cellType != CellType.Effect4) {
                            if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                                if (WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect1Parameter ||
                                    WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect2Parameter ||
                                    WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect3Parameter ||
                                    WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect4Parameter &&
                                    Helpers.IsEffectHex((char)SelectionPattern[r, c - 1])) {
                                    SelectionPattern[r, c] -= 16;
                                }
                                else {
                                    SelectionPattern[r, c] -= 10;
                                }

                                didSomething = true;
                            }
                        }
                    }
                }
                if (didSomething) {
                    AddToUndoHistory();
                }
            }
            if (App.Shortcuts["Pattern\\Coarse increase values"].IsPressedRepeat) {
                bool didSomething = false;
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                        CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                        if (cellType != CellType.Note &&
                            cellType != CellType.Effect1 &&
                            cellType != CellType.Effect2 &&
                            cellType != CellType.Effect3 &&
                            cellType != CellType.Effect4) {
                            if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                                if (WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect1Parameter ||
                                    WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect2Parameter ||
                                    WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect3Parameter ||
                                    WTPattern.GetCellTypeFromCellColumn(c) == CellType.Effect4Parameter &&
                                    Helpers.IsEffectHex((char)SelectionPattern[r, c - 1])) {
                                    SelectionPattern[r, c] += 16;
                                }
                                else {
                                    SelectionPattern[r, c] += 10;
                                }

                                didSomething = true;
                            }
                        }
                    }
                }
                if (didSomething) {
                    AddToUndoHistory();
                }
            }
            #endregion
            #region interpolate and reverse
            if (SelectionIsActive) {
                if (App.Shortcuts["Pattern\\Interpolate"].IsPressedRepeat) {
                    InterpolateSelection();
                }
                if (App.Shortcuts["Pattern\\Reverse"].IsPressedRepeat) {
                    ReverseSelection();
                }
            }
            #endregion
            #region copy/paste/cut
            if (SelectionIsActive) {
                if (App.Shortcuts["Edit\\Cut"].IsPressedRepeat) {
                    Cut();
                }
            }
            if (clipboard != null) {
                if (App.Shortcuts["Edit\\Paste"].IsPressedRepeat) {
                    PasteFromClipboard();
                }
                if (App.Shortcuts["Edit\\Paste and mix"].IsPressedRepeat) {
                    PasteAndMix();
                }
            }
            #endregion
            #region undo/redo
            if (App.Shortcuts["Edit\\Undo"].IsPressedRepeat) {
                Undo();
            }

            if (App.Shortcuts["Edit\\Redo"].IsPressedRepeat) {
                Redo();
            }
            #endregion
            #region alt+s replace instrument
            if (SelectionIsActive) {
                if (App.Shortcuts["Pattern\\Replace instrument"].IsPressedDown) {
                    ReplaceInstrument();
                }
            }
            #endregion
            #region alt+h humanize
            if (App.Shortcuts["Pattern\\Humanize volumes"].IsPressedDown) {
                Humanize();
            }
            #endregion
        }

        #region Draw Methods
        public void Draw() {
            playbackFrame = Playback.position.Frame;
            playbackRow = Playback.position.Row;
            if (Playback.IsPlaying && !AudioEngine.IsRendering && FollowMode) {
                SnapToPlaybackPosition();
            }
            renderCursorPos = cursorPosition;
            renderCursorPos.Normalize(App.CurrentSong);
            DrawRect(0, 0, width, height, App.Settings.Colors.Theme["Row background"]);

            DrawHeaderRect(0, -32, width);
            DrawRect(ROW_COLUMN_WIDTH - 2, -32, ChannelHeaders[LastVisibleChannel].x + ChannelHeaders[LastVisibleChannel].width - ROW_COLUMN_WIDTH + 4, 1, App.Settings.Colors.Theme["Channel separator"]);

            int frameWrap = 0;
            int frame = renderCursorPos.Frame;
            int row = renderCursorPos.Row;
            int length = App.CurrentSong[frame].GetModifiedLength();
            for (int i = NumVisibleLines / 2; i < NumVisibleLines; i++) {
                if (frameWrap == 0 || App.Settings.PatternEditor.ShowPreviousNextFrames) {
                    DrawRowBG(i, frame, row, frameWrap);
                }
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
                if (frameWrap == 0 || App.Settings.PatternEditor.ShowPreviousNextFrames) {
                    DrawRowBG(i, frame, row, frameWrap);
                }
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
            if (!App.CurrentSong[renderCursorPos.Frame].CellIsEmpty(renderCursorPos.Row, renderCursorPos.Channel, renderCursorPos.Column.ToCellType())) {
                DrawCursor(ref renderCursorPos);
            }

            frameWrap = 0;
            frame = renderCursorPos.Frame;
            row = renderCursorPos.Row;
            length = App.CurrentSong[frame].GetModifiedLength();
            for (int i = NumVisibleLines / 2; i < NumVisibleLines; i++) {
                if (frameWrap == 0 || App.Settings.PatternEditor.ShowPreviousNextFrames) {
                    DrawRow(i, frame, row, frameWrap);
                    if (frameWrap != 0) {
                        DrawRect(0, i * ROW_HEIGHT, width, ROW_HEIGHT, Helpers.Alpha(App.Settings.Colors.Theme["Row background"], 180));
                    }
                }
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
                if (frameWrap == 0 || App.Settings.PatternEditor.ShowPreviousNextFrames) {
                    DrawRow(i, frame, row, frameWrap);
                    if (frameWrap != 0) {
                        DrawRect(0, i * ROW_HEIGHT, width, ROW_HEIGHT, Helpers.Alpha(App.Settings.Colors.Theme["Row background"], 180));
                    }
                }
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
            if (App.CurrentSong[renderCursorPos.Frame].CellIsEmpty(renderCursorPos.Row, renderCursorPos.Channel, renderCursorPos.Column.ToCellType())) {
                DrawCursor(ref renderCursorPos);
            }

            DrawRect(ROW_COLUMN_WIDTH - 1, -32, 1, height + 32, App.Settings.Colors.Theme["Channel separator"]);
            for (int i = FirstVisibleChannel; i <= LastVisibleChannel; ++i) {
                ChannelHeaders[i].Draw();
                DrawRect(ChannelHeaders[i].x + ChannelHeaders[i].width, -32, 1, height + 32, App.Settings.Colors.Theme["Channel separator"]);
            }
            // DrawRect(channelHeaders[LastVisibleChannel].x + channelHeaders[LastVisibleChannel].x - 2, -32, 3, 1, App.Settings.Appearance.Theme.rowSeparator);
            //Write(selection.min.ToString(), 0, 0, Color.Red);
            //Write(selection.max.ToString(), 0, 20, Color.Red);
            //Write(cursorPosition.ToString(), 0, 30, Color.Cyan);
            DrawRect(width, -32, 1, height + 32, App.Settings.Colors.Theme["Channel separator"]);
            DrawRect(width + 1, -32, 1, 1, App.Settings.Colors.Theme["Channel separator"]);
            DrawRect(LastChannelEndPos + 1, 0, width - LastChannelEndPos - 1, height, App.Settings.Colors.Theme["Row background"]);
            DrawRect(0, channelScrollbar.y, ROW_COLUMN_WIDTH, channelScrollbar.height, UIColors.panel);
            channelScrollbar.Draw();
        }

        private void DrawRow(int line, int frame, int row, int frameWrap) {
            // get the row color
            Color rowTextColor = App.Settings.Colors.Theme["Row text"];
            if (row % App.CurrentSong.RowHighlightPrimary == 0) {
                rowTextColor = App.Settings.Colors.Theme["Row text (primary highlight)"];
            }
            else if (row % App.CurrentSong.RowHighlightSecondary == 0) {
                rowTextColor = App.Settings.Colors.Theme["Row text (secondary highlight)"];
            }

            // draw row numbers
            if (App.Settings.PatternEditor.ShowRowNumbersInHex) {
                WriteMonospaced(row.ToString("X2"), 6, line * ROW_HEIGHT, rowTextColor, 4);
            }
            else {
                WriteMonospaced(row.ToString("D3"), 4, line * ROW_HEIGHT, rowTextColor, 4);
            }

            // draw pattern events
            for (int channel = FirstVisibleChannel; channel <= LastVisibleChannel; ++channel) {
                DrawPatternEvent(frame, row, channel, frameWrap, GetStartPositionOfChannel(channel), line * ROW_HEIGHT, App.CurrentSong.NumEffectColumns[channel], rowTextColor);
            }
        }

        private void DrawPatternEvent(int frame, int row, int channel, int frameWrap, int x, int y, int effectColumns, Color rowTextColor) {

            int noteValue = App.CurrentSong[frame][row, channel, CellType.Note];
            int instrumentValue = App.CurrentSong[frame][row, channel, CellType.Instrument];
            int volumeValue = App.CurrentSong[frame][row, channel, CellType.Volume];

            bool isCursorOnThisRow = frameWrap == 0 && renderCursorPos.Row == row;
            bool isCursorOnThisEvent = isCursorOnThisRow && renderCursorPos.Channel == channel;
            Color emptyColor;
            if (isCursorOnThisRow) {
                emptyColor = App.Settings.Colors.Theme["Empty dashes tint"].MultiplyWith(rowTextColor);
            }
            else {
                emptyColor = App.Settings.Colors.Theme["Empty dashes tint"].MultiplyWith(rowTextColor);
            }
            // draw note

            if (noteValue == WTPattern.EVENT_NOTE_CUT) {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText) {
                    Write("OFF", x + 2, y, rowTextColor);
                }
                else {
                    DrawRect(x + 3, y + 2, 13, 2, rowTextColor);
                }
            }
            else if (noteValue == WTPattern.EVENT_NOTE_RELEASE) {
                if (App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText) {
                    Write("REL", x + 2, y, rowTextColor);
                }
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
                    if (App.CurrentModule.Instruments[instrumentValue] is WaveInstrument) {
                        instrumentColor = App.Settings.Colors.Theme["Instrument (wave)"];
                    }
                    else {
                        instrumentColor = App.Settings.Colors.Theme["Instrument (sample)"];
                    }
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
                volumeColor = App.Settings.PatternEditor.FadeVolumeColumn && !isCursorOverThisVolumeText
                    ? Helpers.Alpha(App.Settings.Colors.Theme["Volume"], (int)(volumeValue / 100f * 180 + (255 - 180)))
                    : App.Settings.Colors.Theme["Volume"];

                WriteMonospaced(volumeValue.ToString("D2"), x + 34, y, volumeColor, 4);
            }

            for (int i = 0; i < effectColumns; ++i) {
                int thisEffectType = App.CurrentSong[frame][row, channel, CellType.Effect1 + i * 2];
                int thisEffectParameter = App.CurrentSong[frame][row, channel, CellType.Effect1Parameter + i * 2];

                if (thisEffectType == WTPattern.EVENT_EMPTY) {
                    WriteMonospaced("···", x + 48 + 18 * i, y, emptyColor, 4);
                }
                else {
                    Write(Helpers.FlushString((char)thisEffectType + ""), x + 47 + 18 * i, y, App.Settings.Colors.Theme["Effect"]);
                    if (Helpers.IsEffectHex((char)thisEffectType)) {
                        WriteMonospaced(thisEffectParameter.ToString("X2"), x + 52 + 18 * i, y, App.Settings.Colors.Theme["Effect parameter"], 4);
                    }
                    else {
                        WriteMonospaced(thisEffectParameter.ToString("D2"), x + 52 + 18 * i, y, App.Settings.Colors.Theme["Effect parameter"], 4);
                    }
                }
            }
        }

        private void DrawHeaderRect(int x, int y, int width) {
            DrawRect(x, y, width, 31, Color.White);
            DrawRect(x, y + 20, width, 11, new Color(223, 224, 232));
        }

        private void DrawCursor(ref CursorPos position) {
            Rectangle rect = GetRectFromCursorPos(ref position);
            int width = position.Column == 0 ? 17 : 6;
            int offset = (int)position.Column switch {
                7 or 10 or 13 or 16 => -1,
                2 or 4 or 6 or 9 or 12 or 15 => 0,
                _ => 1
            };
            if (rect.X + offset > ROW_COLUMN_WIDTH) {
                DrawRect(rect.X + offset, rect.Y, width, ROW_HEIGHT, App.Settings.Colors.Theme["Cursor"]);
            }
            //Write("Chan: " + (position.Channel + 1), rect.X, rect.Y + 10, Color.White);
            //Write("Col: " + position.Column, rect.X, rect.Y + 20, Color.White);
            //Write("Oct: " + CurrentOctave, rect.X, rect.Y + 30, Color.White);
        }

        private void DrawRowBG(int line, int frame, int row, int frameWrap) {
            CursorPos thisPos = new CursorPos();
            thisPos.Row = row;
            thisPos.Frame = frame;
            Color rowBGcolor = new Color();
            bool needToDrawRowBG = true;
            int linePositionY = line * ROW_HEIGHT;

            if (frame == renderCursorPos.Frame && row == renderCursorPos.Row && frameWrap == 0) {
                rowBGcolor = EditMode ? App.Settings.Colors.Theme["Current row (editing)"] : App.Settings.Colors.Theme["Current row (default)"];
            }
            else if (!AudioEngine.IsRendering && Playback.IsPlaying && playbackFrame == frame && playbackRow == row) {
                rowBGcolor = App.Settings.Colors.Theme["Playback row"];
            }
            else if (row % App.CurrentSong.RowHighlightPrimary == 0) {
                rowBGcolor = App.Settings.Colors.Theme["Row background (primary highlight)"];
            }
            else if (row % App.CurrentSong.RowHighlightSecondary == 0) {
                rowBGcolor = App.Settings.Colors.Theme["Row background (secondary highlight)"];
            }
            else {
                // this row is not highlighted, no need to draw a background
                needToDrawRowBG = false;
            }

            if (needToDrawRowBG) {
                DrawRect(ROW_COLUMN_WIDTH, linePositionY, width - ROW_COLUMN_WIDTH, ROW_HEIGHT, rowBGcolor);
            }

            // if this row is within the selection bounds
            bool above = frame == selection.min.Frame ? row < selection.min.Row : frame < selection.min.Frame;
            bool below = frame == selection.min.Frame ? row > selection.max.Row : frame > selection.max.Frame;
            if (SelectionIsActive && !above && !below) {
                int start = ChannelHeaders[selection.min.Channel].x + GetColumnStartPositionOffset(selection.min.Column);
                int end = ChannelHeaders[selection.max.Channel].x + GetColumnStartPositionOffset(selection.max.Column) + GetWidthOfCursorColumn(selection.max.Column);
                bool endsOnRowSeparator = selection.max.Column == App.CurrentSong.GetLastCursorColumnOfChannel(selection.max.Channel);
                if (endsOnRowSeparator) {
                    end--;
                }

                if (start < ROW_COLUMN_WIDTH - 1) {
                    start = ROW_COLUMN_WIDTH - 1;
                }

                if (end < ROW_COLUMN_WIDTH - 1) {
                    end = ROW_COLUMN_WIDTH - 1;
                }

                if (start > end) {
                    return;
                }

                DrawRect(start, linePositionY, end - start + 1, ROW_HEIGHT, App.Settings.Colors.Theme["Selection"]);

                // draw selection outline
                DrawRect(start, linePositionY, 1, ROW_HEIGHT, App.Settings.Colors.Theme["Selection"]);
                DrawRect(end, linePositionY, 1, ROW_HEIGHT, App.Settings.Colors.Theme["Selection"]);
                if (selection.min.Row == row && selection.min.Frame == frame) {
                    DrawRect(start + 1, linePositionY, end - start - 1, 1, App.Settings.Colors.Theme["Selection"]);
                }

                if (selection.max.Row == row && selection.max.Frame == frame) {
                    DrawRect(start + 1, linePositionY + ROW_HEIGHT - 1, end - start - 1, 1, App.Settings.Colors.Theme["Selection"]);
                }
            }
        }
        #endregion

        #region move cursor methods

        /// <summary>
        /// Resets the cursor position and view to the beginning of the song, and clears undo history
        /// </summary>
        public void OnSwitchSong(bool haltPlayback = false) {
            if (Playback.IsPlaying && !haltPlayback) {
                Playback.Stop();
                Playback.Goto(0, 0);
                Playback.PlayFromBeginning();
            }
            else {
                Playback.Stop();
                Playback.Goto(0, 0);
            }
            cursorPosition.Initialize();
            lastCursorPosition.Initialize();
            CancelSelection();
            selection.Set(App.CurrentSong, cursorPosition, cursorPosition);
            lastSelection.Set(App.CurrentSong, cursorPosition, cursorPosition);
            ClearHistory();
            FirstVisibleChannel = 0;
            CalculateChannelPositioning(false);
        }

        /// <summary>
        /// Called when the module channel size is changed
        /// </summary>
        public void OnResizeChannels() {
            cursorPosition.Normalize(App.CurrentSong);
            lastCursorPosition.Normalize(App.CurrentSong);
            CancelSelection();
            selection.Set(App.CurrentSong, cursorPosition, cursorPosition);
            lastSelection.Set(App.CurrentSong, cursorPosition, cursorPosition);
            FirstVisibleChannel = 0;
            CalculateChannelPositioning(false);
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
        private void MoveCursorLeft() {
            int channel = cursorPosition.Channel;
            cursorPosition.MoveLeft(App.CurrentSong);
            if (!App.Settings.PatternEditor.WrapCursorHorizontally && cursorPosition.Channel > channel) {
                cursorPosition.MoveRight(App.CurrentSong);
            }
        }

        /// <summary>
        /// Moves the cursor one column to the right
        /// </summary>
        private void MoveCursorRight() {
            int channel = cursorPosition.Channel;
            cursorPosition.MoveRight(App.CurrentSong);
            if (!App.Settings.PatternEditor.WrapCursorHorizontally && cursorPosition.Channel < channel) {
                cursorPosition.MoveLeft(App.CurrentSong);
            }
        }

        private void MakeCursorVisibleInBounds() {
            if (cursorPosition.Channel > LastVisibleChannel - 1) {
                while (cursorPosition.Channel > LastVisibleChannel - 1) {
                    FirstVisibleChannel++;
                    if (FirstVisibleChannel > MaxChannelScroll) {
                        FirstVisibleChannel = MaxChannelScroll;
                        break;
                    }
                    CalculateChannelPositioning(true);
                }
            }

            if (cursorPosition.Channel < FirstVisibleChannel) {
                while (cursorPosition.Channel < FirstVisibleChannel) {
                    FirstVisibleChannel--;
                }
                CalculateChannelPositioning(true);
            }
        }

        /// <summary>
        /// Moves the cursor to the first column of a channel
        /// </summary>
        /// <param name="channel"></param>
        private void MoveToChannel(int channel) {
            cursorPosition.MoveToChannel(channel, App.CurrentSong);
        }
        #endregion

        #region selectionMethods

        /// <summary>
        /// Sets the selection start position
        /// </summary>
        /// <param name="pos"></param>
        private void SetSelectionStart(CursorPos pos) {
            selectionStart = pos;
        }

        /// <summary>
        /// Sets the selection end position
        /// </summary>
        /// <param name="pos"></param>
        private void SetSelectionEnd(CursorPos pos) {
            selectionEnd = pos;
        }

        /// <summary>
        /// Removes the selection
        /// </summary>
        private void CancelSelection() {
            selection = new PatternSelection(App.CurrentSong, cursorPosition, cursorPosition);
            selection.IsActive = false;
            scaleClipboard = null;
        }

        /// <summary>
        /// Selects the current channel, then the whole pattern
        /// </summary>
        public void SelectAll() {
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
            else { // full channel
                selection.IsActive = true;
                SetSelectionStart(cursorPosition);
                selectionStart.Row = 0;
                selectionStart.Column = 0;
                SetSelectionEnd(cursorPosition);
                selectionEnd.Row = CurrentPattern.GetModifiedLength() - 1;
                selectionEnd.Column = App.CurrentSong.GetLastCursorColumnOfChannel(selectionEnd.Channel);
            }
        }

        private void RecordOriginalSelectionContents() {
            scaleClipboard = new float[selection.Height, selection.Width];
            //clipboardStartCellType = selection.min.Column.ToCellType();
            for (int row = 0; row < selection.Height; row++) {
                for (int column = 0; column < selection.Width; column++) {
                    scaleClipboard[row, column] = (byte)SelectionPattern[selection.min.Row + row, selection.min.CellColumn + column];
                }
            }
        }

        #endregion

        public void ToggleEditMode() {
            EditMode = !EditMode;
        }

        /// <summary>
        /// Pulls all cells below the given position up one, leaving a blank cell at the end.
        /// </summary>
        private void PullCellsUp(int row, int cellColumn) {
            for (int i = row; i < 255; i++) {
                SelectionPattern.SetCellRaw(i, cellColumn, (byte)SelectionPattern[i + 1, cellColumn]);
            }
            SelectionPattern.SetCellRaw(255, cellColumn, WTPattern.EVENT_EMPTY);
        }

        /// <summary>
        /// Pushes all cell starting from pos down one. Creating a blank cell at pos
        /// </summary>
        private void PushCellsDown(int row, int cellColumn) {
            for (int i = 255; i > row; i--) {
                SelectionPattern.SetCellRaw(i, cellColumn, (byte)SelectionPattern[i - 1, cellColumn]);
            }
            SelectionPattern.SetCellRaw(row, cellColumn, WTPattern.EVENT_EMPTY);
        }

        /// <summary>
        /// Scales volumes and effect parameters in a direction relative to the maximum value in the selection
        /// </summary>
        /// <param name="direction"></param>
        private void ScaleSelection(int direction) {
            int selX = 0;
            int selY;
            float max = 0;
            int c = selection.min.CellColumn;
            CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
            if (cellType is CellType.Volume or
                CellType.Effect1Parameter or
                CellType.Effect2Parameter or
                CellType.Effect3Parameter or
                CellType.Effect4Parameter) {

                selY = 0;
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    if (cellType != CellType.Volume) {
                        if (Helpers.IsEffectHex((char)SelectionPattern[r, c - 1])) {
                            continue;
                        }
                    }
                    if (scaleClipboard[selY, selX] != WTPattern.EVENT_EMPTY) {
                        if (scaleClipboard[selY, selX] > max) {
                            max = scaleClipboard[selY, selX];
                        }
                    }
                    selY++;
                }

                float factor = (max + direction) / (float)max;

                selY = 0;
                for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                    if (cellType != CellType.Volume) {
                        if (Helpers.IsEffectHex((char)SelectionPattern[r, c - 1])) {
                            continue;
                        }
                    }
                    if (SelectionPattern[r, c] != WTPattern.EVENT_EMPTY) {
                        scaleClipboard[selY, selX] *= factor;
                        SelectionPattern[r, c] = (int)scaleClipboard[selY, selX];
                    }
                    selY++;
                }

            }

        }

        /// <summary>
        /// Multiplies all volumes and effect parameters in the selection by <c>factor</c>
        /// </summary>
        /// <param name="factor"></param>
        private void ScaleSelection(float factor) {
            for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                    if (SelectionPattern[r, c] != WTPattern.EVENT_EMPTY) {
                        CellType cellType = WTPattern.GetCellTypeFromCellColumn(c);
                        if (cellType is CellType.Volume or
                            CellType.Effect1Parameter or
                            CellType.Effect2Parameter or
                            CellType.Effect3Parameter or
                            CellType.Effect4Parameter) {
                            if (cellType != CellType.Volume) {
                                if (Helpers.IsEffectHex((char)SelectionPattern[r, c - 1])) {
                                    continue;
                                }
                            }
                            SelectionPattern[r, c] = (int)(SelectionPattern[r, c] * factor + 0.5f);
                        }
                    }
                }
            }
        }

        public void TryToEnterNote(int note, int? volume) {
            if (Input.focus != null || Input.focusTimer < 1 || App.VisualizerMode) {
                return;
            }

            if (EditMode && cursorPosition.Column == CursorColumnType.Note) {
                CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Note] = (byte)note;
                if (!InstrumentMask) {
                    CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Instrument] = (byte)App.InstrumentBank.CurrentInstrumentIndex;
                }
                if (volume.HasValue) {
                    CurrentPattern[cursorPosition.Row, cursorPosition.Channel, CellType.Volume] = (byte)volume.Value;
                }
                MoveToRow(cursorPosition.Row + InputStep);
                AddToUndoHistory();
            }
        }

        public void ClearHistory() {
            App.CurrentSong.SetPatternsDirty();
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
            if (!EditMode) {
                return;
            }

            while (history.Count - 1 > historyIndex) {
                history.RemoveAt(history.Count - 1);
            }
            history.Add(new PatternEditorState(App.CurrentSong, GetPreviousEditorPosition(), GetCurrentEditorPosition()));
            historyIndex++;
            if (history.Count > 64) {
                history.RemoveAt(0);
                historyIndex--;
            }
            App.CurrentModule.SetDirty();
        }

        public void Undo() {
            cursorPosition = history[historyIndex].PrePosition.CursorPosition;
            SetSelectionStart(history[historyIndex].PrePosition.SelectionStart);
            SetSelectionEnd(history[historyIndex].PrePosition.SelectionEnd);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            selection.IsActive = history[historyIndex].PrePosition.SelectionIsActive;
            historyIndex--;
            if (historyIndex < 0) {
                historyIndex = 0;
            }

            history[historyIndex].RestoreIntoSong(App.CurrentSong);
            cursorPosition.Normalize(App.CurrentSong);
            App.CurrentModule.SetDirty();
        }
        public void Redo() {
            historyIndex++;
            if (historyIndex >= history.Count) {
                historyIndex = history.Count - 1;
            }

            history[historyIndex].RestoreIntoSong(App.CurrentSong);
            cursorPosition = history[historyIndex].PostPosition.CursorPosition;
            cursorPosition.Normalize(App.CurrentSong);
            SetSelectionStart(history[historyIndex].PostPosition.SelectionStart);
            SetSelectionEnd(history[historyIndex].PostPosition.SelectionEnd);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            selection.IsActive = history[historyIndex].PostPosition.SelectionIsActive;
            App.CurrentModule.SetDirty();
        }

        public PatternEditorPosition GetCurrentEditorPosition() {
            return new PatternEditorPosition(cursorPosition, selection);
        }
        public PatternEditorPosition GetPreviousEditorPosition() {
            return new PatternEditorPosition(lastCursorPosition, lastSelection);
        }

        /// <summary>
        /// Removes all data in the selection and copies it to the clipboard
        /// </summary>
        public void Cut() {
            if (SelectionIsActive) {
                CopyToClipboard();
                Delete();
            }
        }

        /// <summary>
        /// Removes all data in the selection
        /// </summary>
        public void Delete() {
            if (SelectionIsActive) {
                for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                    for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                        SelectionPattern[row, column] = WTPattern.EVENT_EMPTY;
                    }
                }
            }
            else {
                for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                    SelectionPattern[cursorPosition.Row, column] = WTPattern.EVENT_EMPTY;
                    if (WTPattern.GetCellTypeFromCellColumn(column) == CellType.Note) {
                        SelectionPattern[cursorPosition.Row, column + 1] = WTPattern.EVENT_EMPTY;
                    }
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
                for (int c = 0; c < App.CurrentSong.GetNumColumns(cursorPosition.Channel); ++c) {
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
                    clipboard[row, column] = (byte)SelectionPattern[selection.min.Row + row, selection.min.CellColumn + column];
                }
            }
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the song at the current cursor position
        /// </summary>
        public void PasteFromClipboard() {

            int clipboardWidth = clipboard.GetLength(1);
            bool isOnlyEffects = false;
            for (int i = 0; i < 4; ++i) {
                if (clipboardStartCellType == CellType.Effect1 + i * 2 && clipboardWidth < 9 - i * 2) {
                    isOnlyEffects = true;
                }
            }

            CursorPos p = cursorPosition;
            p.Column = clipboardStartCellType.ToNearestCursorColumn();
            if (isOnlyEffects) {
                for (int i = 0; i < 4; ++i) {
                    if (cursorPosition.Column == CursorColumnType.Effect1 + i * 3 ||
                    cursorPosition.Column == CursorColumnType.Effect1Param1 + i * 3 ||
                    cursorPosition.Column == CursorColumnType.Effect1Param2 + i * 3) {
                        p.Column = CursorColumnType.Effect1 + i * 3;
                        if (clipboardWidth > 8 - i * 2) {
                            clipboardWidth = 8 - i * 2;
                        }
                    }
                }
            }

            int columnStart = p.CellColumn;
            int patternHeight = 256;
            int patternWidth = CurrentPattern.Width;
            for (int row = 0; row < clipboard.GetLength(0); row++) {
                if (cursorPosition.Row + row >= patternHeight) {
                    break;
                }

                for (int column = 0; column < clipboardWidth; column++) {
                    if (columnStart + column >= patternWidth) {
                        break;
                    }

                    CurrentPattern[p.Row + row, columnStart + column] = clipboard[row, column];
                }
            }
            selection.IsActive = true;
            SetSelectionStart(p);
            p.MoveToRow(Math.Clamp(p.Row + clipboard.GetLength(0) - 1, 0, CurrentPattern.GetModifiedLength() - 1), App.CurrentSong);
            p.MoveToCellColumn(p.CellColumn + clipboardWidth - 1);
            p.NormalizeHorizontally(App.CurrentSong);
            SetSelectionEnd(p);
            selection.Set(App.CurrentSong, selectionStart, selectionEnd);
            AddToUndoHistory();
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the song at the current cursor position, without overwriting existing contents.
        /// </summary>
        public void PasteAndMix() {
            int clipboardWidth = clipboard.GetLength(1);
            bool isOnlyEffects = false;
            for (int i = 0; i < 4; ++i) {
                if (clipboardStartCellType == CellType.Effect1 + i * 2 && clipboardWidth < 9 - i * 2) {
                    isOnlyEffects = true;
                }
            }

            CursorPos p = cursorPosition;
            p.Column = clipboardStartCellType.ToNearestCursorColumn();
            if (isOnlyEffects) {
                for (int i = 0; i < 4; ++i) {
                    if (cursorPosition.Column == CursorColumnType.Effect1 + i * 3 ||
                    cursorPosition.Column == CursorColumnType.Effect1Param1 + i * 3 ||
                    cursorPosition.Column == CursorColumnType.Effect1Param2 + i * 3) {
                        p.Column = CursorColumnType.Effect1 + i * 3;
                        if (clipboardWidth > 8 - i * 2) {
                            clipboardWidth = 8 - i * 2;
                        }
                    }
                }
            }

            int columnStart = p.CellColumn;
            int patternHeight = 256;
            int patternWidth = CurrentPattern.Width;
            for (int row = 0; row < clipboard.GetLength(0); row++) {
                if (cursorPosition.Row + row >= patternHeight) {
                    break;
                }

                for (int column = 0; column < clipboardWidth; column++) {
                    if (columnStart + column >= patternWidth) {
                        break;
                    }
                    if (WTPattern.GetCellTypeFromCellColumn(columnStart + column) is CellType.Effect1 or
                        CellType.Effect2 or
                        CellType.Effect3 or
                        CellType.Effect4) {
                        if (CurrentPattern.CellIsEmpty(cursorPosition.Row + row, columnStart + column)) {
                            CurrentPattern[cursorPosition.Row + row, columnStart + column] = clipboard[row, column];
                            CurrentPattern[cursorPosition.Row + row, columnStart + column + 1] = clipboard[row, column + 1];
                        }
                    }
                    else if (CurrentPattern.CellIsEmpty(cursorPosition.Row + row, columnStart + column)) {
                        CurrentPattern[cursorPosition.Row + row, columnStart + column] = clipboard[row, column];
                    }
                }
            }
            selection.IsActive = true;
            SetSelectionStart(p);
            p.MoveToRow(Math.Clamp(p.Row + clipboard.GetLength(0) - 1, 0, CurrentPattern.GetModifiedLength() - 1), App.CurrentSong);
            p.MoveToCellColumn(p.CellColumn + clipboardWidth - 1);
            p.NormalizeHorizontally(App.CurrentSong);
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
            if (p.Column is CursorColumnType.Effect1 or CursorColumnType.Effect2 or CursorColumnType.Effect3 or CursorColumnType.Effect4) {
                col++;
            }
            WTPattern pattern = SelectionPattern;
            int val1 = pattern[selection.min.Row, col];
            int val2 = pattern[selection.max.Row, col];
            if (pattern.CellIsEmptyOrNoteCutRelease(selection.min.Row, col)) {
                return;
            }

            if (pattern.CellIsEmptyOrNoteCutRelease(selection.max.Row, col)) {
                return;
            }

            int startRow = selection.min.Row;
            int endRow = selection.max.Row;
            for (int r = startRow; r < endRow; r++) {
                if (p.Column is CursorColumnType.Effect1 or CursorColumnType.Effect2 or CursorColumnType.Effect3 or CursorColumnType.Effect4) {
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
            int startRow = selection.min.Row;
            int endRow = selection.max.Row;
            for (int r = 0; r < selection.Height / 2; ++r) {
                for (int col = selection.min.CellColumn; col <= selection.max.CellColumn; ++col) {
                    int val1 = SelectionPattern[startRow + r, col];
                    int val2 = SelectionPattern[endRow - r, col];
                    SelectionPattern.SetCellRaw(startRow + r, col, (byte)val2);
                    SelectionPattern.SetCellRaw(endRow - r, col, (byte)val1);
                }
            }
            AddToUndoHistory();
        }

        /// <summary>
        /// Replaces all instrument columns in the selection with the currently selected instrument in the instrument bank
        /// </summary>
        public void ReplaceInstrument() {
            for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                    if (WTPattern.GetCellTypeFromCellColumn(column) == CellType.Instrument && !SelectionPattern.CellIsEmpty(row, column)) {
                        SelectionPattern[row, column] = (byte)App.InstrumentBank.CurrentInstrumentIndex;
                    }
                }
            }
            AddToUndoHistory();

        }

        /// <summary>
        /// Randomizes volumes in the selection by +/-<c>randomOffset</c>
        /// </summary>
        /// <param name="randomOffset"></param>
        public void RandomizeSelectedVolumes(int randomOffset) {
            if (SelectionIsActive) {
                Random r = new Random();
                for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                    for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                        if (WTPattern.GetCellTypeFromCellColumn(column) == CellType.Volume && !SelectionPattern.CellIsEmpty(row, column)) {
                            SelectionPattern[row, column] = (byte)Math.Clamp(SelectionPattern[row, column] + r.Next(-randomOffset, randomOffset), 0, 99);
                        }
                    }
                }
            }
            AddToUndoHistory();
        }

        public void ExpandSelection() {
            StretchSelection("1 0");
        }
        public void ShrinkSelection() {
            if (SelectionIsActive) {
                StretchSelection("2");
            }
        }
        public void OpenStretchDialog() {
            if (SelectionIsActive) {
                Dialogs.stretchDialog.Open(this);
            }
        }

        public void StretchSelection(string stretchPattern) {
            if (SelectionIsActive) {
                int[] values = StretchPatternToIntArray(stretchPattern);
                if (values.Length > 0) {

                    byte[,] selectClip = new byte[selection.Height, selection.Width];
                    for (int row = 0; row < selection.Height; row++) {
                        for (int column = 0; column < selection.Width; column++) {
                            selectClip[row, column] = (byte)SelectionPattern[selection.min.Row + row, selection.min.CellColumn + column];
                        }
                    }

                    int valueIndex = 0;
                    int rowIndex = 0;
                    for (int row = selection.min.Row; row <= selection.max.Row; ++row) {
                        int c = 0;
                        for (int column = selection.min.CellColumn; column <= selection.max.CellColumn; ++column) {
                            SelectionPattern[row, column] = values[valueIndex] == 0 || rowIndex >= selectClip.GetLength(0) ? WTPattern.EVENT_EMPTY : selectClip[rowIndex, c];
                            c++;
                        }
                        rowIndex += values[valueIndex];
                        valueIndex++;
                        if (valueIndex >= values.Length) {
                            valueIndex = 0;
                        }
                    }
                    AddToUndoHistory();
                }
            }
        }

        private static int[] StretchPatternToIntArray(string text) {
            List<int> ticks = [];
            foreach (string word in text.Split(' ')) {
                if (word.IsNumeric()) {
                    if (int.TryParse(word, out int val)) {
                        ticks.Add(val);
                    }
                }
            }
            if (ticks.Count == 0) {
                ticks.Add(1);
            }

            return ticks.ToArray();
        }

        public void IncreaseNote() {
            bool didSomething = false;
            for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                    if (WTPattern.GetCellTypeFromCellColumn(c) == CellType.Note) {
                        if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                            SelectionPattern[r, c] += 1;
                            didSomething = true;
                        }
                    }
                }
            }
            if (didSomething) {
                AddToUndoHistory();
            }
        }

        public void DecreaseNote() {
            bool didSomething = false;
            for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                    if (WTPattern.GetCellTypeFromCellColumn(c) == CellType.Note) {
                        if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                            SelectionPattern[r, c] -= 1;
                            didSomething = true;
                        }
                    }
                }
            }
            if (didSomething) {
                AddToUndoHistory();
            }
        }

        public void IncreaseOctave() {
            bool didSomething = false;
            for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                    if (WTPattern.GetCellTypeFromCellColumn(c) == CellType.Note) {
                        if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                            SelectionPattern[r, c] += 12;
                            didSomething = true;
                        }
                    }
                }
            }
            if (didSomething) {
                AddToUndoHistory();
            }
        }

        public void DecreaseOctave() {
            bool didSomething = false;
            for (int r = selection.min.Row; r <= selection.max.Row; ++r) {
                for (int c = selection.min.CellColumn; c <= selection.max.CellColumn; ++c) {
                    if (WTPattern.GetCellTypeFromCellColumn(c) == CellType.Note) {
                        if (!SelectionPattern.CellIsEmptyOrNoteCutRelease(r, c)) {
                            SelectionPattern[r, c] -= 12;
                            didSomething = true;
                        }
                    }
                }
            }
            if (didSomething) {
                AddToUndoHistory();
            }
        }

        public void Humanize() {
            if (SelectionIsActive) {
                Dialogs.humanizeDialog.Open(this);
            }
        }

        #region frame methods

        /// <summary>
        /// Moves to the next frame in the song
        /// </summary>
        public void NextFrame() {
            if (Playback.IsPlaying && FollowMode) {
                Playback.GotoNextFrame();
            }
            else {
                MoveToFrame(cursorPosition.Frame + 1);
                CancelSelection();
            }
        }

        /// <summary>
        /// Moves to the previous frame in the song
        /// </summary>
        public void PreviousFrame() {
            if (Playback.IsPlaying && FollowMode) {
                Playback.GotoPreviousFrame();
            }
            else {
                MoveToFrame(cursorPosition.Frame - 1);
                CancelSelection();
            }
        }

        /// <summary>
        /// Inserts a new frame at the currrent editor position, and moves the editor to that frame.<br></br>
        /// The new frame will have the next empty pattern
        /// </summary>
        public void InsertNewFrame() {
            App.CurrentSong.InsertNewFrame(cursorPosition.Frame + 1);
            if (Playback.IsPlaying && App.PatternEditor.FollowMode) {
                Playback.GotoNextFrame();
            }
            else {
                MoveToFrame(cursorPosition.Frame + 1);
            }
        }

        /// <summary>
        /// Duplicates the frame at the current position
        /// </summary>
        public void DuplicateFrame() {
            App.CurrentSong.DuplicateFrame(cursorPosition.Frame);
            if (Playback.IsPlaying && App.PatternEditor.FollowMode) {
                Playback.GotoNextFrame();
            }
            else {
                MoveToFrame(cursorPosition.Frame + 1);
            }
        }

        /// <summary>
        /// Removes the current frame
        /// </summary>
        public void RemoveFrame() {
            App.CurrentSong.RemoveFrame(cursorPosition.Frame);
            if (cursorPosition.Frame > 0) {
                MoveToFrame(cursorPosition.Frame - 1);
                if (Playback.IsPlaying && App.PatternEditor.FollowMode) {
                    Playback.GotoPreviousFrame();
                }
            }
        }

        /// <summary>
        /// Moves the current frame forward in the sequence
        /// </summary>
        public void MoveFrameRight() {
            App.CurrentSong.SwapFrames(cursorPosition.Frame, cursorPosition.Frame + 1);
            MoveToFrame(cursorPosition.Frame + 1);
        }
        /// <summary>
        /// Moves the current frame backward in the sequence
        /// </summary>
        public void MoveFrameLeft() {
            App.CurrentSong.SwapFrames(cursorPosition.Frame, cursorPosition.Frame - 1);
            MoveToFrame(cursorPosition.Frame - 1);
        }
        /// <summary>
        /// Increases the current frame's pattern
        /// </summary>
        public void IncreaseFramePatternIndex() {
            CurrentFrame.PatternIndex++;
            AddToUndoHistory();
        }
        /// <summary>
        /// Decreases the current frame's pattern
        /// </summary>
        public void DecreaseFramePatternIndex() {
            CurrentFrame.PatternIndex--;
            AddToUndoHistory();
        }
        #endregion

        /// <summary>
        /// Snaps the cursor position to the playback row.
        /// </summary>
        public void SnapToPlaybackPosition() {
            if (FollowMode) {
                Playback.position.Normalize(App.CurrentSong);
                cursorPosition.Frame = Playback.position.Frame;
                cursorPosition.Row = Playback.position.Row;
            }
        }

        /// <summary>
        /// Moves the cursor to the first row of the current frame
        /// </summary>
        public void GoToTopOfFrame() {
            cursorPosition.Row = 0;
        }

        /// <summary>
        /// Moves the cursor to the last row of the current frame
        /// </summary>
        public void GoToBottomOfFrame() {
            cursorPosition.Row = CurrentPattern.GetModifiedLength() - 1;
        }

        /// <summary>
        /// Gets a key press, taking into account the key repeat setting in preferences
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyModifier"></param>
        /// <returns></returns>
        private static bool KeyPress(Keys key, KeyModifier keyModifier) {

            return App.Settings.PatternEditor.KeyRepeat ? Input.GetKeyRepeat(key, keyModifier) : Input.GetKeyDown(key, keyModifier);
        }

        /// <summary>
        /// Gets a key press, taking into account the key repeat setting in preferences
        /// </summary>
        /// <returns></returns>
        private static bool KeyPress(KeyboardShortcut shortcut) {
            return KeyPress(shortcut.Key, shortcut.Modifier);
        }

        /// <summary>
        /// Calculates where channels are positioned based on the horziontal scroll and their expansions
        /// </summary>
        public void CalculateChannelPositioning(bool updateChannelHeaders) {
            // get the total width of channels that are not visible on the left side
            int prevWidth = 0;
            for (int i = 0; i < FirstVisibleChannel; ++i) {
                prevWidth += GetWidthOfChannel(i);
            }
            // figure out which channel headers are visible, and enable/position/update them
            int px = ROW_COLUMN_WIDTH - prevWidth;
            for (int channel = 0; channel < App.CurrentModule.ChannelCount; ++channel) {

                if (px >= ROW_COLUMN_WIDTH && px < width) {
                    ChannelHeaders[channel].enabled = true;
                    LastVisibleChannel = channel;
                }
                else {
                    ChannelHeaders[channel].enabled = false;
                }
                ChannelHeaders[channel].x = px;
                ChannelHeaders[channel].width = GetWidthOfChannel(channel) - 1;
                ChannelHeaders[channel].NumEffectColumns = App.CurrentSong.NumEffectColumns[channel];
                if (updateChannelHeaders) {
                    ChannelHeaders[channel].Update();

                    // if the user changed the number of effect columns
                    if (ChannelHeaders[channel].NumEffectColumns != App.CurrentSong.NumEffectColumns[channel]) {
                        App.CurrentSong.NumEffectColumns[channel] = ChannelHeaders[channel].NumEffectColumns;
                        ChannelHeaders[channel].width = GetWidthOfChannel(channel) - 1;
                        cursorPosition.Normalize(App.CurrentSong);
                    }
                }
                px += GetWidthOfChannel(channel);
            }
            LastChannelEndPos = ChannelHeaders[App.CurrentModule.ChannelCount - 1].x + ChannelHeaders[App.CurrentModule.ChannelCount - 1].width;
            MaxChannelScroll = GetMaximumChannelScroll();
        }

        private int GetMaximumChannelScroll() {
            int w = width - ROW_COLUMN_WIDTH;
            for (int channel = App.CurrentModule.ChannelCount - 1; channel >= 0; --channel) {
                w -= ChannelHeaders[channel].width + 1;
                if (w <= 0) {
                    return channel + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns the line index the mouse is under (Render row)
        /// </summary>
        /// <returns></returns>
        private int GetMouseLineNumber() {
            return MouseY < 0 ? -1 : MouseY / ROW_HEIGHT;
        }

        /// <summary>
        /// Returns the number of lines to be rendered in the pattern editor
        /// </summary>
        /// <returns></returns>
        private int NumVisibleLines {
            get {
                return height / ROW_HEIGHT + 1;
            }
        }

        /// <summary>
        /// Returns the channel under an x position
        /// </summary>
        /// <param name="x"></param>
        /// <returns>Channel number, -1 means the row number's column</returns>
        private int GetChannelAtPoint(int x) {

            // row column width
            int width = ROW_COLUMN_WIDTH;

            // return -1 if the mouse is over the row numbers column
            if (x <= width) {
                return -1;
            }

            // start at the first visible channel
            int c = FirstVisibleChannel;
            width += GetWidthOfChannel(c);

            while (width <= x) {
                c++;
                if (c >= App.CurrentModule.ChannelCount) {
                    return c - 1;
                }

                width += GetWidthOfChannel(c);
            }
            return c;
        }

        /// <summary>
        /// Returns the column under an x position
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private CursorColumnType GetColumnAtPoint(int x) {

            int channel = GetChannelAtPoint(x);
            if (channel < 0) {
                return 0;
            }

            if (channel > LastVisibleChannel) {
                return App.CurrentSong.GetLastCursorColumnOfChannel(channel);
            }

            x -= GetStartPositionOfChannel(channel);

            CursorColumnType col = 0;
            x -= GetWidthOfCursorColumn(col);
            while (x > 0) {
                col++;
                if (col > App.CurrentSong.GetLastCursorColumnOfChannel(channel)) {
                    return col - 1;
                }

                x -= GetWidthOfCursorColumn(col);
            }

            return col;

        }

        /// <summary>
        /// Gets the cursor position under a point
        /// </summary>
        /// <returns></returns>
        private CursorPos GetCursorPositionFromPoint(int x, int y) {
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
        private CursorPos GetCursorPositionFromPointClampedToFrame(int x, int y, int frame, bool isAboveClampedFrame) {
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
                p.Row = isAboveClampedFrame ? App.CurrentSong[frame].GetModifiedLength() - 1 : 0;
            }
            return p;
        }

        /// <summary>
        /// Returns the screen bounds of a cursor position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Rectangle GetRectFromCursorPos(ref CursorPos position) {

            //position.Normalize(CurrentSong);
            int x = ChannelHeaders[position.Channel].x + GetColumnStartPositionOffset(position.Column);

            int lineNumber = NumVisibleLines / 2;
            //while (p.IsBelow(position)) {
            //    p.MoveToRow(p.Row - 1, App.CurrentSong);
            //    lineNumber--;
            //}
            //while (p.IsAbove(position)) {
            //    p.MoveToRow(p.Row + 1, App.CurrentSong);
            //    lineNumber++;
            //}
            return new Rectangle(x, lineNumber * ROW_HEIGHT, GetWidthOfCursorColumn(position.Column), ROW_HEIGHT);
        }

        /// <summary>
        /// Returns the starting x position of a channel visible in the frame view
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private int GetStartPositionOfChannel(int channel) {
            return ChannelHeaders[channel].x;
        }

        /// <summary>
        /// Returns the width of a channel track in pixels
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private static int GetWidthOfChannel(int channel) {
            // width of note, inst, vol together is 45
            // width of effect + effect param is 18
            // width of the channel separator is 1
            return 45 + 18 * App.CurrentSong.NumEffectColumns[channel] + 1;
        }

        /// <summary>
        /// The offset start position of columns from the left of a channel track
        /// </summary>
        private static int GetColumnStartPositionOffset(CursorColumnType cursorColumn) {
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
        private static int GetWidthOfCursorColumn(CursorColumnType cursorColumn) {
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
        private readonly Dictionary<string, int> KeyInputs_Piano = new Dictionary<string, int>() {
            { "Piano\\Note off", WTPattern.EVENT_NOTE_CUT },
            { "Piano\\Note release", WTPattern.EVENT_NOTE_RELEASE },
        };
        private readonly Dictionary<Keys, char> KeyInputs_Effect = new Dictionary<Keys, char>() {
            {Keys.D0, '0'},
            {Keys.NumPad0, '0'},
            {Keys.D1, '1'},
            {Keys.NumPad1, '1'},
            {Keys.D2, '2'},
            {Keys.NumPad2, '2'},
            {Keys.D3, '3'},
            {Keys.NumPad3, '3'},
            {Keys.D4, '4'},
            {Keys.NumPad4, '4'},
            {Keys.D7, '7'},
            {Keys.NumPad7, '7'},
            {Keys.D8, '8'},
            {Keys.NumPad8, '8'},
            {Keys.D9, '9'},
            {Keys.NumPad9, '9'},
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
            {Keys.Y, 'Y'},
            {Keys.K, 'K'},
            {Keys.X, 'X'},
            {Keys.H, 'H'},
        };
        private readonly Dictionary<Keys, int> KeyInputs_Hex = new Dictionary<Keys, int>() {
            {Keys.D0, 0},
            {Keys.NumPad0, 0},
            {Keys.D1, 1},
            {Keys.NumPad1, 1},
            {Keys.D2, 2},
            {Keys.NumPad2, 2},
            {Keys.D3, 3},
            {Keys.NumPad3, 3},
            {Keys.D4, 4},
            {Keys.NumPad4, 4},
            {Keys.D5, 5},
            {Keys.NumPad5, 5},
            {Keys.D6, 6},
            {Keys.NumPad6, 6},
            {Keys.D7, 7},
            {Keys.NumPad7, 7},
            {Keys.D8, 8},
            {Keys.NumPad8, 8},
            {Keys.D9, 9},
            {Keys.NumPad9, 9},
            {Keys.A, 10},
            {Keys.B, 11},
            {Keys.C, 12},
            {Keys.D, 13},
            {Keys.E, 14},
            {Keys.F, 15},
        };
        private readonly Dictionary<Keys, int> KeyInputs_Decimal = new Dictionary<Keys, int>() {
            {Keys.D0, 0},
            {Keys.NumPad0, 0},
            {Keys.D1, 1},
            {Keys.NumPad1, 1},
            {Keys.D2, 2},
            {Keys.NumPad2, 2},
            {Keys.D3, 3},
            {Keys.NumPad3, 3},
            {Keys.D4, 4},
            {Keys.NumPad4, 4},
            {Keys.D5, 5},
            {Keys.NumPad5, 5},
            {Keys.D6, 6},
            {Keys.NumPad6, 6},
            {Keys.D7, 7},
            {Keys.NumPad7, 7},
            {Keys.D8, 8},
            {Keys.NumPad8, 8},
            {Keys.D9, 9},
            {Keys.NumPad9, 9},
        };
        #endregion
    }
}
