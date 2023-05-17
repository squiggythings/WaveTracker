using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaveTracker
{
    [Serializable]
    public static class Preferences
    {

        public static bool showRowNumbersInHex = false;
        public static bool showNoteCutAndReleaseAsSymbols = true;
        public static bool fadeVolumeColumn = true;
        public static bool ignoreStepWhenMoving = true;
        public static bool restoreChannelState = true;
        public static bool keyRepeat = true;
        public static bool automaticallyNormalizeSamples = true;
        public static bool automaticallyTrimSamples = true;
        public static int oscilloscopeMode = 3; // 1 - mono; 2 - stereo split; 3 - stereo overlap
        public static int pageJumpAmount = 4;
        public static bool visualizerShowSamplesInPianoRoll = false;
        public static int visualizerPianoSpeed = 10;//10;
        public static int visualizerScopeZoom = 40;
        public static string lastBrowseDirectory = @"%USERPROFILE%";

    }
}
