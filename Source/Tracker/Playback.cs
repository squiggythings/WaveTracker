using WaveTracker.Audio;

namespace WaveTracker.Tracker {
    public static class Playback {
        public static bool IsPlaying { get; private set; }

        private static bool lastIsPlaying;
        public static CursorPos position;
        private static int nextPlaybackFrame;
        private static int nextPlaybackRow;
        private static bool hasGotoTriggerFlag;
        private static int tickCounter;
        public static int TicksPerRowOverride { get; set; }

        private static int ticksPerRow;
        public static WTFrame Frame {
            get {
                return App.CurrentSong.FrameSequence[position.Frame];
            }
        }

        public static void Update() {
            if (App.Shortcuts["General\\Play from beginning"].IsPressedDown) {
                PlayFromBeginning();
            }

            if (App.Shortcuts["General\\Play from cursor"].IsPressedDown) {
                PlayFromCursor();
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

        /// <summary>
        /// Plays from the beginning of the frame that the cursor is on
        /// </summary>
        public static void Play() {
            Stop();
            IsPlaying = true;
            tickCounter = 0;
            ChannelManager.Reset();
            ChannelManager.PreviewChannel.Reset();
            position.Frame = App.PatternEditor.cursorPosition.Frame;
            position.Row = 0;
            if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                App.PatternEditor.SnapToPlaybackPosition();
            }

            Restore();
            PlayRow();
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
            PlayRow();
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
            PlayRow();
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public static void Stop() {
            IsPlaying = false;
            foreach (Channel c in ChannelManager.Channels) {
                c.Cut();
            }

            ChannelManager.Reset();
        }

        private static void SetTicksPerRow() {
            ticksPerRow = TicksPerRowOverride == -1
                ? App.CurrentSong.TicksPerRow[position.Row % App.CurrentSong.TicksPerRow.Length]
                : TicksPerRowOverride;
        }

        public static void Tick() {
            if (IsPlaying) {
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
            position.MoveToRow(position.Row + 1, App.CurrentSong);
            if (IsPlaying) {
                if (App.PatternEditor.FollowMode && !AudioEngine.IsRendering) {
                    App.PatternEditor.SnapToPlaybackPosition();
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

        private static void Restore() {
            if (App.Settings.PatternEditor.RestoreChannelState) {
                RestoreUntil(position.Frame, position.Row);
            }
        }

        private static void RestoreUntil(int frame, int row) {
            TicksPerRowOverride = -1;
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
