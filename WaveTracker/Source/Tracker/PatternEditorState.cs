namespace WaveTracker.Tracker {

    public record PatternEditorState {
        private string[] patternStrings;
        /// <summary>
        /// The position before this action was committed
        /// </summary>
        public PatternEditorPosition PrePosition { get; private set; }
        /// <summary>
        /// The position after this action was committed
        /// </summary>
        public PatternEditorPosition PostPosition { get; private set; }

        /// <summary>
        /// The song's frameSequence
        /// </summary>
        private byte[] frameSequence;

        public PatternEditorState(WTSong song, PatternEditorPosition previous, PatternEditorPosition next) {
            patternStrings = song.ForcePackPatternsToStrings();
            frameSequence = song.GetFrameSequenceAsByteArray();
            PrePosition = previous;
            PostPosition = next;
        }

        /// <summary>
        /// Writes this state's data into <c>song</c>
        /// </summary>
        /// <param name="song"></param>
        public void RestoreIntoSong(WTSong song) {
            song.UnpackFrameSequence(frameSequence);
            song.UnpackPatternsFromStrings(patternStrings);
        }
    }
}