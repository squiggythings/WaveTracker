using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class ModuleSettingsDialog : Dialog {
        Button ok;
        ListBox<WTSong> songsList;
        NumberBox numberOfChannels;
        public ModuleSettingsDialog() : base("Module Properties", 224, 264) {
            ok = AddNewBottomButton("OK", this);
            numberOfChannels = new NumberBox("Number of channels: ", 8, 140, this);
            numberOfChannels.SetValueLimits(1, 24);
            numberOfChannels.SetTooltip("", "Change the number of channels in this module (1-24)");

            songsList = new ListBox<WTSong>(9, 25, 149, 10, this);
            songsList.ShowItemNumbers = true;
        }

        public new void Close() {
            App.CurrentModule.ResizeChannelCount(numberOfChannels.Value);
            base.Close();
        }

        public new void Open() {
            numberOfChannels.Value = App.CurrentModule.ChannelCount;
            songsList.SetList(App.CurrentModule.Songs);
            base.Open();
        }

        public void Update() {
            if (windowIsOpen) {
                if (ExitButton.Clicked || ok.Clicked) {
                    Close();
                }
                numberOfChannels.Update();
                if (Input.GetKeyRepeat(Keys.D1, KeyModifier.Ctrl)) {
                    App.CurrentModule.ResizeChannelCount(App.CurrentModule.ChannelCount - 1);
                }
                if (Input.GetKeyRepeat(Keys.D2, KeyModifier.Ctrl)) {
                    App.CurrentModule.ResizeChannelCount(App.CurrentModule.ChannelCount + 1);
                }
                songsList.Update();
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                numberOfChannels.Draw();
                songsList.Draw();
                DrawHorizontalLabel("Songs", 9, 17, width - 18);
            }
        }
    }
}
