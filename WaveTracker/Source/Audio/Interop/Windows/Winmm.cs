// https://github.com/naudio/NAudio/blob/f8568cd4ad20c0683389cc9bc878beb945528047/NAudio.WinMM/MmeInterop/WaveInterop.cs
// https://github.com/zserge/fenster/blob/e700581dfb7956dd161aee44fc0cff0663e789a1/fenster_audio.h#L97-L128

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WaveTracker.Audio.Interop.Windows {
    [SupportedOSPlatform("Windows")]
    internal class Winmm {
        public enum WaveFormatEncoding : ushort {
            Unknown = 0x0000,
            Pcm = 0x0001,
            Adpcm = 0x0002,
            IeeeFloat = 0x0003,
            Vselp = 0x0004,
            IbmCvsd = 0x0005,
            ALaw = 0x0006,
            MuLaw = 0x0007,
            Dts = 0x0008,
            Drm = 0x0009,
            WmaVoice9 = 0x000A,
            OkiAdpcm = 0x0010,
            DviAdpcm = 0x0011,
            ImaAdpcm = DviAdpcm,
            MediaspaceAdpcm = 0x0012,
            SierraAdpcm = 0x0013,
            G723Adpcm = 0x0014,
            DigiStd = 0x0015,
            DigiFix = 0x0016,
            DialogicOkiAdpcm = 0x0017,
            MediaVisionAdpcm = 0x0018,
            CUCodec = 0x0019,
            YamahaAdpcm = 0x0020,
            SonarC = 0x0021,
            DspGroupTrueSpeech = 0x0022,
            EchoSpeechCorporation1 = 0x0023,
            AudioFileAf36 = 0x0024,
            Aptx = 0x0025,
            AudioFileAf10 = 0x0026,
            Prosody1612 = 0x0027,
            Lrc = 0x0028,
            DolbyAc2 = 0x0030,
            Gsm610 = 0x0031,
            MsnAudio = 0x0032,
            AntexAdpcme = 0x0033,
            ControlResVqlpc = 0x0034,
            DigiReal = 0x0035,
            DigiAdpcm = 0x0036,
            ControlResCr10 = 0x0037,
            WAVE_FORMAT_NMS_VBXADPCM = 0x0038,
            WAVE_FORMAT_CS_IMAADPCM = 0x0039,
            WAVE_FORMAT_ECHOSC3 = 0x003A,
            WAVE_FORMAT_ROCKWELL_ADPCM = 0x003B,
            WAVE_FORMAT_ROCKWELL_DIGITALK = 0x003C,
            WAVE_FORMAT_XEBEC = 0x003D,
            WAVE_FORMAT_G721_ADPCM = 0x0040,
            WAVE_FORMAT_G728_CELP = 0x0041,
            WAVE_FORMAT_MSG723 = 0x0042,
            Mpeg = 0x0050,
            WAVE_FORMAT_RT24 = 0x0052,
            WAVE_FORMAT_PAC = 0x0053,
            MpegLayer3 = 0x0055,
            WAVE_FORMAT_LUCENT_G723 = 0x0059,
            WAVE_FORMAT_CIRRUS = 0x0060,
            WAVE_FORMAT_ESPCM = 0x0061,
            WAVE_FORMAT_VOXWARE = 0x0062,
            WAVE_FORMAT_CANOPUS_ATRAC = 0x0063,
            WAVE_FORMAT_G726_ADPCM = 0x0064,
            WAVE_FORMAT_G722_ADPCM = 0x0065,
            WAVE_FORMAT_DSAT_DISPLAY = 0x0067,
            WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 0x0069,
            WAVE_FORMAT_VOXWARE_AC8 = 0x0070,
            WAVE_FORMAT_VOXWARE_AC10 = 0x0071,
            WAVE_FORMAT_VOXWARE_AC16 = 0x0072,
            WAVE_FORMAT_VOXWARE_AC20 = 0x0073,
            WAVE_FORMAT_VOXWARE_RT24 = 0x0074,
            WAVE_FORMAT_VOXWARE_RT29 = 0x0075,
            WAVE_FORMAT_VOXWARE_RT29HW = 0x0076,
            WAVE_FORMAT_VOXWARE_VR12 = 0x0077,
            WAVE_FORMAT_VOXWARE_VR18 = 0x0078,
            WAVE_FORMAT_VOXWARE_TQ40 = 0x0079,
            WAVE_FORMAT_SOFTSOUND = 0x0080,
            WAVE_FORMAT_VOXWARE_TQ60 = 0x0081,
            WAVE_FORMAT_MSRT24 = 0x0082,
            WAVE_FORMAT_G729A = 0x0083,
            WAVE_FORMAT_MVI_MVI2 = 0x0084,
            WAVE_FORMAT_DF_G726 = 0x0085,
            WAVE_FORMAT_DF_GSM610 = 0x0086,
            WAVE_FORMAT_ISIAUDIO = 0x0088,
            WAVE_FORMAT_ONLIVE = 0x0089,
            WAVE_FORMAT_SBC24 = 0x0091,
            WAVE_FORMAT_DOLBY_AC3_SPDIF = 0x0092,
            WAVE_FORMAT_MEDIASONIC_G723 = 0x0093,
            WAVE_FORMAT_PROSODY_8KBPS = 0x0094,
            WAVE_FORMAT_ZYXEL_ADPCM = 0x0097,
            WAVE_FORMAT_PHILIPS_LPCBB = 0x0098,
            WAVE_FORMAT_PACKED = 0x0099,
            WAVE_FORMAT_MALDEN_PHONYTALK = 0x00A0,
            Gsm = 0x00A1,
            G729 = 0x00A2,
            G723 = 0x00A3,
            Acelp = 0x00A4,
            RawAac = 0x00FF,
            WAVE_FORMAT_RHETOREX_ADPCM = 0x0100,
            WAVE_FORMAT_IRAT = 0x0101,
            WAVE_FORMAT_VIVO_G723 = 0x0111,
            WAVE_FORMAT_VIVO_SIREN = 0x0112,
            WAVE_FORMAT_DIGITAL_G723 = 0x0123,
            WAVE_FORMAT_SANYO_LD_ADPCM = 0x0125,
            WAVE_FORMAT_SIPROLAB_ACEPLNET = 0x0130,
            WAVE_FORMAT_SIPROLAB_ACELP4800 = 0x0131,
            WAVE_FORMAT_SIPROLAB_ACELP8V3 = 0x0132,
            WAVE_FORMAT_SIPROLAB_G729 = 0x0133,
            WAVE_FORMAT_SIPROLAB_G729A = 0x0134,
            WAVE_FORMAT_SIPROLAB_KELVIN = 0x0135,
            WAVE_FORMAT_G726ADPCM = 0x0140,
            WAVE_FORMAT_QUALCOMM_PUREVOICE = 0x0150,
            WAVE_FORMAT_QUALCOMM_HALFRATE = 0x0151,
            WAVE_FORMAT_TUBGSM = 0x0155,
            WAVE_FORMAT_MSAUDIO1 = 0x0160,
            WindowsMediaAudio = 0x0161,
            WindowsMediaAudioProfessional = 0x0162,
            WindowsMediaAudioLosseless = 0x0163,
            WindowsMediaAudioSpdif = 0x0164,
            WAVE_FORMAT_UNISYS_NAP_ADPCM = 0x0170,
            WAVE_FORMAT_UNISYS_NAP_ULAW = 0x0171,
            WAVE_FORMAT_UNISYS_NAP_ALAW = 0x0172,
            WAVE_FORMAT_UNISYS_NAP_16K = 0x0173,
            WAVE_FORMAT_CREATIVE_ADPCM = 0x0200,
            WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 0x0202,
            WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 0x0203,
            WAVE_FORMAT_UHER_ADPCM = 0x0210,
            WAVE_FORMAT_QUARTERDECK = 0x0220,
            WAVE_FORMAT_ILINK_VC = 0x0230,
            WAVE_FORMAT_RAW_SPORT = 0x0240,
            WAVE_FORMAT_ESST_AC3 = 0x0241,
            WAVE_FORMAT_IPI_HSX = 0x0250,
            WAVE_FORMAT_IPI_RPELP = 0x0251,
            WAVE_FORMAT_CS2 = 0x0260,
            WAVE_FORMAT_SONY_SCX = 0x0270,
            WAVE_FORMAT_FM_TOWNS_SND = 0x0300,
            WAVE_FORMAT_BTV_DIGITAL = 0x0400,
            WAVE_FORMAT_QDESIGN_MUSIC = 0x0450,
            WAVE_FORMAT_VME_VMPCM = 0x0680,
            WAVE_FORMAT_TPC = 0x0681,
            WAVE_FORMAT_OLIGSM = 0x1000,
            WAVE_FORMAT_OLIADPCM = 0x1001,
            WAVE_FORMAT_OLICELP = 0x1002,
            WAVE_FORMAT_OLISBC = 0x1003,
            WAVE_FORMAT_OLIOPR = 0x1004,
            WAVE_FORMAT_LH_CODEC = 0x1100,
            WAVE_FORMAT_NORRIS = 0x1400,
            WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 0x1500,
            MPEG_ADTS_AAC = 0x1600,
            MPEG_RAW_AAC = 0x1601,
            MPEG_LOAS = 0x1602,
            NOKIA_MPEG_ADTS_AAC = 0x1608,
            NOKIA_MPEG_RAW_AAC = 0x1609,
            VODAFONE_MPEG_ADTS_AAC = 0x160A,
            VODAFONE_MPEG_RAW_AAC = 0x160B,
            MPEG_HEAAC = 0x1610,
            WAVE_FORMAT_DVM = 0x2000,
            Vorbis1 = 0x674f,
            Vorbis2 = 0x6750,
            Vorbis3 = 0x6751,
            Vorbis1P = 0x676f,
            Vorbis2P = 0x6770,
            Vorbis3P = 0x6771,
            Extensible = 0xFFFE,
            WAVE_FORMAT_DEVELOPMENT = 0xFFFF,
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WaveFormatEx {
            public WaveFormatEncoding wFormatTag;
            public short nChannels;
            public int nSamplesPerSec;
            public int nAvgBytesPerSec;
            public short nBlockAlign;
            public short wBitsPerSample;
            public short cbSize;
        }

        public enum WaveMessage {
            WaveInOpen = 0x3BE,
            WaveInClose = 0x3BF,
            WaveInData = 0x3C0,
            WaveOutClose = 0x3BC,
            WaveOutDone = 0x3BD,
            WaveOutOpen = 0x3BB,
        }

        [Flags]
        public enum WaveHeaderFlags {
            BeginLoop = 0x00000004,
            Done = 0x00000001,
            EndLoop = 0x00000008,
            InQueue = 0x00000010,
            Prepared = 0x00000002,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WaveHeader {
            public nint dataBuffer;
            public int bufferLength;
            public int bytesRecorded;
            public nint userData;
            public WaveHeaderFlags flags;
            public int loops;
            public nint next;
            public nint reserved;
        }

        [Flags]
        public enum WaveInOutOpenFlags {
            CallbackNull = 0,
            CallbackFunction = 0x30000,
            CallbackEvent = 0x50000,
            CallbackWindow = 0x10000,
            CallbackThread = 0x20000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WaveOutCaps {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            public fixed sbyte szPname[32];
            public uint dwFormats;
            public short wChannels;
            public short wReserved1;
            public int dwSupport;
        }

        public delegate void WaveCallback(nint hWaveOut, WaveMessage message, nint dwInstance, ref WaveHeader wavhdr, nint dwReserved);

        [DllImport("winmm")]
        public static extern MmResult waveOutOpen(out nint hWaveOut, uint uDeviceID, ref WaveFormatEx lpFormat, WaveCallback dwCallback, nint dwInstance, WaveInOutOpenFlags dwFlags);

        [DllImport("winmm")]
        public static extern uint waveOutGetNumDevs();

        [DllImport("winmm")]
        public static extern MmResult waveOutGetDevCaps(uint uDeviceNumber, ref WaveOutCaps pwoc, uint cbwoc);

        [DllImport("winmm")]
        public static extern MmResult waveOutPrepareHeader(nint hWaveOut, ref WaveHeader lpWaveOutHdr, int uSize);

        [DllImport("winmm")]
        public static extern MmResult waveOutUnprepareHeader(nint hWaveOut, ref WaveHeader lpWaveOutHdr, int uSize);

        [DllImport("winmm")]
        public static extern MmResult waveOutWrite(nint hWaveOut, ref WaveHeader lpWaveOutHdr, int uSize);

        [DllImport("winmm")]
        public static extern MmResult waveOutClose(nint hWaveOut);
    }
}