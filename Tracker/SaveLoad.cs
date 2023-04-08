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

namespace WaveTracker
{
    public static class SaveLoad
    {
        public static Song savedSong;

        public static bool isSaved { get { if (Game1.currentSong.Equals(Game1.newSong)) return true; if (savedSong == null) return false; else return savedSong.Equals(Game1.currentSong); } }
        public static string filePath = "";
        static char delimiter = (char)(10);
        const string fileHeaderCheck = "WaveTrackerModule_v1.0";
        public static bool isWorking;
        public static string fileName { get { if (filePath == "") return "Untitled.wtm"; return Path.GetFileName(filePath); } }
        public static int savecooldown = 0;
        public static bool thisSongExists()
        {
            return File.Exists(filePath);
        }

        static void SaveTo(string path)
        {
            //using (StreamWriter fileStream = new StreamWriter(path))
            //{
            Game1.currentSong.frameEdits = 0;
            Stopwatch sw = Stopwatch.StartNew();
            savedSong = Game1.currentSong.Clone();
            StringBuilder str = new StringBuilder();

            str.Append(fileHeaderCheck + delimiter);
            str.Append(savedSong.name + delimiter);
            str.Append(savedSong.author + delimiter);
            str.Append(savedSong.year + delimiter);
            str.Append(savedSong.comment + delimiter);
            str.Append("" + savedSong.ticksPerRow.Length + delimiter);
            foreach (int t in savedSong.ticksPerRow)
            {
                str.Append("" + t + delimiter);
            }
            str.Append("" + savedSong.rowsPerFrame + delimiter);
            str.Append("" + savedSong.tickRate + delimiter);
            str.Append("" + savedSong.quantizeChannelAmplitude + delimiter);

            foreach (Wave wave in savedSong.waves)
            {
                str.Append(wave.Pack() + delimiter);
            }

            str.Append("" + savedSong.instruments.Count + delimiter);
            foreach (Macro m in savedSong.instruments)
            {
                str.Append(m.Pack());
                str.Append(m.sample.stringBuild.ToString() + delimiter);
            }

            str.Append("" + savedSong.frames.Count + delimiter);
            foreach (Frame frame in savedSong.frames)
            {
                str.Append(frame.Pack() + delimiter);
            }

            str.Append("" + savedSong.rowHighlight1 + delimiter);
            str.Append("" + savedSong.rowHighlight2 + delimiter);
            byte[] bytes = new byte[str.Length * 2];

            int i = 0;
            foreach (char c in str.ToString())
            {
                bytes[i++] = (byte)(256 - BitConverter.GetBytes(c)[0]);
                bytes[i++] = (byte)(256 - BitConverter.GetBytes(c)[1]);
            }
            File.WriteAllBytes(path, bytes);

            // fileStream.Flush();
            sw.Stop();
            Debug.WriteLine("done " + sw.ElapsedMilliseconds);
            //}

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
            if (!isSaved)
            {
                PromptUnsaved();
            }
            filePath = "";
            FrameEditor.ClearHistory();
            FrameEditor.Goto(0, 0);
            FrameEditor.cursorColumn = 0;
            savedSong = new Song();
            Game1.currentSong = savedSong.Clone();
        }

        public static void SaveFileAs()
        {
            // set filepath to dialogresult
            if (SetFilePathThroughSaveAsDialog())
                SaveTo(filePath);
        }

        public static void OpenFile()
        {
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
                        FrameEditor.Goto(0, 0);
                        FrameEditor.cursorColumn = 0;
                    }
                    else
                    {
                        LoadError();
                        filePath = currentPath;

                    }

            }
            savecooldown = 4;
        }

        public static void DoUnsavedCheck()
        {
            if (PromptUnsaved2() == DialogResult.Cancel)
            {
                return;
            }
        }

        static bool LoadFrom(string path)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (FileStream fs = File.OpenRead(path))
            {
                if (ReadNextAsString(fs) != fileHeaderCheck)
                {
                    stopwatch.Stop();
                    //    Debug.WriteLine("file header match failed" + stopwatch.ElapsedMilliseconds);
                    return false;
                }
                savedSong = new Song();
                //  Debug.WriteLine("name");
                savedSong.name = ReadNextAsString(fs);
                //  Debug.WriteLine("author");
                savedSong.author = ReadNextAsString(fs);
                //  Debug.WriteLine("year");
                savedSong.year = ReadNextAsString(fs);
                // Debug.WriteLine("comment");
                savedSong.comment = ReadNextAsString(fs);
                // Debug.WriteLine("ticks per row");
                int count = ReadNextAsInt(fs);
                savedSong.ticksPerRow = new int[count];
                for (int i = 0; i < count; i++)
                {
                    savedSong.ticksPerRow[i] = ReadNextAsInt(fs);
                }
                //Debug.WriteLine("rows per frame");
                savedSong.rowsPerFrame = ReadNextAsInt(fs);
                //Debug.WriteLine("tick rate");
                savedSong.tickRate = ReadNextAsInt(fs);
                //Debug.WriteLine("quantize");
                savedSong.quantizeChannelAmplitude = ReadNextAsBool(fs);
                //Debug.WriteLine("waves");
                for (int i = 0; i < 100; ++i)
                {
                    savedSong.waves[i].Unpack(ReadNextAsString(fs));
                }
                //Debug.WriteLine("instruments count");
                count = ReadNextAsInt(fs);
                //Debug.WriteLine("instruments");
                savedSong.instruments.Clear();
                for (int i = 0; i < count; i++)
                {
                    // Debug.WriteLine(" > instrument " + i);
                    savedSong.instruments.Add(new Macro(MacroType.Wave));
                    savedSong.instruments[i].Unpack(ReadNextAsString(fs));
                }
                //Debug.WriteLine("frames count");
                count = ReadNextAsInt(fs);
                //Debug.WriteLine("frames");
                savedSong.frames.Clear();
                for (int i = 0; i < count; i++)
                {
                    savedSong.frames.Add(new Frame());
                    savedSong.frames[i].Unpack(ReadNextAsString(fs));
                }
                savedSong.rowHighlight1 = ReadNextAsInt(fs);
                savedSong.rowHighlight2 = ReadNextAsInt(fs);
                Game1.currentSong = savedSong.Clone();
            }
            stopwatch.Stop();
            Debug.WriteLine("opened in " + stopwatch.ElapsedMilliseconds);
            return true;
        }

        static string ReadNextAsString(FileStream fs)
        {
            return next(fs);
        }

        static int ReadNextAsInt(FileStream fs)
        {

            return int.Parse(next(fs));
        }

        static float ReadNextAsFloat(FileStream fs)
        {
            return float.Parse(next(fs), CultureInfo.InvariantCulture.NumberFormat);
        }

        static bool ReadNextAsBool(FileStream fs)
        {
            return next(fs) == "1";
        }

        static string next(FileStream fs)
        {
            StringBuilder sb = new StringBuilder();
            int readLen;
            byte[] b = new byte[2];
            while ((readLen = fs.Read(b, 0, 2)) > 0)
            {
                //Encrypt(b);
                int c = (256 - b[1]) * 256;
                c += (256 - b[0]);
                if ((char)c == delimiter)
                    break;
                sb.Append((char)c);
            }
            return sb.ToString();
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

                MessageBox.Show("Error loading " + fileName, "WaveTracker", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public static bool SetFilePathThroughSaveAsDialog()
        {
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
                        filePath = saveFileDialog.FileName;
                        didIt = true;
                    }

                }));

                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
            }
            return didIt;
        }
    }
}
