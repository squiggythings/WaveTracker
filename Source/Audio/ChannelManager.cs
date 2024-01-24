using System.Collections.Generic;
using WaveTracker.Tracker;
using WaveTracker.UI;

namespace WaveTracker.Audio {
    public static class ChannelManager {
        public static Channel previewChannel;
        public static List<Channel> channels;
        public static WaveBank waveBank;

        public static void Initialize(int numChannels, WaveBank waveBank) {
            ChannelManager.waveBank = waveBank;
            previewChannel = new Channel(-1);
            channels = new List<Channel>();
            for (int i = 0; i < numChannels; i++) {
                channels.Add(new Channel(i));
            }
        }


        public static void Reset() {
            foreach (Channel channel in channels) {
                channel.Reset();
            }
        }

        public static void ResetTicks(int num) {
            //foreach (Channel channel in channels)
            //{
            //    channel.ResetTick(num);
            //}
        }

        public static void PlayRow(short[] row) {
            int channelNum = 0;
            for (int i = 0; i < row.Length; i += 5) {
                int note = row[i];
                int instrument = row[i + 1];
                int volume = row[i + 2];
                int effect = row[i + 3];
                int parameter = row[i + 4];

                int delay = 0;
                if (effect == 17)
                    delay = parameter + 1;
                else if (effect == 20) // CXX
                {
                    Playback.StopNext();
                }
                else if (effect == 21) // BXX
                  {
                    Playback.GotoNext(parameter % FrameEditor.thisSong.frames.Count, 0);
                }
                else if (effect == 22) // DXX
                  {
                    Playback.GotoNext(Playback.playbackFrame + 1, parameter);
                }
                else if (effect == 15) // FXX
                  {
                    Playback.ticksPerRowOverride = parameter;
                }
                else if (effect >= 0 && effect != 10 && effect != 11 && effect != 18 && effect != 25)
                    channels[channelNum].QueueEvent(TickEventType.Effect, effect, parameter, delay);
                if (volume >= 0)
                    channels[channelNum].QueueEvent(TickEventType.Volume, volume, 0, delay);
                if (instrument >= 0)
                    channels[channelNum].QueueEvent(TickEventType.Instrument, instrument, 0, delay);
                if (note != -1)
                    channels[channelNum].QueueEvent(TickEventType.Note, note, 0, delay);
                if (effect == 18) // SXX
                {
                    channels[channelNum].QueueEvent(TickEventType.Note, Frame.NOTE_CUT_VALUE, 0, parameter + 1);
                }
                if (effect == 25) // LXX
                {
                    channels[channelNum].QueueEvent(TickEventType.Note, Frame.NOTE_RELEASE_VALUE, 0, parameter + 1);
                }
                if (effect == 10 || effect == 11) // QXX RXX
                {
                    channels[channelNum].QueueEvent(TickEventType.Effect, effect, parameter, delay);
                }
                channelNum++;
            }
        }

        public static void RestoreRow(short[] row) {
            int channelNum = 0;
            for (int i = 0; i < row.Length; i += 5) {
                int instrument = row[i + 1];
                int volume = row[i + 2];
                int effect = row[i + 3];
                int parameter = row[i + 4];

                string effectChar = Helpers.GetEffectCharacter(effect);
                if (effect >= 0 && "12347890PAV".Contains(effectChar))
                    channels[channelNum].EffectCommand(effect, parameter);
                if (effect == 15) // FXX
                    Playback.ticksPerRowOverride = parameter;
                if (volume >= 0)
                    channels[channelNum].SetVolume(volume);
                if (instrument >= 0)
                    channels[channelNum].SetMacro(instrument);
                channelNum++;
            }
        }

        /// <summary>
        /// Returns true if channel is muted
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IsChannelMuted(int channel) {
            return channels[channel].IsMuted;
        }
        /// <summary>
        /// Mutes a specific channel
        /// </summary>
        /// <param name="channel"></param>
        public static void MuteChannel(int channel) {
            channels[channel].IsMuted = true;
        }
        /// <summary>
        /// Unmutes a specific channel
        /// </summary>
        /// <param name="channel"></param>
        public static void UnmuteChannel(int channel) {
            channels[channel].IsMuted = false;
        }
        /// <summary>
        /// Unmutes all playback channels
        /// </summary>
        public static void UnmuteAllChannels() {
            foreach (Channel channel in channels) {
                channel.IsMuted = false;
            }
        }

        /// <summary>
        /// Toggles a specific channel
        /// </summary>
        /// <param name="channel"></param>
        public static void ToggleChannel(int channel) {
            channels[channel].IsMuted = !channels[channel].IsMuted;
        }

        /// <summary>
        /// Mutes all playback channels
        /// </summary>
        public static void MuteAllChannels() {
            foreach (Channel channel in channels) {
                channel.IsMuted = true;
            }
        }
        /// <summary>
        /// Mutes all channels except for the selected
        /// </summary>
        /// <param name="channel"></param>
        public static void SoloChannel(int channel) {
            for (int i = 0; i < channels.Count; i++) {
                channels[i].IsMuted = channel != i;
            }
        }
        /// <summary>
        /// Returns true if all channels are muted
        /// </summary>
        /// <returns></returns>
        public static bool IsEveryChannelMuted() {
            for (int i = 0; i < channels.Count; i++) {
                if (!channels[i].IsMuted)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given channel is soloed
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IsChannelSoloed(int channel) {
            for (int i = 0; i < channels.Count; i++) {
                if (channels[i].IsMuted != (channel != i))
                    return false;
            }
            return true;
        }
    }
}
