using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class FramesPanel : Panel {
        private FrameButton[] frames;
        public SpriteButton bNewFrame, bDeleteFrame, bDuplicateFrame, bMoveLeft, bMoveRight;

        //public Button increasePattern, decreasePattern;
        private MouseRegion scrollRegion;
        //Menu contextMenu;
        public FramesPanel(int x, int y, int width, int height) : base("Frames", x, y, width, height) {
            bNewFrame = new SpriteButton(4, 10, 15, 15, 285, 0, this);
            bNewFrame.SetTooltip("Insert Frame", "Insert a new frame after this one");
            bDeleteFrame = new SpriteButton(19, 10, 15, 15, 360, 0, this);
            bDeleteFrame.SetTooltip("Delete Frame", "Delete this frame from the track");
            bDuplicateFrame = new SpriteButton(34, 10, 15, 15, 255, 0, this);
            bDuplicateFrame.SetTooltip("Duplicate Frame", "Create a copy of this frame and insert it after");

            bMoveLeft = new SpriteButton(4, 25, 15, 15, 300, 0, this);
            bMoveLeft.SetTooltip("Move Left", "Move this frame to be earlier in the song");
            bMoveRight = new SpriteButton(19, 25, 15, 15, 315, 0, this);
            bMoveRight.SetTooltip("Move Right", "Move this frame to be later in the song");
            scrollRegion = new MouseRegion(80, 12, 397, 28, this);
            frames = new FrameButton[25];
            for (int i = 0; i < frames.Length; ++i) {
                frames[i] = new FrameButton(i - frames.Length / 2, this);
                frames[i].x = 54 + i * 18;
                frames[i].y = 21;
            }
        }

        public void Update() {
            if (InFocus) {
                foreach (FrameButton button in frames) {
                    button.Update();
                }
                bDeleteFrame.enabled = App.CurrentSong.FrameSequence.Count > 1;
                bNewFrame.enabled = bDuplicateFrame.enabled = App.CurrentSong.FrameSequence.Count < 100;
                bMoveRight.enabled = App.PatternEditor.cursorPosition.Frame < App.CurrentSong.FrameSequence.Count - 1;
                bMoveLeft.enabled = App.PatternEditor.cursorPosition.Frame > 0;
                if (scrollRegion.IsHovered && Input.focus == null) {
                    if (!Input.GetClick(KeyModifier._Any)) {
                        if (Input.MouseScrollWheel(KeyModifier.None) < 0) {
                            App.PatternEditor.NextFrame();
                        }
                        if (Input.MouseScrollWheel(KeyModifier.None) > 0) {
                            App.PatternEditor.PreviousFrame();
                        }
                    }
                }
                if (bNewFrame.Clicked) {
                    App.PatternEditor.InsertNewFrame();
                }
                if (bDuplicateFrame.Clicked || App.Shortcuts["Frame\\Duplicate frame"].IsPressedRepeat) {
                    App.PatternEditor.DuplicateFrame();
                }
                if (bDeleteFrame.Clicked || App.Shortcuts["Frame\\Remove frame"].IsPressedRepeat) {
                    App.PatternEditor.RemoveFrame();
                }

                if (bMoveRight.Clicked) {
                    App.PatternEditor.MoveFrameRight();
                }
                if (bMoveLeft.Clicked) {
                    App.PatternEditor.MoveFrameLeft();
                }
                if (App.Shortcuts["Frame\\Increase pattern value"].IsPressedRepeat) {
                    App.PatternEditor.IncreaseFramePatternIndex();
                }
                if (App.Shortcuts["Frame\\Decrease pattern value"].IsPressedRepeat) {
                    App.PatternEditor.DecreaseFramePatternIndex();
                }
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
            foreach (FrameButton button in frames) {
                button.Draw();
            }
            DrawRect(54, 11, 13, 29, new Color(223, 224, 232));
            DrawRect(490, 11, 13, 29, new Color(223, 224, 232));
        }
    }
}
