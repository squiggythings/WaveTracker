using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using WaveTracker.Audio.Native;

namespace WaveTracker.Audio {
    public class AudioReader : IWaveStream, IDisposable {
        private unsafe AVFormatContext* formatCtx;
        private unsafe AVCodecContext* codecCtx;
        private unsafe SwrContext* swrCtx;
        private unsafe AVFrame* frame;
        private unsafe AVPacket* packet;

        private readonly int audioStreamIdx;

        private bool disposed = false;

        private readonly int numChannels;
        private readonly int sampleRate;
        private readonly TimeSpan duration;

        private readonly Queue<float> frameSampleBuffer;

        public int NumChannels => numChannels;
        public int SampleRate => sampleRate;
        public TimeSpan Duration => duration;

        public bool LoopingEnabled { get; set; } = false;

        public AudioReader(string filepath, bool loopingEnabled = false) {
            unsafe {
                // open audio file
                AVFormatContext* formatCtx = null;
                if (ffmpeg.avformat_open_input(&formatCtx, filepath, null, null) < 0)
                    throw new Exception("Failed to read audio file");

                if (ffmpeg.avformat_find_stream_info(formatCtx, null) < 0)
                    throw new Exception("Failed to read audio file: cannot find stream info");

                // get audio stream
                audioStreamIdx = -1;
                for (int i = 0; i < formatCtx->nb_streams; i++) {
                    if (formatCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO) {
                        audioStreamIdx = i;
                        break;
                    }
                }
                if (audioStreamIdx == -1)
                    throw new Exception("Failed to read audio file: no audio stream");

                // get decoder for this file
                AVStream* stream = formatCtx->streams[audioStreamIdx];
                AVCodecParameters* codecParams = stream->codecpar;
                AVCodec* codec = ffmpeg.avcodec_find_decoder(codecParams->codec_id);
                if (codec == null)
                    throw new Exception("Failed to read audio file: unsupported codec");

                AVCodecContext* codecCtx = ffmpeg.avcodec_alloc_context3(codec);
                if (ffmpeg.avcodec_parameters_to_context(codecCtx, codecParams) < 0)
                    throw new Exception("Failed to read audio file: cannot copy codec parameters to decoder context");

                if (ffmpeg.avcodec_open2(codecCtx, codec, null) < 0)
                    throw new Exception("Failed to read audio file: cannot open codec");

                if (codecCtx->channel_layout == 0) {
                    // Fallback to default channel layout if not set
                    codecCtx->channel_layout = (ulong)ffmpeg.av_get_default_channel_layout(codecCtx->channels);
                }

                // create resampling context
                SwrContext* swrCtx = ffmpeg.swr_alloc_set_opts(
                    s: null,
                    out_ch_layout: (long)codecCtx->channel_layout,
                    out_sample_fmt: AVSampleFormat.AV_SAMPLE_FMT_FLT,
                    out_sample_rate: codecCtx->sample_rate,
                    in_ch_layout: (long)codecCtx->channel_layout,
                    in_sample_fmt: codecCtx->sample_fmt,
                    in_sample_rate: codecCtx->sample_rate,
                    log_offset: 0,
                    log_ctx: null
                );

                if (ffmpeg.swr_init(swrCtx) < 0)
                    throw new Exception("Failed to read audio file: cannot initialize resampling context");

                this.formatCtx = formatCtx;
                this.codecCtx = codecCtx;
                this.swrCtx = swrCtx;
                this.frame = ffmpeg.av_frame_alloc();
                this.packet = ffmpeg.av_packet_alloc();

                numChannels = codecCtx->channels;
                sampleRate = codecCtx->sample_rate;

                frameSampleBuffer = [];
                LoopingEnabled = loopingEnabled;
                duration = TimeSpan.FromMicroseconds(formatCtx->duration);
            }
        }

        public int ReadSamples(float[] buffer, int sampleCount) {
            while (frameSampleBuffer.Count < sampleCount) {
                if (!DecodeFrame(frameSampleBuffer))
                    break;
            }

            int numSamples = Math.Min(sampleCount, frameSampleBuffer.Count);

            for (int i = 0; i < numSamples; i++)
                buffer[i] = frameSampleBuffer.Dequeue();

            return numSamples;
        }

        /// <summary>
        /// Decodes one audio frame and fills out a growable sample buffer.
        /// </summary>
        /// <param name="frame">A pre-allocated FFmpeg frame</param>
        /// <param name="packet">A pre-allocated FFmpeg packet</param>
        /// <param name="sampleBuffer">Growable sample buffer to fill out</param>
        /// <returns>Whether a frame was decoded</returns>
        public bool DecodeFrame(Queue<float> sampleBuffer) {
            unsafe {
                bool isFrameAvailable = ffmpeg.av_read_frame(formatCtx, packet) >= 0;
                if (!isFrameAvailable && LoopingEnabled) {
                    SeekToStart();
                    isFrameAvailable = ffmpeg.av_read_frame(formatCtx, packet) >= 0;
                }

                if (isFrameAvailable) {
                    if (packet->stream_index == audioStreamIdx) {
                        if (ffmpeg.avcodec_send_packet(codecCtx, packet) == 0) {
                            while (ffmpeg.avcodec_receive_frame(codecCtx, frame) == 0) {
                                // Resample audio to float format if necessary
                                byte** convertedData = null;
                                ffmpeg.av_samples_alloc_array_and_samples(&convertedData, null, codecCtx->channels, frame->nb_samples, AVSampleFormat.AV_SAMPLE_FMT_FLT, 0);

                                int numConvertedSamples = ffmpeg.swr_convert(swrCtx, convertedData, frame->nb_samples, frame->extended_data, frame->nb_samples) * codecCtx->channels;

                                // Write audio samples to sample buffer
                                ReadOnlySpan<float> convertedSamples = new ReadOnlySpan<float>(convertedData[0], numConvertedSamples);
                                foreach (float convertedSample in convertedSamples)
                                    sampleBuffer.Enqueue(convertedSample);

                                ffmpeg.av_freep(&convertedData[0]);
                                ffmpeg.av_freep(&convertedData);
                            }
                        }
                    }
                    ffmpeg.av_packet_unref(packet);

                    return true;
                }

                return false;
            }
        }

        public void SeekToStart() {
            unsafe {
                ffmpeg.av_seek_frame(formatCtx, audioStreamIdx, 0, ffmpeg.AVSEEK_FLAG_FRAME);
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                unsafe {
                    AVFormatContext* formatCtx = this.formatCtx;
                    AVCodecContext* codecCtx = this.codecCtx;
                    SwrContext* swrCtx = this.swrCtx;
                    AVFrame* frame = this.frame;
                    AVPacket* packet = this.packet;

                    ffmpeg.swr_free(&swrCtx);
                    ffmpeg.avcodec_free_context(&codecCtx);
                    ffmpeg.avformat_close_input(&formatCtx);
                    ffmpeg.av_frame_free(&frame);
                    ffmpeg.av_packet_free(&packet);

                    this.formatCtx = null;
                    this.codecCtx = null;
                    this.swrCtx = null;
                    this.frame = null;
                    this.packet = null;
                }
            }

            disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AudioReader() {
            Dispose(false);
        }
    }
}