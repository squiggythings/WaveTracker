using System;
using System.Runtime.InteropServices;
using WaveTracker.Midi.Interop.Windows;

namespace WaveTracker.Midi {
    /// <summary>
    /// Represents a MIDI out device
    /// </summary>
    public class MidiOut : IDisposable {
        private IntPtr hMidiOut = IntPtr.Zero;
        private bool disposed = false;
        Winmm.MidiOutCallback callback;

        /// <summary>
        /// Gets the number of MIDI devices available in the system
        /// </summary>
        public static int NumberOfDevices {
            get {
                return Winmm.midiOutGetNumDevs();
            }
        }

        /// <summary>
        /// Gets the MIDI Out device info
        /// </summary>
        public static MidiOutCapabilities DeviceInfo(int midiOutDeviceNumber) {
            MidiOutCapabilities caps = new MidiOutCapabilities();
            int structSize = Marshal.SizeOf(caps);
            MmException.Try(Winmm.midiOutGetDevCaps((IntPtr)midiOutDeviceNumber, out caps, structSize), "midiOutGetDevCaps");
            return caps;
        }


        /// <summary>
        /// Opens a specified MIDI out device
        /// </summary>
        /// <param name="deviceNo">The device number</param>
        public MidiOut(int deviceNo) {
            this.callback = new Winmm.MidiOutCallback(Callback);
            MmException.Try(Winmm.midiOutOpen(out hMidiOut, (IntPtr)deviceNo, callback, IntPtr.Zero, Winmm.CALLBACK_FUNCTION), "midiOutOpen");
        }

        /// <summary>
        /// Closes this MIDI out device
        /// </summary>
        public void Close() {
            Dispose();
        }

        /// <summary>
        /// Closes this MIDI out device
        /// </summary>
        public void Dispose() {
            GC.KeepAlive(callback);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets or sets the volume for this MIDI out device
        /// </summary>
        public int Volume {
            // TODO: Volume can be accessed by device ID
            get {
                int volume = 0;
                MmException.Try(Winmm.midiOutGetVolume(hMidiOut, ref volume), "midiOutGetVolume");
                return volume;
            }
            set {
                MmException.Try(Winmm.midiOutSetVolume(hMidiOut, value), "midiOutSetVolume");
            }
        }

        /// <summary>
        /// Resets the MIDI out device
        /// </summary>
        public void Reset() {
            MmException.Try(Winmm.midiOutReset(hMidiOut), "midiOutReset");
        }

        /// <summary>
        /// Sends a MIDI out message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="param1">Parameter 1</param>
        /// <param name="param2">Parameter 2</param>
        public void SendDriverMessage(int message, int param1, int param2) {
            MmException.Try(Winmm.midiOutMessage(hMidiOut, message, (IntPtr)param1, (IntPtr)param2), "midiOutMessage");
        }

        /// <summary>
        /// Sends a MIDI message to the MIDI out device
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(int message) {
            MmException.Try(Winmm.midiOutShortMsg(hMidiOut, message), "midiOutShortMsg");
        }

        /// <summary>
        /// Closes the MIDI out device
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                //if(disposing) Components.Dispose();
                Winmm.midiOutClose(hMidiOut);
            }
            disposed = true;
        }

        private void Callback(IntPtr midiInHandle, Winmm.MidiOutMessage message, IntPtr userData, IntPtr messageParameter1, IntPtr messageParameter2) {
        }

        /// <summary>
        /// Send a long message, for example sysex.
        /// </summary>
        /// <param name="byteBuffer">The bytes to send.</param>
        public void SendBuffer(byte[] byteBuffer) {
            var header = new Winmm.MIDIHDR();
            header.lpData = Marshal.AllocHGlobal(byteBuffer.Length);
            Marshal.Copy(byteBuffer, 0, header.lpData, byteBuffer.Length);

            header.dwBufferLength = byteBuffer.Length;
            header.dwBytesRecorded = byteBuffer.Length;
            int size = Marshal.SizeOf(header);
            Winmm.midiOutPrepareHeader(this.hMidiOut, ref header, size);
            var errcode = Winmm.midiOutLongMsg(this.hMidiOut, ref header, size);
            if (errcode != MmResult.NoError) {
                Winmm.midiOutUnprepareHeader(this.hMidiOut, ref header, size);
            }
            Marshal.FreeHGlobal(header.lpData);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        ~MidiOut() {
            System.Diagnostics.Debug.Assert(false);
            Dispose(false);
        }
    }
}