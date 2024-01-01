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

namespace WaveTracker.Rendering
{
    public class WaveBank : Panel
    {
        WaveBankElement[] waveBankElements;
        public Song song => Game1.currentSong;
        public WaveEditor editor;
        public static int currentWave;
        public static int lastSelectedWave;
        public WaveBank()
        {
            waveBankElements = new WaveBankElement[100];
            InitializePanel("Wave Bank", 510, 18, 448, 130);
            lastSelectedWave = 0;
            int index = 0;
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    waveBankElements[index] = new WaveBankElement(this, index);
                    waveBankElements[index].x = x * 22 + 4;
                    waveBankElements[index].y = y * 22 + 13;
                    waveBankElements[index].SetTooltip("", "Wave " + index.ToString("D2"));
                    index++;
                }
            }
        }
        public Wave GetWave(int num)
        {
            return song.waves[num];
        }
        public void Initialize()
        {

        }

        public void Update()
        {
            int i = 0;
            if (Input.focus == null)
                currentWave = -1;
            foreach (WaveBankElement e in waveBankElements)
            {
                e.Update();
                if (e.Clicked)
                {
                    currentWave = i;
                    lastSelectedWave = i;
                    editor.EditWave(song.waves[i], i);
                    if (!Audio.ChannelManager.previewChannel.waveEnv.toPlay.isActive)
                        Audio.ChannelManager.previewChannel.SetWave(i);
                }

                ++i;
            }
        }

        public void Draw()
        {
            DrawPanel();
            DrawRect(2, 11, 444, 114, new Color(43, 49, 81));
            foreach (WaveBankElement e in waveBankElements)
            {
                e.Draw();
            }
        }
    }
}
