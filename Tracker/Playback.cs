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
using System.Diagnostics;

namespace WaveTracker.Tracker
{
    public static class Playback
    {
        public static bool isPlaying;
        static bool lastIsPlaying;
        public static int playbackFrame;
        public static int playbackRow;
        public static int nextPlaybackFrame;
        public static int nextPlaybackRow;
        public static bool hasNext;
        static int tickCounter;
        public static ChannelManager channelManager;
        public static int ticksPerRowOverride;
        public static int ticksPerRow;

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
            playbackFrame = FrameEditor.currentFrame;
            playbackRow = 0;
            Restore();
            channelManager.ResetTicks(0);
        }

        public static void PlayFromCursor()
        {
            isPlaying = true;
            channelManager.Reset();

            playbackFrame = FrameEditor.currentFrame;
            playbackRow = FrameEditor.currentRow;
            Restore();
            channelManager.ResetTicks(0);
        }

        public static void PlayFromBeginning()
        {
            isPlaying = true;
            channelManager.Reset();
            playbackFrame = 0;
            playbackRow = 0;
            Restore();
            channelManager.ResetTicks(0);
        }

        public static void Stop()
        {
            isPlaying = false;
            foreach (Channel c in channelManager.channels)
            {
                c.Cut();
            }
            channelManager.Reset();
        }

        public static void Step()
        {
            Tick();
        }

        static void Tick()
        {
            if (isPlaying)
            {
                if (!lastIsPlaying)
                {
                    tickCounter = 0;
                    channelManager.ResetTicks(0);
                    lastIsPlaying = true;
                    PlayRow();
                    if (ticksPerRowOverride == -1)
                        ticksPerRow = FrameEditor.thisSong.ticksPerRow[playbackRow % FrameEditor.thisSong.ticksPerRow.Length];
                    else
                        ticksPerRow = ticksPerRowOverride;
                    hasNext = false;
                }
                if (tickCounter >= ticksPerRow)
                {
                    tickCounter = 0;
                    if (hasNext)
                    {
                        hasNext = false;
                        if (nextPlaybackFrame < 0)
                        {
                            Stop();
                            return;
                        }
                        else
                        {
                            playbackFrame = nextPlaybackFrame;
                            playbackRow = nextPlaybackRow;
                        }
                    }
                    else
                    {
                        MoveNextRow();
                    }
                    PlayRow();
                    if (ticksPerRowOverride == -1)
                        ticksPerRow = FrameEditor.thisSong.ticksPerRow[playbackRow % FrameEditor.thisSong.ticksPerRow.Length];
                    else
                        ticksPerRow = ticksPerRowOverride;

                }
                tickCounter++;
            }
            else
            {
                if (lastIsPlaying)
                {
                    lastIsPlaying = false;
                    channelManager.Reset();
                }

            }
            //channelManager.Tick(0);
        }

        static void PlayRow()
        {
            channelManager.PlayRow(Game1.currentSong.frames[playbackFrame].pattern[playbackRow]);
        }

        static void MoveNextRow()
        {
            playbackRow++;
            if (playbackRow >= Game1.currentSong.rowsPerFrame)
            {
                playbackRow = 0;
                playbackFrame++;
                if (playbackFrame >= Game1.currentSong.frames.Count)
                    playbackFrame = 0;
            }
            if (Playback.isPlaying)
            {
                if (FrameEditor.followMode)
                {
                    FrameEditor.cursorRow = Playback.playbackRow;
                    FrameEditor.currentFrame = Playback.playbackFrame;
                }
            }
        }

        public static void StopNext()
        {
            hasNext = true;
            nextPlaybackFrame = -1;
        }

        public static void GotoNext(int fr, int row)
        {
            hasNext = true;
            nextPlaybackFrame = fr % FrameEditor.thisSong.frames.Count;
            nextPlaybackRow = row % FrameEditor.thisSong.rowsPerFrame;
        }

        static void Restore()
        {
            if (Preferences.restoreChannelState)
            {
                RestoreUntil(playbackFrame, playbackRow);
            }
        }
        public static void RestoreUntil(int frame, int row)
        {
            ticksPerRowOverride = -1;
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
