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
        Textbox songTitle;
        Button addSong, insertSong, removeSong, moveSongUp, moveSongDown;
        NumberBox numberOfChannels;
        public ModuleSettingsDialog() : base("Module Properties", 224, 264) {
            ok = AddNewBottomButton("OK", this);
            numberOfChannels = new NumberBox("Number of channels: ", 8, 190, this);
            numberOfChannels.SetValueLimits(1, 24);
            numberOfChannels.SetTooltip("", "Change the number of channels in this module (1-24)");

            songsList = new ListBox<WTSong>(9, 25, 149, 10, this);
            songsList.ShowItemNumbers = true;
            addSong = new Button("Add", 166, 25, this);
            addSong.width = 51;
            addSong.SetTooltip("", "Add a song to the end of this module");
            insertSong = new Button("Insert", 166, 39, this);
            insertSong.width = 51;
            insertSong.SetTooltip("", "Insert a song after the currently selected song");
            removeSong = new Button("Remove", 166, 53, this);
            removeSong.width = 51;
            removeSong.SetTooltip("", "Remove the currently selected song from this module");
            moveSongUp = new Button("Move up", 166, 67, this);
            moveSongUp.width = 51;
            moveSongUp.SetTooltip("", "Move the currently selected song up one space in the list");
            moveSongUp = new Button("Move down", 166, 81, this);
            moveSongUp.width = 51;
            moveSongUp.SetTooltip("", "Move the currently selected song down one space in the list");


            songTitle = new Textbox("Title", 9, songsList.y + songsList.height + 4, songsList.width, this);
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
                DoDragging();
                insertSong.enabled = addSong.enabled = App.CurrentModule.Songs.Count < WTModule.MAX_SONG_COUNT;
                removeSong.enabled = App.CurrentModule.Songs.Count > 1;
                moveSongUp.enabled = songsList.SelectedIndex > 0;
                moveSongDown.enabled = songsList.SelectedIndex < App.CurrentModule.Songs.Count - 1;

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

                songTitle.Text = App.CurrentSong.Name;
                songTitle.Update();
                if (songTitle.ValueWasChangedInternally) {
                    songsList.SelectedItem.Name = songTitle.Text;
                }
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                DrawHorizontalLabel("Songs", 9, 17, width - 18);
                songsList.Draw();
                songTitle.Draw();
                addSong.Draw();
                insertSong.Draw();
                removeSong.Draw();
                moveSongUp.Draw();
                moveSongDown.Draw();

                DrawHorizontalLabel("Module", 9, 170, width - 18);
                numberOfChannels.Draw();
                if (numberOfChannels.Value < App.CurrentModule.ChannelCount) {
                    WriteMultiline("WARNING: This will permanently delete all data in the removed channels.", numberOfChannels.x, numberOfChannels.y + 16, 200, Color.Red, lineSpacing: 8);
                }


            }
        }
    }
}
