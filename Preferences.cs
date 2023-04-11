using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker
{
    public static class Preferences
    {

        public static bool showRowNumbersInHex = true;
        public static bool showNoteCutAndReleaseAsSymbols = true;
        public static bool fadeVolumeColumn = true;
        public static bool ignoreStepWhenMoving = true;
        public static bool restoreChannelState = true;
        public static bool keyRepeat = true;
        public static bool automaticallyNormalizeSamples = true;
        public static bool automaticallyTrimSamples = true;
        public static int oscilloscopeMode = 2; // 1 - mono; 2 - stereo split; 3 - stereo overlap
        public static int pageJumpAmount = 4;
        public static bool visualizerShowSamplesInPianoRoll = false;


    }
}
