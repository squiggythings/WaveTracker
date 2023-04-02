using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.Tracker
{
    public struct TEMP_ChannelInstruction
    {
        string note;
        string instrument;
        string volume;
        string command;

        public TEMP_ChannelInstruction()
        {
            note = "...";
            instrument = "..";
            volume = "..";
            command = "...";
        }

        public void SetNote(int noteNum, int instrument)
        {
            note = Helpers.GetNoteName(noteNum);
            if (noteNum < 0)
                this.instrument = "..";
            else
                this.instrument = instrument.ToString("D2");
        }

        public void SetValue(int index, string value)
        {
            if (index == 0) // note
            {
                note = value;
            }
            if (index == 1) // instrument tens
            {
                setCharacterOfString(instrument, 0, value[0]);
            }
            if (index == 2) // instrument ones
            {
                setCharacterOfString(instrument, 1, value[0]);
            }
            if (index == 3) // volume tens
            {
                setCharacterOfString(volume, 0, value[0]);
            }
            if (index == 4) // volume ones
            {
                setCharacterOfString(volume, 1, value[0]);
            }
            if (index == 5) // effect id
            {
                setCharacterOfString(command, 1, value[0]);
            }

            // cannot edit command if command is not set
            if (command[0] == '-')
                return;

            if (index == 6) // effect parameter tens
            {
                setCharacterOfString(command, 2, value[0]);
            }
            if (index == 7) // effect parameter ones
            {
                setCharacterOfString(command, 3, value[0]);
            }
        }

        void RemoveValue(int index)
        {
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                    note = "---";
                    instrument = "--";
                    break;
                case 3:
                case 4:
                    volume = "--";
                    break;
                case 5:
                case 6:
                case 7:
                    volume = "--";
                    break;
            }
        }

        string setCharacterOfString(string input, int index, char character)
        {
            char[] ch = input.ToCharArray();
            ch[index] = character;
            return new string(ch);
        }

        public void Clear()
        {
            note = "...";
            instrument = "..";
            volume = "..";
            command = "...";
        }
    }
}
