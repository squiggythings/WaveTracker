using System.Collections.Generic;
using System.Diagnostics;
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

        public static void PlayRow(int frame, int row) {
            int channelNum = 0;
            for (int channelIndex = 0; channelIndex < App.CurrentModule.ChannelCount; channelIndex++) {
                int numEffects = App.CurrentSong.NumEffectColumns[channelIndex];
                int delayTicks = 0;


                // get delay from Gxx effects
                for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                    char effectType = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1 + 2 * effectIndex];
                    int effectParam = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1Parameter + 2 * effectIndex];
                    if (effectType == 'G') {
                        delayTicks += effectParam;
                    }
                }

                // process all effects except cut/release and delay effects
                for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                    char effectType = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1 + 2 * effectIndex];
                    int effectParam = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1Parameter + 2 * effectIndex];
                    if (effectType == 'C') {
                        Playback.StopNext();
                    }
                    else if (effectType == 'B') {
                        Playback.GotoNext(effectParam % App.CurrentSong.FrameSequence.Count, 0);
                    }
                    else if (effectType == 'D') {
                        Playback.GotoNext(Playback.position.Frame + 1, effectParam);
                    }
                    else if (effectType == 'F') {
                        Playback.ticksPerRowOverride = effectParam;
                    }
                    else if (effectType != WTPattern.EVENT_EMPTY && effectType != 'L' && effectType != 'S' && effectType != 'G' && effectType != 'Q' && effectType != 'R') {
                        channels[channelIndex].QueueEvent(TickEventType.Effect, effectType, effectParam, delayTicks);
                    }
                }
                // process volume
                if (App.CurrentSong[frame][row, channelIndex, CellType.Volume] != WTPattern.EVENT_EMPTY)
                    channels[channelIndex].QueueEvent(TickEventType.Volume, App.CurrentSong[frame][row, channelIndex, CellType.Volume], 0, delayTicks);
                if (App.CurrentSong[frame][row, channelIndex, CellType.Instrument] != WTPattern.EVENT_EMPTY)
                    channels[channelIndex].QueueEvent(TickEventType.Instrument, App.CurrentSong[frame][row, channelIndex, CellType.Instrument], 0, delayTicks);
                if (App.CurrentSong[frame][row, channelIndex, CellType.Note] != WTPattern.EVENT_EMPTY)
                    channels[channelIndex].QueueEvent(TickEventType.Note, App.CurrentSong[frame][row, channelIndex, CellType.Note], 0, delayTicks);

                // process cut/release effects
                for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                    char effectType = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1 + 2 * effectIndex];
                    int effectParam = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1Parameter + 2 * effectIndex];
                    if (effectType == 'L') {
                        channels[channelNum].QueueEvent(TickEventType.Note, WTPattern.EVENT_NOTE_RELEASE, 0, delayTicks + effectParam);
                    }
                    if (effectType == 'S') {
                        channels[channelNum].QueueEvent(TickEventType.Note, WTPattern.EVENT_NOTE_CUT, 0, delayTicks + effectParam);
                    }
                    if (effectType == 'Q' || effectType == 'R') {
                        channels[channelIndex].QueueEvent(TickEventType.Effect, effectType, effectParam, delayTicks);
                    }
                }

                channelNum++;
            }
        }

        public static void RestoreRow(int frame, int row) {
            for (int channelNum = 0; channelNum < App.CurrentModule.ChannelCount; ++channelNum) {
                int instrument = App.CurrentSong[frame][row, channelNum, CellType.Instrument];
                int volume = App.CurrentSong[frame][row, channelNum, CellType.Volume];


                for (int i = 0; i < App.CurrentSong.NumEffectColumns[channelNum]; ++i) {
                    char effectType = (char)App.CurrentSong[frame][row, channelNum, CellType.Effect1 + 2 * i];
                    int effectParam = App.CurrentSong[frame][row, channelNum, CellType.Effect1Parameter + 2 * i];
                    if (effectType != WTPattern.EVENT_EMPTY && !"BCDQRSL".Contains(effectType))
                        channels[channelNum].ApplyEffect(effectType, effectParam);
                    if (effectType == 'F') // FXX
                        Playback.ticksPerRowOverride = effectParam;
                }
                if (volume >= 0)
                    channels[channelNum].SetVolume(volume);
                if (instrument >= 0)
                    channels[channelNum].SetMacro(instrument);
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
