using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;


namespace WaveTracker.Rendering
{
    public class PreferencesDialog : Dialog
    {
        public SpriteButton closeX;
        public Button ok, cancel, apply;
        public Button closeButton;
        //public List<Option>
        public PreferencesDialog()
        {
            InitializeDialogCentered("Preferences", 376, 336);
            closeX = newCloseButton();
            apply = newBottomButton("Apply", this);
            cancel = newBottomButton("Cancel", this);
            ok = newBottomButton("OK", this);

        }



        void Update()
        {
            if (enabled)
            {
                if (closeX.Clicked || cancel.Clicked)
                {
                    Close();
                }

            }
        }

        void Draw()
        {
            if (enabled)
            {
                DrawDialog();
            }
        }

        void ApplyChanges()
        {

        }
    }
}
