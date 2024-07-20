using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace WaveTracker.UI {
    public class Toolbar : Element {
        public SpriteButton file_new;
        public SpriteButton file_open;
        public SpriteButton file_save;
        public SpriteButton file_saveAs;
        public SpriteButton file_export;

        public SpriteButton edit_undo;
        public SpriteButton edit_redo;

        public SpriteButton edit_cut;
        public SpriteButton edit_copy;
        public SpriteButton edit_paste;

        public SpriteButton playback_play;
        public SpriteButton playback_playFromBeginning;
        public SpriteButton playback_stop;
        public SpriteToggle playback_record;

        public SpriteButton frame_prev;
        public SpriteButton frame_next;

        public SpriteButton configuration;
        public Toggle followModeToggle;
        public Toggle visualizerModeToggle;
        public bool saveDialogOpen, loadDialogOpen;

        public Toolbar(int x, int y) {
            this.x = x;
            this.y = y;
            int px = 0;
            file_new = new SpriteButton(px, 0, 15, 15, 0, 0, this);
            file_new.SetTooltip("New", "Create a new song");
            px += 15;
            file_open = new SpriteButton(px, 0, 15, 15, 15, 0, this);
            file_open.SetTooltip("Open", "Open an existing song");
            px += 15;
            file_save = new SpriteButton(px, 0, 15, 15, 30, 0, this);
            file_save.SetTooltip("Save", "Save this song");
            px += 15;
            file_saveAs = new SpriteButton(px, 0, 15, 15, 45, 0, this);
            file_saveAs.SetTooltip("Save As", "Save this song as a new one");
            px += 15;
            file_export = new SpriteButton(px, 0, 15, 15, 60, 0, this);
            file_export.SetTooltip("Export", "Export the song as a .wav");
            px += 20;


            edit_undo = new SpriteButton(px, 0, 15, 15, 390, 0, this);
            edit_undo.SetTooltip("Undo", "Undo the last action");
            px += 15;
            edit_redo = new SpriteButton(px, 0, 15, 15, 405, 0, this);
            edit_redo.SetTooltip("Redo", "Redo the previously undone action");
            px += 20;


            edit_cut = new SpriteButton(px, 0, 15, 15, 75, 0, this);
            edit_cut.SetTooltip("Cut", "Cut this selection");
            px += 15;
            edit_copy = new SpriteButton(px, 0, 15, 15, 90, 0, this);
            edit_copy.SetTooltip("Copy", "Copy this selection to the clipboard");
            px += 15;
            edit_paste = new SpriteButton(px, 0, 15, 15, 105, 0, this);
            edit_paste.SetTooltip("Paste", "Insert clipboard contents");
            px += 20;

            playback_play = new SpriteButton(px, 0, 15, 15, 120, 0, this);
            playback_play.SetTooltip("Play", "Play the song from the current frame");
            px += 15;
            playback_playFromBeginning = new SpriteButton(px, 0, 15, 15, 135, 0, this);
            playback_playFromBeginning.SetTooltip("Play From Beginning", "Play the song from the first frame");
            px += 15;
            playback_stop = new SpriteButton(px, 0, 15, 15, 150, 0, this);
            playback_stop.SetTooltip("Stop", "Stop playing");
            px += 15;
            playback_record = new SpriteToggle(px, 0, 15, 15, 165, 0, this);
            playback_record.SetTooltip("Toggle Edit Mode", "Enable/disable edit mode");
            px += 20;

            frame_prev = new SpriteButton(px, 0, 15, 15, 180, 0, this);
            frame_prev.SetTooltip("Previous Frame", "Go to the previous frame");
            px += 15;
            frame_next = new SpriteButton(px, 0, 15, 15, 195, 0, this);
            frame_next.SetTooltip("Next Frame", "Go to the next frame");
            px += 20;

            configuration = new SpriteButton(px, 0, 15, 15, 210, 0, this);
            configuration.SetTooltip("Configuration", "Open WaveTracker configuration");
            px += 20;

            followModeToggle = new Toggle("Follow mode", px, 1, this);
            followModeToggle.SetTooltip("", "Toggle whether the cursor follows the playhead during playback");

            visualizerModeToggle = new Toggle("Visualizer", 0, 1, this);
            visualizerModeToggle.x = 955 - visualizerModeToggle.width;
            visualizerModeToggle.SetTooltip("", "Toggle visualizer presentation mode");
            Dialogs.exportDialog = new ExportDialog();
        }

        public void Update() {
            file_export.enabled = !App.VisualizerMode;
            playback_record.enabled = !App.VisualizerMode;
            edit_copy.enabled = App.PatternEditor.SelectionIsActive && !App.VisualizerMode;
            edit_cut.enabled = App.PatternEditor.SelectionIsActive && !App.VisualizerMode;
            edit_paste.enabled = App.PatternEditor.HasClipboard && !App.VisualizerMode;
            edit_redo.enabled = App.PatternEditor.CanRedo && !App.VisualizerMode;
            edit_undo.enabled = App.PatternEditor.CanUndo && !App.VisualizerMode;
            frame_next.enabled = !App.VisualizerMode;
            frame_prev.enabled = !App.VisualizerMode;

            if (Input.GetKeyDown(Keys.S, KeyModifier.Ctrl)) {
                SaveLoad.SaveFile();
            }
            if (Input.GetKeyDown(Keys.O, KeyModifier.Ctrl)) { SaveLoad.OpenFile(); }

            if (file_new.Clicked) { SaveLoad.NewFile(); }

            if (file_open.Clicked) { SaveLoad.OpenFile(); }
            if (file_save.Clicked) { SaveLoad.SaveFile(); }
            if (file_saveAs.Clicked) { SaveLoad.SaveFileAs(); }
            if (file_export.Clicked) {
                Input.CancelClick();
                Dialogs.exportDialog.Open();
            }




            if (edit_undo.Clicked) { App.PatternEditor.Undo(); }
            if (edit_redo.Clicked) { App.PatternEditor.Redo(); }

            if (edit_cut.Clicked) { App.PatternEditor.Cut(); }
            if (edit_copy.Clicked) { App.PatternEditor.CopyToClipboard(); }
            if (edit_paste.Clicked) { App.PatternEditor.PasteFromClipboard(); }

            if (playback_play.Clicked) { Tracker.Playback.Play(); }
            if (playback_playFromBeginning.Clicked) { Tracker.Playback.PlayFromBeginning(); }
            if (playback_stop.Clicked) { Tracker.Playback.Stop(); }
            playback_record.Value = App.PatternEditor.EditMode;
            playback_record.Update();
            if (playback_record.ValueWasChangedInternally) {
                App.PatternEditor.EditMode = playback_record.Value;
            }


            if (frame_next.Clicked) { App.PatternEditor.NextFrame(); }
            if (frame_prev.Clicked) { App.PatternEditor.PreviousFrame(); }

            if (configuration.Clicked) { Dialogs.configurationDialog.Open(); }

            followModeToggle.Value = App.PatternEditor.FollowMode;
            followModeToggle.Update();
            App.PatternEditor.FollowMode = followModeToggle.Value;

            visualizerModeToggle.x = App.WindowWidth - visualizerModeToggle.width - 3;
            visualizerModeToggle.Value = App.VisualizerMode;
            visualizerModeToggle.Update();
            if (visualizerModeToggle.Value != App.VisualizerMode) {
                if (visualizerModeToggle.Value)
                    App.Visualizer.GenerateWaveColors();
                App.VisualizerMode = visualizerModeToggle.Value;
            }

            if (SaveLoad.savecooldown > 0) {
                SaveLoad.savecooldown--;
            }
        }
        public void Draw() {
            DrawRect(-x, 0, App.WindowWidth, 15, Color.White);
            file_new.Draw();
            file_open.Draw();
            file_save.Draw();
            file_saveAs.Draw();
            file_export.Draw();

            edit_undo.Draw();
            edit_redo.Draw();

            edit_cut.Draw();
            edit_copy.Draw();
            edit_paste.Draw();

            playback_play.Draw();
            playback_playFromBeginning.Draw();
            playback_stop.Draw();
            playback_record.Draw();

            frame_prev.Draw();
            frame_next.Draw();

            configuration.Draw();

            followModeToggle.Draw();
            visualizerModeToggle.Draw();
        }
    }
}
