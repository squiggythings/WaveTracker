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


namespace WaveTracker.UI
{
    public class FrameButton : Clickable
    {
        public int offset { get; private set; }
        public FrameButton(int offset, Element parent)
        {
            height = 15;
            width = 17;
            this.offset = offset;
            SetParent(parent);
        }

        public void Update()
        {
            if (FrameEditor.currentFrame + offset == FrameEditor.thisSong.frames.Count && FrameEditor.thisSong.frames.Count < 100)
            {
                if (Clicked && offset < 12)
                {
                    if (!Playback.isPlaying)
                        FrameEditor.thisSong.frames.Add(new Frame());
                }
            }
            else if (Clicked && offset < 12 && offset > -12)
            {
                if (Playback.isPlaying)
                {
                    Playback.playbackFrame += offset;
                    Playback.NextFrame();
                    Playback.PreviousFrame();
                }
                else
                    FrameEditor.currentFrame += offset;
            }
            enabled = FrameEditor.currentFrame + offset >= 0 && FrameEditor.currentFrame + offset <= FrameEditor.thisSong.frames.Count;
        }

        public Color getTextColor()
        {
            if (IsHovered || offset == 0)
            {
                if (offset < 12 && offset > -12)
                    return Color.White;
            }
            return new Color(174, 176, 199);
        }

        public void Draw()
        {
            if (FrameEditor.currentFrame + offset >= 0 && FrameEditor.currentFrame + offset < FrameEditor.thisSong.frames.Count)
            {
                if (offset == 0)
                    DrawRoundedRect(0, 0, width, height, new Color(8, 124, 232));
                else if (IsPressed && offset > -12 && offset < 12)
                    DrawRoundedRect(0, 0, width, height, new Color(89, 96, 138));
                else
                    DrawRoundedRect(0, 0, width, height, new Color(64, 73, 115));
                string label = (FrameEditor.currentFrame + offset).ToString("D2");
                Write(label, (width - Helpers.getWidthOfText(label)) / 2, (height + 1) / 2 - 4, getTextColor());
            }
            if (FrameEditor.currentFrame + offset == FrameEditor.thisSong.frames.Count && FrameEditor.thisSong.frames.Count < 100 && offset < 12)
            {
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
                if (IsPressed)
                {
                    DrawRect(8, 5, 1, 5, Color.White);
                    DrawRect(6, 7, 5, 1, Color.White);
                }
                else
                {
                    DrawRect(8, 5, 1, 5, stroke);
                    DrawRect(6, 7, 5, 1, stroke);
                }
            }
        }
    }
}
