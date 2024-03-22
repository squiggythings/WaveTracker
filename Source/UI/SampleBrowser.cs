using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;
using WaveTracker.UI;
using System.IO;
using System.Diagnostics;
using NAudio.Wave;
using System.Threading;

namespace WaveTracker.UI {
    public class SampleBrowser : Element {
        public bool enabled;
        int selectedFileIndex = -1;
        public string selectedFilePath;
        Toggle sortName, sortType;
        SpriteButton backButton;
        Scrollbar scrollbar;
        Button ok, cancel;
        string currentPath = @"C:\";
        string lastPath;
        int listLength = 24;
        public string[] entriesInDirectory;
        InstrumentEditor launched;
        int width = 500;
        int height = 320;
        WaveOutEvent previewOut;
        AudioFileReader reader;
        enum SortingMethod { ByName, ByType };
        SortingMethod sortMethod;
        bool selectedAnAudioFile => reader != null && (selectedFileIndex < 0 || selectedFileIndex >= entriesInDirectory.Length ? false : File.Exists(entriesInDirectory[selectedFileIndex]));
        public SampleBrowser() {
            this.x = (960 - width) / 2;
            this.y = (500 - height) / 2;
            backButton = new SpriteButton(2, 11, 15, 15, Rendering.Graphics.img, 300, 0, this);
            scrollbar = new Scrollbar(2, 29, width - 111, listLength * 11, this);
            scrollbar.CoarseStepAmount = 3;
            ok = new Button("OK", width - 108, height - 16, this);
            ok.width = 51;
            cancel = new Button("Cancel", width - 54, height - 16, this);
            cancel.width = 51;
            sortName = new Toggle("Name", width - 65, 30, this);
            sortType = new Toggle("Type", width - 36, 30, this);
            entriesInDirectory = new string[0];
            previewOut = new WaveOutEvent();
            if (Directory.Exists(Preferences.profile.lastBrowseDirectory)) {
                currentPath = Preferences.profile.lastBrowseDirectory;
            }
            else {
                currentPath = Directory.GetCurrentDirectory();
            }
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
                    if (Directory.GetParent(currentPath) == null)
                        currentPath = "";
                    else
                        currentPath = Directory.GetParent(currentPath).ToString();
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
                    if (MouseX > 2 && MouseX < width - 117) {
                        if (MouseY > y && MouseY <= y + 11) {
                            if (Input.GetClickDown(KeyModifier.None)) {
                                if (selectedFileIndex != i)
                                    newFile = true;
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
                    y += 11;
                }
                if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None)) {
                    selectedFileIndex--;
                    if (selectedFileIndex < 0)
                        selectedFileIndex = 0;
                    moveBounds();
                    SelectedANewEntry();
                }
                if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None)) {
                    selectedFileIndex++;
                    if (selectedFileIndex > entriesInDirectory.Length - 1)
                        selectedFileIndex = entriesInDirectory.Length - 1;
                    SelectedANewEntry();
                    moveBounds();
                }
                scrollbar.SetSize(entriesInDirectory.Length, listLength);
                scrollbar.ScrollValue = Math.Clamp(scrollbar.ScrollValue, 0, Math.Clamp(entriesInDirectory.Length - listLength, 0, 999999));
                scrollbar.UpdateScrollValue();
                scrollbar.Update();
                ok.enabled = selectedAnAudioFile;
            }
        }

        public void moveBounds() {
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

        void SelectedANewEntry() {
            previewOut.Stop();
            if (File.Exists(entriesInDirectory[selectedFileIndex])) {
                //Thread.Sleep(1);
                reader = new AudioFileReader(entriesInDirectory[selectedFileIndex]);
                if ((reader.TotalTime.TotalSeconds * reader.WaveFormat.SampleRate) / reader.WaveFormat.Channels <= 400) {
                    LoopStream loop = new LoopStream(reader);
                    previewOut.Init(loop);
                }
                else {
                    previewOut.Init(reader);
                }
                if (Preferences.profile.previewSamples)
                    previewOut.Play();
            }
        }

        void GetFileEntries(bool overrideOptimization) {
            if (currentPath != lastPath || overrideOptimization) {
                // the topmost in the tree, choosing a drive
                lastPath = currentPath;
                List<string> entries = new List<string>();
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
                            if (ext == ".wav" || ext == ".mp3" || ext == ".flac" || ext == ".aiff") {
                                continue;
                            }
                        }
                        entries.RemoveAt(i);
                    }
                }
                if (sortMethod == SortingMethod.ByName)
                    entries.Sort();
                if (sortMethod == SortingMethod.ByType)
                    entries.Sort((a, b) => SortByType(a, b));
                entriesInDirectory = entries.ToArray();
            }
        }


        int SortByType(string a, string b) {
            int val = Path.GetExtension(a).CompareTo(Path.GetExtension(b));
            if (val == 0) {
                return a.CompareTo(b);
            }
            else {
                return val;
            }
        }
        public void Open(InstrumentEditor launch) {
            previewOut.Stop();
            launched = launch;
            selectedFileIndex = -1;
            scrollbar.ScrollValue = 0;
            enabled = true;
            Input.focus = this;
        }

        public void Close() {
            enabled = false;
            Input.focus = launched;
            if (File.Exists(selectedFilePath))
                launched.LoadSampleFromFile(selectedFilePath);
            launched.startcooldown = 14;
            Preferences.profile.lastBrowseDirectory = currentPath;
            Preferences.SaveToFile();
            previewOut.Stop();
        }

        public void DrawList() {
            Color odd = new Color(43, 49, 81);
            Color even = new Color(59, 68, 107);
            Color selected = UIColors.selection;
            int y = 0;
            for (int i = scrollbar.ScrollValue; i < listLength + scrollbar.ScrollValue; i++) {
                Color row;
                if (i == selectedFileIndex)
                    row = selected;
                else if (i % 2 == 0)
                    row = even;
                else
                    row = odd;
                DrawRect(2, 29 + y * 11, width - 111, 11, row);
                if (entriesInDirectory.Length > i && i >= 0) {
                    if (currentPath == "")
                        Write(entriesInDirectory[i], 20, 31 + y * 11, Color.White);
                    else
                        Write(Helpers.FlushString(Path.GetFileName(entriesInDirectory[i])), 20, 31 + y * 11, Color.White);

                    if (Directory.Exists(entriesInDirectory[i])) // draw folder icon
                        DrawSprite(5, 29 + y * 11, new Rectangle(72, 80, 12, 11));
                    else if (File.Exists(entriesInDirectory[i])) // draw audio file icon
                        DrawSprite(5, 29 + y * 11, new Rectangle(72, 91, 12, 11));
                }
                ++y;
            }
            scrollbar.Draw();
        }

        string GetNicePathString(string path) {

            string ret = "";
            for (int i = path.Length - 1; i >= 0; i--) {
                char c = path[i];
                if (c == '\\' || c == '/') {
                    if (Helpers.GetWidthOfText(ret) > width - 200)
                        return "... > " + ret;
                    ret = " > " + ret;
                }
                else
                    ret = c + ret;
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
                if (selectedAnAudioFile) {
                    // write file name
                    if (reader != null) {
                        Write(Helpers.FlushString(Helpers.TrimTextToWidth(105, Path.GetFileName(reader.FileName))), width - 106, 85, UIColors.label);
                        Write(reader.WaveFormat.Channels == 1 ? "Mono" : "Stereo", width - 106, 95, UIColors.label);
                        Write(reader.WaveFormat.SampleRate + " Hz", width - 106, 105, UIColors.label);
                        Write(reader.TotalTime.TotalSeconds + " sec", width - 106, 115, UIColors.label);
                        //Write(Math.Round(reader.TotalTime.TotalMilliseconds * (reader.WaveFormat.SampleRate / 1000.0)) + " samples", width - 106, 125, UIColors.label);
                    }
                }
            }
        }
    }

    public class LoopStream : WaveStream {
        WaveStream sourceStream;
        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream) {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
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
