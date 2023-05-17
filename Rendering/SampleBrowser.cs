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
using Microsoft.Xna.Framework.Media;

namespace WaveTracker.Rendering
{
    public class SampleBrowser : Element
    {
        public bool enabled;
        public Texture2D icons;
        int selectedFile;
        public Toggle sortName, sortType;
        public SpriteButton backButton;
        public Scrollbar scrollbar;
        public Button ok, cancel;
        string currentPath;
        int listLength = 18;
        public string[] entriesInDirectory;
        Element launched;

        public SampleBrowser(Texture2D tex)
        {
            icons = tex;
            backButton = new SpriteButton(2, 11, 15, 15, Toolbar.sprite, 20, this);
            backButton.isPartOfInternalDialog = true;
            scrollbar = new Scrollbar(284, 29, 4, 198, this);
            scrollbar.isPartOfInternalDialog = true;
        }

        public void Update()
        {
            List<string> entries = Directory.GetFileSystemEntries(currentPath, "*", SearchOption.TopDirectoryOnly).ToList();
            for(int i = entries.Count - 1; i >= 0; i--)
            {
                if (File.GetAttributes(entries[i]) != FileAttributes.Directory)
                {
                    string extension = Path.GetExtension(entries[i]);
                    if (extension != "wav")
                    {
                        entries.RemoveAt(i);
                    }
                }
            }
            entriesInDirectory = entries.ToArray();
            scrollbar.SetSize(entriesInDirectory.Length, listLength);
            scrollbar.scrollValue = Math.Clamp(scrollbar.scrollValue, 0, Math.Clamp(entriesInDirectory.Length - listLength, 0, 100));
            scrollbar.doUpdate();
        }

        public void Open(Element launch)
        {
            launched = launch;
            enabled = true;
            Input.internalDialogIsOpen = true;
            Input.focus = this;
        }

        public void Close()
        {
            enabled = false;
            Input.internalDialogIsOpen = false;
            Input.focus = launched;
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
                if (i == selectedFile)
                    row = selected;
                else if (i % 2 == 0)
                    row = even;
                else
                    row = odd;
                DrawRect(2, 29 + y * 11, 282, 11, row);
                if (entriesInDirectory.Length > i && i >= 0)
                {
                    Write(entriesInDirectory[i], 20, 29 + y * 11, Color.White);
                    //if (song.instruments[i].macroType == MacroType.Wave)
                    //    DrawSprite(NumberBox.buttons, 2, 29 + y * 11, new Rectangle(30, 0, 10, 9));
                    //else
                    //    DrawSprite(NumberBox.buttons, 2, 29 + y * 11, new Rectangle(30, 9, 10, 9));
                }
                ++y;
            }
        }

        public void Draw()
        {
            Color bg = new Color(223, 224, 232);
            DrawRoundedRect(0, 0, 401, 245, bg);
            DrawRect(1, 0, 399, 1, Color.White);
            DrawRect(0, 1, 401, 26, Color.White);
            DrawList();
            Write(currentPath, 20, 15, ButtonColors.Round.backgroundColor);
        }
    }
}
