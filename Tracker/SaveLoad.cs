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


namespace WaveTracker
{
    public static class SaveLoad
    {
        public static Song savedSong;


        public static bool isSaved { get { if (Game1.currentSong.Equals(Game1.newSong)) return true; if (savedSong == null) return false; else return savedSong.Equals(Game1.currentSong); } }
        public static string filePath = "";

        public static string fileName { get { if (filePath == "") return "Untitled.wtm"; return Path.GetFileName(filePath); } }
        public static string fileNameWithoutExtension { get { if (filePath == "") return "Untitled"; return Path.GetFileNameWithoutExtension(filePath); } }
        public static int savecooldown = 0;
        public static bool thisSongExists()
        {
            return File.Exists(filePath);
        }

        static void SaveTo(string path)
        {
            Stopwatch sw = Stopwatch.StartNew();
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                savedSong = Game1.currentSong.Clone();
            }
            catch
            {
                Debug.WriteLine("failed to save");
                return;
            }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(fs, savedSong);
            }
            sw.Stop();
            Debug.WriteLine("saved in " + sw.ElapsedMilliseconds + " ms");
            return;

        }

        public static void SaveFile()
        {
            if (savecooldown == 0)
                if (!File.Exists(filePath))
                {
                    SaveFileAs();
                }
                else
                {
                    SaveTo(filePath);
                }
            savecooldown = 4;
        }

        public static void NewFile()
        {
            if (Input.internalDialogIsOpen)
                return;
            
            if (!isSaved)
            {
                if (PromptUnsaved() == DialogResult.Cancel) return;
            }
            Playback.Stop();
            filePath = "";
            FrameEditor.ClearHistory();
            FrameEditor.Goto(0, 0);
            Playback.Goto(0, 0);
            FrameEditor.cursorColumn = 0;
            savedSong = new Song();
            Game1.currentSong = savedSong.Clone();
        }

        public static void SaveFileAs()
        {
            if (Input.internalDialogIsOpen)
                return;
            // set filepath to dialogresult
            Playback.Stop();
            if (SetFilePathThroughSaveAsDialog(out filePath))
                SaveTo(filePath);
        }

        public static void OpenFile()
        {
            if (Input.internalDialogIsOpen)
                return;
            Playback.Stop();
            if (savecooldown == 0)
            {
                // set filepath to dialog result
                string currentPath = filePath;
                if (!isSaved)
                {
                    if (PromptUnsaved() == DialogResult.Cancel)
                    {
                        return;
                    }
                    Input.dialogOpenCooldown = 0;
                }
                if (SetFilePathThroughOpenDialog())
                    if (LoadFrom(filePath))
                    {
                        Rendering.Visualization.GetWaveColors();
                        Audio.ChannelManager.instance.Reset();
                        FrameEditor.Goto(0, 0);
                        Playback.Goto(0, 0);
                        FrameEditor.cursorColumn = 0;
                        FrameEditor.ClearHistory();
                    }
                    else
                    {
                        LoadError();
                        filePath = currentPath;

                    }

            }
            savecooldown = 4;
        }

        public static bool LoadFrom(string path)
        {
            if (!File.Exists(path))
                return false;
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                BinaryFormatter formatter = new BinaryFormatter();

                MemoryStream ms = new MemoryStream();
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    savedSong = (Song)formatter.Deserialize(fs);
                }
                Game1.currentSong = savedSong.Clone();
                Audio.ChannelManager.instance.Reset();
                FrameEditor.Goto(0, 0);
                FrameEditor.cursorColumn = 0;
                stopwatch.Stop();
                Debug.WriteLine("opened in " + stopwatch.ElapsedMilliseconds + " ms");
            }
            catch
            {
                Debug.WriteLine("failed to open");
                return false;
            }
            filePath = path;
            return true;
        }



        public static bool SetFilePathThroughOpenDialog()
        {
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0)
            {
                Thread t = new Thread((ThreadStart)(() =>
                {

                    Input.DialogStarted();
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "WaveTracker modules (*wtm)|*.wtm";
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = "Open";
                    openFileDialog.ValidateNames = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
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

        public static DialogResult PromptUnsaved()
        {
            DialogResult ret = DialogResult.Cancel;
            if (Input.dialogOpenCooldown == 0)
            {
                Input.DialogStarted();
                ret = MessageBox.Show("Save changes to " + fileName + "?", "WaveTracker", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            }
            if (ret == DialogResult.Yes)
            {
                SaveFile();
            }
            return ret;
        }
        public static DialogResult PromptUnsaved2()
        {
            DialogResult ret = DialogResult.Cancel;
            if (Input.dialogOpenCooldown == 0)
            {
                Input.DialogStarted();
                ret = MessageBox.Show("Save changes to " + fileName + "?", "WaveTracker", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            }
            if (ret == DialogResult.Yes)
            {
                SaveFile();
            }
            return ret;
        }

        public static void LoadError()
        {
            if (Input.dialogOpenCooldown == 0)
            {
                Input.DialogStarted();

                MessageBox.Show("Could not open " + fileName, "WaveTracker", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public static bool SetFilePathThroughSaveAsDialog(out string filepath)
        {
            string ret = "";
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0)
            {
                Thread t = new Thread((ThreadStart)(() =>
                {

                    Input.DialogStarted();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = "wtm";
                    saveFileDialog.Filter = "WaveTracker modules (*.wtm)|*.wtm|All files (*.*)|*.*";
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.FileName = fileName;
                    saveFileDialog.Title = "Save As";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.ValidateNames = true;


                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
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

        public static bool ChooseExportPath(out string filepath)
        {
            string ret = "";
            bool didIt = false;
            if (Input.dialogOpenCooldown == 0)
            {
                Thread t = new Thread((ThreadStart)(() =>
                {

                    Input.DialogStarted();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = "wav";
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.FileName = fileNameWithoutExtension;
                    saveFileDialog.Title = "Export .wav";
                    saveFileDialog.Filter = "Waveform Audio File Format (*.wav)|*.wav|All files (*.*)|*.*";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.ValidateNames = true;


                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
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



    public class SongSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            // One way to discover expected types is through testing deserialization
            // of **valid** data and logging the types used.

            ////Console.WriteLine($"BindToType('{assemblyName}', '{typeName}')");

            if (typeName == "WaveTracker.Tracker.Song")
            {
                return typeof(Song);
            }
            if (typeName == "WaveTracker.Tracker.Frame")
                return typeof(Frame);
            if (typeName == "WaveTracker.Tracker.Wave")
                return typeof(Wave);
            if (typeName == "WaveTracker.Tracker.Macro")
                return typeof(Macro);
            if (typeName == "WaveTracker.Tracker.Envelope")
                return typeof(Envelope);
            if (typeName == "WaveTracker.Tracker.Sample")
                return typeof(Sample);
            else
            {
                throw new ArgumentException("Unexpected type " + typeName, nameof(typeName));
            }
        }
    }
}
