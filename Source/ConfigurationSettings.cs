using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace WaveTracker {
    [Serializable]
    public struct ConfigurationSettings {
        [XmlElement(ElementName = "General")]
        public General general;
        public struct General {
            public int screenScale;
            public bool followMode;
            public bool previewNotesOnInput;
        }

        [XmlElement(ElementName = "Files")]
        public Files files;
        public struct Files {
            public string defaultSpeed;
            public int defaultRowLength;

            public bool previewNotesOnInput;
        }

        [XmlElement(ElementName = "PatternEditor")]
        public PatternEditor patternEditor;
        public struct PatternEditor {
            public int screenScale;
            public bool followMode;

            public bool previewNotesOnInput;
        }
    }
}
