using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace WaveTracker.Tracker
{
    public struct FrameEditorState
    {
        public List<Frame> sequence;
        public FrameEditorPosition positionBefore;
        public FrameEditorPosition positionAfter;

        public FrameEditorState Clone()
        {
            FrameEditorState ret = new FrameEditorState();
            ret.sequence = CloneSequence();
            ret.positionBefore = positionBefore;
            ret.positionAfter = positionAfter;
            return ret;
        }

        private List<Frame> CloneSequence()
        {
            List<Frame> ret = new List<Frame>();
            for (int i = 0; i < sequence.Count; i++)
            {
                ret.Add(sequence[i].Clone());
            }
            return ret;
        }

        public bool IsSequenceCurrent()
        {
            if (sequence.Count != FrameEditor.thisSong.frames.Count)
                return false;
            for (int i = 0; i < sequence.Count; i++)
            {
                if (!sequence[i].Equals(FrameEditor.thisSong.frames[i]))
                    return false;
            }
            return true;
        }


        public void Load()
        {
            FrameEditor.thisSong.LoadSequenceFrom(sequence);
        }

        public static FrameEditorState Current()
        {
            FrameEditorState ret = new FrameEditorState();
            ret.sequence = FrameEditor.thisSong.CloneSequence();
            ret.positionBefore = FrameEditorPosition.Previous();
            ret.positionAfter = FrameEditorPosition.Current();

            //ret.selectionMin = FrameEditor.selectionMin;
            //ret.selectionMax = FrameEditor.selectionMax;
            //ret.selectionActive = FrameEditor.selectionActive;
            //ret.currentCol = FrameEditor.cursorColumn;
            //ret.currentRow = FrameEditor.cursorRow;
            //ret.currentFrame = FrameEditor.currentFrame;
            return ret;
        }

    }

    public struct FrameEditorPosition
    {
        public int row, col, frame;
        public Point selectionMin, selectionMax;
        public bool selectionActive;

        public static FrameEditorPosition Current()
        {
            FrameEditorPosition ret = new FrameEditorPosition();
            ret.selectionMin = FrameEditor.selectionMin;
            ret.selectionMax = FrameEditor.selectionMax;
            ret.selectionActive = FrameEditor.selectionActive;
            ret.col = FrameEditor.cursorColumn;
            ret.row = FrameEditor.cursorRow;
            ret.frame = FrameEditor.currentFrame;
            return ret;
        }

        public static FrameEditorPosition Previous()
        {
            FrameEditorPosition ret = new FrameEditorPosition();
            ret.selectionMin = FrameEditor.lastSelMin;
            ret.selectionMax = FrameEditor.lastSelMax;
            ret.selectionActive = FrameEditor.lastSelActive;
            ret.col = FrameEditor.lastCol;
            ret.row = FrameEditor.lastRow;
            ret.frame = FrameEditor.lastFrame;
            return ret;
        }

        public string selDimensions()
        {
            return selHeight() + "x" + selWidth();
        }

        public int selWidth()
        {
            return selectionMax.X - selectionMin.X + 1;
        }
        public int selHeight()
        {
            return selectionMax.Y - selectionMin.Y + 1;
        }

        public void SetSelection(bool active, Point min, Point max)
        {
            selectionActive = active;
            selectionMin = min;
            selectionMax = max;
        }

        public void Load(bool selection)
        {
            if (selection)
            {
                FrameEditor.selectionStart = selectionMin;
                FrameEditor.selectionEnd = selectionMax;
                FrameEditor.selectionMin = selectionMin;
                FrameEditor.selectionMax = selectionMax;
                FrameEditor.selectionActive = selectionActive;
                //FrameEditor.CreateSelectionBounds();
            }
            FrameEditor.cursorColumn = col;
            FrameEditor.cursorRow = row;
            FrameEditor.currentFrame = frame;
        }
    }
}
