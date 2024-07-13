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
        public static SetFramePatternDialog setFramePatternDialog;
        public static WaveAddFuzzDialog waveAddFuzzDialog;
        public static WaveSampleAndHoldDialog waveSampleAndHoldDialog;
        public static WaveSmoothDialog waveSmoothDialog;
        public static WaveSyncDialog waveSyncDialog;
        public static ConfigurationDialog configurationDialog;



        public static MessageDialog messageDialog;

        public static void Initialize() {
            moduleSettings = new ModuleSettingsDialog();
            preferences = new PreferencesDialog();
            colorPicker = new ColorPickerDialog();
            humanizeDialog = new HumanizeDialog();
            exportDialog = new ExportDialog();
            exportingDialog = new ExportingDialog();
            setFramePatternDialog = new SetFramePatternDialog();
            waveAddFuzzDialog = new WaveAddFuzzDialog();
            waveSampleAndHoldDialog = new WaveSampleAndHoldDialog();
            waveSmoothDialog = new WaveSmoothDialog();
            waveSyncDialog = new WaveSyncDialog();
            configurationDialog = new ConfigurationDialog();

            messageDialog = new MessageDialog();
        }

        
        public static void Update() {
            moduleSettings.Update();
            preferences.Update();
            humanizeDialog.Update();
            exportDialog.Update();
            exportingDialog.Update();
            setFramePatternDialog.Update();
            waveAddFuzzDialog.Update();
            waveSampleAndHoldDialog.Update();
            waveSmoothDialog.Update();
            waveSyncDialog.Update();
            configurationDialog.Update();
            colorPicker.Update();

            messageDialog.Update();
        }

        public static void Draw() {
            moduleSettings.Draw();
            preferences.Draw();
            humanizeDialog.Draw();
            exportDialog.Draw();
            exportingDialog.Draw();
            setFramePatternDialog.Draw();
            waveAddFuzzDialog.Draw();
            waveSampleAndHoldDialog.Draw();
            waveSmoothDialog.Draw();
            waveSyncDialog.Draw();
            configurationDialog.Draw();
            colorPicker.Draw();


            messageDialog.Draw();
        }
    }
}
