using System;
using System.IO;
using System.Linq;

namespace WaveTracker.Audio.Native {
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
        public readonly uint AvgByteRate;
        public readonly ushort BlockAlign;
        public readonly ushort BitsPerSample;
        public readonly byte[] SoundData;

        private static readonly char[] MAGIC_RIFF = ['R', 'I', 'F', 'F'];
        private static readonly char[] MAGIC_WAVE = ['W', 'A', 'V', 'E'];
        private static readonly char[] MAGIC_FMT_ = ['f', 'm', 't', ' '];
        private static readonly char[] MAGIC_DATA = ['d', 'a', 't', 'a'];

        public Wav(Stream stream) {
            BinaryReader reader = new BinaryReader(stream);

            //////////////////
            // RIFF header

            char[] chunkID = reader.ReadChars(4);
            if (!chunkID.SequenceEqual(MAGIC_RIFF))
                throw new WavFormatException("not a wav file");

            // chunk size
            reader.ReadUInt32();

            char[] format = reader.ReadChars(4);
            if (!format.SequenceEqual(MAGIC_WAVE))
                throw new WavFormatException("not a wav file");

            ///////////////////
            // fmt subchunk

            char[] subchunk1ID = reader.ReadChars(4);
            if (!subchunk1ID.SequenceEqual(MAGIC_FMT_))
                throw new WavFormatException($"wav file is corrupted (unrecognized chunk ID: {subchunk1ID})");

            // subchunk 1 size
            reader.ReadUInt32();

            ushort wavFormat = reader.ReadUInt16();
            switch (wavFormat) {
                case 1:
                    Format = WavFormat.PCMInteger;
                    break;
                case 3:
                    throw new WavFormatException("wav files with IEEE 754 float samples are unsupported");
                case 6:
                    throw new WavFormatException("wav files with 8-bit ITU-T G.711 A-law samples are unsupported");
                case 7:
                    throw new WavFormatException("wav files with 8-bit ITU-T G.711 µ-law samples are unsupported");
                case 0xFFFE:
                    throw new WavFormatException("wav files with an extensible sample format are not supported");
                default:
                    throw new WavFormatException($"wav file is corrupted (unrecognized sample format {wavFormat})");
            }

            NumChannels = reader.ReadUInt16();
            SampleRate = reader.ReadUInt32();
            AvgByteRate = reader.ReadUInt32();
            BlockAlign = reader.ReadUInt16();
            BitsPerSample = reader.ReadUInt16();

            ////////////////////
            // data subchunk

            char[] subchunk2ID = reader.ReadChars(4);
            if (!subchunk2ID.SequenceEqual(MAGIC_DATA))
                throw new WavFormatException($"wav file is corrupted (unrecognized chunk ID: {subchunk2ID})");

            uint subchunk2Size = reader.ReadUInt32();

            SoundData = reader.ReadBytes((int)subchunk2Size);
        }

        public Wav(short[] pcm16Samples, ushort numChannels, uint sampleRate) {
            Format = WavFormat.PCMInteger;
            NumChannels = numChannels;
            SampleRate = sampleRate;
            AvgByteRate = sampleRate * numChannels * 2;
            BlockAlign = (ushort)(numChannels * 2);
            BitsPerSample = 16;

            SoundData = new byte[pcm16Samples.Length * 2];
            Buffer.BlockCopy(pcm16Samples, 0, SoundData, 0, SoundData.Length);
        }

        public Wav(int[] pcm32Samples, ushort numChannels, uint sampleRate) {
            Format = WavFormat.PCMInteger;
            NumChannels = numChannels;
            SampleRate = sampleRate;
            AvgByteRate = sampleRate * numChannels * 4;
            BlockAlign = (ushort)(numChannels * 4);
            BitsPerSample = 32;

            SoundData = new byte[pcm32Samples.Length * 4];
            Buffer.BlockCopy(pcm32Samples, 0, SoundData, 0, SoundData.Length);
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
            writer.Write(AvgByteRate);
            writer.Write(BlockAlign);
            writer.Write(BitsPerSample);

            ////////////////////
            // data subchunk

            writer.Write(MAGIC_DATA);
            writer.Write(dataSubchunkSize);
            writer.Write(SoundData);
        }

        public short[] GetPCM16Samples() {
            if (Format != WavFormat.PCMInteger || BitsPerSample != 16)
                throw new WavFormatException("wav sample format is not PCM16");

            BinaryReader reader = new BinaryReader(new MemoryStream(SoundData));

            short[] pcm16Samples = new short[SoundData.Length / 2];
            for (int i = 0; i < pcm16Samples.Length; i++)
                pcm16Samples[i] = reader.ReadInt16();

            return pcm16Samples;
        }

        public int[] GetPCM32Samples() {
            if (Format != WavFormat.PCMInteger || BitsPerSample != 32)
                throw new WavFormatException("wav sample format is not PCM32");

            BinaryReader reader = new BinaryReader(new MemoryStream(SoundData));

            int[] pcm32Samples = new int[SoundData.Length / 4];
            for (int i = 0; i < pcm32Samples.Length; i++)
                pcm32Samples[i] = reader.ReadInt32();

            return pcm32Samples;
        }

        public float[] GetSamplesAsFloat() {
            if (Format == WavFormat.PCMInteger) {
                if (BitsPerSample == 16)
                    return PCM16ToFloat(GetPCM16Samples());
                else if (BitsPerSample == 32)
                    return PCM32ToFloat(GetPCM32Samples());
            }

            throw new WavFormatException("wav sample format is not PCM16 or PCM32");
        }

        public static float[] PCM16ToFloat(short[] pcm16Samples) {
            float[] floatSamples = new float[pcm16Samples.Length];

            for (int i = 0; i < pcm16Samples.Length; i++)
                floatSamples[i] = (float)pcm16Samples[i] / (1 << 15);

            return floatSamples;
        }

        public static float[] PCM32ToFloat(int[] pcm32Samples) {
            float[] floatSamples = new float[pcm32Samples.Length];

            for (int i = 0; i < pcm32Samples.Length; i++)
                floatSamples[i] = (float)pcm32Samples[i] / (1 << 31);

            return floatSamples;
        }

        public static short[] FloatToPCM16(float[] floatSamples) {
            short[] pcm16Samples = new short[floatSamples.Length];

            for (int i = 0; i < floatSamples.Length; i++)
                pcm16Samples[i] = (short)(floatSamples[i] * (1 << 15));

            return pcm16Samples;
        }

        public static int[] FloatToPCM32(float[] floatSamples) {
            int[] pcm32Samples = new int[floatSamples.Length];

            for (int i = 0; i < floatSamples.Length; i++)
                pcm32Samples[i] = (short)(floatSamples[i] * (1 << 31));

            return pcm32Samples;
        }

        internal static void PlayFloatSamples(AudioLinuxContext audioCtx, float[] samples) {
            const int advance = 480;
            for (int i = 0; i < samples.Length; i += advance) {
                float[] currSamples = samples[i..Math.Min(i + advance, samples.Length)];
                audioCtx.Write(currSamples);
            }
        }
    }
}
