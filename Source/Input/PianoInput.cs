using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WaveTracker.Audio;
using WaveTracker.Tracker;
using WaveTracker.UI;

namespace WaveTracker {
    /// <summary>
    /// Handles MIDI piano input and keyboard piano input
    /// </summary>
    public static class PianoInput {
        public static List<int> currentlyHeldDownNotes = [];
        public static List<int> keyboardNotes = [];
        public static List<int> midiNotes = [];
        private static int previewNote;
        /// <summary>
        /// The current note's velocity
        /// </summary>
        public static int CurrentVelocity { get; private set; }
        /// <summary>
        /// The current note pressed, -1 if none are held down
        /// </summary>
        public static int CurrentNote { get; private set; }
        private static MidiIn MidiIn_ { get; set; }
        /// <summary>
        /// The names of all midi devices detected. The first item is reserved for "(none)"
        /// </summary>
        public static string[] MIDIDevicesNames { get; private set; }
        public static string CurrentMidiDeviceName { get; private set; }
        public static void Initialize() {
            ReadMidiDevices();
            SetMIDIDevice(App.Settings.MIDI.InputDevice);
        }

        public static void Update() {
            foreach (KeyValuePair<string, int> binding in PianoKeyInputs) {
                int midiNote = binding.Value + (App.PatternEditor.CurrentOctave + 1) * 12;
                if (App.Shortcuts[binding.Key].IsPressedRepeat) {
                    if (App.PatternEditor.cursorPosition.Column == CursorColumnType.Note || App.VisualizerMode || App.WaveEditor.IsOpen || App.InstrumentEditor.IsOpen) {
                        if (App.Shortcuts[binding.Key].IsPressedDown) {
                            KeyboardNoteOn(midiNote);
                        }
                        else if (App.Settings.PatternEditor.KeyRepeat) {
                            App.PatternEditor.TryToEnterNote(midiNote, null);
                        }
                    }
                }
                if (App.Shortcuts[binding.Key].WasReleasedThisFrame) {
                    KeyboardNoteOff(midiNote);
                }
            }

        }

        public static void ReceivePreviewPianoInput(int previewPianoCurrentNote) {
            if (previewPianoCurrentNote != previewNote) {
                if (previewPianoCurrentNote >= 0) {
                    OnNoteOnEvent(previewPianoCurrentNote, 99);
                }

                OnNoteOffEvent(previewNote);
                previewNote = previewPianoCurrentNote;

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
                MidiIn_.Close();
                MidiIn_.Dispose();
            }
            try {
                if (name == "(none)") {
                    MidiIn_ = null;
                    CurrentMidiDeviceName = name;
                }
                else {
                    int index = Array.IndexOf(MIDIDevicesNames, name);
                    MidiIn_ = new MidiIn(index - 1);
                    CurrentMidiDeviceName = name;
                    MidiIn_.MessageReceived += OnMIDIMessageReceived;
                    MidiIn_.ErrorReceived += OnMIDIErrorReceived;
                    MidiIn_.Start();
                }
            } catch {
                MidiIn_ = null;
                CurrentMidiDeviceName = "(none)";
                Dialogs.OpenMessageDialog(
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
            if (!MIDIDevicesNames.Contains(CurrentMidiDeviceName)) {
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
        }

        /// <summary>
        /// Called when a note is turned on, from either keyboard or midi
        /// </summary>
        /// <param name="note"></param>
        /// <param name="velocity"></param>
        private static void OnNoteOnEvent(int note, int? velocity, bool enterToPatternEditor = false) {
            if (!currentlyHeldDownNotes.Contains(note)) {
                currentlyHeldDownNotes.Add(note);
                CurrentNote = note;
                CurrentVelocity = velocity ?? 99;
                if (!Playback.IsPlaying) {
                    AudioEngine.ResetTicks();
                }
                ChannelManager.PreviewChannel.SetMacro(App.InstrumentBank.CurrentInstrumentIndex);
                ChannelManager.PreviewChannel.SetVolume(CurrentVelocity);
                ChannelManager.PreviewChannel.TriggerNote(CurrentNote);
                if (enterToPatternEditor) {
                    App.PatternEditor.TryToEnterNote(note, velocity);
                }
            }
        }

        /// <summary>
        /// Called when a note is turned off, from either keyboard or midi
        /// </summary>
        /// <param name="note"></param>
        private static void OnNoteOffEvent(int note) {
            currentlyHeldDownNotes.Remove(note);
            if (currentlyHeldDownNotes.Count > 0) {
                if (CurrentNote != currentlyHeldDownNotes[currentlyHeldDownNotes.Count - 1]) {
                    CurrentNote = currentlyHeldDownNotes[currentlyHeldDownNotes.Count - 1];
                    ChannelManager.PreviewChannel.SetMacro(App.InstrumentBank.CurrentInstrumentIndex);
                    ChannelManager.PreviewChannel.SetVolume(CurrentVelocity);
                    ChannelManager.PreviewChannel.TriggerNote(CurrentNote);
                }
            }
            else {
                CurrentNote = -1;
                if (!Playback.IsPlaying) {
                    AudioEngine.ResetTicks();
                }

                ChannelManager.PreviewChannel.PreviewCut();
            }
        }

        /// <summary>
        /// Called when a note on a midi keyboard is pressed
        /// </summary>
        /// <param name="note"></param>
        /// <param name="velocity"></param>
        public static void MIDINoteOn(int note, int velocity) {
            note = Math.Clamp(note, 12, 131);
            if (!midiNotes.Contains(note)) {
                midiNotes.Add(note);
                OnNoteOnEvent(note, (int)Math.Ceiling(velocity / 127f * 99), true);
            }
        }
        /// <summary>
        /// Called when a note on a midi keyboard is released
        /// </summary>
        /// <param name="note"></param>
        public static void MIDINoteOff(int note) {
            note = Math.Clamp(note, 12, 131);
            OnNoteOffEvent(note);
            midiNotes.Remove(note);
        }

        private static void KeyboardNoteOn(int note) {
            note = Math.Clamp(note, 12, 131);
            if (!keyboardNotes.Contains(note)) {
                keyboardNotes.Add(note);
                OnNoteOnEvent(note, null, true);
            }
        }

        private static void KeyboardNoteOff(int note) {
            note = Math.Clamp(note, 12, 131);
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

        private static void OnMIDIErrorReceived(object sender, MidiInMessageEventArgs e) {
            Dialogs.OpenMessageDialog(string.Format("Time {0} Message 0x{1:X8} Event {2}", e.Timestamp, e.RawMessage, e.MidiEvent), MessageDialog.Icon.Error, "OK");
        }

        private static void OnMIDIMessageReceived(object sender, MidiInMessageEventArgs e) {
            MidiEvent midiEvent = e.MidiEvent;
            if (App.Settings.MIDI.UseProgramChangeToSelectInstrument) {
                if (midiEvent is PatchChangeEvent patchEvent) {
                    if (!App.InstrumentEditor.IsOpen && !Menu.IsAMenuOpen) {
                        App.InstrumentBank.CurrentInstrumentIndex = Math.Clamp(patchEvent.Patch, 0, App.CurrentModule.Instruments.Count - 1);
                    }
                }
            }
            if (App.Settings.MIDI.ReceivePlayStopMessages) {
                if (midiEvent.CommandCode == MidiCommandCode.ContinueSequence) {
                    Playback.PlayFromCursor();
                }
                if (midiEvent.CommandCode == MidiCommandCode.StartSequence) {
                    Playback.Play();
                }
                if (midiEvent.CommandCode == MidiCommandCode.StopSequence) {
                    Playback.Stop();
                }
            }
            if (midiEvent is NoteEvent noteEvent) {
                if (noteEvent.Velocity == 0 || midiEvent.CommandCode == MidiCommandCode.NoteOff) {
                    MIDINoteOff(noteEvent.NoteNumber + App.Settings.MIDI.MIDITranspose + (App.Settings.MIDI.ApplyOctaveTranspose ? (App.PatternEditor.CurrentOctave - 4) * 12 : 0));
                }
                else {
                    MIDINoteOn(noteEvent.NoteNumber + App.Settings.MIDI.MIDITranspose + (App.Settings.MIDI.ApplyOctaveTranspose ? (App.PatternEditor.CurrentOctave - 4) * 12 : 0), noteEvent.Velocity);
                }
            }
        }

        private static readonly Dictionary<string, int> PianoKeyInputs = new Dictionary<string, int>() {
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

            { "Piano\\3rd octave C", 24 },
            { "Piano\\3rd octave C#", 25 },
            { "Piano\\3rd octave D", 26 },
            { "Piano\\3rd octave D#", 27 },
            { "Piano\\3rd octave E", 28 },
            { "Piano\\3rd octave F", 29 },
            { "Piano\\3rd octave F#", 30 },
            { "Piano\\3rd octave G", 31 },
            { "Piano\\3rd octave G#", 32 },
            { "Piano\\3rd octave A", 33 },
            { "Piano\\3rd octave A#", 34 },
            { "Piano\\3rd octave B", 35 },

            { "Piano\\4th octave C", 36 },
            { "Piano\\4th octave C#", 37 },
            { "Piano\\4th octave D", 38 },
            { "Piano\\4th octave D#", 39 },
            { "Piano\\4th octave E", 40 },
            { "Piano\\4th octave F", 41 },
            { "Piano\\4th octave F#", 42 },
            { "Piano\\4th octave G", 43 },
            { "Piano\\4th octave G#", 44 },
            { "Piano\\4th octave A", 45 },
            { "Piano\\4th octave A#", 46 },
            { "Piano\\4th octave B", 47 },
        };
    }
}
