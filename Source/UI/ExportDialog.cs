using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;
using WaveTracker.Rendering;

namespace WaveTracker.UI{
    public class ExportDialog : Dialog {
        Button begin, cancel;
        Button all, none;
        NumberBox loops;
        Checkbox[] channels;
        public static bool isOpen;
        public ExportDialog() : base("Export to .wav", 206, 114) {
            cancel = AddNewBottomButton("Cancel", this);
            begin = AddNewBottomButton("Begin", this);
            loops = new NumberBox("", 54, 16, 42, 42, this);
            all = new Button("All", 168, 49, this);
            all.width = 31;
            none = new Button("None", 168, 63, this);
            none.width = 31;
            loops.Value = 1;
            loops.SetValueLimits(1, 99);
            channels = new Checkbox[24];
            int i = 0;
            for (int y = 49; y <= 68; y += 19) {
                for (int x = 7; x <= 150; x += 13) {
                    channels[i] = new Checkbox(x, y, this);
                    ++i;
                }
            }
        }

        public new void Open() {
            isOpen = true;
            base.Open();
            for (int i = 0; i < channels.Length; ++i) {
                channels[i].Value = FrameEditor.channelToggles[i];
            }
        }

        public new void Close() {
            isOpen = false;
            base.Close();
        }

        public new void Update() {
            if (windowIsOpen) {
                loops.Update();
                if (cancel.Clicked || ExitButton.Clicked)
                    Close();
                if (begin.Clicked) {
                    Close();
                    Audio.AudioEngine.instance.RenderTo("", loops.Value, false);
                }
                if (all.Clicked) {
                    FrameEditor.UnmuteAllChannels();
                }
                if (none.Clicked) {
                    FrameEditor.MuteAllChannels();
                }
                for (int i = 0; i < channels.Length; ++i) {
                    channels[i].Value = FrameEditor.channelToggles[i];
                    channels[i].Update();
                    FrameEditor.channelToggles[i] = channels[i].Value;
                }
            }
        }

        public void Draw() {
            if (windowIsOpen) {
                base.Draw();
                loops.Draw();
                Write("Play song", 7, 19, UIColors.label);
                Write("times", 104, 19, UIColors.label);
                Write("Channels", 16, 38, UIColors.labelLight);
                DrawRect(7, 41, 6, 1, UIColors.labelLight);
                DrawRect(55, 41, 104, 1, UIColors.labelLight);
                all.Draw();
                none.Draw();
                for (int i = 0; i < channels.Length; ++i) {
                    channels[i].Draw();
                    WriteCenter((i + 1) + "", channels[i].x + 4, channels[i].y + 10, UIColors.label);
                }
            }
        }
    }
}
