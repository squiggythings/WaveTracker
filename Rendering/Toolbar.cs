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

namespace WaveTracker.Rendering
{
    public class Toolbar : Element
    {
        public static Texture2D sprite;
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

        public SpriteButton preferences;
        public Toggle followMode;
        public Toggle visualizerMode;
        public bool saveDialogOpen, loadDialogOpen;
        public ExportDialog exportDialog;

        public Toolbar(Texture2D sprite)
        {
            Toolbar.sprite = sprite;
            x = 2;
            int px = 0;
            file_new = new SpriteButton(px, 0, 15, 15, sprite, 0, this);
            file_new.SetTooltip("New", "Create a new song");
            px += 15;
            file_open = new SpriteButton(px, 0, 15, 15, sprite, 1, this);
            file_open.SetTooltip("Open", "Open an existing song");
            px += 15;
            file_save = new SpriteButton(px, 0, 15, 15, sprite, 2, this);
            file_save.SetTooltip("Save", "Save this song");
            px += 15;
            file_saveAs = new SpriteButton(px, 0, 15, 15, sprite, 3, this);
            file_saveAs.SetTooltip("Save As", "Save this song as a new one");
            px += 15;
            file_export = new SpriteButton(px, 0, 15, 15, sprite, 4, this);
            file_export.SetTooltip("Export", "Export the song as a .wav");
            px += 20;


            edit_undo = new SpriteButton(px, 0, 15, 15, sprite, 26, this);
            edit_undo.SetTooltip("Undo", "Undo the last action");
            px += 15;
            edit_redo = new SpriteButton(px, 0, 15, 15, sprite, 27, this);
            edit_redo.SetTooltip("Redo", "Redo the previously undone action");
            px += 20;


            edit_cut = new SpriteButton(px, 0, 15, 15, sprite, 5, this);
            edit_cut.SetTooltip("Cut", "Cut this selection");
            px += 15;
            edit_copy = new SpriteButton(px, 0, 15, 15, sprite, 6, this);
            edit_copy.SetTooltip("Copy", "Copy this selection to the clipboard");
            px += 15;
            edit_paste = new SpriteButton(px, 0, 15, 15, sprite, 7, this);
            edit_paste.SetTooltip("Paste", "Insert clipboard contents");
            px += 20;

            playback_play = new SpriteButton(px, 0, 15, 15, sprite, 8, this);
            playback_play.SetTooltip("Play", "Play the song from the current frame");
            px += 15;
            playback_playFromBeginning = new SpriteButton(px, 0, 15, 15, sprite, 9, this);
            playback_playFromBeginning.SetTooltip("Play From Beginning", "Play the song from the first frame");
            px += 15;
            playback_stop = new SpriteButton(px, 0, 15, 15, sprite, 10, this);
            playback_stop.SetTooltip("Stop", "Stop playing");
            px += 15;
            playback_record = new SpriteToggle(px, 0, 15, 15, sprite, 11, this);
            playback_record.SetTooltip("Toggle Edit Mode", "Enable/disable edit mode");
            px += 20;

            frame_prev = new SpriteButton(px, 0, 15, 15, sprite, 12, this);
            frame_prev.SetTooltip("Previous Frame", "Go to the previous frame");
            px += 15;
            frame_next = new SpriteButton(px, 0, 15, 15, sprite, 13, this);
            frame_next.SetTooltip("Next Frame", "Go to the next frame");
            px += 20;

            preferences = new SpriteButton(px, 0, 15, 15, sprite, 14, this);
            preferences.SetTooltip("Preferences", "Open WaveTracker preferences");
            px += 20;

            followMode = new Toggle("Follow mode", px, 1, this);
            followMode.SetTooltip("", "Toggle whether the cursor follows the playhead during playback");

            visualizerMode = new Toggle("Visualizer", 0, 1, this);
            visualizerMode.x = 955 - visualizerMode.width;
            visualizerMode.SetTooltip("", "Toggle visualizer presentation mode");
            exportDialog = new ExportDialog();
        }

        public void Update()
        {
            file_export.enabled = !Game1.VisualizerMode;
            playback_record.enabled = !Game1.VisualizerMode;
            edit_copy.enabled = FrameEditor.selectionActive && !Game1.VisualizerMode;
            edit_cut.enabled = FrameEditor.selectionActive && !Game1.VisualizerMode;
            edit_paste.enabled = FrameEditor.clipboard.Count > 0 && !Game1.VisualizerMode;
            edit_redo.enabled = FrameEditor.historyIndex < FrameEditor.history.Count - 1 && !Game1.VisualizerMode;
            edit_undo.enabled = FrameEditor.historyIndex > 0 && !Game1.VisualizerMode;
            frame_next.enabled = !Game1.VisualizerMode;
            frame_prev.enabled = !Game1.VisualizerMode;

            if (Input.GetKeyDown(Keys.S, KeyModifier.Ctrl))
            {
                SaveLoad.SaveFile();
            }
            if (Input.GetKeyDown(Keys.O, KeyModifier.Ctrl))
            { SaveLoad.OpenFile(); }

            if (file_new.Clicked) { SaveLoad.NewFile(); }

            if (file_open.Clicked) { SaveLoad.OpenFile(); }
            if (file_save.Clicked) { SaveLoad.SaveFile(); }
            if (file_saveAs.Clicked) { SaveLoad.SaveFileAs(); }
            if (file_export.Clicked)
            {
                Input.CancelClick();
                exportDialog.Open();
            }




            if (edit_undo.Clicked) { FrameEditor.Undo(); }
            if (edit_redo.Clicked) { FrameEditor.Redo(); }

            if (edit_cut.Clicked) { FrameEditor.Cut(); }
            if (edit_copy.Clicked) { FrameEditor.CopyToClipboard(); }
            if (edit_paste.Clicked) { FrameEditor.PasteFromClipboard(); }

            if (playback_play.Clicked) { Tracker.Playback.Play(); }
            if (playback_playFromBeginning.Clicked) { Tracker.Playback.PlayFromBeginning(); }
            if (playback_stop.Clicked) { Tracker.Playback.Stop(); }
            if (playback_record.Clicked) { FrameEditor.canEdit = !FrameEditor.canEdit; }
            playback_record.Value = FrameEditor.canEdit;

            if (frame_next.Clicked) { FrameEditor.NextFrame(); }
            if (frame_prev.Clicked) { FrameEditor.PreviousFrame(); }

            if (preferences.Clicked) { Preferences.dialog.Open(); }

            followMode.Value = FrameEditor.followMode;
            visualizerMode.Value = Game1.VisualizerMode;
            followMode.Update();
            visualizerMode.Update();
            Game1.VisualizerMode = visualizerMode.Value;
            FrameEditor.followMode = followMode.Value;
            exportDialog.Update();
            if (SaveLoad.savecooldown > 0)
            {
                
                SaveLoad.savecooldown--;
            }
        }
        public void Draw()
        {
            DrawRect(-x, 0, 960, 15, Color.White);
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

            preferences.Draw();

            followMode.Draw();
            visualizerMode.Draw();
            //exportDialog.Draw();
        }
    }
}
