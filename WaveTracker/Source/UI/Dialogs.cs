using System;
using System.Collections.Generic;

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
        public static WaveMathExpressionDialog waveMathExpressionDialog;
        public static SampleModifyDialog currentSampleModifyDialog;
        public static ConfigurationDialog configurationDialog;
        private static MessageDialog currentMessageDialog;
        private static Queue<MessageDialog> messageDialogs;

        public static MessageDialog messageDialog;

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
            waveMathExpressionDialog = new WaveMathExpressionDialog();
            configurationDialog = new ConfigurationDialog();

            messageDialogs = new Queue<MessageDialog>();
        }
        /// <summary>
        /// Opens a message dialog
        /// </summary>
        /// <param name="message"></param>
        /// <param name="icon"></param>
        /// <param name="buttonNames"></param>
        /// <param name="playSound"></param>
        public static void OpenMessageDialog(string message, MessageDialog.Icon icon, string[] buttonNames, Action<string> onExitCallback, bool playSound = true) {
            messageDialogs.Enqueue(new MessageDialog(message, icon, buttonNames, onExitCallback, playSound));
            if (currentMessageDialog == null) {
                currentMessageDialog = messageDialogs.Dequeue();
                currentMessageDialog.Open();
            }
        }

        /// <summary>
        /// Opens a message dialog
        /// </summary>
        /// <param name="message"></param>
        /// <param name="icon"></param>
        /// <param name="buttonName"></param>
        /// <param name="playSound"></param>
        public static void OpenMessageDialog(string message, MessageDialog.Icon icon, string buttonName, bool playSound = true) {
            messageDialogs.Enqueue(new MessageDialog(message, icon, [buttonName], null, playSound));
            if (currentMessageDialog == null) {
                currentMessageDialog = messageDialogs.Dequeue();
                currentMessageDialog.Open();
            }
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
            waveMathExpressionDialog.Update();
            configurationDialog.Update();
            colorPicker.Update();
            currentSampleModifyDialog?.Update();
            if (currentMessageDialog != null && currentMessageDialog.WindowIsOpen) {
                currentMessageDialog.Update();
            }
            else {
                if (messageDialogs.TryDequeue(out MessageDialog dialog)) {
                    currentMessageDialog = dialog;
                    currentMessageDialog.Open();
                }
                else {
                    currentMessageDialog = null;
                }
            }
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
            waveMathExpressionDialog.Draw();
            configurationDialog.Draw();
            colorPicker.Draw();
            currentSampleModifyDialog?.Draw();
            currentMessageDialog?.Draw();
        }
    }
}
