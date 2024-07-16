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
        //public static int CurrentMidiDeviceIndex { get; set; }
        //public static MidiIn MidiIn_ { get; private set; }

        //public static Func<int, int> NoteOnFunction { get; set; }
        //public static Func<int> NoteOffFunction { get; set; }
        ///// <summary>
        ///// The names of all midi devices detected. The first item is reserved for "(none)"
        ///// </summary>
        //public static string[] MidiDevicesNames { get; set; }

        ////public static void ReadMidiDevices() {

        ////    if (MidiIn_ != null) {
        ////        MidiIn_.Stop();
        ////        MidiIn_.Dispose();
        ////        currentMidiNote = -1;
        ////        currentVelocity = 0;
        ////    }
        ////    MidiDevicesNames = new string[MidiIn.NumberOfDevices + 1];
        ////    MidiDevicesNames[0] = "(none)";
        ////    for (int deviceIndex = 0; deviceIndex < MidiIn.NumberOfDevices; deviceIndex++) {
        ////        MidiDevicesNames[deviceIndex + 1] = MidiIn.DeviceInfo(deviceIndex).ProductName;
        ////    }
        ////    if (autoAssign) {
        ////        if (MidiIn.NumberOfDevices > 0)
        ////            ChangeMidiDevice(1);
        ////        else
        ////            ChangeMidiDevice(0);
        ////    }
        ////}

        ///// <summary>
        ///// Reads connected midi devices into MidiDevicesNames
        ///// </summary>
        //public static void ReadMidiDevices() {
        //    MidiDevicesNames = new string[MidiIn.NumberOfDevices + 1];
        //    MidiDevicesNames[0] = "(none)";
        //    for (int deviceIndex = 0; deviceIndex < MidiIn.NumberOfDevices; deviceIndex++) {
        //        MidiDevicesNames[deviceIndex + 1] = MidiIn.DeviceInfo(deviceIndex).ProductName;
        //    }
        //    if (CurrentMidiDeviceIndex > MidiDevicesNames.Length + 1) {
        //        ChangeMidiDevice(0);
        //    }
        //}

        ///// <summary>
        ///// Automatically assigns a midi device if the current device
        ///// </summary>
        //public static void AutoAssignMidiDevice() {
        //    if (MidiIn_ != null) {
        //        if (CurrentMidiDeviceIndex > MidiDevicesNames.Length) {
        //            if (MidiIn.NumberOfDevices > 0) {
        //                ChangeMidiDevice(1);
        //            }
        //            else {
        //                ChangeMidiDevice(0);
        //            }
        //        }
        //    }
        //    else {
        //        CurrentMidiDeviceIndex = 0;
        //    }

        //}

        ///// <summary>
        ///// Sets the midi device to number <c>index</c> in the list of midi devices.
        ///// 0 is reserved for no midi device
        ///// </summary>
        ///// <param name="index"></param>
        //public static bool ChangeMidiDevice(int index) {
        //    if (MidiIn_ != null) {
        //        //MidiIn_.Reset();
        //        //MidiIn_.Stop();
        //        MidiIn_.Close();
        //        MidiIn_.Dispose();
        //    }
        //    try {
        //        CurrentMidiDeviceIndex = index;
        //        if (CurrentMidiDeviceIndex <= 0) {
        //            MidiIn_ = null;
        //        }
        //        else {
        //            MidiIn_ = new MidiIn(CurrentMidiDeviceIndex - 1);
        //            MidiIn_.MessageReceived += OnMessageReceived;
        //            MidiIn_.ErrorReceived += OnErrorReceived;
        //            MidiIn_.Start();
        //        }
        //    } catch {
        //        MidiIn_ = null;
        //        CurrentMidiDeviceIndex = 0;
        //        Dialogs.messageDialog.Open("Error opening MIDI device \"" + MidiDevicesNames[index] + "\"!",
        //        MessageDialog.Icon.Error,
        //        "OK");
        //        return false;
        //    }
        //    return true;

        //}

        //public static void Stop() {
        //    if (MidiIn_ != null) {
        //        MidiIn_.Stop();
        //        MidiIn_.Dispose();
        //    }
        //}

        //static void OnErrorReceived(object sender, MidiInMessageEventArgs e) {
        //    Debug.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}", e.Timestamp, e.RawMessage, e.MidiEvent));
        //}

        //static void OnMessageReceived(object sender, MidiInMessageEventArgs e) {
        //    //e.MidiEvent.CommandCode;
        //    MidiEvent midiEvent = e.MidiEvent;
        //    if (midiEvent is NoteEvent noteEvent) {
        //        if (noteEvent.Velocity == 0) {
        //            PianoInput.MIDINoteOn(noteEvent.NoteNumber, noteEvent.Velocity);
        //        }
        //        else {
        //            PianoInput.MIDINoteOff(noteEvent.NoteNumber);
        //        }
        //    }
        //}
    }

}
