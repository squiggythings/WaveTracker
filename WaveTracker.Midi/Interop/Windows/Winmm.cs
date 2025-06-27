using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WaveTracker.Midi.Interop.Windows {
    [SupportedOSPlatform("Windows")]
    internal class Winmm {

        public enum MidiInMessage {
            /// <summary>
            /// MIM_OPEN
            /// </summary>
            Open = 0x3C1,
            /// <summary>
            /// MIM_CLOSE
            /// </summary>
            Close = 0x3C2,
            /// <summary>
            /// MIM_DATA
            /// </summary>
            Data = 0x3C3,
            /// <summary>
            /// MIM_LONGDATA
            /// </summary>
            LongData = 0x3C4,
            /// <summary>
            /// MIM_ERROR
            /// </summary>
            Error = 0x3C5,
            /// <summary>
            /// MIM_LONGERROR
            /// </summary>
            LongError = 0x3C6,
            /// <summary>
            /// MIM_MOREDATA
            /// </summary>
            MoreData = 0x3CC,
        }

        // http://msdn.microsoft.com/en-us/library/dd798460%28VS.85%29.aspx
        public delegate void MidiInCallback(IntPtr midiInHandle, MidiInMessage message, IntPtr userData, IntPtr messageParameter1, IntPtr messageParameter2);

        // http://msdn.microsoft.com/en-us/library/dd798446%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiConnect(IntPtr hMidiIn, IntPtr hMidiOut, IntPtr pReserved);

        // http://msdn.microsoft.com/en-us/library/dd798447%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiDisconnect(IntPtr hMidiIn, IntPtr hMidiOut, IntPtr pReserved);

        // http://msdn.microsoft.com/en-us/library/dd798450%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInAddBuffer(IntPtr hMidiIn, IntPtr lpMidiInHdr, int uSize);

        // http://msdn.microsoft.com/en-us/library/dd798452%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInClose(IntPtr hMidiIn);

        // http://msdn.microsoft.com/en-us/library/dd798453%28VS.85%29.aspx
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern MmResult midiInGetDevCaps(IntPtr deviceId, out MidiInCapabilities capabilities, int size);

        // http://msdn.microsoft.com/en-us/library/dd798454%28VS.85%29.aspx
        // TODO: review this, probably doesn't work
        [DllImport("winmm.dll")]
        public static extern MmResult midiInGetErrorText(int err, string lpText, int uSize);

        // http://msdn.microsoft.com/en-us/library/dd798455%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInGetID(IntPtr hMidiIn, out int lpuDeviceId);

        // http://msdn.microsoft.com/en-us/library/dd798456%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern int midiInGetNumDevs();

        // http://msdn.microsoft.com/en-us/library/dd798457%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInMessage(IntPtr hMidiIn, int msg, IntPtr dw1, IntPtr dw2);

        // http://msdn.microsoft.com/en-us/library/dd798458%28VS.85%29.aspx
        [DllImport("winmm.dll", EntryPoint = "midiInOpen")]
        public static extern MmResult midiInOpen(out IntPtr hMidiIn, IntPtr uDeviceID, MidiInCallback callback, IntPtr dwInstance, int dwFlags);

        // http://msdn.microsoft.com/en-us/library/dd798458%28VS.85%29.aspx
        [DllImport("winmm.dll", EntryPoint = "midiInOpen")]
        public static extern MmResult midiInOpenWindow(out IntPtr hMidiIn, IntPtr uDeviceID, IntPtr callbackWindowHandle, IntPtr dwInstance, int dwFlags);

        // http://msdn.microsoft.com/en-us/library/dd798459%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInPrepareHeader(IntPtr hMidiIn, IntPtr lpMidiInHdr, int uSize);

        // http://msdn.microsoft.com/en-us/library/dd798461%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInReset(IntPtr hMidiIn);

        // http://msdn.microsoft.com/en-us/library/dd798462%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInStart(IntPtr hMidiIn);

        // http://msdn.microsoft.com/en-us/library/dd798463%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInStop(IntPtr hMidiIn);

        // http://msdn.microsoft.com/en-us/library/dd798464%28VS.85%29.aspx
        [DllImport("winmm.dll")]
        public static extern MmResult midiInUnprepareHeader(IntPtr hMidiIn, IntPtr lpMidiInHdr, int uSize);

        // TODO: this is general MM interop
        public const int CALLBACK_FUNCTION = 0x30000;
        public const int CALLBACK_NULL = 0;

        // http://msdn.microsoft.com/en-us/library/dd757347%28VS.85%29.aspx
        // TODO: not sure this is right
        [StructLayout(LayoutKind.Sequential)]
        public struct MMTIME {
            public int wType;
            public int u;
        }

        // TODO: check for ANSI strings in these structs
        // TODO: check for WORD params
        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIEVENT {
            public int dwDeltaTime;
            public int dwStreamID;
            public int dwEvent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public int dwParms;
        }

        // http://msdn.microsoft.com/en-us/library/dd798449%28VS.85%29.aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIHDR {
            public IntPtr lpData; // LPSTR
            public int dwBufferLength; // DWORD
            public int dwBytesRecorded; // DWORD
            public IntPtr dwUser; // DWORD_PTR
            public int dwFlags; // DWORD
            public IntPtr lpNext; // struct mididhdr_tag *
            public IntPtr reserved; // DWORD_PTR
            public int dwOffset; // DWORD
            // n.b. MSDN documentation incorrect, see mmsystem.h
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public IntPtr[] dwReserved; // DWORD_PTR dwReserved[8]
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIPROPTEMPO {
            public int cbStruct;
            public int dwTempo;
        }
    }
}