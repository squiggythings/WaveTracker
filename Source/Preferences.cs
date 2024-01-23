using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Diagnostics;
using WaveTracker.Rendering;

namespace WaveTracker {
    public static class Preferences {

        public static PreferenceProfile profile;
        public static Rendering.PreferencesDialog dialog;

        static string settingspath => Directory.GetCurrentDirectory() + "/WaveTracker_pref";
        public static void ReadFromFile() {
            if (File.Exists(settingspath)) {
                var serializer = new XmlSerializer(typeof(PreferenceProfile));
                try {
                    using (Stream reader = new FileStream(settingspath, FileMode.Open)) {
                        profile = (PreferenceProfile)serializer.Deserialize(reader);
                    }
                } catch {
                    profile = PreferenceProfile.defaultProfile;
                    SaveToFile();
                }
            } else {
                profile = PreferenceProfile.defaultProfile;
                SaveToFile();
            }
        }

        public static void SaveToFile() {
            using (var writer = new StreamWriter(settingspath)) {
                var serializer = new XmlSerializer(typeof(PreferenceProfile));
                serializer.Serialize(writer, profile);
                writer.Flush();
                Debug.WriteLine("Saved settings to: " + settingspath);
            }
        }

        /*
         * 
         * 
         * 
General
Fade volume column
Ignore step when moving
Restore channel state on playback
Show note off and release as text
Show row numbers in hex
Reflect instrument index changes
Normalize imported samples
Trim silence on imported samples
Oscilloscope mode:

Visualizer
piano roll speed  [   #]
oscilloscope zoom [   #]

audio
volume              25%
<------[]------------->
[Reset audio]
         *  categories
         *      general
         *          [] show row numbers in hex
         *          [] show note cut and release text
         *          [] fade volume column
         *          [] ignore step when moving
         *          [] restore channel state
         *          [] normalize samples on import
         *          [] trim sample silence on import
         *          Oscilloscope mode:
         *          
         *          Visualizer
         *          piano roll speed  [   #]
         *          oscilloscope zoom [   #]
         *      
         *      audio
         *          volume              25%
         *          <------[]------------->
         *          
         */
    }
    [Serializable]
    public class PreferenceProfile {
        [XmlElement(ElementName = "hexRows")]
        public bool showRowNumbersInHex = false;
        [XmlElement(ElementName = "noteCutLines")]
        public bool showNoteCutAndReleaseAsText = false;
        [XmlElement(ElementName = "fadeVol")]
        public bool fadeVolumeColumn = true;
        [XmlElement(ElementName = "ignoreStep")]
        public bool ignoreStepWhenMoving = true;
        [XmlElement(ElementName = "restoreChan")]
        public bool restoreChannelState = true;
        [XmlElement(ElementName = "keyRpt")]
        public bool keyRepeat = true;
        [XmlElement(ElementName = "pgJump")]
        public int pageJumpAmount = 4;

        [XmlElement(ElementName = "oscMode")]
        public int oscilloscopeMode = 1; // 0 - mono; 1 - stereo split; 2 - stereo overlap
        [XmlElement(ElementName = "meterDecay")]
        public int meterDecaySpeed = 1; // 0 - slow, 1 - medium, 2 - fast
        [XmlElement(ElementName = "meterFlat")]
        public int meterColorMode = 1;
        [XmlElement(ElementName = "meterFlash")]
        public bool meterFlashWhenClipping = true;


        [XmlElement(ElementName = "smpNorm")]
        public bool automaticallyNormalizeSamples = true;
        [XmlElement(ElementName = "smpTrim")]
        public bool automaticallyTrimSamples = true;
        [XmlElement(ElementName = "smpResamp")]
        public bool automaticallyResampleSamples = true;
        [XmlElement(ElementName = "smpPreview")]
        public bool previewSamples = true;
        [XmlElement(ElementName = "smpBase")]
        public int defaultBaseKey = 48;
        [XmlElement(ElementName = "smpInclude")]
        public bool includeSamplesInVisualizer = false;
        [XmlElement(ElementName = "resamp_wav")]
        public int defaultResampleWave = 2;
        [XmlElement(ElementName = "resamp_smp")]
        public int defaultResampleSample = 1;

        //[XmlElement(ElementName = "colorTheme")]
        //public ColorTheme theme = ColorTheme.Default;

        [XmlElement(ElementName = "visPianSpeed")]
        public int visualizerPianoSpeed = 8; // 10 default
        [XmlElement(ElementName = "visPianFade")]
        public bool visualizerPianoFade = true;
        [XmlElement(ElementName = "visPianWidth")]
        public bool visualizerPianoChangeWidth = true;
        [XmlElement(ElementName = "visHighlightKeys")]
        public bool visualizerHighlightKeys = true;
        [XmlElement(ElementName = "visScopeZoom")]
        public int visualizerScopeZoom = 100;
        [XmlElement(ElementName = "visScopeColors")]
        public bool visualizerScopeColors = true;
        [XmlElement(ElementName = "visScopeThickness")]
        public int visualizerScopeThickness = 1;
        [XmlElement(ElementName = "visScopeCrosshair")]
        public int visualizerScopeCrosshairs = 0;


        [XmlElement(ElementName = "masVol")]
        public float master_volume = 1f;
        [XmlElement(ElementName = "smpDir")]
        public string lastBrowseDirectory = @"";
        public static PreferenceProfile defaultProfile {
            get {
                PreferenceProfile profile = new PreferenceProfile();
                return profile;
            }
        }

    }
}
