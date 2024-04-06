using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class EnvelopeListItem : Clickable {
        /// <summary>
        /// The envelope this list item represents
        /// </summary>
        public Envelope Envelope { get; set; }

        /// <summary>
        /// If the item was clicked on, not including the bypass toggle or delete button
        /// </summary>
        public bool WasClickedOnToSelect { get; private set; }
        public bool WasClickedOnToDelete { get; private set; }

        Clickable exitButton;
        SwitchToggle bypassToggle;

        public EnvelopeListItem(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            bypassToggle = new SwitchToggle(1, 2, this);
            bypassToggle.HasContrastOutline = true;
            exitButton = new MouseRegion(79, 2, 11, 11, this);
            SetParent(parent);
        }

        public void Update() {
            if (Envelope == null)
                return;
            WasClickedOnToSelect = false;
            WasClickedOnToDelete = false;
            if (ClickedDown && !bypassToggle.IsHovered) {
                if (!exitButton.IsHovered) {
                    WasClickedOnToSelect = true;
                }
            }
            if (exitButton.Clicked) {
                WasClickedOnToDelete = true;
            }
            bypassToggle.Value = !Envelope.IsActive;
            bypassToggle.Update();
            if (bypassToggle.ValueWasChangedInternally) {
                Envelope.IsActive = !bypassToggle.Value;
            }
        }

        public void Draw(bool isSelected) {
            if (Envelope != null) {
                if (isSelected) {
                    DrawRoundedRect(0, 0, width, height, UIColors.selection);
                    bypassToggle.Draw();
                    Write(Envelope.GetName(), 15, 4, Helpers.Alpha(Color.White, Envelope.IsActive ? 255 : 64));
                    if (exitButton.IsHovered) {
                        DrawRoundedRect(79, 2, 11, 11, UIColors.selectionLight);
                    }
                    DrawSprite(82, 5, new Rectangle(472, 48, 5, 5), Color.White);
                }
                else {
                    DrawRoundedRect(0, 0, width, height, Color.White);
                    bypassToggle.Draw();
                    Write(Envelope.GetName(), 15, 4, Helpers.Alpha(UIColors.label, Envelope.IsActive ? 255 : 64));
                    if (exitButton.IsHovered) {
                        DrawRoundedRect(79, 2, 11, 11, UIColors.labelLight);
                    }
                    if (IsHovered) {
                        DrawSprite(82, 5, new Rectangle(472, 48, 5, 5), UIColors.label);
                    }
                }
            }
        }
    }
}
