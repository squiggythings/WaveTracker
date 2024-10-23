using System;
using System.IO;

namespace WaveTracker.Audio.Native {

    public interface IWaveStream {
        public int NumChannels { get; }
        public int SampleRate { get; }
        public TimeSpan Duration { get; }

        /// <summary>
        /// Read 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public int ReadSamples(float[] buffer, int sampleCount);
    }

    public class WaveStream : MemoryStream, IWaveStream {
        public readonly Wav SourceWav;

        private byte[] byteBuffer = new byte[4096];
        private short[] pcm16Buffer = new short[2048];

        public WaveStream(Wav wav) : base(wav.SoundData) {
            SourceWav = wav;
        }

        public int NumChannels => SourceWav.NumChannels;
        public int SampleRate => (int)SourceWav.SampleRate;
        public TimeSpan Duration => SourceWav.Duration;

        public int ReadSamples(float[] buffer, int sampleCount) {
            int byteCount = sampleCount * SourceWav.BytesPerSample;
            if (byteBuffer.Length < byteCount)
                byteBuffer = new byte[byteCount];

            int bytesRead = Read(byteBuffer, 0, byteCount);
            int samplesRead = Wav.ReadPCM16Samples(byteBuffer[..bytesRead], pcm16Buffer);

            Wav.ReadPCM16ToFloat(pcm16Buffer[..samplesRead], buffer);
            return samplesRead;
        }
    }

    public enum WavFormat : ushort {
        /// PCM integer
        PCMInteger = 1,

        // /// IEEE 754 float
        // IEEEFloat = 3,
        // /// 8-bit ITU-T G.711 A-law
        // Alaw = 6,
        // /// 8-bit ITU-T G.711 µ-law
        // Mulaw = 7,
    }

    public class WavFormatException : Exception {
        public WavFormatException(string message) : base(message) { }
    }

    public class Wav {
        public readonly WavFormat Format;
        public readonly ushort NumChannels;
        public readonly uint SampleRate;
        public readonly uint ByteRate;
        public readonly ushort BlockAlign;
        public readonly ushort BitsPerSample;
        public readonly byte[] SoundData;

        public TimeSpan Duration { get; init; }
        public int BytesPerSample => BitsPerSample / 8;

        private static readonly char[] MAGIC_RIFF = ['R', 'I', 'F', 'F'];
        private static readonly char[] MAGIC_WAVE = ['W', 'A', 'V', 'E'];
        private static readonly char[] MAGIC_FMT_ = ['f', 'm', 't', ' '];
        private static readonly char[] MAGIC_DATA = ['d', 'a', 't', 'a'];

        public Wav(short[] pcm16Samples, ushort numChannels, uint sampleRate) {
            Format = WavFormat.PCMInteger;
            NumChannels = numChannels;
            SampleRate = sampleRate;
            ByteRate = sampleRate * numChannels * 2;
            BlockAlign = (ushort)(numChannels * 2);
            BitsPerSample = 16;

            SoundData = new byte[pcm16Samples.Length * 2];
            Buffer.BlockCopy(pcm16Samples, 0, SoundData, 0, SoundData.Length);
        }

        public void Write(Stream stream) {
            BinaryWriter writer = new BinaryWriter(stream);

            uint fmtSubchunkSize = 16;
            uint dataSubchunkSize = (uint)SoundData.Length;
            uint chunkSize = 20 + fmtSubchunkSize + dataSubchunkSize;

            //////////////////
            // RIFF header

            writer.Write(MAGIC_RIFF);
            writer.Write(chunkSize);
            writer.Write(MAGIC_WAVE);

            ///////////////////
            // fmt subchunk

            writer.Write(MAGIC_FMT_);
            writer.Write(fmtSubchunkSize);
            writer.Write((ushort)Format);
            writer.Write(NumChannels);
            writer.Write(SampleRate);
            writer.Write(ByteRate);
            writer.Write(BlockAlign);
            writer.Write(BitsPerSample);

            ////////////////////
            // data subchunk

            writer.Write(MAGIC_DATA);
            writer.Write(dataSubchunkSize);
            writer.Write(SoundData);
        }

        public static int ReadPCM16Samples(byte[] soundData, short[] outPCM16Samples) {
            BinaryReader reader = new BinaryReader(new MemoryStream(soundData));

            int samplesRead = Math.Min(outPCM16Samples.Length, soundData.Length / 2);
            for (int i = 0; i < samplesRead; i++)
                outPCM16Samples[i] = reader.ReadInt16();

            return samplesRead;
        }

        public static int ReadPCM16ToFloat(short[] pcm16Samples, float[] outFloatSamples) {
            int samplesRead = Math.Min(pcm16Samples.Length, outFloatSamples.Length);

            for (int i = 0; i < samplesRead; i++)
                outFloatSamples[i] = (float)pcm16Samples[i] / (1 << 15);

            return samplesRead;
        }

        public static short[] FloatToPCM16(float[] floatSamples) {
            short[] pcm16Samples = new short[floatSamples.Length];

            for (int i = 0; i < floatSamples.Length; i++)
                pcm16Samples[i] = (short)(floatSamples[i] * (1 << 15));

            return pcm16Samples;
        }
    }
}
