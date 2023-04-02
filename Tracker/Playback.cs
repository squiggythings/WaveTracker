using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;

namespace WaveTracker.Tracker
{
    public static class Playback
    {
        public static bool isPlaying;
        public static int playbackRow;
        public static float timePerTick = (1 / 60f);
        static int tickCounter;
        static int ticksPerRowIndex;
        public static int sampleCounter;
        public static ChannelManager channelManager;

        public static void Update(GameTime gameTime)
        {
            if (Input.GetKeyDown(Keys.Enter, KeyModifier.None))
            {
                if (Input.dialogOpenCooldown == 0)
                    if (isPlaying)
                    {
                        Stop();
                    }
                    else
                    {
                        Play();
                    }
            }

            if (Input.GetKeyRepeat(Keys.Enter, KeyModifier.Ctrl))
            {
                if (Input.dialogOpenCooldown == 0)
                {
                    channelManager.PlayRow(FrameEditor.thisRow);
                    FrameEditor.Move(0, 1);
                }
            }
            if (Input.GetKeyDown(Keys.Enter, KeyModifier.Alt))
            {
                if (Input.dialogOpenCooldown == 0)
                    if (!isPlaying)
                        PlayFromCursor();
            }

        }

        public static void Play()
        {
            isPlaying = true;
            channelManager.Reset();
            if (Preferences.restoreChannelState)
            {
                RestoreUntil(FrameEditor.currentFrame, 0);
            }
            FrameEditor.Goto(FrameEditor.currentFrame, 0);
            sampleCounter = 0;
            channelManager.ResetTicks(0);
        }

        public static void PlayFromCursor()
        {
            isPlaying = true;
            channelManager.Reset();
            if (Preferences.restoreChannelState)
            {
                RestoreUntil(FrameEditor.currentFrame, FrameEditor.currentRow);
            }
            FrameEditor.Goto(FrameEditor.currentFrame, FrameEditor.currentRow);
            sampleCounter = 0;
            channelManager.ResetTicks(0);
        }

        public static void PlayFromBeginning()
        {
            isPlaying = true;
            channelManager.Reset();
            FrameEditor.Goto(0, 0);
            sampleCounter = 0;
            channelManager.ResetTicks(0);
        }

        public static void Stop()
        {
            isPlaying = false;
            channelManager.Reset();
        }

        public static void Step(bool b)
        {
            Tick();
        }

        static void Tick()
        {
            if (isPlaying)
            {
                if (tickCounter >= FrameEditor.thisSong.ticksPerRow[FrameEditor.currentRow % FrameEditor.thisSong.ticksPerRow.Length])
                {
                    channelManager.PlayRow(FrameEditor.thisRow);
                    FrameEditor.Move(0, 1);
                    playbackRow = FrameEditor.currentRow;
                    tickCounter = 0;
                }
                tickCounter++;
            }
            //channelManager.Tick(0);
        }

        public static void RestoreUntil(int frame, int row)
        {
            for (int f = 0; f <= frame; f++)
            {
                for (int r = 0; r < FrameEditor.thisSong.rowsPerFrame; r++)
                {
                    if (f == frame && r == row)
                        return;
                    channelManager.RestoreRow(FrameEditor.thisSong.frames[f].pattern[r]);
                }
            }
        }
    }
}
