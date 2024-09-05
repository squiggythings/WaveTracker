using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WaveTracker.Audio.Native {
    // [SupportedOSPlatform("Linux")]
    internal static class Alsa {
        private const string alsa_library = "asound";

        public enum snd_pcm_stream_t : int {
            /// Playback stream
            PLAYBACK = 0,
            /// Capture stream
            CAPTURE,
        }

        public enum snd_pcm_access_t {
            /// mmap access with simple interleaved channels
            MMAP_INTERLEAVED = 0,
            /// mmap access with simple non interleaved channels
            MMAP_NONINTERLEAVED,
            /// mmap access with complex placement
            MMAP_COMPLEX,
            /// snd_pcm_readi/snd_pcm_writei access
            RW_INTERLEAVED,
            /// snd_pcm_readn/snd_pcm_writen access
            RW_NONINTERLEAVED,
        };

        public enum snd_pcm_format_t {
            /// Unknown
            UNKNOWN = -1,
            /// Signed 8 bit
            S8 = 0,
            /// Unsigned 8 bit
            U8,
            /// Signed 16 bit Little Endian
            S16_LE,
            /// Signed 16 bit Big Endian
            S16_BE,
            /// Unsigned 16 bit Little Endian
            U16_LE,
            /// Unsigned 16 bit Big Endian
            U16_BE,
            /// Signed 24 bit Little Endian using low three bytes in 32-bit word
            S24_LE,
            /// Signed 24 bit Big Endian using low three bytes in 32-bit word
            S24_BE,
            /// Unsigned 24 bit Little Endian using low three bytes in 32-bit word
            U24_LE,
            /// Unsigned 24 bit Big Endian using low three bytes in 32-bit word
            U24_BE,
            /// Signed 32 bit Little Endian
            S32_LE,
            /// Signed 32 bit Big Endian
            S32_BE,
            /// Unsigned 32 bit Little Endian
            U32_LE,
            /// Unsigned 32 bit Big Endian
            U32_BE,
            /// Float 32 bit Little Endian, Range -1.0 to 1.0
            FLOAT_LE,
            /// Float 32 bit Big Endian, Range -1.0 to 1.0
            FLOAT_BE,
            /// Float 64 bit Little Endian, Range -1.0 to 1.0
            FLOAT64_LE,
            /// Float 64 bit Big Endian, Range -1.0 to 1.0
            FLOAT64_BE,
            /// IEC-958 Little Endian
            IEC958_SUBFRAME_LE,
            /// IEC-958 Big Endian
            IEC958_SUBFRAME_BE,
            /// Mu-Law
            MU_LAW,
            /// A-Law
            A_LAW,
            /// Ima-ADPCM
            IMA_ADPCM,
            /// MPEG
            MPEG,
            /// GSM
            GSM,
            /// Signed 20bit Little Endian in 4bytes format, LSB justified
            S20_LE,
            /// Signed 20bit Big Endian in 4bytes format, LSB justified
            S20_BE,
            /// Unsigned 20bit Little Endian in 4bytes format, LSB justified
            U20_LE,
            /// Unsigned 20bit Big Endian in 4bytes format, LSB justified
            U20_BE,
            /// Special
            SPECIAL = 31,
            /// Signed 24bit Little Endian in 3bytes format
            S24_3LE = 32,
            /// Signed 24bit Big Endian in 3bytes format
            S24_3BE,
            /// Unsigned 24bit Little Endian in 3bytes format
            U24_3LE,
            /// Unsigned 24bit Big Endian in 3bytes format
            U24_3BE,
            /// Signed 20bit Little Endian in 3bytes format
            S20_3LE,
            /// Signed 20bit Big Endian in 3bytes format
            S20_3BE,
            /// Unsigned 20bit Little Endian in 3bytes format
            U20_3LE,
            /// Unsigned 20bit Big Endian in 3bytes format
            U20_3BE,
            /// Signed 18bit Little Endian in 3bytes format
            S18_3LE,
            /// Signed 18bit Big Endian in 3bytes format
            S18_3BE,
            /// Unsigned 18bit Little Endian in 3bytes format
            U18_3LE,
            /// Unsigned 18bit Big Endian in 3bytes format
            U18_3BE,
            /// G.723 (ADPCM) 24 kbit/s, 8 samples in 3 bytes
            G723_24,
            /// G.723 (ADPCM) 24 kbit/s, 1 sample in 1 byte
            G723_24_1B,
            /// G.723 (ADPCM) 40 kbit/s, 8 samples in 3 bytes
            G723_40,
            /// G.723 (ADPCM) 40 kbit/s, 1 sample in 1 byte
            G723_40_1B,
            /// Direct Stream Digital (DSD) in 1-byte samples (x8)
            DSD_U8,
            /// Direct Stream Digital (DSD) in 2-byte samples (x16)
            DSD_U16_LE,
            /// Direct Stream Digital (DSD) in 4-byte samples (x32)
            DSD_U32_LE,
            /// Direct Stream Digital (DSD) in 2-byte samples (x16)
            DSD_U16_BE,
            /// Direct Stream Digital (DSD) in 4-byte samples (x32)
            DSD_U32_BE,
        };

        [DllImport(alsa_library)]
        public unsafe static extern int snd_pcm_open(nint* pcm, string name, snd_pcm_stream_t stream, int mode);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_pcm_set_params(nint pcm, snd_pcm_format_t format, snd_pcm_access_t access, uint channels, uint rate, int soft_resample, uint latency);

        [DllImport(alsa_library)]
        public unsafe static extern long snd_pcm_avail(nint pcm);

        [DllImport(alsa_library)]
        public unsafe static extern long snd_pcm_writei(nint pcm, void* buffer, ulong size);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_pcm_recover(nint pcm, int err, int silent);

        [DllImport(alsa_library)]
        public unsafe static extern int snd_pcm_close(nint pcm);
    }

    // [SupportedOSPlatform("Linux")]
    internal class AudioLinuxContext {
        private nint _pcm = 0;
        public Alsa.snd_pcm_format_t sample_format = Alsa.snd_pcm_format_t.FLOAT_LE;
        public Alsa.snd_pcm_access_t access = Alsa.snd_pcm_access_t.RW_INTERLEAVED;
        public uint channels = 2;
        public uint sample_rate = 48000;
        public bool soft_resample = true;

        /// required overall latency in us 
        public uint latency = 10_000;

        public bool Open() {
            nint pcm;
            unsafe {
                int status = Alsa.snd_pcm_open(&pcm, "default", Alsa.snd_pcm_stream_t.PLAYBACK, 0);
                if (status != 0)
                    return false;

                _pcm = pcm;

                return Alsa.snd_pcm_set_params(pcm, sample_format, access, channels,
                                          sample_rate, soft_resample ? 1 : 0, latency) == 0;
            }
        }

        public bool IsAvailable() {
            long frame_count = Alsa.snd_pcm_avail(_pcm);
            if (frame_count < 0)
                Alsa.snd_pcm_recover(_pcm, (int)frame_count, 0);
            return frame_count > 0;
        }

        public void Write(float[] buffer) {
            unsafe {
                fixed (float* bufferPtr = buffer) {
                    long frame_count = Alsa.snd_pcm_writei(_pcm, bufferPtr, (ulong)buffer.Length / channels);
                    if (frame_count < 0)
                        Alsa.snd_pcm_recover(_pcm, (int)frame_count, 0);
                }
            }
        }

        public void Close() {
            Alsa.snd_pcm_close(_pcm);
        }
    };
}
