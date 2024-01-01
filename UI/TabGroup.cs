﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using System.Diagnostics;

namespace WaveTracker.UI
{
    public class TabGroup : Element
    {
        public List<Tab> tabs;
        public int selected { get; set; }
        public TabGroup(int x, int y, Element parent)
        {
            this.x = x;
            this.y = y;
            tabs = new List<Tab>();
            SetParent(parent);
        }

        public Tab GetSelectedTab => tabs[selected];

        public void ClearTabs()
        {
            tabs.Clear();
        }

        public void AddTab(string label, bool hasToggle)
        {
            if (tabs.Count == 0)
            {
                tabs.Add(new Tab(label, 0, 0, hasToggle, this));
            }
            else
            {
                tabs.Add(new Tab(label, tabs[tabs.Count - 1].x + tabs[tabs.Count - 1].width + 1, 0, hasToggle, this));
            }
        }

        public void Update()
        {
            int i = 0;
            foreach (Tab tab in tabs)
            {
                tab.Update();
                if (tab.IsPressed)
                    selected = i;
                ++i;
            }
        }

        public void Draw()
        {
            int i = 0;
            foreach (Tab tab in tabs)
            {
                tab.toggle.y = selected == i ? 2 : 3;
                tab.Draw(selected == i);
                ++i;
            }
        }

    }

    public class Tab : Clickable
    {
        public bool hasToggle;
        public string label;
        public Checkbox toggle;

        public Tab(string label, int x, int y, bool hasToggle, Element parent)
        {
            width = Helpers.getWidthOfText(label) + (hasToggle ? 20 : 12);
            height = 13;
            this.x = x;
            this.y = y;
            this.hasToggle = hasToggle;
            toggle = new Checkbox(2, 0, this);
            this.label = label;
            SetParent(parent);
        }

        public void Update()
        {
            if (hasToggle)
            {
                toggle.Update();
            }
        }

        public void Draw(bool selected)
        {
            Color bgCol;
            if (selected)
            {
                bgCol = Color.White;
            }
            else
            {
                if (IsHovered)
                {
                    bgCol = new Color(191, 194, 212);
                }
                else
                {
                    bgCol = new Color(176, 180, 202);
                }
            }
            DrawRect(0, 1, width, height, bgCol);
            DrawRect(1, 0, width - 2, height, bgCol);
            int y = selected ? 3 : 4;
            if (hasToggle)
            {
                if (selected)
                {
                    toggle.Draw();
                }
                else
                {
                    toggle.DrawAsTabToggle();
                }
                Write(label, 14, y, Helpers.Alpha(new Color(20, 24, 46), toggle.Value ? 255 : 80));
            }
            else
            {
                Write(label, 5, y, new Color(20, 24, 46));
            }
        }
    }
}
