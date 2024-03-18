using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class WarningMessage : Dialog {
        public string Message { get; private set; }
        Button ok, cancel;
        public WarningMessage() : base("WaveTracker", 194, 96, hasExitButton: false) {
            ok = AddNewBottomButton("OK", this);
            cancel = AddNewBottomButton("Cancel", this);
        }

        public void Open(string message) {
            Message = message;
            Open();
        }

        public void Update() {
            if (cancel.Clicked) {
                Close();
            }
        }

        public new void Draw() {
            base.Draw();
            WriteMultiline(Message, 16, 26, width - 32, UIColors.labelDark);
        }

    }
}
