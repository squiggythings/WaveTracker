using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;

namespace WaveTracker.Source.UI {
    public class ConfigurationDialog : Dialog {

        public ConfigurationDialog() : base("Preferences...", 376, 290) {

        }
        public override void Update() {

        }

        class ConfigurationsGeneralPage : Clickable {
            public ConfigurationsGeneralPage(Element parent) {
                width = 289;
                height = 258;
            }


            ConfigurationOption.Bool AddBool() {

            }
        }
    }

    public class ConfigurationOption : Clickable {
        public string Label { get; private set; }
        public string Description { get; private set; }

        public ConfigurationOption(int y, int height, Clickable parent) {
            this.height = height;
            width = parent.width - 8;
            x = 4;
            this.y = y;
            this.height = height;
            SetParent(parent);
        }

        public class Bool : ConfigurationOption {
            public Bool(string label, string description, int y, int height, Clickable parent) : base(y, height, parent) {

            }
        }
    }

}
