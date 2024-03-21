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


        static void SaveTo(string path) {
            Debug.WriteLine("Saving to: " + path);
            Stopwatch stopwatch = Stopwatch.StartNew();
            //BinaryFormatter formatter = new BinaryFormatter();

            try {
                //savedModule = App.CurrentModule.Clone();
            } catch {
                Debug.WriteLine("failed to save");
                return;
            }

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

        public static void SaveFile() {
            if (savecooldown == 0)
                if (!File.Exists(filePath)) {
                    SaveFileAs();
                }
                else {
                    SaveTo(filePath);
                }
            savecooldown = 4;

        }

        public static void NewFile() {
            if (Input.focus != null)
                return;

            if (!IsSaved) {
                if (PromptUnsaved() == DialogResult.Cancel) return;
            }
            Playback.Stop();
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

        public static void SaveFileAs() {
            if (Input.focus != null)
                return;
            Playback.Stop();
            // set filepath to dialogresult
            if (SetFilePathThroughSaveAsDialog(out filePath)) {
                Debug.WriteLine("Saving as: " + filePath);
                SaveTo(filePath);
                Debug.WriteLine("Saved as: " + filePath);
                App.CurrentModule.OnSaveModule();
            }

        }

        public static void OpenFile() {
            if (Input.internalDialogIsOpen)
                return;
            Playback.Stop();
            if (savecooldown == 0) {
                // set filepath to dialog result
                string currentPath = filePath;
                if (!IsSaved) {
                    if (PromptUnsaved() == DialogResult.Cancel) {
                        return;
                    }
                    Input.dialogOpenCooldown = 0;
                }
                if (SetFilePathThroughOpenDialog())
                    if (LoadFrom(filePath)) {
                        Visualization.GetWaveColors();
                        ChannelManager.Reset();
                        ChannelManager.UnmuteAllChannels();
                        App.CurrentModule.OnSaveModule();
                        App.CurrentSongIndex = 0;
                        App.PatternEditor.OnSwitchSong();
                        Playback.Goto(0, 0);
                    }
                    else {
                        LoadError();
                        filePath = currentPath;

                    }

            }
            savecooldown = 4;
        }

        /// <summary>
        /// Loads a module from the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns><c>true</c> if successfully loaded<br></br>
        /// <c>false</c> if loading failed</returns>
        public static bool LoadFrom(string path) {
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



        public static bool SetFilePathThroughOpenDialog() {
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread((ThreadStart)(() => {

                    Input.DialogStarted();
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "WaveTracker modules (*wtm)|*.wtm";
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = "Open";
                    openFileDialog.ValidateNames = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK) {
                        filePath = openFileDialog.FileName;

                        didIt = true;
                    }

                }));

                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

            }
            return didIt;
        }

        public static DialogResult PromptUnsaved() {
            DialogResult ret = DialogResult.Cancel;
            if (Input.dialogOpenCooldown == 0) {
                Input.DialogStarted();
                ret = MessageBox.Show("Save changes to " + FileName + "?", "WaveTracker", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            }
            if (ret == DialogResult.Yes) {
                SaveFile();
            }
            return ret;
        }

        public static void LoadError() {
            if (Input.dialogOpenCooldown == 0) {
                Input.DialogStarted();

                MessageBox.Show("Could not open " + FileName, "WaveTracker", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public static bool SetFilePathThroughSaveAsDialog(out string filepath) {
            string ret = "";
            bool didIt = false;
            filepath = filePath;
            if (Input.dialogOpenCooldown == 0) {
                Input.DialogStarted();
                Thread t = new Thread((ThreadStart)(() => {

                    Input.DialogStarted();
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
                t.Start();
                t.Join();
                filepath = ret;
            }
            return didIt;
        }

        public static bool ChooseExportPath(out string filepath) {
            string ret = "";
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread((ThreadStart)(() => {

                    Input.DialogStarted();
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
                t.Start();
                t.Join();
            }
            filepath = ret;
            return didIt;
        }
    }
}
