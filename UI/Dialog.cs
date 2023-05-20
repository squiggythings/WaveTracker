using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI
{
    public class Dialog : Panel
    {
        protected Element opened;
        protected bool enabled;
        int bottomButtons;

        public void InitializeDialogCentered(string name, int w, int h)
        {
            InitializePanel(name, (960 - w) / 2, (500 - h) / 2, w, h);
        }

        protected SpriteButton newCloseButton()
        {
            return new SpriteButton(width - 10, 0, 10, 9, UI.NumberBox.buttons, 4, this);
        }

        protected Button newBottomButton(string name, Element parent)
        {
            bottomButtons++;

            Button ret = new Button(name, width - 54 * bottomButtons, height - 16, parent);
            ret.width = 51;
            return ret;
        }

        protected void Open()
        {
            Input.focus = this;
            enabled = true;
            opened = null;
        }
        protected void Open(Element opened)
        {
            Input.focus = this;
            enabled = true;
            this.opened = opened;
        }
        protected void Close()
        {
            Input.focus = opened;
            enabled = false;
        }

        public void DrawDialog()
        {
            if (enabled)
            {
                // black box across screen behind window
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));
                DrawPanel();
            }
        }
    }
}

