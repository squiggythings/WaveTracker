using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;


namespace WaveTracker.Rendering
{
    public class InstrumentEditor : UI.Panel
    {
        public static InstrumentEditor Instance { get; set; }
        public bool enabled;
        public Macro currentMacro;



        public InstrumentEditor()
        {
            Instance = this;
        }

        public void Initialize()
        {

        }

        public void Update()
        {

        }


        public void Draw()
        {
            DrawPanel();
        }


        public void Open(Macro m)
        {
            currentMacro = m;

        }

        public static void LoadSampleFromFile(Macro macro)
        {
            if (Input.dialogOpenCooldown == 0)
            {
                if (macro.macroType == MacroType.Sample)
                {
                    bool successfulReadWAV = false;
                    bool didReadWAV;
                    string fileName = "";
                    didReadWAV = false;
                    List<float> sampleDataLeft, sampleDataRight;
                    sampleDataLeft = new List<float>();
                    sampleDataRight = new List<float>();
                    Thread t = new Thread((ThreadStart)(() =>
                     {

                         Input.DialogStarted();
                         OpenFileDialog openFileDialog = new OpenFileDialog();
                         openFileDialog.Filter = "Audio Files (*.wav, *.mp3, *.flac)|*.wav;*.mp3;*.flac";
                         openFileDialog.Multiselect = false;
                         openFileDialog.Title = "Import Sample...";
                         sampleDataLeft = new List<float>();
                         sampleDataRight = new List<float>();
                         if (openFileDialog.ShowDialog() == DialogResult.OK)
                         {
                             didReadWAV = true;
                             fileName = openFileDialog.SafeFileName;
                             successfulReadWAV = (Helpers.readWav(openFileDialog.FileName, out macro.sample.sampleDataLeft, out macro.sample.sampleDataRight));
                         }

                     }));

                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();
                    if (didReadWAV)
                    {
                        if (successfulReadWAV)
                        {

                            macro.sample.Normalize();
                            macro.name = "" + fileName;
                        }
                        else
                        {
                            macro.sample.sampleDataLeft = new List<float>();
                            macro.sample.sampleDataRight = new List<float>();
                        }
                    }
                }
            }
        }
    }
}
