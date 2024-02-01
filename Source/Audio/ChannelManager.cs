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

        public static void ResetTicks(int num) {
            //foreach (Channel channel in channels)
            //{
            //    channel.ResetTick(num);
            //}
        }

        public static void PlayRow(PatternRow row) {
            int channelNum = 0;
            for (int channelIndex = 0; channelIndex < row.Length; channelIndex++) {
                PatternEvent e = row[channelIndex];
                if (!e.IsEmpty) {
                    int numEffects = WTSong.currentSong.NumEffectColumns[channelIndex];

                    int delayTicks = 0;

                    // get delay from Gxx effects
                    for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                        char effectType = e.GetEffect(effectIndex).Type;
                        int effectParam = e.GetEffect(effectIndex).Parameter;
                        if (effectType == 'G') {
                            delayTicks = effectParam;
                        }
                    }

                    // process all effects except release effects
                    for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                        char effectType = e.GetEffect(effectIndex).Type;
                        int effectParam = e.GetEffect(effectIndex).Parameter;
                        if (effectType == 'C') {
                            Playback.StopNext();
                        }
                        else if (effectType == 'B') {
                            Playback.GotoNext(effectParam % WTSong.currentSong.FrameSequence.Count, 0);
                        }
                        else if (effectType == 'D') {
                            Playback.GotoNext(Playback.position.Frame + 1, effectParam);
                        }
                        else if (effectType == 'F') {
                            Playback.ticksPerRowOverride = effectParam;
                        }
                        else if (effectType != PatternEvent.EMPTY && effectType != 'L' && effectType != 'S') {
                            channels[channelIndex].QueueEvent(TickEventType.Effect, effectType, effectParam, delayTicks);
                        }
                    }
                    // process volume
                    if (e.Volume != PatternEvent.EMPTY)
                        channels[channelIndex].QueueEvent(TickEventType.Volume, e.Volume, 0, delayTicks);
                    if (e.Instrument != PatternEvent.EMPTY)
                        channels[channelIndex].QueueEvent(TickEventType.Instrument, e.Instrument, 0, delayTicks);
                    if (e.Note != PatternEvent.EMPTY)
                        channels[channelIndex].QueueEvent(TickEventType.Note, e.Note, 0, delayTicks);

                    for (int effectIndex = 0; effectIndex < numEffects; effectIndex++) {
                        char effectType = e.GetEffect(effectIndex).Type;
                        int effectParam = e.GetEffect(effectIndex).Parameter;
                        if (effectType == 'L') {
                            channels[channelNum].QueueEvent(TickEventType.Note, PatternEvent.NOTE_RELEASE, 0, delayTicks + effectParam + 1);
                        }
                        if (effectType == 'S') {
                            channels[channelNum].QueueEvent(TickEventType.Note, PatternEvent.NOTE_CUT, 0, delayTicks + effectParam + 1);
                        }
                    }
                }

                //else if (effect == 20) // CXX
                //{
                //    Playback.StopNext();
                //}
                //else if (effect == 21) // BXX
                //  {
                //    Playback.GotoNext(parameter % FrameEditor.thisSong.frames.Count, 0);
                //}
                //else if (effect == 22) // DXX
                //  {
                //    Playback.GotoNext(Playback.playbackFrame + 1, parameter);
                //}
                //else if (effect == 15) // FXX
                //  {
                //    Playback.ticksPerRowOverride = parameter;
                //}
                //else if (effect >= 0 && effect != 10 && effect != 11 && effect != 18 && effect != 25)
                //    channels[channelNum].QueueEvent(TickEventType.Effect, effect, parameter, delay);
                //if (volume >= 0)
                //    channels[channelNum].QueueEvent(TickEventType.Volume, volume, 0, delay);
                //if (instrument >= 0)
                //    channels[channelNum].QueueEvent(TickEventType.Instrument, instrument, 0, delay);
                //if (note != -1)
                //    channels[channelNum].QueueEvent(TickEventType.Note, note, 0, delay);
                //if (effect == 18) // SXX
                //{
                //    channels[channelNum].QueueEvent(TickEventType.Note, Frame.NOTE_CUT_VALUE, 0, parameter + 1);
                //}
                //if (effect == 25) // LXX
                //{
                //    channels[channelNum].QueueEvent(TickEventType.Note, Frame.NOTE_RELEASE_VALUE, 0, parameter + 1);
                //}
                //if (effect == 10 || effect == 11) // QXX RXX
                //{
                //    channels[channelNum].QueueEvent(TickEventType.Effect, effect, parameter, delay);
                //}
                channelNum++;
            }
        }

        public static void RestoreRow(PatternRow row) {
            for (int channelNum = 0; channelNum < row.Length; ++channelNum) {
                PatternEvent e = row[channelNum];
                int instrument = e.Instrument;
                int volume = e.Volume;
                

                for (int i = 0; i < WTSong.currentSong.NumEffectColumns[channelNum]; ++i) {
                    char effectType = e.GetEffect(i).Type;
                    int effectParam = e.GetEffect(i).Parameter;
                    if (effectType != PatternEvent.EMPTY && !"BCDQRSL".Contains(effectType))
                        channels[channelNum].ApplyEffect(effectType, effectParam);
                    if (effectType == 'F') // FXX
                        Playback.ticksPerRowOverride = effectParam;
                }
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
