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
using WaveTracker.Audio;

namespace WaveTracker.UI {
    public class ExportDialog : Dialog {
        private Button begin, cancel;
        private Button all, none;
        private NumberBox loops;
        private Checkbox[] channels;
        public static bool IsOpen { get; private set; }
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
            IsOpen = true;
            base.Open();
            for (int i = 0; i < channels.Length; ++i) {
                channels[i].Value = ChannelManager.IsChannelOn(i);
            }
        }

        public new void Close() {
            IsOpen = false;
            base.Close();
        }

        public override void Update() {
            if (WindowIsOpen) {
                DoDragging();
                loops.Update();
                if (cancel.Clicked || ExitButton.Clicked)
                    Close();
                if (begin.Clicked) {
                    Close();
                    Audio.AudioEngine.instance.RenderTo("", loops.Value, false);
                }
                if (all.Clicked) {
                    ChannelManager.UnmuteAllChannels();
                }
                if (none.Clicked) {
                    ChannelManager.MuteAllChannels();
                }
                for (int i = 0; i < channels.Length; ++i) {
                    channels[i].Value = ChannelManager.IsChannelOn(i);
                    channels[i].Update();
                    if (channels[i].Value != ChannelManager.IsChannelOn(i))
                        ChannelManager.ToggleChannel(i);
                }
            }
        }

        public new void Draw() {
            if (WindowIsOpen) {
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
