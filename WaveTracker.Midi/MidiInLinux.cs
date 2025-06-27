using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using WaveTracker.Midi.Interop.Linux;

namespace WaveTracker.Midi {
    /// <summary>
    /// Represents a MIDI in device
    /// </summary>
    [SupportedOSPlatform("Linux")]
    public class MidiInLinux : IMidiIn, IDisposable {
        private bool disposeIsRunning = false; // true while the Dispose() method run.
        private bool disposed = false;

        private int inPort;
        private int outClient;
        private int outPort;

        private bool isListening;

        private Thread listenThread;

        private static nint __seqHandle = 0;
        private static nint inClient;

        private static nint SeqHandle {
            get {
                if (__seqHandle == 0) {
                    unsafe {
                        nint seqHandle;
                        Alsa.snd_seq_open(&seqHandle, "default", Alsa.SND_SEQ_OPEN_INPUT, Alsa.SND_SEQ_NONBLOCK);
                        Alsa.snd_seq_set_client_name(seqHandle, "WaveTracker MIDI");
                        __seqHandle = seqHandle;
                        inClient = Alsa.snd_seq_client_id(seqHandle);
                        return seqHandle;
                    }
                }
                else {
                    return __seqHandle;
                }
            }
        }

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
        /// Opens a specified MIDI in device
        /// </summary>
        public MidiInLinux(int outClient, int outPort) {
            unsafe {
                // TODO: error handling?

                uint caps = Alsa.SND_SEQ_PORT_CAP_WRITE | Alsa.SND_SEQ_PORT_CAP_SUBS_WRITE;
                this.inPort = Alsa.snd_seq_create_simple_port(SeqHandle, "input", caps, Alsa.SND_SEQ_PORT_TYPE_APPLICATION);

                Alsa.snd_seq_connect_from(SeqHandle, inPort, outClient, outPort);
            }
        }

        /// <summary>
        /// Closes this MIDI in device
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start the MIDI in device
        /// </summary>
        public void Start() {
            isListening = true;
            listenThread = new Thread(listenLoop);
            listenThread.Start();
        }

        /// <summary>
        /// Stop the MIDI in device
        /// </summary>
        public void Stop() {
            isListening = false;
            listenThread?.Join();
        }

        private void listenLoop() {
            while (true) {
                unsafe {
                    Alsa.snd_seq_event* seqEvent = null;
                    int pollStatus = Alsa.snd_seq_event_input(SeqHandle, &seqEvent);
                    if (pollStatus == -Alsa.EAGAIN || seqEvent == null) {
                        if (!isListening)
                            break;
                        Thread.Sleep(1);
                        continue;
                    }

                    if (pollStatus < 0) {
                        break;
                    }

                    Alsa.snd_seq_real_time timestamp = seqEvent->time.time;
                    int timestampMillis = (int)timestamp.tv_sec * 1000 + (int)(timestamp.tv_nsec / 1_000_000);
                    
                    switch (seqEvent->type) {
                        case Alsa.snd_seq_event_type.NOTE: {
                            var ev = seqEvent->data.note;
                            var midiEvent = new NoteOnEvent(0, ev.channel + 1, ev.note, ev.velocity, (int)ev.duration);
                            MessageReceived(this, new MidiInMessageEventArgs(midiEvent, timestampMillis));
                            break;
                        }
                        case Alsa.snd_seq_event_type.KEYPRESS:
                        case Alsa.snd_seq_event_type.NOTEON: {
                            var ev = seqEvent->data.note;
                            var midiEvent = new NoteEvent(0, ev.channel + 1, MidiCommandCode.NoteOn, ev.note, ev.velocity);
                            MessageReceived(this, new MidiInMessageEventArgs(midiEvent, timestampMillis));
                            break;
                        }
                        case Alsa.snd_seq_event_type.NOTEOFF: {
                            var ev = seqEvent->data.note;
                            var midiEvent = new NoteEvent(0, ev.channel + 1, MidiCommandCode.NoteOff, ev.note, ev.velocity);
                            MessageReceived(this, new MidiInMessageEventArgs(midiEvent, timestampMillis));
                            break;
                        }
                        // case Alsa.snd_seq_event_type.TEMPO: {
                        //     var ev = seqEvent->data.queue;
                        //     var midiEvent = new TempoEvent(ev.param.value, 0);
                        //     MessageReceived(this, new MidiInMessageEventArgs(midiEvent, timestampMillis));
                        //     break;
                        // }
                        case Alsa.snd_seq_event_type.SYSEX: {
                            var ev = seqEvent->data.ext;
                    
                            byte[] sysexBytes = new byte[ev.len];
                            Marshal.Copy((nint)ev.ptr, sysexBytes, 0, sysexBytes.Length);
                    
                            SysexMessageReceived(this, new MidiInSysexMessageEventArgs(sysexBytes, timestampMillis));
                            break;
                        }
                        default:
                            break;
                    }
                    
                }
            }
        }

        /// <summary>
        /// Closes the MIDI in device
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                disposeIsRunning = true;

                Stop();

                Alsa.snd_seq_disconnect_from(SeqHandle, inPort, outClient, outPort);
                Alsa.snd_seq_delete_port(SeqHandle, inPort);
            }

            disposed = true;
            disposeIsRunning = false;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        ~MidiInLinux() {
            System.Diagnostics.Debug.Assert(false, "MIDI In was not finalised");
            Dispose(false);
        }
    }
}