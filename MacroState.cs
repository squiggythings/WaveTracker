using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker
{
    public struct MacroState
    {
        public int arpOffset = 0;
        public int pitchOffset = 0;
        public float volumeMultiplier = 1;
        public int waveNum = -1;

        public MacroState()
        {
            
        }

    }
}