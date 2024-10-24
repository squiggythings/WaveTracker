using Microsoft.Xna.Framework;
using System.Collections.Generic;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class FrameButton : Clickable {
        public int Offset { get; private set; }

        /// <summary>
        /// The index of this button's frame in the song's frame sequence
        /// </summary>
        private int ThisFrameIndex {
            get {
                return App.PatternEditor.cursorPosition.Frame + Offset;
            }
        }

        private int valueSaved;
        private bool isDragging;

        /// <summary>
        /// The frame this button represents
        /// </summary>
        private WTFrame ThisFrame {
            get {
                return App.CurrentSong.FrameSequence[ThisFrameIndex];
            }
        }

        /// <summary>
        /// Stores the frame that this button represented when its context menu was opened.
        /// </summary>
        private WTFrame contextMenuFrame;
        private int contextMenuFrameIndex;

        private static List<WTFrame> FrameSequence {
            get {
                return App.CurrentSong.FrameSequence;
            }
        }

        public FrameButton(int offset, Element parent) {
            height = 15;
            width = 17;
            Offset = offset;
            SetParent(parent);
        }

        public void Update() {
            if (Input.focus == null) {
                if (RightClicked && ThisFrameIndex < FrameSequence.Count && ThisFrameIndex >= 0) {
                    contextMenuFrame = ThisFrame;
                    contextMenuFrameIndex = ThisFrameIndex;
                    ContextMenu.Open(new Menu([
                    new MenuOption("Insert Frame",InsertFrameAfterThis, App.CurrentSong.FrameSequence.Count < 100),
                        new MenuOption("Remove Frame",RemoveThisFrame, App.CurrentSong.FrameSequence.Count > 1),
                        new MenuOption("Duplicate Frame",DuplicateThisFrame, App.CurrentSong.FrameSequence.Count < 100),
                        null,
                        new MenuOption("Move Left", MoveThisFrameLeft, contextMenuFrameIndex > 0),
                        new MenuOption("Move Right", MoveThisFrameRight, contextMenuFrameIndex < App.CurrentSong.FrameSequence.Count - 1),
                        null,
                        new MenuOption("Increase Pattern",IncreaseThisFramePattern, contextMenuFrame.PatternIndex < 99),
                        new MenuOption("Decrease Pattern",DecreaseThisFramePattern, contextMenuFrame.PatternIndex > 0),
                        new MenuOption("Make unique", MakeThisFrameUnique, !contextMenuFrame.GetPattern().IsEmpty),
                        new MenuOption("Set pattern...", SetThisPatternIndex)
                ]));
                }

                if (App.PatternEditor.cursorPosition.Frame + Offset == FrameSequence.Count && FrameSequence.Count < 100) {
                    SetTooltip("Add frame", "Add a new frame at the end of the song");
                    if (Clicked && Offset < 12) {
                        App.CurrentSong.AddNewFrame();
                    }
                }
                else if (Offset < 12 && Offset < 12) {
                    SetTooltip("Frame " + ThisFrameIndex.ToString("D2"), "Click+Drag or Shift+Scroll to change pattern number");
                    if (Clicked && !isDragging) {
                        if (Playback.IsPlaying && App.PatternEditor.FollowMode) {
                            Playback.position.Frame += Offset;
                            Playback.GotoNextFrame();
                            Playback.GotoPreviousFrame();
                        }
                        else {
                            App.PatternEditor.MoveToFrame(App.PatternEditor.cursorPosition.Frame + Offset);
                        }
                    }
                    if (ThisFrameIndex < FrameSequence.Count && ThisFrameIndex >= 0) {
                        if (IsHovered) {
                            ThisFrame.PatternIndex += Input.MouseScrollWheel(KeyModifier.Shift);
                        }

                        if (LastClickPos.X >= 0 && LastClickPos.Y >= 0) {
                            if (LastClickPos.X <= width && LastClickPos.Y <= height) {
                                if (Input.GetClickDown(KeyModifier.None)) {
                                    valueSaved = ThisFrame.PatternIndex;
                                }

                                if (Input.GetClick(KeyModifier.None)) {
                                    if ((MouseY - LastClickPos.Y) / 4 != 0 || Input.ClickTime > 250) {
                                        if ((MouseY - LastClickPos.Y) / 4 != 0) {
                                            isDragging = true;
                                        }
                                        App.MouseCursor = Microsoft.Xna.Framework.Input.MouseCursor.SizeNS;
                                    }
                                    if (isDragging) {
                                        ThisFrame.PatternIndex = valueSaved - (MouseY - LastClickPos.Y) / 4;
                                    }
                                }
                                else {
                                    isDragging = false;
                                }
                            }
                        }

                    }
                }
            }
            enabled = ThisFrameIndex >= 0 && ThisFrameIndex <= FrameSequence.Count;
        }

        private void InsertFrameAfterThis() {
            if (App.PatternEditor.cursorPosition.Frame >= contextMenuFrameIndex) {
                App.CurrentSong.InsertNewFrame(contextMenuFrameIndex + 1);
                if (Playback.IsPlaying && App.PatternEditor.FollowMode) {
                    Playback.GotoNextFrame();
                }
                else {
                    App.PatternEditor.NextFrame();
                }
            }
            else {
                App.CurrentSong.InsertNewFrame(contextMenuFrameIndex);
            }
        }
        private void DuplicateThisFrame() {
            App.CurrentSong.DuplicateFrame(contextMenuFrameIndex);
            if (App.PatternEditor.cursorPosition.Frame >= contextMenuFrameIndex) {
                if (Playback.IsPlaying && App.PatternEditor.FollowMode) {
                    Playback.GotoNextFrame();
                }
                else {
                    App.PatternEditor.NextFrame();
                }
            }
        }
        private void RemoveThisFrame() {
            App.CurrentSong.RemoveFrame(contextMenuFrameIndex);
            if (App.PatternEditor.cursorPosition.Frame >= contextMenuFrameIndex && App.PatternEditor.cursorPosition.Frame > 0) {
                App.PatternEditor.PreviousFrame();
            }
        }
        private void MoveThisFrameRight() {
            if (contextMenuFrameIndex == App.PatternEditor.cursorPosition.Frame) {
                App.PatternEditor.MoveFrameRight();
            }
            else {
                App.CurrentSong.SwapFrames(contextMenuFrameIndex, contextMenuFrameIndex + 1);
            }
        }
        private void MoveThisFrameLeft() {
            if (contextMenuFrameIndex == App.PatternEditor.cursorPosition.Frame) {
                App.PatternEditor.MoveFrameLeft();
            }
            else {
                App.CurrentSong.SwapFrames(contextMenuFrameIndex, contextMenuFrameIndex - 1);
            }
        }
        private void IncreaseThisFramePattern() {
            contextMenuFrame.PatternIndex++;
        }
        private void DecreaseThisFramePattern() {
            contextMenuFrame.PatternIndex--;
        }
        private void MakeThisFrameUnique() {
            App.CurrentSong.MakeFrameUnique(contextMenuFrameIndex);
        }
        private void SetThisPatternIndex() {
            Dialogs.setFramePatternDialog.Open(contextMenuFrame);
        }

        private Color GetTextColor() {
            bool matchesPatternIndex = ThisFrame.PatternIndex == App.CurrentSong.FrameSequence[App.PatternEditor.cursorPosition.Frame].PatternIndex;
            if (Offset < 12 && Offset > -12) {
                if (Offset == 0) {
                    return Color.White;
                }
                else {
                    if (IsHovered || isDragging) {
                        return matchesPatternIndex ? Color.White : new Color(147, 152, 178).Lerp(Color.White, 0.75f);
                    }
                    else {
                        return matchesPatternIndex ? new Color(147, 152, 178).Lerp(Color.White, 0.55f) : new Color(147, 152, 178);
                    }
                }
            }
            else {
                return new Color(174, 176, 199);
            }
        }

        public void Draw() {
            if (ThisFrameIndex >= 0 && ThisFrameIndex < FrameSequence.Count) {
                // regular button
                Color buttonColor;
                if (Offset == 0) {
                    buttonColor = new Color(8, 124, 232);
                }
                else {
                    if (!App.PatternEditor.FollowMode && Playback.IsPlaying && Playback.position.Frame - App.PatternEditor.cursorPosition.Frame == Offset) {
                        buttonColor = App.Settings.Colors.Theme["Playback row"].AddTo(new Color(40, 20, 40));
                    }
                    else {
                        buttonColor = IsPressed && Offset > -12 && Offset < 12 && !isDragging ? new Color(89, 96, 138) : new Color(64, 73, 115);
                    }
                }

                DrawRoundedRect(0, 0, width, height, buttonColor);
                if (IsHovered && Input.CurrentModifier == KeyModifier.Shift) {
                    DrawRect(1, 0, width - 2, 1, Helpers.Alpha(Color.White, 70));
                    DrawRect(0, 1, 1, height - 2, Helpers.Alpha(Color.White, 70));
                    DrawRect(1, height - 1, width - 2, 1, Helpers.Alpha(Color.White, 70));
                    DrawRect(width - 1, 1, 1, height - 2, Helpers.Alpha(Color.White, 70));
                }
                string label = ThisFrame.PatternIndex.ToString("D2");
                Write(label, (width - Helpers.GetWidthOfText(label)) / 2, (height + 1) / 2 - 4, GetTextColor());
                string frameNumber = ThisFrameIndex.ToString("D2");
                Write(frameNumber, (width - Helpers.GetWidthOfText(frameNumber)) / 2, -8, buttonColor.AddTo(new Color(10, 10, 10)));
            }
            else if (ThisFrameIndex == App.CurrentSong.FrameSequence.Count && App.CurrentSong.FrameSequence.Count < 100 && Offset < 12) {
                // add-new-frame plus button
                Color stroke = IsPressed ? new Color(104, 111, 153) : IsHovered ? new Color(89, 96, 138) : new Color(64, 73, 115);
                DrawRoundedRect(2, 1, 13, 13, stroke);
                if (!IsPressed) {
                    DrawRect(3, 2, 11, 11, new Color(32, 37, 64));
                }

                if (IsPressed) {
                    DrawRect(8, 5, 1, 5, Color.White);
                    DrawRect(6, 7, 5, 1, Color.White);
                }
                else {
                    DrawRect(8, 5, 1, 5, stroke);
                    DrawRect(6, 7, 5, 1, stroke);
                }
            }
        }
    }
}
