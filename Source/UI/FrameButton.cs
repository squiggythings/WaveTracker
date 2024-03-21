using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Tracker;
using WaveTracker.Rendering;


namespace WaveTracker.UI {
    public class FrameButton : Clickable {
        public int offset { get; private set; }
        /// <summary>
        /// The index of this button's frame in the song's frame sequence
        /// </summary>
        int ThisFrameIndex => App.PatternEditor.cursorPosition.Frame + offset;
        int valueSaved;
        bool isDragging;
        /// <summary>
        /// The frame this button represents
        /// </summary>
        WTFrame ThisFrame => App.CurrentSong.FrameSequence[ThisFrameIndex];
        List<WTFrame> FrameSequence => App.CurrentSong.FrameSequence;

        public FrameButton(int offset, Element parent) {
            height = 15;
            width = 17;
            this.offset = offset;
            SetParent(parent);
        }

        public void Update() {
            if (App.PatternEditor.cursorPosition.Frame + offset == FrameSequence.Count && FrameSequence.Count < 100) {
                SetTooltip("Add frame", "Add a new frame at the end of the song");
                if (Clicked && offset < 12) {
                    App.CurrentSong.AddNewFrame();
                }
            }
            else if (offset < 12 && offset > -12) {
                SetTooltip("Frame " + ThisFrameIndex.ToString("D2"), "Click+Drag or Shift+Scroll to change pattern number");
                if (Clicked && !isDragging) {
                    if (Playback.isPlaying && App.PatternEditor.FollowMode) {
                        Playback.position.Frame += offset;
                        Playback.GotoNextFrame();
                        Playback.GotoPreviousFrame();
                    }
                    else {
                        App.PatternEditor.MoveToFrame(App.PatternEditor.cursorPosition.Frame + offset);
                        App.PatternEditor.GoToTopOfFrame();
                    }
                }
                if (ThisFrameIndex < FrameSequence.Count && ThisFrameIndex >= 0) {
                    if (IsHovered)
                        ThisFrame.PatternIndex += Input.MouseScrollWheel(KeyModifier.Shift);
                    if (LastClickPos.X >= 0 && LastClickPos.Y >= 0) {
                        if (LastClickPos.X <= width && LastClickPos.Y <= height) {
                            if (Input.GetClickDown(KeyModifier.None))
                                valueSaved = ThisFrame.PatternIndex;
                            if (Input.GetClick(KeyModifier.None)) {
                                if ((MouseY - LastClickPos.Y) / 4 != 0) {
                                    isDragging = true;
                                    App.mouseCursorArrow = 2;
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

            //if (LastClickPos.X >= 0 && LastClickPos.Y >= 0) {
            //    if (LastClickPos.X <= width - 10 && LastClickPos.Y <= height) {
            //        if (Input.GetClickDown(KeyModifier.None))
            //            valueSaved = Value;
            //        if (Input.GetClick(KeyModifier.None)) {
            //            Value = valueSaved - (MouseY - LastClickPos.Y) / 2;
            //            Game1.mouseCursorArrow = 2;
            //        }
            //    }
            //}
            enabled = ThisFrameIndex >= 0 && ThisFrameIndex <= FrameSequence.Count;
        }

        public Color GetTextColor() {
            bool matchesPatternIndex = ThisFrame.PatternIndex == App.CurrentSong.FrameSequence[App.PatternEditor.cursorPosition.Frame].PatternIndex;
            if (offset < 12 && offset > -12) {
                if (offset == 0)
                    return Color.White;
                if (IsHovered || isDragging) {
                    return matchesPatternIndex ? Color.White : new Color(147, 152, 178).Lerp(Color.White, 0.75f);
                }
                else {
                    return matchesPatternIndex ? new Color(147, 152, 178).Lerp(Color.White, 0.55f) : new Color(147, 152, 178);
                }
            }
            return new Color(174, 176, 199);
        }

        public void Draw() {
            if (ThisFrameIndex >= 0 && ThisFrameIndex < FrameSequence.Count) {
                // regular button
                Color buttonColor;
                if (offset == 0)
                    buttonColor = new Color(8, 124, 232);
                else if (!App.PatternEditor.FollowMode && Playback.isPlaying && Playback.position.Frame - App.PatternEditor.cursorPosition.Frame == offset)
                    buttonColor = Colors.theme.rowPlaybackColor.AddTo(new Color(40, 20, 40));
                else if (IsPressed && offset > -12 && offset < 12 && !isDragging)
                    buttonColor = new Color(89, 96, 138);
                else
                    buttonColor = new Color(64, 73, 115);
                DrawRoundedRect(0, 0, width, height, buttonColor);
                if (IsHovered && Input.CurrentModifier == KeyModifier.Shift) {
                    DrawRect(1, 0, width - 2, 1, Helpers.Alpha(Color.White,70));
                    DrawRect(0, 1, 1, height - 2, Helpers.Alpha(Color.White, 70));
                    DrawRect(1, height - 1, width - 2, 1, Helpers.Alpha(Color.White, 70));
                    DrawRect(width - 1, 1, 1, height - 2, Helpers.Alpha(Color.White, 70));
                }
                string label = ThisFrame.PatternIndex.ToString("D2");
                Write(label, (width - Helpers.GetWidthOfText(label)) / 2, (height + 1) / 2 - 4, GetTextColor());
                string frameNumber = ThisFrameIndex.ToString("D2");
                Write(frameNumber, (width - Helpers.GetWidthOfText(frameNumber)) / 2, -8, buttonColor.AddTo(new Color(10, 10, 10)));
            }
            else if (ThisFrameIndex == App.CurrentSong.FrameSequence.Count && App.CurrentSong.FrameSequence.Count < 100 && offset < 12) {
                // add-new-frame plus button
                Color stroke;
                if (IsPressed)
                    stroke = new Color(104, 111, 153);
                else if (IsHovered)
                    stroke = new Color(89, 96, 138);
                else
                    stroke = new Color(64, 73, 115);
                DrawRoundedRect(2, 1, 13, 13, stroke);
                if (!IsPressed)
                    DrawRect(3, 2, 11, 11, new Color(32, 37, 64));
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
