using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using System.Diagnostics;
using System.Threading;
using WaveTracker.Rendering;
using WaveTracker;
using WaveTracker.Audio;

namespace WaveTracker.Tracker {
    public static class Playback {
        public static bool isPlaying;
        static bool lastIsPlaying;
        public static CursorPos position;
        public static int nextPlaybackFrame;
        public static int nextPlaybackRow;
        public static bool hasGotoTriggerFlag;
        static int tickCounter;
        public static int ticksPerRowOverride;
        public static int ticksPerRow;
        public static PatternEditor patternEditor;
        public static WTFrame frame => App.CurrentSong.FrameSequence[position.Frame];

        public static void Update(GameTime gameTime) {
            if (Input.GetKeyDown(Keys.F5, KeyModifier.None)) {
                PlayFromBeginning();
            }
            if (Input.GetKeyDown(Keys.F7, KeyModifier.None)) {
                PlayFromCursor();
            }
            if (Input.GetKeyDown(Keys.F8, KeyModifier.None)) {
                Stop();
            }
            if (Input.GetKeyDown(Keys.Enter, KeyModifier.None)) {
                if (Input.dialogOpenCooldown == 0)
                    if (isPlaying) {
                        Stop();
                    }
                    else {
                        Play();
                    }
            }
            if (!App.VisualizerMode) {
                if (Input.GetKeyRepeat(Keys.Enter, KeyModifier.Ctrl)) {
                    if (Input.dialogOpenCooldown == 0) {
                        ChannelManager.PlayRow(patternEditor.cursorPosition.Frame, patternEditor.cursorPosition.Row);
                        patternEditor.MoveToRow(patternEditor.cursorPosition.Row + 1);
                    }
                }
                if (Input.GetKeyDown(Keys.Enter, KeyModifier.Alt)) {
                    if (Input.dialogOpenCooldown == 0)
                        PlayFromCursor();
                }
            }
        }

        /// <summary>
        /// Plays from the beginning of the frame that the cursor is on
        /// </summary>
        public static void Play() {
            Stop();
            isPlaying = true;
            tickCounter = 0;
            ChannelManager.Reset();
            ChannelManager.previewChannel.Reset();
            position.Frame = patternEditor.cursorPosition.Frame;
            position.Row = 0;
            if (patternEditor.FollowMode && !AudioEngine.rendering) {
                patternEditor.SnapToPlaybackPosition();
            }
            Restore();
            PlayRow();
        }

        /// <summary>
        /// Plays from wherever the cursor is in the song
        /// </summary>
        public static void PlayFromCursor() {
            Stop();
            isPlaying = true;
            ChannelManager.Reset();
            tickCounter = 0;
            position = patternEditor.cursorPosition;
            if (patternEditor.FollowMode && !AudioEngine.rendering) {
                patternEditor.SnapToPlaybackPosition();
            }
            Restore();
            PlayRow();
        }

        /// <summary>
        /// Plays from the beginning of the current song
        /// </summary>
        public static void PlayFromBeginning() {
            Stop();
            tickCounter = 0;
            isPlaying = true;
            ChannelManager.Reset();

            position.Frame = 0;
            position.Row = 0;
            if (patternEditor.FollowMode && !AudioEngine.rendering) {
                patternEditor.SnapToPlaybackPosition();
            }
            Restore();
            PlayRow();
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public static void Stop() {
            isPlaying = false;
            foreach (Channel c in ChannelManager.channels) {
                c.Cut();
            }
            ChannelManager.Reset();
        }

        static void SetTicksPerRow() {
            if (ticksPerRowOverride == -1)
                ticksPerRow = App.CurrentSong.TicksPerRow[position.Row % App.CurrentSong.TicksPerRow.Length];
            else
                ticksPerRow = ticksPerRowOverride;
        }

        public static void Tick() {
            if (isPlaying) {
                if (!lastIsPlaying) {
                    // start playback
                    tickCounter = 0;
                    lastIsPlaying = true;
                    hasGotoTriggerFlag = false;
                    PlayRow();
                    SetTicksPerRow();
                }
                if (tickCounter >= ticksPerRow) {
                    // reached next row
                    tickCounter = 0;
                    if (hasGotoTriggerFlag) {
                        hasGotoTriggerFlag = false;
                        if (nextPlaybackFrame < 0) // CXX command
                        {
                            AudioEngine.processedRows += 2;
                            Stop();
                            return;
                        }
                        else {
                            position.Frame = nextPlaybackFrame;
                            position.Row = nextPlaybackRow;
                            if (patternEditor.FollowMode && !AudioEngine.rendering) {
                                patternEditor.SnapToPlaybackPosition();
                            }
                        }
                    }
                    else {
                        MoveNextRow();
                    }
                    if (AudioEngine.rendering) {
                        AudioEngine.processedRows++;
                    }
                    PlayRow();
                    SetTicksPerRow();

                }
                tickCounter++;
            }
            else {
                if (lastIsPlaying) {
                    lastIsPlaying = false;
                    ChannelManager.Reset();
                }

            }
            //ChannelManager.Tick(0);
        }

        static void PlayRow() {
            if (position.Frame < App.CurrentSong.FrameSequence.Count)
                ChannelManager.PlayRow(position.Frame, position.Row);
        }

        static void MoveNextRow() {
            position.MoveToRow(position.Row + 1, App.CurrentSong);
            if (isPlaying) {
                if (patternEditor.FollowMode && !AudioEngine.rendering) {
                    patternEditor.SnapToPlaybackPosition();
                }
            }
        }

        public static void GotoNextFrame() {
            position.MoveToFrame(position.Frame + 1, App.CurrentSong);
            position.Row = 0;
            PlayRow();
        }

        public static void GotoPreviousFrame() {
            position.MoveToFrame(position.Frame - 1, App.CurrentSong);
            position.Row = 0;
            PlayRow();
        }
        /// <summary>
        /// Makes sure playback will stop on the next row
        /// </summary>
        public static void StopNext() {
            hasGotoTriggerFlag = true;
            nextPlaybackFrame = -1;
        }

        /// <summary>
        /// Sets the playback position to apply when playback finishes its current row
        /// </summary>
        /// <param name="fr"></param>
        /// <param name="row"></param>
        public static void GotoNext(int fr, int row) {
            hasGotoTriggerFlag = true;
            nextPlaybackFrame = fr % App.CurrentSong.FrameSequence.Count;
            nextPlaybackRow = row % (App.CurrentSong.FrameSequence[nextPlaybackFrame].GetLength() + 1);
        }

        public static void Goto(int frame, int row) {
            position.Frame = frame;
            position.Row = row;
        }

        static void Restore() {
            if (Preferences.profile.restoreChannelState) {
                RestoreUntil(position.Frame, position.Row);
            }
        }
        static void RestoreUntil(int frame, int row) {
            ticksPerRowOverride = -1;
            for (int f = 0; f <= frame; f++) {
                int frameLength = App.CurrentSong[f].GetModifiedLength();
                for (int r = 0; r < frameLength; r++) {
                    ChannelManager.RestoreRow(f, r);
                    if (f == frame && r == row) {
                        return;
                    }
                }
            }
        }
    }
}
