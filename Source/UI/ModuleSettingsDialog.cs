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
        Button ok, cancel;
        ListBox<WTSong> songsList;
        Textbox songTitle;
        Button addSong, insertSong, duplicateSong, removeSong, moveSongUp, moveSongDown;
        NumberBox numberOfChannels;
        Dropdown tickSpeedMode;
        HorizontalSlider tickRateSlider;
        public ModuleSettingsDialog() : base("Module Settings", 232, 264) {
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);

            songsList = new ListBox<WTSong>(8, 25, 149, 10, this);
            songsList.ShowItemNumbers = true;
            addSong = new Button("Add", songsList.width + 16, 25, this);
            addSong.width = 59;
            addSong.SetTooltip("", "Add a song to the end of this module");
            insertSong = new Button("Insert", songsList.width + 16, 39, this);
            insertSong.width = 59;
            insertSong.SetTooltip("", "Insert a song after the currently selected song");
            duplicateSong = new Button("Duplicate", songsList.width + 16, 53, this);
            duplicateSong.width = 59;
            duplicateSong.SetTooltip("", "Create a copy of the currently selected song");
            removeSong = new Button("Remove", songsList.width + 16, 67, this);
            removeSong.width = 59;
            removeSong.SetTooltip("", "Remove the currently selected song from this module");
            moveSongUp = new Button("Move up", songsList.width + 16, 81, this);
            moveSongUp.width = 59;
            moveSongUp.SetTooltip("", "Move the currently selected song up one space in the list");
            moveSongDown = new Button("Move down", songsList.width + 16, 95, this);
            moveSongDown.width = 59;
            moveSongDown.SetTooltip("", "Move the currently selected song down one space in the list");

            songTitle = new Textbox("Title", 8, songsList.y + songsList.height + 4, songsList.width, this);

            numberOfChannels = new NumberBox("Channels", 8, 170, this);
            numberOfChannels.SetValueLimits(1, 24);
            numberOfChannels.SetTooltip("", "Change the number of channels in this module (1-24)");

            tickRateSlider = new HorizontalSlider(width - 10 - 112, 214, 112, 14, this);
            tickRateSlider.SetValueLimits(16, 240);
            tickRateSlider.CoarseAdjustAmount = 16;
            tickRateSlider.FineAdjustAmount = 4;
            tickSpeedMode = new Dropdown(58, 198, this, scrollWrap: false);
            tickSpeedMode.SetMenuItems(new string[] { "Default (60 Hz)", "Custom" });
        }

        public new void Close() {
            base.Close();
        }

        public void ApplyClose() {
            if (numberOfChannels.Value != App.CurrentModule.ChannelCount) {
                App.CurrentModule.ResizeChannelCount(numberOfChannels.Value);
                App.CurrentModule.SetDirty();
            }
            if (tickSpeedMode.Value == 0) {
                tickRateSlider.Value = 60;
            }
            if (tickRateSlider.Value != App.CurrentModule.TickRate) {
                App.CurrentModule.TickRate = tickRateSlider.Value;
                App.CurrentModule.SetDirty();
            }
            base.Close();
        }

        public new void Open() {
            numberOfChannels.Value = App.CurrentModule.ChannelCount;
            songsList.SetList(App.CurrentModule.Songs);
            songsList.SelectedIndex = App.CurrentSongIndex;
            tickRateSlider.Value = App.CurrentModule.TickRate;
            if (tickRateSlider.Value == 60) {
                tickSpeedMode.Value = 0;
            }
            base.Open();
        }

        public void Update() {
            if (windowIsOpen) {
                DoDragging();
                duplicateSong.enabled = insertSong.enabled = addSong.enabled = App.CurrentModule.Songs.Count < WTModule.MAX_SONG_COUNT;
                removeSong.enabled = App.CurrentModule.Songs.Count > 1;
                moveSongUp.enabled = songsList.SelectedIndex > 0;
                moveSongDown.enabled = songsList.SelectedIndex < App.CurrentModule.Songs.Count - 1;

                if (ExitButton.Clicked || cancel.Clicked) {
                    Close();
                    return;
                }
                if (ok.Clicked) {
                    ApplyClose();
                    return;
                }

                if (addSong.Clicked) {
                    App.CurrentModule.Songs.Add(new WTSong(App.CurrentModule));
                    App.CurrentModule.SetDirty();
                    songsList.SelectedIndex = App.CurrentModule.Songs.Count;
                    songsList.MoveBounds();
                }
                if (insertSong.Clicked) {
                    App.CurrentModule.Songs.Insert(songsList.SelectedIndex, new WTSong(App.CurrentModule));
                    App.CurrentModule.SetDirty();
                }
                if (duplicateSong.Clicked) {
                    App.CurrentModule.Songs.Insert(songsList.SelectedIndex + 1, songsList.SelectedItem.Clone());
                    App.CurrentModule.SetDirty();
                    songsList.SelectedIndex++;
                }
                if (removeSong.Clicked) {
                    App.CurrentModule.Songs.RemoveAt(songsList.SelectedIndex);
                    App.CurrentModule.SetDirty();
                    songsList.SelectedIndex--;
                    songsList.MoveBounds();
                }
                if (moveSongUp.Clicked) {
                    App.CurrentModule.Songs.Reverse(songsList.SelectedIndex - 1, 2);
                    App.CurrentModule.SetDirty();
                    songsList.SelectedIndex--;
                    songsList.MoveBounds();
                }
                if (moveSongDown.Clicked) {
                    App.CurrentModule.Songs.Reverse(songsList.SelectedIndex, 2);
                    App.CurrentModule.SetDirty();
                    songsList.SelectedIndex++;
                    songsList.MoveBounds();
                }


                songsList.Update();
                songTitle.Text = songsList.SelectedItem.Name;
                songTitle.Update();
                if (songTitle.ValueWasChangedInternally) {
                    songsList.SelectedItem.Name = songTitle.Text;
                    App.CurrentModule.SetDirty();
                }

                numberOfChannels.Update();
                tickSpeedMode.Update();
                tickRateSlider.enabled = tickSpeedMode.Value == 1;
                tickRateSlider.Update();

            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                DrawHorizontalLabel("Songs", 8, 17, width - 16);
                songsList.Draw();
                songTitle.Draw();
                addSong.Draw();
                insertSong.Draw();
                duplicateSong.Draw();
                removeSong.Draw();
                moveSongUp.Draw();
                moveSongDown.Draw();

                DrawHorizontalLabel("Module", 8, 161, width - 16);
                numberOfChannels.Draw();
                if (numberOfChannels.Value < App.CurrentModule.ChannelCount) {
                    WriteMultiline("WARNING: This will delete all data in the removed channels, there is no undo!", numberOfChannels.x + numberOfChannels.width + 8, numberOfChannels.y, width - (numberOfChannels.x + numberOfChannels.width + 8), Color.Red, lineSpacing: 8);
                }
                Write("Tick speed", 8, tickSpeedMode.y + 3, UIColors.label);
                if (tickRateSlider.enabled) {
                    tickRateSlider.Draw();
                    Write("Custom speed: " + tickRateSlider.Value + " Hz", 8, tickRateSlider.y + 2, UIColors.label);
                    Write("(" + 44100 / tickRateSlider.Value + " samples/tick)", 8, tickRateSlider.y + 12, UIColors.labelLight);
                }
                tickSpeedMode.Draw();
            }
        }
    }
}
