using System;
using System.Diagnostics;
using WaveTracker.Audio;
using WaveTracker.UI;

namespace WaveTracker.Tracker {
    public static class Playback {
        public static bool IsPlaying { get; private set; }

        private static bool lastIsPlaying;
        public static CursorPos position;
        private static int nextPlaybackFrame;
        private static int nextPlaybackRow;
        private static bool hasGotoTriggerFlag;
        private static int tickCounter;
        private static bool loopCurrentPattern;
        public static int TicksPerRowOverride { get; set; } = -1;

        /// <summary>
        /// Current playback time in seconds
        /// </summary>
        public static DateTime PlaybackTime { get; private set; }
        public static TimeSpan Time;

        public static int CurrentTicksPerRow { get; private set; }
        public static WTFrame Frame {
            get {
                return App.CurrentSong.FrameSequence[position.Frame];
            }
        }

        public static void Update() {
            if (!InputField.IsAnInputFieldBeingEdited) {
                if (App.Shortcuts["General\\Play from beginning"].IsPressedDown) {
                    PlayFromBeginning();
                }

                if (App.Shortcuts["General\\Play from cursor"].IsPressedDown) {
                    PlayFromCursor();
                }

                if (App.Shortcuts["General\\Play and loop pattern"].IsPressedDown) {
                    PlayAndLoopPattern();
                }

                if (App.Shortcuts["General\\Stop"].IsPressedDown) {
                    Stop();
                }

                if (App.Shortcuts["General\\Play/Stop"].IsPressedDown) {
                    if (Input.windowFocusTimer == 0 && Input.focusTimer > 1) {
                        if (IsPlaying) {
                            Stop();
                        }
                        else {
                            Play();
                        }
                    }
                }

                if (!App.VisualizerMode) {
                    if (App.Shortcuts["General\\Play row"].IsPressedRepeat) {
                        if (Input.dialogOpenCooldown == 0) {
                            ChannelManager.PlayRow(App.PatternEditor.cursorPosition.Frame, App.PatternEditor.cursorPosition.Row);
                            App.PatternEditor.MoveToRow(App.PatternEditor.cursorPosition.Row + 1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Plays from the beginning of the frame that the cursor is on
        /// </summary>
        public static void Play() {
            Stop();
            IsPlaying = true;
            tickCounter = 0;
            ChannelManager.Reset();
            position.Frame = App.PatternEditor.cursorPosition.Frame;
            position.Row = 0;
            if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                App.PatternEditor.SnapToPlaybackPosition();
            }

            Restore();
            AudioEngine.ResetTicks();
            //PlayRow();
        }

        /// <summary>
        /// Plays from wherever the cursor is in the song
        /// </summary>
        public static void PlayFromCursor() {
            Stop();
            IsPlaying = true;
            ChannelManager.Reset();
            tickCounter = 0;
            position = App.PatternEditor.cursorPosition;
            if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                App.PatternEditor.SnapToPlaybackPosition();
            }
            Restore();
            AudioEngine.ResetTicks();
            //PlayRow();
        }

        /// <summary>
        /// Plays from wherever the cursor is in the song
        /// </summary>
        public static void PlayAndLoopPattern() {
            Stop();

            loopCurrentPattern = true;
            IsPlaying = true;
            ChannelManager.Reset();
            tickCounter = 0;
            position.Frame = App.PatternEditor.cursorPosition.Frame;
            position.Row = 0;
            if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                App.PatternEditor.SnapToPlaybackPosition();
            }
            Restore();
            AudioEngine.ResetTicks();
            //PlayRow();
        }

        /// <summary>
        /// Plays from the beginning of the current song
        /// </summary>
        public static void PlayFromBeginning() {
            Stop();
            tickCounter = 0;
            IsPlaying = true;
            ChannelManager.Reset();

            position.Frame = 0;
            position.Row = 0;
            if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                App.PatternEditor.SnapToPlaybackPosition();
            }
            Restore();
            AudioEngine.ResetTicks();
            PlayRow();
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public static void Stop() {
            PlaybackTime = new DateTime();
            TicksPerRowOverride = -1;
            IsPlaying = false;
            foreach (Channel c in ChannelManager.Channels) {
                c.Cut();
            }
            loopCurrentPattern = false;
            ChannelManager.Reset();
        }
        public static void SetTicksPerRow() {
            if (TicksPerRowOverride == -1) {
                CurrentTicksPerRow = App.CurrentSong.TicksPerRow[position.Row % App.CurrentSong.TicksPerRow.Length];
            }
            else {
                CurrentTicksPerRow = TicksPerRowOverride;
            }
        }

        public static void SetTicksPerRow(int row) {
            if (TicksPerRowOverride == -1) {
                CurrentTicksPerRow = App.CurrentSong.TicksPerRow[row % App.CurrentSong.TicksPerRow.Length];
            }
            else {
                CurrentTicksPerRow = TicksPerRowOverride;
            }
        }

        public static void Tick() {
            if (IsPlaying) {
                PlaybackTime = PlaybackTime.AddSeconds(AudioEngine.SamplesPerTick / AudioEngine.SampleRate);
                if (!lastIsPlaying) {
                    // start playback
                    tickCounter = 0;
                    lastIsPlaying = true;
                    hasGotoTriggerFlag = false;
                    SetTicksPerRow();
                    PlayRow();
                }

                if (tickCounter >= CurrentTicksPerRow) {
                    // reached next row
                    tickCounter = 0;
                    if (hasGotoTriggerFlag) {
                        hasGotoTriggerFlag = false;
                        if (nextPlaybackFrame < 0) // CXX command
                        {
                            AudioEngine.RenderProcessedRows += 2;
                            Stop();
                            return;
                        }
                        else {
                            position.Frame = nextPlaybackFrame;
                            position.Row = nextPlaybackRow;
                            if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                                App.PatternEditor.SnapToPlaybackPosition();
                            }
                        }
                    }
                    else {
                        MoveNextRow();
                    }

                    if (AudioEngine.IsRendering) {
                        AudioEngine.RenderProcessedRows++;
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
        }

        private static void PlayRow() {
            if (position.Frame < App.CurrentSong.FrameSequence.Count) {
                ChannelManager.PlayRow(position.Frame, position.Row);
            }
        }

        private static void MoveNextRow() {
            int frameWrap = 0;
            int currentFrame = position.Frame;
            position.MoveToRow(position.Row + 1, App.CurrentSong, ref frameWrap);
            if (loopCurrentPattern && frameWrap != 0) {
                Goto(currentFrame, 0);
            }
            if (IsPlaying) {
                if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                    App.PatternEditor.SnapToPlaybackPosition();
                }
            }
        }

        public static void GotoNextFrame() {
            position.MoveToFrame(position.Frame + 1, App.CurrentSong);
            position.Row = 0;
            AudioEngine.ResetTicks();
            PlayRow();
        }

        public static void GotoPreviousFrame() {
            position.MoveToFrame(position.Frame - 1, App.CurrentSong);
            position.Row = 0;
            AudioEngine.ResetTicks();
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
            if (loopCurrentPattern) {
                nextPlaybackFrame = position.Frame;
                nextPlaybackRow = 0;
            }
            else {
                nextPlaybackFrame = fr % App.CurrentSong.FrameSequence.Count;
                nextPlaybackRow = row % (App.CurrentSong.FrameSequence[nextPlaybackFrame].GetLength() + 1);
            }
        }

        public static void Goto(int frame, int row) {
            position.Frame = frame;
            position.Row = row;
        }

        private static void Restore() {
            if (App.Settings.PatternEditor.RestoreChannelState) {
                RestoreUntil(position.Frame, position.Row);
            }
        }

        private static void RestoreUntil(int frame, int row) {
            TicksPerRowOverride = -1;
            PlaybackTime = new DateTime();
            for (int f = 0; f <= frame; f++) {
                int frameLength = App.CurrentSong[f].GetModifiedLength();
                for (int r = 0; r < frameLength; r++) {
                    ChannelManager.RestoreRow(f, r);
                    SetTicksPerRow(r);
                    if (f == frame && r == row) {
                        return;
                    }
                    PlaybackTime = PlaybackTime.AddSeconds(AudioEngine.SamplesPerTick / AudioEngine.SampleRate * CurrentTicksPerRow);
                }
            }
        }
    }
}
