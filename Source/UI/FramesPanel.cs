using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class FramesPanel : Panel {
        FrameButton[] frames;
        public SpriteButton bNewFrame, bDeleteFrame, bDuplicateFrame, bMoveLeft, bMoveRight;
        //public Button increasePattern, decreasePattern;

        public FramesPanel(int x, int y, int width, int height) : base("Frames", x, y, width, height) {
        }

        public void Initialize() {
            bNewFrame = new SpriteButton(4, 10, 15, 15, Rendering.Graphics.img, 285, 0, this);
            bNewFrame.SetTooltip("Insert Frame", "Insert a new frame after this one");
            bDeleteFrame = new SpriteButton(19, 10, 15, 15, Rendering.Graphics.img, 360, 0, this);
            bDeleteFrame.SetTooltip("Delete Frame", "Delete this frame from the track");
            bDuplicateFrame = new SpriteButton(34, 10, 15, 15, Rendering.Graphics.img, 255, 0, this);
            bDuplicateFrame.SetTooltip("Duplicate Frame", "Create a copy of this frame and insert it after");

            bMoveLeft = new SpriteButton(4, 25, 15, 15, Rendering.Graphics.img, 300, 0, this);
            bMoveLeft.SetTooltip("Move Left", "Move this frame to be earlier in the song");
            bMoveRight = new SpriteButton(19, 25, 15, 15, Rendering.Graphics.img, 315, 0, this);
            bMoveRight.SetTooltip("Move Right", "Move this frame to be later in the song");

            //increasePattern = new Button("+", 484, 12, this);
            //increasePattern.width = 18;
            //increasePattern.SetTooltip("Increase Pattern", "Increase this frame's pattern");
            //decreasePattern = new Button("-", 484, 26, this);
            //decreasePattern.width = 18;
            //increasePattern.SetTooltip("Decrease Pattern", "Decrease this frame's pattern");


            frames = new FrameButton[25];
            for (int i = 0; i < frames.Length; ++i) {
                frames[i] = new FrameButton(i - frames.Length / 2, this);
                frames[i].x = 54 + i * 18;
                frames[i].y = 21;
            }
        }

        public void Update() {
            foreach (FrameButton button in frames) {
                button.Update();
            }
            bDeleteFrame.enabled = App.CurrentSong.FrameSequence.Count > 1;
            bNewFrame.enabled = bDuplicateFrame.enabled = App.CurrentSong.FrameSequence.Count < 100;
            bMoveRight.enabled = App.PatternEditor.cursorPosition.Frame < App.CurrentSong.FrameSequence.Count - 1;
            bMoveLeft.enabled = App.PatternEditor.cursorPosition.Frame > 0;
            if (new Rectangle(80, 12, 397, 28).Contains(MouseX, MouseY) && Input.focus == null) {
                if (!Input.GetClick(KeyModifier._Any)) {
                    if (Input.MouseScrollWheel(KeyModifier.None) < 0) {
                        if (Playback.isPlaying && App.PatternEditor.FollowMode)
                            Playback.GotoNextFrame();
                        else
                            App.PatternEditor.NextFrame();
                    }
                    if (Input.MouseScrollWheel(KeyModifier.None) > 0) {
                        if (Playback.isPlaying && App.PatternEditor.FollowMode)
                            Playback.GotoPreviousFrame();
                        else
                            App.PatternEditor.PreviousFrame();
                    }
                }
            }
            if (!Playback.isPlaying) {
                if (bNewFrame.Clicked) {
                    App.PatternEditor.InsertNewFrame();
                    //FrameEditor.thisSong.frames.Insert(++FrameEditor.currentFrame, new Frame());
                    //FrameEditor.Goto(FrameEditor.currentFrame, FrameEditor.currentRow);
                }
                if (bDuplicateFrame.Clicked || Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.D, KeyModifier.Ctrl)) {
                    App.PatternEditor.DuplicateFrame();
                    //FrameEditor.thisSong.frames.Insert(FrameEditor.currentFrame + 1, FrameEditor.thisFrame.Clone());
                    //FrameEditor.Goto(FrameEditor.currentFrame, FrameEditor.currentRow);

                }
                if (bDeleteFrame.Clicked) {
                    App.PatternEditor.RemoveFrame();
                    //FrameEditor.thisSong.frames.RemoveAt(FrameEditor.currentFrame);
                    //FrameEditor.currentFrame--;
                    //if (FrameEditor.currentFrame < 0)
                    //    FrameEditor.currentFrame = 0;
                    //FrameEditor.Goto(FrameEditor.currentFrame, FrameEditor.currentRow);
                }


                if (bMoveRight.Clicked) {
                    App.PatternEditor.MoveFrameRight();
                    //FrameEditor.thisSong.frames.Reverse(FrameEditor.currentFrame++, 2);
                    //FrameEditor.Goto(FrameEditor.currentFrame, FrameEditor.currentRow);
                }
                if (bMoveLeft.Clicked) {
                    App.PatternEditor.MoveFrameLeft();
                    //FrameEditor.thisSong.frames.Reverse(--FrameEditor.currentFrame, 2);
                    //FrameEditor.Goto(FrameEditor.currentFrame, FrameEditor.currentRow);
                }

                //if (increasePattern.Clicked) {
                //    patternEditor.IncreaseFramePatternIndex();
                //}
                //if (decreasePattern.Clicked) {
                //    patternEditor.DecreaseFramePatternIndex();
                //}
            }
        }

        public new void Draw() {
            base.Draw();
            DrawRect(1, 9, 52, 33, Color.White);
            DrawRect(0, 9, 1, 32, Color.White);
            bNewFrame.Draw();
            bDeleteFrame.Draw();
            bDuplicateFrame.Draw();
            bMoveLeft.Draw();
            bMoveRight.Draw();
            DrawRect(67, 11, 423, 29, new Color(32, 37, 64));
            //DrawSprite(arrow, 275, 15, new Rectangle(0, 0, 7, 4));
            foreach (FrameButton button in frames) {
                button.Draw();
            }
            DrawRect(54, 11, 13, 29, new Color(223, 224, 232));
            DrawRect(490, 11, 13, 29, new Color(223, 224, 232));
        }
    }
}
