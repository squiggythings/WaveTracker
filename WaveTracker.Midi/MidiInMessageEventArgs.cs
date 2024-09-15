using System;
using System.Runtime.Versioning;

namespace WaveTracker.Midi {
    /// <summary>
    /// MIDI In Message Information
    /// </summary>
    public class MidiInMessageEventArgs : EventArgs {
        /// <summary>
        /// Create a new MIDI In Message EventArgs
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timestamp"></param>
        [SupportedOSPlatform("Windows")]
        public MidiInMessageEventArgs(int message, int timestamp) {
            this.RawMessage = message;
            this.Timestamp = timestamp;
            try {
                this.MidiEvent = MidiEvent.FromRawMMEMessage(message);
            } catch (Exception) {
                // don't worry too much - might be an invalid message
            }
        }

        [UnsupportedOSPlatform("Windows", "This doesn't set the RawMessage")]
        internal MidiInMessageEventArgs(MidiEvent midiEvent, int timestamp) {
            this.Timestamp = timestamp;
            this.MidiEvent = midiEvent;
        }

        /// <summary>
        /// The Raw message received from the MIDI In API
        /// </summary>
        [SupportedOSPlatform("Windows")]
        public int RawMessage { get; private set; }

        /// <summary>
        /// The raw message interpreted as a MidiEvent
        /// </summary>
        public MidiEvent MidiEvent { get; private set; }

        /// <summary>
        /// The timestamp in milliseconds for this message
        /// </summary>
        public int Timestamp { get; private set; }
    }
}