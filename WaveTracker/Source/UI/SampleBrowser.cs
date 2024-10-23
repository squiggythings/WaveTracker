using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class SampleBrowser : Element {
        public bool enabled;
        private int selectedFileIndex = -1;
        public string selectedFilePath;
        private Toggle sortName, sortType;
        private SpriteButton backButton;
        private Scrollbar scrollbar;
        private Button ok, cancel;
        private CheckboxLabeled loopPreview;
        private string currentPath = @"C:\";
        private string lastPath;
        private int listLength = 24;
        public string[] entriesInDirectory;
        private Element opened;
        private SampleEditor launched;
        private int width = 500;
        private int height = 320;

        private string audioFilePath;
        private AudioReader audioFile;

        private enum SortingMethod { ByName, ByType };

        private SortingMethod sortMethod;

        private bool SelectedAnAudioFile {
            get {
                return audioFile != null && selectedFileIndex >= 0 && selectedFileIndex < entriesInDirectory.Length && File.Exists(entriesInDirectory[selectedFileIndex]);
            }
        }

        public SampleBrowser() {
            x = (960 - width) / 2;
            y = (500 - height) / 2;
            backButton = new SpriteButton(2, 11, 15, 15, 300, 0, this);
            scrollbar = new Scrollbar(2, 29, width - 111, listLength * 11, this);
            scrollbar.CoarseStepAmount = 3;
            ok = new Button("OK", width - 108, height - 16, this);
            ok.width = 51;
            cancel = new Button("Cancel", width - 54, height - 16, this);
            cancel.width = 51;
            sortName = new Toggle("Name", width - 65, 30, this);
            sortType = new Toggle("Type", width - 36, 30, this);
            loopPreview = new CheckboxLabeled("Loop", width - 105, 125, 40, this);
            entriesInDirectory = [];
            currentPath = SaveLoad.ReadPath("sample", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        public void Update() {
            if (enabled) {
                sortName.Value = sortMethod == SortingMethod.ByName;
                sortType.Value = sortMethod == SortingMethod.ByType;
                if (sortName.Clicked) {
                    sortMethod = SortingMethod.ByName;
                    GetFileEntries(true);
                    selectedFileIndex = -1;
                    scrollbar.ScrollValue = 0;
                    AudioEngine.PreviewStream = null;
                }
                if (sortType.Clicked) {
                    sortMethod = SortingMethod.ByType;
                    GetFileEntries(true);
                    selectedFileIndex = -1;
                    scrollbar.ScrollValue = 0;
                    AudioEngine.PreviewStream = null;
                }
                sortName.Value = sortMethod == SortingMethod.ByName;
                sortType.Value = sortMethod == SortingMethod.ByType;

                backButton.enabled = currentPath != "";
                if (backButton.Clicked) {
                    selectedFileIndex = -1;
                    currentPath = Directory.GetParent(currentPath) == null ? "" : Directory.GetParent(currentPath).ToString();
                    scrollbar.ScrollValue = 0;
                    AudioEngine.PreviewStream = null;
                }
                if (ok.Clicked) {
                    selectedFilePath = entriesInDirectory[selectedFileIndex];
                    Close();
                    return;
                }
                if (cancel.Clicked) {
                    selectedFilePath = "";
                    Close();
                    return;
                }
                GetFileEntries(false);

                int y = 29;
                bool newFile = false;
                for (int i = scrollbar.ScrollValue; i < listLength + scrollbar.ScrollValue; i++) {
                    if (i < entriesInDirectory.Length) {
                        if (MouseX > 2 && MouseX < width - 117) {
                            if (MouseY > y && MouseY <= y + 11) {
                                if (Input.GetClickDown(KeyModifier.None)) {

                                    if (selectedFileIndex != i) {
                                        newFile = true;
                                    }

                                    selectedFileIndex = i;
                                    if (newFile) {
                                        SelectedANewEntry();
                                        break;
                                    }

                                }
                                if (Input.GetDoubleClickDown(KeyModifier.None)) {
                                    if (Directory.Exists(entriesInDirectory[i])) {
                                        // double clicked on a folder
                                        currentPath = entriesInDirectory[i];
                                        selectedFileIndex = -1;
                                        scrollbar.ScrollValue = 0;
                                        GetFileEntries(false);
                                        break;
                                    }
                                    else {
                                        // double clicked on a file
                                        selectedFilePath = entriesInDirectory[selectedFileIndex];
                                        Close();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    y += 11;
                }
                if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None)) {
                    selectedFileIndex--;
                    if (selectedFileIndex < 0) {
                        selectedFileIndex = 0;
                    }

                    MoveBounds();
                    SelectedANewEntry();
                }
                if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None)) {
                    selectedFileIndex++;
                    if (selectedFileIndex > entriesInDirectory.Length - 1) {
                        selectedFileIndex = entriesInDirectory.Length - 1;
                    }

                    SelectedANewEntry();
                    MoveBounds();
                }
                scrollbar.SetSize(entriesInDirectory.Length, listLength);
                scrollbar.ScrollValue = Math.Clamp(scrollbar.ScrollValue, 0, Math.Clamp(entriesInDirectory.Length - listLength, 0, 999999));
                scrollbar.UpdateScrollValue();
                scrollbar.Update();
                if (SelectedAnAudioFile) {
                    loopPreview.Update();
                    if (loopPreview.Clicked) {
                        SelectedANewEntry();
                    }
                }
                ok.enabled = SelectedAnAudioFile;
            }
        }

        public void MoveBounds() {
            if (selectedFileIndex > scrollbar.ScrollValue + listLength - 1) {
                scrollbar.ScrollValue = selectedFileIndex - listLength + 1;
            }
            if (selectedFileIndex < scrollbar.ScrollValue) {
                scrollbar.ScrollValue = selectedFileIndex;
            }
            scrollbar.SetSize(entriesInDirectory.Length, listLength);
            scrollbar.ScrollValue = Math.Clamp(scrollbar.ScrollValue, 0, Math.Clamp(entriesInDirectory.Length - listLength, 0, 999999));
            scrollbar.UpdateScrollValue();

        }

        private void SelectedANewEntry() {
            AudioEngine.PreviewStream = null;
            if (App.Settings.SamplesWaves.PreviewSamplesInBrowser) {
                if (File.Exists(entriesInDirectory[selectedFileIndex])) {
                    try {
                        audioFilePath = entriesInDirectory[selectedFileIndex];

                        if (audioFile != null) {
                            audioFile.Dispose();
                            audioFile = null;
                        }

                        audioFile = new AudioReader(audioFilePath, loopPreview.Value);
                        AudioEngine.PreviewStream = audioFile;
                    } catch {

                    }
                }
            }
        }

        private void GetFileEntries(bool overrideOptimization) {
            if (currentPath != lastPath || overrideOptimization) {
                if (!Path.Exists(currentPath)) {
                    currentPath = "";
                }
                // the topmost in the tree, choosing a drive
                lastPath = currentPath;
                List<string> entries = [];
                if (currentPath == "") {

                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo drive in allDrives) {
                        entries.Add(drive.RootDirectory.FullName);
                    }
                    entriesInDirectory = entries.ToArray();
                    return;
                }

                // a drive is already chosen, use the regular Directory system
                entries = Directory.GetFileSystemEntries(currentPath, "*", SearchOption.TopDirectoryOnly).ToList();
                for (int i = entries.Count - 1; i >= 0; i--) {
                    if ((File.GetAttributes(entries[i]) & FileAttributes.Hidden) == FileAttributes.Hidden) {
                        entries.RemoveAt(i);
                        continue;
                    }
                    if (Directory.Exists(entries[i])) {
                        continue;
                    }
                    else {
                        if (File.Exists(entries[i])) {
                            string ext = Path.GetExtension(entries[i]);
                            if (string.Compare(ext, ".wav", true) == 0 ||
                                string.Compare(ext, ".mp3", true) == 0 ||
                                string.Compare(ext, ".flac", true) == 0 ||
                                string.Compare(ext, ".aiff", true) == 0) {
                                continue;
                            }
                        }
                        entries.RemoveAt(i);
                    }
                }
                if (sortMethod == SortingMethod.ByName) {
                    entries.Sort();
                }

                if (sortMethod == SortingMethod.ByType) {
                    entries.Sort(SortByType);
                }

                entriesInDirectory = entries.ToArray();
            }
        }

        private static int SortByType(string a, string b) {
            int val = Path.GetExtension(a).CompareTo(Path.GetExtension(b));
            return val == 0 ? a.CompareTo(b) : val;
        }
        public void Open(SampleEditor launched) {
            AudioEngine.PreviewStream = null;
            opened = Input.focus;
            this.launched = launched;
            selectedFileIndex = -1;
            scrollbar.ScrollValue = 0;
            enabled = true;
            x = (App.WindowWidth - width) / 2;
            y = (App.WindowHeight - height) / 2;
            Input.focus = this;
            if (!Path.Exists(currentPath)) {
                SaveLoad.ReadPath("sample", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
            GetFileEntries(true);
        }

        public void Close() {
            enabled = false;
            Input.focus = opened;
            if (File.Exists(selectedFilePath)) {
                LoadSampleFromFile(selectedFilePath, launched.Sample);
                launched.ClearHistory();
            }

            SaveLoad.SavePath("sample", currentPath);
            AudioEngine.PreviewStream = null;
        }

        private void LoadSampleFromFile(string path, Sample sample) {
            bool successfulRead = Helpers.ReadAudioFile(path, out sample.sampleDataL, out sample.sampleDataR, out sample.sampleRate);
            sample.resampleMode = App.Settings.SamplesWaves.DefaultResampleModeSample;
            sample.SetBaseKey(App.Settings.SamplesWaves.DefaultSampleBaseKey);
            sample.SetDetune(0);
            sample.loopPoint = 0;
            sample.loopType = loopPreview.Value ? Sample.LoopType.Forward : Sample.LoopType.OneShot;
            if (successfulRead) {
                sample.name = Path.GetFileNameWithoutExtension(path);

                if (App.Settings.SamplesWaves.AutomaticallyTrimSamples) {
                    sample.TrimSilence();
                }

                if (App.Settings.SamplesWaves.AutomaticallyNormalizeSamplesOnImport) {
                    sample.Normalize();
                }

                sample.resampleMode = App.Settings.SamplesWaves.DefaultResampleModeSample;
            }
            else {
                sample.name = null;
                sample.sampleDataL = [];
                sample.sampleDataR = [];
            }
            App.CurrentModule.SetDirty();
        }

        private void DrawList() {
            Color odd = new Color(43, 49, 81);
            Color even = new Color(59, 68, 107);
            Color selected = UIColors.selection;
            int y = 0;
            for (int i = scrollbar.ScrollValue; i < listLength + scrollbar.ScrollValue; i++) {
                Color row = i == selectedFileIndex ? selected : i % 2 == 0 ? even : odd;
                DrawRect(2, 29 + y * 11, width - 111, 11, row);
                if (entriesInDirectory.Length > i && i >= 0) {
                    if (currentPath == "") {
                        Write(entriesInDirectory[i], 20, 31 + y * 11, Color.White);
                    }
                    else {
                        Write(Path.GetFileName(entriesInDirectory[i]), 20, 31 + y * 11, Color.White);
                    }

                    if (Directory.Exists(entriesInDirectory[i])) { // draw folder icon
                        DrawSprite(5, 29 + y * 11, new Rectangle(72, 80, 12, 11));
                    }
                    else if (File.Exists(entriesInDirectory[i])) { // draw audio file icon
                        DrawSprite(5, 29 + y * 11, new Rectangle(72, 91, 12, 11));
                    }
                }
                ++y;
            }
            scrollbar.Draw();
        }

        private string GetNicePathString(string path) {

            string ret = "";
            for (int i = path.Length - 1; i >= 0; i--) {
                char c = path[i];
                if (c is '\\' or '/') {
                    if (Helpers.GetWidthOfText(ret) > width - 200) {
                        return "... > " + ret;
                    }

                    ret = " > " + ret;
                }
                else {
                    ret = c + ret;
                }
            }
            return ret;
        }

        public void Draw() {
            if (enabled) {
                // black box across screen behind window
                DrawRect(-x, -y, App.WindowWidth, App.WindowHeight, Helpers.Alpha(Color.Black, 90));

                DrawRoundedRect(0, 0, width, height, UIColors.panel);
                DrawRect(1, 0, width - 2, 1, Color.White);
                DrawRect(0, 1, width, 26, Color.White);
                Write("Choose sample... (.wav, .mp3, .flac)", 4, 1, UIColors.panelTitle);
                DrawList();
                backButton.Draw();
                ok.Draw();
                cancel.Draw();
                Write(GetNicePathString(currentPath), 20, 15, UIColors.label);
                Write("Sort by:", width - 104, 32, UIColors.labelDark);
                sortName.Draw();
                sortType.Draw();

                if (SelectedAnAudioFile) {
                    // write file name
                    if (audioFile != null) {
                        Write(Helpers.TrimTextToWidth(105, Path.GetFileName(audioFilePath)), width - 104, 85, UIColors.label);
                        Write(audioFile.NumChannels == 1 ? "Mono" : "Stereo", width - 104, 95, UIColors.label);
                        Write(audioFile.SampleRate + " Hz", width - 104, 105, UIColors.label);
                        Write(audioFile.Duration.TotalSeconds + " sec", width - 104, 115, UIColors.label);
                        loopPreview.Draw();
                    }
                }
            }
        }
    }
}
