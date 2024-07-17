using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;
using WaveTracker.Audio;
using WaveTracker.UI;
using System.Diagnostics;
using NAudio.Midi;


namespace WaveTracker {
    /// <summary>
    /// Handles MIDI piano input and keyboard piano input
    /// </summary>
    public static class PianoInput {
        public static List<int> currentlyHeldDownNotes = new List<int>();
        public static List<int> keyboardNotes = new List<int>();
        public static List<int> midiNotes = new List<int>();
        public static int CurrentVelocity { get; private set; }
        public static int CurrentNote { get; private set; }
        static MidiIn MidiIn_ { get; set; }
        /// <summary>
        /// The names of all midi devices detected. The first item is reserved for "(none)"
        /// </summary>
        public static string[] MIDIDevicesNames { get; private set; }
        public static string CurrentMidiDevice { get; private set; }
        public static void Initialize() {
            ReadMidiDevices();
            SetMIDIDevice(App.Settings.MIDI.InputDevice);
        }

        public static void Update() {

            foreach (KeyValuePair<string, int> binding in PianoKeyInputs) {
                int midiNote = binding.Value + (App.PatternEditor.CurrentOctave + 1) * 12;
                if (App.Shortcuts[binding.Key].IsPressedDown) {
                    if (App.PatternEditor.cursorPosition.Column == CursorColumnType.Note) {
                        KeyboardNoteOn(midiNote);
                        if (App.Shortcuts[binding.Key].IsPressedRepeat) {
                            App.PatternEditor.TryToEnterNote(midiNote, null);
                        }
                    }
                }
                if (App.Shortcuts[binding.Key].WasReleasedThisFrame) {
                    KeyboardNoteOff(midiNote);
                }
            }

        }

        /// <summary>
        /// Tries to set the midi device to <c>name</c>. Returns true if successful, false if not.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool SetMIDIDevice(string name) {
            Debug.WriteLine("Set midi device to: " + name);
            if (MidiIn_ != null) {
                //MidiIn_.Reset();
                //MidiIn_.Stop();
                MidiIn_.Close();
                MidiIn_.Dispose();
            }
            try {
                if (name == "(none)") {
                    MidiIn_ = null;
                    CurrentMidiDevice = name;
                }
                else {
                    int index = Array.IndexOf(MIDIDevicesNames, name);
                    MidiIn_ = new MidiIn(index - 1);
                    CurrentMidiDevice = name;
                    MidiIn_.MessageReceived += OnMIDIMessageReceived;
                    MidiIn_.ErrorReceived += OnMIDIErrorReceived;
                    MidiIn_.Start();
                }
            } catch {
                MidiIn_ = null;
                CurrentMidiDevice = "(none)";
                Dialogs.messageDialog.Open(
                    "Error opening MIDI device \"" + name + "\"!",
                    MessageDialog.Icon.Error,
                    "OK"
                    );
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads connected midi devices into MIDIDevicesNames
        /// </summary>
        public static void ReadMidiDevices() {
            MIDIDevicesNames = new string[MidiIn.NumberOfDevices + 1];
            MIDIDevicesNames[0] = "(none)";
            for (int deviceIndex = 0; deviceIndex < MidiIn.NumberOfDevices; deviceIndex++) {
                MIDIDevicesNames[deviceIndex + 1] = MidiIn.DeviceInfo(deviceIndex).ProductName;
            }
            if (!MIDIDevicesNames.Contains(CurrentMidiDevice)) {
                SetMIDIDevice("(none)");
            }
        }

        public static void ClearAllNotes() {

            for (int i = midiNotes.Count - 1; i >= 0; --i) {
                MIDINoteOff(midiNotes[i]);
            }
            for (int i = keyboardNotes.Count - 1; i >= 0; --i) {
                KeyboardNoteOff(keyboardNotes[i]);
            }
            //midiNotes.Clear();
            //keyboardNotes.Clear();
            //currentlyHeldDownNotes.Clear();
        }

        static bool IsKeyboardNotePressed(int midiNote) {
            foreach (KeyValuePair<string, int> binding in PianoKeyInputs) {
                if (App.Shortcuts[binding.Key].IsPressed) {
                    if (midiNote == binding.Value + (App.PatternEditor.CurrentOctave + 1) * 12)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when a note is turned on, from either keyboard or midi
        /// </summary>
        /// <param name="note"></param>
        /// <param name="velocity"></param>
        static void OnNoteOnEvent(int note, int velocity) {
            if (!currentlyHeldDownNotes.Contains(note)) {
                currentlyHeldDownNotes.Add(note);
                CurrentNote = note;
                CurrentVelocity = velocity;
                if (!Playback.IsPlaying) {
                    AudioEngine.ResetTicks();
                }
                ChannelManager.previewChannel.SetMacro(App.InstrumentBank.CurrentInstrumentIndex);
                ChannelManager.previewChannel.SetVolume(CurrentVelocity);
                ChannelManager.previewChannel.TriggerNote(currentlyHeldDownNotes[currentlyHeldDownNotes.Count - 1]);
            }
        }
        /// <summary>
        /// Called when a note is turned off, from either keyboard or midi
        /// </summary>
        /// <param name="note"></param>
        static void OnNoteOffEvent(int note) {
            currentlyHeldDownNotes.Remove(note);
            if (currentlyHeldDownNotes.Count > 0) {
                CurrentNote = currentlyHeldDownNotes[currentlyHeldDownNotes.Count - 1];
            }
            else {
                CurrentNote = -1;
                if (!Playback.IsPlaying)
                    AudioEngine.ResetTicks();
                ChannelManager.previewChannel.PreviewCut();
            }
        }

        /// <summary>
        /// Called when a note on a midi keyboard is pressed
        /// </summary>
        /// <param name="note"></param>
        /// <param name="velocity"></param>
        public static void MIDINoteOn(int note, int velocity) {
            if (!midiNotes.Contains(note)) {
                midiNotes.Add(note);
                OnNoteOnEvent(note, (int)Math.Ceiling(velocity / 127f * 99));
            }
        }
        /// <summary>
        /// Called when a note on a midi keyboard is released
        /// </summary>
        /// <param name="note"></param>
        public static void MIDINoteOff(int note) {
            OnNoteOffEvent(note);
            midiNotes.Remove(note);
        }

        static void KeyboardNoteOn(int note) {
            if (!keyboardNotes.Contains(note)) {
                keyboardNotes.Add(note);
                OnNoteOnEvent(note, 99);
            }
        }

        static void KeyboardNoteOff(int note) {
            keyboardNotes.Remove(note);
            if (!midiNotes.Contains(note)) {
                OnNoteOffEvent(note);
            }
        }


        public static void StopMIDI() {
            if (MidiIn_ != null) {
                MidiIn_.Stop();
                MidiIn_.Dispose();
            }
        }

        static void OnMIDIErrorReceived(object sender, MidiInMessageEventArgs e) {
            Dialogs.messageDialog.Open(String.Format("Time {0} Message 0x{1:X8} Event {2}", e.Timestamp, e.RawMessage, e.MidiEvent), MessageDialog.Icon.Error, "OK");
        }

        static void OnMIDIMessageReceived(object sender, MidiInMessageEventArgs e) {
            MidiEvent midiEvent = e.MidiEvent;
            if (midiEvent is NoteEvent noteEvent) {
                if (noteEvent.Velocity == 0) {
                    MIDINoteOff(noteEvent.NoteNumber);
                }
                else {
                    MIDINoteOn(noteEvent.NoteNumber, noteEvent.Velocity);
                }
            }
        }


        static readonly Dictionary<string, int> PianoKeyInputs = new Dictionary<string, int>() {
            { "Piano\\Lower C-1", 0 },
            { "Piano\\Lower C#1", 1 },
            { "Piano\\Lower D-1", 2 },
            { "Piano\\Lower D#1", 3 },
            { "Piano\\Lower E-1", 4 },
            { "Piano\\Lower F-1", 5 },
            { "Piano\\Lower F#1", 6 },
            { "Piano\\Lower G-1", 7 },
            { "Piano\\Lower G#1", 8 },
            { "Piano\\Lower A-1", 9 },
            { "Piano\\Lower A#1", 10 },
            { "Piano\\Lower B-1", 11 },
            { "Piano\\Lower C-2", 12 },
            { "Piano\\Lower C#2", 13 },
            { "Piano\\Lower D-2", 14 },
            { "Piano\\Lower D#2", 15 },
            { "Piano\\Lower E-2", 16 },

            { "Piano\\Upper C-2", 12 },
            { "Piano\\Upper C#2", 13 },
            { "Piano\\Upper D-2", 14 },
            { "Piano\\Upper D#2", 15 },
            { "Piano\\Upper E-2", 16 },
            { "Piano\\Upper F-2", 17 },
            { "Piano\\Upper F#2", 18 },
            { "Piano\\Upper G-2", 19 },
            { "Piano\\Upper G#2", 20 },
            { "Piano\\Upper A-2", 21 },
            { "Piano\\Upper A#2", 22 },
            { "Piano\\Upper B-2", 23 },
            { "Piano\\Upper C-3", 24 },
            { "Piano\\Upper C#3", 25 },
            { "Piano\\Upper D-3", 26 },
            { "Piano\\Upper D#3", 27 },
            { "Piano\\Upper E-3", 28 },
        };
    }
}
