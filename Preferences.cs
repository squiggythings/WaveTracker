using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker
{
    public static class Preferences
    {
        
        public static bool showRowNumbersInHex = false;
        public static bool showNoteCutAndReleaseAsSymbols = true;
        public static bool fadeVolumeColumn = true;
        public static bool ignoreStepWhenMoving = true;
        public static bool restoreChannelState = true;
        public static bool keyRepeat = true;
        public static int oscilloscopeMode = 1; // 1 - mono; 2 - stereo split; 3 - stereo overlap


    }
}
