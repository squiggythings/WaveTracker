using System.Collections.Generic;

namespace WaveTracker.Audio.Native {
    public interface IAudioContext {
        public bool Open(AudioDevice device);
        public List<AudioDevice> EnumerateAudioDevices();

        public bool SetSampleRate(int sampleRate);

        public bool IsAvailable();
        public void Write(float[] buffer);
        public void Close();
    }
}
