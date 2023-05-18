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

namespace WaveTracker.Rendering
{
    public class SampleBrowser : Element
    {
        public bool enabled;
        public Texture2D icons;
        int selectedFileIndex;
        public string selectedFilePath;
        public Toggle sortName, sortType;
        public SpriteButton backButton;
        public Scrollbar scrollbar;
        public Button ok, cancel;
        string currentPath = @"C:\USERS\Elias\Desktop\stuff that takes up space\pxtone\my_material";
        int listLength = 24;
        public string[] entriesInDirectory;
        InstrumentEditor launched;
        int width = 500;
        int height = 320;
        WaveOutEvent previewOut;
        AudioFileReader reader;
        bool selectedAnAudioFile => reader != null && (selectedFileIndex < 0 || selectedFileIndex >= entriesInDirectory.Length ? false : File.Exists(entriesInDirectory[selectedFileIndex]));
        public SampleBrowser(Texture2D tex)
        {
            this.x = 12;
            this.y = 12;
            icons = tex;
            backButton = new SpriteButton(2, 11, 15, 15, Toolbar.sprite, 20, this);
            scrollbar = new Scrollbar(2, 29, width - 111, listLength * 11, this);
            scrollbar.coarseStepAmount = 3;
            ok = new Button("OK", width - 105, height - 16, this);
            ok.width = 51;
            cancel = new Button("Cancel", width - 53, height - 16, this);
            cancel.width = 51;
            entriesInDirectory = new string[0];
            previewOut = new WaveOutEvent();
            if (Directory.Exists(Preferences.lastBrowseDirectory))
            {
                currentPath = Preferences.lastBrowseDirectory;
            }
            else
            {
                currentPath = Directory.GetCurrentDirectory();
            }
        }

        public void Update()
        {
            if (enabled)
            {

                if (backButton.Clicked)
                {
                    currentPath = Directory.GetParent(currentPath).ToString();
                }
                if (ok.Clicked)
                {
                    selectedFilePath = entriesInDirectory[selectedFileIndex];
                    Close();
                    return;
                }
                if (cancel.Clicked)
                {
                    selectedFilePath = "";
                    Close();
                    return;
                }
                GetFileEntries();

                int y = 29;
                bool newFile = false;
                for (int i = scrollbar.scrollValue; i < listLength + scrollbar.scrollValue; i++)
                {
                    if (MouseX > 2 && MouseX < 183)
                    {
                        if (MouseY > y && MouseY <= y + 11)
                        {
                            if (Input.GetClickDown(KeyModifier.None))
                            {
                                if (selectedFileIndex != i)
                                    newFile = true;
                                selectedFileIndex = i;
                                if (newFile)
                                {
                                    SelectedANewEntry();
                                    break;
                                }
                            }
                            if (Input.GetDoubleClick(KeyModifier.None))
                            {
                                if (Directory.Exists(entriesInDirectory[i]))
                                {
                                    // folder
                                    currentPath = entriesInDirectory[i];
                                    selectedFileIndex = -1;
                                    GetFileEntries();
                                }
                                else
                                {
                                    selectedFilePath = entriesInDirectory[selectedFileIndex];
                                    Close();
                                    return;
                                }
                            }
                        }
                    }
                    y += 11;
                }
                if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None))
                {
                    selectedFileIndex--;
                    if (selectedFileIndex < 0)
                        selectedFileIndex = 0;
                    SelectedANewEntry();
                }
                if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None))
                {
                    selectedFileIndex++;
                    if (selectedFileIndex > entriesInDirectory.Length - 1)
                        selectedFileIndex = entriesInDirectory.Length - 1;
                    SelectedANewEntry();
                }
                scrollbar.SetSize(entriesInDirectory.Length, listLength);
                scrollbar.scrollValue = Math.Clamp(scrollbar.scrollValue, 0, Math.Clamp(entriesInDirectory.Length - listLength, 0, 999999));
                scrollbar.doUpdate();
                scrollbar.Update();
                ok.enabled = selectedAnAudioFile;
            }
        }

        void SelectedANewEntry()
        {
            previewOut.Stop();
            if (File.Exists(entriesInDirectory[selectedFileIndex]))
            {
                //Thread.Sleep(1);
                reader = new AudioFileReader(entriesInDirectory[selectedFileIndex]);
                previewOut.Init(reader);
                previewOut.Play();
            }
        }

        void GetFileEntries()
        {
            List<string> entries = Directory.GetFileSystemEntries(currentPath, "*", SearchOption.TopDirectoryOnly).ToList();
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if ((File.GetAttributes(entries[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    entries.RemoveAt(i);
                    continue;
                }
                if (Directory.Exists(entries[i]))
                {
                    continue;
                }
                else
                {
                    if (File.Exists(entries[i]))
                    {
                        string ext = Path.GetExtension(entries[i]);
                        if (ext == ".wav" || ext == ".mp3" || ext == ".flac")
                        {
                            continue;
                        }
                    }
                    entries.RemoveAt(i);
                }
            }
            entriesInDirectory = entries.ToArray();
        }

        public void Open(InstrumentEditor launch)
        {
            previewOut.Stop();
            launched = launch;
            enabled = true;
            Input.focus = this;
        }

        public void Close()
        {
            enabled = false;
            Input.focus = launched;
            if (File.Exists(selectedFilePath))
                launched.LoadSampleFromFile(selectedFilePath);
            previewOut.Stop();
        }

        public void DrawList()
        {
            Color odd = new Color(43, 49, 81);
            Color even = new Color(59, 68, 107);
            Color selected = new Color(8, 121, 232);
            int y = 0;
            for (int i = scrollbar.scrollValue; i < listLength + scrollbar.scrollValue; i++)
            {
                Color row;
                if (i == selectedFileIndex)
                    row = selected;
                else if (i % 2 == 0)
                    row = even;
                else
                    row = odd;
                DrawRect(2, 29 + y * 11, width - 111, 11, row);
                if (entriesInDirectory.Length > i && i >= 0)
                {
                    Write(Helpers.FlushString(Path.GetFileName(entriesInDirectory[i])), 20, 31 + y * 11, Color.White);
                    if (Directory.Exists(entriesInDirectory[i]))
                        DrawSprite(NumberBox.buttons, 5, 29 + y * 11, new Rectangle(10, 34, 12, 11));
                    else if (File.Exists(entriesInDirectory[i]))
                        DrawSprite(NumberBox.buttons, 5, 29 + y * 11, new Rectangle(22, 34, 12, 11));
                }
                ++y;
            }
            scrollbar.Draw();
        }

        string getNicePathString(string path)
        {

            string ret = "";
            for (int i = path.Length - 1; i >= 0; i--)
            {
                char c = path[i];
                if (c == '\\')
                {
                    if (Helpers.getWidthOfText(ret) > width - 200)
                        return "... > " + ret;
                    ret = " > " + ret;
                }
                else
                    ret = c + ret;
            }
            return ret;
        }

        public void Draw()
        {
            if (enabled)
            {
                // black box across screen behind window
                DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));

                Color bg = new Color(223, 224, 232);
                DrawRoundedRect(0, 0, width, height, bg);
                DrawRect(1, 0, width - 2, 1, Color.White);
                DrawRect(0, 1, width, 26, Color.White);
                DrawList();
                backButton.Draw();
                ok.Draw();
                cancel.Draw();
                Write(Helpers.FlushString(getNicePathString(currentPath)), 20, 15, ButtonColors.Round.backgroundColor);
                if (selectedAnAudioFile)
                {
                    // write file name
                    if (reader != null)
                    {
                        Write(Helpers.FlushString(Helpers.TrimTextToWidth(105, Path.GetFileName(reader.FileName))), width - 106, 85, ButtonColors.Round.backgroundColor);
                        Write(reader.WaveFormat.Channels == 1 ? "Mono" : "Stereo", width - 106, 95, ButtonColors.Round.backgroundColor);
                        Write(reader.WaveFormat.SampleRate + " Hz", width - 106, 105, ButtonColors.Round.backgroundColor);
                        Write(reader.TotalTime.TotalSeconds + " sec", width - 106, 115, ButtonColors.Round.backgroundColor);
                    }
                }
            }
        }
    }
}
