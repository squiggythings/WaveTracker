using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using System.Windows.Forms;

namespace WaveTracker.Rendering
{
    public class FrameView : UI.Panel
    {
        Texture2D arrow;
        FrameButton[] frames = new FrameButton[25];
        public SpriteButton bNewFrame, bDeleteFrame, bDuplicateFrame, bMoveLeft, bMoveRight;
        public void Initialize(Texture2D sprite, GraphicsDevice device)
        {
            bNewFrame = new SpriteButton(4, 10, 15, 15, sprite, 19, this);
            bNewFrame.SetTooltip("Insert Frame", "Insert a new frame after this one");
            bDeleteFrame = new SpriteButton(19, 10, 15, 15, sprite, 24, this);
            bDeleteFrame.SetTooltip("Delete Frame", "Delete this frame from the track");
            bDuplicateFrame = new SpriteButton(34, 10, 15, 15, sprite, 17, this);
            bDuplicateFrame.SetTooltip("Duplicate Frame", "Create a copy of this frame and insert it after");

            bMoveLeft = new SpriteButton(4, 25, 15, 15, sprite, 20, this);
            bMoveLeft.SetTooltip("Move Left", "Move this frame to be earlier in the song");
            bMoveRight = new SpriteButton(19, 25, 15, 15, sprite, 21, this);
            bMoveRight.SetTooltip("Move Right", "Move this frame to be later in the song");

            // create arrow texture
            arrow = new Texture2D(device, 7, 4);
            Color[] data = new Color[7 * 4];
            Color arrowColor = new Color(8, 124, 232);
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 7; ++x)
                {
                    if (x >= y && x < 7 - y)
                        data[x + y * 7] = arrowColor;
                    else
                        data[x + y * 7] = Color.Transparent;
                }
            }
            arrow.SetData(data);
            for (int i = 0; i < frames.Length; ++i)
            {
                frames[i] = new FrameButton(i - frames.Length / 2, this);
                frames[i].x = 54 + i * 18;
                frames[i].y = 21;
            }


            InitializePanel("Frames", 2, 106, 504, 42);
        }

        public void Update()
        {
            foreach (FrameButton button in frames)
            {
                button.Update();
            }
            bDeleteFrame.enabled = FrameEditor.thisSong.frames.Count > 1;
            bNewFrame.enabled = bDuplicateFrame.enabled = FrameEditor.thisSong.frames.Count < 100;
            bMoveRight.enabled = FrameEditor.currentFrame < FrameEditor.thisSong.frames.Count - 1;
            bMoveLeft.enabled = FrameEditor.currentFrame > 0;
            if (new Rectangle(80, 12, 397, 28).Contains(MouseX, MouseY))
            {
                if (Input.MouseScrollWheel(KeyModifier.None) < 0)
                {
                    if (Playback.isPlaying)
                        Playback.NextFrame();
                    else
                        FrameEditor.NextFrame();
                }
                if (Input.MouseScrollWheel(KeyModifier.None) > 0)
                {
                    if (Playback.isPlaying)
                        Playback.PreviousFrame();
                    else
                        FrameEditor.PreviousFrame();
                }
            }
            if (!Playback.isPlaying)
            {
                if (bNewFrame.Clicked)
                {
                    FrameEditor.thisSong.frames.Insert(++FrameEditor.currentFrame, new Frame());
                }
                if (bDuplicateFrame.Clicked || Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.D, KeyModifier.Ctrl))
                {
                    FrameEditor.thisSong.frames.Insert(FrameEditor.currentFrame + 1, FrameEditor.thisFrame.Clone());
                    FrameEditor.currentFrame++;
                }
                if (bDeleteFrame.Clicked)
                {
                    FrameEditor.thisSong.frames.RemoveAt(FrameEditor.currentFrame);
                    FrameEditor.currentFrame--;
                    if (FrameEditor.currentFrame < 0)
                        FrameEditor.currentFrame = 0;
                }


                if (bMoveRight.Clicked)
                {
                    FrameEditor.thisSong.frames.Reverse(FrameEditor.currentFrame++, 2);
                }
                if (bMoveLeft.Clicked)
                {
                    FrameEditor.thisSong.frames.Reverse(--FrameEditor.currentFrame, 2);
                }
            }
        }

        public void Draw()
        {
            DrawPanel();
            DrawRect(1, 9, 52, 33, Color.White);
            DrawRect(0, 9, 1, 32, Color.White);
            bNewFrame.Draw();
            bDeleteFrame.Draw();
            bDuplicateFrame.Draw();
            bMoveLeft.Draw();
            bMoveRight.Draw();
            DrawRect(67, 12, 423, 28, new Color(32, 37, 64));
            DrawSprite(arrow, 275, 15, new Rectangle(0, 0, 7, 4));
            foreach (FrameButton button in frames)
            {
                button.Draw();
            }
            DrawRect(54, 11, 13, 29, new Color(223, 224, 232));
            DrawRect(490, 11, 13, 29, new Color(223, 224, 232));
        }
    }
}
