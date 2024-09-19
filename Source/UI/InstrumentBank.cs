using Microsoft.Xna.Framework;
using System;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class InstrumentBank : Panel {
        private int lastIndex;
        private int listLength = 32;
        public Scrollbar scrollbar;
        private InputField InputField { get; set; }

        public int CurrentInstrumentIndex { get; set; }
        public Instrument GetCurrentInstrument {
            get {
                return App.CurrentModule.Instruments[CurrentInstrumentIndex];
            }
        }

        public SpriteButton bNewWave, bNewSample, bRemove, bDuplicate, bMoveUp, bMoveDown, bRename;
        public SpriteButton bEdit;
        public Menu menu;
        // 790, 152
        public InstrumentBank(int x, int y) : base("Instrument Bank", x, y, 156, 488) {
            bNewWave = new SpriteButton(1, 10, 15, 15, 225, 0, this);
            bNewWave.SetTooltip("New Wave Instrument", "Add a new wave instrument to the track");
            bNewSample = new SpriteButton(16, 10, 15, 15, 240, 0, this);
            bNewSample.SetTooltip("New Sample Instrument", "Add a new sample instrument to the track");

            bRemove = new SpriteButton(31, 10, 15, 15, 360, 0, this);
            bRemove.SetTooltip("Remove Instrument", "Delete this instrument from the track");

            bDuplicate = new SpriteButton(46, 10, 15, 15, 255, 0, this);
            bDuplicate.SetTooltip("Duplicate Instrument", "Create a copy of this instrument and add it to the track");

            bMoveDown = new SpriteButton(70, 10, 15, 15, 345, 0, this);
            bMoveDown.SetTooltip("Move Down", "Move this instrument to be lower down the list");

            bMoveUp = new SpriteButton(85, 10, 15, 15, 330, 0, this);
            bMoveUp.SetTooltip("Move Up", "Move this instrument to be higher up the list");

            bEdit = new SpriteButton(109, 10, 15, 15, 270, 0, this);
            bEdit.SetTooltip("Edit Instrument", "Open the instrument editor");

            bRename = new SpriteButton(124, 10, 15, 15, 375, 0, this);
            bRename.SetTooltip("Rename Instrument", "Rename this instrument");

            scrollbar = new Scrollbar(1, 28, width - 1, 367, this);
            InputField = new InputField(25, 0, width - 31, this);
            InputField.MaximumLength = 32;
        }

        public Menu CreateInstrumentMenu() {
            return new Menu([
                        new MenuOption("Rename...", Rename, !App.VisualizerMode),
                        new MenuOption("Edit...", Edit, !App.VisualizerMode),
                        null,
                        new MenuOption("Move up", MoveUp, CurrentInstrumentIndex > 0 && !App.VisualizerMode),
                        new MenuOption("Move down", MoveDown, CurrentInstrumentIndex < App.CurrentModule.Instruments.Count - 1 && !App.VisualizerMode),
                        null,
                        new MenuOption("Add wave instrument",AddWave, App.CurrentModule.Instruments.Count < 100 && !App.VisualizerMode),
                        new MenuOption("Add noise instrument",AddNoise,App.CurrentModule.Instruments.Count < 100 && !App.VisualizerMode),
                        new MenuOption("Add sample instrument",AddSample,App.CurrentModule.Instruments.Count < 100 && !App.VisualizerMode),
                        new MenuOption("Duplicate",DuplicateInstrument,App.CurrentModule.Instruments.Count < 100 && !App.VisualizerMode),
                        new MenuOption("Remove",RemoveInstrument,App.CurrentModule.Instruments.Count > 1 && !App.VisualizerMode)
                   ]);
        }

        public void Update() {
            x = App.WindowWidth - width;
            height = App.WindowHeight - y;
            InputField.y = 27 + (-scrollbar.ScrollValue + CurrentInstrumentIndex) * 11;

            listLength = (App.WindowHeight - y - 28 - 8) / 11;
            scrollbar.height = listLength * 11;
            scrollbar.SetSize(App.CurrentModule.Instruments.Count, listLength);
            if (listLength <= 0) {
                listLength = 1;
            }

            if (!Menu.IsAMenuOpen && !Dropdown.IsAnyDropdownOpen && !InputField.IsAnInputFieldBeingEdited) {
                if (App.Shortcuts["General\\Next instrument"].IsPressedRepeat) {
                    CurrentInstrumentIndex++;
                    CurrentInstrumentIndex = Math.Clamp(CurrentInstrumentIndex, 0, App.CurrentModule.Instruments.Count - 1);
                    MoveBounds();
                    if (App.InstrumentEditor.IsOpen) {
                        Edit();
                    }
                }
                if (App.Shortcuts["General\\Previous instrument"].IsPressedRepeat) {
                    CurrentInstrumentIndex--;
                    CurrentInstrumentIndex = Math.Clamp(CurrentInstrumentIndex, 0, App.CurrentModule.Instruments.Count - 1);
                    MoveBounds();
                    if (App.InstrumentEditor.IsOpen) {
                        Edit();
                    }
                }
            }

            if (Input.focus == null || InputField.InFocus) {
                scrollbar.Update();
                scrollbar.height = listLength * 11;
                scrollbar.SetSize(App.CurrentModule.Instruments.Count, listLength);
                if (Input.internalDialogIsOpen) {
                    return;
                }
                if (InputField.IsBeingEdited) {
                    InputField.Update();
                    if (InputField.ValueWasChangedInternally) {
                        if (InputField.EditedText.Length > 0) {
                            App.CurrentModule.Instruments[CurrentInstrumentIndex].SetName(InputField.EditedText);
                            App.CurrentModule.SetDirty();
                        }
                    }
                    return;
                }
                if (MouseX > 1 && MouseX < 162) {
                    if (MouseY > 28) {
                        if (Input.GetRightClickUp(KeyModifier._Any)) {
                            CurrentInstrumentIndex = Math.Clamp((MouseY - 28) / 11 + scrollbar.ScrollValue, 0, App.CurrentModule.Instruments.Count - 1);

                            ContextMenu.Open(CreateInstrumentMenu());
                        }
                        // click on item
                        if (Input.GetClickDown(KeyModifier._Any) || Input.GetRightClickDown(KeyModifier._Any)) {
                            CurrentInstrumentIndex = Math.Clamp((MouseY - 28) / 11 + scrollbar.ScrollValue, 0, App.CurrentModule.Instruments.Count - 1);
                        }
                        if (Input.GetDoubleClickDown(KeyModifier._Any)) {
                            int ix = (MouseY - 28) / 11 + scrollbar.ScrollValue;
                            if (ix < App.CurrentModule.Instruments.Count && ix >= 0) {
                                App.InstrumentEditor.Open(GetCurrentInstrument, CurrentInstrumentIndex);
                            }
                        }
                    }
                }

                bRemove.enabled = App.CurrentModule.Instruments.Count > 1;
                bNewWave.enabled = bNewSample.enabled = bDuplicate.enabled = App.CurrentModule.Instruments.Count < 100;
                bMoveDown.enabled = CurrentInstrumentIndex < App.CurrentModule.Instruments.Count - 1;
                bMoveUp.enabled = CurrentInstrumentIndex > 0;
                if (bNewWave.Clicked) {
                    AddWave();
                }
                if (bNewSample.Clicked) {
                    AddSample();
                }
                if (bRemove.Clicked) {
                    RemoveInstrument();
                }
                if (bDuplicate.Clicked) {
                    DuplicateInstrument();
                }
                if (bMoveDown.Clicked || App.Shortcuts["General\\Move instrument down"].IsPressedDown) {
                    MoveDown();
                }
                if (bMoveUp.Clicked || App.Shortcuts["General\\Move instrument up"].IsPressedDown) {
                    MoveUp();
                }

                if (bEdit.Clicked || App.Shortcuts["General\\Edit instrument"].IsPressedDown) {
                    Edit();
                }

                if (bRename.Clicked || App.Shortcuts["General\\Rename instrument"].IsPressedDown) {
                    Rename();
                }

                CurrentInstrumentIndex = Math.Clamp(CurrentInstrumentIndex, 0, App.CurrentModule.Instruments.Count - 1);
                if (lastIndex != CurrentInstrumentIndex) {
                    lastIndex = CurrentInstrumentIndex;
                }
                scrollbar.UpdateScrollValue();
            }
        }

        public void AddWave() {
            App.CurrentModule.Instruments.Add(new WaveInstrument());
            App.CurrentModule.SetDirty();
            CurrentInstrumentIndex = App.CurrentModule.Instruments.Count - 1;
            Goto(App.CurrentModule.Instruments.Count - 1);
        }

        public void AddNoise() {
            App.CurrentModule.Instruments.Add(new NoiseInstrument());
            App.CurrentModule.SetDirty();
            CurrentInstrumentIndex = App.CurrentModule.Instruments.Count - 1;
            Goto(App.CurrentModule.Instruments.Count - 1);
        }

        public void AddSample() {
            App.CurrentModule.Instruments.Add(new SampleInstrument());
            App.CurrentModule.SetDirty();
            CurrentInstrumentIndex = App.CurrentModule.Instruments.Count - 1;
            Goto(App.CurrentModule.Instruments.Count - 1);
        }

        public void DuplicateInstrument() {
            App.CurrentModule.Instruments.Add(GetCurrentInstrument.Clone());
            App.CurrentModule.SetDirty();
            Goto(App.CurrentModule.Instruments.Count - 1);
        }

        public void MoveUp() {
            if (CurrentInstrumentIndex > 0) {
                App.CurrentModule.SwapInstrumentsInSongs(CurrentInstrumentIndex, CurrentInstrumentIndex - 1);
                App.CurrentModule.Instruments.Reverse(CurrentInstrumentIndex - 1, 2);
                App.CurrentModule.SetDirty();
                CurrentInstrumentIndex--;
                MoveBounds();
            }
        }
        public void MoveDown() {
            if (CurrentInstrumentIndex < App.CurrentModule.Instruments.Count - 1) {
                App.CurrentModule.SwapInstrumentsInSongs(CurrentInstrumentIndex, CurrentInstrumentIndex + 1);
                App.CurrentModule.Instruments.Reverse(CurrentInstrumentIndex, 2);
                App.CurrentModule.SetDirty();
                CurrentInstrumentIndex++;
                MoveBounds();
            }
        }

        public void RemoveInstrument() {
            App.CurrentModule.AdjustForDeletedInstrument(CurrentInstrumentIndex);
            App.CurrentModule.Instruments.RemoveAt(CurrentInstrumentIndex);
            App.CurrentModule.SetDirty();
            if (CurrentInstrumentIndex >= App.CurrentModule.Instruments.Count) {
                Goto(App.CurrentModule.Instruments.Count - 1);
            }
        }

        public void Rename() {
            InputField.Open(GetCurrentInstrument.name, true);
        }

        public void Edit() {
            App.InstrumentEditor.Open(GetCurrentInstrument, CurrentInstrumentIndex);
        }

        private void Goto(int index) {
            CurrentInstrumentIndex = index;
            MoveBounds();
        }

        private void MoveBounds() {
            if (CurrentInstrumentIndex > scrollbar.ScrollValue + listLength - 1) {
                scrollbar.ScrollValue = CurrentInstrumentIndex - listLength + 1;
            }
            if (CurrentInstrumentIndex < scrollbar.ScrollValue) {
                scrollbar.ScrollValue = CurrentInstrumentIndex;
            }
            scrollbar.SetSize(App.CurrentModule.Instruments.Count, listLength);
            scrollbar.ScrollValue = Math.Clamp(scrollbar.ScrollValue, 0, Math.Clamp(App.CurrentModule.Instruments.Count - listLength, 0, 100));
            scrollbar.UpdateScrollValue();
        }
        public void DrawList() {
            Color odd = new Color(43, 49, 81);
            Color even = new Color(59, 68, 107);
            Color selected = UIColors.selection;
            int y = 0;
            for (int i = scrollbar.ScrollValue; i < listLength + scrollbar.ScrollValue; i++) {
                Color row = i == CurrentInstrumentIndex ? selected : i % 2 == 0 ? even : odd;
                DrawRect(1, 28 + y * 11, width - 7, 11, row);
                if (App.CurrentModule.Instruments.Count > i && i >= 0) {
                    WriteMonospaced(i.ToString("D2"), 15, 30 + y * 11, Color.White, 4);
                    Write(Helpers.TrimTextToWidth(InputField.width, App.CurrentModule.Instruments[i].name), 29, 30 + y * 11, Color.White);

                    if (App.CurrentModule.Instruments[i] is WaveInstrument) {
                        DrawSprite(3, 30 + y * 11, new Rectangle(88, 80, 8, 7));
                    }
                    else {
                        DrawSprite(3, 30 + y * 11, new Rectangle(88, 87, 8, 7));
                    }
                }
                ++y;
            }
            if (InputField.IsBeingEdited) {
                InputField.Draw(GetCurrentInstrument.name);
            }
        }

        public new void Draw() {
            base.Draw();
            DrawRect(0, 9, width, 17, Color.White);
            bNewWave.Draw();
            bNewSample.Draw();
            bRemove.Draw();
            bDuplicate.Draw();
            bMoveUp.Draw();
            bMoveDown.Draw();
            bEdit.Draw();
            bRename.Draw();
            DrawList();
            scrollbar.Draw();
        }
    }
}
