using System.Collections.Generic;
using System.Diagnostics;
using WaveTracker.Tracker;
using WaveTracker.UI;

namespace WaveTracker.Audio {
    public static class ChannelManager {
        /// <summary>
        /// An extra channel used for previewing notes and waves in the editor
        /// </summary>
        public static Channel PreviewChannel { get; private set; }
        public static List<Channel> Channels { get; private set; }

        
        public static void Initialize(int numChannels) {
            PreviewChannel = new Channel(-1);
            Channels = new List<Channel>();
            for (int i = 0; i < numChannels; i++) {
                Channels.Add(new Channel(i));
            }
        }


        /// <summary>
        /// Reset all channels to their default state
        /// </summary>
        public static void Reset() {
            foreach (Channel channel in Channels) {
                channel.Reset();
            }
        }

        /// <summary>
        /// Plays the contents of a row
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="row"></param>
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
                        Playback.TicksPerRowOverride = effectParam;
                    }
                    else if (effectType != WTPattern.EVENT_EMPTY && effectType != 'L' && effectType != 'S' && effectType != 'G' && effectType != 'Q' && effectType != 'R') {
                        Channels[channelIndex].QueueEvent(TickEventType.Effect, effectType, effectParam, delayTicks);
                    }
                }

                // process volume
                if (App.CurrentSong[frame][row, channelIndex, CellType.Volume] != WTPattern.EVENT_EMPTY)
                    Channels[channelIndex].QueueEvent(TickEventType.Volume, App.CurrentSong[frame][row, channelIndex, CellType.Volume], 0, delayTicks);
                if (App.CurrentSong[frame][row, channelIndex, CellType.Instrument] != WTPattern.EVENT_EMPTY)
                    Channels[channelIndex].QueueEvent(TickEventType.Instrument, App.CurrentSong[frame][row, channelIndex, CellType.Instrument], 0, delayTicks);
                if (App.CurrentSong[frame][row, channelIndex, CellType.Note] != WTPattern.EVENT_EMPTY)
                    Channels[channelIndex].QueueEvent(TickEventType.Note, App.CurrentSong[frame][row, channelIndex, CellType.Note], 0, delayTicks);

                // process cut/release effects
                for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                    char effectType = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1 + 2 * effectIndex];
                    int effectParam = (char)App.CurrentSong[frame][row, channelIndex, CellType.Effect1Parameter + 2 * effectIndex];
                    if (effectType == 'L') {
                        Channels[channelNum].QueueEvent(TickEventType.Note, WTPattern.EVENT_NOTE_RELEASE, 0, delayTicks + effectParam);
                    }
                    if (effectType == 'S') {
                        Channels[channelNum].QueueEvent(TickEventType.Note, WTPattern.EVENT_NOTE_CUT, 0, delayTicks + effectParam);
                    }
                    if (effectType == 'Q' || effectType == 'R') {
                        Channels[channelIndex].QueueEvent(TickEventType.Effect, effectType, effectParam, delayTicks);
                    }
                }
                
                channelNum++;
            }
        }

        /// <summary>
        /// Sets the channel states from the events in a row
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="row"></param>
        public static void RestoreRow(int frame, int row) {
            for (int channelNum = 0; channelNum < App.CurrentModule.ChannelCount; ++channelNum) {
                int instrument = App.CurrentSong[frame][row, channelNum, CellType.Instrument];
                int volume = App.CurrentSong[frame][row, channelNum, CellType.Volume];


                for (int i = 0; i < App.CurrentSong.NumEffectColumns[channelNum]; ++i) {
                    char effectType = (char)App.CurrentSong[frame][row, channelNum, CellType.Effect1 + 2 * i];
                    int effectParam = App.CurrentSong[frame][row, channelNum, CellType.Effect1Parameter + 2 * i];
                    if (effectType != WTPattern.EVENT_EMPTY && !"BCDQRSL".Contains(effectType))
                        Channels[channelNum].ApplyEffect(effectType, effectParam);
                    if (effectType == 'F') // FXX
                        Playback.TicksPerRowOverride = effectParam;
                }
                if (volume != WTPattern.EVENT_EMPTY)
                    Channels[channelNum].SetVolume(volume);
                if (instrument != WTPattern.EVENT_EMPTY)
                    Channels[channelNum].SetMacro(instrument);
            }
        }

        /// <summary>
        /// Returns true if channel is muted
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IsChannelMuted(int channel) {
            return Channels[channel].IsMuted;
        }

        /// <summary>
        /// Returns true if channel is unmuted
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IsChannelOn(int channel) {
            return !Channels[channel].IsMuted;
        }
        /// <summary>
        /// Mutes a specific channel
        /// </summary>
        /// <param name="channel"></param>
        public static void MuteChannel(int channel) {
            Channels[channel].IsMuted = true;
        }
        /// <summary>
        /// Unmutes a specific channel
        /// </summary>
        /// <param name="channel"></param>
        public static void UnmuteChannel(int channel) {
            Channels[channel].IsMuted = false;
        }
        /// <summary>
        /// Unmutes all playback channels
        /// </summary>
        public static void UnmuteAllChannels() {
            foreach (Channel channel in Channels) {
                channel.IsMuted = false;
            }
        }

        /// <summary>
        /// Toggles a specific channel
        /// </summary>
        /// <param name="channel"></param>
        public static void ToggleChannel(int channel) {
            Channels[channel].IsMuted = !Channels[channel].IsMuted;
        }

        /// <summary>
        /// Toggles the current channel
        /// </summary>
        /// <param name="channel"></param>
        public static void ToggleCurrentChannel() {
            ToggleChannel(App.PatternEditor.cursorPosition.Channel);
        }

        /// <summary>
        /// Mutes all playback channels
        /// </summary>
        public static void MuteAllChannels() {
            foreach (Channel channel in Channels) {
                channel.IsMuted = true;
            }
        }
        /// <summary>
        /// Mutes all channels except for the selected
        /// </summary>
        /// <param name="channel"></param>
        public static void SoloChannel(int channel) {
            for (int i = 0; i < Channels.Count; i++) {
                Channels[i].IsMuted = channel != i;
            }
        }

        /// <summary>
        /// Mutes all channels except for the selected one
        /// </summary>
        public static void SoloCurrentChannel() {
            SoloChannel(App.PatternEditor.cursorPosition.Channel);
        }
        /// <summary>
        /// Returns true if all channels are muted
        /// </summary>
        /// <returns></returns>
        public static bool IsEveryChannelMuted() {
            for (int i = 0; i < Channels.Count; i++) {
                if (!Channels[i].IsMuted)
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
            for (int i = 0; i < Channels.Count; i++) {
                if (Channels[i].IsMuted != (channel != i))
                    return false;
            }
            return true;
        }
    }
}
