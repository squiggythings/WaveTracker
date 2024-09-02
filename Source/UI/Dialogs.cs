using WaveTracker.Forms;

namespace WaveTracker.UI {
    public static class Dialogs {
        public static ModuleSettingsDialog moduleSettings;
        public static ColorPickerDialog colorPicker;
        public static HumanizeDialog humanizeDialog;
        public static StretchDialog stretchDialog;
        public static ExportDialog exportDialog;
        public static ExportingDialog exportingDialog;
        public static SetFramePatternDialog setFramePatternDialog;
        public static WaveAddFuzzDialog waveAddFuzzDialog;
        public static WaveSampleAndHoldDialog waveSampleAndHoldDialog;
        public static WaveSmoothDialog waveSmoothDialog;
        public static WaveSyncDialog waveSyncDialog;
        public static ConfigurationDialog configurationDialog;

        public static MessageDialog messageDialog;
        public static EnterText enterTextDialog;

        public static void Initialize() {
            moduleSettings = new ModuleSettingsDialog();
            colorPicker = new ColorPickerDialog();
            humanizeDialog = new HumanizeDialog();
            stretchDialog = new StretchDialog();
            exportDialog = new ExportDialog();
            exportingDialog = new ExportingDialog();
            setFramePatternDialog = new SetFramePatternDialog();
            waveAddFuzzDialog = new WaveAddFuzzDialog();
            waveSampleAndHoldDialog = new WaveSampleAndHoldDialog();
            waveSmoothDialog = new WaveSmoothDialog();
            waveSyncDialog = new WaveSyncDialog();
            configurationDialog = new ConfigurationDialog();

            messageDialog = new MessageDialog();
            enterTextDialog = new EnterText("hmmm");
        }

        public static void Update() {
            moduleSettings.Update();
            humanizeDialog.Update();
            stretchDialog.Update();
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
            enterTextDialog.Update();
        }

        public static void Draw() {
            moduleSettings.Draw();
            humanizeDialog.Draw();
            stretchDialog.Draw();
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
            enterTextDialog.Draw();
        }
    }
}
