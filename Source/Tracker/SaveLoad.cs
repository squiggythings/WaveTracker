using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using System.IO;
using System.Text.Encodings;
using WaveTracker.Tracker;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using ProtoBuf;
using WaveTracker.Rendering;
using WaveTracker;
using WaveTracker.Audio;

namespace WaveTracker {
    public static class SaveLoad {
        public static bool IsSaved { get { return !App.CurrentModule.IsDirty; } }
        public static string filePath = "";

        public static string FileName { get { if (filePath == "") return "Untitled.wtm"; return Path.GetFileName(filePath); } }
        public static string FileNameWithoutExtension { get { if (filePath == "") return "Untitled"; return Path.GetFileNameWithoutExtension(filePath); } }
        public static int savecooldown = 0;


        static void WriteTo(string path) {
            Debug.WriteLine("Saving to: " + path);
            Stopwatch stopwatch = Stopwatch.StartNew();
            //BinaryFormatter formatter = new BinaryFormatter();

            //try {
            //    //savedModule = App.CurrentModule.Clone();
            //} catch {
            //    Debug.WriteLine("failed to save");
            //    return;
            //}

            //savedSong.InitializeForSerialization();
            //path = Path.ChangeExtension(path, ".wtm");
            using (FileStream fs = new FileStream(path, FileMode.Create)) {
                Serializer.Serialize(fs, App.CurrentModule);
            }
            //using (FileStream fs = new FileStream(path, FileMode.Create))
            //{
            //    formatter.Serialize(fs, savedSong);
            //}
            stopwatch.Stop();
            App.CurrentModule.OnSaveModule();
            Debug.WriteLine("saved in " + stopwatch.ElapsedMilliseconds + " ms");
            return;

        }

        public static void SaveFileVoid() {
            SaveFile();
        }

        public static bool SaveFile() {
            if (savecooldown == 0) {
                savecooldown = 4;
                if (!File.Exists(filePath)) {
                    return SaveFileAs();
                }
                else {
                    WriteTo(filePath);
                    return true;
                }
            }
            savecooldown = 4;
            return false;
        }


        public static void NewFile() {
            if (Input.focus != null) {
                return;
            }
            Playback.Stop();
            if (!IsSaved) {
                DoSaveChangesDialog(NewFileUnsavedChangesCallback);
            }
            else {
                OpenANewFile();
            }

        }

        /// <summary>
        /// Handles results to "Save changes to [file]?" when opening a new file
        /// </summary>
        /// <param name="ret"></param>
        static void NewFileUnsavedChangesCallback(string ret) {
            if (ret == "Yes") {
                if (!File.Exists(filePath)) {
                    SaveFileAs();
                }
                else {
                    WriteTo(filePath);
                }
                SaveFile();
                OpenANewFile();
            }
            if (ret == "No") {
                OpenANewFile();
            }
            if (ret == "Cancel") {
                return;
            }

        }

        /// <summary>
        /// Discards the current file and opens a new one
        /// </summary>
        static void OpenANewFile() {
            filePath = "";
            //FrameEditor.ClearHistory();
            //FrameEditor.Goto(0, 0);
            Playback.Goto(0, 0);
            ChannelManager.Reset();
            ChannelManager.UnmuteAllChannels();
            //FrameEditor.cursorColumn = 0;
            //FrameEditor.UnmuteAllChannels();
            Song.currentSong = null;
            App.CurrentModule = new WTModule();
            App.CurrentSongIndex = 0;
            App.PatternEditor.OnSwitchSong();
        }

        public static void SaveFileAsVoid() {
            SaveFileAs();
        }
        public static bool SaveFileAs() {
            if (Input.focus != null) {
                return false;
            }
            Playback.Stop();
            // set filepath to dialogresult
            if (SetFilePathThroughSaveAsDialog(out filePath)) {
                Debug.WriteLine("Saving as: " + filePath);
                WriteTo(filePath);
                Debug.WriteLine("Saved as: " + filePath);
                App.CurrentModule.OnSaveModule();
                return true;
            }
            return false;

        }

        public static void OpenFile() {
            if (Input.internalDialogIsOpen) {
                return;
            }
            Playback.Stop();
            if (savecooldown == 0) {
                // set filepath to dialog result
                if (!IsSaved) {
                    DoSaveChangesDialog(OpenUnsavedCallback);
                }
                else {
                    ChooseFileToOpenAndLoad();
                }
            }
            savecooldown = 4;
        }

        /// <summary>
        /// Opens the open dialog and tries to load the chosen file<br></br>
        /// Opens the error dialog if failed
        /// </summary>
        static void ChooseFileToOpenAndLoad() {
            string currentPath = filePath;
            if (SetFilePathThroughOpenDialog(out filePath)) {
                if (ReadFrom(filePath)) {
                    Visualization.GetWaveColors();
                    ChannelManager.Reset();
                    ChannelManager.UnmuteAllChannels();
                    App.CurrentModule.OnSaveModule();
                    App.CurrentSongIndex = 0;
                    App.PatternEditor.OnSwitchSong();
                    Debug.WriteLine("cursorPos" + App.PatternEditor.cursorPosition.ToString());
                    Playback.Goto(0, 0);
                }
                else {
                    Dialogs.messageDialog.Open(
                        "Could not open " + FileName,
                        MessageDialog.Icon.Error,
                        "OK"
                        );
                    filePath = currentPath;

                }
            }
        }

        /// <summary>
        /// Loads a module from the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns><c>true</c> if successfully loaded<br></br>
        /// <c>false</c> if loading failed</returns>
        public static bool ReadFrom(string path) {
            if (!File.Exists(path))
                return false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //BinaryFormatter formatter = new BinaryFormatter();

            //MemoryStream ms = new MemoryStream();
            //savedSong = new Song();

            try {
                // try loading the module regularly
                using (FileStream fs = new FileStream(path, FileMode.Open)) {
                    fs.Position = 0;
                    App.CurrentModule = Serializer.Deserialize<WTModule>(fs);
                }
            } catch {
                try {
                    // try converting from the old format
                    using (FileStream fs = new FileStream(path, FileMode.Open)) {
                        fs.Position = 0;
                        Song.currentSong = Serializer.Deserialize<Song>(fs);
                    }
                    Song.currentSong.Deserialize();
                    App.CurrentModule = WTModule.FromOldSongFormat(Song.currentSong);
                    Song.currentSong = null;
                } catch {
                    // invalid file format or file is corrupted
                    return false;
                }

            }
            stopwatch.Stop();
            Debug.WriteLine("opened in " + stopwatch.ElapsedMilliseconds + " ms");
            filePath = path;
            return true;
        }


        /// <summary>
        /// Opens a "Save changes to current file?" question box
        /// </summary>
        /// <param name="callback"></param>
        public static void DoSaveChangesDialog(Action<string> callback) {
            Dialogs.messageDialog.Open(
                    "Save changes to " + FileName + "?",
                    MessageDialog.Icon.Question,
                    new string[] { "Yes", "No", "Cancel" },
                    callback
                    );
        }

        /// <summary>
        /// If the user tried to open a file while changes were present on the current file.
        /// This handles the results from the dialog that pops up
        /// </summary>
        /// <param name="result"></param>
        static void OpenUnsavedCallback(string result) {
            if (result == "Yes") {
                if (SaveFile())
                    ChooseFileToOpenAndLoad();
            }
            if (result == "No") {
                ChooseFileToOpenAndLoad();
            }
            if (result == "Cancel") {
                return;
            }
        }


        /// <summary>
        /// Opens a file browser and asks the user to choose a wtm file. (Open)
        /// </summary>
        /// <returns>true if they choose a file and accept, false otherwise.</returns>
        static bool SetFilePathThroughOpenDialog(out string filepath) {
            bool didIt = false;
            string ret = "";
            filepath = filePath;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread((ThreadStart)(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "WaveTracker modules (*wtm)|*.wtm";
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = "Open";
                    openFileDialog.ValidateNames = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = openFileDialog.FileName;
                        didIt = true;
                    }

                }));

                t.SetApartmentState(ApartmentState.STA);
                App.ForceUpdate();
                t.Start();
                t.Join();
                filepath = ret;

            }
            return didIt;
        }

        /// <summary>
        /// Opens a file browser and asks the user to choose a path to save a wtm file. (Save As)
        /// </summary>
        /// <returns>Returns true if they choose a file and accept, false otherwise.</returns>
        static bool SetFilePathThroughSaveAsDialog(out string filepath) {
            string ret = "";
            bool didIt = false;
            filepath = filePath;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread((ThreadStart)(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = "wtm";
                    saveFileDialog.Filter = "WaveTracker modules (*.wtm)|*.wtm|All files (*.*)|*.*";
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.FileName = FileName;
                    saveFileDialog.Title = "Save As";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.ValidateNames = true;


                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = saveFileDialog.FileName;
                        didIt = true;
                    }

                }));

                t.SetApartmentState(ApartmentState.STA);
                App.ForceUpdate();
                t.Start();
                t.Join();
                filepath = ret;
            }
            return didIt;
        }
        /// <summary>
        /// Opens a file browser and asks the user to choose a path to export to. (Export .wav)
        /// </summary>
        /// <returns>Returns true if they choose a file and accept, false otherwise.</returns>
        public static bool ChooseExportPath(out string filepath) {
            string ret = "";
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread((ThreadStart)(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = "wav";
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.FileName = FileNameWithoutExtension;
                    saveFileDialog.Title = "Export .wav";
                    saveFileDialog.Filter = "Waveform Audio File Format (*.wav)|*.wav|All files (*.*)|*.*";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.ValidateNames = true;


                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = saveFileDialog.FileName;
                        didIt = true;
                    }

                }));

                t.SetApartmentState(ApartmentState.STA);
                App.ForceUpdate();
                t.Start();
                t.Join();
            }
            filepath = ret;
            return didIt;
        }
    }
}
