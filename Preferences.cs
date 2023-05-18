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
        public static int oscilloscopeMode = 2; // 1 - mono; 2 - stereo split; 3 - stereo overlap
        public static int pageJumpAmount = 4;
        public static int visualizerPianoSpeed = 8; // 10 default
        public static int visualizerScopeZoom = 40; // 40 default
        public static string lastBrowseDirectory = @"C:/Users/Elias/Desktop/stuff that takes up space/pxtone/my_material";

    }
}
