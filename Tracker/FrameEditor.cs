using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Tracker;
using System.Diagnostics;

namespace WaveTracker {
    public static class FrameEditor {

        /// <summary>
        /// starting point of when channels will be rendered, 0 is leftmost
        /// </summary>
        public static int channelScroll;
        public static Song thisSong => Song.currentSong;
        public static bool canEdit;
        public static bool followMode = true;
        static bool playback;
        public static Frame thisFrame => thisSong.frames[currentFrame];
        public static short[] thisRow => thisFrame.pattern[currentRow];
        public static bool active = true;
        public static int currentOctave = 3;
        public static int step = 1;
        public static int currentFrame;
        public static bool[] channelToggles = new bool[Song.CHANNEL_COUNT];
        public static bool selectionActive;
        public static Point selectionStart, selectionEnd;
        public static Point selectionMin, selectionMax;
        public static Point lastSelMin, lastSelMax;
        public static int selectionFrame;
        public static bool lastSelActive;
        /// <summary>
        /// Which row was the cursor on last frame
        /// </summary>
        public static int lastCursorRow;
        /// <summary>
        /// Which column was the cursor in last frame
        /// </summary>
        public static int lastCursorCol;
        /// <summary>
        /// Which song-frame was being edited last frame
        /// </summary>
        public static int lastFrame;
        public static bool isDragging;
        public static bool instrumentMask;
        public static List<List<short>> clipboard = new List<List<short>>();
        public static List<List<float>> scaleclipboard = new List<List<float>>();

        public static int clipboardStartCol;
        public static List<FrameEditorState> history = new List<FrameEditorState>();
        public static int historyIndex = 0;
        public static UI.ScrollbarHorizontal channelScrollbar;
        static bool lastSelectionActive;

        public static int patternEditorLeftBound { get { return 22; } }
        public static int patternEditorRightBound { get { return 790; } }
        public static int patternEditorTopBound { get { return 182; } }
        public static int patternEditorBottomBound { get { return Game1.bottomOfScreen - 15; } }

        /// <summary>
        /// The row that the cursor is on
        /// </summary>
        public static int currentRow { get { return cursorRow; } }
        /// <summary>
        /// The column in the pattern data that the cursor will edit, (5 per channel: note, instrument, volume, effect or effect parameter)
        /// </summary>
        public static int currentColumn {
            get {
                int channel = cursorColumn / 8;
                int positionAlongChannel = cursorColumn % 8;
                return positionAlongChannel switch {
                    0 => 0 + channel * 5,
                    1 or 2 => 1 + channel * 5,
                    3 or 4 => 2 + channel * 5,
                    5 => 3 + channel * 5,
                    6 or 7 => 4 + channel * 5,
                    _ => 0,
                };
            }
        }

        /// <summary>
        /// The position of the cursor along the x axis (8 per channel, note, instrument digit 1, instrument digit 2, volume digit 1, volume digit 2, 
        /// </summary>
        public static int cursorColumn { get; set; }
        /// <summary>
        /// The position of the cursor along the y axis
        /// </summary>
        public static int cursorRow { get; set; }


        public static void Update() {

            // If out of focus do not run anything
            if (Input.focus != null)
                return;

            // if history is empty, intialize it.
            if (history.Count == 0)
                ClearHistory();


            lastCursorCol = cursorColumn;
            lastCursorRow = cursorRow;
            lastSelActive = selectionActive;
            lastSelMin = selectionMin;
            lastSelMax = selectionMax;
            lastFrame = currentFrame;
            playback = Playback.isPlaying;
            int mcolumn = getCursorColumnFromGlobalX(Input.lastClickLocation.X);
            int mrow = getRowFromGlobalY(Input.lastClickLocation.Y);

            if (Input.GetKeyRepeat(Keys.Space, KeyModifier.None))
                canEdit = !canEdit;
            if (!Game1.VisualizerMode) {
                #region selection with mouse

                if (Input.MousePositionX < patternEditorRightBound) {
                    if (Input.MousePositionY > patternEditorTopBound && Input.MouseScrollWheel(KeyModifier.Alt) != 0) {
                        channelScroll -= Input.MouseScrollWheel(KeyModifier.Alt);
                    }
                    if (Input.GetClickDown(KeyModifier._Any)) {
                        if (Input.MousePositionY > patternEditorTopBound && Input.MousePositionX < patternEditorRightBound)
                            selectionActive = false;
                    }
                    if (Input.GetDoubleClick(KeyModifier._Any)) {
                        if (mouseInBounds(mrow, mcolumn - channelScroll * 8) && !selectionActive) {
                            int chan = currentColumn / 5;
                            selectionStart.X = chan * 5;
                            selectionEnd.X = chan * 5 + 4;
                            selectionStart.Y = 0;
                            selectionEnd.Y = thisFrame.GetLastRow();
                            selectionFrame = currentFrame;
                            selectionActive = true;
                        }
                    }
                    if (Input.GetClick(KeyModifier._Any)) {
                        if (mouseInBounds(mrow, mcolumn - channelScroll * 8) && Input.doubleClick == false) {
                            if (Vector2.Distance(Input.MousePos, Input.lastClickLocation.ToVector2()) > 7) {
                                if (!isDragging) {
                                    selectionStart = new Point(cursorColToFileCol(mcolumn), mrow);
                                    selectionFrame = currentFrame;
                                    selectionActive = true;
                                }
                                isDragging = true;
                                selectionEnd = new Point(cursorColToFileCol(getCursorColumnFromGlobalX(Input.MousePositionX)), getRowFromGlobalY(Input.MousePositionY));
                            }
                            if (selectionActive) // scroll when dragging off bounds
                            {
                                if (Input.MousePositionY > 180 + (Rendering.FrameRenderer.numOfVisibleRows - 1) * 7 && currentRow < thisFrame.GetLastRow())
                                    Move(0, 1);
                                if (Input.MousePositionY < 190 && currentRow > 0)
                                    Move(0, -1);
                            }
                        }
                    }

                    // click on a cell to move to it
                    if (Input.GetSingleClickUp(KeyModifier._Any)) {
                        if (!isDragging && mouseInBounds(mrow, mcolumn - channelScroll * 8)) {
                            if (!playback || !followMode)
                                cursorRow = mrow;
                            cursorColumn = mcolumn;
                        }
                        isDragging = false;
                    }
                }
                #endregion
            }
            // moving cursor with scroll
            if (!playback || !followMode) {
                if (Input.MousePositionY > 151 && Input.MousePositionX < 790 && Input.MousePositionY < Game1.bottomOfScreen - 15)
                    Move(0, Input.MouseScrollWheel(KeyModifier.None) * -Preferences.profile.pageJumpAmount);
                if (Input.GetKeyRepeat(Keys.PageUp, KeyModifier.None))
                    Move(0, -Preferences.profile.pageJumpAmount);
                if (Input.GetKeyRepeat(Keys.PageDown, KeyModifier.None))
                    Move(0, Preferences.profile.pageJumpAmount);
                if (Input.GetKeyDown(Keys.Escape, KeyModifier.None))
                    selectionActive = false;
                if (Input.GetKeyDown(Keys.Home, KeyModifier.None))
                    Goto(currentFrame, 0);
                if (Input.GetKeyDown(Keys.End, KeyModifier.None))
                    Goto(currentFrame, thisFrame.GetLastRow());
            }
            #region moving cursor with arrows
            if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None) || Input.GetKeyDown(Keys.Down, KeyModifier.Alt)) {
                selectionActive = false;
                if (!playback || !followMode)
                    Move(0, Preferences.profile.ignoreStepWhenMoving ? 1 : step);
            }

            if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None) || Input.GetKeyDown(Keys.Up, KeyModifier.Alt)) {
                selectionActive = false;
                if (!playback || !followMode)
                    Move(0, Preferences.profile.ignoreStepWhenMoving ? -1 : -step);
            }

            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.None)) {
                selectionActive = false;
                Move(1, 0);
                correctChanScroll();
            }
            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.None)) {
                selectionActive = false;
                Move(-1, 0);
                correctChanScroll();
            }
            if (Input.focusTimer == 0) {
                if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Alt) || Input.GetKeyRepeat(Keys.Tab, KeyModifier.None)) {
                    if (Input.GetKeyRepeat(Keys.Tab, KeyModifier.None))
                        cursorColumn = cursorColumn / 8 * 8;
                    Move(8, 0);
                    selectionActive = false;
                    correctChanScroll();
                }
                if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Alt) || Input.GetKeyRepeat(Keys.Tab, KeyModifier.Shift)) {
                    if (Input.GetKeyRepeat(Keys.Tab, KeyModifier.Shift))
                        cursorColumn = cursorColumn / 8 * 8;
                    Move(-8, 0);
                    selectionActive = false;
                    correctChanScroll();
                }
            }
            if (selectionActive && currentFrame != selectionFrame) {
                selectionActive = false;
            }

            #endregion
            #region muting and unmuting channels with function keys
            if (Input.GetKeyDown(Keys.F10, KeyModifier.Alt)) {
                SoloChannel(currentColumn / 5);
            }
            if (Input.GetKeyDown(Keys.F9, KeyModifier.Alt)) {
                ToggleChannel(currentColumn / 5);
            }
            #endregion
            if (!Game1.VisualizerMode) {
                #region selection with arrows
                if (!playback && Input.GetKeyRepeat(Keys.Down, KeyModifier.Shift)) {
                    if (!selectionActive) {
                        selectionStart = new Point(currentColumn, currentRow);
                        selectionFrame = currentFrame;
                        selectionActive = true;
                    }
                    Move(0, Preferences.profile.ignoreStepWhenMoving ? 1 : step);
                    selectionEnd = new Point(currentColumn, currentRow);
                }

                if (!playback && Input.GetKeyRepeat(Keys.Up, KeyModifier.Shift)) {
                    if (!selectionActive) {
                        selectionStart = new Point(currentColumn, currentRow);
                        selectionFrame = currentFrame;
                        selectionActive = true;
                    }
                    Move(0, Preferences.profile.ignoreStepWhenMoving ? -1 : -step);
                    selectionEnd = new Point(currentColumn, currentRow);
                }

                if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Shift)) {
                    if (!selectionActive) {
                        selectionStart = new Point(currentColumn, currentRow);
                        selectionFrame = currentFrame;
                        selectionActive = true;
                    }
                    Move(1, 0);
                    selectionEnd = new Point(currentColumn, currentRow);
                }
                if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Shift)) {
                    if (!selectionActive) {
                        selectionStart = new Point(currentColumn, currentRow);
                        selectionFrame = currentFrame;
                        selectionActive = true;
                    }
                    Move(-1, 0);
                    selectionEnd = new Point(currentColumn, currentRow);
                }
                #endregion

                #region Ctrl-A selection
                if (Input.GetKeyDown(Keys.A, KeyModifier.Ctrl)) {
                    if (selectionMax.X - selectionMin.X == 4 && selectionMin.Y == 0 && selectionMax.Y == thisFrame.GetLastRow()) {
                        selectionStart.X = 0;
                        selectionEnd.X = Song.CHANNEL_COUNT * 5 - 1;
                    } else {
                        int chan = currentColumn / 5;
                        selectionStart.X = chan * 5;
                        selectionEnd.X = chan * 5 + 4;
                        selectionStart.Y = 0;
                        selectionEnd.Y = thisFrame.GetLastRow();
                        selectionFrame = currentFrame;
                        selectionActive = true;
                    }
                }
                #endregion

                #region create selection bounds
                // create bounds of selection
                CreateSelectionBounds();
                if (selectionActive) {
                    if (!lastSelectionActive)
                        CopyToScaleClipboard();
                    lastSelectionActive = true;
                } else {
                    if (lastSelectionActive)
                        scaleclipboard.Clear();
                    lastSelectionActive = false;
                }
                #endregion
            }
            #region moving frames with ctrl-left and ctrl-right
            if (Input.GetKeyRepeat(Keys.Right, KeyModifier.Ctrl)) {
                selectionActive = false;
                if (playback && followMode)
                    Playback.NextFrame();
                else
                    NextFrame();
            }


            if (Input.GetKeyRepeat(Keys.Left, KeyModifier.Ctrl)) {
                selectionActive = false;
                if (playback && followMode)
                    Playback.PreviousFrame();
                else
                    PreviousFrame();
            }
            #endregion
            if (!Game1.VisualizerMode) {
                #region cell input
                {
                    int input;
                    switch (currentColumn % 5) {
                        case 0:
                            input = getPianoInput();
                            if (input != -1) {
                                SetCellValue(Math.Clamp(input, -3, 119), cursorColumn, step);
                                selectionActive = false;
                                AddToHistory();
                            }
                            break;
                        case 1:
                        case 2:
                            input = getDecimalInput();
                            if (input != -1) {
                                SetCellValue(input, cursorColumn, step);

                                if (currentColumn % 5 == 1 && thisFrame.pattern[currentRow - step][currentColumn] < thisSong.instruments.Count)
                                    Rendering.InstrumentBank.CurrentInstrumentIndex = thisFrame.pattern[currentRow - step][currentColumn];
                                selectionActive = false;
                                AddToHistory();
                            }
                            break;
                        case 3:
                            input = getEffectInput();
                            if (input != -1) {
                                SetCellValue(input, cursorColumn, Helpers.isEffectFrameTerminator(input) ? 0 : step);
                                selectionActive = false;
                                AddToHistory();
                            }
                            break;
                        case 4:
                            if (Helpers.isEffectHexadecimal(thisRow[currentColumn - 1]))
                                input = getHexInput();
                            else
                                input = getDecimalInput();
                            if (input != -1) {
                                SetCellValue(input, cursorColumn, step);
                                selectionActive = false;
                                AddToHistory();
                            }
                            break;
                    }
                }
                // cut/deleting
                if (Input.GetKeyDown(Keys.X, KeyModifier.Ctrl)) {
                    Cut();
                }
                if (Input.GetKeyRepeat(Keys.Delete, KeyModifier.None)) {
                    Delete();
                }
                #endregion

                #region incrementing values and transposition
                // shift up 1
                if (canEdit) {
                    if (Input.MouseScrollWheel(KeyModifier.Shift) == 1 || Input.GetKeyRepeat(Keys.F2, KeyModifier.Shift)) {
                        for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                            for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                                if (x % 5 != 0)
                                    IncrementCell(y, x, 1);
                            }
                        }
                        AddToHistory();
                    }
                    // shift down 1
                    if (Input.MouseScrollWheel(KeyModifier.Shift) == -1 || Input.GetKeyRepeat(Keys.F1, KeyModifier.Shift)) {
                        for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                            for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                                if (x % 5 != 0)
                                    IncrementCell(y, x, -1);
                            }
                        }
                        AddToHistory();
                    }


                    // note up 1
                    if (Input.MouseScrollWheel(KeyModifier.Ctrl) == 1 || Input.GetKeyRepeat(Keys.F2, KeyModifier.Ctrl)) {
                        for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                            for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                                if (x % 5 == 0)
                                    IncrementCell(y, x, 1);
                            }
                        }
                        AddToHistory();
                    }

                    // note down 1
                    if (Input.MouseScrollWheel(KeyModifier.Ctrl) == -1 || Input.GetKeyRepeat(Keys.F1, KeyModifier.Ctrl)) {
                        for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                            for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                                if (x % 5 == 0)
                                    IncrementCell(y, x, -1);
                            }
                        }
                        AddToHistory();
                    }

                    // note octave down
                    if (Input.GetKeyRepeat(Keys.F4, KeyModifier.Ctrl)) {
                        for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                            for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                                if (x % 5 == 0)
                                    IncrementCell(y, x, 12);
                            }
                        }
                        AddToHistory();
                    }

                    // note octave up
                    if (Input.GetKeyRepeat(Keys.F3, KeyModifier.Ctrl)) {
                        for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                            for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                                if (x % 5 == 0)
                                    IncrementCell(y, x, -12);
                            }
                        }
                        AddToHistory();
                    }

                    // volume scale up 1
                    if (Input.MouseScrollWheel(KeyModifier.ShiftAlt) == 1 || Input.GetKeyRepeat(Keys.F2, KeyModifier.ShiftAlt)) {
                        ScaleVolumes(1);
                        AddToHistory();
                    }

                    // volume scale down 1
                    if (Input.MouseScrollWheel(KeyModifier.ShiftAlt) == -1 || Input.GetKeyRepeat(Keys.F1, KeyModifier.ShiftAlt)) {
                        ScaleVolumes(-1);
                        AddToHistory();
                    }
                }
                #endregion

                #region interpolation
                if (Input.GetKey(Keys.G, KeyModifier.Ctrl) && canEdit) {
                    if (selectionActive) {
                        Interpolate(selectionMin.Y, selectionMax.Y, selectionMin.X);
                        AddToHistory();
                    }
                }
                #endregion

                #region inserting and backspace
                if (Input.GetKeyRepeat(Keys.Insert, KeyModifier.None) && canEdit) {
                    if (selectionActive) {
                        for (int i = selectionMin.X; i <= selectionMax.X; i++) {
                            InsertVal(selectionMin.Y, i, -1);
                        }
                    } else {
                        for (int i = currentColumn / 5 * 5; i < currentColumn / 5 * 5 + 5; ++i) {
                            InsertVal(currentRow, i, -1);
                        }
                    }
                    AddToHistory();
                }
                if (Input.GetKeyRepeat(Keys.Back, KeyModifier.None) && currentRow > 0 && canEdit) {
                    if (selectionActive) {
                        for (int j = selectionMax.Y; j >= selectionMin.Y; j--) {
                            for (int i = selectionMin.X; i <= selectionMax.X; i++) {
                                Backspace(j, i);
                            }
                        }
                        selectionActive = false;
                    } else {
                        Move(0, -1);
                        for (int i = currentColumn / 5 * 5; i < currentColumn / 5 * 5 + 5; ++i) {
                            Backspace(currentRow, i);
                        }
                    }
                    AddToHistory();
                }
                #endregion

                #region reversing
                if (Input.GetKeyRepeat(Keys.R, KeyModifier.Ctrl) && canEdit) {
                    if (selectionActive) {
                        Reverse();
                        AddToHistory();
                    }
                }
                #endregion

                #region alt+s instrument change 
                if (canEdit && selectionActive) {
                    if (Input.GetKeyDown(Keys.S, KeyModifier.Alt)) {
                        for (int i = selectionMin.X; i <= selectionMax.X; ++i) {
                            for (int j = selectionMin.Y; j <= selectionMax.Y; ++j) {
                                if (i % 5 == 1 && thisFrame.pattern[j][i] >= 0)
                                    thisFrame.pattern[j][i] = (short)Rendering.InstrumentBank.CurrentInstrumentIndex;
                            }
                        }
                    }
                }
                #endregion

                #region alt+r randomization
                if (canEdit && selectionActive) {
                    float range = 0.5f;
                    Random r = new Random();
                    if (Input.GetKeyDown(Keys.R, KeyModifier.Alt)) {
                        for (int i = selectionMin.X; i <= selectionMax.X; ++i) {
                            for (int j = selectionMin.Y; j <= selectionMax.Y; ++j) {
                                if (i % 5 == 2 && thisFrame.pattern[j][i] >= 0) {
                                    double val = thisFrame.pattern[j][i] * (1 + ((r.NextDouble() - 0.5f) * range));
                                    thisFrame.pattern[j][i] = (short)Math.Clamp(val, 0, 99);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region clipboard

                if (Input.GetKeyRepeat(Keys.C, KeyModifier.Ctrl)) {
                    CopyToClipboard();
                }
                if (Input.GetKeyRepeat(Keys.V, KeyModifier.Ctrl)) {
                    PasteFromClipboard();
                }
                if (Input.GetKeyRepeat(Keys.M, KeyModifier.Ctrl)) {
                    PasteAndMix();
                }

                #endregion

                #region undo/redo
                if (Input.GetKeyRepeat(Keys.Z, KeyModifier.Ctrl))
                    Undo();
                if (Input.GetKeyRepeat(Keys.Y, KeyModifier.Ctrl))
                    Redo();
                #endregion
            }

            if (channelScroll < 0)
                channelScroll = 0;
            if (channelScroll > Song.CHANNEL_COUNT - 12)
                channelScroll = Song.CHANNEL_COUNT - 12;
            channelScrollbar.scrollValue = channelScroll;
            channelScrollbar.Update();

            channelScroll = channelScrollbar.scrollValue;
        }

        public static void ClearHistory() {
            history.Clear();
            history.Add(FrameEditorState.Current());
            historyIndex = 0;
        }

        public static void AddToHistory() {
            if (history.Count == 0) {
                history.Add(FrameEditorState.Current());
                historyIndex = 0;
                return;
            }
            if (!canEdit)
                return;
            while (history.Count - 1 > historyIndex) {
                history.RemoveAt(history.Count - 1);
            }
            history.Add(FrameEditorState.Current());
            thisSong.frameEdits++;
            historyIndex++;
            if (history.Count > 64) {
                history.RemoveAt(0);
                historyIndex--;
            }
        }
        public static void Undo() {
            history[historyIndex].positionBefore.Load(true);
            historyIndex--;
            if (historyIndex < 0)
                historyIndex = 0;
            history[historyIndex].Load();
            thisSong.frameEdits--;
        }
        public static void Redo() {
            historyIndex++;
            if (historyIndex >= history.Count)
                historyIndex = history.Count - 1;
            history[historyIndex].Load();
            //history[historyIndex].positionBefore.Load(true);
            history[historyIndex].positionAfter.Load(true);
            thisSong.frameEdits++;
        }

        /// <summary>
        /// Delete selection
        /// </summary>
        public static void Delete() {
            if (canEdit) {
                for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                    for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                        thisFrame.pattern[y][x] = -1;

                        if (x % 5 == 0) thisFrame.pattern[y][x + 1] = -1;
                        if (x % 5 == 4) thisFrame.pattern[y][x - 1] = -1;
                        if (x % 5 == 3) thisFrame.pattern[y][x + 1] = -1;
                        if (!selectionActive && !playback)
                            Move(0, 1);
                    }
                }
                AddToHistory();
            }
        }

        /// <summary>
        /// Copy selection to clipboard then delete
        /// </summary>
        public static void Cut() {
            if (selectionActive) {
                CopyToClipboard();
                Delete();
            }
        }
        /// <summary>
        /// Copy selection to the clipboard
        /// </summary>
        public static void CopyToClipboard() {
            if (selectionActive) {
                clipboard.Clear();

                // clipboard is the same size as the selection
                clipboardStartCol = selectionMin.X;

                for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                    clipboard.Add(new List<short>());
                    for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                        int cy = y - selectionMin.Y;
                        int cx = x - selectionMin.X;
                        clipboard[cx].Add(thisFrame.pattern[y][x]);
                        cy++;
                    }

                }
            }
        }

        /// <summary>
        /// When about to scale, copy the original values to the scale clipboard so as little fidelity is lost when scaling down then back up.
        /// </summary>
        public static void CopyToScaleClipboard() {
            if (selectionActive) {
                scaleclipboard.Clear();

                foreach (short[] row in thisFrame.pattern) {
                    scaleclipboard.Add(new List<float>());
                    foreach (short val in row) {
                        scaleclipboard[scaleclipboard.Count - 1].Add(val);
                    }
                }
            }
        }

        public static void PasteFromClipboard() {
            if (!canEdit)
                return;
            if (clipboard.Count > 0) {
                int startRow = (currentColumn / 5 * 5) + clipboardStartCol % 5;
                int startCol = currentRow;
                for (int y = 0; y < clipboard.Count; y++) {
                    if (startRow + y < Song.CHANNEL_COUNT * 5) {
                        for (int x = 0; x < clipboard[y].Count; x++) {
                            if (startCol + x < thisSong.rowsPerFrame) {
                                thisFrame.pattern[startCol + x][startRow + y] = clipboard[y][x];
                            }
                        }
                    }
                }
                selectionStart = new Point(startRow, startCol);
                selectionEnd = new Point(startRow + clipboard.Count - 1, startCol + clipboard[0].Count - 1);
                selectionMin = new Point(startRow, startCol);
                selectionMax = new Point(startRow + clipboard.Count - 1, startCol + clipboard[0].Count - 1);
                selectionFrame = currentFrame;
                selectionActive = true;
                CreateSelectionBounds();
                AddToHistory();
                //history[historyIndex].positionAfter.SetSelection(true, selectionStart, selectionEnd);
            }
        }
        /// <summary>
        /// Paste contents of clipboards only in empty spaces
        /// </summary>
        public static void PasteAndMix() {
            if (!canEdit)
                return;
            if (clipboard.Count > 0) {
                int startRow = (currentColumn / 5 * 5) + clipboardStartCol % 5;
                int startCol = currentRow;
                for (int y = 0; y < clipboard.Count; y++) {
                    if (startRow + y < Song.CHANNEL_COUNT * 5) {
                        for (int x = 0; x < clipboard[y].Count; x++) {
                            if (startCol + x < thisSong.rowsPerFrame) {
                                int row = startCol + x;
                                int col = startRow + y;
                                if (thisFrame.pattern[row][col] == -1)
                                    thisFrame.pattern[row][col] = clipboard[y][x];
                            }
                        }
                    }
                }
                selectionStart = new Point(startRow, startCol);
                selectionEnd = new Point(startRow + clipboard.Count - 1, startCol + clipboard[0].Count - 1);
                selectionMin = new Point(startRow, startCol);
                selectionMax = new Point(startRow + clipboard.Count - 1, startCol + clipboard[0].Count - 1);
                selectionActive = true;
                CreateSelectionBounds();
                AddToHistory();
            }
        }
        /// <summary>
        /// Converts selection start and end to selection min and max.
        /// </summary>
        public static void CreateSelectionBounds() {
            if (selectionActive) {

                selectionStart.X = Math.Clamp(selectionStart.X, 0, Song.CHANNEL_COUNT * 5 - 1);
                selectionStart.Y = Math.Clamp(selectionStart.Y, 0, thisFrame.GetLastRow());
                selectionEnd.X = Math.Clamp(selectionEnd.X, 0, Song.CHANNEL_COUNT * 5 - 1);
                selectionEnd.Y = Math.Clamp(selectionEnd.Y, 0, thisFrame.GetLastRow());
                selectionMin.X = Math.Min(selectionStart.X, selectionEnd.X);
                selectionMin.Y = Math.Min(selectionStart.Y, selectionEnd.Y);
                selectionMax.X = Math.Max(selectionStart.X, selectionEnd.X);
                selectionMax.Y = Math.Max(selectionStart.Y, selectionEnd.Y);
                if (selectionMax.X % 5 == 3)
                    selectionMax.X += 1;
                if (selectionMin.X % 5 == 4)
                    selectionMin.X -= 1;

            } else {
                selectionMin.Y = currentRow;
                selectionMin.X = currentColumn;
                selectionMax.Y = currentRow;
                selectionMax.X = currentColumn;
            }
        }
        /// <summary>
        /// Increments the value in the cell by amt
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="amt"></param>
        public static void IncrementCell(int row, int col, int amt) {
            int currentCellValue = thisFrame.pattern[row][col];
            if (currentCellValue < 0) return;
            if (col % 5 != 3) {
                thisFrame.pattern[row][col] += (short)amt;

                // clamp values
                if (col % 5 == 4 && Helpers.isEffectHexadecimal(thisFrame.pattern[row][col / 5 * 5 + 3]))
                    thisFrame.pattern[row][col] = (short)Math.Clamp(currentCellValue + amt, 0, 255); // hex values
                else if (col % 5 == 0)
                    thisFrame.pattern[row][col] = (short)Math.Clamp(currentCellValue + amt, 0, 119); // note values
                else
                    thisFrame.pattern[row][col] = (short)Math.Clamp(currentCellValue + amt, 0, 99); // regular values
            }
        }

        /// <summary>
        /// Insert 'val' at (row, column)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="val"></param>
        public static void InsertVal(int row, int column, int val) {
            for (int i = thisSong.rowsPerFrame - 1; i > row; i--) {
                thisFrame.pattern[i][column] = thisFrame.pattern[i - 1][column];
            }
            thisFrame.pattern[row][column] = (short)val;
        }

        /// <summary>
        /// Delete value at (row, column) and pull the entries in the rest of the column up 1
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public static void Backspace(int row, int column) {
            for (int i = row; i < thisSong.rowsPerFrame - 1; i++) {
                thisFrame.pattern[i][column] = thisFrame.pattern[i + 1][column];
            }
            thisFrame.pattern[thisSong.rowsPerFrame - 1][column] = -1;
        }

        /// <summary>
        /// Reverse current selection
        /// </summary>
        static void Reverse() {
            if (canEdit) {
                int row = 0;
                for (int y = selectionMin.Y; y <= selectionMax.Y; y += 2) {
                    for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                        short one = thisFrame.pattern[selectionMin.Y + row][x];
                        short two = thisFrame.pattern[selectionMax.Y - row][x];
                        thisFrame.pattern[selectionMin.Y + row][x] = two;
                        thisFrame.pattern[selectionMax.Y - row][x] = one;
                    }
                    row++;
                }
            }
        }

        /// <summary>
        /// Interpolates values from startRow to endRow in column, col
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="col"></param>
        static void Interpolate(int startRow, int endRow, int col) {
            if (canEdit) {
                // if column is effect move to effect parameter
                if (col % 5 == 3)
                    col++;

                int startVal = thisFrame.pattern[startRow][col];
                int endVal = thisFrame.pattern[endRow][col];

                // if either start or end is empty, then break out
                if (startVal < 0 || endVal < 0 || col % 5 == 3) return;

                // lerp from start to end filling in the rows in between
                for (int i = startRow; i <= endRow; i++) {
                    float percentage = (i - startRow) / (float)(endRow - startRow);
                    thisFrame.pattern[i][col] = (short)Math.Round(MathHelper.Lerp(startVal, endVal, percentage));
                    if (col % 5 == 4) {
                        thisFrame.pattern[i][col - 1] = thisFrame.pattern[startRow][col - 1];
                    }
                }
            }
        }

        static void ScaleVolumes(int direction) {
            if (canEdit) {
                int cx = 0;
                for (int x = selectionMin.X; x <= selectionMax.X; x++) {
                    if (x % 5 == 2) {
                        // gets max value in selection
                        float max = 0;
                        for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                            if (scaleclipboard[y][x] > max)
                                max = scaleclipboard[y][x];
                        }

                        // how much to multiply every value in the selection
                        float factor = (max + direction) / (float)max;

                        // multiply every value in the selection to scale the values
                        for (int y = selectionMin.Y; y <= selectionMax.Y; y++) {
                            if (thisFrame.pattern[y][x] >= 0) {
                                scaleclipboard[y][x] *= factor;
                                int val = (short)Math.Round(scaleclipboard[y][x]);
                                thisFrame.pattern[y][x] = (short)Math.Clamp(val, 0, 99);
                            }
                        }
                        cx++;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value at the 'cursorCol' of the current row to 'val' and steps down 'stepAmt' rows
        /// </summary>
        /// <param name="val"></param>
        /// <param name="cursorCol"></param>
        /// <param name="stepAmt"></param>
        static void SetCellValue(int val, int cursorCol, int stepAmt) {
            if (canEdit) {
                short[] previousRow = thisRow;
                int previousCellValue = thisRow[currentColumn];
                int channelIDX = currentColumn / 5 * 5;
                int chanCol = cursorCol % 8;
                if (chanCol == 0) // note
                {
                    thisFrame.pattern[currentRow][channelIDX + 0] = (short)val;
                    if (!instrumentMask)
                        thisFrame.pattern[currentRow][channelIDX + 1] = (short)Rendering.InstrumentBank.CurrentInstrumentIndex;
                    if (val < 0)
                        thisFrame.pattern[currentRow][channelIDX + 1] = -1;
                }

                if (chanCol == 1) // instrument tens
                {

                    if (previousCellValue == -1)
                        thisFrame.pattern[currentRow][channelIDX + 1] = (short)(val * 10);
                    else
                        thisFrame.pattern[currentRow][channelIDX + 1] = (short)(val * 10 + previousCellValue % 10);
                }
                if (chanCol == 2) // instrument ones
                {

                    if (previousCellValue == -1)
                        thisFrame.pattern[currentRow][channelIDX + 1] = (short)(val);
                    else
                        thisFrame.pattern[currentRow][channelIDX + 1] = (short)(val + previousCellValue / 10 * 10);
                }

                if (chanCol == 3) // volume tens
                {

                    if (previousCellValue == -1)
                        thisFrame.pattern[currentRow][channelIDX + 2] = (short)(val * 10);
                    else
                        thisFrame.pattern[currentRow][channelIDX + 2] = (short)(val * 10 + previousCellValue % 10);
                }
                if (chanCol == 4) // volume ones
                {

                    if (previousCellValue == -1)
                        thisFrame.pattern[currentRow][channelIDX + 2] = (short)(val);
                    else
                        thisFrame.pattern[currentRow][channelIDX + 2] = (short)(val + previousCellValue / 10 * 10);
                }

                if (chanCol == 5) // effect
                {
                    thisFrame.pattern[currentRow][channelIDX + 3] = (short)val;
                    if (thisFrame.pattern[currentRow][channelIDX + 4] == -1) {
                        thisFrame.pattern[currentRow][channelIDX + 4] = 0;
                    }
                    if (!Helpers.isEffectHexadecimal(val) && thisFrame.pattern[currentRow][channelIDX + 4] > 99) {
                        thisFrame.pattern[currentRow][channelIDX + 4] = 99;
                    }
                }
                if (thisFrame.pattern[currentRow][channelIDX + 3] != -1) {
                    int baseNum = Helpers.isEffectHexadecimal(thisFrame.pattern[currentRow][channelIDX + 3]) ? 16 : 10;
                    if (chanCol == 6) // effect parameter tens
                    {

                        if (previousCellValue == -1)
                            thisFrame.pattern[currentRow][channelIDX + 4] = (short)(val * baseNum);
                        else
                            thisFrame.pattern[currentRow][channelIDX + 4] = (short)(val * baseNum + previousCellValue % baseNum);
                    }
                    if (chanCol == 7) // effect parameter ones
                    {
                        if (previousCellValue == -1)
                            thisFrame.pattern[currentRow][channelIDX + 4] = (short)val;
                        else
                            thisFrame.pattern[currentRow][channelIDX + 4] = (short)(val + previousCellValue / baseNum * baseNum);
                    }
                }
                if (!playback)
                    Move(0, stepAmt);
            }
        }
        /// <summary>
        /// Gets keyboard input into hex values (0123456789ABCDEF)
        /// </summary>
        /// <returns>
        /// </returns>
        static int getHexInput() {
            if (Preferences.profile.keyRepeat) {
                if (Input.GetKeyRepeat(Keys.D0, KeyModifier.None))
                    return 0;
                if (Input.GetKeyRepeat(Keys.D1, KeyModifier.None))
                    return 1;
                if (Input.GetKeyRepeat(Keys.D2, KeyModifier.None))
                    return 2;
                if (Input.GetKeyRepeat(Keys.D3, KeyModifier.None))
                    return 3;
                if (Input.GetKeyRepeat(Keys.D4, KeyModifier.None))
                    return 4;
                if (Input.GetKeyRepeat(Keys.D5, KeyModifier.None))
                    return 5;
                if (Input.GetKeyRepeat(Keys.D6, KeyModifier.None))
                    return 6;
                if (Input.GetKeyRepeat(Keys.D7, KeyModifier.None))
                    return 7;
                if (Input.GetKeyRepeat(Keys.D8, KeyModifier.None))
                    return 8;
                if (Input.GetKeyRepeat(Keys.D9, KeyModifier.None))
                    return 9;
                if (Input.GetKeyRepeat(Keys.A, KeyModifier.None))
                    return 10;
                if (Input.GetKeyRepeat(Keys.B, KeyModifier.None))
                    return 11;
                if (Input.GetKeyRepeat(Keys.C, KeyModifier.None))
                    return 12;
                if (Input.GetKeyRepeat(Keys.D, KeyModifier.None))
                    return 13;
                if (Input.GetKeyRepeat(Keys.E, KeyModifier.None))
                    return 14;
                if (Input.GetKeyRepeat(Keys.F, KeyModifier.None))
                    return 15;
            } else {
                if (Input.GetKeyDown(Keys.D0, KeyModifier.None))
                    return 0;
                if (Input.GetKeyDown(Keys.D1, KeyModifier.None))
                    return 1;
                if (Input.GetKeyDown(Keys.D2, KeyModifier.None))
                    return 2;
                if (Input.GetKeyDown(Keys.D3, KeyModifier.None))
                    return 3;
                if (Input.GetKeyDown(Keys.D4, KeyModifier.None))
                    return 4;
                if (Input.GetKeyDown(Keys.D5, KeyModifier.None))
                    return 5;
                if (Input.GetKeyDown(Keys.D6, KeyModifier.None))
                    return 6;
                if (Input.GetKeyDown(Keys.D7, KeyModifier.None))
                    return 7;
                if (Input.GetKeyDown(Keys.D8, KeyModifier.None))
                    return 8;
                if (Input.GetKeyDown(Keys.D9, KeyModifier.None))
                    return 9;
                if (Input.GetKeyDown(Keys.A, KeyModifier.None))
                    return 10;
                if (Input.GetKeyDown(Keys.B, KeyModifier.None))
                    return 11;
                if (Input.GetKeyDown(Keys.C, KeyModifier.None))
                    return 12;
                if (Input.GetKeyDown(Keys.D, KeyModifier.None))
                    return 13;
                if (Input.GetKeyDown(Keys.E, KeyModifier.None))
                    return 14;
                if (Input.GetKeyDown(Keys.F, KeyModifier.None))
                    return 15;
            }
            return -1;
        }
        /// <summary>
        /// Gets keyboard number input into integer values
        /// </summary>
        /// <returns>
        /// </returns>
        static int getDecimalInput() {
            if (Preferences.profile.keyRepeat) {
                if (Input.GetKeyRepeat(Keys.D0, KeyModifier.None)) return 0;
                if (Input.GetKeyRepeat(Keys.D1, KeyModifier.None)) return 1;
                if (Input.GetKeyRepeat(Keys.D2, KeyModifier.None)) return 2;
                if (Input.GetKeyRepeat(Keys.D3, KeyModifier.None)) return 3;
                if (Input.GetKeyRepeat(Keys.D4, KeyModifier.None)) return 4;
                if (Input.GetKeyRepeat(Keys.D5, KeyModifier.None)) return 5;
                if (Input.GetKeyRepeat(Keys.D6, KeyModifier.None)) return 6;
                if (Input.GetKeyRepeat(Keys.D7, KeyModifier.None)) return 7;
                if (Input.GetKeyRepeat(Keys.D8, KeyModifier.None)) return 8;
                if (Input.GetKeyRepeat(Keys.D9, KeyModifier.None)) return 9;
            } else {
                if (Input.GetKeyDown(Keys.D0, KeyModifier.None)) return 0;
                if (Input.GetKeyDown(Keys.D1, KeyModifier.None)) return 1;
                if (Input.GetKeyDown(Keys.D2, KeyModifier.None)) return 2;
                if (Input.GetKeyDown(Keys.D3, KeyModifier.None)) return 3;
                if (Input.GetKeyDown(Keys.D4, KeyModifier.None)) return 4;
                if (Input.GetKeyDown(Keys.D5, KeyModifier.None)) return 5;
                if (Input.GetKeyDown(Keys.D6, KeyModifier.None)) return 6;
                if (Input.GetKeyDown(Keys.D7, KeyModifier.None)) return 7;
                if (Input.GetKeyDown(Keys.D8, KeyModifier.None)) return 8;
                if (Input.GetKeyDown(Keys.D9, KeyModifier.None)) return 9;
            }
            return -1;
        }
        /// <summary>
        /// Gets keyboard input into effect values
        /// </summary>
        /// <returns>Effect ID of the current key pressed <br></br>
        /// Returns -1 if none.
        /// </returns>
        static int getEffectInput() {
            if (Preferences.profile.keyRepeat) {
                if (Input.GetKeyRepeat(Keys.D0, KeyModifier.None)) return 0;
                if (Input.GetKeyRepeat(Keys.D1, KeyModifier.None)) return 1;
                if (Input.GetKeyRepeat(Keys.D2, KeyModifier.None)) return 2;
                if (Input.GetKeyRepeat(Keys.D3, KeyModifier.None)) return 3;
                if (Input.GetKeyRepeat(Keys.D4, KeyModifier.None)) return 4;
                if (Input.GetKeyRepeat(Keys.D7, KeyModifier.None)) return 7;
                if (Input.GetKeyRepeat(Keys.D8, KeyModifier.None)) return 8;
                if (Input.GetKeyRepeat(Keys.D9, KeyModifier.None)) return 9;
                if (Input.GetKeyRepeat(Keys.Q, KeyModifier.None)) return 10;
                if (Input.GetKeyRepeat(Keys.R, KeyModifier.None)) return 11;
                if (Input.GetKeyRepeat(Keys.P, KeyModifier.None)) return 14;
                if (Input.GetKeyRepeat(Keys.F, KeyModifier.None)) return 15;
                if (Input.GetKeyRepeat(Keys.V, KeyModifier.None)) return 16;
                if (Input.GetKeyRepeat(Keys.C, KeyModifier.None)) return 20;
                if (Input.GetKeyRepeat(Keys.B, KeyModifier.None)) return 21;
                if (Input.GetKeyRepeat(Keys.D, KeyModifier.None)) return 22;
                if (Input.GetKeyRepeat(Keys.G, KeyModifier.None)) return 17;
                if (Input.GetKeyRepeat(Keys.S, KeyModifier.None)) return 18;
                if (Input.GetKeyRepeat(Keys.A, KeyModifier.None)) return 12;
                if (Input.GetKeyRepeat(Keys.W, KeyModifier.None)) return 13;
                if (Input.GetKeyRepeat(Keys.M, KeyModifier.None)) return 23;
                if (Input.GetKeyRepeat(Keys.I, KeyModifier.None)) return 19;
                if (Input.GetKeyRepeat(Keys.J, KeyModifier.None)) return 24;
                if (Input.GetKeyRepeat(Keys.L, KeyModifier.None)) return 25;
            } else {
                if (Input.GetKeyDown(Keys.D0, KeyModifier.None)) return 0;
                if (Input.GetKeyDown(Keys.D1, KeyModifier.None)) return 1;
                if (Input.GetKeyDown(Keys.D2, KeyModifier.None)) return 2;
                if (Input.GetKeyDown(Keys.D3, KeyModifier.None)) return 3;
                if (Input.GetKeyDown(Keys.D4, KeyModifier.None)) return 4;
                if (Input.GetKeyDown(Keys.D7, KeyModifier.None)) return 7;
                if (Input.GetKeyDown(Keys.D8, KeyModifier.None)) return 8;
                if (Input.GetKeyDown(Keys.D9, KeyModifier.None)) return 9;
                if (Input.GetKeyDown(Keys.Q, KeyModifier.None)) return 10;
                if (Input.GetKeyDown(Keys.R, KeyModifier.None)) return 11;
                if (Input.GetKeyDown(Keys.P, KeyModifier.None)) return 14;
                if (Input.GetKeyDown(Keys.F, KeyModifier.None)) return 15;
                if (Input.GetKeyDown(Keys.V, KeyModifier.None)) return 16;
                if (Input.GetKeyDown(Keys.C, KeyModifier.None)) return 20;
                if (Input.GetKeyDown(Keys.B, KeyModifier.None)) return 21;
                if (Input.GetKeyDown(Keys.D, KeyModifier.None)) return 22;
                if (Input.GetKeyDown(Keys.G, KeyModifier.None)) return 17;
                if (Input.GetKeyDown(Keys.S, KeyModifier.None)) return 18;
                if (Input.GetKeyDown(Keys.A, KeyModifier.None)) return 12;
                if (Input.GetKeyDown(Keys.W, KeyModifier.None)) return 13;
                if (Input.GetKeyDown(Keys.M, KeyModifier.None)) return 23;
                if (Input.GetKeyDown(Keys.I, KeyModifier.None)) return 19;
                if (Input.GetKeyDown(Keys.J, KeyModifier.None)) return 24;
                if (Input.GetKeyDown(Keys.L, KeyModifier.None)) return 25;
            }

            return -1;
        }

        /// <summary>
        /// Gets keyboard input into MIDI note-value
        /// </summary>
        /// <returns>MIDI note number of the key pressed on the keyboard (taking into account current octave selection). <br></br>
        /// Returns -1 if none.
        /// </returns>
        static int getPianoInput() {
            if (Preferences.profile.keyRepeat) {
                if (Input.GetKeyRepeat(Keys.OemPlus, KeyModifier.None)) return -3;
                if (Input.GetKeyRepeat(Keys.D1, KeyModifier.None)) return -2;

                if (Input.GetKeyRepeat(Keys.Z, KeyModifier.None)) return currentOctave * 12 + 0;
                if (Input.GetKeyRepeat(Keys.S, KeyModifier.None)) return currentOctave * 12 + 1;
                if (Input.GetKeyRepeat(Keys.X, KeyModifier.None)) return currentOctave * 12 + 2;
                if (Input.GetKeyRepeat(Keys.D, KeyModifier.None)) return currentOctave * 12 + 3;
                if (Input.GetKeyRepeat(Keys.C, KeyModifier.None)) return currentOctave * 12 + 4;
                if (Input.GetKeyRepeat(Keys.V, KeyModifier.None)) return currentOctave * 12 + 5;
                if (Input.GetKeyRepeat(Keys.G, KeyModifier.None)) return currentOctave * 12 + 6;
                if (Input.GetKeyRepeat(Keys.B, KeyModifier.None)) return currentOctave * 12 + 7;
                if (Input.GetKeyRepeat(Keys.H, KeyModifier.None)) return currentOctave * 12 + 8;
                if (Input.GetKeyRepeat(Keys.N, KeyModifier.None)) return currentOctave * 12 + 9;
                if (Input.GetKeyRepeat(Keys.J, KeyModifier.None)) return currentOctave * 12 + 10;
                if (Input.GetKeyRepeat(Keys.M, KeyModifier.None)) return currentOctave * 12 + 11;
                if (Input.GetKeyRepeat(Keys.OemComma, KeyModifier.None)) return currentOctave * 12 + 12;
                if (Input.GetKeyRepeat(Keys.L, KeyModifier.None)) return currentOctave * 12 + 13;
                if (Input.GetKeyRepeat(Keys.OemPeriod, KeyModifier.None)) return currentOctave * 12 + 14;
                if (Input.GetKeyRepeat(Keys.OemSemicolon, KeyModifier.None)) return currentOctave * 12 + 15;
                if (Input.GetKeyRepeat(Keys.OemQuestion, KeyModifier.None)) return currentOctave * 12 + 16;
                if (Input.GetKeyRepeat(Keys.Q, KeyModifier.None)) return currentOctave * 12 + 12;
                if (Input.GetKeyRepeat(Keys.D2, KeyModifier.None)) return currentOctave * 12 + 13;
                if (Input.GetKeyRepeat(Keys.W, KeyModifier.None)) return currentOctave * 12 + 14;
                if (Input.GetKeyRepeat(Keys.D3, KeyModifier.None)) return currentOctave * 12 + 15;
                if (Input.GetKeyRepeat(Keys.E, KeyModifier.None)) return currentOctave * 12 + 16;
                if (Input.GetKeyRepeat(Keys.R, KeyModifier.None)) return currentOctave * 12 + 17;
                if (Input.GetKeyRepeat(Keys.D5, KeyModifier.None)) return currentOctave * 12 + 18;
                if (Input.GetKeyRepeat(Keys.T, KeyModifier.None)) return currentOctave * 12 + 19;
                if (Input.GetKeyRepeat(Keys.D6, KeyModifier.None)) return currentOctave * 12 + 20;
                if (Input.GetKeyRepeat(Keys.Y, KeyModifier.None)) return currentOctave * 12 + 21;
                if (Input.GetKeyRepeat(Keys.D7, KeyModifier.None)) return currentOctave * 12 + 22;
                if (Input.GetKeyRepeat(Keys.U, KeyModifier.None)) return currentOctave * 12 + 23;
                if (Input.GetKeyRepeat(Keys.I, KeyModifier.None)) return currentOctave * 12 + 24;
                if (Input.GetKeyRepeat(Keys.D9, KeyModifier.None)) return currentOctave * 12 + 25;
                if (Input.GetKeyRepeat(Keys.O, KeyModifier.None)) return currentOctave * 12 + 26;
                if (Input.GetKeyRepeat(Keys.D0, KeyModifier.None)) return currentOctave * 12 + 27;
                if (Input.GetKeyRepeat(Keys.P, KeyModifier.None)) return currentOctave * 12 + 28;
            } else {
                if (Input.GetKeyDown(Keys.OemPlus, KeyModifier.None)) return -3;
                if (Input.GetKeyDown(Keys.D1, KeyModifier.None)) return -2;

                if (Input.GetKeyDown(Keys.Z, KeyModifier.None)) return currentOctave * 12 + 0;
                if (Input.GetKeyDown(Keys.S, KeyModifier.None)) return currentOctave * 12 + 1;
                if (Input.GetKeyDown(Keys.X, KeyModifier.None)) return currentOctave * 12 + 2;
                if (Input.GetKeyDown(Keys.D, KeyModifier.None)) return currentOctave * 12 + 3;
                if (Input.GetKeyDown(Keys.C, KeyModifier.None)) return currentOctave * 12 + 4;
                if (Input.GetKeyDown(Keys.V, KeyModifier.None)) return currentOctave * 12 + 5;
                if (Input.GetKeyDown(Keys.G, KeyModifier.None)) return currentOctave * 12 + 6;
                if (Input.GetKeyDown(Keys.B, KeyModifier.None)) return currentOctave * 12 + 7;
                if (Input.GetKeyDown(Keys.H, KeyModifier.None)) return currentOctave * 12 + 8;
                if (Input.GetKeyDown(Keys.N, KeyModifier.None)) return currentOctave * 12 + 9;
                if (Input.GetKeyDown(Keys.J, KeyModifier.None)) return currentOctave * 12 + 10;
                if (Input.GetKeyDown(Keys.M, KeyModifier.None)) return currentOctave * 12 + 11;
                if (Input.GetKeyDown(Keys.OemComma, KeyModifier.None)) return currentOctave * 12 + 12;
                if (Input.GetKeyDown(Keys.L, KeyModifier.None)) return currentOctave * 12 + 13;
                if (Input.GetKeyDown(Keys.OemPeriod, KeyModifier.None)) return currentOctave * 12 + 14;
                if (Input.GetKeyDown(Keys.OemSemicolon, KeyModifier.None)) return currentOctave * 12 + 15;
                if (Input.GetKeyDown(Keys.OemQuestion, KeyModifier.None)) return currentOctave * 12 + 16;
                if (Input.GetKeyDown(Keys.Q, KeyModifier.None)) return currentOctave * 12 + 12;
                if (Input.GetKeyDown(Keys.D2, KeyModifier.None)) return currentOctave * 12 + 13;
                if (Input.GetKeyDown(Keys.W, KeyModifier.None)) return currentOctave * 12 + 14;
                if (Input.GetKeyDown(Keys.D3, KeyModifier.None)) return currentOctave * 12 + 15;
                if (Input.GetKeyDown(Keys.E, KeyModifier.None)) return currentOctave * 12 + 16;
                if (Input.GetKeyDown(Keys.R, KeyModifier.None)) return currentOctave * 12 + 17;
                if (Input.GetKeyDown(Keys.D5, KeyModifier.None)) return currentOctave * 12 + 18;
                if (Input.GetKeyDown(Keys.T, KeyModifier.None)) return currentOctave * 12 + 19;
                if (Input.GetKeyDown(Keys.D6, KeyModifier.None)) return currentOctave * 12 + 20;
                if (Input.GetKeyDown(Keys.Y, KeyModifier.None)) return currentOctave * 12 + 21;
                if (Input.GetKeyDown(Keys.D7, KeyModifier.None)) return currentOctave * 12 + 22;
                if (Input.GetKeyDown(Keys.U, KeyModifier.None)) return currentOctave * 12 + 23;
                if (Input.GetKeyDown(Keys.I, KeyModifier.None)) return currentOctave * 12 + 24;
                if (Input.GetKeyDown(Keys.D9, KeyModifier.None)) return currentOctave * 12 + 25;
                if (Input.GetKeyDown(Keys.O, KeyModifier.None)) return currentOctave * 12 + 26;
                if (Input.GetKeyDown(Keys.D0, KeyModifier.None)) return currentOctave * 12 + 27;
                if (Input.GetKeyDown(Keys.P, KeyModifier.None)) return currentOctave * 12 + 28;
            }
            return -1;
        }
        /// <summary>
        /// Swaps instances of inst1 and inst2 with each other across the whole song.
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="inst2"></param>
        public static void SwapInstrumentsInSong(int inst, int inst2) {
            foreach (Frame f in Song.currentSong.frames) {
                for (int row = 0; row < f.pattern.Length; row++) {
                    for (int col = 0; col < f.pattern[row].Length; col++) {
                        if (col % 5 == 1) {
                            // ex: swapping 00 with 01
                            // all instruments that are 01 are set to 255
                            // all instruments that are 00 are set to 01
                            if (f.pattern[row][col] == inst2) {
                                f.pattern[row][col] = 255;
                            }
                            if (f.pattern[row][col] == inst) {
                                f.pattern[row][col] = (short)inst2;
                            }
                        }
                    }
                }
                for (int row = 0; row < f.pattern.Length; row++) {
                    for (int col = 0; col < f.pattern[row].Length; col++) {
                        if (col % 5 == 1) {
                            // ex: swapping 00 with 01
                            // all instruments that are 255 are set to 00
                            if (f.pattern[row][col] == 255) {
                                f.pattern[row][col] = (short)inst;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If an instrument is deleted, this will shift the values of all instances of instruments in the song, so that they match the new order.
        /// Any instance where the deleted instrument was used will be deleted as well.
        /// </summary>
        /// <param name="indexOfDeletedInstrument"></param>
        public static void AdjustForDeletedInstrument(int indexOfDeletedInstrument) {
            foreach (Frame f in Song.currentSong.frames) {
                for (int row = 0; row < f.pattern.Length; row++) {
                    for (int col = 0; col < f.pattern[row].Length; col++) {
                        if (col % 5 == 1) {
                            if (f.pattern[row][col] > indexOfDeletedInstrument) {
                                f.pattern[row][col]--;
                            } else if (f.pattern[row][col] == indexOfDeletedInstrument) {
                                f.pattern[row][col] = -1;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Determines if the current mouse row/column is in bounds
        /// </summary>
        /// <param name="mrow"></param>
        /// <param name="mcolumn"></param>
        /// <returns></returns>
        static bool mouseInBounds(int mrow, int mcolumn) {
            if (Input.MousePositionY > Game1.bottomOfScreen - 15)
                return false;
            if (channelScrollbar.barisPressed || channelScrollbar.barWasPressed > 0)
                return false;
            return mcolumn >= 0 && (mrow - cursorRow + Rendering.FrameRenderer.numOfVisibleRows - Rendering.FrameRenderer.centerRow) > 0 && mcolumn < 96 && (mrow - cursorRow + Rendering.FrameRenderer.centerRow) < 55 && mrow >= 0 && mrow <= thisFrame.GetLastRow();
        }
        /// <summary>
        /// Returns the top-most coordinate of the row 'row'. Used for rendering purposes
        /// </summary>
        public static int getStartPosOfRow(int row) {
            return (row + Rendering.FrameRenderer.centerRow - currentRow) * 7 + 184;
        }
        /// <summary>
        /// Returns the bottom-most coordinate of the row 'row'. Used for rendering purposes
        /// </summary>
        public static int getEndPosOfRow(int row) {
            return (row + Rendering.FrameRenderer.centerRow - currentRow) * 7 + 184 + 7;
        }


        /// <summary>
        /// Returns the left-most coordinate of the column 'col'
        /// . Used for rendering purposes
        /// <br></br>
        /// Note: 'col' is a file column(5 per channel) not a cursor column(8 per channel) 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static int getStartPosOfFileColumn(int col) {
            int channel = col / 5;
            int internalColumn = col % 5;
            int ret = 0;
            switch (internalColumn) {
                case 0:
                    ret = 0;
                    break;
                case 1:
                    ret = 19;
                    break;
                case 2:
                    ret = 32;
                    break;
                case 3:
                    ret = 45;
                    break;
                case 4:
                    ret = 51;
                    break;
            }
            return ret + channel * 64 - channelScroll * 64;
        }

        /// <summary>
        /// Returns the right-most coordinate of the column 'col'. Used for rendering purposes
        /// <br></br>
        /// Note: 'col' is a file column(5 per channel) not a cursor column(8 per channel)
        /// </summary>
        public static int getEndPosOfFileColumn(int col) {
            int channel = col / 5;
            int internalColumn = col % 5;
            int ret = 0;
            switch (internalColumn) {
                case 0:
                    ret = 18;
                    break;
                case 1:
                    ret = 32;
                    break;
                case 2:
                    ret = 45;
                    break;
                case 3:
                    ret = 52;
                    break;
                case 4:
                    ret = 63;
                    break;
            }
            return ret + channel * 64 - channelScroll * 64;
        }

        /// <summary>
        /// Takes a cursor-column(8) and condenses it to the corresponding file column(5)
        /// <br></br>
        /// </summary>
        /// <param name="cursorCol"></param>
        /// <returns></returns>
        public static int cursorColToFileCol(int cursorCol) {
            int chan = cursorCol / 8;
            switch (cursorCol % 8) {
                case 0: // note
                    return 0 + chan * 5; // returns note column of current channel
                case 1: // instrument digit 1
                case 2: // instrument digit 2
                    return 1 + chan * 5; // returns instrument column of current channel
                case 3: // volume digit 1
                case 4: // volume digit 2
                    return 2 + chan * 5; // returns volume column of current channel
                case 5: // effect
                    return 3 + chan * 5; // returns effect column of current channel
                case 6: // effect parameter digit 1
                case 7: // effect parameter digit 2
                    return 4 + chan * 5; // returns effect parameter column of current channel
            }
            return -1;
        }

        /// <summary>
        /// Converts a y position in screen-coordinates to whatever cursor row is under that position.
        /// </summary>
        /// <param name="my"></param>
        /// <returns></returns>
        public static int getRowFromGlobalY(int my) { return (my - 184) / 7 + currentRow - Rendering.FrameRenderer.centerRow; }

        /// <summary>
        /// Converts an x position in screen-coordinates to whatever cursor column is under that position.
        /// </summary>
        /// <param name="mx"></param>
        /// <returns></returns>
        public static int getCursorColumnFromGlobalX(int mx) {
            mx -= 22;
            int channel = mx / 64;
            int positionAlongChannel = (mx + 0) % 64;
            int columnWithinChannel;
            if (positionAlongChannel < 0)
                columnWithinChannel = -1;
            else if (positionAlongChannel < 19)
                columnWithinChannel = 0;
            else if (positionAlongChannel < 26)
                columnWithinChannel = 1;
            else if (positionAlongChannel < 32)
                columnWithinChannel = 2;
            else if (positionAlongChannel < 38)
                columnWithinChannel = 3;
            else if (positionAlongChannel < 45)
                columnWithinChannel = 4;
            else if (positionAlongChannel < 52)
                columnWithinChannel = 5;
            else if (positionAlongChannel < 57)
                columnWithinChannel = 6;
            else
                columnWithinChannel = 7;
            if ((columnWithinChannel + channel * 8 + channelScroll * 8) > channelScroll * 8 + 95)
                return channelScroll * 8 + 95;
            return columnWithinChannel + channel * 8 + channelScroll * 8;
        }

        public static void Move(int x, int y) {
            int frameLength = thisFrame.GetLastRow() + 1;
            cursorColumn += x;
            while (y < 0) {
                MoveOneRow(-1);
                y++;
            }
            while (y > 0) {
                MoveOneRow(1);
                y--;
            }

            cursorColumn = (cursorColumn + Song.CHANNEL_COUNT * 8) % (Song.CHANNEL_COUNT * 8);
        }
        /// <summary>
        /// Moves the cursor up or down one row depending on the value of 'dir'.<br></br>
        /// 'dir' must be either 1 or -1
        /// </summary>
        /// <param name="dir"></param>
        static void MoveOneRow(int dir) {
            cursorRow += dir;
            if (cursorRow < 0) {
                PreviousFrame();
                cursorRow = thisFrame.GetLastRow();
            }
            if (cursorRow > thisFrame.GetLastRow()) {
                NextFrame();
                cursorRow = 0;
            }
        }

        static void correctChanScroll() {
            while (cursorColumn - channelScroll * 8 >= 96)
                channelScroll++;
            while (cursorColumn - channelScroll * 8 < 0)
                channelScroll--;
        }

        public static void Goto(int frame, int row) {
            currentFrame = frame;
            cursorRow = Math.Clamp(row, 0, thisFrame.GetLastRow());
            if (cursorRow > thisFrame.GetLastRow())
                cursorRow = thisFrame.GetLastRow();

        }
        public static void NextFrame() {
            currentFrame++;
            currentFrame %= Song.currentSong.frames.Count;
            if (cursorRow > thisFrame.GetLastRow())
                cursorRow = thisFrame.GetLastRow();
        }

        public static void PreviousFrame() {
            currentFrame--;
            currentFrame += Song.currentSong.frames.Count;
            currentFrame %= Song.currentSong.frames.Count;
            if (cursorRow > thisFrame.GetLastRow())
                cursorRow = thisFrame.GetLastRow();
        }

        public static void ToggleChannel(int num) {
            channelToggles[num] = !channelToggles[num];
        }

        public static void SoloChannel(int num) {
            for (int i = 0; i < channelToggles.Length; ++i)
                channelToggles[i] = i == num;
        }

        public static void MuteAllChannels() {
            for (int i = 0; i < channelToggles.Length; ++i)
                channelToggles[i] = false;
        }
        public static void UnmuteAllChannels() {
            for (int i = 0; i < channelToggles.Length; ++i)
                channelToggles[i] = true;
        }

        public static bool isChannelSoloed(int num) {
            for (int i = 0; i < channelToggles.Length; ++i) {
                if (i == num) {
                    if (channelToggles[i] == false) return false;
                } else {
                    if (channelToggles[i] == true) return false;
                }
            }
            return true;
        }
    }
}