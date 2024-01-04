using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;

namespace WaveTracker.Audio {
    public static class ChannelManager {
        public static Channel previewChannel;
        public static List<Channel> channels;
        public static Rendering.WaveBank waveBank;
        
        public static void Initialize(int numChannels, Rendering.WaveBank waveBank) {
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
                } else if (effect == 21) // BXX
                  {
                    Playback.GotoNext(parameter % FrameEditor.thisSong.frames.Count, 0);
                } else if (effect == 22) // DXX
                  {
                    Playback.GotoNext(Playback.playbackFrame + 1, parameter);
                } else if (effect == 15) // FXX
                  {
                    Playback.ticksPerRowOverride = parameter;
                } else if (effect >= 0 && effect != 10 && effect != 11 && effect != 18 && effect != 25)
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
    }
}
