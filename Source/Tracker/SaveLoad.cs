using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WaveTracker.Audio;
using WaveTracker.Tracker;
using WaveTracker.UI;

namespace WaveTracker {
    public static class SaveLoad {
        /// <summary>
        /// Returns true if the current module is saved
        /// </summary>
        public static bool IsSaved { get { return !App.CurrentModule.IsDirty; } }

        /// <summary>
        /// The full filepath of the current file
        /// </summary>
        public static string CurrentFilepath { get; private set; }

        /// <summary>
        /// Maximum length of the recent files list
        /// </summary>
        private const int MAX_RECENT_FILES = 10;

        /// <summary>
        /// Holds paths to the last recently opened files
        /// </summary>
        private static List<string> recentFilePaths = [];

        /// <summary>
        /// The path of the folder containing themes
        /// </summary>
        public static string ThemeFolderPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "WaveTracker", "Themes"); } }
        /// <summary>
        /// The path of the folder containing the app settings
        /// </summary>
        public static string SettingsFolderPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "WaveTracker"); } }
        /// <summary>
        /// The path of the configuration file
        /// </summary>
        public static string SettingsPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "WaveTracker", "wtpref"); } }
        /// <summary>
        /// The path of the folder containing all saved paths
        /// </summary>
        public static string PathsFolderPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "WaveTracker", "path"); } }
        /// <summary>
        /// The current filename, including the extension
        /// </summary>
        public static string FileName { get { return CurrentFilepath == "" ? "Untitled.wtm" : Path.GetFileName(CurrentFilepath); } }
        /// <summary>
        /// The current filename, without the extension
        /// </summary>
        public static string FileNameWithoutExtension { get { return CurrentFilepath == "" ? "Untitled" : Path.GetFileNameWithoutExtension(CurrentFilepath); } }
        public static int savecooldown = 0;

        /// <summary>
        /// Writes the current module to <c>path</c>
        /// </summary>
        /// <param name="path"></param>
        private static void WriteTo(string path) {
            Debug.WriteLine("Saving to: " + path);
            Stopwatch stopwatch = Stopwatch.StartNew();

            using (FileStream fs = new FileStream(path, FileMode.Create)) {
                Serializer.Serialize(fs, App.CurrentModule);
            }

            stopwatch.Stop();
            App.CurrentModule.OnSaveModule();
            Debug.WriteLine("saved in " + stopwatch.ElapsedMilliseconds + " ms");
            return;

        }

        /// <summary>
        /// Calls SaveFile() as a void instead of bool
        /// </summary>
        public static void SaveFileVoid() {
            _ = SaveFile();
        }

        /// <summary>
        /// Saves the file to the current filepath, if it does not exist, prompts a save as dialog.
        /// </summary>
        /// <returns></returns>
        public static bool SaveFile() {
            if (savecooldown == 0) {
                savecooldown = 4;
                if (!File.Exists(CurrentFilepath)) {
                    return SaveFileAs();
                }
                else {
                    WriteTo(CurrentFilepath);
                    return true;
                }
            }
            savecooldown = 4;
            return false;
        }

        /// <summary>
        /// Prompts to save any unsaved changes, then loads a blank file.
        /// </summary>
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
        private static void NewFileUnsavedChangesCallback(string ret) {
            if (ret == "Yes") {
                if (!File.Exists(CurrentFilepath)) {
                    _ = SaveFileAs();
                }
                else {
                    WriteTo(CurrentFilepath);
                }
                _ = SaveFile();
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
        private static void OpenANewFile() {
            CurrentFilepath = "";
            //FrameEditor.ClearHistory();
            //FrameEditor.Goto(0, 0);
            //FrameEditor.cursorColumn = 0;
            //FrameEditor.UnmuteAllChannels();
            App.CurrentModule = new WTModule();
            App.CurrentSongIndex = 0;
            App.PatternEditor.OnSwitchSong(true);
            Playback.Goto(0, 0);
            WaveBank.currentWaveID = 0;
            WaveBank.lastSelectedWave = 0;
            ChannelManager.Reset();
            ChannelManager.UnmuteAllChannels();
            ChannelManager.PreviewChannel.SetWave(0);
        }

        public static void SaveFileAsVoid() {
            _ = SaveFileAs();
        }
        public static bool SaveFileAs() {
            if (Input.focus != null) {
                return false;
            }
            Playback.Stop();
            // set filepath to dialogresult
            if (SetFilePathThroughSaveAsDialog(out string filepath)) {
                CurrentFilepath = filepath;
                Debug.WriteLine("Saving as: " + CurrentFilepath);
                WriteTo(CurrentFilepath);
                Debug.WriteLine("Saved as: " + CurrentFilepath);
                App.CurrentModule.OnSaveModule();
                return true;
            }
            return false;

        }

        /// <summary>
        /// Prompts the user to save any unsaved changes, then creates an 'open file' dialog and loads the file if one was selected
        /// </summary>
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

        public static Menu CreateFileMenu() {
            return new Menu([
                new MenuOption("New", NewFile),
                new MenuOption("Open...", OpenFile),
                new MenuOption("Save", SaveFileVoid),
                new MenuOption("Save As...", SaveFileAsVoid),
                null,
                new MenuOption("Export as WAV...", Dialogs.exportDialog.Open),
                null,
                new SubMenu("Recent Files", CreateRecentFilesMenu()),
                null,
                new MenuOption("Exit", App.ExitApplication),
            ]);
        }

        private static MenuItemBase[] CreateRecentFilesMenu() {
            if (recentFilePaths.Count == 0) {
                return [new MenuOption("Clear", recentFilePaths.Clear)];
            }
            else {
                MenuItemBase[] menu = new MenuItemBase[recentFilePaths.Count + 2];
                menu[0] = new MenuOption("Clear", recentFilePaths.Clear);
                menu[1] = null;
                for (int i = 0; i < recentFilePaths.Count; i++) {
                    menu[i + 2] = new MenuOption(i + 1 + ". " + recentFilePaths[i], TryToLoadFile, recentFilePaths[i]);
                }
                return menu;
            }
        }

        /// <summary>
        /// Reads paths from the 'recent.path' saved in settings into recentFilePaths
        /// </summary>
        public static void ReadRecentFiles() {
            recentFilePaths = [];
            recentFilePaths = ReadPaths("recent", []).ToList();
        }

        /// <summary>
        /// Adds a path to recentFilePaths and saves it to 'recent.path'
        /// </summary>
        /// <param name="path"></param>
        public static void AddPathToRecentFiles(string path) {
            _ = recentFilePaths.Remove(path);
            recentFilePaths.Insert(0, path);
            if (recentFilePaths.Count > MAX_RECENT_FILES) {
                recentFilePaths.RemoveAt(recentFilePaths.Count - 1);
            }
            SavePaths("recent", recentFilePaths.ToArray());
        }

        /// <summary>
        /// Attempts to open the file at <c>path</c>. Displays an error dialog if opening failed.
        /// </summary>
        /// <param name="path"></param>
        private static void TryToLoadFile(string path) {
            string currentPath = CurrentFilepath;

            if (LoadFile(path)) {

                Visualizer.GenerateWaveColors();
                App.CurrentModule.OnSaveModule();
                App.CurrentSongIndex = 0;
                App.PatternEditor.OnSwitchSong(true);
                WaveBank.currentWaveID = 0;
                WaveBank.lastSelectedWave = 0;
                Playback.Goto(0, 0);
                ChannelManager.Reset();
                ChannelManager.UnmuteAllChannels();
                ChannelManager.PreviewChannel.SetWave(0);
                CurrentFilepath = path;
                AddPathToRecentFiles(CurrentFilepath);
            }
            else {
                Dialogs.messageDialog.Open(
                    "Could not open " + Path.GetFileName(path),
                    MessageDialog.Icon.Error,
                    "OK"
                    );
                CurrentFilepath = currentPath;

            }
        }

        /// <summary>
        /// Opens the open dialog and tries to load the chosen file<br/>
        /// Opens the error dialog if failed
        /// </summary>
        private static void ChooseFileToOpenAndLoad() {
            if (SetFilePathThroughOpenDialog(out string filepath)) {
                TryToLoadFile(filepath);
            }
        }

        /// <summary>
        /// Loads a module from the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns><c>true</c> if successfully loaded<br/>
        /// <c>false</c> if loading failed</returns>
        public static bool LoadFile(string path) {
            if (!File.Exists(path)) {
                return false;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                // try loading the module
                using (FileStream fs = new FileStream(path, FileMode.Open)) {
                    fs.Position = 0;
                    App.CurrentModule = Serializer.Deserialize<WTModule>(fs);
                }
            } catch {
                // invalid file format or file is corrupted
                return false;
            }
            stopwatch.Stop();
            Debug.WriteLine("opened in " + stopwatch.ElapsedMilliseconds + " ms");
            CurrentFilepath = path;
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
                    ["Yes", "No", "Cancel"],
                    callback
                    );
        }

        /// <summary>
        /// If the user tried to open a file while changes were present on the current file.
        /// This handles the results from the dialog that pops up
        /// </summary>
        /// <param name="result"></param>
        private static void OpenUnsavedCallback(string result) {
            if (result == "Yes") {
                if (SaveFile()) {
                    ChooseFileToOpenAndLoad();
                }
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
        private static bool SetFilePathThroughOpenDialog(out string filepath) {
            bool didIt = false;
            string ret = "";
            filepath = CurrentFilepath;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.InitialDirectory = ReadPath("openwtm", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    openFileDialog.Filter = "WaveTracker modules (*wtm)|*.wtm";
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = "Open";
                    openFileDialog.ValidateNames = true;
                    openFileDialog.CheckPathExists = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = openFileDialog.FileName;
                        SavePath("openwtm", Directory.GetParent(ret).FullName + "");

                        didIt = true;
                    }

                });

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
        private static bool SetFilePathThroughSaveAsDialog(out string filepath) {
            string ret = "";
            bool didIt = false;
            filepath = CurrentFilepath;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.InitialDirectory = ReadPath("savewtm", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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
                        SavePath("savewtm", Directory.GetParent(ret).FullName + "");
                        AddPathToRecentFiles(ret);
                        didIt = true;
                    }

                });

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
                Thread t = new Thread(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = "wav";
                    saveFileDialog.InitialDirectory = ReadPath("export", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.FileName = FileNameWithoutExtension;
                    saveFileDialog.Title = "Export .wav";
                    saveFileDialog.Filter = "Waveform Audio File Format (*.wav)|*.wav|All files (*.*)|*.*";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.ValidateNames = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = saveFileDialog.FileName;
                        SavePath("export", Directory.GetParent(ret).FullName + "");

                        didIt = true;
                    }

                });

                t.SetApartmentState(ApartmentState.STA);
                App.ForceUpdate();
                t.Start();
                t.Join();
            }
            filepath = ret;
            return didIt;
        }

        /// <summary>
        /// Opens a file browser and asks the user to choose a path to export to. (Export .wav)
        /// </summary>
        /// <returns>Returns true if they choose a file and accept, false otherwise.</returns>
        public static bool ChooseThemeSaveLocation(out string filepath) {
            string ret = "";
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = "wttheme";
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.InitialDirectory = ThemeFolderPath;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.FileName = "New theme";
                    saveFileDialog.Title = "Save theme";
                    saveFileDialog.Filter = "WaveTracker Theme (*.wttheme)|*.wttheme|All files (*.*)|*.*";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.ValidateNames = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = saveFileDialog.FileName;
                        didIt = true;
                    }

                });

                t.SetApartmentState(ApartmentState.STA);
                App.ForceUpdate();
                t.Start();
                t.Join();
            }
            filepath = ret;
            return didIt;
        }

        /// <summary>
        /// Opens a file browser and asks the user to choose a wtm file. (Open)
        /// </summary>
        /// <returns>true if they choose a file and accept, false otherwise.</returns>
        public static bool GetThemePathThroughOpenDialog(out string filepath) {
            bool didIt = false;
            string ret = "";
            filepath = CurrentFilepath;
            if (Input.dialogOpenCooldown == 0) {
                Thread t = new Thread(() => {
                    Input.DialogStarted();
                    Input.CancelClick();
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.InitialDirectory = ThemeFolderPath;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.Filter = "WaveTracker Theme (*.wttheme)|*.wttheme";
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = "Open";
                    openFileDialog.ValidateNames = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK) {
                        ret = openFileDialog.FileName;
                        didIt = true;
                    }

                });

                t.SetApartmentState(ApartmentState.STA);
                App.ForceUpdate();
                t.Start();
                t.Join();
                filepath = ret;

            }
            return didIt;
        }

        /// <summary>
        /// Saves a path to a file in the paths folder
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="path"></param>
        public static void SavePath(string pathName, string path) {
            _ = Directory.CreateDirectory(PathsFolderPath);
            File.WriteAllText(Path.Combine(PathsFolderPath, pathName + ".path"), path);
        }
        /// <summary>
        /// Reads a saved path from the paths folder into a string, returns the default path if it does not exist.
        /// </summary>
        /// <param name="pathName"></param>WaveTracker\path
        /// <param name="defaultPath"></param>
        /// <returns></returns>
        public static string ReadPath(string pathName, string defaultPath) {
            string filepath = Path.Combine(PathsFolderPath, pathName + ".path");
            if (File.Exists(filepath)) {
                return File.ReadAllLines(filepath)[0];
            }
            else {
                SavePath(pathName, defaultPath);
                return defaultPath;
            }
        }

        /// <summary>
        /// Saves paths to a file in the paths folder
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="paths"></param>
        public static void SavePaths(string pathName, string[] paths) {
            _ = Directory.CreateDirectory(PathsFolderPath);
            File.WriteAllLines(Path.Combine(PathsFolderPath, pathName + ".path"), paths);
        }
        /// <summary>
        /// Reads saved paths from the paths folder into a string array, returns the default paths if it does not exist.
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="defaultPaths"></param>
        /// <returns></returns>
        public static string[] ReadPaths(string pathName, string[] defaultPaths) {
            string filepath = Path.Combine(PathsFolderPath, pathName + ".path");
            if (File.Exists(filepath)) {
                return File.ReadAllLines(filepath);
            }
            else {
                SavePaths(pathName, defaultPaths);
                return defaultPaths;
            }
        }
    }
}
