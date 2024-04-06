using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Midi;
using System.Threading.Tasks;
using System.Diagnostics;
using WaveTracker.Tracker;
using WaveTracker.Audio;
using WaveTracker.UI;

namespace WaveTracker {
    public static class MidiInput {
        public static int CurrentMidiDeviceIndex { get; set; }
        public static MidiIn MidiIn_ { get; private set; }

        public static bool MidiNoteWasPressedThisFrame { get; private set; }

        /// <summary>
        /// The note that was pressed on this frame, -1 if otherwise
        /// </summary>
        public static int GetMidiNoteDown { get; private set; }
        public static int GetVelocity { get; private set; }
        public static int GetMidiNote { get; private set; }

        static int currentMidiNote;
        static int currentVelocity;

        static int lastMidiNote;

        static List<int> currentlyHeldDownNotes;
        /// <summary>
        /// The names of all midi devices detected. The first item is reserved for "(none)"
        /// </summary>
        public static string[] MidiDevicesNames { get; set; }

        public static void ReadMidiDevices() {

            if (MidiIn_ != null) {
                MidiIn_.Stop();
                MidiIn_.Dispose();
                currentMidiNote = -1;
                currentVelocity = 0;
            }
            MidiDevicesNames = new string[MidiIn.NumberOfDevices + 1];
            MidiDevicesNames[0] = "(none)";
            for (int deviceIndex = 0; deviceIndex < MidiIn.NumberOfDevices; deviceIndex++) {
                MidiDevicesNames[deviceIndex] = MidiIn.DeviceInfo(deviceIndex).ProductName;
            }
            if (MidiIn.NumberOfDevices > 0)
                ChangeMidiDevice(1);
            else
                ChangeMidiDevice(0);
        }

        /// <summary>
        /// Sets the midi device to number <c>index</c> in the list of midi devices.
        /// 0 is reserved for no midi device
        /// </summary>
        /// <param name="index"></param>
        public static bool ChangeMidiDevice(int index) {
            currentlyHeldDownNotes = new List<int>();
            try {
                if (MidiIn_ != null) {
                    MidiIn_.Reset();
                    MidiIn_.Stop();
                    MidiIn_.Close();
                    MidiIn_.Dispose();
                }
                CurrentMidiDeviceIndex = index;
                if (CurrentMidiDeviceIndex <= 0) {
                    MidiIn_ = null;
                    currentMidiNote = -1;
                    currentVelocity = 0;
                }
                else {
                    MidiIn_ = new MidiIn(CurrentMidiDeviceIndex - 1);
                    MidiIn_.MessageReceived += OnMessageReceived;
                    MidiIn_.ErrorReceived += OnErrorReceived;
                    MidiIn_.Start();
                }
            } catch {
                MidiIn_ = null;
                CurrentMidiDeviceIndex = 0;
                currentMidiNote = WTPattern.EVENT_NOTE_CUT;
                currentVelocity = 0;
                Dialogs.messageDialog.Open("Error opening MIDI device \"" + MidiDevicesNames[CurrentMidiDeviceIndex] + "\"!",
                MessageDialog.Icon.Error,
                "OK");
                return false;
            }
            return true;

        }

        public static void GetInput() {
            GetMidiNote = currentMidiNote;
            GetVelocity = currentVelocity;
            if (currentMidiNote != lastMidiNote) {
                lastMidiNote = currentMidiNote;
                GetMidiNoteDown = currentMidiNote;
                MidiNoteWasPressedThisFrame = true;
                if (currentVelocity == 0) {
                    if (!Playback.IsPlaying)
                        AudioEngine.ResetTicks();
                    ChannelManager.previewChannel.PreviewCut();
                }
            }
            else {
                GetMidiNoteDown = -1;
                MidiNoteWasPressedThisFrame = false;
            }

        }

        public static void Stop() {
            MidiIn_.Stop();
            MidiIn_.Dispose();
            currentMidiNote = WTPattern.EVENT_NOTE_CUT;
            currentVelocity = 0;
        }

        static void OnErrorReceived(object sender, MidiInMessageEventArgs e) {
            Debug.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}", e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        static void OnMessageReceived(object sender, MidiInMessageEventArgs e) {
            //e.MidiEvent.CommandCode;
            MidiEvent midiEvent = e.MidiEvent;
            if (midiEvent is NoteEvent) {
                NoteEvent noteEvent = (NoteEvent)midiEvent;
                if (noteEvent.Velocity == 0) {
                    currentlyHeldDownNotes.Remove(noteEvent.NoteNumber);
                    if (currentlyHeldDownNotes.Count == 0) {
                        currentMidiNote = -1;
                        currentVelocity = 0;
                    }
                    else {
                        ChannelManager.previewChannel.TriggerNote(currentlyHeldDownNotes[currentlyHeldDownNotes.Count - 1]);
                    }
                }
                else {
                    currentlyHeldDownNotes.Add(noteEvent.NoteNumber);
                    currentMidiNote = noteEvent.NoteNumber;
                    currentVelocity = noteEvent.Velocity;
                    if (!Playback.IsPlaying)
                        AudioEngine.ResetTicks();
                    ChannelManager.previewChannel.SetMacro(App.InstrumentBank.CurrentInstrumentIndex);
                    ChannelManager.previewChannel.SetVolume((int)(noteEvent.Velocity / 127f * 99));
                    ChannelManager.previewChannel.TriggerNote(currentlyHeldDownNotes[currentlyHeldDownNotes.Count - 1]);
                }
                //Debug.WriteLine(noteEvent.NoteNumber + " (" + Helpers.MIDINoteToText(noteEvent.NoteNumber) + ") velocity: " + noteEvent.Velocity);
            }
        }

    }

}
