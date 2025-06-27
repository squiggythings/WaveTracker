using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WaveTracker.Midi.Interop.Windows;

namespace WaveTracker.Midi {
    /// <summary>
    /// Represents a MIDI in device
    /// </summary>
    [SupportedOSPlatform("Windows")]
    public class MidiInWindows : IMidiIn, IDisposable {
        private IntPtr hMidiIn = IntPtr.Zero;
        private bool disposeIsRunning = false; // true while the Dispose() method run.
        private bool disposed = false;
        private Winmm.MidiInCallback callback;

        //  Buffer headers created and marshalled to recive incoming Sysex mesages
        private IntPtr[] SysexBufferHeaders = new IntPtr[0];

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
        /// Gets the number of MIDI input devices available in the system
        /// </summary>
        public static int NumberOfDevices {
            get {
                return Winmm.midiInGetNumDevs();
            }
        }

        /// <summary>
        /// Opens a specified MIDI in device
        /// </summary>
        /// <param name="deviceNo">The device number</param>
        public MidiInWindows(int deviceNo) {
            this.callback = new Winmm.MidiInCallback(Callback);
            MmException.Try(Winmm.midiInOpen(out hMidiIn, (IntPtr)deviceNo, this.callback, IntPtr.Zero, Winmm.CALLBACK_FUNCTION), "midiInOpen");
        }

        /// <summary>
        /// Closes this MIDI in device
        /// </summary>
        public void Dispose() {
            GC.KeepAlive(callback);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start the MIDI in device
        /// </summary>
        public void Start() {
            MmException.Try(Winmm.midiInStart(hMidiIn), "midiInStart");
        }

        /// <summary>
        /// Stop the MIDI in device
        /// </summary>
        public void Stop() {
            MmException.Try(Winmm.midiInStop(hMidiIn), "midiInStop");
        }

        /// <summary>
        /// Reset the MIDI in device
        /// </summary>
        public void Reset() {
            MmException.Try(Winmm.midiInReset(hMidiIn), "midiInReset");
        }

        /// <summary>
        /// Create a number of buffers and make them available to receive incoming Sysex messages
        /// </summary>
        /// <param name="bufferSize">The size of each buffer, ideally large enough to hold a complete message from the device</param>
        /// <param name="numberOfBuffers">The number of buffers needed to handle incoming Midi while busy</param>
        public void CreateSysexBuffers(int bufferSize, int numberOfBuffers) {
            SysexBufferHeaders = new IntPtr[numberOfBuffers];

            var hdrSize = Marshal.SizeOf(typeof(Winmm.MIDIHDR));
            for (var i = 0; i < numberOfBuffers; i++) {
                var hdr = new Winmm.MIDIHDR();

                hdr.dwBufferLength = bufferSize;
                hdr.dwBytesRecorded = 0;
                hdr.lpData = Marshal.AllocHGlobal(bufferSize);
                hdr.dwFlags = 0;

                var lpHeader = Marshal.AllocHGlobal(hdrSize);
                Marshal.StructureToPtr(hdr, lpHeader, false);

                MmException.Try(Winmm.midiInPrepareHeader(hMidiIn, lpHeader, Marshal.SizeOf(typeof(Winmm.MIDIHDR))), "midiInPrepareHeader");
                MmException.Try(Winmm.midiInAddBuffer(hMidiIn, lpHeader, Marshal.SizeOf(typeof(Winmm.MIDIHDR))), "midiInAddBuffer");
                SysexBufferHeaders[i] = lpHeader;
            }
        }

        private void Callback(IntPtr midiInHandle, Winmm.MidiInMessage message, IntPtr userData, IntPtr messageParameter1, IntPtr messageParameter2) {
            switch (message) {
                case Winmm.MidiInMessage.Open:
                    // message Parameter 1 & 2 are not used
                    break;
                case Winmm.MidiInMessage.Data:
                    // parameter 1 is packed MIDI message
                    // parameter 2 is milliseconds since MidiInStart
                    if (MessageReceived != null) {
                        MessageReceived(this, new MidiInMessageEventArgs(messageParameter1.ToInt32(), messageParameter2.ToInt32()));
                    }
                    break;
                case Winmm.MidiInMessage.Error:
                    // parameter 1 is invalid MIDI message
                    if (ErrorReceived != null) {
                        ErrorReceived(this, new MidiInMessageEventArgs(messageParameter1.ToInt32(), messageParameter2.ToInt32()));
                    }
                    break;
                case Winmm.MidiInMessage.Close:
                    // message Parameter 1 & 2 are not used
                    break;
                case Winmm.MidiInMessage.LongData:
                    // parameter 1 is pointer to MIDI header
                    // parameter 2 is milliseconds since MidiInStart
                    if (SysexMessageReceived != null) {
                        Winmm.MIDIHDR hdr = (Winmm.MIDIHDR)Marshal.PtrToStructure(messageParameter1, typeof(Winmm.MIDIHDR));

                        //  Copy the bytes received into an array so that the buffer is immediately available for re-use
                        var sysexBytes = new byte[hdr.dwBytesRecorded];
                        Marshal.Copy(hdr.lpData, sysexBytes, 0, hdr.dwBytesRecorded);

                        if (sysexBytes.Length != 0) // do not trigger the sysex event if no data in SYSEX message
                            SysexMessageReceived(this, new MidiInSysexMessageEventArgs(sysexBytes, messageParameter2.ToInt32()));

                        //  Re-use the buffer - but not if we have no event handler registered as we are closing
                        //  BUT When disposing the (resetting the MidiIn port), LONGDATA midi message are fired with a zero length.
                        //  In that case, buffer should no be ReAdd to avoid an inifinite loop of callback as buffer are reused forever.
                        if (!disposeIsRunning)
                            Winmm.midiInAddBuffer(hMidiIn, messageParameter1, Marshal.SizeOf(typeof(Winmm.MIDIHDR)));
                    }
                    break;
                case Winmm.MidiInMessage.LongError:
                    // parameter 1 is pointer to MIDI header
                    // parameter 2 is milliseconds since MidiInStart
                    break;
                case Winmm.MidiInMessage.MoreData:
                    // parameter 1 is packed MIDI message
                    // parameter 2 is milliseconds since MidiInStart
                    break;
            }
        }

        /// <summary>
        /// Gets the MIDI in device info
        /// </summary>
        public static MidiInCapabilities DeviceInfo(int midiInDeviceNumber) {
            MidiInCapabilities caps = new MidiInCapabilities();
            int structSize = Marshal.SizeOf(caps);
            MmException.Try(Winmm.midiInGetDevCaps((IntPtr)midiInDeviceNumber, out caps, structSize), "midiInGetDevCaps");
            return caps;
        }

        /// <summary>
        /// Closes the MIDI in device
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                disposeIsRunning = true;
                //if(disposing) Components.Dispose();

                if (SysexBufferHeaders.Length > 0) {
                    //// When SysexMessageReceived contains event handlers (!=null) , the 'midiInReset' call generate a infinit loop of CallBack call with LONGDATA message having a zero length. 
                    //SysexMessageReceived = null; // removin all event handler to avoir the infinit loop.

                    //  Reset in order to release any Sysex buffers
                    //  We can't Unprepare and free them until they are flushed out. Neither can we close the handle.
                    MmException.Try(Winmm.midiInReset(hMidiIn), "midiInReset");

                    //  Free up all created and allocated buffers for incoming Sysex messages
                    foreach (var lpHeader in SysexBufferHeaders) {
                        Winmm.MIDIHDR hdr = (Winmm.MIDIHDR)Marshal.PtrToStructure(lpHeader, typeof(Winmm.MIDIHDR));
                        MmException.Try(Winmm.midiInUnprepareHeader(hMidiIn, lpHeader, Marshal.SizeOf(typeof(Winmm.MIDIHDR))), "midiInPrepareHeader");
                        Marshal.FreeHGlobal(hdr.lpData);
                        Marshal.FreeHGlobal(lpHeader);
                    }

                    //  Defensive protection against double disposal
                    SysexBufferHeaders = new IntPtr[0];
                }
                Winmm.midiInClose(hMidiIn);
            }
            disposed = true;
            disposeIsRunning = false;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        ~MidiInWindows() {
            System.Diagnostics.Debug.Assert(false, "MIDI In was not finalised");
            Dispose(false);
        }
    }
}