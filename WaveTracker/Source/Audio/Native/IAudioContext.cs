using System.Collections.Generic;

namespace WaveTracker.Audio.Native {
    public interface IAudioContext {
        public bool Open(AudioDevice device);
        public List<AudioDevice> EnumerateAudioDevices();

        /// <summary>
        /// Set the sample rate in hertz.
        /// </summary>
        public bool SetSampleRate(int sampleRate);

        /// <summary>
        /// Set the latency in milliseconds.
        /// </summary>
        public void SetLatency(int latency);

        public bool IsAvailable();
        public void Write(float[] buffer);
        public void Close();
    }
}
