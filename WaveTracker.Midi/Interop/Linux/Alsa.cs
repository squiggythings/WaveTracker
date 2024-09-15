using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WaveTracker.Midi.Interop.Linux {
    [SupportedOSPlatform("Linux")]
    internal static class Alsa {
        internal const int EAGAIN = 11;

        internal const int SND_SEQ_NONBLOCK = 0x0001;

        internal const int SND_SEQ_OPEN_INPUT = 2;

        internal const int SND_SEQ_PORT_CAP_READ = 1 << 0;
        internal const int SND_SEQ_PORT_CAP_WRITE = 1 << 1;
        internal const int SND_SEQ_PORT_CAP_SUBS_WRITE = 1 << 6;

        internal const int SND_SEQ_PORT_TYPE_APPLICATION = 1 << 20;

        /// <summary>
        /// Sequencer event type
        /// </summary>
        internal enum snd_seq_event_type : byte {
            /// <summary>
            /// system status; event data type = #snd_seq_result_t
            /// </summary>
            SYSTEM = 0,
            /// <summary>
            /// returned result status; event data type = #snd_seq_result_t
            /// </summary>
            RESULT,

            /// <summary>
            /// note on and off with duration; event data type = #snd_seq_ev_note_t
            /// </summary>
            NOTE = 5,
            /// <summary>
            /// note on; event data type = #snd_seq_ev_note_t
            /// </summary>
            NOTEON,
            /// <summary>
            /// note off; event data type = #snd_seq_ev_note_t
            /// </summary>
            NOTEOFF,
            /// <summary>
            /// key pressure change (aftertouch); event data type = #snd_seq_ev_note_t
            /// </summary>
            KEYPRESS,

            /// <summary>
            /// controller; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            CONTROLLER = 10,
            /// <summary>
            /// program change; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            PGMCHANGE,
            /// <summary>
            /// channel pressure; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            CHANPRESS,
            /// <summary>
            /// pitchwheel; event data type = #snd_seq_ev_ctrl_t; data is from -8192 to 8191)
            /// </summary>
            PITCHBEND,
            /// <summary>
            /// 14 bit controller value; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            CONTROL14,
            /// <summary>
            /// 14 bit NRPN;  event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            NONREGPARAM,
            /// <summary>
            /// 14 bit RPN; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            REGPARAM,

            /// <summary>
            /// SPP with LSB and MSB values; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            SONGPOS = 20,
            /// <summary>
            /// Song Select with song ID number; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            SONGSEL,
            /// <summary>
            /// midi time code quarter frame; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            QFRAME,
            /// <summary>
            /// SMF Time Signature event; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            TIMESIGN,
            /// <summary>
            /// SMF Key Signature event; event data type = #snd_seq_ev_ctrl_t
            /// </summary>
            KEYSIGN,

            /// <summary>
            /// MIDI Real Time Start message; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            START = 30,
            /// <summary>
            /// MIDI Real Time Continue message; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            CONTINUE,
            /// <summary>
            /// MIDI Real Time Stop message; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            STOP,
            /// <summary>
            /// Set tick queue position; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            SETPOS_TICK,
            /// <summary>
            /// Set real-time queue position; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            SETPOS_TIME,
            /// <summary>
            /// (SMF) Tempo event; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            TEMPO,
            /// <summary>
            /// MIDI Real Time Clock message; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            CLOCK,
            /// <summary>
            /// MIDI Real Time Tick message; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            TICK,
            /// <summary>
            /// Queue timer skew; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            QUEUE_SKEW,
            /// <summary>
            /// Sync position changed; event data type = #snd_seq_ev_queue_control_t
            /// </summary>
            SYNC_POS,

            /// <summary>
            /// Tune request; event data type = none
            /// </summary>
            TUNE_REQUEST = 40,
            /// <summary>
            /// Reset to power-on state; event data type = none
            /// </summary>
            RESET,
            /// <summary>
            /// Active sensing event; event data type = none
            /// </summary>
            SENSING,

            /// <summary>
            /// Echo-back event; event data type = any type
            /// </summary>
            ECHO = 50,
            /// <summary>
            /// OSS emulation raw event; event data type = any type
            /// </summary>
            OSS,

            /// <summary>
            /// New client has connected; event data type = #snd_seq_addr_t
            /// </summary>
            CLIENT_START = 60,
            /// <summary>
            /// Client has left the system; event data type = #snd_seq_addr_t
            /// </summary>
            CLIENT_EXIT,
            /// <summary>
            /// Client status/info has changed; event data type = #snd_seq_addr_t
            /// </summary>
            CLIENT_CHANGE,
            /// <summary>
            /// New port was created; event data type = #snd_seq_addr_t
            /// </summary>
            PORT_START,
            /// <summary>
            /// Port was deleted from system; event data type = #snd_seq_addr_t
            /// </summary>
            PORT_EXIT,
            /// <summary>
            /// Port status/info has changed; event data type = #snd_seq_addr_t
            /// </summary>
            PORT_CHANGE,

            /// <summary>
            /// Ports connected; event data type = #snd_seq_connect_t
            /// </summary>
            PORT_SUBSCRIBED,
            /// <summary>
            /// Ports disconnected; event data type = #snd_seq_connect_t
            /// </summary>
            PORT_UNSUBSCRIBED,

            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR0 = 90,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR1,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR2,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR3,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR4,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR5,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR6,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR7,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR8,
            /// <summary>
            /// user-defined event; event data type = any (fixed size)
            /// </summary>
            USR9,

            /// <summary>
            /// system exclusive data (variable length);  event data type = #snd_seq_ev_ext_t
            /// </summary>
            SYSEX = 130,
            /// <summary>
            /// error event;  event data type = #snd_seq_ev_ext_t
            /// </summary>
            BOUNCE,
            /// <summary>
            /// reserved for user apps;  event data type = #snd_seq_ev_ext_t
            /// </summary>
            USR_VAR0 = 135,
            /// <summary>
            /// reserved for user apps; event data type = #snd_seq_ev_ext_t
            /// </summary>
            USR_VAR1,
            /// <summary>
            /// reserved for user apps; event data type = #snd_seq_ev_ext_t
            /// </summary>
            USR_VAR2,
            /// <summary>
            /// reserved for user apps; event data type = #snd_seq_ev_ext_t
            /// </summary>
            USR_VAR3,
            /// <summary>
            /// reserved for user apps; event data type = #snd_seq_ev_ext_t
            /// </summary>
            USR_VAR4,

            /// <summary>
            /// NOP; ignored in any case
            /// </summary>
            NONE = 255
        };

        /// <summary>
        /// Result events
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_result {
            /// <summary>
            /// processed event type
            /// </summary>
            public int event_;

            /// <summary>
            /// status
            /// </summary>
            public int result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_ev_note {
            /// <summary>
            /// channel number
            /// </summary>
            public byte channel;
            /// <summary>
            /// note
            /// </summary>
            public byte note;
            /// <summary>
            /// velocity
            /// </summary>
            public byte velocity;
            /// <summary>
            /// note-off velocity; only for #SND_SEQ_EVENT_NOTE
            /// </summary>
            public byte off_velocity;
            /// <summary>
            /// duration until note-off; only for #SND_SEQ_EVENT_NOTE
            /// </summary>
            public uint duration;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_ev_ctrl {
            /// <summary>
            /// channel number
            /// </summary>
            public byte channel;
            /// <summary>
            /// reserved
            /// </summary>
            public fixed byte unused[3];
            /// <summary>
            /// control parameter
            /// </summary>
            public uint param;
            /// <summary>
            /// control value
            /// </summary>
            public int value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_ev_raw8 {
            /// <summary>
            /// 8 bit value
            /// </summary>
            public fixed byte d[12];
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_ev_raw32 {
            /// <summary>
            /// 32 bit value
            /// </summary>
            public fixed int d[3];
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_ev_ext {
            /// <summary>
            /// length of data
            /// </summary>
            public uint len;
            /// <summary>
            /// pointer to data (note: can be 64-bit)
            /// </summary>
            public void* ptr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_queue_skew {
            /// <summary>
            /// skew value
            /// </summary>
            public uint value;
            /// <summary>
            /// skew base
            /// </summary>
            public uint base_;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_ev_queue_control {
            /// <summary>
            /// affected queue
            /// </summary>
            public byte queue;
            /// <summary>
            /// reserved
            /// </summary>
            public fixed byte unused[3];

            [StructLayout(LayoutKind.Explicit)]
            public unsafe struct param_union {
                /// <summary>
                /// affected value (e.g. tempo)
                /// </summary>
                [FieldOffset(0)]
                public int value;
                /// <summary>
                /// time
                /// </summary>
                [FieldOffset(0)]
                public snd_seq_timestamp time;
                /// <summary>
                /// sync position
                /// </summary>
                [FieldOffset(0)]
                public uint position;
                /// <summary>
                /// queue skew
                /// </summary>
                [FieldOffset(0)]
                public snd_seq_queue_skew skew;
                /// <summary>
                /// any data
                /// </summary>
                [FieldOffset(0)]
                public fixed uint d32[2];
                /// <summary>
                /// any data
                /// </summary>
                [FieldOffset(0)]
                public fixed byte d8[8];
            }

            /// <summary>
            /// data value union
            /// </summary>
            public param_union param;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_real_time {
            /// <summary>
            /// seconds
            /// </summary>
            public uint tv_sec;
            /// <summary>
            /// nanoseconds
            /// </summary>
            public uint tv_nsec;
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct snd_seq_timestamp {
            /// <summary>
            /// tick-time
            /// </summary>
            [FieldOffset(0)]
            public uint tick;
            /// <summary>
            /// real-time
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_real_time time;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_addr {
            /// <summary>
            /// Client id
            /// </summary>
            public byte client;
            /// <summary>
            /// Port id
            /// </summary>
            public byte port;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_connect {
            /// <summary>
            /// sender address
            /// </summary>
            public snd_seq_addr sender;
            /// <summary>
            /// destination address
            /// </summary>
            public snd_seq_addr dest;
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct snd_seq_event_data {
            /// <summary>
            /// note information
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_ev_note note;
            /// <summary>
            /// MIDI control information
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_ev_ctrl control;
            /// <summary>
            /// raw8 data
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_ev_raw8 raw8;
            /// <summary>
            /// raw32 data
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_ev_raw32 raw32;
            /// <summary>
            /// external data
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_ev_ext ext;
            /// <summary>
            /// queue control
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_ev_queue_control queue;
            /// <summary>
            /// timestamp
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_timestamp time;
            /// <summary>
            /// address
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_addr addr;
            /// <summary>
            /// connect information
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_connect connect;
            /// <summary>
            /// operation result code
            /// </summary>
            [FieldOffset(0)]
            public snd_seq_result result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct snd_seq_event {
            /// <summary>
            /// event type
            /// </summary>
            public snd_seq_event_type type;
            /// <summary>
            /// event flags
            /// </summary>
            public byte flags;
            /// <summary>
            /// tag
            /// </summary>
            public byte tag;
            /// <summary>
            /// schedule queue
            /// </summary>
            public byte queue;
            /// <summary>
            /// schedule time
            /// </summary>
            public snd_seq_timestamp time;
            /// <summary>
            /// source address
            /// </summary>
            public snd_seq_addr source;
            /// <summary>
            /// destination address
            /// </summary>
            public snd_seq_addr dest;
            /// <summary>
            /// event data...
            /// </summary>
            public snd_seq_event_data data;
        }

        private const string alsa_library = "asound";

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_open(nint* handle, string name, int streams, int mode);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_close(nint handle);

        [DllImport(alsa_library)]
        public unsafe static extern void snd_seq_set_client_name(nint seq, string name);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_connect_from(nint seq, int my_port, int src_client, int src_port);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_disconnect_from(nint seq, int my_port, int src_client, int src_port);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_event_input(nint handle, snd_seq_event** ev);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_client_id(nint seq);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_client_info_malloc(nint* ptr);

        [DllImport(alsa_library)]
        public unsafe static extern void snd_seq_client_info_free(nint ptr);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_client_info_get_client(nint info);

        [DllImport(alsa_library)]
        public unsafe static extern void snd_seq_client_info_set_client(nint info, int client);

        [DllImport(alsa_library)]
        public unsafe static extern sbyte* snd_seq_client_info_get_name(nint info);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_query_next_client(nint handle, nint info);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_query_next_port(nint handle, nint info);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_port_info_malloc(nint* ptr);

        [DllImport(alsa_library)]
        public unsafe static extern void snd_seq_port_info_free(nint ptr);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_create_simple_port(nint seq, string name, uint caps, uint type);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_delete_port(nint seq, int port);

        [DllImport(alsa_library)]
        public unsafe static extern void snd_seq_port_info_set_client(nint info, int client);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_seq_port_info_get_port(nint info);

        [DllImport(alsa_library)]
        public unsafe static extern void snd_seq_port_info_set_port(nint info, int port);

        [DllImport(alsa_library)]
        public unsafe static extern sbyte* snd_seq_port_info_get_name(nint info);

        [DllImport(alsa_library)]
        public unsafe static extern uint snd_seq_port_info_get_capability(nint info);
    }
}