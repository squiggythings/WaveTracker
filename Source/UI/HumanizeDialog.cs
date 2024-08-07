namespace WaveTracker.UI {
    public class HumanizeDialog : Dialog {
        private PatternEditor parentEditor;
        private Button cancel, ok;
        private NumberBox volumeRange;
        public HumanizeDialog() : base("Humanize volumes", 146, 58) {
            PositionInCenterOfScreen();
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            volumeRange = new NumberBox("Randomization Range", 8, 19, 132, 36, this);
            volumeRange.SetValueLimits(0, 99);
            volumeRange.SetTooltip("How much to randomize volumes in this selection");
            volumeRange.Value = 5;
        }

        public void Open(PatternEditor parentEditor) {
            this.parentEditor = parentEditor;
            Open();
        }

        public override void Update() {
            if (WindowIsOpen) {
                DoDragging();
                if (cancel.Clicked || ExitButton.Clicked) {
                    Close();
                }

                if (ok.Clicked) {
                    parentEditor.RandomizeSelectedVolumes(volumeRange.Value);
                    Close();
                }
                volumeRange.Update();
            }
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                volumeRange.Draw();
            }
        }
    }
}
