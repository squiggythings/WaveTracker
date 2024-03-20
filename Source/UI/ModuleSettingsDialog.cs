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
        Button addSong, insertSong, duplicateSong, removeSong, moveSongUp, moveSongDown;
        NumberBox numberOfChannels;
        HorizontalSlider tickRateSlider;
        public ModuleSettingsDialog() : base("Module Settings", 224, 264) {
            ok = AddNewBottomButton("OK", this);
            numberOfChannels = new NumberBox("Number of channels: ", 8, 170, this);
            numberOfChannels.SetValueLimits(1, 24);
            numberOfChannels.SetTooltip("", "Change the number of channels in this module (1-24)");

            tickRateSlider = new HorizontalSlider(8, 225, 192, 0, this);
            tickRateSlider.SetValueLimits(16, 400);
            tickRateSlider.CoarseAdjustAmount = 76;
            tickRateSlider.FineAdjustAmount = 7;


            songsList = new ListBox<WTSong>(9, 25, 149, 10, this);
            songsList.ShowItemNumbers = true;
            addSong = new Button("Add", 166, 25, this);
            addSong.width = 51;
            addSong.SetTooltip("", "Add a song to the end of this module");
            insertSong = new Button("Insert", 166, 39, this);
            insertSong.width = 51;
            insertSong.SetTooltip("", "Insert a song after the currently selected song");
            duplicateSong = new Button("Duplicate", 166, 53, this);
            duplicateSong.width = 51;
            duplicateSong.SetTooltip("", "Create a copy of the currently selected song");
            removeSong = new Button("Remove", 166, 67, this);
            removeSong.width = 51;
            removeSong.SetTooltip("", "Remove the currently selected song from this module");
            moveSongUp = new Button("Move up", 166, 81, this);
            moveSongUp.width = 51;
            moveSongUp.SetTooltip("", "Move the currently selected song up one space in the list");
            moveSongDown = new Button("Move down", 166, 95, this);
            moveSongDown.width = 51;
            moveSongDown.SetTooltip("", "Move the currently selected song down one space in the list");

            songTitle = new Textbox("Title", 9, songsList.y + songsList.height + 4, songsList.width, this);
        }

        public new void Close() {
            App.CurrentModule.ResizeChannelCount(numberOfChannels.Value);
            base.Close();
        }

        public new void Open() {
            numberOfChannels.Value = App.CurrentModule.ChannelCount;
            songsList.SetList(App.CurrentModule.Songs);
            songsList.SelectedIndex = App.CurrentSongIndex;
            base.Open();
        }

        public void Update() {
            if (windowIsOpen) {
                DoDragging();
                duplicateSong.enabled = insertSong.enabled = addSong.enabled = App.CurrentModule.Songs.Count < WTModule.MAX_SONG_COUNT;
                removeSong.enabled = App.CurrentModule.Songs.Count > 1;
                moveSongUp.enabled = songsList.SelectedIndex > 0;
                moveSongDown.enabled = songsList.SelectedIndex < App.CurrentModule.Songs.Count - 1;

                if (ExitButton.Clicked || ok.Clicked) {
                    Close();
                    return;
                }

                if (addSong.Clicked) {
                    App.CurrentModule.Songs.Add(new WTSong(App.CurrentModule));
                    songsList.SelectedIndex = App.CurrentModule.Songs.Count;
                    songsList.MoveBounds();
                }
                if (insertSong.Clicked) {
                    App.CurrentModule.Songs.Insert(songsList.SelectedIndex, new WTSong(App.CurrentModule));
                }
                if (duplicateSong.Clicked) {
                    App.CurrentModule.Songs.Insert(songsList.SelectedIndex + 1, songsList.SelectedItem.Clone());
                    songsList.SelectedIndex++;
                }
                if (removeSong.Clicked) {
                    App.CurrentModule.Songs.RemoveAt(songsList.SelectedIndex);
                    songsList.SelectedIndex--;
                    songsList.MoveBounds();
                }
                if (moveSongUp.Clicked) {
                    App.CurrentModule.Songs.Reverse(songsList.SelectedIndex - 1, 2);
                    songsList.SelectedIndex--;
                    songsList.MoveBounds();
                }
                if (moveSongDown.Clicked) {
                    App.CurrentModule.Songs.Reverse(songsList.SelectedIndex, 2);
                    songsList.SelectedIndex++;
                    songsList.MoveBounds();
                }


                songsList.Update();
                songTitle.Text = songsList.SelectedItem.Name;
                songTitle.Update();
                if (songTitle.ValueWasChangedInternally) {
                    songsList.SelectedItem.Name = songTitle.Text;
                }

                numberOfChannels.Update();
                tickRateSlider.Update();
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
                duplicateSong.Draw();
                removeSong.Draw();
                moveSongUp.Draw();
                moveSongDown.Draw();

                DrawHorizontalLabel("Module", 9, 160, width - 18);
                numberOfChannels.Draw();
                if (numberOfChannels.Value < App.CurrentModule.ChannelCount) {
                    WriteMultiline("WARNING: This will permanently delete all data in the removed channels.", numberOfChannels.x, numberOfChannels.y + 16, 200, Color.Red, lineSpacing: 8);
                }
                tickRateSlider.Draw();

            }
        }
    }
}
