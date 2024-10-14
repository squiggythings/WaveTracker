using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public abstract class SampleModifyDialog : Dialog {
        private Button ok, cancel, preview;
        protected Sample sampleToEdit;
        protected SampleEditor openedEditor;
        protected int startIndex, endIndex;
        protected short[] originalDataL;
        protected short[] originalDataR;
        public double _time;
        public SampleModifyDialog currentlyOpen;
        private bool stopPreview;
        public SampleModifyDialog(string name) : base(name, 192, 78, false) {
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            preview = new Button("Preview", 3, height - 16, this);
            preview.width = 51;
            stopPreview = false;
        }

        public void Open(SampleEditor opened, int start, int end) {
            this.openedEditor = opened;
            sampleToEdit = opened.Sample;
            originalDataL = new short[end - start];
            startIndex = start;
            endIndex = end;
            stopPreview = true;
            Array.Copy(sampleToEdit.sampleDataL, start, originalDataL, 0, end - start);
            if (sampleToEdit.IsStereo) {
                originalDataR = new short[end - start];
                Array.Copy(sampleToEdit.sampleDataR, start, originalDataR, 0, end - start);
            }
            _time = end - start;
            Open();
        }

        public override void Update() {
            if (WindowIsOpen) {
                if (ok.Clicked) {
                    Apply();
                    Close();
                }
                if (cancel.Clicked) {
                    Close();
                }
                if (_time > originalDataL.Length - 1) {
                    preview.SetLabel("Preview");
                    if (preview.Clicked) {
                        stopPreview = false;
                        _time = 0;
                    }
                }
                else {
                    preview.SetLabel("Stop");
                    if (preview.Clicked) {
                        stopPreview = true;
                    }
                }
            }
        }

        public void GetPreviewSample(out float left, out float right) {
            if (stopPreview) {
                _time = originalDataL.Length;
            }
            if (_time < originalDataL.Length) {
                _time += (double)sampleToEdit.sampleRate / Audio.AudioEngine.SampleRate;
            }
            if (_time > originalDataL.Length - 1) {
                left = 0;
                right = 0;
            }
            else {
                left = GetSampleValue((int)_time, 0) / (float)short.MaxValue;
                if (sampleToEdit.IsStereo) {
                    right = GetSampleValue((int)_time, 1) / (float)short.MaxValue;
                }
                else {
                    right = left;
                }
            }
        }

        /// <summary>
        /// Applies the effect to the wave
        /// </summary>
        public void Apply() {
            if (sampleToEdit != null) {
                for (int i = startIndex; i < endIndex; i++) {
                    sampleToEdit.sampleDataL[i] = GetSampleValue(i - startIndex, 0);
                    if (sampleToEdit.IsStereo) {
                        sampleToEdit.sampleDataR[i] = GetSampleValue(i - startIndex, 1);
                    }
                }
                openedEditor.AddToUndoHistory();
            }
        }

        /// <summary>
        /// Given the input index, define the output sample
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract short GetSampleValue(int index, int channel);

        public new virtual void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                preview.Draw();
            }
        }
    }
}
