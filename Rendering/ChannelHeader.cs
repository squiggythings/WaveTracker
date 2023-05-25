using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.Rendering
{
    public class ChannelHeader : Clickable
    {
        public int id;
        public const int MeterDecayRate = 4;
        float timeSinceSinceSoloed;
        public int amplitude;
        int currentAmp;
        Audio.Channel channel;
        public float sampVolume;
        public ChannelHeader(int id, Audio.Channel ch)
        {
            this.id = id;
            this.width = 64;
            this.height = 31;
            channel = ch;
        }
        public void Update(GameTime gameTime)
        {
            if (id < 0)
            {
                if (Clicked)
                {
                    FrameEditor.UnmuteAllChannels();
                }
                return;
            }
            UpdateAmplitude();


            if (FrameEditor.isChannelSoloed(id)) timeSinceSinceSoloed = 0;
            else timeSinceSinceSoloed += gameTime.ElapsedGameTime.Milliseconds;
            if (DoubleClickedM(KeyModifier.None))
            {
                if (timeSinceSinceSoloed <= Input.DOUBLE_CLICK_TIME)
                    FrameEditor.UnmuteAllChannels();
                else
                    FrameEditor.SoloChannel(id);
            }
            if (DoubleClickedM(KeyModifier.Ctrl))
            {
                FrameEditor.UnmuteAllChannels();
            }
            if (SingleClickedM(KeyModifier.Ctrl))
            {
                if (timeSinceSinceSoloed <= Input.DOUBLE_CLICK_TIME)
                    FrameEditor.UnmuteAllChannels();
                else
                    FrameEditor.SoloChannel(id);
            }
            if (SingleClickedM(KeyModifier.None))
            {
                FrameEditor.ToggleChannel(id);
            }
            channel._sampleVolume = -1;
        }

        public void UpdateAmplitude()
        {
            if (id < 0)
            {
                return;
            }

            currentAmp = Math.Clamp((int)(channel.CurrentAmplitude * 50), 0, 50);
            if (currentAmp >= amplitude)
                amplitude = currentAmp;
            else
            {
                amplitude -= MeterDecayRate;
                if (currentAmp >= amplitude)
                    amplitude = currentAmp;
            }
            if (amplitude < 0)
                amplitude = 0;
            channel._sampleVolume *= 0.6f;
        }

        public void Draw()
        {
            if (id < 0)
            {
                DrawSprite(Game1.channelHeaderSprite, 0, 0, new Rectangle(63 * 3, 0, 63, 31));
                return;
            }
            if (FrameEditor.channelToggles[id])
            {
                if (IsPressed)
                    DrawSprite(Game1.channelHeaderSprite, 0, 0, new Rectangle(63 * 2, 0, 63, 31));
                else
                    DrawSprite(Game1.channelHeaderSprite, 0, 0, new Rectangle(0, 0, 63, 31));
                Write("Channel " + (id + 1), 6, 10, new Color(104, 111, 153));

                if (!Audio.AudioEngine.rendering)
                {
                    DrawRect(6, 25, amplitude == 0 ? 0 : amplitude + 1, 3, new Color(0, 219, 39));
                    DrawRect(21 + (int)(channel.CurrentPan * 19), 22, 3, 2, Color.White);
                }
            }
            else
            {
                DrawSprite(Game1.channelHeaderSprite, 0, 0, new Rectangle(63, 0, 63, 31));
                Write("Channel " + (id + 1), 6, 11, new Color(230, 69, 57));
            }
        }
    }
}
