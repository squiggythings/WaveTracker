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
        PatternEditor patternEditor;
        public int offset { get; private set; }
        /// <summary>
        /// The index of this button's frame in the song's frame sequence
        /// </summary>
        int ThisFrameIndex => patternEditor.CursorPosition.Frame + offset;

        /// <summary>
        /// The frame this button represents
        /// </summary>
        WTFrame ThisFrame => patternEditor.CurrentSong.FrameSequence[ThisFrameIndex];
        List<WTFrame> FrameSequence => patternEditor.CurrentSong.FrameSequence;

        public FrameButton(int offset, PatternEditor patternEditor, Element parent) {
            height = 15;
            width = 17;
            this.patternEditor = patternEditor;
            this.offset = offset;
            SetParent(parent);
        }

        public void Update() {
            if (patternEditor.CursorPosition.Frame + offset == FrameSequence.Count && FrameSequence.Count < 100) {
                if (Clicked && offset < 12) {
                    if (!Playback.isPlaying)
                        patternEditor.CurrentSong.InsertNewFrame(FrameSequence.Count - 1);
                }
            }
            else if (offset < 12 && offset > -12) {
                if (Clicked) {
                    if (Playback.isPlaying && FrameEditor.followMode) {
                        Playback.position.Frame += offset;
                        Playback.NextFrame();
                        Playback.PreviousFrame();
                    }
                    else {
                        patternEditor.MoveToFrame(FrameEditor.currentFrame + offset);
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
            if (IsHovered || offset == 0) {
                if (offset < 12 && offset > -12)
                    return Color.White;
            }
            return new Color(174, 176, 199);
        }

        public void Draw() {
            if (ThisFrameIndex >= 0 && ThisFrameIndex < FrameSequence.Count) {
                // button
                if (offset == 0)
                    DrawRoundedRect(0, 0, width, height, new Color(8, 124, 232));
                else if (!patternEditor.FollowMode && Playback.position.Frame - patternEditor.CursorPosition.Frame == offset)
                    DrawRoundedRect(0, 0, width, height, Colors.theme.rowPlaybackColor.AddTo(new Color(40, 20, 40)));
                else if (IsPressed && offset > -12 && offset < 12)
                    DrawRoundedRect(0, 0, width, height, new Color(89, 96, 138));
                else
                    DrawRoundedRect(0, 0, width, height, new Color(64, 73, 115));
                string label = ThisFrame.PatternIndex.ToString("D2");
                Write(label, (width - Helpers.getWidthOfText(label)) / 2, (height + 1) / 2 - 4, GetTextColor());
            }
            else if (ThisFrameIndex == patternEditor.CurrentSong.FrameSequence.Count && FrameEditor.thisSong.frames.Count < 100 && offset < 12) {
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
