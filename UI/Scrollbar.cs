using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Rendering;

namespace WaveTracker.UI
{
    public class Scrollbar : Clickable
    {
        public int totalSize;
        public int viewportSize;
        public Rectangle bar;
        bool lastClickWasOnScrollbar;
        public int scrollValue { get; set; }
        public int coarseStepAmount { get; set; }
        int barClickOffset;
        public Scrollbar(int x, int y, int width, int height, Element parent)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            coarseStepAmount = 1;
            SetParent(parent);
        }

        public void SetSize(int totalSize, int viewportSize)
        {
            this.viewportSize = viewportSize;
            this.totalSize = totalSize;
            bar.Width = 6;
            bar.X = width - bar.Width;
            bar.Height = (int)(height * (viewportSize / (float)totalSize));
        }



        public void Update()
        {
            if (!Input.internalDialogIsOpen || isPartOfInternalDialog)
            {
                if (viewportSize < totalSize)
                {
                    if (Input.GetClickDown(KeyModifier._Any))
                    {
                        lastClickWasOnScrollbar = bar.Contains(lastClickPosition);
                        if (MouseX >= bar.X && MouseX <= bar.X + bar.Width)
                        {
                            if (lastClickWasOnScrollbar)
                            {
                                barClickOffset = bar.Y - MouseY;
                            }
                            else
                            {
                                // step bar towards mouse
                                if (MouseY > bar.Y)
                                {
                                    scrollValue += coarseStepAmount;
                                }
                                else
                                {
                                    scrollValue -= coarseStepAmount;
                                }
                            }
                        }
                    }
                    if (barisPressed)
                    {
                        bar.Y = MouseY + barClickOffset;

                        scrollValue = (int)Math.Round(barValFromPos() * (float)(totalSize - viewportSize));
                    }
                    else
                    {
                        if (IsHovered)
                            scrollValue -= Input.MouseScrollWheel(KeyModifier._Any) * coarseStepAmount;
                    }
                    doUpdate();
                }
            }
        }

        public void doUpdate()
        {
            if (viewportSize < totalSize)
            {
                scrollValue = Math.Clamp(scrollValue, 0, totalSize - viewportSize);
                bar.Y = (int)Math.Round(barValFromVal() * (height - 2) + 1);
            }
        }

        public void Draw()
        {
            if (viewportSize < totalSize)
            {
                Color background = UIColors.panel;
                Color barSpace = UIColors.labelLight;
                Color barDefault = ButtonColors.Round.backgroundColor;
                Color barHover = UIColors.labelDark;
                Color barPressed = UIColors.black;
                //DrawRect(0, 0, width, height, new Color(255, 0, 0, 40));

                DrawRect(bar.X, 0, bar.Width, height, background);
                DrawRoundedRect(bar.X + 1, 1, bar.Width - 2, height - 2, barSpace);
                if (barisPressed && (!Input.internalDialogIsOpen || isPartOfInternalDialog))
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barPressed);
                else if (barisHovered && (!Input.internalDialogIsOpen || isPartOfInternalDialog))
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barHover);
                else
                    DrawRoundedRect(bar.X + 1, bar.Y, bar.Width - 2, bar.Height, barDefault);
            }
        }

        float barValFromPos()
        {
            return (bar.Y - 1) / (float)(height - 2 - bar.Height);
        }

        float barValFromVal()
        {
            return scrollValue / (float)(totalSize);
        }



        bool barisHovered => bar.Contains(MouseX, MouseY);
        bool barisPressed => Input.GetClick(KeyModifier._Any) && lastClickWasOnScrollbar;

        Point lastClickPosition
        {
            get { return new Point(Input.lastClickLocation.X - (x + offX), Input.lastClickLocation.Y - (y + offY)); }
        }
    }
}
