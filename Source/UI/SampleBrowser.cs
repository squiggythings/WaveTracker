using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NAudio.Wave;
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
        private string currentPath = @"C:\";
        private string lastPath;
        private int listLength = 24;
        public string[] entriesInDirectory;
        private Element opened;
        private SampleEditor launched;

        //InstrumentEditor launched;
        private int width = 500;
        private int height = 320;
        private WaveOutEvent previewOut;
        private AudioFileReader reader;

        private enum SortingMethod { ByName, ByType };

        private SortingMethod sortMethod;

        private bool SelectedAnAudioFile {
            get {
                return reader != null && selectedFileIndex >= 0 && selectedFileIndex < entriesInDirectory.Length && File.Exists(entriesInDirectory[selectedFileIndex]);
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
            entriesInDirectory = [];
            previewOut = new WaveOutEvent();
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
                    previewOut.Stop();
                }
                if (sortType.Clicked) {
                    sortMethod = SortingMethod.ByType;
                    GetFileEntries(true);
                    selectedFileIndex = -1;
                    scrollbar.ScrollValue = 0;
                    previewOut.Stop();
                }
                sortName.Value = sortMethod == SortingMethod.ByName;
                sortType.Value = sortMethod == SortingMethod.ByType;

                backButton.enabled = currentPath != "";
                if (backButton.Clicked) {
                    selectedFileIndex = -1;
                    currentPath = Directory.GetParent(currentPath) == null ? "" : Directory.GetParent(currentPath).ToString();
                    scrollbar.ScrollValue = 0;
                    previewOut.Stop();
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
                                if (Input.GetDoubleClick(KeyModifier.None)) {
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
            previewOut.Stop();
            if (File.Exists(entriesInDirectory[selectedFileIndex])) {
                //Thread.Sleep(1);
                try {
                    reader = new AudioFileReader(entriesInDirectory[selectedFileIndex]);
                    if (reader.TotalTime.TotalSeconds * reader.WaveFormat.SampleRate <= 400) {
                        LoopStream loop = new LoopStream(reader);
                        previewOut.Init(loop);
                    }
                    else {
                        previewOut.Init(reader);
                    }
                    if (App.Settings.SamplesWaves.PreviewSamplesInBrowser) {
                        previewOut.Volume = 0.75f * App.Settings.Audio.MasterVolume / 100f;
                        previewOut.Play();
                    }
                } catch {

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
            previewOut.Stop();
            opened = Input.focus;
            this.launched = launched;
            selectedFileIndex = -1;
            scrollbar.ScrollValue = 0;
            enabled = true;
            x = (App.WindowWidth - width) / 2;
            y = (App.WindowHeight - height) / 2;
            Input.focus = this;
            GetFileEntries(true);
        }

        public void Close() {
            enabled = false;
            Input.focus = opened;
            if (File.Exists(selectedFilePath)) {
                LoadSampleFromFile(selectedFilePath, launched.Sample);
            }

            SaveLoad.SavePath("sample", currentPath);
            previewOut.Stop();
        }

        public static void LoadSampleFromFile(string path, Sample sample) {
            bool successfulRead = Helpers.ReadAudioFile(path, out sample.sampleDataAccessL, out sample.sampleDataAccessR, out sample.sampleRate);
            sample.SetBaseKey(App.Settings.SamplesWaves.DefaultSampleBaseKey);
            sample.SetDetune(0);
            sample.loopPoint = 0;
            sample.loopType = sample.Length < 1000 ? Sample.LoopType.Forward : Sample.LoopType.OneShot;
            sample.resampleMode = App.Settings.SamplesWaves.DefaultResampleModeSample;
            sample.name = Path.GetFileName(path);
            if (successfulRead) {
                if (App.Settings.SamplesWaves.AutomaticallyTrimSamples) {
                    sample.TrimSilence();
                }

                if (App.Settings.SamplesWaves.AutomaticallyNormalizeSamplesOnImport) {
                    sample.Normalize();
                }

                sample.resampleMode = App.Settings.SamplesWaves.DefaultResampleModeSample;
                App.CurrentModule.SetDirty();
            }
            else {
                sample.sampleDataAccessL = [];
                sample.sampleDataAccessR = [];
                App.CurrentModule.SetDirty();
            }
        }

        public void DrawList() {
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
                        Write(Helpers.FlushString(Path.GetFileName(entriesInDirectory[i])), 20, 31 + y * 11, Color.White);
                    }

                    if (Directory.Exists(entriesInDirectory[i])) // draw folder icon
{
                        DrawSprite(5, 29 + y * 11, new Rectangle(72, 80, 12, 11));
                    }
                    else if (File.Exists(entriesInDirectory[i])) // draw audio file icon
{
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
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));

                DrawRoundedRect(0, 0, width, height, UIColors.panel);
                DrawRect(1, 0, width - 2, 1, Color.White);
                DrawRect(0, 1, width, 26, Color.White);
                Write("Choose sample... (.wav, .mp3, .flac)", 4, 1, UIColors.panelTitle);
                DrawList();
                backButton.Draw();
                ok.Draw();
                cancel.Draw();
                Write(Helpers.FlushString(GetNicePathString(currentPath)), 20, 15, UIColors.label);
                Write("Sort by:", width - 104, 31, UIColors.labelDark);
                sortName.Draw();
                sortType.Draw();
                if (SelectedAnAudioFile) {
                    // write file name
                    if (reader != null) {
                        Write(Helpers.FlushString(Helpers.TrimTextToWidth(105, Path.GetFileName(reader.FileName))), width - 106, 85, UIColors.label);
                        Write(reader.WaveFormat.Channels == 1 ? "Mono" : "Stereo", width - 106, 95, UIColors.label);
                        Write(reader.WaveFormat.SampleRate + " Hz", width - 106, 105, UIColors.label);
                        Write(reader.TotalTime.TotalSeconds + " sec", width - 106, 115, UIColors.label);
                    }
                }
            }
        }
    }

    public class LoopStream : WaveStream {
        private WaveStream sourceStream;
        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream) {
            this.sourceStream = sourceStream;
            EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat {
            get { return sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length {
            get { return sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int totalBytesRead = 0;

            while (totalBytesRead < count) {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0) {
                    if (sourceStream.Position == 0 || !EnableLooping) {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
