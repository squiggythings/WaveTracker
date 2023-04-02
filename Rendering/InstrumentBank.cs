using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;
using System.Windows.Forms;


namespace WaveTracker.Rendering
{
    public class InstrumentBank : UI.Panel
    {
        private Forms.EnterText renameDialog;
        bool dialogOpen, fileOpen;
        public static Song song => Game1.currentSong;
        int scrollValue;
        int lastIndex;
        int listLength = 32;

        public static int CurrentInstrumentIndex { get; set; }
        public static Macro GetCurrentInstrument => song.instruments[CurrentInstrumentIndex];

        public SpriteButton bNewWave, bNewSample, bRemove, bDuplicate, bMoveUp, bMoveDown, bRename;
        public SpriteButton bEdit;

        public InstrumentBank()
        {

        }
        public void Initialize(Texture2D sprite)
        {
            bNewWave = new SpriteButton(1, 10, 15, 15, sprite, 15, this);
            bNewWave.SetTooltip("New Wave Instrument", "Add a new wave instrument to the track");
            bNewSample = new SpriteButton(16, 10, 15, 15, sprite, 16, this);
            bNewSample.SetTooltip("New Sample Instrument", "Add a new sample instrument to the track");

            bRemove = new SpriteButton(31, 10, 15, 15, sprite, 24, this);
            bRemove.SetTooltip("Remove Instrument", "Delete this instrument from the track");

            bDuplicate = new SpriteButton(46, 10, 15, 15, sprite, 17, this);
            bDuplicate.SetTooltip("Duplicate Instrument", "Create a copy of this instrument and add it to the track");


            bMoveDown = new SpriteButton(70, 10, 15, 15, sprite, 23, this);
            bMoveDown.SetTooltip("Move Down", "Move this instrument to be lower down the list");

            bMoveUp = new SpriteButton(85, 10, 15, 15, sprite, 22, this);
            bMoveUp.SetTooltip("Move Up", "Move this instrument to be higher up the list");


            bEdit = new SpriteButton(109, 10, 15, 15, sprite, 18, this);
            bEdit.SetTooltip("Edit Instrument", "Open the instrument editor");

            bRename = new SpriteButton(124, 10, 15, 15, sprite, 25, this);
            bRename.SetTooltip("Rename Instrument", "Rename this instrument");

            InitializePanel("Instrument Bank", 790, 152, 170, 488);
        }

        public void Update()
        {
            listLength = (Game1.bottomOfScreen - 180 - 10) / 11;
            if (listLength <= 0)
                listLength = 1;
            if (Input.internalDialogIsOpen)
                return;
            if (MouseX > 0 && MouseY > 26)
            {
                scrollValue -= Input.MouseScrollWheel(KeyModifier.None);
            }
            if (MouseX > 1 && MouseX < 162)
            {
                if (MouseY > 28)
                {
                    // click on item
                    if (Input.GetClickDown(KeyModifier.None))
                    {
                        CurrentInstrumentIndex = Math.Clamp((MouseY - 28) / 11 + scrollValue, 0, song.instruments.Count - 1);
                    }
                    if (Input.GetDoubleClick(KeyModifier.None))
                    {
                        if (!fileOpen)
                        {
                            //InstrumentEditor.Instance.Open(GetCurrentInstrument);
                            fileOpen = true;
                            if (GetCurrentInstrument.macroType == MacroType.Sample) { }
                            InstrumentEditor.LoadSampleFromFile(song.instruments[CurrentInstrumentIndex]);
                        }
                    }
                    else
                    {
                        fileOpen = false;
                    }
                }
            }
            if (Input.GetKeyRepeat(Microsoft.Xna.Framework.Input.Keys.Down, KeyModifier.Ctrl))
                CurrentInstrumentIndex++;
            if (Input.GetKeyRepeat(Microsoft.Xna.Framework.Input.Keys.Up, KeyModifier.Ctrl))
                CurrentInstrumentIndex--;
            CurrentInstrumentIndex = Math.Clamp(CurrentInstrumentIndex, 0, song.instruments.Count - 1);

            bRemove.enabled = song.instruments.Count > 1;
            bNewWave.enabled = bNewSample.enabled = bDuplicate.enabled = song.instruments.Count < 100;
            bMoveDown.enabled = CurrentInstrumentIndex < song.instruments.Count - 1;
            bMoveUp.enabled = CurrentInstrumentIndex > 0;
            if (bNewWave.Clicked)
            {
                song.instruments.Add(new Macro(MacroType.Wave));
                CurrentInstrumentIndex = song.instruments.Count - 1;
                Goto(song.instruments.Count - 1);
            }
            if (bNewSample.Clicked)
            {
                song.instruments.Add(new Macro(MacroType.Sample));
                CurrentInstrumentIndex = song.instruments.Count - 1;
                Goto(song.instruments.Count - 1);
            }
            if (bRemove.Clicked)
            {
                song.instruments.RemoveAt(CurrentInstrumentIndex);
                if (CurrentInstrumentIndex >= song.instruments.Count)
                {
                    Goto(song.instruments.Count - 1);
                }
            }
            if (bDuplicate.Clicked)
            {
                song.instruments.Add(GetCurrentInstrument.Clone());
                Goto(song.instruments.Count - 1);
            }
            if (bMoveDown.Clicked)
            {
                song.instruments.Reverse(CurrentInstrumentIndex, 2);
                CurrentInstrumentIndex++;
                moveBounds();
            }
            if (bMoveUp.Clicked)
            {
                song.instruments.Reverse(CurrentInstrumentIndex - 1, 2);
                CurrentInstrumentIndex--;
                moveBounds();
            }


            if (bRename.Clicked)
            {
                if (!dialogOpen)
                {
                    dialogOpen = true;
                    StartDialog();
                }
            }
            else { dialogOpen = false; }
            CurrentInstrumentIndex = Math.Clamp(CurrentInstrumentIndex, 0, song.instruments.Count - 1);
            scrollValue = Math.Clamp(scrollValue, 0, Math.Clamp(song.instruments.Count - listLength, 0, 100));
            if (lastIndex != CurrentInstrumentIndex)
            {
                lastIndex = CurrentInstrumentIndex;
                ChannelManager.instance.GetCurrentChannel().SetMacro(CurrentInstrumentIndex);
            }
            if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.OemComma, KeyModifier.Ctrl))
            {
                GetCurrentInstrument.sample.sampleBaseKey--;
                GetCurrentInstrument.name = "base key " + GetCurrentInstrument.sample.sampleBaseKey;
            }
            if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.OemPeriod, KeyModifier.Ctrl))
            {
                GetCurrentInstrument.sample.sampleBaseKey++;
                GetCurrentInstrument.name = "base key " + GetCurrentInstrument.sample.sampleBaseKey;
            }
            if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.T, KeyModifier.Ctrl))
            {
                GetCurrentInstrument.packunpack();
                GetCurrentInstrument.name = "data";
            }
            if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.L, KeyModifier.Ctrl))
            {
                GetCurrentInstrument.sample.sampleLoopType = SampleLoopType.Forward;
                GetCurrentInstrument.name = "loop";
            }
        }

        public void Goto(int index)
        {
            CurrentInstrumentIndex = index;
            moveBounds();
        }
        public void moveBounds()
        {
            if (CurrentInstrumentIndex > scrollValue + listLength - 1)
            {
                scrollValue = CurrentInstrumentIndex - listLength + 1;
            }
            if (CurrentInstrumentIndex < scrollValue)
            {
                scrollValue = CurrentInstrumentIndex;
            }
        }
        public void DrawList()
        {
            Color odd = new Color(43, 49, 81);
            Color even = new Color(59, 68, 107);
            Color selected = new Color(8, 121, 232);
            int y = 0;
            for (int i = scrollValue; i < listLength + scrollValue; i++)
            {
                Color row;
                if (i == CurrentInstrumentIndex)
                    row = selected;
                else if (i % 2 == 0)
                    row = even;
                else
                    row = odd;
                DrawRect(1, 28 + y * 11, 163, 11, row);
                if (song.instruments.Count > i && i >= 0)
                {
                    WriteMonospaced(i.ToString("D2"), 15, 30 + y * 11, Color.White, 4);
                    Write(song.instruments[i].name, 29, 30 + y * 11, Color.White);
                    if (song.instruments[i].macroType == MacroType.Wave)
                        DrawSprite(NumberBox.buttons, 2, 29 + y * 11, new Rectangle(30, 0, 10, 9));
                    else
                        DrawSprite(NumberBox.buttons, 2, 29 + y * 11, new Rectangle(30, 9, 10, 9));
                }
                ++y;
            }
        }

        public void Draw()
        {
            DrawPanel();
            DrawRect(0, 9, 170, 17, Color.White);
            bNewWave.Draw();
            bNewSample.Draw();
            bRemove.Draw();
            bDuplicate.Draw();
            bMoveUp.Draw();
            bMoveDown.Draw();
            bEdit.Draw();
            bRename.Draw();

            DrawList();
        }

        public void StartDialog()
        {
            Input.DialogStarted();
            renameDialog = new Forms.EnterText();
            renameDialog.textBox.Text = GetCurrentInstrument.name;
            renameDialog.Text = "Rename Instrument " + CurrentInstrumentIndex.ToString("D2");
            renameDialog.label.Text = "";
            if (renameDialog.ShowDialog() == DialogResult.OK)
            {
                song.instruments[CurrentInstrumentIndex].SetName(Helpers.FlushString(renameDialog.textBox.Text));
            }
        }

    }
}
