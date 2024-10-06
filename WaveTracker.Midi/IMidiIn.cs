using System;

namespace WaveTracker.Midi {
    /// <summary>
    /// Represents a MIDI in device
    /// </summary>
    public interface IMidiIn : IDisposable {
        /// <summary>
        /// Called when a MIDI message is received
        /// </summary>
        public event EventHandler<MidiInMessageEventArgs> MessageReceived;

        /// <summary>
        /// An invalid MIDI message
        /// </summary>
        public event EventHandler<MidiInMessageEventArgs> ErrorReceived;

        /// <summary>
        /// Called when a Sysex MIDI message is received
        /// </summary>
        public event EventHandler<MidiInSysexMessageEventArgs> SysexMessageReceived;

        /// <summary>
        /// Start the MIDI in device
        /// </summary>
        public void Start();

        /// <summary>
        /// Stop the MIDI in device
        /// </summary>
        public void Stop();
    }
}