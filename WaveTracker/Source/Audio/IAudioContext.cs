using System.Collections.Generic;

namespace WaveTracker.Audio {
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

        public int AvailableFrames();
        public void Write(float[] buffer);
        public void Close();
    }
}
