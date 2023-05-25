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
    public class ScrollbarHorizontal : Clickable
    {
        public int totalSize;
        public int viewportSize;
        public Rectangle bar;
        bool lastClickWasOnScrollbar;
        public int minimumSize;
        public int scrollValue { get; set; }
        public int coarseStepAmount { get; set; }
        int barClickOffset;
        public int barWasPressed;
        public ScrollbarHorizontal(int x, int y, int width, int height, Element parent)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            minimumSize = 8;
            coarseStepAmount = 1;
            if (parent != null)
                SetParent(parent);
        }

        public void SetSize(int totalSize, int viewportSize)
        {
            this.viewportSize = viewportSize;
            this.totalSize = totalSize;
            bar.Height = 6;
            bar.X = width - bar.Width;
            bar.Width = (int)(width * (viewportSize / (float)totalSize));
            if (bar.Width < minimumSize)
                bar.Width = minimumSize;
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
                        if (MouseY >= bar.Y && MouseY <= bar.Y + bar.Height)
                        {
                            if (lastClickWasOnScrollbar)
                            {
                                barClickOffset = bar.X - MouseX;
                            }
                            else
                            {
                                // step bar towards mouse
                                if (MouseX > bar.X)
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
                        bar.X = MouseX + barClickOffset;

                        scrollValue = (int)Math.Round(barValFromPos() * (float)(totalSize - viewportSize));
                    }
                    else
                    {
                        if (IsHovered)
                            scrollValue -= Input.MouseScrollWheel(KeyModifier._Any) * coarseStepAmount;
                    }
                    scrollValue = Math.Clamp(scrollValue, 0, totalSize - viewportSize);
                    bar.X = (int)Math.Round(barValFromVal() * (width - 2) + 1);
                }
                if (barisPressed)
                {
                    barWasPressed = 2;
                }
                else
                {
                    if (barWasPressed > 0)
                        barWasPressed--;
                }
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

                DrawRect(0, bar.Y, width, bar.Height, background);
                DrawRoundedRect(1, bar.Y + 1, width - 2, bar.Height - 2, barSpace);
                if (barisPressed && (!Input.internalDialogIsOpen || isPartOfInternalDialog))
                    DrawRoundedRect(bar.X, bar.Y + 1, bar.Width, bar.Height - 2, barPressed);
                else if (barisHovered && (!Input.internalDialogIsOpen || isPartOfInternalDialog))
                    DrawRoundedRect(bar.X, bar.Y + 1, bar.Width, bar.Height - 2, barHover);
                else
                    DrawRoundedRect(bar.X, bar.Y + 1, bar.Width, bar.Height - 2, barDefault);
            }
        }

        float barValFromPos()
        {
            return (bar.X - 1) / (float)(width - 2 - bar.Width);
        }

        float barValFromVal()
        {
            return scrollValue / (float)totalSize;
        }



        bool barisHovered => inFocus && bar.Contains(MouseX, MouseY);
        public bool barisPressed => inFocus && Input.GetClick(KeyModifier._Any) && lastClickWasOnScrollbar;

        Point lastClickPosition
        {
            get { return new Point(Input.lastClickLocation.X - (x + offX), Input.lastClickLocation.Y - (y + offY)); }
        }
    }
}
