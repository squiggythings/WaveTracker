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
        public static int loops, maxLoops;
        public static bool rendering, stopRendering;
        public static Frame frame => Game1.currentSong.frames[playbackFrame];

        public static void Update(GameTime gameTime)
        {
            if (Input.GetKeyDown(Keys.F5, KeyModifier.None))
            {
                PlayFromBeginning();
            }
            if (Input.GetKeyDown(Keys.F7, KeyModifier.None))
            {
                PlayFromCursor();
            }
            if (Input.GetKeyDown(Keys.F8, KeyModifier.None))
            {
                Stop();
            }
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
            if (!Game1.VisualizerMode)
            {
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
                        PlayFromCursor();
                }
            }
        }

        public static void Play()
        {
            isPlaying = true;
            channelManager.Reset();
            playbackFrame = FrameEditor.currentFrame;
            playbackRow = 0;
            Rendering.Visualization.GetWaveColors();
            if (FrameEditor.followMode)
            {
                FrameEditor.cursorRow = Playback.playbackRow;
                FrameEditor.currentFrame = Playback.playbackFrame;
            }
            Restore();
            channelManager.ResetTicks(0);
        }

        public static void PlayFromCursor()
        {
            isPlaying = true;
            channelManager.Reset();

            playbackFrame = FrameEditor.currentFrame;
            playbackRow = FrameEditor.currentRow;
            Rendering.Visualization.GetWaveColors();
            if (FrameEditor.followMode)
            {
                FrameEditor.cursorRow = Playback.playbackRow;
                FrameEditor.currentFrame = Playback.playbackFrame;
            }
            Restore();
            channelManager.ResetTicks(0);
        }

        public static void ResetPlayhead()
        {

        }

        public static void PlayFromBeginning()
        {
            isPlaying = true;
            channelManager.Reset();
            playbackFrame = 0;
            Rendering.Visualization.GetWaveColors();
            playbackRow = 0;
            if (FrameEditor.followMode)
            {
                FrameEditor.cursorRow = Playback.playbackRow;
                FrameEditor.currentFrame = Playback.playbackFrame;
            }
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

        public static bool RenderStep()
        {
            if (tickCounter >= ticksPerRow)
            {
                tickCounter = 0;
                if (hasNext)
                {
                    hasNext = false;
                    if (nextPlaybackFrame < 0)
                    {
                        Stop();
                        return false;
                    }
                    else
                    {
                        if (nextPlaybackFrame <= playbackFrame)
                        {
                            loops++;
                            if (rendering && loops > maxLoops)
                            {
                                return false;
                            }
                        }
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
            return true;
        }

        public static void RenderInitialize()
        {
            channelManager.Reset();
            playbackFrame = 0;
            Rendering.Visualization.GetWaveColors();
            playbackRow = 0;
            loops = 0;
            isPlaying = false;
            Restore();
            channelManager.ResetTicks(0);
        }

        public static void RenderStart()
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
                            if (rendering)
                                stopRendering = true;
                            return;
                        }
                        else
                        {
                            if (nextPlaybackFrame <= playbackFrame)
                            {
                                loops++;
                            }
                            playbackFrame = nextPlaybackFrame;
                            playbackRow = nextPlaybackRow;
                            if (FrameEditor.followMode)
                            {
                                FrameEditor.cursorRow = Playback.playbackRow;
                                FrameEditor.currentFrame = Playback.playbackFrame;
                            }
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
            if (playbackFrame < Game1.currentSong.frames.Count)
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
                {
                    playbackFrame = 0;
                    loops++;
                }
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

        public static void NextFrame()
        {
            playbackFrame++;
            playbackFrame %= Game1.currentSong.frames.Count;
            playbackRow = 0;
            PlayRow();
        }

        public static void PreviousFrame()
        {
            playbackFrame--;
            playbackFrame += Game1.currentSong.frames.Count;
            playbackFrame %= Game1.currentSong.frames.Count;
            playbackRow = 0;
            PlayRow();
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
            if (nextPlaybackFrame <= playbackFrame)
                loops++;
        }

        public static void Goto(int fr, int row)
        {
            playbackFrame = fr;
            playbackRow = row;
        }

        static void Restore()
        {
            if (Preferences.profile.restoreChannelState)
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
                    {
                        //channelManager.PlayRow(FrameEditor.thisSong.frames[frame].pattern[row]);
                        channelManager.RestoreRow(FrameEditor.thisSong.frames[f].pattern[r]);
                        return;
                    }
                    channelManager.RestoreRow(FrameEditor.thisSong.frames[f].pattern[r]);
                }
            }
        }
    }
}
