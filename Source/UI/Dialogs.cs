using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.UI {
    public static class Dialogs {
        public static ModuleSettingsDialog moduleSettings;
        public static PreferencesDialog preferences;
        public static ColorPickerDialog colorPicker;
        public static HumanizeDialog humanizeDialog;
        public static ExportDialog exportDialog;
        public static ExportingDialog exportingDialog;

        //public static WarningMessage warningMessage;

        public static void Initialize() {
            moduleSettings = new ModuleSettingsDialog();
            preferences = new PreferencesDialog();
            colorPicker = new ColorPickerDialog();
            humanizeDialog = new HumanizeDialog();
            exportDialog = new ExportDialog();
            exportingDialog = new ExportingDialog();
            //warningMessage = new WarningMessage();
        }

        public static void Update() {
            moduleSettings.Update();
            preferences.Update();
            colorPicker.Update();
            humanizeDialog.Update();
            exportDialog.Update();
            exportingDialog.Update();

            //warningMessage.Update();
        }

        public static void Draw() {
            moduleSettings.Draw();
            preferences.Draw();
            colorPicker.Draw();
            humanizeDialog.Draw();
            exportDialog.Draw();
            exportingDialog.Draw();

           // warningMessage.Draw();
        }
    }
}
